using Microsoft.VisualBasic.Logging;
using Proiect_SPRC;
using Proiect_SPRC.Piese;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Drawing.Text;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace Proiect_SPRC
{
    class Server
    {
        private TcpListener _server = default!;
        private List<TcpClient> _clients = new List<TcpClient>();
        private bool _isRunning = false;
        private string _dbFile = "database.db";
        private string _connectionString => $"Data Source={_dbFile};Version=3";
        private List<MeciSah> _meciuriActive = new List<MeciSah>();

        #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event Action<string> OnLogReceived;
        public event Action OnServerStopped;
        #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public void Start(int port)
        {
            if (_isRunning) return;
            InitializareDB();
            _isRunning = true;
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();

            Log($"[SERVER] Se deschide portul {port}...");

            Thread t = new Thread(ListenForClients);
            t.IsBackground = true;
            t.Start();

            Thread healthCheck = new Thread(KeepAlive);
            healthCheck.IsBackground = true;
            healthCheck.Start();
        }
        private void ListenForClients()
        {
            try
            {
                while (true)
                {
                    TcpClient client = _server.AcceptTcpClient();
                    lock (_clients) { _clients.Add(client); }

                    Thread t = new Thread(() => HandleClient(client));
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                Log("[SERVER] Serverul a fost oprit cu succes.");
            }
            catch (Exception ex)
            {
                Log($"[SERVER] Eroare server: {ex.Message}");
            }
        }
        private string HandleCreateLobby(TcpClient creator, string lobbyCode)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    // Folosim parametri (@cod, @data, @status) în loc de concatenare
                    string sql = "INSERT INTO Lobby (lobbyCode, DataCreare, Status) VALUES (@cod, @data, @status)";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", lobbyCode);
                        cmd.Parameters.AddWithValue("@data", DateTime.Now); // SQLite va converti corect data
                        cmd.Parameters.AddWithValue("@status", "Asteptare");

                        cmd.ExecuteNonQuery();
                    }
                }

                lock (_meciuriActive)
                {
                    MeciSah nouMeci = new MeciSah(lobbyCode, creator);
                    _meciuriActive.Add(nouMeci);
                }

                return $"ACK_CREATE|{lobbyCode}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EROARE SQL]: {ex.Message}");
                return $"ACK: Error occured: {ex.Message}";
            }
        }
        private string HandleJoinLobby(TcpClient jucator, string lobbyCode)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);

                if (meci == null) return "ERR|Lobby inexistent";
                if (meci.JucatorAlb == null)
                {
                    meci.JucatorAlb = jucator;
                    Log($"[LOBBY] {lobbyCode}: Primul jucător (ALB) a intrat în camera de așteptare.");
                    return $"WAITING|{lobbyCode}|Ai intrat ca ALB, se asteapta adversar...";
                }
                else if (meci.JucatorNegru == null)
                {
                    meci.JucatorNegru = jucator;
                    Log($"[LOBBY] {lobbyCode}: Al doilea jucător (NEGRU) a intrat.");
                    StartMatchCountdown(lobbyCode);
                    return $"WAITING|{lobbyCode}|Ai intrat ca NEGRU, meciul incepe in 3 secunde...";
                }
                else
                {
                    meci.Spectatori.Add(jucator);
                    SendMessage(jucator, $"JOIN_SUCCESS|{meci.StareTabla}");
                    return $"WAITING|{lobbyCode}|Ai intrat ca spectator.";
                }
            }
        }

        private void StartMatchCountdown(string lobbyCode)
        {
            Thread countdownThread = new Thread(() =>
            {
                MeciSah meci = null;

                // Căutăm meciul în lista de meciuri active sub un lock
                lock (_meciuriActive)
                {
                    meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);
                }

                if (meci == null)
                {
                    Log($"[MECI] {lobbyCode}: Eroare - Meciul nu a fost găsit pentru countdown.");
                    return;
                }

                Log($"[MECI] {meci.lobbyCode}: Incepe numaratoarea inversa (3s)...");
                SendMessage(meci.JucatorAlb, "COUNTDOWN|3");
                SendMessage(meci.JucatorNegru, "COUNTDOWN|3");

                Thread.Sleep(3000);

                // Verificăm din nou dacă meciul mai există și jucătorii sunt prezenți
                // S-ar putea ca cineva să fi ieșit în cele 3 secunde de Sleep
                if (meci.JucatorAlb != null && meci.JucatorAlb.Connected &&
                    meci.JucatorNegru != null && meci.JucatorNegru.Connected)
                {
                    Log($"[MECI] {meci.lobbyCode}: START!");
                    SendMessage(meci.JucatorAlb, "MATCH_START|ALB");
                    SendMessage(meci.JucatorNegru, "MATCH_START|NEGRU");

                    UpdateLobbyStatus(meci.lobbyCode, "InDesfasurare");
                }
                else
                {
                    Log($"[MECI] {meci.lobbyCode}: Start anulat. Un jucator s-a deconectat.");
                }
            });

            countdownThread.IsBackground = true; // Recomandat pentru a nu bloca închiderea serverului
            countdownThread.Start();
            /*Thread countdownThread = new Thread(() =>
            {
                Log($"[MECI] {meci.lobbyCode}: Incepe numaratoarea inversa (3s)...");
                SendMessage(meci.JucatorAlb, "COUNTDOWN|3");
                SendMessage(meci.JucatorNegru, "COUNTDOWN|3");

                Thread.Sleep(3000);

                if (meci.JucatorAlb != null && meci.JucatorAlb.Connected && meci.JucatorNegru != null && meci.JucatorNegru.Connected)
                {
                    Log($"[MECI] {meci.lobbyCode}: START!");
                    SendMessage(meci.JucatorAlb, "MATCH_START|ALB");
                    SendMessage(meci.JucatorNegru, "MATCH_START|NEGRU");

                    UpdateLobbyStatus(meci.lobbyCode, "InDesfasurare");
                }
                else
                {
                    Log($"[MECI] {meci.lobbyCode}: Start anulat. Un jucator s-a deconectat.");
                }
            });*/
        }

        private void UpdateLobbyStatus(string lobbyCode, string status)
        {
            try
            {
                using(var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE Lobby SET Status = @status WHERE codlobby = @cod";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", status);
                        cmd.Parameters.AddWithValue("@cod", lobbyCode);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"[EROARE DB UPDATE] {ex.Message}");
            }
        }

        private void HandleClient(TcpClient client)
        {
            Log("[SERVER] Client nou conectat.");
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (_isRunning)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead <= 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                    Log($"[SERVER] S-a primit: {msg}");

                    string response = ProcessCommand(msg);
                    Log($"[SERVER] S-a trimis ({response})");
                    SendMessage(client, response);
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Log($"[SERVER] Eroare client: {ex.Message}");
            }
            finally
            {
                CleanupClient(client);
            }
        }
        private void KeepAlive()
        {
            while (_isRunning)
            {
                Thread.Sleep(5000);
                lock (_clients)
                {
                    foreach (var client in _clients.ToList())
                    {
                        if (!client.Connected)
                        {
                            CleanupClient(client);
                        }
                    }
                }
            }
        }
        private void CleanupClient(TcpClient client)
        {
            Log("[SERVER] Se curăță resursele pentru un client deconectat.");

            lock (_meciuriActive)
            {
                // Găsim meciul în care era acest client
                var meci = _meciuriActive.FirstOrDefault(m => m.JucatorAlb == client || m.JucatorNegru == client);

                if (meci != null)
                {
                    // Anunțăm celălalt jucător
                    TcpClient adversar = (meci.JucatorAlb == client) ? meci.JucatorNegru : meci.JucatorAlb;

                    if (adversar != null && adversar.Connected)
                    {
                        SendMessage(adversar, "OPPONENT_DISCONNECTED|Adversarul tău a părăsit jocul.");
                    }

                    // Putem alege să închidem meciul sau să îl ștergem din listă
                    _meciuriActive.Remove(meci);
                    UpdateLobbyStatus(meci.lobbyCode, "Abandonat");
                }
            }

            lock (_clients) { _clients.Remove(client); }
            client.Close();
        }
        public void ProcessServerCommand(string msg)
        {
            Log(ProcessCommand(msg));
        }
        protected string ProcessCommand(string msg) => ProcessCommand(msg, null);
        protected string ProcessCommand(string msg, TcpClient sender)
        {
            // Presupunem formatul "TIP_OPERATIUNE|COD_LOBBY"
            string[] parts = msg.Split('|');
            string command = parts[0].ToUpper();
            string lobbyCode = parts.Length > 1 ? parts[1] : "";
            Log(command);
            // Aici citim buffer-ul si vedem ce comenzi am primit, si facem actiunea in concordanta cu mesajul primit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de intrare in joc, atunci se foloseste codul primit pentru conectarea clientului la jocul deja pornit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de creearea cod, atunci se creeaza un nou lobby cu codul primit, se introduce in baza de date si se trimite codul clientului
            //      - daca se apasa primeste acelasi lucru cu inchide lobby = true, atunci se inchide lobby-ul.
            // Daca se primeste apasarea butonului joaca cu modifier-ul de urmarire joc, se foloseste codul primit pentru conectarea clientului la jocul cu acel cod drept spectator
            switch (command)
            {
                // CREATE|{lobbyCode} -> creeaza un nou joc cu codul primit
                case "CREATE":
                    return HandleCreateLobby(sender, lobbyCode);
                //JOIN|{lobbyCode} -> intra in lobby-ul de joc cu codul primit daca sunt mai putin de 2 jucatori
                case "JOIN":
                    return HandleJoinLobby(sender, lobbyCode);
                //START|{lobbyCode} -> incepe jocul cu codul primit la primirea comenzii
                case "START":
                    StartMatchCountdown(lobbyCode);
                    return $"ACK|A pornit jocul de sah {lobbyCode}";
                //CLOSE -> opreste jocul
                case "CLOSE":
                    Stop();
                    return $"ACK|Lobby-ul cu codul {lobbyCode} a fost inchis";
                //SPECTATE|{lobbyCode} -> jucatorul intra ca spectator in jocul cu codul de la primirea comenzii
                case "SPECTATE":        
                    return HandleJoinLobby(sender, lobbyCode);
                //UPDATE|lobbyCode|vectorTabla -> se actualizeaza starea jocului cu optiunile primite
                case "UPDATE":
                    string stareTabla = ValidareMutare(lobbyCode, parts[2]);
                    if (stareTabla != "")
                    {
                        BroadcastToLobby(lobbyCode, $"UPDATE_SUCCESS|{stareTabla}");
                        return $"ACK|Actualizat stare {lobbyCode}";
                    }
                    return $"ERR|Mutare invalida conform regulilor";
                //CHAT|lobbyCode|mesaj -> se transmite mesajul primit tuturor participantilor la jocul cu codul primit
                case "CHAT":
                    if(parts.Length < 3) return "ERR|Mesaj Gol";
                    BroadcastChat(lobbyCode, parts[2], false, sender);
                    return "";
                //CHAT_PRIVATE|lobbyCode|mesaj -> se transmite mesajul primit doar adversarului la jocul cu codul primit
                case "CHAT_PRIVATE":
                    if(parts.Length < 3) return "ERR|Mesaj Privat Gol";
                    BroadcastChat(lobbyCode, parts[2], true, sender);
                    return "";
                case "TEST":
                    return $"ACK|Test primit";
                default:
                    return "ERR|Comanda Invalida";
            }
        }
        private void BroadcastToLobby(string lobbyCode, string mesaj)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);
                if (meci == null) return;

                // Trimitem la Jucătorul Alb
                if (meci.JucatorAlb != null && meci.JucatorAlb.Connected)
                    SendMessage(meci.JucatorAlb, mesaj);

                // Trimitem la Jucătorul Negru
                if (meci.JucatorNegru != null && meci.JucatorNegru.Connected)
                    SendMessage(meci.JucatorNegru, mesaj);

                // Trimitem tuturor spectatorilor
                foreach (var spectator in meci.Spectatori)
                {
                    if (spectator != null && spectator.Connected)
                        SendMessage(spectator, mesaj);
                }
            }
        }
        private void BroadcastChat(string lobbyCode, string mesaj, bool isPrivate, TcpClient sender)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);
                if (meci == null) return;

                string prefix = isPrivate ? "CHAT_PRIVATE_MESSAGE" : "CHAT_MSG";
                string expeditor = (sender == meci.JucatorAlb) ? "Alb" : (sender == meci.JucatorNegru) ? "Negru" : "Spectator";
                string mesajFinal = $"{prefix}|{expeditor}|{mesaj}";
                
                SendMessage(meci.JucatorAlb, mesajFinal);
                SendMessage(meci.JucatorNegru, mesajFinal);

                if (!isPrivate)
                {
                    foreach (var spectator in meci.Spectatori)
                    {
                        SendMessage(spectator, mesajFinal);
                    }
                }
            }
        }
        public static int[,] ConvertesteInMatrice(string vectorString)
        {
            int[,] matrice = new int[8, 8];
            string[] valori = vectorString.Split(','); // Sau orice separator folosești

            for (int i = 0; i < 64; i++)
            {
                matrice[i / 8, i % 8] = int.Parse(valori[i]);
            }
            return matrice;
        }
        public static bool ExecutaValidarePiesa(int startX, int startY, int stopX, int stopY, int[,] tabla)
        {
            int piesaId = tabla[startX, startY];
            PiesaSah? piesa = null;

            // Mapare ID la Clase (Exemplu: 1=Pion, 2=Cal, 3=Nebun, 4=Turn, 5=Regina, 6=Rege)
            // piesaId pozitiv = Alb, piesaId negativ = Negru
            switch (Math.Abs(piesaId))
            {
                case 1: piesa = new Pion(); break;
                case 2: piesa = new Cal(); break;
                case 3: piesa = new Nebun(); break;
                case 4: piesa = new Turn(); break;
                case 5: piesa = new Regina(); break;
                case 6: piesa = new Rege(); break;
            }

            if (piesa == null) return false;

            //obtinere culoare jucator
            piesa.Culoare = piesaId > 0 ? "Alb" : "Negru";

            // Verificăm dacă nu cumva jucătorul încearcă să-și mănânce propria piesă
            if (tabla[stopX, stopY] != 0)
            {
                //verificarea randului de mutare
                bool tintaEsteAlba = tabla[stopX, stopY] > 0;
                if ((piesa.Culoare == "Alb" && tintaEsteAlba) || (piesa.Culoare == "Negru" && !tintaEsteAlba))
                    return false;
            }

            return piesa.EsteMutareValida(startX, startY, stopX, stopY, tabla);
        }
        private string ValidareMutare(string lobbyCode, string stareNouaVector)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);
                if (meci == null) return "";

                // 1. Transformăm string-ul/vectorul primit în matrice int[8,8]
                int[,] tablaNoua = ConvertesteInMatrice(stareNouaVector);
                int[,] tablaVeche = ConvertesteInMatrice(meci.StareTabla); // Presupunem că StareTabla reține ultima stare validă

                int startX = -1, startY = -1, stopX = -1, stopY = -1;

                // 2. Detectăm ce piesă s-a mutat și unde (Detectare diferențe)
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (tablaVeche[i, j] != 0 && tablaNoua[i, j] == 0)
                        {
                            startX = i; startY = j;
                        }
                        else if (tablaNoua[i, j] != tablaVeche[i, j])
                        {
                            stopX = i; stopY = j;
                        }
                    }
                }

                // Dacă nu s-a detectat o mutare clară, respingem
                if (startX == -1 || stopX == -1) return meci.StareTabla;

                // 3. Executăm validarea propriu-zisă
                if (ExecutaValidarePiesa(startX, startY, stopX, stopY, tablaVeche))
                {
                    string culoareJucator = (tablaVeche[startX, startY] > 0) ? "Alb" : "Negru";
                    if (meci.ArFiInSahDupaMutare(startX, startY, stopX, stopY, culoareJucator))
                    {
                        return ""; // Mutare invalidă: regele rămâne/intră în șah - String gol !TREBUIE VERIFICAT
                    }

                    // Dacă e validă, actualizăm starea meciului
                    meci.StareTabla = stareNouaVector;
                    return meci.StareTabla;
                }

                return meci.StareTabla;
            }
        }
        public void InitializareDB()
        {
            if (!File.Exists(_dbFile)) SQLiteConnection.CreateFile(_dbFile);
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"CREATE TABLE IF NOT EXISTS Lobby (
                                lobbyCode TEXT PRIMARY KEY,
                                DataCreare DATETIME,
                                Status TEXT);";
                using (var cmd = new SQLiteCommand(sql, conn)) cmd.ExecuteNonQuery();
            }
        }

        public void Stop()
        {
            Log("[SERVER] Se oprește serverul...");
            _isRunning = false;
            lock (_clients)
            {
                foreach (var c in _clients.ToList())
                {
                    try
                    {
                        if (c.Connected)
                        {
                            SendMessage(c, "SERVER_STOPPED|Serverul se închide.");
                            c.GetStream().Close();
                            c.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"[SERVER] Eroare stop: {ex.Message}");
                    }
                }
                _clients.Clear();
            }
            _server?.Stop();
            OnServerStopped?.Invoke();
            Log("[SERVER] Server-ul s-a oprit complet.");
        }

        public void SendMessage(TcpClient client, string msg)
        {
            try
            {
                if (client == null || !client.Connected) return;
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(msg);

                lock (stream)
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Log($"[SERVER] Eroare la trimiterea mesajului: {ex.Message}");
            }
        }
        private string GenerareCod(int nr)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random rnd = new Random();

            string cod = "";

            for (int i = 0; i < nr-1; i++)
            {
                cod += chars[rnd.Next(chars.Length)];
            }
            return cod;
        }
        private void Log(string message)
        {
            OnLogReceived?.Invoke(message);
        }
    }
}

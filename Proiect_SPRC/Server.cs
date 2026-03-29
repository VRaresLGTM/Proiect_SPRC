using Microsoft.VisualBasic.Logging;
using Proiect_SPRC;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Drawing.Text;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Proiect_SPRC
{
    class Server
    {
        private TcpListener _server;
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
        private string HandleCreateLobby(TcpClient creator, string preferintaCuloare)
        {
            string lobbyCode = GenerareCod(6);
            /*
            string culoareAtribuita = preferintaCuloare.ToUpper();

            if(string.IsNullOrEmpty(culoareAtribuita))
            {
                culoareAtribuita = (new Random().Next(2) == 0) ? "ALB" : "NEGRU";
            }
            */
            #pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = $"INSERT INTO Lobby (lobbyCode, DataCreare, Status) VALUES (@cod, {DateTime.Now}, 'Asteptare')";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", lobbyCode);
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
                return $"Ack: Lobby creat cu codul {lobbyCode}";
            }
        #pragma warning restore CS0168 // Variable is declared but never used
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
                    StartMatchCountdown(meci);
                    return $"WAITING|{lobbyCode}|Ai intrat ca NEGRU, meciul incepe in 3 secunde...";
                }
                else return "ERR|Lobby deja plin";
            }
        }

        private void StartMatchCountdown(MeciSah meci)
        {
            Thread countdownThread = new Thread(() =>
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
            });
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
                client.Close();
                lock (_clients) { _clients.Remove(client); }
            }
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
            // Aici citim buffer-ul si vedem ce comenzi am primit, si facem actiunea in corcondanta cu mesajul primit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de intrare in joc, atunci se foloseste codul primit pentru conectarea clientului la jocul deja pornit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de creearea cod, atunci se creeaza un nou lobby cu codul primit, se introduce in baza de date si se trimite codul clientului
            //      - daca se apasa primeste acelasi lucru cu inchide lobby = true, atunci se inchide lobby-ul.
            // Daca se primeste apasarea butonului joaca cu modifier-ul de urmarire joc, se foloseste codul primit pentru conectarea clientului la jocul cu acel cod drept spectator
            switch (command)
            {
                // CREATE|{lobbyCode} -> creeaza un nou joc cu codul primit
                case "CREATE":
                    //de implementat
                    string preferinta = parts.Length > 1 ? parts[2] : "";
                    return HandleCreateLobby(sender, preferinta);
                //JOIN|{lobbyCode} -> intra in lobby-ul de joc cu codul primit daca sunt mai putin de 2 jucatori
                case "JOIN":
                    //de implementat
                    return HandleJoinLobby(sender, lobbyCode);
                //START|{lobbyCode} -> incepe jocul cu codul primit la primirea comenzii
                case "START":
                    //de implementat
                    return $"ACK|A inceput jocul de sah {lobbyCode}";
                //CLOSE|{lobbyCode} -> opreste jocul cu codul primit la primirea comenzii
                case "CLOSE":
                    //de implementat
                    return $"ACK|Lobby-ul cu codul {lobbyCode} a fost inchis";
                //SPECTATE|{lobbyCode} -> jucatorul intra ca spectator in jocul cu codul de la primirea comenzii
                case "SPECTATE":
                    //de implementat
                    return $"ACK|Urmaresti {lobbyCode}";
                //UPDATE|{optiuni} -> se actualizeaza starea jocului cu optiunile primite (tabla curenta de sah, 
                case "UPDATE":
                    //de implementat
                    return $"ACK|Actualizat stare {lobbyCode}";
                case "TEST":
                    return $"ACK|Test primit";
                default:
                    return "ERR|Comanda Invalida";
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
            _isRunning = false;
            lock (_clients)
            {
                foreach (var c in _clients) c.Close();
                _clients.Clear();
            }
            _server?.Stop();
            OnServerStopped?.Invoke();
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

using System.Data.SQLite;
using System.Net;
using System.Net.Sockets;
using System.Text;

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
            IncarcaLobbyuriDinDB();

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
                Log("[SERVER_LISTENER] Serverul a fost oprit cu succes.");
            }
            catch (Exception ex)
            {
                Log($"[SERVER_LISTENER] Eroare server: {ex.Message}");
            }
        }
        private void IncarcaLobbyuriDinDB()
        {
            lock (_meciuriActive)
            {
                _meciuriActive.Clear();
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "SELECT lobbyCode FROM Lobby WHERE Status = 'Asteptare'";

                    using (var cmd = new SQLiteCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            string cod = reader.GetString(0);
                            MeciSah meci = new MeciSah(cod, null);
                            _meciuriActive.Add(meci);
                        }
                    }
                }
            }
        }
        private string StergeLobbyDinBD(string lobbyCode)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Lobby WHERE lobbyCode = @cod";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", lobbyCode);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows == 0)
                            return "ERR|Lobby inexistent in DB";
                    }
                }
                lock (_meciuriActive)
                {
                    var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode); 
                    if (meci != null) 
                        _meciuriActive.Remove(meci);
                }
                return $"ACK|Lobby {lobbyCode} sters";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EROARE SQL]: {ex.Message}");
                return $"ERR|Eroare la ștergerea lobby-ului: {ex.Message}";
            }
        }
        private string HandleCreateLobby(TcpClient creator, string lobbyCode)
        {
            try
            {
                // 1. Verificăm dacă lobby-ul există deja în memorie pentru a evita duplicatele
                lock (_meciuriActive)
                {
                    if (_meciuriActive.Any(m => m.lobbyCode == lobbyCode))
                    {
                        return "ERR|Cod lobby deja existent";
                    }
                }

                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Lobby (lobbyCode, DataCreare, Status) VALUES (@cod, @data, @status)";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@cod", lobbyCode);
                        cmd.Parameters.AddWithValue("@data", DateTime.Now);
                        cmd.Parameters.AddWithValue("@status", "Asteptare");
                        cmd.ExecuteNonQuery();
                    }
                }

                lock (_meciuriActive)
                {
                    // IMPORTANT: Constructorul MeciSah trebuie să asigneze creatorul ca JucatorAlb
                    MeciSah nouMeci = new MeciSah(lobbyCode, creator);
                    _meciuriActive.Add(nouMeci);
                }

                // Creatorul primește confirmarea. El este DEJA înregistrat ca ALB.
                return $"ACK_CREATE|{lobbyCode}|Esti ALB, asteapta adversar...";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EROARE SQL]: {ex.Message}");
                return $"ERR|Eroare la crearea lobby-ului: {ex.Message}";
            }
        }
        /*private string HandleCreateLobby(TcpClient creator, string lobbyCode)
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
        }*/
        private string HandleJoinLobby(TcpClient jucator, string lobbyCode)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);

                if (meci == null) return "ERR|Lobby inexistent";

                // 2. VERIFICARE CRITICĂ: Verificăm dacă jucătorul este deja în lobby
                if (meci.JucatorAlb == jucator)
                {
                    return $"WAITING|{lobbyCode}|Esti deja ALB, se asteapta adversar...";
                }
                if (meci.JucatorNegru == jucator)
                {
                    return $"WAITING|{lobbyCode}|Esti deja NEGRU, meciul incepe...";
                }

                // 3. Logică de atribuire a locurilor libere
                if (meci.JucatorAlb == null)
                {
                    meci.JucatorAlb = jucator;
                    Log($"[LOBBY] {lobbyCode}: ALB a intrat.");
                    return $"WAITING|{lobbyCode}|Ai intrat ca ALB, se asteapta adversar...";
                }
                else if (meci.JucatorNegru == null)
                {
                    meci.JucatorNegru = jucator;
                    Log($"[LOBBY] {lobbyCode}: NEGRU a intrat.");

                    // Notificăm și jucătorul ALB că a venit un adversar (opțional, depinde de SendMessage)
                    SendMessage(meci.JucatorAlb, "OPPONENT_JOINED|Negru a intrat în meci!");

                    StartMatchCountdown(lobbyCode);
                    return $"WAITING|{lobbyCode}|Ai intrat ca NEGRU, meciul incepe curand...";
                }
                else
                {
                    // Dacă ambele locuri sunt ocupate, intră ca spectator
                    meci.Spectatori.Add(jucator);
                    return $"JOIN_SUCCESS|{meci.StareTabla}|Ai intrat ca spectator.";
                }
            }
        }
        /*private string HandleJoinLobby(TcpClient jucator, string lobbyCode)
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
        }*/

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
                    string sql = "UPDATE Lobby SET Status = @status WHERE lobbyCode = @cod";
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

                    string response = ProcessCommand(msg, client);
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
                List<TcpClient> deCuratat = new List<TcpClient>();

                // Doar colectăm clienții deconectați sub lock, fără să apelăm alte metode grele
                lock (_clients)
                {
                    foreach (var client in _clients)
                    {
                        if (!client.Connected)
                        {
                            deCuratat.Add(client);
                        }
                    }
                }

                // Apelăm CleanupClient în AFARA lock-ului pentru a elimina riscul de Deadlock
                foreach (var client in deCuratat)
                {
                    CleanupClient(client);
                }
            }
        }
        private void CleanupClient(TcpClient client)
        {
            try
            {
                Log("[SERVER] Se curăță resursele pentru un client deconectat.");

                lock (_meciuriActive)
                {
                    // Găsim meciul în care era acest client (verificăm sigur dacă Spectatori nu e null)
                    var meci = _meciuriActive.FirstOrDefault(m =>
                        m.JucatorAlb == client ||
                        m.JucatorNegru == client ||
                        (m.Spectatori != null && m.Spectatori.Contains(client)));

                    if (meci != null)
                    {
                        // 2. Eliminăm clientul din rolul corespunzător
                        if (meci.JucatorAlb == client)
                        {
                            meci.JucatorAlb = null;
                            Log($"[LOBBY] {meci.lobbyCode}: Jucătorul ALB a părăsit meciul.");

                            if (meci.JucatorNegru != null && meci.JucatorNegru.Connected)
                                SendMessage(meci.JucatorNegru, "OPPONENT_LEFT|Adversarul ALB s-a deconectat.");
                        }
                        else if (meci.JucatorNegru == client)
                        {
                            meci.JucatorNegru = null;
                            Log($"[LOBBY] {meci.lobbyCode}: Jucătorul NEGRU a părăsit meciul.");

                            if (meci.JucatorAlb != null && meci.JucatorAlb.Connected)
                                SendMessage(meci.JucatorAlb, "OPPONENT_LEFT|Adversarul NEGRU s-a deconectat.");
                        }
                        else
                        {
                            meci.Spectatori?.Remove(client);
                            Log($"[LOBBY] {meci.lobbyCode}: Un spectator a plecat.");
                        }

                        // 3. Condiția critică: Verificăm dacă lobby-ul a rămas complet gol
                        int spectatoriCount = meci.Spectatori?.Count ?? 0;
                        if (meci.JucatorAlb == null && meci.JucatorNegru == null && spectatoriCount == 0)
                        {
                            Log($"[LOBBY] {meci.lobbyCode} este complet gol. Se elimină din memorie și DB...");

                            // Ștergem din baza de date
                            StergeLobbyDinBD(meci.lobbyCode);
                            UpdateLobbyStatus(meci.lobbyCode, "Inchis");

                            // Eliminăm meciul din lista de meciuri active din RAM
                            _meciuriActive.Remove(meci);
                        }
                    }
                }

                // MUTAT AFARĂ din lock-ul de meciuri pentru a preveni Deadlock-ul
                lock (_clients)
                {
                    _clients.Remove(client);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                // Protecție finală: Serverul nu va mai crapa niciodată de la deconectări
                Log($"[SERVER EROARE CLEANUP] A apărut o eroare la curățare: {ex.Message}");
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
                case "DELETE":
                    StergeLobbyDinBD(lobbyCode);
                    return $"ACK|Lobby-ul cu codul {lobbyCode} a fost șters";
                case "START":
                    StartMatchCountdown(lobbyCode);
                    return $"ACK|A pornit jocul de sah {lobbyCode}";
                //CLOSE -> opreste jocul
                case "CLOSE":
                    StopMatch(lobbyCode);
                    return $"ACK|Lobby-ul cu codul {lobbyCode} a fost inchis";
                //SPECTATE|{lobbyCode} -> jucatorul intra ca spectator in jocul cu codul de la primirea comenzii
                case "SPECTATE":        
                    return HandleJoinLobby(sender, lobbyCode);
                //UPDATE|lobbyCode|vectorTabla -> se actualizeaza starea jocului cu optiunile primite
                case "UPDATE":
                    string stareTabla = ValidareMeci(lobbyCode, parts[2]);
                    if (stareTabla != "")
                    {
                        BroadcastToLobby(lobbyCode, $"UPDATE_SUCCESS|{stareTabla}");
                        // Am sters return ACK pentru a preveni lipirea pachetelor TCP!
                        return "";
                    }
                    return $"ERR|Mutare invalida conform regulilor";
                //CHAT|lobbyCode|emitator|mesaj -> se transmite mesajul primit tuturor participantilor la jocul cu codul primit
                case "CHAT":
                    if(parts.Length < 4) return "ERR|Mesaj Gol";
                    string emitatorsimesaj = parts[2]+": " + parts[3];
                    BroadcastChat(lobbyCode, emitatorsimesaj, false, sender);
                    return emitatorsimesaj;
                //CHAT_PRIVATE|lobbyCode|mesaj -> se transmite mesajul primit doar adversarului la jocul cu codul primit
                case "CHAT_PRIVATE":
                    if(parts.Length < 4) return "ERR|Mesaj Privat Gol";
                    string emitatorsimesajprivat = parts[2] + ": " + parts[3];
                    BroadcastChat(lobbyCode, emitatorsimesajprivat, true, sender);
                    return emitatorsimesajprivat;
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
                if (meci == null)
                {
                    Log("Meci este null");
                    return;
                }
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
        
        
        private string ValidareMeci(string lobbyCode, string stareNouaVector)
        {
            lock (_meciuriActive)
            {
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);
                if (meci == null) return "";

                // Serverul acționează ca Relay: Clientul a validat deja mutarea.
                // Salvăm direct noua stare a tablei în memorie.
                meci.StareTabla = stareNouaVector;

                // Returnăm noua stare pentru a fi trimisă mai departe prin Broadcast (UPDATE_SUCCESS)
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
        public void StopServer()
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
        public void StopMatch(string lobbyCode)
        {
            Log($"[SERVER] Se oprește doar meciul: {lobbyCode}...");

            lock (_meciuriActive)
            {
                // 1. Găsim meciul specific în lista de meciuri active din RAM
                var meci = _meciuriActive.FirstOrDefault(m => m.lobbyCode == lobbyCode);

                if (meci != null)
                {
                    // 2. Colectăm doar clienții care fac parte din acest meci (Alb, Negru și Spectatori)
                    List<TcpClient> clientiMeci = new List<TcpClient>();
                    if (meci.JucatorAlb != null) clientiMeci.Add(meci.JucatorAlb);
                    if (meci.JucatorNegru != null) clientiMeci.Add(meci.JucatorNegru);
                    if (meci.Spectatori != null) clientiMeci.AddRange(meci.Spectatori);

                    // 3. Notificăm și închidem conexiunea DOAR pentru acești clienți
                    foreach (var c in clientiMeci)
                    {
                        try
                        {
                            if (c.Connected)
                            {
                                // Trimitem un mesaj specific către client ca să știe că meciul s-a terminat
                                SendMessage(c, "MATCH_FORCE_STOPPED|Meciul a fost oprit de server.");

                                // Închidem fluxul și conexiunea pentru acest client
                                c.GetStream().Close();
                                c.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"[SERVER] Eroare la oprirea unui client din meciul {lobbyCode}: {ex.Message}");
                        }

                        // Îi scoatem și din lista globală de clienți activi ai serverului
                        lock (_clients)
                        {
                            _clients.Remove(c);
                        }
                    }

                    // 4. Curățăm baza de date SQLite pentru acest cod ca să poată fi refolosit
                    StergeLobbyDinBD(lobbyCode);

                    Log($"[SERVER] Meciul {lobbyCode} a fost oprit cu succes. Serverul rulează în continuare.");
                }
                else
                {
                    Log($"[SERVER] Meciul cu codul {lobbyCode} nu a fost găsit sau este deja închis.");
                }
            }
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
        
        
        private void Log(string message)
        {
            OnLogReceived?.Invoke(message);
        }
    }
}

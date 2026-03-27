using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Proiect_SPRC
{
    class Server
    {
        private TcpListener? _server;
        private List<TcpClient> _clients = new List<TcpClient>();
        private bool _isRunning = false;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public event Action<string> OnLogReceived;
        public event Action OnServerStopped;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public void Start(int port)
        {
            if (_isRunning) return;

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
        protected string ProcessCommand(string msg)
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
                case "JOIN":
                    //de implementat
                    return $"Ack: Conectat la {lobbyCode}";
                case "CREATE":
                    //de implementat
                    return $"Ack: Lobby creat cu codul {lobbyCode}";
                case "CLOSE":
                    //de implementat
                    return $"Ack: Lobby-ul cu codul {lobbyCode} a fost inchis";
                case "SPECTATE":
                    //de implementat
                    return $"Ack: Urmaresti {lobbyCode}";
                case "TEST":
                    return $"Ack: test primit";
                default:
                    return "Eroare: Comanda invalida";
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
        private void Log(string message)
        {
            OnLogReceived?.Invoke(message);
        }
    }
}

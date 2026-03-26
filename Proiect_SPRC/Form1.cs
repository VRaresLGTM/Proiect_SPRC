using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Proiect_SPRC
{
    public partial class Form1 : Form
    {
        //String curSelect = "comenzi";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        TcpListener server;
        List<TcpClient> clients = new List<TcpClient>();

        void StartServer()
        {
            try
            {
                Invoke((MethodInvoker)(() =>
                {
                    labelServerStatus.Text = "Status Server: ON  ";
                    jurnalTextBox.AppendText("[SERVER] Se deschide portul pentru joc (5000)...");
                }));
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    clients.Add(client);

                    Thread t = new Thread(() => HandleClient(client));
                    t.IsBackground = true;
                    t.Start();
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                Invoke((MethodInvoker)(() =>
                {
                    jurnalTextBox.AppendText("\n[SERVER] Serverul a fost oprit cu succes.\n");
                    labelServerStatus.Text = "Status Server: OFF ";
                }));
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)(() =>
                {
                    jurnalTextBox.AppendText($"\n[SERVER] Eroare server: {ex.Message}\n");

                }));
            }
        }

        void HandleClient(TcpClient client)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\n[SERVER] Se obțin informații din socket...");
            }));
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            // Aici citim buffer-ul si vedem ce comenzi am primit, si facem actiunea in corcondanta cu mesajul primit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de intrare in joc, atunci se foloseste codul primit pentru conectarea clientului la jocul deja pornit
            // Daca se primeste apasarea butonului joaca cu modifier-ul de creearea cod, atunci se creeaza un nou lobby cu codul primit, se introduce in baza de date si se trimite codul clientului
            //      - daca se apasa primeste acelasi lucru cu inchide lobby = true, atunci se inchide lobby-ul.
            // Daca se primeste apasarea butonului joaca cu modifier-ul de urmarire joc, se foloseste codul primit pentru conectarea clientului la jocul cu acel cod drept spectator
            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    Invoke((MethodInvoker)(() =>
                    {
                        jurnalTextBox.AppendText($"\n[SERVER] S-a primit urmatoarea informație:\n{msg}");
                    }));

                    // Procesare comenzi
                    string response = "\n[SERVER] Eroare - Comanda invalida";
                    string[] parts = msg.Split('|');
                    string command = parts[0].ToUpper();
                    string lobbyCode = parts.Length > 1 ? parts[1] : "";
                    switch (command)
                    {
                        case "JOIN":
                            //de implementat
                            response = $"\n[SERVER] Ack - Conectat la jocul {lobbyCode} ca jucator.";
                            break;
                        case "CREATE":
                            //de implementat
                            response = $"\n[SERVER] Ack - Lobby {lobbyCode} creat cu succes.";
                            break;
                        case "CLOSE":
                            //de implementat
                            response = $"\n[SERVER] Ack - Lobby {lobbyCode} a fost inchis.";
                            break;
                        case "SPECTATE":
                            //de implementat
                            response = $"\n[SERVER] Ack - Urmaresti jocul {lobbyCode} ca spectator.";
                            break;
                        default:
                            response = "\n[SERVER] Ack - Mesaj primit (fara comanda).";
                            break;
                    }
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);

                    Invoke((MethodInvoker)(() =>
                    {
                        jurnalTextBox.AppendText($"\n[SERVER] Raspuns trimis: {response}");
                    }));
                }
            }
            catch (Exception ex){
                Invoke((MethodInvoker)(() => jurnalTextBox.AppendText($"\n[SERVER] Eroare comunicare client: {ex.Message}")));
            }

            client.Close();
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\n[SERVER] Se pornește serverul...");
            }));
            Thread serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
            buttonStartServer.Enabled = false;
            buttonStopServer.Enabled = true;
        }

        private void buttonStopServer_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\n[SERVER] Se încearcă oprirea serverului...");
            }));
            foreach (var c in clients) { c.Close(); }

            clients.Clear();
            try { server?.Stop(); }
            catch
            {
                Invoke((MethodInvoker)(() =>
                {
                    jurnalTextBox.AppendText("\n[SERVER] Serverul nu a putut fi oprit...");
                }));
            }
            buttonStartServer.Enabled = true;
            buttonStopServer.Enabled = false;
        }

        private void labelJurnalServer_Click(object sender, EventArgs e)
        {

        }
    }
}

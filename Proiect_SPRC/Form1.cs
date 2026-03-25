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
        bool running = false;

        void StartServer()
        {
            running = true;
            Invoke((MethodInvoker)(() =>
            {
                labelServerStatus.Text = "Status Server: ON  ";
                jurnalTextBox.AppendText("\nSe deschide portul pentru joc (5000)...");
            }));
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            try
            {
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
                    jurnalTextBox.AppendText("\nServerul a fost oprit cu succes.\n");
                    labelServerStatus.Text = "Status Server: OFF ";
                }));
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)(() =>
                {
                    jurnalTextBox.AppendText($"\nEroare server: {ex.Message}\n");

                }));
            }
        }

        void HandleClient(TcpClient client)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\nSe obțin informații din socket...");
            }));
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // OPTIONAL: afișezi în UI
                    Invoke((MethodInvoker)(() =>
                    {
                        jurnalTextBox.AppendText("\nS-a primit urmatoarea informație: \n");
                        jurnalTextBox.AppendText(msg);
                    }));

                    string response = "OK";
                    byte[] data = Encoding.UTF8.GetBytes(response);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch { }

            client.Close();
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\nSe pornește serverul...");
            }));
            Thread serverThread = new Thread(StartServer);
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private void buttonStopServer_Click(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)(() =>
            {
                jurnalTextBox.AppendText("\nSe încearcă oprirea serverului...");
            }));
            foreach (var c in clients) { c.Close(); }

            clients.Clear();
            try { server?.Stop(); }
            catch
            {
                Invoke((MethodInvoker)(() =>
                {
                    jurnalTextBox.AppendText("\nServerul nu a putut fi oprit...");
                }));
            }
            server?.Stop();
            running = false;
        }

        private void labelJurnalServer_Click(object sender, EventArgs e)
        {

        }
        /*
        private void buttonTestSwap_Click(object sender, EventArgs e)
        {
            switch (curSelect)
            {
                case "comenzi":
                    labelJurnalServer.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
                    labelCommand.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    labelJurnalServer.ForeColor = Color.Black;
                    labelCommand.ForeColor = Color.Gray;
                    curSelect = "jurnal";
                    break;
                case "jurnal":
                    labelJurnalServer.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
                    labelCommand.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
                    labelJurnalServer.ForeColor = Color.Gray;
                    labelCommand.ForeColor = Color.Black;
                    curSelect = "comenzi";
                    break;
            }
        }
        */
    }
}

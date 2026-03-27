using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Proiect_SPRC
{
    public partial class ServerMenu : Form
    {
        private Server gameServer = new Server();
        public ServerMenu()
        {
            InitializeComponent();

            gameServer.OnLogReceived += (msg) =>
            {
                Invoke((MethodInvoker)(() => jurnalTextBox.AppendText("\r\n" + msg)));
            };

            gameServer.OnServerStopped += () =>
            {
                Invoke((MethodInvoker)(() => {
                    labelServerStatus.Text = "Status Server: OFF";
                    buttonStartServer.Enabled = true;
                    buttonStopServer.Enabled = false;
                }));
            };
        }

        private void buttonStartServer_Click(object sender, EventArgs e)
        {
            buttonStartServer.Enabled = false;
            buttonStopServer.Enabled = true;
            labelServerStatus.Text = "Status Server: ON ";
            gameServer.Start(5000);
        }

        private void buttonStopServer_Click(object sender, EventArgs e)
        {
            gameServer.Stop();
        }
    }
}

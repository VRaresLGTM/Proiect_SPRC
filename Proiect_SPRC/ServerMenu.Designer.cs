namespace Proiect_SPRC
{
    partial class ServerMenu
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerMenu));
            panel1 = new Panel();
            labelPort = new Label();
            textBoxPort = new TextBox();
            buttonSendCommand = new Button();
            labelJurnalServer = new Label();
            statusStrip = new StatusStrip();
            labelServerStatus = new ToolStripStatusLabel();
            jurnalTextBox = new RichTextBox();
            textBoxCommand = new TextBox();
            labelCommand = new Label();
            buttonStopServer = new Button();
            buttonStartServer = new Button();
            panel1.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(labelPort);
            panel1.Controls.Add(textBoxPort);
            panel1.Controls.Add(buttonSendCommand);
            panel1.Controls.Add(labelJurnalServer);
            panel1.Controls.Add(statusStrip);
            panel1.Controls.Add(jurnalTextBox);
            panel1.Controls.Add(textBoxCommand);
            panel1.Controls.Add(labelCommand);
            panel1.Controls.Add(buttonStopServer);
            panel1.Controls.Add(buttonStartServer);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(568, 437);
            panel1.TabIndex = 0;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(192, 16);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(32, 15);
            labelPort.TabIndex = 9;
            labelPort.Text = "Port:";
            // 
            // textBoxPort
            // 
            textBoxPort.AccessibleDescription = "Introdu comenzi care sa fie executate de server";
            textBoxPort.AccessibleName = "Câmp Comenzi";
            textBoxPort.BackColor = Color.Silver;
            textBoxPort.Location = new Point(230, 13);
            textBoxPort.Name = "textBoxPort";
            textBoxPort.Size = new Size(45, 23);
            textBoxPort.TabIndex = 8;
            textBoxPort.Text = "5000";
            textBoxPort.TextChanged += textBox1_TextChanged;
            // 
            // buttonSendCommand
            // 
            buttonSendCommand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSendCommand.Location = new Point(503, 13);
            buttonSendCommand.Name = "buttonSendCommand";
            buttonSendCommand.Size = new Size(53, 23);
            buttonSendCommand.TabIndex = 7;
            buttonSendCommand.Text = "Trimite";
            buttonSendCommand.UseVisualStyleBackColor = true;
            buttonSendCommand.Click += buttonSendCommand_Click;
            buttonSendCommand.KeyPress += textBoxCommand_KeyPress;
            // 
            // labelJurnalServer
            // 
            labelJurnalServer.AutoSize = true;
            labelJurnalServer.BackColor = Color.Transparent;
            labelJurnalServer.ForeColor = Color.Black;
            labelJurnalServer.Location = new Point(14, 47);
            labelJurnalServer.Name = "labelJurnalServer";
            labelJurnalServer.Size = new Size(76, 15);
            labelJurnalServer.TabIndex = 6;
            labelJurnalServer.Text = "Jurnal Server:";
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { labelServerStatus });
            statusStrip.Location = new Point(0, 413);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(568, 24);
            statusStrip.TabIndex = 5;
            // 
            // labelServerStatus
            // 
            labelServerStatus.BorderSides = ToolStripStatusLabelBorderSides.Right;
            labelServerStatus.BorderStyle = Border3DStyle.Etched;
            labelServerStatus.Margin = new Padding(5, 3, 0, 2);
            labelServerStatus.Name = "labelServerStatus";
            labelServerStatus.Size = new Size(108, 19);
            labelServerStatus.Text = "Status Server: OFF ";
            // 
            // jurnalTextBox
            // 
            jurnalTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            jurnalTextBox.BackColor = Color.Silver;
            jurnalTextBox.BorderStyle = BorderStyle.FixedSingle;
            jurnalTextBox.Location = new Point(14, 65);
            jurnalTextBox.Name = "jurnalTextBox";
            jurnalTextBox.ReadOnly = true;
            jurnalTextBox.Size = new Size(542, 345);
            jurnalTextBox.TabIndex = 3;
            jurnalTextBox.Text = "";
            // 
            // textBoxCommand
            // 
            textBoxCommand.AccessibleDescription = "Introdu comenzi care sa fie executate de server";
            textBoxCommand.AccessibleName = "Câmp Comenzi";
            textBoxCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxCommand.BackColor = Color.Silver;
            textBoxCommand.Location = new Point(355, 13);
            textBoxCommand.Name = "textBoxCommand";
            textBoxCommand.Size = new Size(142, 23);
            textBoxCommand.TabIndex = 2;
            textBoxCommand.TextChanged += textBoxCommand_TextChanged;
            textBoxCommand.KeyPress += textBoxCommand_KeyPress;
            // 
            // labelCommand
            // 
            labelCommand.AccessibleDescription = "Introdu comenzi care sa fie executate de server";
            labelCommand.AccessibleName = "Label Comenzi";
            labelCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelCommand.AutoSize = true;
            labelCommand.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCommand.ForeColor = Color.Black;
            labelCommand.Location = new Point(292, 16);
            labelCommand.Name = "labelCommand";
            labelCommand.Size = new Size(57, 15);
            labelCommand.TabIndex = 0;
            labelCommand.Text = "Comenzi:";
            // 
            // buttonStopServer
            // 
            buttonStopServer.BackColor = Color.Transparent;
            buttonStopServer.Enabled = false;
            buttonStopServer.Location = new Point(96, 12);
            buttonStopServer.Name = "buttonStopServer";
            buttonStopServer.Size = new Size(81, 24);
            buttonStopServer.TabIndex = 1;
            buttonStopServer.Text = "Stop Server";
            buttonStopServer.UseVisualStyleBackColor = false;
            buttonStopServer.Click += buttonStopServer_Click;
            // 
            // buttonStartServer
            // 
            buttonStartServer.BackColor = Color.Transparent;
            buttonStartServer.Location = new Point(12, 12);
            buttonStartServer.Name = "buttonStartServer";
            buttonStartServer.Size = new Size(78, 24);
            buttonStartServer.TabIndex = 0;
            buttonStartServer.Text = "Start Server";
            buttonStartServer.UseVisualStyleBackColor = false;
            buttonStartServer.Click += buttonStartServer_Click;
            // 
            // ServerMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(568, 437);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(500, 300);
            Name = "ServerMenu";
            Text = "Server Șah";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Label labelCommand;
        private Button buttonStopServer;
        private Button buttonStartServer;
        private StatusStrip statusStrip;
        private RichTextBox jurnalTextBox;
        private TextBox textBoxCommand;
        private ToolStripStatusLabel labelServerStatus;
        private Label labelJurnalServer;
        private Label labelSpace;
        private Button buttonSendCommand;
        private TextBox textBoxPort;
        private Label labelPort;
    }
}

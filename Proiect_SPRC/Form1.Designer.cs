namespace Proiect_SPRC
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new Panel();
            labelSpace = new Label();
            labelJurnalServer = new Label();
            statusStrip = new StatusStrip();
            labelServerStatus = new ToolStripStatusLabel();
            labelProgress = new ToolStripStatusLabel();
            toolStripProgressBar1 = new ToolStripProgressBar();
            labelSeparator = new ToolStripStatusLabel();
            jurnalTextBox = new RichTextBox();
            textBox1 = new TextBox();
            labelCommand = new Label();
            buttonStopServer = new Button();
            buttonStartServer = new Button();
            panel1.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(labelSpace);
            panel1.Controls.Add(labelJurnalServer);
            panel1.Controls.Add(statusStrip);
            panel1.Controls.Add(jurnalTextBox);
            panel1.Controls.Add(textBox1);
            panel1.Controls.Add(labelCommand);
            panel1.Controls.Add(buttonStopServer);
            panel1.Controls.Add(buttonStartServer);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(800, 419);
            panel1.TabIndex = 0;
            // 
            // labelSpace
            // 
            labelSpace.AutoSize = true;
            labelSpace.BackColor = Color.Silver;
            labelSpace.Location = new Point(86, 54);
            labelSpace.Name = "labelSpace";
            labelSpace.Size = new Size(358, 15);
            labelSpace.TabIndex = 7;
            labelSpace.Text = "                                                                                                                     ";
            // 
            // labelJurnalServer
            // 
            labelJurnalServer.AutoSize = true;
            labelJurnalServer.BackColor = Color.Silver;
            labelJurnalServer.ForeColor = Color.Black;
            labelJurnalServer.Location = new Point(14, 54);
            labelJurnalServer.Name = "labelJurnalServer";
            labelJurnalServer.Size = new Size(76, 15);
            labelJurnalServer.TabIndex = 6;
            labelJurnalServer.Text = "Jurnal Server:";
            labelJurnalServer.Click += labelJurnalServer_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { labelServerStatus, labelProgress, toolStripProgressBar1, labelSeparator });
            statusStrip.Location = new Point(0, 395);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(800, 24);
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
            // labelProgress
            // 
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(94, 19);
            labelProgress.Text = "Progres Pornire: ";
            // 
            // toolStripProgressBar1
            // 
            toolStripProgressBar1.Name = "toolStripProgressBar1";
            toolStripProgressBar1.Size = new Size(100, 18);
            // 
            // labelSeparator
            // 
            labelSeparator.BorderSides = ToolStripStatusLabelBorderSides.Left;
            labelSeparator.BorderStyle = Border3DStyle.Etched;
            labelSeparator.Margin = new Padding(5, 3, 0, 2);
            labelSeparator.Name = "labelSeparator";
            labelSeparator.Size = new Size(4, 19);
            // 
            // jurnalTextBox
            // 
            jurnalTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            jurnalTextBox.BackColor = Color.Silver;
            jurnalTextBox.BorderStyle = BorderStyle.FixedSingle;
            jurnalTextBox.Location = new Point(12, 52);
            jurnalTextBox.Name = "jurnalTextBox";
            jurnalTextBox.Size = new Size(776, 340);
            jurnalTextBox.TabIndex = 4;
            jurnalTextBox.Text = "";
            // 
            // textBox1
            // 
            textBox1.AccessibleDescription = "Introdu comenzi care sa fie executate de server";
            textBox1.AccessibleName = "Câmp Comenzi";
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBox1.BackColor = Color.Silver;
            textBox1.Location = new Point(271, 13);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(517, 23);
            textBox1.TabIndex = 3;
            // 
            // labelCommand
            // 
            labelCommand.AccessibleDescription = "Introdu comenzi care sa fie executate de server";
            labelCommand.AccessibleName = "Label Comenzi";
            labelCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelCommand.AutoSize = true;
            labelCommand.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCommand.ForeColor = Color.Black;
            labelCommand.Location = new Point(208, 16);
            labelCommand.Name = "labelCommand";
            labelCommand.Size = new Size(57, 15);
            labelCommand.TabIndex = 2;
            labelCommand.Text = "Comenzi:";
            // 
            // buttonStopServer
            // 
            buttonStopServer.Location = new Point(93, 12);
            buttonStopServer.Name = "buttonStopServer";
            buttonStopServer.Size = new Size(75, 23);
            buttonStopServer.TabIndex = 1;
            buttonStopServer.Text = "Stop Server";
            buttonStopServer.UseVisualStyleBackColor = true;
            buttonStopServer.Click += buttonStopServer_Click;
            // 
            // buttonStartServer
            // 
            buttonStartServer.Location = new Point(12, 12);
            buttonStartServer.Name = "buttonStartServer";
            buttonStartServer.Size = new Size(75, 23);
            buttonStartServer.TabIndex = 0;
            buttonStartServer.Text = "Start Server";
            buttonStartServer.UseVisualStyleBackColor = true;
            buttonStartServer.Click += buttonStartServer_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 419);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(500, 300);
            Name = "Form1";
            Text = "Server Șah";
            Load += Form1_Load;
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
        private TextBox textBox1;
        private ToolStripStatusLabel labelServerStatus;
        private ToolStripStatusLabel labelProgress;
        private ToolStripProgressBar toolStripProgressBar1;
        private ToolStripStatusLabel labelSeparator;
        private Label labelJurnalServer;
        private Label labelSpace;
    }
}

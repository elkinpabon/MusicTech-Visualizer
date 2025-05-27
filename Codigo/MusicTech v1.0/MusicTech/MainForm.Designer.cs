namespace MusicTech.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Panel panelSkiaHost;

        private System.Windows.Forms.Button btnLoadFile;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnStop;

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.TrackBar trackProgress;
        private System.Windows.Forms.TrackBar trackVolume;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.panelControls = new System.Windows.Forms.Panel();
            this.btnLoadFile = new System.Windows.Forms.Button();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.trackProgress = new System.Windows.Forms.TrackBar();
            this.lblTime = new System.Windows.Forms.Label();
            this.trackVolume = new System.Windows.Forms.TrackBar();
            this.panelSkiaHost = new System.Windows.Forms.Panel();
            this.panelTop.SuspendLayout();
            this.panelControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackProgress)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panelTop.Controls.Add(this.panelControls);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1278, 112);
            this.panelTop.TabIndex = 1;
            this.panelTop.Paint += new System.Windows.Forms.PaintEventHandler(this.PanelTop_Paint);
            // 
            // panelControls
            // 
            this.panelControls.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.panelControls.BackColor = System.Drawing.Color.Transparent;
            this.panelControls.Controls.Add(this.btnLoadFile);
            this.panelControls.Controls.Add(this.btnPlay);
            this.panelControls.Controls.Add(this.btnPause);
            this.panelControls.Controls.Add(this.btnStop);
            this.panelControls.Controls.Add(this.lblTitle);
            this.panelControls.Controls.Add(this.trackProgress);
            this.panelControls.Controls.Add(this.lblTime);
            this.panelControls.Controls.Add(this.trackVolume);
            this.panelControls.Location = new System.Drawing.Point(275, 2);
            this.panelControls.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelControls.Name = "panelControls";
            this.panelControls.Size = new System.Drawing.Size(881, 104);
            this.panelControls.TabIndex = 0;
            // 
            // btnLoadFile
            // 
            this.btnLoadFile.Location = new System.Drawing.Point(15, 12);
            this.btnLoadFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLoadFile.Name = "btnLoadFile";
            this.btnLoadFile.Size = new System.Drawing.Size(80, 26);
            this.btnLoadFile.TabIndex = 0;
            this.btnLoadFile.Text = "🎵 Abrir";
            this.btnLoadFile.Click += new System.EventHandler(this.BtnLoadFile_Click);
            // 
            // btnPlay
            // 
            this.btnPlay.Location = new System.Drawing.Point(110, 12);
            this.btnPlay.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(60, 26);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "▶";
            this.btnPlay.Click += new System.EventHandler(this.BtnPlay_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(180, 12);
            this.btnPause.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(60, 26);
            this.btnPause.TabIndex = 2;
            this.btnPause.Text = "⏸";
            this.btnPause.Click += new System.EventHandler(this.BtnPause_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(250, 12);
            this.btnStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(60, 26);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "⏹";
            this.btnStop.Click += new System.EventHandler(this.BtnStop_Click);
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Unispace", 7.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTitle.Location = new System.Drawing.Point(316, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(430, 36);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "🎧 Sin archivo cargado";
            this.lblTitle.Click += new System.EventHandler(this.lblTitle_Click);
            // 
            // trackProgress
            // 
            this.trackProgress.Location = new System.Drawing.Point(20, 56);
            this.trackProgress.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.trackProgress.Name = "trackProgress";
            this.trackProgress.Size = new System.Drawing.Size(600, 56);
            this.trackProgress.TabIndex = 5;
            this.trackProgress.Scroll += new System.EventHandler(this.TrackProgress_Scroll);
            // 
            // lblTime
            // 
            this.lblTime.Font = new System.Drawing.Font("Unispace", 7.8F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTime.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.lblTime.Location = new System.Drawing.Point(626, 68);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(153, 24);
            this.lblTime.TabIndex = 6;
            this.lblTime.Text = "00:00 / 00:00";
            this.lblTime.Click += new System.EventHandler(this.lblTime_Click);
            // 
            // trackVolume
            // 
            this.trackVolume.Location = new System.Drawing.Point(785, 12);
            this.trackVolume.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.trackVolume.Maximum = 100;
            this.trackVolume.Name = "trackVolume";
            this.trackVolume.Size = new System.Drawing.Size(80, 56);
            this.trackVolume.TabIndex = 7;
            this.trackVolume.Value = 50;
            this.trackVolume.Scroll += new System.EventHandler(this.TrackVolume_Scroll);
            // 
            // panelSkiaHost
            // 
            this.panelSkiaHost.BackColor = System.Drawing.Color.Black;
            this.panelSkiaHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelSkiaHost.Location = new System.Drawing.Point(0, 112);
            this.panelSkiaHost.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelSkiaHost.Name = "panelSkiaHost";
            this.panelSkiaHost.Size = new System.Drawing.Size(1278, 650);
            this.panelSkiaHost.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1278, 762);
            this.Controls.Add(this.panelSkiaHost);
            this.Controls.Add(this.panelTop);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "🎶 MusicTech Player";
            this.panelTop.ResumeLayout(false);
            this.panelControls.ResumeLayout(false);
            this.panelControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackProgress)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackVolume)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

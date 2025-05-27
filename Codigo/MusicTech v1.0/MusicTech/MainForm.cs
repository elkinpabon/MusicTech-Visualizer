using System;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using MusicTech.Audio;
using MusicTech.Rendering;

namespace MusicTech.Forms
{
    public partial class MainForm : Form
    {
        private SKControl skiaSurface;
        private WaveOutEvent waveOut;
        private AudioFileReader audioFile;
        private string currentFilePath;

        private AudioAnalyzer audioAnalyzer = new AudioAnalyzer();
        private VisualizerEngine visualizer = new VisualizerEngine();

        private DateTime lastFrameTime = DateTime.Now;
        private readonly TimeSpan frameInterval = TimeSpan.FromMilliseconds(1000.0 / 60);
        private bool isPlaying = false;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load; // se ejecuta después de cargar el diseñador
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeSkiaSurface(); // carga visualización al iniciar
            Application.Idle += GameLoop;
        }

        private void InitializeSkiaSurface()
        {
            skiaSurface = new SKControl
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };
            skiaSurface.PaintSurface += SkiaSurface_PaintSurface;
            panelSkiaHost.Controls.Add(skiaSurface); // en el panel especial
        }

        private void SkiaSurface_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            visualizer.IsPlaying = isPlaying;
            visualizer.Magnitudes = audioAnalyzer.Magnitudes;
            visualizer.Render(e.Surface.Canvas, e.Info);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            while (AppStillIdle)
            {
                var now = DateTime.Now;
                if (now - lastFrameTime >= frameInterval)
                {
                    skiaSurface.Invalidate();
                    lastFrameTime = now;

                    if (audioFile != null && isPlaying)
                    {
                        trackProgress.Value = Math.Min((int)audioFile.CurrentTime.TotalSeconds, trackProgress.Maximum);
                        lblTime.Text = audioFile.CurrentTime.ToString(@"mm\:ss") + " / " + audioFile.TotalTime.ToString(@"mm\:ss");
                    }
                }
                Application.DoEvents();
            }
        }

        private bool AppStillIdle
        {
            get
            {
                NativeMethods.PeekMessage(out var msg, IntPtr.Zero, 0, 0, 0);
                return msg.message == 0;
            }
        }

        private void BtnLoadFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Audio Files|*.mp3;*.wav";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = openFileDialog.FileName;
                    LoadAudio(currentFilePath);
                }
            }
        }

        private void LoadAudio(string filePath)
        {
            waveOut?.Dispose();
            audioFile?.Dispose();

            audioFile = new AudioFileReader(filePath);
            audioAnalyzer = new AudioAnalyzer();
            var sampleAggregator = new Utils.SampleAggregator(audioFile.ToSampleProvider(), audioAnalyzer);

            waveOut = new WaveOutEvent();
            waveOut.Init(sampleAggregator);

            lblTitle.Text = "🎧 " + Path.GetFileName(filePath);
            trackProgress.Value = 0;
            trackProgress.Maximum = (int)audioFile.TotalTime.TotalSeconds;
            lblTime.Text = "00:00 / " + audioFile.TotalTime.ToString(@"mm\:ss");

            audioFile.Volume = trackVolume.Value / 100f;
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            waveOut?.Play();
            isPlaying = true;
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            waveOut?.Pause();
            isPlaying = false;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            waveOut?.Stop();
            isPlaying = false;
        }

        private void TrackProgress_Scroll(object sender, EventArgs e)
        {
            if (audioFile != null)
                audioFile.CurrentTime = TimeSpan.FromSeconds(trackProgress.Value);
        }

        private void TrackVolume_Scroll(object sender, EventArgs e)
        {
            if (audioFile != null)
                audioFile.Volume = trackVolume.Value / 100f;
        }

        private void PanelTop_Paint(object sender, PaintEventArgs e)
        {
            var rect = this.panelTop.ClientRectangle;
            using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                System.Drawing.Color.FromArgb(160, 20, 20, 20),
                System.Drawing.Color.FromArgb(160, 50, 50, 50),
                System.Drawing.Drawing2D.LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        private void lblTitle_Click(object sender, EventArgs e)
        {

        }

        private void lblTime_Click(object sender, EventArgs e)
        {

        }
    }
}

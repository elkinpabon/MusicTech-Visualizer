using System;
using System.Collections.Generic;
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
        private IVisualizer visualizer;
        private bool isPlaying = false;

        private DateTime lastFrameTime = DateTime.Now;
        private readonly TimeSpan frameInterval = TimeSpan.FromMilliseconds(1000.0 / 60);

        private Dictionary<string, IVisualizer> visualizers = new Dictionary<string, IVisualizer>();

        public MainForm()
        {
            InitializeComponent();
            InitializeVisualizersMenu();
            visualizer = visualizers["Efecto 1 (Círculo)"];
            this.Load += MainForm_Load;
        }

        private void InitializeVisualizersMenu()
        {
            visualizers.Add("Efecto 1 (Círculo)", new VisualizerEngine());
            visualizers.Add("Efecto 2 (Espectro)", new SpectrumWave());
            visualizers.Add("Efecto 3 (Fractal)", new FractalEffect());
            visualizers.Add("Efecto 4 (Mandelbrot)", new Mandelbrot());


            var visualizerMenu = new ToolStripMenuItem("Efectos de Visualizacion");

            foreach (var entry in visualizers)
            {
                var item = new ToolStripMenuItem(entry.Key) { CheckOnClick = true };
                item.Click += (s, e) =>
                {
                    visualizer = entry.Value;
                    foreach (ToolStripMenuItem mi in visualizerMenu.DropDownItems)
                        mi.Checked = false;
                    item.Checked = true;
                };
                visualizerMenu.DropDownItems.Add(item);
            }

            menuStrip1.Items.Add(visualizerMenu);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            InitializeSkiaSurface();
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
            panelSkiaHost.Controls.Add(skiaSurface);
        }

        private void SkiaSurface_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (visualizer == null || audioAnalyzer == null) return;

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
                    lastFrameTime = now;

                    // Solo repintar si hay algo que mostrar
                    skiaSurface.Invalidate();

                    if (audioFile != null && isPlaying)
                    {
                        try
                        {
                            double current = audioFile.CurrentTime.TotalSeconds;
                            double total = audioFile.TotalTime.TotalSeconds;

                            // Validación de rango
                            if (total > 0 &&
                                !double.IsNaN(current) && !double.IsInfinity(current) &&
                                !double.IsNaN(total) && !double.IsInfinity(total))
                            {
                                int max = Math.Max(trackProgress.Maximum, 1);
                                int currentSec = (int)Math.Max(0, Math.Min(current, max));

                                // Actualiza barra y tiempo si han cambiado
                                if (trackProgress.Value != currentSec)
                                {
                                    trackProgress.Value = currentSec;

                                    lblTime.Text = TimeSpan.FromSeconds(currentSec).ToString(@"mm\:ss") +
                                                   " / " + TimeSpan.FromSeconds(total).ToString(@"mm\:ss");
                                }
                            }
                        }
                        catch
                        {
                            // Silencia errores por valores inesperados del archivo
                        }
                    }
                }

                // Mínimo respiro para evitar freeze (solo 1 por ciclo)
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
            try
            {
                waveOut?.Dispose();
                audioFile?.Dispose();

                audioFile = new AudioFileReader(filePath);
                audioAnalyzer = new AudioAnalyzer();
                var sampleAggregator = new Utils.SampleAggregator(audioFile.ToSampleProvider(), audioAnalyzer);

                waveOut = new WaveOutEvent();
                waveOut.Init(sampleAggregator);

                lblTitle.Text = "🎧 " + Path.GetFileName(filePath);

                double totalSeconds = audioFile.TotalTime.TotalSeconds;
                if (double.IsNaN(totalSeconds) || double.IsInfinity(totalSeconds) || totalSeconds <= 0)
                {
                    throw new FormatException("Duración inválida en el archivo de audio.");
                }

                trackProgress.Value = 0;
                trackProgress.Maximum = (int)Math.Max(totalSeconds, 1);
                lblTime.Text = "00:00 / " + TimeSpan.FromSeconds(totalSeconds).ToString(@"mm\:ss");

                audioFile.Volume = trackVolume.Value / 100f;
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Error de formato en el archivo:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el archivo:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            if (rect.Width > 0 && rect.Height > 0)
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect,
                    System.Drawing.Color.FromArgb(160, 20, 20, 20),
                    System.Drawing.Color.FromArgb(160, 50, 50, 50),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
            }
        }

        private void lblTitle_Click(object sender, EventArgs e) { }
        private void lblTime_Click(object sender, EventArgs e) { }
    }
}

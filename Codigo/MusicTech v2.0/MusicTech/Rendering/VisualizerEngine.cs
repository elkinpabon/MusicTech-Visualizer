using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTech.Rendering
{
    public class VisualizerEngine : IVisualizer
    {
        public float[] Magnitudes { get; set; }
        public bool IsPlaying { get; set; } = false;

        private Random random = new Random();
        private float time = 0f;
        private float backgroundHue = 0f;

        // Historial de magnitudes optimizado
        private Queue<float> magnitudeHistory = new Queue<float>(MAX_HISTORY);
        private const int MAX_HISTORY = 30;

        // Sistema de partículas optimizado
        private List<Particle> particles = new List<Particle>(MAX_PARTICLES);
        private const int MAX_PARTICLES = 100; // Reducido de 200 a 100

        // Efectos visuales
        private float waveIntensity = 0f;
        private float pulseSize = 1f;
        private float rotationSpeed = 0.5f;
        private float kaleidoscopeIntensity = 0f;

        // Detectores de ritmo
        private float beatDetectionThreshold = 0.6f;
        private float lastBeatTime = 0f;
        private bool beatDetected = false;
        private float beatCooldown = 0.2f;

        // Cache de valores calculados para evitar recálculos
        private float cachedMaxMagnitude = 0f;
        private float cachedAvgMagnitude = 0f;
        private float magnitudeHistorySum = 0f;
        private int frameCounter = 0;

        // Paleta de colores pre-calculada
        private SKColor[] colorPalette = new SKColor[]
        {
            SKColor.FromHsv(0, 100, 100),    // Rojo
            SKColor.FromHsv(45, 100, 100),   // Naranja
            SKColor.FromHsv(60, 100, 100),   // Amarillo
            SKColor.FromHsv(120, 100, 100),  // Verde
            SKColor.FromHsv(240, 100, 100),  // Azul
            SKColor.FromHsv(280, 100, 100),  // Púrpura
            SKColor.FromHsv(320, 100, 100)   // Rosa
        };

        // Objetos reutilizables para evitar allocaciones
        private SKPaint reusablePaint = new SKPaint { IsAntialias = true };
        private SKPath reusablePath = new SKPath();
        private SKPoint[] gradientPoints = new SKPoint[2];
        private SKColor[] gradientColors = new SKColor[2];
        private float[] gradientPositions = new float[] { 0, 1 };

        // Configuraciones de rendimiento
        private const int SPECTRUM_PARTICLES_PER_BAND = 8; // Reducido de 20 a 8
        private const int WAVE_PATTERN_LINES = 8; // Reducido de variable a constante
        private const int KALEIDOSCOPE_SEGMENTS = 6; // Reducido de 8 a 6
        private const int CENTRAL_PULSE_RINGS = 3; // Reducido de 5 a 3

        // Control de frame rate
        private const float TARGET_FRAME_TIME = 1f / 60f; // 60 FPS
        private const int OPTIMIZATION_FRAME_SKIP = 2; // Actualizar algunos efectos cada 2 frames

        public VisualizerEngine()
        {
            // Inicializar historial de magnitudes
            for (int i = 0; i < MAX_HISTORY; i++)
            {
                magnitudeHistory.Enqueue(0f);
            }
        }

        public void Render(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null) return;

            frameCounter++;
            bool isOptimizationFrame = frameCounter % OPTIMIZATION_FRAME_SKIP == 0;

            // Calcular magnitudes solo cuando es necesario
            if (isOptimizationFrame || frameCounter == 1)
            {
                UpdateMagnitudeCache();
            }

            // Detectar beats
            DetectBeats(cachedMaxMagnitude);

            // Actualizar efectos basados en música
            UpdateMusicDrivenEffects(cachedMaxMagnitude, cachedAvgMagnitude);

            // Renderizar elementos (algunos con optimización de frames)
            RenderBackground(canvas, info);
            RenderWaveform(canvas, info);
            RenderSpectrum(canvas, info);

            if (isOptimizationFrame)
            {
                RenderParticles(canvas, info);
            }

            RenderCentralPulse(canvas, info);

            if (kaleidoscopeIntensity > 0.2f) // Solo renderizar si es visible
            {
                RenderKaleidoscope(canvas, info);
            }

            // Actualizar tiempo
            if (IsPlaying)
                time += TARGET_FRAME_TIME;
        }

        private void UpdateMagnitudeCache()
        {
            if (Magnitudes.Length == 0) return;

            cachedMaxMagnitude = 0f;
            float sum = 0f;

            // Un solo loop para calcular max y average
            for (int i = 0; i < Magnitudes.Length; i++)
            {
                float mag = Magnitudes[i];
                if (mag > cachedMaxMagnitude)
                    cachedMaxMagnitude = mag;
                sum += mag;
            }

            cachedAvgMagnitude = sum / Magnitudes.Length;

            // Actualizar historial de magnitudes
            if (IsPlaying)
            {
                if (magnitudeHistory.Count >= MAX_HISTORY)
                {
                    magnitudeHistorySum -= magnitudeHistory.Dequeue();
                }
                magnitudeHistory.Enqueue(cachedMaxMagnitude);
                magnitudeHistorySum += cachedMaxMagnitude;
            }
        }

        private void DetectBeats(float currentMagnitude)
        {
            if (!IsPlaying) return;

            // Usar suma pre-calculada en lugar de Average()
            float recentAvg = magnitudeHistorySum / magnitudeHistory.Count;

            if (currentMagnitude > recentAvg * (1 + beatDetectionThreshold) &&
                (time - lastBeatTime) > beatCooldown)
            {
                beatDetected = true;
                lastBeatTime = time;
                TriggerBeatEffects();
            }
            else
            {
                beatDetected = false;
            }
        }

        private void TriggerBeatEffects()
        {
            // Crear menos partículas para mejor rendimiento
            CreateParticleExplosion(20); // Reducido de 50 a 20

            pulseSize = 1.8f;
            waveIntensity += 0.5f;
            kaleidoscopeIntensity = 1.0f;
            rotationSpeed *= 0.9f;
            backgroundHue = (backgroundHue + 40) % 360;
        }

        private void UpdateMusicDrivenEffects(float maxMagnitude, float avgMagnitude)
        {
            if (!IsPlaying) return;

            backgroundHue = (backgroundHue + maxMagnitude * 20f) % 360;

            // Usar multiplicadores pre-calculados
            pulseSize = Math.Max(1.0f, pulseSize * 0.95f);
            waveIntensity = Math.Max(0.1f, waveIntensity * 0.97f);
            kaleidoscopeIntensity = Math.Max(0.0f, kaleidoscopeIntensity * 0.95f);
            rotationSpeed = Math.Max(0.2f, rotationSpeed * 0.77f);

            // Generar partículas con menos frecuencia
            if (random.NextDouble() < avgMagnitude)
            {
                CreateRandomParticle();
            }
        }

        private void RenderBackground(SKCanvas canvas, SKImageInfo info)
        {
            reusablePaint.Style = SKPaintStyle.Fill;

            // Configurar gradient points una sola vez
            gradientPoints[0] = new SKPoint(info.Width / 2f, info.Height / 2f);
            gradientColors[0] = SKColor.FromHsv(backgroundHue, 70, 20);
            gradientColors[1] = SKColor.FromHsv((backgroundHue + 180) % 360, 80, 10);

            reusablePaint.Shader = SKShader.CreateRadialGradient(
                gradientPoints[0],
                info.Width * 0.7f,
                gradientColors,
                gradientPositions,
                SKShaderTileMode.Clamp);

            canvas.DrawRect(0, 0, info.Width, info.Height, reusablePaint);

            // Patrón de ondas simplificado
            DrawWavePattern(canvas, info);
        }

        private void DrawWavePattern(SKCanvas canvas, SKImageInfo info)
        {
            reusablePaint.Style = SKPaintStyle.Stroke;
            reusablePaint.StrokeWidth = 2f;
            reusablePaint.Shader = null;

            for (int i = 0; i < WAVE_PATTERN_LINES; i++)
            {
                byte alpha = (byte)(40 + (i % 2) * 40);
                reusablePaint.Color = SKColor.FromHsv((backgroundHue + i * 10) % 360, 80, 50, alpha);

                reusablePath.Reset();
                reusablePath.MoveTo(0, 0);

                // Usar paso más grande para menos puntos
                for (float x = 0; x < info.Width; x += 20)
                {
                    float amplitude = info.Height * 0.1f * (1 + waveIntensity * 0.5f);
                    float frequency = 0.005f + (i * 0.001f);
                    float phase = time * (0.5f + i * 0.1f);

                    float y = info.Height / 2f +
                              amplitude * (float)Math.Sin(x * frequency + phase) +
                              amplitude * 0.5f * (float)Math.Sin(x * frequency * 2 + phase * 1.5f);

                    reusablePath.LineTo(x, y);
                }

                canvas.DrawPath(reusablePath, reusablePaint);
            }
        }

        private void RenderWaveform(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null || Magnitudes.Length == 0) return;

            reusablePaint.Style = SKPaintStyle.Stroke;
            reusablePaint.StrokeWidth = 3f;

            reusablePath.Reset();
            float stepX = info.Width / (float)Magnitudes.Length;
            float centerY = info.Height / 2f;

            reusablePath.MoveTo(0, centerY);

            for (int i = 0; i < Magnitudes.Length; i++)
            {
                float x = i * stepX;
                float intensity = Magnitudes[i] * 4000f * (1 + waveIntensity);
                float y = centerY + intensity * (float)Math.Sin(time * 2 + i * 0.2f);
                reusablePath.LineTo(x, y);
            }

            reusablePaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(info.Width, 0),
                colorPalette,
                null,
                SKShaderTileMode.Clamp);

            canvas.DrawPath(reusablePath, reusablePaint);
        }

        private void RenderSpectrum(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null) return;

            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;

            reusablePaint.Style = SKPaintStyle.Fill;
            reusablePaint.Shader = null;

            // Pre-calcular valores que no cambian en el loop
            float baseRotationSpeed = time * rotationSpeed * 6f;
            float baseTimeComponent = time * 4f;

            for (int i = 0; i < Magnitudes.Length; i++)
            {
                float intensity = Magnitudes[i] * 5000f;
                if (intensity < 0.1f) continue; // Skip if too small

                for (int j = 0; j < SPECTRUM_PARTICLES_PER_BAND; j++)
                {
                    float baseRadius = 100f + intensity * 5f + (float)Math.Sin(time * 0.8f + i * 0.1f) * 50f;
                    baseRadius *= pulseSize * (1 + 0.2f * (float)Math.Sin(baseTimeComponent + i * 0.2f));

                    float angle = (i * SPECTRUM_PARTICLES_PER_BAND + j) * (360f / (Magnitudes.Length * SPECTRUM_PARTICLES_PER_BAND)) + baseRotationSpeed;
                    float radians = angle * (float)Math.PI / 180f;

                    float distortion = 1f + intensity * 0.1f * (float)Math.Sin(time * 3 + i * 0.3f + j * 0.1f);
                    float radius = baseRadius * distortion;

                    float x = centerX + radius * (float)Math.Cos(radians);
                    float y = centerY + radius * (float)Math.Sin(radians);

                    float size = 2f + Magnitudes[i] * 80f * pulseSize;
                    float hueShift = (i * 360f / Magnitudes.Length + j * 18 + time * 30) % 360;

                    reusablePaint.Color = SKColor.FromHsv(hueShift, 100, 100, 200);

                    if (beatDetected && j == 0) // Solo algunas partículas con brillo
                    {
                        reusablePaint.Color = SKColor.FromHsv(hueShift, 100, 100, 100);
                        canvas.DrawCircle(x, y, size * 2f, reusablePaint);
                        reusablePaint.Color = SKColor.FromHsv(hueShift, 100, 100, 200);
                        size *= 1.5f;
                    }

                    canvas.DrawCircle(x, y, size, reusablePaint);
                }
            }
        }

        private void RenderKaleidoscope(SKCanvas canvas, SKImageInfo info)
        {
            if (kaleidoscopeIntensity <= 0.1f) return;

            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;
            float radius = Math.Min(info.Width, info.Height) * 0.4f;

            reusablePaint.Style = SKPaintStyle.Fill;
            reusablePaint.Shader = null;

            for (int segment = 0; segment < KALEIDOSCOPE_SEGMENTS; segment++)
            {
                float startAngle = segment * (360f / KALEIDOSCOPE_SEGMENTS);
                float endAngle = (segment + 1) * (360f / KALEIDOSCOPE_SEGMENTS);

                reusablePath.Reset();
                reusablePath.MoveTo(centerX, centerY);

                float startRad = startAngle * (float)Math.PI / 180f;
                float endRad = endAngle * (float)Math.PI / 180f;

                reusablePath.LineTo(
                    centerX + radius * (float)Math.Cos(startRad),
                    centerY + radius * (float)Math.Sin(startRad));

                reusablePath.ArcTo(new SKRect(
                    centerX - radius,
                    centerY - radius,
                    centerX + radius,
                    centerY + radius),
                    startAngle,
                    (endAngle - startAngle),
                    false);

                reusablePath.Close();

                reusablePaint.Color = SKColor.FromHsv(
                    (backgroundHue + segment * (360f / KALEIDOSCOPE_SEGMENTS) + time * 10) % 360,
                    90,
                    70,
                    (byte)(50 * kaleidoscopeIntensity));

                canvas.DrawPath(reusablePath, reusablePaint);
            }
        }

        private void RenderCentralPulse(SKCanvas canvas, SKImageInfo info)
        {
            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;
            float baseRadius = 50f * pulseSize;

            reusablePaint.Style = SKPaintStyle.Stroke;
            reusablePaint.Shader = null;

            for (int i = 0; i < CENTRAL_PULSE_RINGS; i++)
            {
                float phase = time * 2f + i * (float)Math.PI / CENTRAL_PULSE_RINGS;
                float pulseFactor = 1f + 0.5f * (float)Math.Sin(phase);
                float radius = baseRadius * (1 + i * 0.5f) * pulseFactor;

                reusablePaint.StrokeWidth = 2f + 3f * (float)Math.Sin(phase * 0.7f + i);
                reusablePaint.Color = SKColor.FromHsv(
                    (backgroundHue + i * 30 + time * 20) % 360,
                    100,
                    100,
                    (byte)(150 + 100 * (float)Math.Sin(phase)));

                canvas.DrawCircle(centerX, centerY, radius, reusablePaint);
            }

            // Resplandor central optimizado
            if (beatDetected)
            {
                gradientPoints[0] = new SKPoint(centerX, centerY);
                gradientColors[0] = SKColor.FromHsv(backgroundHue, 100, 100, 200);
                gradientColors[1] = SKColor.FromHsv(backgroundHue, 100, 100, 0);

                reusablePaint.Style = SKPaintStyle.Fill;
                reusablePaint.Shader = SKShader.CreateRadialGradient(
                    gradientPoints[0],
                    baseRadius * 2f,
                    gradientColors,
                    gradientPositions,
                    SKShaderTileMode.Clamp);

                canvas.DrawCircle(centerX, centerY, baseRadius * 3f, reusablePaint);
            }
        }

        private void CreateRandomParticle()
        {
            if (particles.Count >= MAX_PARTICLES) return;

            Particle particle = new Particle
            {
                X = random.Next(-20, 20),
                Y = random.Next(-20, 20),
                Size = 1f + (float)random.NextDouble() * 4f,
                SpeedX = -5f + (float)random.NextDouble() * 10f,
                SpeedY = -5f + (float)random.NextDouble() * 10f,
                Hue = (backgroundHue + random.Next(0, 180)) % 360,
                Opacity = (byte)random.Next(100, 255),
                LifeTime = 3f + (float)random.NextDouble() * 5f,
                CurrentLife = 0f
            };

            particles.Add(particle);
        }

        private void CreateParticleExplosion(int count)
        {
            for (int i = 0; i < count && particles.Count < MAX_PARTICLES; i++)
            {
                float angle = (float)random.NextDouble() * 360f;
                float speed = 2f + (float)random.NextDouble() * 8f;
                float radians = angle * (float)Math.PI / 180f;

                Particle particle = new Particle
                {
                    X = random.Next(-20, 20),
                    Y = random.Next(-20, 20),
                    Size = 2f + (float)random.NextDouble() * 6f,
                    SpeedX = speed * (float)Math.Cos(radians),
                    SpeedY = speed * (float)Math.Sin(radians),
                    Hue = (backgroundHue + random.Next(0, 360)) % 360,
                    Opacity = (byte)random.Next(150, 255),
                    LifeTime = 1f + (float)random.NextDouble() * 3f,
                    CurrentLife = 0f
                };

                particles.Add(particle);
            }
        }

        private void RenderParticles(SKCanvas canvas, SKImageInfo info)
        {
            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;

            reusablePaint.Style = SKPaintStyle.Fill;
            reusablePaint.Shader = null;

            for (int i = particles.Count - 1; i >= 0; i--)
            {
                Particle p = particles[i];

                if (IsPlaying)
                {
                    p.X += p.SpeedX;
                    p.Y += p.SpeedY;

                    // Aplicar fuerzas de audio simplificadas
                    if (Magnitudes != null && Magnitudes.Length > 0)
                    {
                        int freqBand = i % Magnitudes.Length;
                        float audioForce = Magnitudes[freqBand] * 50f; // Reducido para mejor rendimiento

                        float angleToCenter = (float)Math.Atan2(p.Y, p.X);
                        p.SpeedX += audioForce * (float)Math.Cos(angleToCenter + Math.PI / 2) * 0.05f;
                        p.SpeedY += audioForce * (float)Math.Sin(angleToCenter + Math.PI / 2) * 0.05f;

                        // Limitación de velocidad simplificada
                        float speedSq = p.SpeedX * p.SpeedX + p.SpeedY * p.SpeedY;
                        if (speedSq > 225f) // 15^2
                        {
                            float invSpeed = 15f / (float)Math.Sqrt(speedSq);
                            p.SpeedX *= invSpeed;
                            p.SpeedY *= invSpeed;
                        }
                    }

                    p.CurrentLife += TARGET_FRAME_TIME;
                }

                // Renderizar partícula
                float x = centerX + p.X;
                float y = centerY + p.Y;
                float remainingLifeRatio = Math.Max(0, 1 - (p.CurrentLife / p.LifeTime));

                if (remainingLifeRatio <= 0)
                {
                    particles.RemoveAt(i);
                    continue;
                }

                float hue = (p.Hue + time * 20) % 360;
                byte opacity = (byte)(p.Opacity * remainingLifeRatio);
                reusablePaint.Color = SKColor.FromHsv(hue, 100, 100, opacity);

                float size = p.Size * remainingLifeRatio;

                // Simplificar formas - solo círculos para mejor rendimiento
                canvas.DrawCircle(x, y, size, reusablePaint);

                particles[i] = p;
            }
        }

        // Limpiar recursos al finalizar
        public void Dispose()
        {
            reusablePaint?.Dispose();
            reusablePath?.Dispose();
        }
    }

    /// <summary>
    /// Estructura optimizada para partículas
    /// </summary>
    public struct Particle
    {
        public float X;
        public float Y;
        public float Size;
        public float SpeedX;
        public float SpeedY;
        public float Hue;
        public byte Opacity;
        public float LifeTime;
        public float CurrentLife;
    }
}
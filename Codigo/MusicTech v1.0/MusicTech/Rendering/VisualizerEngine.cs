using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTech.Rendering
{
    public class VisualizerEngine
    {
        public float[] Magnitudes { get; set; }
        public bool IsPlaying { get; set; } = false;

        private Random random = new Random();
        private float time = 0f;
        private float backgroundHue = 0f;

        // Historial de magnitudes para efectos que responden al ritmo
        private Queue<float> magnitudeHistory = new Queue<float>();
        private const int MAX_HISTORY = 30;

        // Configuración para el sistema de partículas
        private List<Particle> particles = new List<Particle>();
        private const int MAX_PARTICLES = 500;

        // Efectos visuales
        private float waveIntensity = 0f;
        private float pulseSize = 1f;
        // Velocidad de rotación (valor más bajo = rotación más lenta)
        private float rotationSpeed = 0.5f;
        private float kaleidoscopeIntensity = 0f;

        // Detectores de ritmo
        private float beatDetectionThreshold = 0.6f;
        private float lastBeatTime = 0f;
        private bool beatDetected = false;
        private float beatCooldown = 0.2f; // Tiempo mínimo entre beats

        // Configuración de colores
        private List<SKColor> colorPalette = new List<SKColor>
        {
            SKColor.FromHsv(0, 100, 100),    // Rojo
            SKColor.FromHsv(45, 100, 100),   // Naranja
            SKColor.FromHsv(60, 100, 100),   // Amarillo
            SKColor.FromHsv(120, 100, 100),  // Verde
            SKColor.FromHsv(240, 100, 100),  // Azul
            SKColor.FromHsv(280, 100, 100),  // Púrpura
            SKColor.FromHsv(320, 100, 100)   // Rosa
        };

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

            // Calcular la magnitud máxima actual
            float maxMagnitude = Magnitudes.Max();
            float avgMagnitude = Magnitudes.Average();

            // Actualizar historial de magnitudes
            if (IsPlaying)
            {
                magnitudeHistory.Enqueue(maxMagnitude);
                if (magnitudeHistory.Count > MAX_HISTORY)
                    magnitudeHistory.Dequeue();
            }

            // Detectar beats
            DetectBeats(maxMagnitude);

            // Actualizar variables basadas en la música
            UpdateMusicDrivenEffects(maxMagnitude, avgMagnitude);

            // Renderizar fondo
            RenderBackground(canvas, info);

            // Renderizar elementos visuales
            RenderKaleidoscope(canvas, info);
            RenderWaveform(canvas, info);
            RenderSpectrum(canvas, info);
            RenderParticles(canvas, info);

            // Renderizar pulso central
            RenderCentralPulse(canvas, info);

            // Actualizar tiempo y estado
            if (IsPlaying)
                time += 0.05f;
        }

        private void DetectBeats(float currentMagnitude)
        {
            if (!IsPlaying) return;

            // Calcular el promedio de magnitudes recientes
            float recentAvg = magnitudeHistory.Average();

            // Detectar un beat cuando la magnitud excede el umbral y el tiempo de cooldown ha pasado
            if (currentMagnitude > recentAvg * (1 + beatDetectionThreshold) && (time - lastBeatTime) > beatCooldown)
            {
                beatDetected = true;
                lastBeatTime = time;

                // Generar efectos especiales en el beat
                TriggerBeatEffects();
            }
            else
            {
                beatDetected = false;
            }
        }

        private void TriggerBeatEffects()
        {
            // Crear una explosión de partículas
            CreateParticleExplosion(50);

            // Incrementar temporalmente la intensidad de efectos
            pulseSize = 1.8f;
            waveIntensity += 0.5f;
            kaleidoscopeIntensity = 1.0f;
            rotationSpeed *= 0.9f; // Aumentar menos en cada beat

            // Cambiar el color de fondo
            backgroundHue = (backgroundHue + 40) % 360;
        }

        private void UpdateMusicDrivenEffects(float maxMagnitude, float avgMagnitude)
        {
            if (!IsPlaying) return;

            // Actualizar efectos basados en la música
            backgroundHue = (backgroundHue + maxMagnitude * 20f) % 360;

            // Disminuir gradualmente los efectos de beat
            pulseSize = Math.Max(1.0f, pulseSize * 0.95f);
            waveIntensity = Math.Max(0.1f, waveIntensity * 0.97f);
            kaleidoscopeIntensity = Math.Max(0.0f, kaleidoscopeIntensity * 0.95f);
            rotationSpeed = Math.Max(0.2f, rotationSpeed * 0.77f); // Reducción más pronunciada y valor mínimo más bajo

            // Generar partículas continuamente basadas en la intensidad de la música
            if (random.NextDouble() < avgMagnitude * 2)
            {
                CreateRandomParticle();
            }
        }

        private void RenderBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Crear un fondo con múltiples capas de degradados
            using (var backgroundPaint = new SKPaint())
            {
                // Primera capa: degradado radial
                backgroundPaint.Shader = SKShader.CreateRadialGradient(
                    new SKPoint(info.Width / 2f, info.Height / 2f),
                    info.Width * 0.7f,
                    new[] {
                        SKColor.FromHsv(backgroundHue, 70, 20),
                        SKColor.FromHsv((backgroundHue + 180) % 360, 80, 10)
                    },
                    new float[] { 0, 1 },
                    SKShaderTileMode.Clamp);
                canvas.DrawRect(new SKRect(0, 0, info.Width, info.Height), backgroundPaint);

                // Segunda capa: patrón de ondas
                DrawWavePattern(canvas, info);
            }
        }

        private void DrawWavePattern(SKCanvas canvas, SKImageInfo info)
        {
            using (var wavePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2f })
            {
                // Calcular el número de líneas basado en la intensidad
                int numLines = 10 + (int)(waveIntensity * 5);

                for (int i = 0; i < numLines; i++)
                {
                    float alpha = 40 + (i % 2) * 40;
                    wavePaint.Color = SKColor.FromHsv((backgroundHue + i * 10) % 360, 80, 50, (byte)alpha);

                    using (var path = new SKPath())
                    {
                        path.MoveTo(0, 0);

                        for (float x = 0; x < info.Width; x += 10)
                        {
                            float amplitude = info.Height * 0.1f * (1 + waveIntensity * 0.5f);
                            float frequency = 0.005f + (i * 0.001f);
                            float phase = time * (0.5f + i * 0.1f);

                            float y = info.Height / 2f +
                                      amplitude * (float)Math.Sin(x * frequency + phase) +
                                      amplitude * 0.5f * (float)Math.Sin(x * frequency * 2 + phase * 1.5f);

                            path.LineTo(x, y);
                        }

                        canvas.DrawPath(path, wavePaint);
                    }
                }
            }
        }

        private void RenderWaveform(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null || Magnitudes.Length == 0) return;

            using (var wavePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3f })
            {
                using (var path = new SKPath())
                {
                    float stepX = info.Width / (float)Magnitudes.Length;
                    float centerY = info.Height / 2f;

                    path.MoveTo(0, centerY);

                    for (int i = 0; i < Magnitudes.Length; i++)
                    {
                        float x = i * stepX;
                        float intensity = Magnitudes[i] * 4000f * (1 + waveIntensity);
                        float y = centerY + intensity * (float)Math.Sin(time * 2 + i * 0.2f);

                        path.LineTo(x, y);
                    }

                    // Aplicar un shader de degradado al trazo
                    wavePaint.Shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, 0),
                        new SKPoint(info.Width, 0),
                        colorPalette.ToArray(),
                        null,
                        SKShaderTileMode.Clamp);

                    canvas.DrawPath(path, wavePaint);
                }
            }
        }

        private void RenderSpectrum(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null) return;

            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;

            using (var paint = new SKPaint { IsAntialias = true })
            {
                int particlesPerBand = 20;

                for (int i = 0; i < Magnitudes.Length; i++)
                {
                    for (int j = 0; j < particlesPerBand; j++)
                    {
                        float intensity = Magnitudes[i] * 5000f;
                        float baseRadius = 100f + intensity * 5f + (float)Math.Sin(time * 0.8f + i * 0.1f) * 50f;

                        // Añadir distorsión al radio basada en el beat
                        baseRadius *= pulseSize * (1 + 0.2f * (float)Math.Sin(time * 4 + i * 0.2f));

                        float angle = (i * particlesPerBand + j) * (360f / (Magnitudes.Length * particlesPerBand)) + time * rotationSpeed * 6f; // Factor multiplicador más bajo para rotación más lenta
                        float radians = angle * (float)Math.PI / 180f;

                        float radius = baseRadius;

                        // Aplicar distorsión de la forma
                        float distortion = 1f + intensity * 0.1f * (float)Math.Sin(time * 3 + i * 0.3f + j * 0.1f);
                        radius *= distortion;

                        float x = centerX + radius * (float)Math.Cos(radians);
                        float y = centerY + radius * (float)Math.Sin(radians);

                        // Tamaño basado en la intensidad y el pulso
                        float size = 2f + Magnitudes[i] * 80f * pulseSize + (float)(random.NextDouble() * 2);

                        // Color psicodélico cambiante
                        float hueShift = (i * 360f / Magnitudes.Length + j * 18 + time * 30 + (float)(random.NextDouble() * 60)) % 360;

                        // Aumentar la saturación y el brillo para un efecto más psicodélico
                        paint.Color = SKColor.FromHsv(hueShift, 100, 100, 200);

                        // Añadir un efecto de brillo
                        if (beatDetected && random.NextDouble() < 0.3)
                        {
                            // Crear un resplandor alrededor de algunas partículas
                            using (var glowPaint = new SKPaint { IsAntialias = true })
                            {
                                glowPaint.Color = SKColor.FromHsv(hueShift, 100, 100, 100);
                                canvas.DrawCircle(x, y, size * 2f, glowPaint);
                            }

                            // Aumentar temporalmente el tamaño para el efecto de pulso
                            size *= 1.5f;
                        }

                        canvas.DrawCircle(x, y, size, paint);
                    }
                }
            }
        }

        private void RenderKaleidoscope(SKCanvas canvas, SKImageInfo info)
        {
            if (kaleidoscopeIntensity <= 0.1f) return;

            int numSegments = 8;
            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;
            float radius = Math.Min(info.Width, info.Height) * 0.4f;

            using (var paint = new SKPaint { IsAntialias = true })
            {
                for (int segment = 0; segment < numSegments; segment++)
                {
                    // Dibujar segmentos del caleidoscopio
                    float startAngle = segment * (360f / numSegments);
                    float endAngle = (segment + 1) * (360f / numSegments);

                    using (var path = new SKPath())
                    {
                        path.MoveTo(centerX, centerY);

                        float startRad = startAngle * (float)Math.PI / 180f;
                        float endRad = endAngle * (float)Math.PI / 180f;

                        path.LineTo(
                            centerX + radius * (float)Math.Cos(startRad),
                            centerY + radius * (float)Math.Sin(startRad));

                        path.ArcTo(new SKRect(
                            centerX - radius,
                            centerY - radius,
                            centerX + radius,
                            centerY + radius),
                            startAngle,
                            (endAngle - startAngle),
                            false);

                        path.Close();

                        // Color basado en el tiempo y el segmento
                        paint.Color = SKColor.FromHsv(
                            (backgroundHue + segment * (360f / numSegments) + time * 10) % 360,
                            90,
                            70,
                            (byte)(50 * kaleidoscopeIntensity));

                        canvas.DrawPath(path, paint);
                    }
                }
            }
        }

        private void RenderCentralPulse(SKCanvas canvas, SKImageInfo info)
        {
            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;

            // Radio base que pulsa con la música
            float baseRadius = 50f * pulseSize;

            // Crear múltiples anillos pulsantes
            using (var pulsePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke })
            {
                int numRings = 5;

                for (int i = 0; i < numRings; i++)
                {
                    float phase = time * 2f + i * (float)Math.PI / numRings;
                    float pulseFactor = 1f + 0.5f * (float)Math.Sin(phase);
                    float radius = baseRadius * (1 + i * 0.5f) * pulseFactor;

                    // Variar el grosor del trazo
                    pulsePaint.StrokeWidth = 2f + 3f * (float)Math.Sin(phase * 0.7f + i);

                    // Color que cambia con el tiempo
                    pulsePaint.Color = SKColor.FromHsv(
                        (backgroundHue + i * 30 + time * 20) % 360,
                        100,
                        100,
                        (byte)(150 + 100 * (float)Math.Sin(phase)));

                    canvas.DrawCircle(centerX, centerY, radius, pulsePaint);
                }

                // Añadir un resplandor central cuando se detecta un beat
                if (beatDetected)
                {
                    using (var glowPaint = new SKPaint { IsAntialias = true })
                    {
                        glowPaint.Shader = SKShader.CreateRadialGradient(
                            new SKPoint(centerX, centerY),
                            baseRadius * 2f,
                            new[] {
                                SKColor.FromHsv(backgroundHue, 100, 100, 200),
                                SKColor.FromHsv(backgroundHue, 100, 100, 0)
                            },
                            new float[] { 0, 1 },
                            SKShaderTileMode.Clamp);

                        canvas.DrawCircle(centerX, centerY, baseRadius * 3f, glowPaint);
                    }
                }
            }
        }

        private void CreateRandomParticle()
        {
            if (particles.Count >= MAX_PARTICLES) return;

            Particle particle = new Particle
            {
                X = random.Next(0, 100) < 50 ? -20 : -20 + random.Next(0, 40),
                Y = random.Next(0, 100) < 50 ? -20 : -20 + random.Next(0, 40),
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
            for (int i = 0; i < count; i++)
            {
                if (particles.Count >= MAX_PARTICLES) break;

                float angle = (float)random.NextDouble() * 360f;
                float speed = 2f + (float)random.NextDouble() * 8f;
                float radians = angle * (float)Math.PI / 180f;

                Particle particle = new Particle
                {
                    X = random.Next(0, 100) < 50 ? -20 : -20 + random.Next(0, 40),
                    Y = random.Next(0, 100) < 50 ? -20 : -20 + random.Next(0, 40),
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

            using (var particlePaint = new SKPaint { IsAntialias = true })
            {
                // Actualizar y dibujar partículas
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    Particle p = particles[i];

                    if (IsPlaying)
                    {
                        // Actualizar posición
                        p.X += p.SpeedX;
                        p.Y += p.SpeedY;

                        // Aplicar fuerzas basadas en el audio
                        if (Magnitudes != null && Magnitudes.Length > 0)
                        {
                            int freqBand = i % Magnitudes.Length;
                            float audioForce = Magnitudes[freqBand] * 100f;

                            // Añadir una fuerza giratoria
                            float angleToCenter = (float)Math.Atan2(p.Y - centerY, p.X - centerX);
                            p.SpeedX += audioForce * (float)Math.Cos(angleToCenter + Math.PI / 2) * 0.1f;
                            p.SpeedY += audioForce * (float)Math.Sin(angleToCenter + Math.PI / 2) * 0.1f;

                            // Limitar la velocidad
                            float speed = (float)Math.Sqrt(p.SpeedX * p.SpeedX + p.SpeedY * p.SpeedY);
                            if (speed > 15f)
                            {
                                p.SpeedX = (p.SpeedX / speed) * 15f;
                                p.SpeedY = (p.SpeedY / speed) * 15f;
                            }
                        }

                        // Actualizar tiempo de vida
                        p.CurrentLife += 0.05f;
                    }

                    // Dibujar partícula
                    float x = centerX + p.X;
                    float y = centerY + p.Y;
                    float remainingLifeRatio = Math.Max(0, 1 - (p.CurrentLife / p.LifeTime));

                    // Color cambiante
                    float hue = (p.Hue + time * 20) % 360;
                    byte opacity = (byte)(p.Opacity * remainingLifeRatio);
                    particlePaint.Color = SKColor.FromHsv(hue, 100, 100, opacity);

                    // Tamaño que varía con el tiempo de vida
                    float size = p.Size * remainingLifeRatio * (1 + 0.5f * (float)Math.Sin(time * 5 + p.Hue * 0.1f));

                    // Dibujar con diferentes formas según la partícula
                    int shapeType = (int)p.Hue % 4;
                    switch (shapeType)
                    {
                        case 0: // Círculo
                            canvas.DrawCircle(x, y, size, particlePaint);
                            break;
                        case 1: // Cuadrado
                            canvas.DrawRect(x - size, y - size, size * 2, size * 2, particlePaint);
                            break;
                        case 2: // Estrella
                            DrawStar(canvas, x, y, size * 2, size, 5, particlePaint);
                            break;
                        case 3: // Línea
                            float angle = time * 2 + p.Hue * 0.1f;
                            float dx = size * 2 * (float)Math.Cos(angle);
                            float dy = size * 2 * (float)Math.Sin(angle);
                            canvas.DrawLine(x - dx, y - dy, x + dx, y + dy, particlePaint);
                            break;
                    }

                    // Eliminar partículas muertas
                    if (p.CurrentLife >= p.LifeTime)
                    {
                        particles.RemoveAt(i);
                    }
                    else
                    {
                        // Actualizar la partícula
                        particles[i] = p;
                    }
                }
            }
        }

        private void DrawStar(SKCanvas canvas, float cx, float cy, float outerRadius, float innerRadius, int numPoints, SKPaint paint)
        {
            using (var path = new SKPath())
            {
                float angleStep = (float)(2 * Math.PI / numPoints);

                for (int i = 0; i < numPoints; i++)
                {
                    float outerAngle = i * angleStep - (float)(Math.PI / 2);
                    float innerAngle = outerAngle + angleStep / 2;

                    float outerX = cx + outerRadius * (float)Math.Cos(outerAngle);
                    float outerY = cy + outerRadius * (float)Math.Sin(outerAngle);
                    float innerX = cx + innerRadius * (float)Math.Cos(innerAngle);
                    float innerY = cy + innerRadius * (float)Math.Sin(innerAngle);

                    if (i == 0)
                        path.MoveTo(outerX, outerY);
                    else
                        path.LineTo(outerX, outerY);

                    path.LineTo(innerX, innerY);
                }

                path.Close();
                canvas.DrawPath(path, paint);
            }
        }
    }

    /// <summary>
    /// Clase para el sistema de partículas
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
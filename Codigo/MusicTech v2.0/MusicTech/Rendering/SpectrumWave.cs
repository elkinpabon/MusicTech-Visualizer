using System;
using SkiaSharp;

namespace MusicTech.Rendering
{
    public class SpectrumWave : IVisualizer
    {
        private float[] _smoothedMagnitudes = Array.Empty<float>();
        private float[] _peakValues = Array.Empty<float>();
        private float[] _peakDecay = Array.Empty<float>();
        private float[] _velocities = Array.Empty<float>();
        private float[] _colorOffsets = Array.Empty<float>();
        private float[] _pulseTimers = Array.Empty<float>();
        private float[] _waveOffsets = Array.Empty<float>();
        private DateTime _lastFrameTime = DateTime.Now;
        private float _globalColorShift = 0f;
        private float _pulseIntensity = 0f;
        private float _waveTime = 0f;

        private const float SmoothingFactor = 0.12f;
        private const float PeakDecayRate = 0.88f;
        private const float SpringStrength = 8f;
        private const float Damping = 0.65f;
        private const float GlowIntensity = 4f;
        private const float HeightMultiplier = 0.8f;
        private const float AudioSensitivity = 3f;
        private const float ColorVibrancy = 1.5f;
        private const float PulseSpeed = 4f;
        private const float WaveSpeed = 2f;

        public bool IsPlaying { get; set; }
        public float[] Magnitudes { get; set; } = Array.Empty<float>();

        public void Render(SKCanvas canvas, SKImageInfo info)
        {
            canvas.Clear(SKColors.Black);

            int width = info.Width;
            int height = info.Height;
            int midY = height / 2;
            int bars = Magnitudes.Length;

            if (bars == 0) return;

            if (_smoothedMagnitudes.Length != bars)
            {
                _smoothedMagnitudes = new float[bars];
                _peakValues = new float[bars];
                _peakDecay = new float[bars];
                _velocities = new float[bars];
                _colorOffsets = new float[bars];
                _pulseTimers = new float[bars];
                _waveOffsets = new float[bars];

                Random rand = new Random();
                for (int i = 0; i < bars; i++)
                {
                    _colorOffsets[i] = (float)rand.NextDouble() * 360f;
                    _pulseTimers[i] = (float)rand.NextDouble() * (float)Math.PI * 2f;
                    _waveOffsets[i] = i * 0.2f;
                }
            }

            DateTime currentTime = DateTime.Now;
            float deltaTime = Math.Min((float)(currentTime - _lastFrameTime).TotalSeconds, 1f / 30f);
            _lastFrameTime = currentTime;

            if (IsPlaying)
            {
                UpdateAnimations(deltaTime);
            }

            DrawEnhancedBackground(canvas, width, height);

            int centerX = width / 2;
            int symmetricBars = Math.Min(bars, 64);
            float totalWidth = width * 0.85f;
            float barWidth = Math.Max(10f, totalWidth / (symmetricBars * 2.2f));
            float spacing = totalWidth / symmetricBars;

            DrawSymmetricGlowEffects(canvas, centerX, midY, symmetricBars, spacing, barWidth, height);
            DrawSymmetricReflection(canvas, centerX, midY, symmetricBars, spacing, barWidth, height);
            DrawSymmetricSpectrum(canvas, centerX, midY, symmetricBars, spacing, barWidth, height);
            DrawSymmetricPeaks(canvas, centerX, midY, symmetricBars, spacing, barWidth, height);
        }

        private void UpdateAnimations(float deltaTime)
        {
            _globalColorShift += deltaTime * 60f;
            if (_globalColorShift > 360f) _globalColorShift -= 360f;

            _waveTime += deltaTime * WaveSpeed;

            float avgMagnitude = 0f;
            for (int i = 0; i < Math.Min(Magnitudes.Length, 16); i++)
            {
                avgMagnitude += Magnitudes[i];
            }
            avgMagnitude /= Math.Min(Magnitudes.Length, 16);
            _pulseIntensity = avgMagnitude * 2f;

            for (int i = 0; i < Magnitudes.Length; i++)
            {
                float targetMagnitude = Math.Min(1f, Magnitudes[i] * AudioSensitivity);

                float currentMag = _smoothedMagnitudes[i];
                float force = (targetMagnitude - currentMag) * SpringStrength;
                _velocities[i] = (_velocities[i] + force) * Damping;
                _smoothedMagnitudes[i] = Math.Max(0, currentMag + _velocities[i] * deltaTime * 60f);

                if (_smoothedMagnitudes[i] > _peakValues[i])
                {
                    _peakValues[i] = _smoothedMagnitudes[i];
                    _peakDecay[i] = 1f;
                }
                else
                {
                    _peakDecay[i] *= PeakDecayRate;
                    _peakValues[i] = Math.Max(_smoothedMagnitudes[i], _peakValues[i] * _peakDecay[i]);
                }

                _colorOffsets[i] += deltaTime * (40f + _smoothedMagnitudes[i] * 80f);
                if (_colorOffsets[i] > 360f) _colorOffsets[i] -= 360f;

                _pulseTimers[i] += deltaTime * PulseSpeed * (1f + _smoothedMagnitudes[i] * 2f);
                if (_pulseTimers[i] > (float)Math.PI * 2f) _pulseTimers[i] -= (float)(Math.PI * 2f);
            }
        }

        private void DrawFloatingParticles(SKCanvas canvas, int width, int height)
        {
            using (var particlePaint = new SKPaint())
            {
                particlePaint.IsAntialias = true;
                particlePaint.Style = SKPaintStyle.Fill;

                // Crear partículas flotantes que reaccionen al audio
                int particleCount = 20;
                for (int i = 0; i < particleCount; i++)
                {
                    float particleTime = _waveTime + i * 0.5f;

                    // Posición de partícula con movimiento ondulante
                    float x = (float)(width * 0.1f + (width * 0.8f) * ((Math.Sin(particleTime * 0.3f + i) + 1) / 2));
                    float y = (float)(height * 0.2f + (height * 0.6f) * ((Math.Cos(particleTime * 0.2f + i * 1.5f) + 1) / 2));

                    // Tamaño variable basado en el pulso
                    float size = 2f + _pulseIntensity * 8f + (float)Math.Sin(particleTime * 2 + i) * 3f;

                    // Color de partícula cambiante
                    float hue = (_globalColorShift + i * 30f + particleTime * 20f) % 360f;
                    var particleColor = SKColor.FromHsv(hue, 90f, 60f + _pulseIntensity * 40f);

                    particlePaint.Color = new SKColor(
                        particleColor.Red,
                        particleColor.Green,
                        particleColor.Blue,
                        (byte)(120 + _pulseIntensity * 135)
                    );

                    canvas.DrawCircle(x, y, size, particlePaint);
                }
            }
        }

        private void DrawEnhancedBackground(SKCanvas canvas, int width, int height)
        {
            // Fondo con gradiente animado más dinámico y colorido
            using (var backgroundPaint = new SKPaint())
            {
                // Crear colores de fondo que cambien dinámicamente con pulso
                float hueShift = _globalColorShift * 0.3f + _pulseIntensity * 20f;
                float pulseEffect = (float)Math.Sin(_waveTime * 2) * 0.3f + 0.7f;

                var color1 = SKColor.FromHsv((240f + hueShift) % 360f, 95f, (15f * pulseEffect));  // Azul vibrante
                var color2 = SKColor.FromHsv((280f + hueShift) % 360f, 85f, (8f * pulseEffect));   // Púrpura profundo
                var color3 = SKColor.FromHsv((320f + hueShift) % 360f, 100f, (18f * pulseEffect)); // Magenta vivo
                var color4 = SKColor.FromHsv((200f + hueShift) % 360f, 90f, (12f * pulseEffect));  // Cyan

                using (var shader = SKShader.CreateRadialGradient(
                    new SKPoint(width / 2, height / 2),
                    Math.Max(width, height) * 0.8f,
                    new SKColor[] { color2, color1, color3, color4 },
                    new float[] { 0f, 0.3f, 0.7f, 1f },
                    SKShaderTileMode.Clamp))
                {
                    backgroundPaint.Shader = shader;
                    canvas.DrawRect(0, 0, width, height, backgroundPaint);
                }
            }

            // Añadir partículas de fondo flotantes
            DrawFloatingParticles(canvas, width, height);
        }

        private SKColor GetDynamicColor(int barIndex, int totalBars, float magnitude)
        {
            // Mapeo de colores súper vibrante y dinámico
            float position = (float)barIndex / totalBars;

            // Combinar múltiples efectos de color
            float baseHue = _globalColorShift + _colorOffsets[barIndex] + (position * 120f);

            // Efecto de onda de color que se propaga
            float waveEffect = (float)Math.Sin(_waveTime + _waveOffsets[barIndex]) * 60f;

            // Pulso de color basado en la magnitud
            float pulseEffect = (float)Math.Sin(_pulseTimers[barIndex]) * magnitude * 40f;

            // Efecto de arcoíris dinámico
            float rainbowShift = (float)Math.Sin(_waveTime * 0.5f + position * (float)Math.PI) * 80f;

            float finalHue = (baseHue + waveEffect + pulseEffect + rainbowShift) % 360f;
            if (finalHue < 0) finalHue += 360f;

            // Saturación y brillo súper vibrantes
            float saturation = Math.Min(100f, 95f + magnitude * 5f); // Saturación muy alta
            float brightness = Math.Min(100f, 70f + magnitude * 30f); // Brillo potente

            // Efecto de pulso en el brillo
            float pulseBrightness = (float)Math.Sin(_pulseTimers[barIndex] * 2) * magnitude * 15f;
            brightness = Math.Min(100f, brightness + pulseBrightness);

            // Aplicar multiplicador de vivacidad
            saturation *= ColorVibrancy;
            brightness = Math.Min(100f, brightness * (0.8f + ColorVibrancy * 0.2f));

            // Colores especiales para magnitudes altas (efectos de neón)
            if (magnitude > 0.8f)
            {
                saturation = 100f;
                brightness = Math.Min(100f, brightness + 20f);

                // Efecto neón con colores específicos
                float[] neonHues = { 0f, 60f, 120f, 180f, 240f, 300f }; // Rojo, amarillo, verde, cyan, azul, magenta
                int neonIndex = (int)((_waveTime + barIndex * 0.1f) * 2) % neonHues.Length;
                finalHue = (neonHues[neonIndex] + waveEffect * 0.5f) % 360f;
            }

            return SKColor.FromHsv(finalHue, saturation, brightness);
        }

        private void DrawSymmetricSpectrum(SKCanvas canvas, int centerX, int midY, int bars, float spacing, float barWidth, int height)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.Style = SKPaintStyle.Fill;

                for (int i = 0; i < bars; i++)
                {
                    if (i >= _smoothedMagnitudes.Length) break;

                    float magnitude = _smoothedMagnitudes[i];

                    // Efecto de onda en la altura
                    float waveHeightEffect = (float)Math.Sin(_waveTime + i * 0.3f) * 0.1f + 1f;
                    float barHeight = magnitude * height * HeightMultiplier * waveHeightEffect;

                    if (barHeight < 2f) continue;

                    // Calcular posiciones para barras simétricas con ligero movimiento
                    float waveXOffset = (float)Math.Sin(_waveTime + i * 0.15f) * 2f;
                    float offset = (i + 0.5f) * spacing;
                    float leftX = centerX - offset - barWidth / 2 + waveXOffset;
                    float rightX = centerX + offset - barWidth / 2 - waveXOffset;

                    // Obtener color dinámico súper vibrante
                    var barColor = GetDynamicColor(i, bars, magnitude);

                    // Crear gradiente vertical más dramático
                    var topColor = new SKColor(
                        (byte)Math.Min(255, barColor.Red + 60),
                        (byte)Math.Min(255, barColor.Green + 60),
                        (byte)Math.Min(255, barColor.Blue + 60),
                        barColor.Alpha
                    );

                    var midColor = new SKColor(
                        barColor.Red,
                        barColor.Green,
                        barColor.Blue,
                        (byte)(barColor.Alpha * 0.8f)
                    );

                    using (var gradientShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, midY - barHeight),
                        new SKPoint(0, midY),
                        new SKColor[] { topColor, barColor, midColor },
                        new float[] { 0f, 0.6f, 1f },
                        SKShaderTileMode.Clamp))
                    {
                        paint.Shader = gradientShader;

                        float cornerRadius = Math.Min(barWidth * 0.25f, 8f);

                        // Dibujar barra izquierda con efecto de escala de pulso
                        float pulseScale = 1f + (float)Math.Sin(_pulseTimers[i]) * magnitude * 0.15f;
                        float pulsedWidth = barWidth * pulseScale;
                        float pulsedHeight = barHeight * pulseScale;

                        var leftRect = new SKRect(
                            leftX - (pulsedWidth - barWidth) / 2,
                            midY - pulsedHeight,
                            leftX + barWidth + (pulsedWidth - barWidth) / 2,
                            midY
                        );
                        canvas.DrawRoundRect(leftRect, cornerRadius, cornerRadius, paint);

                        // Dibujar barra derecha con efecto de escala de pulso
                        var rightRect = new SKRect(
                            rightX - (pulsedWidth - barWidth) / 2,
                            midY - pulsedHeight,
                            rightX + barWidth + (pulsedWidth - barWidth) / 2,
                            midY
                        );
                        canvas.DrawRoundRect(rightRect, cornerRadius, cornerRadius, paint);
                    }
                }
            }
        }

        private void DrawSymmetricReflection(SKCanvas canvas, int centerX, int midY, int bars, float spacing, float barWidth, int height)
        {
            using (var reflectionPaint = new SKPaint())
            {
                reflectionPaint.IsAntialias = true;
                reflectionPaint.Style = SKPaintStyle.Fill;

                for (int i = 0; i < bars; i++)
                {
                    if (i >= _smoothedMagnitudes.Length) break;

                    float magnitude = _smoothedMagnitudes[i];
                    float barHeight = magnitude * height * HeightMultiplier;
                    float reflectionHeight = barHeight * 0.7f; // Reflexión más prominente

                    if (reflectionHeight < 2f) continue;

                    float offset = (i + 0.5f) * spacing;
                    float leftX = centerX - offset - barWidth / 2;
                    float rightX = centerX + offset - barWidth / 2;

                    var baseColor = GetDynamicColor(i, bars, magnitude * 0.8f);

                    // Crear efecto de desvanecimiento mejorado para reflexión
                    using (var reflectionShader = SKShader.CreateLinearGradient(
                        new SKPoint(0, midY),
                        new SKPoint(0, midY + reflectionHeight),
                        new SKColor[] {
                            new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, (byte)(baseColor.Alpha * 0.5f)),
                            SKColors.Transparent
                        },
                        null,
                        SKShaderTileMode.Clamp))
                    {
                        reflectionPaint.Shader = reflectionShader;

                        float cornerRadius = Math.Min(barWidth * 0.25f, 8f);

                        // Dibujar reflexión izquierda
                        var leftReflectionRect = new SKRect(leftX, midY, leftX + barWidth, midY + reflectionHeight);
                        canvas.DrawRoundRect(leftReflectionRect, cornerRadius, cornerRadius, reflectionPaint);

                        // Dibujar reflexión derecha
                        var rightReflectionRect = new SKRect(rightX, midY, rightX + barWidth, midY + reflectionHeight);
                        canvas.DrawRoundRect(rightReflectionRect, cornerRadius, cornerRadius, reflectionPaint);
                    }
                }
            }
        }

        private void DrawSymmetricPeaks(SKCanvas canvas, int centerX, int midY, int bars, float spacing, float barWidth, int height)
        {
            using (var peakPaint = new SKPaint())
            {
                peakPaint.IsAntialias = true;
                peakPaint.Style = SKPaintStyle.Fill;

                for (int i = 0; i < bars; i++)
                {
                    if (i >= _peakValues.Length) break;

                    float peakHeight = _peakValues[i] * height * HeightMultiplier;
                    if (peakHeight < 5f) continue;

                    float offset = (i + 0.5f) * spacing;
                    float leftX = centerX - offset - barWidth / 2;
                    float rightX = centerX + offset - barWidth / 2;
                    float alpha = _peakDecay[i];

                    var peakColor = GetDynamicColor(i, bars, 1f);

                    // Hacer los picos más brillantes y visibles
                    var brightPeakColor = new SKColor(
                        (byte)Math.Min(255, peakColor.Red + 50),
                        (byte)Math.Min(255, peakColor.Green + 50),
                        (byte)Math.Min(255, peakColor.Blue + 50),
                        (byte)(255 * alpha)
                    );

                    peakPaint.Color = brightPeakColor;

                    float peakThickness = 3f; // Picos más gruesos
                    float cornerRadius = barWidth * 0.3f;

                    // Dibujar indicadores de picos para barras izquierdas
                    var leftPeakRect = new SKRect(leftX, midY - peakHeight - peakThickness, leftX + barWidth, midY - peakHeight);
                    canvas.DrawRoundRect(leftPeakRect, cornerRadius, cornerRadius, peakPaint);

                    // Dibujar indicadores de picos para barras derechas
                    var rightPeakRect = new SKRect(rightX, midY - peakHeight - peakThickness, rightX + barWidth, midY - peakHeight);
                    canvas.DrawRoundRect(rightPeakRect, cornerRadius, cornerRadius, peakPaint);
                }
            }
        }

        private void DrawSymmetricGlowEffects(SKCanvas canvas, int centerX, int midY, int bars, float spacing, float barWidth, int height)
        {
            // Capa de brillo externa más intensa
            using (var glowPaint = new SKPaint())
            {
                glowPaint.IsAntialias = true;
                glowPaint.Style = SKPaintStyle.Fill;
                glowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20f); // Más desenfoque

                for (int i = 0; i < bars; i++)
                {
                    if (i >= _smoothedMagnitudes.Length) break;

                    float magnitude = _smoothedMagnitudes[i];
                    float barHeight = magnitude * height * HeightMultiplier;

                    // Brillo para barras más pequeñas también
                    if (barHeight < height * 0.05f) continue;

                    float offset = (i + 0.5f) * spacing;
                    float leftX = centerX - offset - barWidth / 2;
                    float rightX = centerX + offset - barWidth / 2;

                    var glowColor = GetDynamicColor(i, bars, magnitude);

                    // Intensidad de brillo súper aumentada con efectos de neón
                    float glowAlpha = Math.Min(1f, magnitude * 3f) * GlowIntensity;

                    // Crear efecto de neón pulsante
                    float neonPulse = (float)Math.Sin(_pulseTimers[i] * 3) * 0.3f + 0.7f;
                    glowAlpha *= neonPulse;

                    glowPaint.Color = new SKColor(
                        glowColor.Red,
                        glowColor.Green,
                        glowColor.Blue,
                        (byte)(220 * glowAlpha)
                    );

                    // Expansión de brillo mayor
                    float glowExpansion = 12f + magnitude * 8f; // Brillo variable
                    float cornerRadius = Math.Min(barWidth * 0.4f, 10f);

                    // Dibujar brillo izquierdo
                    var leftGlowRect = new SKRect(
                        leftX - glowExpansion,
                        midY - barHeight - glowExpansion,
                        leftX + barWidth + glowExpansion,
                        midY + glowExpansion
                    );
                    canvas.DrawRoundRect(leftGlowRect, cornerRadius, cornerRadius, glowPaint);

                    // Dibujar brillo derecho
                    var rightGlowRect = new SKRect(
                        rightX - glowExpansion,
                        midY - barHeight - glowExpansion,
                        rightX + barWidth + glowExpansion,
                        midY + glowExpansion
                    );
                    canvas.DrawRoundRect(rightGlowRect, cornerRadius, cornerRadius, glowPaint);
                }
            }

            // Capa de brillo interna para profundidad adicional
            using (var innerGlowPaint = new SKPaint())
            {
                innerGlowPaint.IsAntialias = true;
                innerGlowPaint.Style = SKPaintStyle.Fill;
                innerGlowPaint.MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10f);

                for (int i = 0; i < bars; i++)
                {
                    if (i >= _smoothedMagnitudes.Length) break;

                    float magnitude = _smoothedMagnitudes[i];
                    float barHeight = magnitude * height * HeightMultiplier;

                    if (barHeight < height * 0.08f) continue;

                    float offset = (i + 0.5f) * spacing;
                    float leftX = centerX - offset - barWidth / 2;
                    float rightX = centerX + offset - barWidth / 2;

                    var glowColor = GetDynamicColor(i, bars, magnitude);

                    innerGlowPaint.Color = new SKColor(
                        glowColor.Red,
                        glowColor.Green,
                        glowColor.Blue,
                        (byte)(200 * magnitude)
                    );

                    float cornerRadius = Math.Min(barWidth * 0.3f, 8f);

                    // Dibujar brillo interno izquierdo
                    var leftInnerGlowRect = new SKRect(
                        leftX - 5,
                        midY - barHeight - 5,
                        leftX + barWidth + 5,
                        midY + 5
                    );
                    canvas.DrawRoundRect(leftInnerGlowRect, cornerRadius, cornerRadius, innerGlowPaint);

                    // Dibujar brillo interno derecho
                    var rightInnerGlowRect = new SKRect(
                        rightX - 5,
                        midY - barHeight - 5,
                        rightX + barWidth + 5,
                        midY + 5
                    );
                    canvas.DrawRoundRect(rightInnerGlowRect, cornerRadius, cornerRadius, innerGlowPaint);
                }
            }
        }
    }
}
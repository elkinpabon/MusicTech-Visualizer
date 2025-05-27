using SkiaSharp;
using System;
using System.Collections.Generic;

namespace MusicTech.Rendering
{
    public class FractalEffect : IVisualizer
    {
        public float[] Magnitudes { get; set; }
        public bool IsPlaying { get; set; }

        private float time = 0f;
        private float smoothBass = 0f;
        private float smoothMid = 0f;
        private float smoothHigh = 0f;
        private float globalRotation = 0f;
        private float spiralPhase = 0f;
        private float foldPhase = 0f;
        private float colorShift = 0f;
        private float pulsePhase = 0f;
        private float waveDistortion = 0f;
        private float energyLevel = 0f;
        private float[] frequencyHistory = new float[60];
        private int historyIndex = 0;
        private float backgroundHue = 0f;

        // Variables 3D
        private float rotationX = 0f;
        private float rotationY = 0f;
        private float rotationZ = 0f;
        private float depth3D = 0f;
        private float perspective = 800f;
        private float[] zBuffer = new float[1000]; // Buffer para efectos de profundidad

        // Variables de efectos mejorados
        private float tunnelEffect = 0f;
        private float hologramPhase = 0f;
        private float particleSystemTime = 0f;
        private float morphingPhase = 0f;
        private float lightingAngle = 0f;
        private BackgroundMode currentBgMode = BackgroundMode.NeonGrid;
        private float bgModeTimer = 0f;
        private const float BG_MODE_SWITCH_TIME = 15f; // Cambiar fondo cada 15 segundos

        // Variables de optimización mejoradas
        private int frameCount = 0;
        private float lastUpdateTime = 0f;
        private const float UPDATE_INTERVAL = 0.0167f; // 60 FPS
        private const float SMOOTHING_FACTOR_PLAYING = 0.12f;
        private const float SMOOTHING_FACTOR_PAUSED = 0.03f;
        private const float TIME_INCREMENT = 0.0167f; // 60 FPS

        // Cache expandido para pinturas reutilizables
        private SKPaint[] cachedPaints = new SKPaint[16];
        private SKPaint[] cachedEffectPaints = new SKPaint[8];
        private bool paintsInitialized = false;

        // Enumeración para modos de fondo
        private enum BackgroundMode
        {
            NeonGrid = 0,
            PlasmaTunnel = 1,
            StarField = 2,
            QuantumWaves = 3,
            HolographicRings = 4
        }

        public void Render(SKCanvas canvas, SKImageInfo info)
        {
            frameCount++;

            // Solo procesar audio si la música está reproduciéndose
            if (IsPlaying && (Magnitudes != null && Magnitudes.Length > 0))
            {
                time += TIME_INCREMENT;
                AnalyzeAudioOptimized();
            }
            else if (!IsPlaying)
            {
                time += TIME_INCREMENT * 0.15f; // Animación más lenta cuando está pausado
                DecayAudioValues();
            }

            // Inicializar pinturas en cache si es necesario
            if (!paintsInitialized)
            {
                InitializeCachedPaints();
                paintsInitialized = true;
            }

            // Actualizar modo de fondo
            UpdateBackgroundMode();

            // Fondo dinámico con múltiples modos
            DrawDynamicBackground(canvas, info);

            float centerX = info.Width / 2f;
            float centerY = info.Height / 2f;
            float baseSize = Math.Min(info.Width, info.Height) * 0.4f;

            // Efectos de cámara 3D reactivos
            canvas.Save();

            if (IsPlaying)
            {
                // Zoom pulsante 3D con perspectiva
                float zoomPulse = 1f + (float)Math.Sin(pulsePhase * 2.5f) * 0.12f + smoothBass * 0.2f;

                // Rotaciones 3D complejas
                rotationX += (smoothMid * 0.015f + smoothHigh * 0.008f) * TIME_INCREMENT * 60f;
                rotationY += (smoothBass * 0.02f + smoothMid * 0.012f) * TIME_INCREMENT * 60f;
                rotationZ += (smoothHigh * 0.025f + smoothBass * 0.01f) * TIME_INCREMENT * 60f;

                // Aplicar transformaciones 3D
                Apply3DTransform(canvas, centerX, centerY, zoomPulse);
            }
            else
            {
                // Movimiento mínimo cuando está pausado
                canvas.Translate(centerX, centerY);
                canvas.Scale(1f + (float)Math.Sin(time * 0.8f) * 0.02f);
                canvas.RotateRadians(time * 0.08f);
            }

            // Fractal principal con efectos 3D mejorados
            DrawEnhanced3DFractalLayers(canvas, baseSize);

            canvas.Restore();

            // Efectos adicionales avanzados
            if (IsPlaying && energyLevel > 0.08f)
            {
                DrawAdvancedEffects(canvas, info, centerX, centerY, baseSize);
            }

            // Post-efectos (siempre visibles pero más sutiles cuando pausado)
            DrawPostEffects(canvas, info);
        }

        private void UpdateBackgroundMode()
        {
            bgModeTimer += TIME_INCREMENT;
            if (bgModeTimer >= BG_MODE_SWITCH_TIME)
            {
                bgModeTimer = 0f;
                int nextMode = ((int)currentBgMode + 1) % Enum.GetValues(typeof(BackgroundMode)).Length;
                currentBgMode = (BackgroundMode)nextMode;
            }
        }

        private void Apply3DTransform(SKCanvas canvas, float centerX, float centerY, float zoom)
        {
            canvas.Translate(centerX, centerY);
            canvas.Scale(zoom);

            var matrix = SKMatrix.CreateIdentity();

            float cosX = (float)Math.Cos(rotationX);
            float sinX = (float)Math.Sin(rotationX);

            float cosY = (float)Math.Cos(rotationY);
            float sinY = (float)Math.Sin(rotationY);

            matrix = matrix.PostConcat(SKMatrix.CreateRotation(rotationZ));

            float perspectiveScale = 1f + (float)Math.Sin(rotationX) * 0.3f;
            matrix = matrix.PostConcat(SKMatrix.CreateScale(perspectiveScale, 1f));

            canvas.Concat(matrix);

        }


        private void AnalyzeAudioOptimized()
        {
            if (Magnitudes == null || Magnitudes.Length == 0) return;

            // Análisis de frecuencias más detallado
            int third = Magnitudes.Length / 3;
            float bass = 0f, mid = 0f, high = 0f;

            // Análisis ponderado para mejor reactividad
            for (int i = 0; i < third; i += 2)
            {
                float weight = 1f - (float)i / third * 0.5f; // Más peso a frecuencias más bajas
                bass += Magnitudes[i] * weight;
            }
            bass /= (third / 2f);

            for (int i = third; i < third * 2; i += 2)
                mid += Magnitudes[i];
            mid /= (third / 2f);

            for (int i = third * 2; i < Magnitudes.Length; i += 3)
            {
                float weight = 1f + (float)(i - third * 2) / (Magnitudes.Length - third * 2) * 0.3f;
                high += Magnitudes[i] * weight;
            }
            high /= ((Magnitudes.Length - third * 2) / 3f);

            // Historial para efectos temporales
            if (frameCount % 2 == 0)
            {
                frequencyHistory[historyIndex] = (bass + mid + high) / 3f;
                historyIndex = (historyIndex + 1) % frequencyHistory.Length;
            }

            // Suavizado mejorado
            float smoothingFactor = IsPlaying ? SMOOTHING_FACTOR_PLAYING : SMOOTHING_FACTOR_PAUSED * 0.25f;

            smoothBass = Lerp(smoothBass, bass, smoothingFactor * 0.85f);
            smoothMid = Lerp(smoothMid, mid, smoothingFactor);
            smoothHigh = Lerp(smoothHigh, high, smoothingFactor * 1.15f);

            // Parámetros de animación más complejos
            float animationMultiplier = IsPlaying ? 1f : 0.08f;

            globalRotation += (smoothMid * 0.03f + smoothHigh * 0.012f + 0.002f) * animationMultiplier;
            spiralPhase += (smoothBass * 0.08f + smoothMid * 0.025f + 0.01f) * animationMultiplier;
            foldPhase += (smoothHigh * 0.1f + smoothBass * 0.02f + 0.008f) * animationMultiplier;
            pulsePhase += (smoothBass * 0.15f + 0.02f) * animationMultiplier;
            colorShift += (energyLevel * 0.6f + 0.25f) * animationMultiplier;
            //backgroundHue += (smoothBass * 2f + smoothMid * 1.2f + 0.4f) * animationMultiplier;

            // Variables 3D y efectos especiales
            tunnelEffect += (smoothBass * 0.05f + 0.008f) * animationMultiplier;
            hologramPhase += (smoothMid * 0.12f + 0.015f) * animationMultiplier;
            particleSystemTime += (energyLevel * 0.8f + 0.3f) * animationMultiplier;
            morphingPhase += (smoothHigh * 0.06f + 0.012f) * animationMultiplier;
            lightingAngle += (smoothBass * 0.04f + smoothHigh * 0.02f + 0.01f) * animationMultiplier;

            // Nivel de energía mejorado
            energyLevel = (smoothBass * 1.2f + smoothMid + smoothHigh * 0.8f) / 3f;
        }

        private void DecayAudioValues()
        {
            float decayRate = 0.975f;
            smoothBass *= decayRate;
            smoothMid *= decayRate;
            smoothHigh *= decayRate;
            energyLevel *= decayRate;
        }

        private void InitializeCachedPaints()
        {
            // Pinturas principales
            for (int i = 0; i < cachedPaints.Length; i++)
            {
                cachedPaints[i] = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round
                };
            }

            // Pinturas para efectos especiales
            for (int i = 0; i < cachedEffectPaints.Length; i++)
            {
                cachedEffectPaints[i] = new SKPaint
                {
                    IsAntialias = true,
                    FilterQuality = SKFilterQuality.Low // Para mejor rendimiento
                };
            }

            paintsInitialized = true;
        }

        private void DrawDynamicBackground(SKCanvas canvas, SKImageInfo info)
        {
            switch (currentBgMode)
            {
                case BackgroundMode.NeonGrid:
                    DrawNeonGridBackground(canvas, info);
                    break;
                case BackgroundMode.PlasmaTunnel:
                    DrawPlasmaTunnelBackground(canvas, info);
                    break;
                case BackgroundMode.StarField:
                    DrawStarFieldBackground(canvas, info);
                    break;
                case BackgroundMode.QuantumWaves:
                    DrawQuantumWavesBackground(canvas, info);
                    break;
                case BackgroundMode.HolographicRings:
                    DrawHolographicRingsBackground(canvas, info);
                    break;
            }
        }

        private void DrawNeonGridBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Fondo base
            var baseColor = IsPlaying ?
                SKColor.FromHsv((backgroundHue + 180f) % 360f, 80f, 12f + energyLevel * 8f) :
                SKColor.FromHsv(backgroundHue % 360f, 40f, 6f);

            canvas.Clear(baseColor);

            // Grid neon reactivo
            using (var gridPaint = new SKPaint())
            {
                gridPaint.Style = SKPaintStyle.Stroke;
                gridPaint.StrokeWidth = 1f + (IsPlaying ? smoothBass * 1.5f : 0f);
                gridPaint.IsAntialias = true;
                gridPaint.BlendMode = SKBlendMode.Plus;

                float gridSize = 40f + (IsPlaying ? smoothMid * 20f : 0f);
                float alpha = IsPlaying ? 80 + (int)(energyLevel * 60) : 30;

                var gridColor = SKColor.FromHsv(backgroundHue % 360f, 70f, 60f);
                gridPaint.Color = gridColor.WithAlpha((byte)alpha);

                // Líneas horizontales
                for (float y = 0; y < info.Height; y += gridSize)
                {
                    float offset = (float)Math.Sin(y * 0.01f + time * 2f) * (IsPlaying ? smoothBass * 10f : 2f);
                    canvas.DrawLine(0, y + offset, info.Width, y + offset, gridPaint);
                }

                // Líneas verticales
                for (float x = 0; x < info.Width; x += gridSize)
                {
                    float offset = (float)Math.Cos(x * 0.01f + time * 1.5f) * (IsPlaying ? smoothMid * 8f : 1.5f);
                    canvas.DrawLine(x + offset, 0, x + offset, info.Height, gridPaint);
                }
            }
        }

        // Variables de clase para reutilizar objetos y evitar allocaciones
        private SKPaint _reusablePaint = new SKPaint();
        private SKPath _reusablePath = new SKPath();
        private SKColor[] _colorBuffer = new SKColor[5];
        private float[] _positionsBuffer = new float[] { 0f, 0.2f, 0.5f, 0.8f, 1f };
        private readonly Dictionary<int, SKShader> _shaderCache = new Dictionary<int, SKShader>();
        private int _frameCounter = 0;

        // Constantes para evitar recálculos
        private const float TWO_PI = 6.28318531f;
        private const float GOLDEN_RATIO = 0.618f;
        private const float INV_GOLDEN_RATIO = 0.382f;

        private void DrawPlasmaTunnelBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Precalcular valores que se usan múltiples veces
            float backgroundHueNormalized = backgroundHue % 360f;
            bool isPlaying = IsPlaying;

            // Configurar colores una sola vez
            if (isPlaying)
            {
                _colorBuffer[0] = SKColor.FromHsv(backgroundHueNormalized, 90f, 5f);
                _colorBuffer[1] = SKColor.FromHsv((backgroundHueNormalized + 60f) % 360f, 80f + smoothBass * 20f, 15f + energyLevel * 20f);
                _colorBuffer[2] = SKColor.FromHsv((backgroundHueNormalized + 120f) % 360f, 70f + smoothMid * 30f, 25f + energyLevel * 25f);
                _colorBuffer[3] = SKColor.FromHsv((backgroundHueNormalized + 180f) % 360f, 60f + smoothHigh * 25f, 20f + energyLevel * 20f);
                _colorBuffer[4] = SKColors.Black;
            }
            else
            {
                _colorBuffer[0] = SKColor.FromHsv(backgroundHueNormalized, 50f, 3f);
                _colorBuffer[1] = SKColor.FromHsv((backgroundHueNormalized + 90f) % 360f, 40f, 8f);
                _colorBuffer[2] = SKColor.FromHsv((backgroundHueNormalized + 180f) % 360f, 30f, 12f);
                _colorBuffer[3] = SKColor.FromHsv((backgroundHueNormalized + 270f) % 360f, 20f, 6f);
                _colorBuffer[4] = SKColors.Black;
            }

            // Precalcular dimensiones
            float minDimension = Math.Min(info.Width, info.Height);
            float tunnelEffectMultiplier = isPlaying ? tunnelEffect * 0.2f : 0f;

            // Configurar paint una sola vez y reutilizar
            _reusablePaint.Reset();
            ConfigurePaintForPerformance(_reusablePaint);

            // Túneles optimizados
            for (int i = 0; i < 3; i++)
            {
                // Precalcular valores trigonométricos
                float timeOffset = time + i;
                float centerX = info.Width * (0.3f + i * 0.2f + (float)Math.Sin(timeOffset) * 0.1f);
                float centerY = info.Height * (0.3f + i * 0.2f + (float)Math.Cos(time * 0.7f + i) * 0.1f);
                float radius = minDimension * (0.6f + i * 0.3f + tunnelEffectMultiplier);

                // Crear shader directamente (sin cache complejo para evitar problemas)
                using (var shader = SKShader.CreateRadialGradient(
                    new SKPoint(centerX, centerY), radius, _colorBuffer, _positionsBuffer, SKShaderTileMode.Clamp))

                {
                    _reusablePaint.Shader = shader;
                    _reusablePaint.BlendMode = i == 0 ? SKBlendMode.Src : SKBlendMode.Plus;

                    canvas.DrawRect(0, 0, info.Width, info.Height, _reusablePaint);
                }
            }

            _frameCounter++;
        }

        private void DrawStarFieldBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Fondo espacial optimizado
            var spaceColor = SKColor.FromHsv((backgroundHue + 200f) % 360f, 30f, 8f + (IsPlaying ? energyLevel * 6f : 0f));
            canvas.Clear(spaceColor);

            // Configurar paint una sola vez
            _reusablePaint.Reset();
            ConfigurePaintForPerformance(_reusablePaint);
            _reusablePaint.Style = SKPaintStyle.Fill;
            _reusablePaint.BlendMode = SKBlendMode.Plus;

            // Reducir cantidad de estrellas y optimizar cálculos
            int starCount = IsPlaying ? Math.Min(100 + (int)(energyLevel * 50), 200) : 60; // Limitar máximo
            float timeMultiplier = time; // Precalcular

            for (int i = 0; i < starCount; i++)
            {
                // Optimizar cálculos de posición usando operaciones más rápidas
                float x = (info.Width * ((i * GOLDEN_RATIO) % 1f) + timeMultiplier * (10f + (i % 20))) % info.Width;
                float y = (info.Height * ((i * INV_GOLDEN_RATIO) % 1f) + timeMultiplier * (5f + (i % 15))) % info.Height;

                // Calcular brillo con menos operaciones trigonométricas
                float brightness = 0.3f + (float)Math.Sin(timeMultiplier * 2f + i) * 0.3f;
                if (IsPlaying) brightness += smoothHigh * 0.4f;

                // Skip estrellas muy tenues para mejorar rendimiento
                if (brightness < 0.1f) continue;

                float size = 0.5f + brightness * 2f + (IsPlaying ? smoothBass * 1.5f : 0f);

                var starColor = SKColor.FromHsv((backgroundHue + i * 10f) % 360f, 60f, brightness * 100f);
                _reusablePaint.Color = starColor.WithAlpha((byte)(brightness * 255));

                canvas.DrawCircle(x, y, size, _reusablePaint);
            }
        }

        private void DrawQuantumWavesBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Fondo cuántico optimizado
            var quantumColor = SKColor.FromHsv(backgroundHue % 360f, 20f, 4f + (IsPlaying ? energyLevel * 8f : 0f));
            canvas.Clear(quantumColor);

            _reusablePaint.Reset();
            ConfigurePaintForPerformance(_reusablePaint);
            _reusablePaint.Style = SKPaintStyle.Stroke;
            _reusablePaint.BlendMode = SKBlendMode.Plus;

            int waveCount = IsPlaying ? 6 : 3; // Reducir ondas para mejor rendimiento
            float heightMultiplier = info.Height;
            float widthStep = 8f; // Incrementar step para menos puntos

            for (int wave = 0; wave < waveCount; wave++)
            {
                float wavePhase = time * (0.5f + wave * 0.1f) + wave * 1.2f;
                float amplitude = (10f + wave * 5f) * (IsPlaying ? (1f + smoothBass * 0.8f) : 0.5f);
                float frequency = 0.01f + wave * 0.003f;
                float frequency2 = frequency * 1.7f;
                float wavePhase2 = wavePhase * 1.3f;

                var waveColor = SKColor.FromHsv((backgroundHue + wave * 45f) % 360f, 70f, 40f + (IsPlaying ? energyLevel * 30f : 0f));
                _reusablePaint.Color = waveColor.WithAlpha(100);
                _reusablePaint.StrokeWidth = 1f + (IsPlaying ? smoothMid * 2f : 0f);

                // Reutilizar path
                _reusablePath.Reset();

                bool first = true;
                float baseY = heightMultiplier * (0.3f + wave * 0.1f);

                for (float x = 0; x <= info.Width; x += widthStep)
                {
                    float y = baseY +
                             (float)Math.Sin(x * frequency + wavePhase) * amplitude +
                             (float)Math.Sin(x * frequency2 + wavePhase2) * amplitude * 0.3f;

                    if (first)
                    {
                        _reusablePath.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        _reusablePath.LineTo(x, y);
                    }
                }
                canvas.DrawPath(_reusablePath, _reusablePaint);
            }
        }

        private void DrawHolographicRingsBackground(SKCanvas canvas, SKImageInfo info)
        {
            // Fondo holográfico optimizado
            var holoColor = SKColor.FromHsv((backgroundHue + 120f) % 360f, 40f, 6f + (IsPlaying ? energyLevel * 10f : 0f));
            canvas.Clear(holoColor);

            _reusablePaint.Reset();
            ConfigurePaintForPerformance(_reusablePaint);
            _reusablePaint.Style = SKPaintStyle.Stroke;
            _reusablePaint.BlendMode = SKBlendMode.Screen;

            float centerX = info.Width * 0.5f;
            float centerY = info.Height * 0.5f;
            int ringCount = IsPlaying ? 8 : 4; // Reducir anillos

            float hologramPhaseMultiplier = hologramPhase * 50f;
            float midMultiplier = IsPlaying ? (1f + smoothMid * 0.3f) : 1f;

            for (int i = 0; i < ringCount; i++)
            {
                float radius = (i + 1) * 25f + (float)Math.Sin(hologramPhase + i * 0.5f) * 15f;
                radius *= midMultiplier;

                float alpha = 150f - i * 10f + (IsPlaying ? smoothHigh * 80f : 0f);
                alpha = Math.Max(alpha, 20f);

                // Skip anillos casi invisibles
                if (alpha < 25f) continue;

                var ringColor = SKColor.FromHsv((backgroundHue + i * 30f + hologramPhaseMultiplier) % 360f, 80f, 70f);
                _reusablePaint.Color = ringColor.WithAlpha((byte)alpha);
                _reusablePaint.StrokeWidth = 1f + (IsPlaying ? smoothBass * 2f : 0f);

                canvas.DrawCircle(centerX, centerY, radius, _reusablePaint);
            }
        }

        // Configuración de SKPaint para mejor rendimiento
        private void ConfigurePaintForPerformance(SKPaint paint)
        {
            // Usar SKSamplingOptions en lugar de las propiedades obsoletas
            paint.IsAntialias = true;
        }
        public void CleanupBackgroundResources()
        {
            _reusablePaint?.Dispose();
            _reusablePath?.Dispose();

            foreach (var shader in _shaderCache.Values)
            {
                shader?.Dispose();
            }
            _shaderCache.Clear();
        }

        private void DrawEnhanced3DFractalLayers(SKCanvas canvas, float baseSize)
        {
            int totalLayers = IsPlaying ? 10 + (int)(energyLevel * 6) : 6;
            totalLayers = Math.Min(totalLayers, 16); // Máximo optimizado

            for (int layer = 0; layer < totalLayers; layer++)
            {
                float layerDepth = (float)layer / (totalLayers - 1);
                float layerScale = 1f - (layerDepth * 0.3f);

                // Efecto de profundidad 3D mejorado
                float zDepth = layerDepth - 0.5f;
                float perspectiveScale = perspective / (perspective + zDepth * 200f);
                layerScale *= perspectiveScale;

                float rotationSpeed = IsPlaying ? (1f - layerDepth * 0.5f) : 0.15f;
                float layerRotation = layer * 0.4f + spiralPhase * rotationSpeed + rotationZ * (1f - layerDepth);

                canvas.Save();
                canvas.Scale(layerScale);
                canvas.RotateRadians(layerRotation);

                // Variedad de fractales con morfing
                float morphFactor = (float)Math.Sin(morphingPhase + layer * 0.3f) * 0.5f + 0.5f;

                if (morphFactor > 0.7f)
                {
                    DrawAdvancedSpiralFractal(canvas, baseSize * layerScale, layer);
                }
                else if (morphFactor > 0.4f)
                {
                    DrawGeometricMandala(canvas, baseSize * layerScale, layer);
                }
                else
                {
                    DrawFlowerOfLife(canvas, baseSize * layerScale, layer);
                }

                canvas.Restore();
            }
        }

        private void DrawAdvancedSpiralFractal(SKCanvas canvas, float radius, int layer)
        {
            var paint = GetEnhanced3DPaint(layer % cachedPaints.Length, layer, 0);

            int numArms = IsPlaying ? 5 + (int)(smoothBass * 4) : 4;
            numArms = Math.Min(numArms, 8);

            for (int arm = 0; arm < numArms; arm++)
            {
                float armRotation = (arm * (float)Math.PI * 2f / numArms) + spiralPhase * 0.3f;

                canvas.Save();
                canvas.RotateRadians(armRotation);
                DrawEnhanced3DSpiralArm(canvas, radius, layer, paint);
                canvas.Restore();
            }
        }

        private void DrawEnhanced3DSpiralArm(SKCanvas canvas, float radius, int layer, SKPaint paint)
        {
            int numPoints = IsPlaying ? 140 : 80;
            var points = new SKPoint[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float t = (float)i / (numPoints - 1);
                float angle = t * (float)Math.PI * 8f + spiralPhase + (layer * 0.2f);
                float spiralRadius = radius * (0.05f + t * t * 0.95f);

                // Ondas más complejas con efectos 3D
                float wave1 = (float)Math.Sin(angle * 1.5f + waveDistortion) * 0.18f;
                float wave2 = (float)Math.Cos(angle * 2.3f + time * 1.2f) * 0.12f;
                float bassWave = IsPlaying ? smoothBass * (float)Math.Sin(angle * 2f + pulsePhase) * 0.35f : 0f;
                float midWave = IsPlaying ? smoothMid * (float)Math.Cos(angle * 3.2f + time * 1.8f) * 0.25f : 0f;
                float highWave = IsPlaying ? smoothHigh * (float)Math.Sin(angle * 4.1f + time * 2.5f) * 0.15f : 0f;

                // Efecto de profundidad 3D
                float depthWave = (float)Math.Sin(angle * 0.8f + tunnelEffect) * 0.1f;

                float totalMod = wave1 + wave2 + bassWave + midWave + highWave + depthWave;
                float finalRadius = spiralRadius * (1f + totalMod);

                // Simular perspectiva Z
                float z = (float)Math.Sin(angle * 1.1f + layer * 0.5f) * 50f;
                float perspectiveFactor = perspective / (perspective + z);

                points[i] = new SKPoint(
                    (float)Math.Cos(angle) * finalRadius * perspectiveFactor,
                    (float)Math.Sin(angle) * finalRadius * perspectiveFactor
                );
            }

            using (var path = new SKPath())
            {
                path.AddPoly(points, false);
                canvas.DrawPath(path, paint);
            }
        }

        private void DrawGeometricMandala(SKCanvas canvas, float radius, int layer)
        {
            var paint = GetEnhanced3DPaint((layer + 2) % cachedPaints.Length, layer, 1);

            int sides = IsPlaying ? 6 + (int)(smoothMid * 5) : 5;
            sides = Math.Min(sides, 12);

            float angleStep = (float)Math.PI * 2f / sides;

            // Múltiples capas de geometría
            for (int ring = 0; ring < 3; ring++)
            {
                float ringRadius = radius * (0.3f + ring * 0.3f);

                using (var path = new SKPath())
                {
                    for (int i = 0; i < sides; i++)
                    {
                        float angle = i * angleStep + globalRotation * (0.5f + ring * 0.2f);
                        float distance = ringRadius * (0.8f + (IsPlaying ? smoothHigh * 0.3f : 0f));

                        // Ondulación 3D
                        float wave = (float)Math.Sin(angle * 2f + time * 1.5f + ring) * 0.1f;
                        distance *= (1f + wave);

                        float x = (float)Math.Cos(angle) * distance;
                        float y = (float)Math.Sin(angle) * distance;

                        if (i == 0) path.MoveTo(x, y);
                        else path.LineTo(x, y);
                    }
                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawFlowerOfLife(SKCanvas canvas, float radius, int layer)
        {
            var paint = GetEnhanced3DPaint((layer + 4) % cachedPaints.Length, layer, 2);

            int petalCount = IsPlaying ? 7 + (int)(smoothBass * 3) : 6;
            petalCount = Math.Min(petalCount, 12);

            float petalAngle = (float)Math.PI * 2f / petalCount;

            for (int petal = 0; petal < petalCount; petal++)
            {
                float angle = petal * petalAngle + foldPhase * 0.5f;
                float petalRadius = radius * (0.4f + (IsPlaying ? smoothMid * 0.2f : 0f));

                canvas.Save();
                canvas.RotateRadians(angle);

                using (var path = new SKPath())
                {
                    // Crear pétalo con curvas complejas
                    for (int i = 0; i <= 30; i++)
                    {
                        float t = (float)i / 30f;
                        float petalAngleLocal = t * (float)Math.PI;

                        float petalX = petalRadius * t * (float)Math.Cos(petalAngleLocal);
                        float petalY = petalRadius * (float)Math.Sin(petalAngleLocal) *
                                      (float)Math.Sin(petalAngleLocal * 2f + time + layer * 0.3f) * 0.8f;

                        if (i == 0) path.MoveTo(petalX, petalY);
                        else path.LineTo(petalX, petalY);
                    }
                    canvas.DrawPath(path, paint);
                }

                canvas.Restore();
            }
        }

        private SKPaint GetEnhanced3DPaint(int index, int layer, int type)
        {
            var paint = cachedPaints[index];

            // Colores más vibrantes y dinámicos
            float dynamicHue = (colorShift * 15f + layer * 30f + time * 8f + lightingAngle * 20f) % 360f;

            float saturation = IsPlaying ?
                (type == 0 ? 85f + smoothBass * 15f :
                 type == 1 ? 75f + smoothMid * 20f :
                 65f + smoothHigh * 25f) : 55f;

            float brightness = IsPlaying ?
                (type == 0 ? 75f + smoothBass * 25f + (float)Math.Sin(lightingAngle + layer) * 15f :
                 type == 1 ? 70f + smoothMid * 30f + (float)Math.Cos(lightingAngle * 1.3f + layer) * 12f :
                 65f + smoothHigh * 35f + (float)Math.Sin(lightingAngle * 0.8f + layer) * 18f) : 45f;

            var color = SKColor.FromHsv(dynamicHue, saturation, brightness);

            // Alpha con efecto de profundidad 3D
            float alpha = 0.85f - (layer * 0.05f) + (IsPlaying ? energyLevel * 0.15f : 0f);
            alpha += (float)Math.Sin(hologramPhase + layer * 0.4f) * 0.1f; // Efecto holográfico

            float strokeWidth = IsPlaying ?
                (type == 0 ? 2.2f + smoothBass * 2.8f :
                 type == 1 ? 1.8f + smoothMid * 2.2f :
                 1.4f + smoothHigh * 2.6f) : 1.2f;

            paint.StrokeWidth = Math.Max(strokeWidth - layer * 0.06f, 0.4f);
            paint.Color = color.WithAlpha((byte)(255 * Math.Max(alpha, 0.15f)));

            // Modos de mezcla más dinámicos
            paint.BlendMode = layer % 4 == 0 ? SKBlendMode.Plus :
                             layer % 4 == 1 ? SKBlendMode.Screen :
                             layer % 4 == 2 ? SKBlendMode.ColorDodge : SKBlendMode.Lighten;

            return paint;
        }

        private void DrawAdvancedEffects(SKCanvas canvas, SKImageInfo info, float centerX, float centerY, float baseSize)
        {
            // Efectos de energía mejorados
            if (energyLevel > 0.2f)
            {
                DrawQuantumEnergyRings(canvas, centerX, centerY, baseSize);
            }

            if (smoothHigh > 0.25f)
            {
                DrawAdvancedParticleSystem(canvas, info, centerX, centerY, baseSize);
            }

            if (smoothBass > 0.3f)
            {
                DrawLightningBolts(canvas, centerX, centerY, baseSize);
            }

            if (smoothMid > 0.35f)
            {
                DrawHolographicProjections(canvas, info, centerX, centerY, baseSize);
            }
        }

        private void DrawQuantumEnergyRings(SKCanvas canvas, float centerX, float centerY, float baseSize)
        {
            int ringCount = Math.Min((int)(energyLevel * 8), 6);

            using (var ringPaint = new SKPaint())
            {
                ringPaint.Style = SKPaintStyle.Stroke;
                ringPaint.IsAntialias = true;
                ringPaint.BlendMode = SKBlendMode.Plus;

                for (int i = 0; i < ringCount; i++)
                {
                    float ringRadius = baseSize * (0.7f + i * 0.25f + (float)Math.Sin(time * 4f + i) * 0.15f);
                    float hue = (colorShift * 12f + i * 72f + time * 30f) % 360f;

                    // Efecto de pulso cuántico
                    float pulse = (float)Math.Sin(pulsePhase * 3f + i * 0.8f) * 0.5f + 0.5f;
                    float quantumGlow = energyLevel * pulse;

                    ringPaint.Color = SKColor.FromHsv(hue, 80f + quantumGlow * 20f, 70f + quantumGlow * 30f)
                                           .WithAlpha((byte)(140 + quantumGlow * 80));
                    ringPaint.StrokeWidth = 1.5f + smoothBass * 3f + pulse * 2f;

                    canvas.DrawCircle(centerX, centerY, ringRadius, ringPaint);

                    // Anillos internos con efecto de interferencia
                    if (i % 2 == 0)
                    {
                        ringPaint.StrokeWidth *= 0.3f;
                        ringPaint.Color = ringPaint.Color.WithAlpha(80);
                        canvas.DrawCircle(centerX, centerY, ringRadius * 0.7f, ringPaint);
                    }
                }
            }
        }

        private void DrawAdvancedParticleSystem(SKCanvas canvas, SKImageInfo info, float centerX, float centerY, float baseSize)
        {
            int particleCount = Math.Min((int)(smoothHigh * 40 + energyLevel * 20), 35);

            using (var particlePaint = new SKPaint())
            {
                particlePaint.Style = SKPaintStyle.Fill;
                particlePaint.IsAntialias = true;
                particlePaint.BlendMode = SKBlendMode.Plus;

                for (int i = 0; i < particleCount; i++)
                {
                    // Múltiples sistemas de partículas
                    float systemAngle = (i * 0.618f * (float)Math.PI * 2f) + particleSystemTime * 1.5f;
                    float systemRadius = baseSize * (0.3f + (float)Math.Sin(particleSystemTime * 1.2f + i * 0.1f) * 0.5f);

                    // Órbitas complejas
                    float orbitAngle = systemAngle * 3f + time * 2f;
                    float orbitRadius = systemRadius * 0.2f * (1f + (float)Math.Sin(time * 3f + i) * 0.3f);

                    float x = centerX + (float)Math.Cos(systemAngle) * systemRadius +
                             (float)Math.Cos(orbitAngle) * orbitRadius;
                    float y = centerY + (float)Math.Sin(systemAngle) * systemRadius +
                             (float)Math.Sin(orbitAngle) * orbitRadius;

                    // Colores dinámicos con brillo
                    float hue = (particleSystemTime * 80f + i * 25f + smoothHigh * 50f) % 360f;
                    float brightness = 60f + smoothHigh * 40f + (float)Math.Sin(time * 4f + i) * 20f;

                    particlePaint.Color = SKColor.FromHsv(hue, 90f, brightness).WithAlpha(180);

                    float size = 1.2f + smoothHigh * 4f + (float)Math.Sin(time * 5f + i * 0.3f) * 1.5f;
                    canvas.DrawCircle(x, y, size, particlePaint);

                    // Estela de partículas
                    if (i % 3 == 0)
                    {
                        particlePaint.Color = particlePaint.Color.WithAlpha(60);
                        float trailX = x - (float)Math.Cos(systemAngle) * size * 3f;
                        float trailY = y - (float)Math.Sin(systemAngle) * size * 3f;
                        canvas.DrawCircle(trailX, trailY, size * 0.4f, particlePaint);
                    }
                }
            }
        }

        private void DrawLightningBolts(SKCanvas canvas, float centerX, float centerY, float baseSize)
        {
            int boltCount = Math.Min((int)(smoothBass * 6), 4);

            using (var lightningPaint = new SKPaint())
            {
                lightningPaint.Style = SKPaintStyle.Stroke;
                lightningPaint.StrokeCap = SKStrokeCap.Round;
                lightningPaint.IsAntialias = true;
                lightningPaint.BlendMode = SKBlendMode.Plus;

                for (int bolt = 0; bolt < boltCount; bolt++)
                {
                    float boltAngle = (bolt * (float)Math.PI * 2f / boltCount) + time * 2f;
                    float intensity = smoothBass + (float)Math.Sin(time * 6f + bolt) * 0.3f;

                    var lightningColor = SKColor.FromHsv((time * 100f + bolt * 90f) % 360f, 70f, 90f);
                    lightningPaint.Color = lightningColor.WithAlpha((byte)(intensity * 200f));
                    lightningPaint.StrokeWidth = 2f + intensity * 4f;

                    using (var path = new SKPath())
                    {
                        float startX = centerX + (float)Math.Cos(boltAngle) * baseSize * 0.2f;
                        float startY = centerY + (float)Math.Sin(boltAngle) * baseSize * 0.2f;
                        path.MoveTo(startX, startY);

                        // Segmentos de rayo con irregularidades
                        int segments = 8;
                        for (int seg = 1; seg <= segments; seg++)
                        {
                            float t = (float)seg / segments;
                            float distance = baseSize * (0.2f + t * 0.6f);

                            // Añadir irregularidad al rayo
                            float irregularity = (float)Math.Sin(time * 10f + bolt * 3f + seg) * baseSize * 0.1f * intensity;
                            float segAngle = boltAngle + irregularity * 0.1f;

                            float x = centerX + (float)Math.Cos(segAngle) * distance +
                                     (float)Math.Sin(time * 8f + seg * 2f) * irregularity;
                            float y = centerY + (float)Math.Sin(segAngle) * distance +
                                     (float)Math.Cos(time * 7f + seg * 1.5f) * irregularity;

                            path.LineTo(x, y);
                        }

                        canvas.DrawPath(path, lightningPaint);
                    }
                }
            }
        }

        private void DrawHolographicProjections(SKCanvas canvas, SKImageInfo info, float centerX, float centerY, float baseSize)
        {
            int projectionCount = Math.Min((int)(smoothMid * 5), 3);

            using (var holoPaint = new SKPaint())
            {
                holoPaint.Style = SKPaintStyle.Stroke;
                holoPaint.IsAntialias = true;
                holoPaint.BlendMode = SKBlendMode.Screen;

                for (int proj = 0; proj < projectionCount; proj++)
                {
                    float projAngle = (proj * (float)Math.PI * 2f / projectionCount) + hologramPhase;
                    float projDistance = baseSize * (1.2f + proj * 0.3f);

                    float projX = centerX + (float)Math.Cos(projAngle) * projDistance;
                    float projY = centerY + (float)Math.Sin(projAngle) * projDistance;

                    // Proyección holográfica con interferencia
                    float holoIntensity = smoothMid * (0.7f + (float)Math.Sin(hologramPhase * 2f + proj) * 0.3f);
                    var holoColor = SKColor.FromHsv((hologramPhase * 60f + proj * 120f) % 360f, 60f, 80f);

                    holoPaint.Color = holoColor.WithAlpha((byte)(holoIntensity * 150f));
                    holoPaint.StrokeWidth = 1f + smoothMid * 2f;

                    // Múltiples capas de proyección
                    for (int layer = 0; layer < 3; layer++)
                    {
                        float layerScale = 1f - layer * 0.2f;
                        float layerAlpha = holoIntensity * (1f - layer * 0.3f);

                        holoPaint.Color = holoColor.WithAlpha((byte)(layerAlpha * 120f));

                        canvas.DrawCircle(projX, projY, baseSize * 0.3f * layerScale, holoPaint);

                        // Líneas de conexión holográfica
                        holoPaint.StrokeWidth = 0.5f + smoothMid;
                        canvas.DrawLine(centerX, centerY, projX, projY, holoPaint);
                    }
                }
            }
        }

        private void DrawPostEffects(SKCanvas canvas, SKImageInfo info)
        {
            // Efecto de viñeta dinámica
            if (energyLevel > 0.1f || !IsPlaying)
            {
                DrawDynamicVignette(canvas, info);
            }

            // Efecto de escaneo holográfico
            if (IsPlaying && smoothMid > 0.2f)
            {
                DrawHolographicScanlines(canvas, info);
            }

            // Efecto de distorsión cromática
            if (IsPlaying && energyLevel > 0.4f)
            {
                DrawChromaticAberration(canvas, info);
            }
        }

        private void DrawDynamicVignette(SKCanvas canvas, SKImageInfo info)
        {
            var vignetteColors = new SKColor[3];

            if (IsPlaying)
            {
                vignetteColors[0] = SKColors.Transparent;
                vignetteColors[1] = SKColor.FromHsv(backgroundHue % 360f, 30f, 5f).WithAlpha(40);
                vignetteColors[2] = SKColor.FromHsv((backgroundHue + 180f) % 360f, 50f, 8f).WithAlpha(120);
            }
            else
            {
                vignetteColors[0] = SKColors.Transparent;
                vignetteColors[1] = SKColors.Black.WithAlpha(20);
                vignetteColors[2] = SKColors.Black.WithAlpha(80);
            }

            var positions = new float[] { 0f, 0.7f, 1f };

            using (var shader = SKShader.CreateRadialGradient(
                new SKPoint(info.Width / 2f, info.Height / 2f),
                Math.Min(info.Width, info.Height) * 0.8f,
                vignetteColors, positions, SKShaderTileMode.Clamp))
            using (var vignettePaint = new SKPaint { Shader = shader, BlendMode = SKBlendMode.Multiply })
            {
                canvas.DrawRect(0, 0, info.Width, info.Height, vignettePaint);
            }
        }

        private void DrawHolographicScanlines(SKCanvas canvas, SKImageInfo info)
        {
            using (var scanPaint = new SKPaint())
            {
                scanPaint.Style = SKPaintStyle.Stroke;
                scanPaint.StrokeWidth = 1f;
                scanPaint.IsAntialias = false; // Para efecto pixelado
                scanPaint.BlendMode = SKBlendMode.Plus;

                float scanSpeed = time * 200f;
                float scanAlpha = smoothMid * 60f;

                var scanColor = SKColor.FromHsv((hologramPhase * 40f) % 360f, 40f, 70f);
                scanPaint.Color = scanColor.WithAlpha((byte)scanAlpha);

                for (float y = (scanSpeed % 6f); y < info.Height; y += 6f)
                {
                    float lineAlpha = scanAlpha * (0.3f + (float)Math.Sin(y * 0.1f + time * 3f) * 0.7f);
                    scanPaint.Color = scanColor.WithAlpha((byte)lineAlpha);
                    canvas.DrawLine(0, y, info.Width, y, scanPaint);
                }
            }
        }

        private void DrawChromaticAberration(SKCanvas canvas, SKImageInfo info)
        {
            // Efecto sutil de aberración cromática en los bordes
            using (var aberrationPaint = new SKPaint())
            {
                aberrationPaint.BlendMode = SKBlendMode.Plus;
                aberrationPaint.Style = SKPaintStyle.Stroke;
                aberrationPaint.StrokeWidth = 2f + energyLevel * 3f;

                float aberrationIntensity = energyLevel * 0.5f;

                // Borde rojo
                aberrationPaint.Color = SKColors.Red.WithAlpha((byte)(aberrationIntensity * 40f));
                canvas.DrawRect(2f, 2f, info.Width - 4f, info.Height - 4f, aberrationPaint);

                // Borde azul
                aberrationPaint.Color = SKColors.Blue.WithAlpha((byte)(aberrationIntensity * 35f));
                canvas.DrawRect(-2f, -2f, info.Width + 4f, info.Height + 4f, aberrationPaint);
            }
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Math.Min(t, 1f);
        }

        public void Dispose()
        {
            if (cachedPaints != null)
            {
                foreach (var paint in cachedPaints)
                {
                    paint?.Dispose();
                }
            }

            if (cachedEffectPaints != null)
            {
                foreach (var paint in cachedEffectPaints)
                {
                    paint?.Dispose();
                }
            }
        }
    }
}
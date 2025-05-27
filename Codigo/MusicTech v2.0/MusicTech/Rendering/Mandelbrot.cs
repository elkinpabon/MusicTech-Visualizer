using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace MusicTech.Rendering
{
    public class Mandelbrot : IVisualizer
    {
        public float[] Magnitudes { get; set; }
        public bool IsPlaying { get; set; }

        private float time = 0f;
        private float zoom = 1.0f;
        private float offsetX = -0.5f;
        private float offsetY = 0f;
        private float rotation = 0f;
        private float colorShift = 0f;
        private float morphFactor = 0f;

        private SKBitmap cachedBitmap;
        private SKImageInfo lastInfo;
        private byte[] pixelBuffer;

        private float[] audioSmooth = new float[8];
        private float bassEnergy = 0f;
        private float midEnergy = 0f;
        private float highEnergy = 0f;
        private float totalEnergy = 0f;

        private float targetZoom = 1.0f;
        private float baseZoom = 1.0f;
        private float targetOffsetX = -0.5f;
        private float targetOffsetY = 0f;
        private float targetRotation = 0f;
        private float targetColorShift = 0f;
        private float targetMorph = 0f;

        private const float ZOOM_SMOOTH = 0.08f;
        private const float MOVE_SMOOTH = 0.06f;
        private const float COLOR_SMOOTH = 0.15f;
        private const float ROTATION_SMOOTH = 0.04f;
        private const float MORPH_SMOOTH = 0.1f;

        private int renderScale = 2;
        private int baseMaxIter = 60;
        private int frameCount = 0;

        public void Render(SKCanvas canvas, SKImageInfo info)
        {
            if (Magnitudes == null || Magnitudes.Length == 0)
            {
                canvas.Clear(SKColors.Black);
                return;
            }

            frameCount++;
            time += 0.016f;

            AnalyzeAudio();
            UpdateMusicEffects();
            SmoothTransitions();
            EnsureBitmapInitialized(info);

            bool needsUpdate = frameCount % 2 == 0 || HasSignificantChange();
            if (needsUpdate)
            {
                UpdateMandelbrot(info);
            }

            RenderWithEffects(canvas, info);
        }

        private void AnalyzeAudio()
        {
            if (Magnitudes == null || Magnitudes.Length == 0) return;

            int bands = Math.Min(8, Magnitudes.Length);
            for (int i = 0; i < bands; i++)
            {
                audioSmooth[i] = audioSmooth[i] * 0.7f + Magnitudes[i] * 0.3f;
            }

            bassEnergy = 0f;
            midEnergy = 0f;
            highEnergy = 0f;
            totalEnergy = 0f;

            for (int i = 0; i < bands; i++)
            {
                float energy = audioSmooth[i] * 100f;
                totalEnergy += energy;

                if (i < bands / 3) bassEnergy += energy;
                else if (i < 2 * bands / 3) midEnergy += energy;
                else highEnergy += energy;
            }

            bassEnergy = Math.Min(1f, bassEnergy / (bands / 3));
            midEnergy = Math.Min(1f, midEnergy / (bands / 3));
            highEnergy = Math.Min(1f, highEnergy / (bands / 3));
            totalEnergy = Math.Min(1f, totalEnergy / bands);
        }

        private void UpdateMusicEffects()
        {
            baseZoom = 1.0f + (float)Math.Sin(time * 0.3f) * 0.05f;
            float zoomPulse = bassEnergy * 0.1f;
            targetZoom = baseZoom + zoomPulse;

            float orbitSpeed = midEnergy * 0.02f;
            targetOffsetX = -0.5f + (float)Math.Sin(time * orbitSpeed) * midEnergy * 0.1f;
            targetOffsetY = (float)Math.Cos(time * orbitSpeed) * midEnergy * 0.1f;

            targetRotation += highEnergy * 0.05f;
            targetColorShift += totalEnergy * 2f + time * 10f;
            targetMorph = (float)Math.Sin(time * 0.5f + bassEnergy * 3f) * totalEnergy;

            if (bassEnergy > 0.8f) targetZoom += 0.05f;
            if (totalEnergy > 0.9f) targetRotation += 0.1f;
        }

        private void SmoothTransitions()
        {
            zoom = Lerp(zoom, targetZoom, ZOOM_SMOOTH);
            offsetX = Lerp(offsetX, targetOffsetX, MOVE_SMOOTH);
            offsetY = Lerp(offsetY, targetOffsetY, MOVE_SMOOTH);
            rotation = Lerp(rotation, targetRotation, ROTATION_SMOOTH);
            colorShift = Lerp(colorShift, targetColorShift, COLOR_SMOOTH);
            morphFactor = Lerp(morphFactor, targetMorph, MORPH_SMOOTH);
        }

        private bool HasSignificantChange()
        {
            return totalEnergy > 0.1f || bassEnergy > 0.3f;
        }

private const int MAX_RENDER_WIDTH = 800;
private const int MAX_RENDER_HEIGHT = 600;

private void EnsureBitmapInitialized(SKImageInfo info)
{
    int renderWidth = Math.Min(info.Width / renderScale, MAX_RENDER_WIDTH);
    int renderHeight = Math.Min(info.Height / renderScale, MAX_RENDER_HEIGHT);

    if (cachedBitmap == null ||
        cachedBitmap.Width != renderWidth ||
        cachedBitmap.Height != renderHeight)
    {
        cachedBitmap?.Dispose();
        cachedBitmap = new SKBitmap(renderWidth, renderHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        pixelBuffer = new byte[renderWidth * renderHeight * 4];
        cachedBitmap.Erase(SKColors.Black);
    }
}



        private void UpdateMandelbrot(SKImageInfo info)
        {
            if (cachedBitmap == null) return;

            int renderWidth = cachedBitmap.Width;
            int renderHeight = cachedBitmap.Height;
            int maxIter = Math.Max(40, (int)(baseMaxIter + totalEnergy * 40));

            float cosR = (float)Math.Cos(rotation);
            float sinR = (float)Math.Sin(rotation);

            Parallel.For(0, renderHeight, y =>
            {
                for (int x = 0; x < renderWidth; x++)
                {
                    var color = CalculateEnhancedMandelbrot(x, y, renderWidth, renderHeight, maxIter, cosR, sinR);
                    int index = (y * renderWidth + x) * 4;

                    pixelBuffer[index] = color.Red;
                    pixelBuffer[index + 1] = color.Green;
                    pixelBuffer[index + 2] = color.Blue;
                    pixelBuffer[index + 3] = color.Alpha;
                }
            });

            if (cachedBitmap != null && pixelBuffer != null)
            {
                IntPtr pixelsAddr = cachedBitmap.GetPixels();
                if (pixelsAddr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, 0, pixelsAddr, pixelBuffer.Length);
                }
            }
        }

        private SKColor CalculateEnhancedMandelbrot(int x, int y, int width, int height,
            int maxIter, float cosR, float sinR)
        {
            float px = (x - width * 0.5f) / (0.5f * zoom * width);
            float py = (y - height * 0.5f) / (0.5f * zoom * height);

            float zx = px * cosR - py * sinR + offsetX;
            float zy = px * sinR + py * cosR + offsetY;

            float cx = zx;
            float cy = zy;

            float morphX = morphFactor * 0.1f;
            float morphY = morphFactor * 0.05f;

            int iterations = 0;
            float zx2 = zx * zx;
            float zy2 = zy * zy;

            while (zx2 + zy2 < 4.0f && iterations < maxIter)
            {
                zy = 2.0f * zx * zy + cy + morphY;
                zx = zx2 - zy2 + cx + morphX;
                zx2 = zx * zx;
                zy2 = zy * zy;
                iterations++;
            }

            if (iterations == maxIter)
            {
                float innerHue = (colorShift + bassEnergy * 60) % 360;
                return SKColor.FromHsv(innerHue, 20 + totalEnergy * 30, 10 + bassEnergy * 20);
            }

            double smoothIter = iterations + 1.0 - Math.Log(Math.Log(Math.Sqrt(zx2 + zy2))) / Math.Log(2.0);

            float baseHue = (colorShift + (float)smoothIter * 12) % 360;
            float hue = (baseHue + bassEnergy * 120 + midEnergy * 60 + highEnergy * 180) % 360;
            float saturation = Math.Min(100, 70 + totalEnergy * 30);
            float brightness = Math.Min(100, 30 + ((float)smoothIter / maxIter) * 70 + totalEnergy * 20);

            if (bassEnergy > 0.7f)
            {
                brightness = Math.Min(100, brightness + 30);
                saturation = Math.Min(100, saturation + 20);
            }

            return SKColor.FromHsv(hue, saturation, brightness);
        }

        private void RenderWithEffects(SKCanvas canvas, SKImageInfo info)
        {
            if (cachedBitmap == null)
            {
                canvas.Clear(SKColors.Black);
                return;
            }

            using (var paint = new SKPaint())
            {
                paint.FilterQuality = SKFilterQuality.Medium;
                paint.IsAntialias = true;

                if (totalEnergy > 0.8f)
                {
                    try
                    {
                        paint.ImageFilter = SKImageFilter.CreateDropShadow(
                            0, 0, 5 + totalEnergy * 10, 5 + totalEnergy * 10,
                            SKColor.FromHsv(colorShift % 360, 100, 50));
                    }
                    catch { }
                }

                var destRect = new SKRect(0, 0, info.Width, info.Height);
                canvas.DrawBitmap(cachedBitmap, destRect, paint);
            }
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        public void Dispose()
        {
            cachedBitmap?.Dispose();
            cachedBitmap = null;
        }

        private void AdjustPerformanceBasedOnEnergy(SKImageInfo info)
        {
            int desiredScale;

            if (totalEnergy > 0.7f)
                desiredScale = 1; // Máxima calidad
            else if (totalEnergy > 0.4f)
                desiredScale = 2;
            else
                desiredScale = 3; // Mayor rendimiento

            // Solo cambiar si es diferente para evitar recrear bitmap en cada frame
            if (desiredScale != renderScale)
            {
                renderScale = desiredScale;
                cachedBitmap?.Dispose();
                cachedBitmap = null;
                pixelBuffer = null;
            }
        }

    }
}

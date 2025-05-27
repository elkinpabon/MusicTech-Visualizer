using SkiaSharp;

namespace MusicTech.Rendering
{
    public interface IVisualizer
    {
        bool IsPlaying { get; set; }
        float[] Magnitudes { get; set; }

        void Render(SKCanvas canvas, SKImageInfo info);
    }
}

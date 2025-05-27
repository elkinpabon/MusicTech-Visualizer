using MusicTech.Audio;
using NAudio.Dsp;
using NAudio.Wave;

namespace MusicTech.Utils
{
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly AudioAnalyzer analyzer;

        public SampleAggregator(ISampleProvider source, AudioAnalyzer analyzer)
        {
            this.source = source;
            this.analyzer = analyzer;
        }

        public WaveFormat WaveFormat => source.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int read = source.Read(buffer, offset, count);

            for (int i = 0; i < read; i++)
            {
                analyzer.AddSample(buffer[offset + i]);
            }

            return read;
        }
    }
}

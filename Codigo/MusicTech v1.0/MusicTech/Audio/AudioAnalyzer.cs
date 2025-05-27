using System;
using NAudio.Dsp;

namespace MusicTech.Audio
{
    public class AudioAnalyzer
    {
        private readonly Complex[] fftBuffer;
        private readonly int fftLength;
        private int bufferPosition;

        public float[] Magnitudes { get; private set; }

        public AudioAnalyzer(int fftLength = 1024)
        {
            this.fftLength = fftLength;
            fftBuffer = new Complex[fftLength];
            Magnitudes = new float[fftLength / 2];
        }

        public void AddSample(float value)
        {
            fftBuffer[bufferPosition].X = value;
            fftBuffer[bufferPosition].Y = 0;
            bufferPosition++;

            if (bufferPosition >= fftLength)
            {
                bufferPosition = 0;
                ComputeFFT();
            }
        }

        private void ComputeFFT()
        {
            FastFourierTransform.FFT(true, (int)Math.Log(fftLength, 2), fftBuffer);
            for (int i = 0; i < fftLength / 2; i++)
            {
                Magnitudes[i] = (float)Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y);
            }
        }
    }
}

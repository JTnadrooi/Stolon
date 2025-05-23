using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Utils;
using System.Collections.ObjectModel;

using AsitLib;


#nullable enable

namespace STOLON
{
    public class CachedAudioSampleProvider : ISampleProvider
    {
        private readonly CachedAudio _cachedAudio;
        private long _position;

        public long Position => _position;
        public CachedAudio CachedAudio => _cachedAudio;
        public WaveFormat WaveFormat => CachedAudio.WaveFormat;
        public long Lenght => CachedAudio.AudioData.Length;
        public bool Finished => AvailableSamples < 1;
        public long AvailableSamples => Lenght - Position;

        public CachedAudioSampleProvider(CachedAudio audio) => _cachedAudio = audio;

        public int Read(float[] buffer, int offset, int count)
        {
            long samplesToCopy = Math.Min(AvailableSamples, count);
            Array.Copy(_cachedAudio.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;
            return (int)samplesToCopy;
        }
    }
    public class CachedAudio
    {
        public float[] AudioData { get; private set; }
        public string ID { get; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedAudio(string audioFileName, string ID)
        {
            this.ID = ID;
            using var audioFileReader = new AudioFileReader(audioFileName);
            WaveFormat = audioFileReader.WaveFormat;
            List<float> wholeFile = new List<float>((int)(audioFileReader.Length / 4));
            float[] readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                wholeFile.AddRange(readBuffer.Take(samplesRead));
            AudioData = wholeFile.ToArray();
        }
        public CachedAudioSampleProvider GetAsSampleProvider() => new CachedAudioSampleProvider(this);
    }
}

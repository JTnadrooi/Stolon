using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Utils;
using System.Collections.ObjectModel;
#nullable enable

namespace Stolon
{
    public class AudioEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly DictionaryMixingSampleProvider mixer;
        private readonly VolumeSampleProvider volumeSampleProvider;
        private readonly FadeInOutSampleProvider fadeInOutSampleProvider;
        public float Volume
        {
            get => volumeSampleProvider.Volume;
            set
            {
                volumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
            }
        }
        public AudioEngine()
        {
            outputDevice = new DirectSoundOut(40);
            mixer = new DictionaryMixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            mixer.ReadFully = true;

            volumeSampleProvider = new VolumeSampleProvider(mixer);
            fadeInOutSampleProvider = new FadeInOutSampleProvider(volumeSampleProvider, true);
            Volume = .1f;
            //outputDevice.Init(mixer);
            outputDevice.Init(volumeSampleProvider);
            outputDevice.Play();
        }
        static AudioEngine()
        {
            AudioLibrary = new Dictionary<string, CachedAudio>();
        }
        public AudioFileReader Play(string fileName, string id) // should never be used
        {
            AudioFileReader input = new AudioFileReader(fileName);
            Console.WriteLine(input.WaveFormat);
            AddMixerInput(new AutoDisposeFileReader(input), id);
            return input;
        }
        public CachedAudio Play(CachedAudio audio)
        {
            AddMixerInput(new CachedAudioSampleProvider(audio), audio.ID);
            return audio;
        }
        public CachedAudio CancelCashed(string audioId)
        {
            CachedAudio audio = (CachedAudio)mixer.Sources[audioId];
            mixer.RemoveMixerInput(audioId);
            return audio;
        }
        private void AddMixerInput(ISampleProvider input, string id)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels) mixer.AddMixerInput(id, input);
            else if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2) mixer.AddMixerInput(id, new MonoToStereoSampleProvider(input));
            else throw new NotImplementedException("Not yet implemented this channel count conversion.");
        }
        public void Dispose()
        {
            StolonGame.Instance.DebugStream.WriteLine("Disposing audio engine..");
            outputDevice.Dispose();
        }
        public static AudioEngine Instance => StolonGame.Instance.AudioEngine;
        public static DictionaryMixingSampleProvider Mixer => Instance.mixer;
        public static Dictionary<string, CachedAudio> AudioLibrary { get; }
    }
    public class CachedAudio
    {
        public float[] AudioData { get; private set; }
        public string ID { get; }
        public WaveFormat WaveFormat { get; private set; }
        public CachedAudio(string audioFileName, string ID)
        {
            this.ID = ID;
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }
                AudioData = wholeFile.ToArray();
            }
        }
    }
    public class CachedAudioSampleProvider : ISampleProvider
    {
        private readonly CachedAudio cachedAudio;
        private long position;

        public CachedAudio CachedAudio => cachedAudio;
        public CachedAudioSampleProvider(CachedAudio audio) => cachedAudio = audio;

        public int Read(float[] buffer, int offset, int count)
        {
            long availableSamples = cachedAudio.AudioData.Length - position;
            long samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedAudio.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public WaveFormat WaveFormat => cachedAudio.WaveFormat;
    }
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed) return 0;
            if (reader.Read(buffer, offset, count) is int read && read == 0) // woaaaaah
            {
                reader.Dispose();
                isDisposed = true;
            }
            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
    /// <summary>
    /// A sample provider mixer, allowing inputs to be added and removed. Also has a dictiorary.
    /// </summary>
    public class DictionaryMixingSampleProvider : ISampleProvider
    {
        private readonly Dictionary<string, ISampleProvider> sources;
        private float[] sourceBuffer;
        private const int maxInputs = 1024; // protect ourselves against doing something rather goofy

        public ReadOnlyDictionary<string, ISampleProvider> Sources { get; }
        /// <summary>
        /// The output WaveFormat of this sample provider
        /// </summary>
        public WaveFormat? WaveFormat { get; private set; }
        /// <summary>
        /// When set to true, the Read method always returns the number
        /// of samples requested, even if there are no inputs, or if the
        /// current inputs reach their end. Setting this to true effectively
        /// makes this a never-ending sample provider, so take care if you plan
        /// to write it out to a file.
        /// </summary>
        public bool ReadFully { get; set; }
        /// <summary>
        /// Creates a new <see cref="DictionaryMixingSampleProvider"/>, with no inputs, but a specified <see cref="NAudio.Wave.WaveFormat"/>.
        /// </summary>
        /// <param name="waveFormat">The WaveFormat of this mixer. All inputs must be in this format</param>
        public DictionaryMixingSampleProvider(WaveFormat waveFormat)
        {
            sourceBuffer = Array.Empty<float>();
            if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat) throw new ArgumentException("Mixer wave format must be IEEE float");
            sources = new Dictionary<string, ISampleProvider>();
            Sources = sources.AsReadOnly();
            WaveFormat = waveFormat;
            Console.WriteLine(WaveFormat);
        }

        /// <summary>
        /// Creates a new MixingSampleProvider, based on the given inputs
        /// </summary>
        /// <param name="sources">Mixer inputs - must all have the same waveformat, and must
        /// all be of the same WaveFormat. There must be at least one input</param>
        public DictionaryMixingSampleProvider(IDictionary<string, ISampleProvider> sources)
        {
            sourceBuffer = Array.Empty<float>();
            this.sources = new Dictionary<string, ISampleProvider>();
            Sources = sources.AsReadOnly();
            foreach (var source in sources) AddMixerInput(source.Key, source.Value);
            if (this.sources.Count == 0) throw new ArgumentException("Must provide at least one input in this constructor");
        }

        /// <summary>
        /// Adds a WaveProvider as a Mixer input.
        /// Must be PCM or IEEE float already
        /// </summary>
        /// <param name="key">Key for the mixer input</param>
        /// <param name="mixerInput">IWaveProvider mixer input</param>
        public void AddMixerInput(string key, IWaveProvider mixerInput)
        {
            ISampleProvider sampleProvider;
            if (mixerInput.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                sampleProvider = mixerInput.WaveFormat.BitsPerSample switch
                {
                    8 => new Pcm8BitToSampleProvider(mixerInput),
                    16 => new Pcm16BitToSampleProvider(mixerInput),
                    24 => new Pcm24BitToSampleProvider(mixerInput),
                    32 => new Pcm32BitToSampleProvider(mixerInput),
                    _ => throw new InvalidOperationException("Unsupported bit depth")
                };
            else if (mixerInput.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                sampleProvider = mixerInput.WaveFormat.BitsPerSample == 64 ? new WaveToSampleProvider64(mixerInput) : new WaveToSampleProvider(mixerInput);
            else throw new ArgumentException("Unsupported source encoding");
            AddMixerInput(key, sampleProvider);
        }

        /// <summary>
        /// Adds a new mixer input
        /// </summary>
        /// <param name="key">Key for the mixer input</param>
        /// <param name="mixerInput">Mixer input</param>
        public void AddMixerInput(string key, ISampleProvider mixerInput)
        {
            lock (sources)
                if (sources.Count >= maxInputs) throw new InvalidOperationException("Too many mixer inputs");
                else sources[key] = mixerInput;
            if (WaveFormat == null) WaveFormat = mixerInput.WaveFormat;
            //else if(WaveFormat.SampleRate != mixerInput.WaveFormat.SampleRate || WaveFormat.Channels != mixerInput.WaveFormat.Channels)
            //    throw new ArgumentException("All mixer inputs must have the same WaveFormat: mx:" + WaveFormat + " other: " + mixerInput.WaveFormat);
            // lay thine eyes upon it and thou shall see that it is barren. (this is a problem for another time)
        }

        /// <summary>
        /// Removes a mixer input by key
        /// </summary>
        /// <param name="key">Key of the mixer input to remove</param>
        public void RemoveMixerInput(string key)
        {
            lock (sources) sources.Remove(key);
        }

        /// <summary>
        /// Removes all mixer inputs
        /// </summary>
        public void RemoveAllMixerInputs()
        {
            lock (sources) sources.Clear();
        }


        /// <summary>
        /// Reads samples from this sample provider
        /// </summary>
        /// <param name="buffer">Sample buffer</param>
        /// <param name="offset">Offset into sample buffer</param>
        /// <param name="count">Number of samples required</param>
        /// <returns>Number of samples read</returns>
        public int Read(float[] buffer, int offset, int count)
        {
            int outputSamples = 0;
            sourceBuffer = BufferHelpers.Ensure(sourceBuffer, count);
            lock (sources)
            {
                foreach (var sourceEntry in sources.ToArray())
                {
                    var source = sourceEntry.Value;
                    int samplesRead = source.Read(sourceBuffer, 0, count);
                    int outIndex = offset;
                    for (int n = 0; n < samplesRead; n++)
                        if (n >= outputSamples) buffer[outIndex++] = sourceBuffer[n];
                        else buffer[outIndex++] += sourceBuffer[n];
                    outputSamples = Math.Max(samplesRead, outputSamples);
                    if (samplesRead < count) sources.Remove(sourceEntry.Key);
                }
            }
            if (ReadFully && outputSamples < count)
            {
                int outputIndex = offset + outputSamples;
                while (outputIndex < offset + count) buffer[outputIndex++] = 0;
                outputSamples = count;
            }
            return outputSamples;
        }
    }
}

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

namespace Stolon
{
    /// <summary>
    /// Implements a way to automatically dispose <see cref="AudioFileReader"/> objects.
    /// </summary>
    public class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader _reader;
        private bool _isDisposed;
        /// <summary>
        /// Create a new <see cref="AutoDisposeFileReader"/> from a <see cref="AudioFileReader"/>.
        /// </summary>
        /// <param name="reader">The reader to create the wrapper around.</param>
        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this._reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            if (_isDisposed) return 0;
            if (_reader.Read(buffer, offset, count) is int read && read == 0) // woaaaaah!
            {
                _reader.Dispose();
                _isDisposed = true;
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
        private readonly Dictionary<string, ISampleProvider> _sources;
        private float[] _sourceBuffer;
        private const int MAX_INPUTS = 256; // to protect ourselves against doing something rather goofy.

        public ReadOnlyDictionary<string, ISampleProvider> Sources { get; }
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
        /// If the output should be pauzed when the Stolon game window loses focus.
        /// </summary>
        public bool PauzeWhenInactive { get; set; }
        /// <summary>
        /// Creates a new <see cref="DictionaryMixingSampleProvider"/>, with no inputs, but a specified <see cref="NAudio.Wave.WaveFormat"/>.
        /// </summary>
        /// <param name="waveFormat">The WaveFormat of this mixer. All inputs must be in this format</param>
        public DictionaryMixingSampleProvider(WaveFormat waveFormat)
        {
            _sourceBuffer = Array.Empty<float>();
            if (waveFormat.Encoding != WaveFormatEncoding.IeeeFloat) throw new ArgumentException("mixer wave format must be IEEE float.");
            _sources = new Dictionary<string, ISampleProvider>();
            Sources = _sources.AsReadOnly();
            WaveFormat = waveFormat;
            STOLON.Debug.Log("created new DictionaryMixingSampleProvider with waveformat:" + WaveFormat);
        }

        /// <summary>
        /// Creates a new MixingSampleProvider, based on the given inputs
        /// </summary>
        /// <param name="sources">Mixer inputs - must all have the same waveformat, and must
        /// all be of the same WaveFormat. There must be at least one input</param>
        public DictionaryMixingSampleProvider(IDictionary<string, ISampleProvider> sources)
        {
            _sourceBuffer = Array.Empty<float>();
            this._sources = new Dictionary<string, ISampleProvider>();
            Sources = sources.AsReadOnly();
            foreach (var source in sources) AddMixerInput(source.Key, source.Value);
            if (this._sources.Count == 0) throw new ArgumentException("must provide at least one input in this constructor.");
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
                    _ => throw new InvalidOperationException("unsupported bit depth.")
                };
            else if (mixerInput.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat)
                sampleProvider = mixerInput.WaveFormat.BitsPerSample == 64 ? new WaveToSampleProvider64(mixerInput) : new WaveToSampleProvider(mixerInput);
            else throw new ArgumentException("unsupported source encoding.");
            AddMixerInput(key, sampleProvider);
        }

        /// <summary>
        /// Adds a new mixer input
        /// </summary>
        /// <param name="key">Key for the mixer input</param>
        /// <param name="mixerInput">Mixer input</param>
        public void AddMixerInput(string key, ISampleProvider mixerInput)
        {
            lock (_sources)
                if (_sources.Count >= MAX_INPUTS) throw new InvalidOperationException("too many mixer inputs.");
                else _sources[key] = mixerInput;
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
            lock (_sources)
            {
                if (_sources.ContainsKey(key))
                    _sources.Remove(key);
                else throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Removes all mixer inputs
        /// </summary>
        public void RemoveAllMixerInputs()
        {
            lock (_sources) _sources.Clear();
        }
        public int Read(float[] buffer, int offset, int count)
        {
            int outputSamples = 0;
            _sourceBuffer = BufferHelpers.Ensure(_sourceBuffer, count);
            lock (_sources)
            {
                if (PauzeWhenInactive && !STOLON.Instance.IsActive)
                {
                    for (var n = 0; n < count; n++)
                        buffer[offset + n] = 0;
                    return count;
                }

                foreach (var sourceEntry in _sources.ToArray())
                {
                    var source = sourceEntry.Value;
                    int samplesRead = source.Read(_sourceBuffer, 0, count);
                    int outIndex = offset;
                    for (int n = 0; n < samplesRead; n++)
                        if (n >= outputSamples) buffer[outIndex++] = _sourceBuffer[n];
                        else buffer[outIndex++] += _sourceBuffer[n];
                    outputSamples = Math.Max(samplesRead, outputSamples);
                    if (samplesRead < count) _sources.Remove(sourceEntry.Key);
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

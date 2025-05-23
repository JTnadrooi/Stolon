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

using NAudio.Mixer;
using Salaros.Configuration;

#nullable enable

namespace Stolon
{
    public enum AudioDomain
    {
        SFX,
        OST,
    }
    /// <summary>
    /// The main <see cref="AudioEngine"/> for the <see cref="STOLON"/> environment. <i>Moderately thread-safe.</i>
    /// </summary>
    public class AudioEngine : IDisposable // NOT DEBUG SAFE
    {
        public Playlist? Current => _currentPlaylist;

        private IWavePlayer _outputDevice;

        #region mixers

        private MixingSampleProvider _masterMixer;
        private VolumeSampleProvider _masterVolumeSampleProvider;

        private DictionaryMixingSampleProvider _fxMixer;
        private DictionaryMixingSampleProvider _ostMixer;
        private VolumeSampleProvider _fxVolumeSampleProvider;
        private VolumeSampleProvider _ostVolumeSampleProvider;

        #endregion

        private FadeInOutSampleProvider? _fadeInOutSampleProvider;
        private CachedAudioSampleProvider? _fadeInOutSampleProviderSource;
        private List<string> _trackHistory;
        private Queue<string> _trackQueue;
        private Playlist? _currentPlaylist;


        /// <summary>
        /// The mixer used to mix all the <see cref="AudioFileReader"/>
        /// </summary>
        public DictionaryMixingSampleProvider FXMixer => _fxMixer;
        public DictionaryMixingSampleProvider OSTMixer => _ostMixer;
        public MixingSampleProvider MasterMixer => _masterMixer;
        /// <summary>
        /// The fx volume.
        /// </summary>
        public float FxVolume
        {
            get => _fxVolumeSampleProvider.Volume;
            set => _fxVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// The master volume.
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolumeSampleProvider.Volume;
            set => _masterVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// The st volume.
        /// </summary>
        public float OstVolume
        {
            get => _ostVolumeSampleProvider.Volume;
            set => _ostVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// Initialize a new <see cref="AudioEngine"/>.
        /// </summary>
        public AudioEngine()
        {
            _outputDevice = new DirectSoundOut(40);
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            ConfigParser parser = new ConfigParser(@"user.cfg");
            Library = new Dictionary<string, CachedAudio>();

            _masterMixer = new MixingSampleProvider(waveFormat);
            _masterMixer.ReadFully = true;
            _masterVolumeSampleProvider = new VolumeSampleProvider(_masterMixer);

            _fxMixer = new DictionaryMixingSampleProvider(waveFormat);
            _fxMixer.ReadFully = true;
            _fxMixer.PauzeWhenInactive = true;
            _fxVolumeSampleProvider = new VolumeSampleProvider(_fxMixer);

            _ostMixer = new DictionaryMixingSampleProvider(waveFormat);
            _ostMixer.ReadFully = true;
            _ostMixer.PauzeWhenInactive = true;
            _ostVolumeSampleProvider = new VolumeSampleProvider(_ostMixer);
            
            _masterMixer.AddMixerInput(_fxVolumeSampleProvider);
            _masterMixer.AddMixerInput(_ostVolumeSampleProvider);

            _trackHistory = new List<string>();
            _trackQueue = new Queue<string>();

            _outputDevice.Init(_masterVolumeSampleProvider);
            _outputDevice.Play();

            FxVolume = (float)parser.GetValue("Audio", "fx_vol", 0.5f);
            OstVolume = (float)parser.GetValue("Audio", "ost_vol", 1f);
            MasterVolume = (float)parser.GetValue("Audio", "master_vol", 1f);
        }
        ///// <summary>
        ///// Play an filename. <br/> <br/><i>Very slow.</i>
        ///// </summary>
        ///// <param name="fileName">The name of the file.</param>
        ///// <param name="id">The id relevant for the <see cref="DictionaryMixingSampleProvider"/> <see cref="Dictionary{TKey, TValue}"/>.</param>
        ///// <returns>The <see cref="AudioFileReader"/> used to read the file.</returns>
        //public AudioFileReader Play(string fileName, string id) // should never be used
        //{
        //    AudioFileReader input = new AudioFileReader(fileName);
        //    Console.WriteLine(input.WaveFormat);
        //    AddMixerInput(new AutoDisposeFileReader(input), id);
        //    return input;
        //}
        /// <summary>
        /// Play a <see cref="CachedAudio"/> object.
        /// </summary>
        /// <param name="audio">The cached audiofragment.</param>
        /// <returns></returns>
        public CachedAudio Play(CachedAudio audio, AudioDomain domain = AudioDomain.SFX)
        {
            DictionaryMixingSampleProvider mixer = GetMixer(domain);

            AddMixerInput(new CachedAudioSampleProvider(audio), audio.ID, domain);
            return audio;
        }
        /// <summary>
        /// Cancel a <see cref="CachedAudio"/> from playing. 
        /// </summary>
        /// <param name="audioId">The <see cref="CachedAudio.ID"/> of the <see cref="CachedAudio"/> object.</param>
        /// <returns>The canceled <see cref="CachedAudio"/> object.</returns>
        public CachedAudio CancelCashed(string audioId, AudioDomain domain)
        {
            DictionaryMixingSampleProvider mixer = GetMixer(domain);

            CachedAudio audio = (CachedAudio)mixer.Sources[audioId];
            mixer.RemoveMixerInput(audioId);
            return audio;
        }
        public void AddMixerInput(ISampleProvider input, string id, AudioDomain domain)
        {
            DictionaryMixingSampleProvider mixer = GetMixer(domain);

            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels) mixer.AddMixerInput(id, input);
            else if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2) mixer.AddMixerInput(id, new MonoToStereoSampleProvider(input));
            else throw new NotImplementedException("Not yet implemented this channel count conversion.");
        }
        public DictionaryMixingSampleProvider GetMixer(AudioDomain domain) => domain == AudioDomain.SFX ? _fxMixer : _ostMixer;
        public void RemoveMixerInput(string id, AudioDomain domain)
        {
            GetMixer(domain).RemoveMixerInput(id);
        }
        public bool TryRemoveMixerInput(string id, AudioDomain domain)
        {
            DictionaryMixingSampleProvider mixer = GetMixer(domain);
            if (mixer.Sources.ContainsKey(id))
            {
                mixer.RemoveMixerInput(id);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Change the background OST.
        /// </summary>
        /// <param name="id">The ost id from the <see cref="Library"/>.</param>
        public void SetTrack(string id, bool fade = true)
        {
            STOLON.Debug.Log(">track changing to " + id);

            string ostProviderId = "__ostProvider";
            string ostTaskId = "ostChange";
            bool alreadyPlaying = _fadeInOutSampleProvider != null;

            if (alreadyPlaying && fade) _fadeInOutSampleProvider.BeginFadeOut(FadeTimeMiliseconds);
            TaskHeap.Instance.SafePush(ostTaskId, new DynamicTask(() => // fire and forget game logic ftw
            {
                TryRemoveMixerInput(ostProviderId, AudioDomain.OST);
                STOLON.Debug.Log("\ttrack changed to " + id);
                _fadeInOutSampleProviderSource = Library[id].GetAsSampleProvider();
                _fadeInOutSampleProvider = new FadeInOutSampleProvider(_fadeInOutSampleProviderSource);

                AddMixerInput(_fadeInOutSampleProvider, ostProviderId, AudioDomain.OST);
                _fadeInOutSampleProvider.BeginFadeIn(1);
                _trackHistory.Add(id);
            }), alreadyPlaying ? FadeTimeMiliseconds : -1);
        }
        public void SetPlayList(Playlist newPlaylist, bool fade = true)
        {
            STOLON.Debug.Log(">changing audio playlist.");
            _currentPlaylist = newPlaylist;
            _trackQueue = new Queue<string>(newPlaylist.Get());

            SetTrack(_trackQueue.Dequeue(), fade);
        }
        //public void ClearPlaylist(bool fade = false)
        //{
        //    trackQueue.Clear();
        //    currentPlaylist = null;
        
        //    if (fade)
        //    {


        //        TaskHeap.Heap.SafePush("playlistClear", new DynamicTask(() => // fire and forget game logic ftw
        //        {

        //        }), FadeTimeMiliseconds);
        //    }

        //}
        /// <summary>
        /// Update the <see cref="AudioEngine"/>.
        /// </summary>
        /// <param name="elapsedMiliseconds">yes.</param>
        public void Update(int elapsedMiliseconds)
        {
            if (_fadeInOutSampleProvider != null && _fadeInOutSampleProviderSource.Finished)
            {
                _fadeInOutSampleProvider = null;
                _fadeInOutSampleProviderSource = null;

                string? nextTrack = null;

                if (_trackQueue.Count > 0)
                {
                    nextTrack = _trackQueue.Dequeue();
                    SetTrack(nextTrack, false); // no fade nessesairy.
                    STOLON.Debug.Log("dequeued next track; " + nextTrack);
                }
                else if (_currentPlaylist != null && _currentPlaylist.Loop)
                {
                    STOLON.Debug.Log(">refreshing loopable playlist queue");
                    _trackQueue = new Queue<string>(_currentPlaylist.Get());
                    SetTrack(_trackQueue.Dequeue(), false); // no fade nessesairy.
                }
            }
        }

        public void Dispose()
        {
            STOLON.Debug.Log("disposing audio engine..");
            _outputDevice.Dispose();
        }
        /// <summary>
        /// All loaded sounds relevant for the stolon <see cref="GameEnvironment"/>
        /// </summary>
        public Dictionary<string, CachedAudio> Library { get; }
        public const int FadeTimeMiliseconds = 2000;
    }
    public class Playlist
    {
        private List<string> trackIds;
        public bool Scramble { get; set; }
        public bool Loop { get; set; }
        public ReadOnlyCollection<string> TrackIds { get; }

        public Playlist(params string[] trackIds) : this(trackIds, true, true) { }
        public Playlist(string[] trackIds, bool scramble, bool loop)
        {
            this.trackIds = trackIds.ToList();
            Scramble = scramble;
            TrackIds = new ReadOnlyCollection<string>(this.trackIds);
            Loop = loop;
        }
        public void AddTrack(string id) => trackIds.Add(id);
        public string[] Get() => Scramble ? GetScrambled() : trackIds.ToArray();
        public string[] GetScrambled()
        {
            Random rng = new Random();
            string[] toret = trackIds.ToArray();
            int n = toret.Length;

            while (n > 1)
            {
                int k = rng.Next(n--);
                string temp = toret[n];
                toret[n] = toret[k];
                toret[k] = temp;
            }

            return toret;
        }

        public static Playlist GetLooped(string trackId) 
            => new Playlist(trackId.ToSingleArray(), false, true);
        public static Playlist Merged(Playlist playlist1, Playlist playlist2, bool scramble, bool loop) 
            => new Playlist(playlist1.Get().Concat(playlist2.Get()).ToHashSet().ToArray(), scramble, loop);
    }
}

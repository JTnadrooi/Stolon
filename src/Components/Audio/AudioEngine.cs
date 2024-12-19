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
using static Stolon.StolonGame;
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
    /// The main <see cref="AudioEngine"/> for the <see cref="StolonGame"/> environment. <i>Moderately thread-safe.</i>
    /// </summary>
    public class AudioEngine : IDisposable
    {
        public Playlist? Current => currentPlaylist;

        private IWavePlayer outputDevice;

        #region mixers

        private MixingSampleProvider masterMixer;
        private VolumeSampleProvider masterVolumeSampleProvider;

        private DictionaryMixingSampleProvider fxMixer;
        private DictionaryMixingSampleProvider ostMixer;
        private VolumeSampleProvider fxVolumeSampleProvider;
        private VolumeSampleProvider ostVolumeSampleProvider;

        #endregion

        private FadeInOutSampleProvider? fadeInOutSampleProvider;
        private CachedAudioSampleProvider? fadeInOutSampleProviderSource;
        private List<string> trackHistory;
        private Queue<string> trackQueue;
        private Playlist? currentPlaylist;


        /// <summary>
        /// The mixer used to mix all the <see cref="AudioFileReader"/>
        /// </summary>
        public DictionaryMixingSampleProvider FXMixer => fxMixer;
        public DictionaryMixingSampleProvider OSTMixer => ostMixer;
        public MixingSampleProvider MasterMixer => masterMixer;
        /// <summary>
        /// The fx volume.
        /// </summary>
        public float FxVolume
        {
            get => fxVolumeSampleProvider.Volume;
            set => fxVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// The master volume.
        /// </summary>
        public float MasterVolume
        {
            get => masterVolumeSampleProvider.Volume;
            set => masterVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// The st volume.
        /// </summary>
        public float OstVolume
        {
            get => ostVolumeSampleProvider.Volume;
            set => ostVolumeSampleProvider.Volume = Math.Clamp(value, 0f, 1f);
        }
        /// <summary>
        /// Initialize a new <see cref="AudioEngine"/>.
        /// </summary>
        public AudioEngine()
        {
            outputDevice = new DirectSoundOut(40);
            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            ConfigParser parser = new ConfigParser(@"user.cfg");

            masterMixer = new MixingSampleProvider(waveFormat);
            masterMixer.ReadFully = true;
            masterVolumeSampleProvider = new VolumeSampleProvider(masterMixer);

            fxMixer = new DictionaryMixingSampleProvider(waveFormat);
            fxMixer.ReadFully = true;
            fxMixer.PauzeWhenInactive = true;
            fxVolumeSampleProvider = new VolumeSampleProvider(fxMixer);

            ostMixer = new DictionaryMixingSampleProvider(waveFormat);
            ostMixer.ReadFully = true;
            ostMixer.PauzeWhenInactive = true;
            ostVolumeSampleProvider = new VolumeSampleProvider(ostMixer);
            
            masterMixer.AddMixerInput(fxVolumeSampleProvider);
            masterMixer.AddMixerInput(ostVolumeSampleProvider);

            trackHistory = new List<string>();
            trackQueue = new Queue<string>();

            outputDevice.Init(masterVolumeSampleProvider);
            outputDevice.Play();

            FxVolume = (float)parser.GetValue("Audio", "fx_vol", 0.5f);
            OstVolume = (float)parser.GetValue("Audio", "ost_vol", 1f);
            MasterVolume = (float)parser.GetValue("Audio", "master_vol", 1f);
        }
        static AudioEngine()
        {
            AudioLibrary = new Dictionary<string, CachedAudio>();
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
        public DictionaryMixingSampleProvider GetMixer(AudioDomain domain) => domain == AudioDomain.SFX ? fxMixer : ostMixer;
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
        /// <param name="id">The ost id from the <see cref="AudioLibrary"/>.</param>
        public void SetTrack(string id, bool fade = true)
        {
            StolonGame.Instance.DebugStream.WriteLine("\ttrack changing to " + id);

            string ostProviderId = "__ostProvider";
            string ostTaskId = "ostChange";
            bool alreadyPlaying = fadeInOutSampleProvider != null;

            if (alreadyPlaying && fade) fadeInOutSampleProvider.BeginFadeOut(FadeTimeMiliseconds);
            TaskHeap.Heap.SafePush(ostTaskId, new DynamicTask(() => // fire and forget game logic ftw
            {
                TryRemoveMixerInput(ostProviderId, AudioDomain.OST);
                StolonGame.Instance.DebugStream.WriteLine("\ttrack changed to " + id);
                fadeInOutSampleProviderSource = AudioLibrary[id].GetAsSampleProvider();
                fadeInOutSampleProvider = new FadeInOutSampleProvider(fadeInOutSampleProviderSource);

                AddMixerInput(fadeInOutSampleProvider, ostProviderId, AudioDomain.OST);
                fadeInOutSampleProvider.BeginFadeIn(1);
                trackHistory.Add(id);
            }), alreadyPlaying ? FadeTimeMiliseconds : -1);
        }
        public void SetPlayList(Playlist newPlaylist, bool fade = true)
        {
            Instance.DebugStream.WriteLine("changing audio playlist..");
            currentPlaylist = newPlaylist;
            trackQueue = new Queue<string>(newPlaylist.Get());

            SetTrack(trackQueue.Dequeue(), fade);
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
            if (fadeInOutSampleProvider != null && fadeInOutSampleProviderSource.Finished)
            {
                fadeInOutSampleProvider = null;
                fadeInOutSampleProviderSource = null;

                string? nextTrack = null;

                if (trackQueue.Count > 0)
                {
                    nextTrack = trackQueue.Dequeue();
                    SetTrack(nextTrack, false); // no fade nessesairy.
                    Instance.DebugStream.WriteLine("dequeued next track; " + nextTrack);
                }
                else if (currentPlaylist != null && currentPlaylist.Loop)
                {
                    Instance.DebugStream.WriteLine("refreshing loopable playlist queue..");
                    trackQueue = new Queue<string>(currentPlaylist.Get());
                    SetTrack(trackQueue.Dequeue(), false); // no fade nessesairy.
                }
            }
        }

        public void Dispose()
        {
            Instance.DebugStream.WriteLine("disposing audio engine..");
            outputDevice.Dispose();
        }
        /// <summary>
        /// The main <see cref="AudioEngine"/> instance.
        /// </summary>
        public static AudioEngine Audio => StolonGame.Instance.AudioEngine;
        /// <summary>
        /// All loaded sounds relevant for the stolon <see cref="StolonEnvironment"/>
        /// </summary>
        public static Dictionary<string, CachedAudio> AudioLibrary { get; }
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

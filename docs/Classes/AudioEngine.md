# Audio Engine

The main logic behind the Audio systems is stored in the `AudioEngine` Instance found in the `AudioEngine.Audio` refrence.

## Uses

-   Fire and forget (cashed) audio files.
    -   Done by calling
-   Changing the background playlist.
    -   _Done by first contructing a new `Playlist` and using the `SetPlayList` method to set the new playlist._
-   Interupting the background playlist.
    -   _Done by simply calling the `SetTrack` method. After the track has finished the `Playlist` will continue._
-   Changing the volume of the FX, OST and MASTER mixers.
    -   _Done by setting the `FXVolume`, `OSTVolume` and `MasterVolume` respectivelly._
-   ...

## Logging

## Performance Cost

## Multi-Threading

## Primary Used Classes and Structs

-   The `Playlist` class holds an `array` of `string` music ids. When inserted in `GetPlaylist`, the `Playlist`
    objects `Get` method gets called. This method returns the array _or_ the scrambled version of that array if
    `Playlist.Scrambled` is `true`. The `AudioEngine` will loop the set `Playlist` if `Playlist.Loop` is set to `true`.
-   The `CachedAudio` `class` stored an read audiofile for use in game. Its faster to load aufiofiles into memory before use. (_Caching_)
-   The `CachedAudioSampleProvider` `class` makes it possible for `CachedAudio` objects to be read by other sub `ISampleProviders`.

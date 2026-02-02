namespace Common;

using Godot;
using System.Collections.Generic;
/// <summary>
/// Manages audio playback with two channels and volume control.
/// </summary>
public sealed class AudioManager(AudioStreamPlayer audioPlayerChannel1, AudioStreamPlayer audioPlayerChannel2, AudioStreamPlayer audioPlayerChannelMusic)
{
    private readonly AudioStreamPlayer _audioPlayerChannel1 = audioPlayerChannel1;
    private readonly AudioStreamPlayer _audioPlayerChannel2 = audioPlayerChannel2;
    private readonly AudioStreamPlayer _audioPlayerChannelMusic = audioPlayerChannelMusic;
    private Dictionary<string, AudioStream> _audioClips = [];
    private Dictionary<string, AudioStream> _musicClips = [];
    private float _channel1Volume = 1.0f;
    private float _channel2Volume = 1.0f;
    private float _channelMusicVolume = 1.0f;
    private bool _isChannel1Playing = false;
    private bool _isChannel1AllowedToPlay = true;
    private bool _isChannel2Playing = false;
    private bool _isChannel2AllowedToPlay = true;
    private bool _isChannelMusicPlaying = false;
    private bool _isChannelMusicAllowedToPlay = true;
    /// <summary>
    /// Adds an audio clip to the manager with the specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="clip"></param>
    public void AddAudioClip(string name, AudioStream clip)
    {
        if (!_audioClips.ContainsKey(name))
            _audioClips[name] = clip;
    }
    /// <summary>
    /// Adds a music track to the manager with the specified name. Make sure to check looping in the AudioStream resource if desired!
    /// </summary>
    /// <param name="name"></param>
    /// <param name="track"></param>
    public void AddMusicTrack(string name, AudioStream track)
    {
        if (!_musicClips.ContainsKey(name))
            _musicClips[name] = track;
    }
    /// <summary>
    /// Plays an audio clip by name on the specified channel.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="channel"></param>
    public void PlayAudioClip(string name, int channel = 1)
    {
        if (_audioClips.ContainsKey(name))
        {
            if (channel == 1 && _isChannel1AllowedToPlay)
            {
                _audioPlayerChannel1.Stream = _audioClips[name];
                _audioPlayerChannel1.VolumeDb = LinearToDb(_channel1Volume);
                _audioPlayerChannel1.Play();
                _isChannel1Playing = true;
            } else if (channel == 2 && _isChannel2AllowedToPlay)
            {
                _audioPlayerChannel2.Stream = _audioClips[name];
                _audioPlayerChannel2.VolumeDb = LinearToDb(_channel2Volume);
                _audioPlayerChannel2.Play();
                _isChannel2Playing = true;
            }
        }
    }
    /// <summary>
    /// Plays a music track by name on the music channel.
    /// </summary>
    /// <param name="name"></param>
    public void PlayMusicTrack(string name)
    {
        if (_musicClips.TryGetValue(name, out AudioStream value) && _isChannelMusicAllowedToPlay)
        {
            _audioPlayerChannelMusic.Stream = value;
            _audioPlayerChannelMusic.VolumeDb = LinearToDb(_channelMusicVolume);
            _audioPlayerChannelMusic.Play();
            _isChannelMusicPlaying = true;
        }
    }
    /// <summary>
    /// Sets the volume for the specified channel.
    /// </summary>
    /// <param name="channel">The channel number (1, 2, or 3 - for music).</param>
    /// <param name="volume">The volume level (0.0 to 1.0).</param>
    public void SetChannelVolume(int channel, float volume)
    {
        volume = Mathf.Clamp(volume, 0.0f, 1.0f);
        if (channel == 1)
        {
            _channel1Volume = volume;
            if (_isChannel1Playing)
                _audioPlayerChannel1.VolumeDb = LinearToDb(_channel1Volume);
        } else if (channel == 2)
        {
            _channel2Volume = volume;
            if (_isChannel2Playing)
                _audioPlayerChannel2.VolumeDb = LinearToDb(_channel2Volume);
        } else if (channel == 3)
        {
            _channelMusicVolume = volume;
            if (_isChannelMusicPlaying)
                _audioPlayerChannelMusic.VolumeDb = LinearToDb(_channelMusicVolume);
        }
    }
    /// <summary>
    /// Stops playback on the specified channel. 1 , 2, or 3 (music).
    /// </summary>
    /// <param name="channel"></param>
    public void StopChannel(int channel)
    {
        if (channel == 1 && _isChannel1Playing)
        {
            _audioPlayerChannel1.Stop();
            _isChannel1Playing = false;
        } else if (channel == 2 && _isChannel2Playing)
        {
            _audioPlayerChannel2.Stop();
            _isChannel2Playing = false;
        } else if (channel == 3 && _isChannelMusicPlaying)
        {
            _audioPlayerChannelMusic.Stop();
            _isChannelMusicPlaying = false;
        }
    }
    /// <summary>
    /// Converts a linear volume value (0.0 to 1.0) to decibels.
    /// </summary>
    /// <param name="linear"></param>
    /// <returns>float</returns>
    private static float LinearToDb(float linear) => linear <= 0.0f ? -80.0f : 20.0f * Mathf.Log(linear);
}
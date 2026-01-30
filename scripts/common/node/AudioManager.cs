namespace Common;

using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// Manages audio playback with two channels and volume control.
/// </summary>
public sealed partial class AudioManager : Node
{
    private static AudioManager _instance;
    private AudioStreamPlayer _audioPlayerChannel1;
    private AudioStreamPlayer _audioPlayerChannel2;
    private Dictionary<string, AudioStream> _audioClips;
    private float _channel1Volume = 1.0f;
    private float _channel2Volume = 1.0f;
    private bool _isChannel1Playing = false;
    private bool _isChannel2Playing = false;
    public override void _Ready()
    {
        if (_instance != null)
            throw new Exception("AudioManager instance already exists.");
        _instance = this;
        _audioPlayerChannel1 = new AudioStreamPlayer();
        _audioPlayerChannel2 = new AudioStreamPlayer();
        AddChild(_audioPlayerChannel1);
        AddChild(_audioPlayerChannel2);
        _audioClips = [];
    }
    /// <summary>
    /// Adds an audio clip to the manager with the specified name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="clip"></param>
    public void AddAudioClip(string name, AudioStream clip)
    {
        if (!_audioClips.ContainsKey(name))
        {
            _audioClips[name] = clip;
        }
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
            if (channel == 1)
            {
                _audioPlayerChannel1.Stream = _audioClips[name];
                _audioPlayerChannel1.VolumeDb = LinearToDb(_channel1Volume);
                _audioPlayerChannel1.Play();
                _isChannel1Playing = true;
            }
            else if (channel == 2)
            {
                _audioPlayerChannel2.Stream = _audioClips[name];
                _audioPlayerChannel2.VolumeDb = LinearToDb(_channel2Volume);
                _audioPlayerChannel2.Play();
                _isChannel2Playing = true;
            }
        }
    }
    /// <summary>
    /// Sets the volume for the specified channel.
    /// </summary>
    /// <param name="channel">The channel number (1 or 2).</param>
    /// <param name="volume">The volume level (0.0 to 1.0).</param>
    public void SetChannelVolume(int channel, float volume)
    {
        volume = Mathf.Clamp(volume, 0.0f, 1.0f);
        if (channel == 1)
        {
            _channel1Volume = volume;
            if (_isChannel1Playing)
                _audioPlayerChannel1.VolumeDb = LinearToDb(_channel1Volume);
        }
        else if (channel == 2)
        {
            _channel2Volume = volume;
            if (_isChannel2Playing)
                _audioPlayerChannel2.VolumeDb = LinearToDb(_channel2Volume);
        }
    }
    /// <summary>
    /// Converts a linear volume value (0.0 to 1.0) to decibels.
    /// </summary>
    /// <param name="linear"></param>
    /// <returns></returns>
    private static float LinearToDb(float linear)
    {
        return linear <= 0.0f ? -80.0f : 20.0f * Mathf.Log(linear);
    }
}
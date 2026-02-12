namespace Common;

using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// Manages saving and loading of settings and game configuration within a CONFIG files (godot cfg) in the user directory. We're running system dicts due to gd dict not supporting certain features.
/// </summary>
public sealed class SettingsManager
{
    public event Action<(Sectional , Dictionary<string, (float, bool)>)> OnSettingsUpdated;
    public string CurrentUsername { get; private set; }
    public (Sectional , Dictionary<string, (float, bool)>) AudioSettings { get; private set;}
    = (Sectional.Audio ,new()
    {
        { "Channel1", (0.9f, true)},
        { "Channel2", (0.7f, true)},
        { "ChannelMusic", (1.0f, true)},
    });
    private ConfigFile _configFile;
    private readonly string _configPath = "user://config/user.config";
    public SettingsManager()
    {
        ConfigFile config = new();
        if (config.Load(_configPath) == Error.Ok)
            _configFile = config;
    }
    public void SaveData(Sectional section, Dictionary<string, (float, bool)> data, ConfigFile config)
    {
        foreach (var (setting, (value,allowed)) in data)
        {
            config.SetValue(section.ToString(), setting, value);
            config.SetValue(section.ToString(), $"{setting}_allowed?", allowed);
        }
        switch (section)
        {
            case Sectional.Audio: AudioSettings = (section, data); break;
        }
        Save();
    }
    public void SaveUsername(string input)
    {
        _configFile.SetValue(Sectional.User.ToString(),"Username", input);
        CurrentUsername = input;
        Save();
    }
    private void Save() => _configFile.Save(_configPath);
}

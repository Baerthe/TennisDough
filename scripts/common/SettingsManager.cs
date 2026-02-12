namespace Common;

using Godot;
using System.Collections.Generic;
/// <summary>
/// Manages saving and loading of settings and game configuration within a CONFIG files (godot cfg) in the user directory.
/// </summary>
public sealed class SettingsManager
{
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
    public void SaveData(Sectional section, Dictionary<string, (float, bool)> Data, ConfigFile config)
    {
        foreach (var (setting, (value,allowed)) in Data)
        {
            config.SetValue(section.ToString(), setting, value);
            config.SetValue(section.ToString(), $"{setting}_allowed?", allowed);
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

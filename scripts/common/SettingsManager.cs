namespace Common;

using Godot;
using System.Collections.Generic;
/// <summary>
/// Manages saving and loading of settings and game configuration within a CONFIG files (godot cfg) in the user directory.
/// </summary>
public sealed class SettingsManager
{
    public (Sectional , Dictionary<string, (float, bool)>) AudioSettings { get; private set;}
    = (Sectional.Audio ,new()
    {
        { "Channel1", (0.9f, true)},
        { "Channel2", (0.7f, true)},
        { "ChannelMusic", (1.0f, true)},
    });
    private readonly string _savePath = "user://config/";
    private readonly string _saveFileName = "user.config";
    public SettingsManager()
    {
        //TODO: Load up some settin'
    }
    //TODO: Saving/Loading settings, loop into mainmenu.
    private void SaveData(Sectional section, Dictionary<string, (float, bool)> Data, ConfigFile config, string fullPath)
    {
        foreach (var (setting, (value,allowed)) in Data)
        {
            config.SetValue(section.ToString(), setting, value);
            config.SetValue(section.ToString(), $"{setting}_allowed?", allowed);
        }
        config.Save(fullPath + _saveFileName);
    }
}

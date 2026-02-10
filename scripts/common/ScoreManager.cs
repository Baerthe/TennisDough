namespace Common;

using Godot;
using Godot.Collections;
/// <summary>
/// Manages loading and saving scores for game packs. Scores are stored in JSON files in the user directory, keyed by pack name. Each score entry is a key-value pair of scorer name and score value.
/// </summary>
public sealed class ScoreManager
{
    private readonly string _savePath = "user://saves/";
    private readonly string _saveFileName = "high.scores";
    private readonly string _section = "High Scores";
    private readonly Dictionary<string, uint> _defaultScores = new()
    {
        { "Tim" , 1100},
        { "Kim" , 1000},
        { "Jim" , 900},
        { "Zim" , 800},
        { "Qim" , 700}
    };
    /// <summary>
    /// Loads scores for the given game pack from a SCORES file. If no file exists, creates a new one with default scores.
    /// <returns>Dictionary of scores keyed by scorer name</returns>
    public Dictionary<string, uint> LoadScores(GamePack pack)
    {
        string fullPath = $"{_savePath}{pack.GameName}";
        Dictionary<string, uint> save = [];
        ConfigFile config = new();
        if (config.Load(fullPath + _saveFileName) == Error.Ok)
        {
            foreach (string player in config.GetSectionKeys(_section))
            {
                var playerScore = (uint)config.GetValue(_section, player);
                save.Add(player, playerScore);
            }
            return save;
        } else
        {
            GD.Print($"ScoreManager: Score table for {pack.GameName} could not be loaded. Creating...");
            SaveData(_defaultScores, config, fullPath);
            return _defaultScores;
        }
    }
    /// <summary>
    /// Saves scores for the given game pack to a SCORES file. If no file exists, creates a new one. If a file exists, it is overwritten with the new scores.
    /// </summary>
    public void SaveScores(Dictionary<string, uint> scores, GamePack pack)
    {
        string fullPath = $"{_savePath}{pack.GameName}";
        ConfigFile config = new();
        if (config.Load(fullPath + _saveFileName) == Error.Ok)
            SaveData(scores, config, fullPath);
        else
        {
            GD.Print($"ScoreManager: Score table for {pack.GameName} could not be saved! Creating...");
            SaveData(_defaultScores, config, fullPath);
        }
    }
    /// <summary>
    /// Helper method to save score data to a file.
    /// </summary>
    private void SaveData(Dictionary<string, uint> scores, ConfigFile config, string fullPath)
    {
        foreach (var (player, score) in scores)
            config.SetValue(_section, player, score);
        config.Save(fullPath + _saveFileName);
    }
}
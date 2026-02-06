namespace Common;

using Godot;
using Godot.Collections;
using System.Linq;
/// <summary>
/// Manages loading and saving scores for game packs. Scores are stored in JSON files in the user directory, keyed by pack name. Each score entry is a key-value pair of scorer name and score value.
/// </summary>
public sealed class ScoreManager
{
    /// <summary>
    /// Loads scores for the given game pack from a JSON file. If no file exists, creates a new one with empty scores.
    /// </summary> <param name="pack"></param>
    /// <returns>Dictionary of scores keyed by scorer name</returns>
    public static Dictionary<string, uint> LoadScores(GamePack pack)
    {
        string fullPath = $"user://saves/{pack.GameName}_scores.json";
        var file = FileAccess.FileExists(fullPath);
        if (!file)
        {
            GD.Print($"ScoreManager: No existing score file found for pack {pack.GameName}. Creating and Returning empty scores.");
            using (var saves = FileAccess.Open(fullPath, FileAccess.ModeFlags.Write))
            {
                saves.StoreString(Json.Stringify(new Dictionary<string, uint>()));
            }
            return [];
        }
        using var scoreFile = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read);
        var jsonString = scoreFile.GetAsText();
        var json = new Json();
        var parseResult = json.Parse(jsonString);
        if (parseResult != Error.Ok)
        {
            GD.PushError($"ScoreManager: Failed to parse score file for pack {pack.GameName} at path {fullPath}. Error: {parseResult}");
            return [];
        }
        var scores = json.Data.AsStringArray().ToDictionary(
            pair => pair.Split(":")[0],
            pair => uint.Parse(pair.Split(":")[1]));
        GD.Print($"ScoreManager: Loaded scores for pack {pack.GameName}: {string.Join(", ", scores.Select(kv => $"{kv.Key}:{kv.Value}"))}");
        var godotScores = new Dictionary<string, uint>();
        foreach (var kvp in scores)
        {
            godotScores[kvp.Key] = kvp.Value;
        }
        return godotScores;
    }
    public static void SaveScores(Dictionary<string, uint> scores, GamePack pack)
    {
        
    }
}
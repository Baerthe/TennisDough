namespace Common;

using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// Manages loading and accessing game packs.
/// </summary>
public sealed class PackManager
{
    public event Action<Node> OnPackLoaded;
    public Dictionary<string, GamePack> GamePacks { get; private set; }
    public GamePack CurrentPack { get; private set; }
    public PackManager()
    {
        GD.Print("PackManager: Initializing PackManager and loading game packs.");
        GamePacks = LoadGamePacks();
    }
    /// <summary>
    /// Loads the specified game pack as the current pack.
    /// </summary>
    /// <param name="pack"></param>
    public void LoadIntoPack(GamePack pack)
    {
        GD.Print($"PackManager: Starting game with pack: {pack.GameName}");
        CurrentPack = pack;
        Node LoadedScene = pack.GameScene.Instantiate();
        OnPackLoaded?.Invoke(LoadedScene);
    }
    /// <summary>
    /// Loads all game packs from the designated resource directory.
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, GamePack> LoadGamePacks()
    {
        string hardPath = "res://assets/resources/packs";
        var dir = DirAccess.Open(hardPath);
        var packs = new Dictionary<string, GamePack>();
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (fileName.EndsWith(".tres"))
                {
                    var packPath = $"{hardPath}/{fileName}";
                    GD.Print($"PackManager: Found pack file at: {packPath}");
                    GamePack pack = GD.Load<GamePack>(packPath);
                    packs.Add(pack.GameName, pack);
                    GD.Print($"PackManager: checked pack: {pack.GameName} from {packPath}");
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        } else
            GD.PrintErr("PackManager: Failed to open packs directory.");
        return packs;
    }
}
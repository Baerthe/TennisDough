namespace Common;

using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// Manages loading and accessing game packs.
/// </summary>
public sealed class PackManager
{
    public event Action<GamePack> OnPackLoaded;
    public GamePack[] GamePacks { get; private set; }
    public GamePack CurrentPack { get; private set; }
    public PackManager()
    {
        GamePacks = LoadGamePacks();
    }
    /// <summary>
    /// Loads the specified game pack as the current pack.
    /// </summary>
    /// <param name="pack"></param>
    public void LoadIntoPack(GamePack pack)
    {
        GD.Print($"PackManager: Starting game with pack: {pack}");
        CurrentPack = pack;
        OnPackLoaded?.Invoke(pack);
    }
    /// <summary>
    /// Loads all game packs from the designated resource directory.
    /// </summary>
    /// <returns></returns>
    private static GamePack[] LoadGamePacks()
    {
        string hardPath = "res://resources/packs";
        var dir = DirAccess.Open(hardPath);
        var packs = new List<GamePack>();
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (fileName.EndsWith(".tres"))
                {
                    var packPath = $"{hardPath}/{fileName}";
                    GamePack pack = GD.Load<GamePack>(packPath);
                    packs.Add(pack);
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
        return [.. packs];
    }
}
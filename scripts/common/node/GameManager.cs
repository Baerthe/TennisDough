namespace Common;

using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// The core game manager responsible for handling game state and transitions.
/// Normally we would want some sort of state orchestratior/scene loader, but we will be handling it inline for simplicity.
/// </summary>
public sealed partial class GameManager : Node
{
    public static GameMonitor Monitor { get; private set; } = new GameMonitor();
    public GamePack[] GamePacks { get; private set; }
    [ExportCategory("Scenes")]
    [Export] private PackedScene _mainMenuScene;
    private MainMenu _mainMenu;
    private PauseWatcher _pauseWatcher;
    public override void _Ready()
    {
        GamePacks = LoadGamePacks();
        _mainMenu = this.InstanceScene(_mainMenuScene) as MainMenu;
        _mainMenu.OnStartGame += HandleStartGame;
        _pauseWatcher = this.AddNode<PauseWatcher>();
        _pauseWatcher.OnTogglePause += HandleTogglePause;
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
    /// <summary>
    /// Handles starting a new game based on the selected game type.
    /// </summary>
    private void HandleStartGame(GamePack selection)
    {
        GD.Print($"Starting game: {selection}");
        // TODO: Handle game start logic based on selection
    }
    /// <summary>
    /// Handles toggling the pause state of the game.
    /// </summary>
    private void HandleTogglePause()
    {
        if (Monitor.CurrentState == GameState.MainMenu)
            return;
        Monitor.ChangeState(GameState.Paused);
        GD.Print($"Game paused: {Monitor.CurrentState == GameState.Paused}");
    }
}
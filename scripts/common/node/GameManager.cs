namespace Common;

using Godot;
using System;
/// <summary>
/// The core game manager responsible for handling game state and transitions.
/// Normally we would want some sort of state orchestratior/scene loader, but we will be handling it inline for simplicity.
/// </summary>
public sealed partial class GameManager : Node
{
    public static GameMonitor Monitor { get; private set; } = new GameMonitor();
    [ExportCategory("Scenes")]
    [Export] private PackedScene _mainMenuScene;
    [Export] private PackedScene _blockGameScene;
    [Export] private PackedScene _tennisGameScene;
    private MainMenu _mainMenu;
    private PauseWatcher _pauseWatcher;
    public override void _Ready()
    {
        _mainMenu = this.InstantScene(_mainMenuScene) as MainMenu;
        _mainMenu.OnStartGame += HandleStartGame;
        _pauseWatcher = this.AddNode<PauseWatcher>();
        _pauseWatcher.OnTogglePause += HandleTogglePause;
    }
    /// <summary>
    /// Handles starting a new game based on the selected game type.
    /// </summary>
    /// <param name="selection"></param>
    private void HandleStartGame(GameSelection selection)
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
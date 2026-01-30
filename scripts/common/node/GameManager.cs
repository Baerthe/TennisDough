namespace Common;

using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

/// <summary>
/// The core game manager responsible for handling game state and transitions. Global Root Node.
/// Normally we would want some sort of state orchestratior/scene loader, but we will be handling it inline for simplicity.
/// </summary>
public sealed partial class GameManager : Node
{
    public static AudioManager Audio { get; private set; }
    public static GameMonitor Monitor { get; private set; } = new GameMonitor();
    [ExportCategory("Scenes")]
    [Export] private PackedScene _mainMenuScene;
    [Export] private PackedScene _loadingScene;
    private Node _LoadedPackedScene;
    private MainMenu _mainMenu;
    private static readonly PackManager PackManager = new();
    private static PauseWatcher _pauseWatcher;
    // -> Godot Overrides
    public override void _EnterTree()
    {
        GD.Print("GameManager: EnterTree");
        // Add our sub-systems
        Audio = this.AddNode<AudioManager>();
        _pauseWatcher = this.AddNode<PauseWatcher>();
        _mainMenu = this.InstanceScene(_mainMenuScene) as MainMenu;
    }
    public override void _Ready()
    {
        // Hook up events
        Monitor.OnGameStateChanged += HandleGameStateRequest;
        PackManager.OnPackLoaded += HandlePackLoaded;
        _mainMenu.OnStartGame += HandleStartGame;
        _pauseWatcher.OnTogglePause += HandleTogglePause;
    }
    // -> Event Handlers
    /// <summary>
    /// Handles game state change requests from the GameMonitor.
    /// </summary>
    /// <param name="newState"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void HandleGameStateRequest(GameState newState)
    {
        GD.Print($"GameManager: Game state changed to: {newState}");
        switch (newState)
        {
            case GameState.MainMenu:
                // Handle main menu state
                _LoadedPackedScene?.QueueFree();
                _LoadedPackedScene = null;
                break;
            case GameState.GameMenu:
                // Handle game menu state
                break;
            case GameState.InGame:
                // Handle in-game state
                break;
            case GameState.Paused:
                HandleTogglePause();
                break;
            case GameState.GameOver:
                // Handle game over state
                break;
            case GameState.Loading:
                // Handle loading state
                // TODO: _loadingScene.Visible = true;
                Monitor.ChangeState(GameState.InGame);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
    /// <summary>
    /// Handles actions to take when a game pack is loaded.
    /// </summary>
    /// <param name="pack"></param>
    private void HandlePackLoaded(GamePack pack)
    {
        GD.Print($"GameManager: Pack loaded: {pack.GameName}");
        _LoadedPackedScene = this.InstanceScene(pack.GameScene);
    }
    /// <summary>
    /// Handles starting a new game based on the selected game type.
    /// </summary>
    private void HandleStartGame(GamePack selection)
    {
        Monitor.ChangeState(GameState.Loading);
        PackManager.LoadIntoPack(selection);
    }
    /// <summary>
    /// Handles toggling the pause state of the game.
    /// </summary>
    private void HandleTogglePause()
    {
        if (Monitor.CurrentState == GameState.MainMenu)
            return;
        Monitor.ChangeState(GameState.Paused);
        GD.Print($"GameManager: Game paused -> {Monitor.CurrentState == GameState.Paused}");
    }
}
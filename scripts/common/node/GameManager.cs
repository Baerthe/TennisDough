namespace Common;

using Godot;
using System;
using Godot.Collections;
/// <summary>
/// The core game manager responsible for handling game state and transitions. Global Root Node.
/// Normally we would want some sort of state orchestratior/scene loader, but we will be handling it inline for simplicity.
/// </summary>
public sealed partial class GameManager : Control
{
    public static AudioManager Audio { get; private set; }
    public static GameMonitor Monitor { get; private set; }
    public static PackManager PackManager { get; private set; }
    public Dictionary<string, uint> CurrentScores { get; private set;}
    [ExportCategory("References")]
    [ExportGroup("Nodes")]
    [Export] private MainMenu _mainMenu;
    [Export] private Control _loadingScreen;
    [Export] private Control _gameScreen;
    [Export] private Control _crtOverlay;
    [ExportGroup("Shaders")]
    [Export] private ShaderMaterial _defaultCrtMaterial;
    [Export] private ShaderMaterial _pausedCrtMaterial;
    [Export] private ShaderMaterial _bootCrtMaterial;
    // *-> Fields
    private Node2D _LoadedPackedScene;
    private static PauseWatcher _pauseWatcher;
    private static ScoreManager _scoreManager;
    // *-> Godot Overrides
    public override void _EnterTree()
    {
        GD.Print("GameManager: EnterTree");
       // Add our sub-nodes
        Audio = new AudioManager(
            this.AddNode<AudioStreamPlayer>("AudioChannel1"),
            this.AddNode<AudioStreamPlayer>("AudioChannel2"),
            this.AddNode<AudioStreamPlayer>("AudioChannelMusic"));
        Monitor = new GameMonitor();
        PackManager = new PackManager();
        _scoreManager = new ScoreManager();
        _pauseWatcher = this.AddNode<PauseWatcher>();
//        _loadingScreen.Visible = false;
    }
    public override void _Ready()
    {
        // Hook up events
        Monitor.OnGameStateChanged += HandleGameStateRequest;
        PackManager.OnPackLoaded += HandlePackLoaded;
        _mainMenu.OnBootSequence += HandleBootSequence;
        _mainMenu.OnStartGame += PackManager.LoadIntoPack;
        _pauseWatcher.OnTogglePause += HandleTogglePause;
        // ! DEBUG
        PackManager.LoadIntoPack(PackManager.GamePacks["Block Game"]);
    }
    // *-> Event Handlers
    private void HandleBootSequence()
    {
        GD.Print("GameManager: Boot sequence started.");
        Monitor.ChangeState(GameState.MainMenu);
        _crtOverlay.Material = _bootCrtMaterial;
    }
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
                GD.Print("GameManager: Switching to Main Menu.");
                _mainMenu.Visible = true;
                _LoadedPackedScene?.QueueFree();
                _loadingScreen.Visible = false;
                break;
            case GameState.GameMenu:
                GD.Print("GameManager: Switching to Game Menu.");
                break;
            case GameState.InGame:
                GD.Print("GameManager: Switching to In-Game.");
                _mainMenu.Visible = false;
                _crtOverlay.Material = _defaultCrtMaterial;
//                _loadingScreen.Visible = false;
                break;
            case GameState.Paused:
                GD.Print("GameManager: Switching to Game Paused.");
                _crtOverlay.Material = _pausedCrtMaterial;
                break;
            case GameState.GameOver:
                GD.Print("GameManager: Switching to Game Over.");
                break;
            case GameState.Loading:
                GD.Print("GameManager: Switching to Loading Screen.");
//                _loadingScreen.Visible = true;
                // ! DEBUG
                Monitor.ChangeState(GameState.InGame);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
    /// <summary>
    /// Handles actions to take when a game pack is loaded.
    /// </summary>
    /// <param name="scene"></param>
    private void HandlePackLoaded(Node scene)
    {
        _LoadedPackedScene?.QueueFree();
        _LoadedPackedScene = scene as Node2D;
        _gameScreen.AddChild(_LoadedPackedScene);
        _LoadedPackedScene.Scale = new Vector2(1.78f, 1.78f);
        CurrentScores = _scoreManager.LoadScores(_LoadedPackedScene);
        Monitor.ChangeState(GameState.Loading);
        GD.Print("GameManager: Pack loaded and scene instantiated.");
    }
    /// <summary>
    /// Handles toggling the pause state of the game.
    /// </summary>
    private void HandleTogglePause()
    {
        if (Monitor.CurrentState == GameState.MainMenu)
            return;
        if (Monitor.CurrentState == GameState.Paused)
            Monitor.ChangeState(Monitor.PriorState);
        else
            Monitor.ChangeState(GameState.Paused);
        GD.Print($"GameManager: Game paused -> {Monitor.CurrentState == GameState.Paused}");
    }
}
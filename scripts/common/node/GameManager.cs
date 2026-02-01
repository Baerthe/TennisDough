namespace Common;

using Godot;
using System;
/// <summary>
/// The core game manager responsible for handling game state and transitions. Global Root Node.
/// Normally we would want some sort of state orchestratior/scene loader, but we will be handling it inline for simplicity.
/// </summary>
public sealed partial class GameManager : Control
{
    public static AudioManager Audio { get; private set; }
    public static GameMonitor Monitor { get; private set; }
    [ExportCategory("Scenes")]
    [Export] private MainMenu _mainMenu;
    [Export] private Control _loadingScreen;
    [Export] private Control _gameScreen;
    private Node2D _LoadedPackedScene;
    private static PackManager PackManager;
    private static PauseWatcher _pauseWatcher;
    // -> Godot Overrides
    public override void _EnterTree()
    {
        GD.Print("GameManager: EnterTree");
        PackManager = new PackManager();
        // Add our sub-nodes
        Audio = new AudioManager(
            this.AddNode<AudioStreamPlayer>("AudioChannel1"),
            this.AddNode<AudioStreamPlayer>("AudioChannel2"));
        Monitor = new GameMonitor();
        _pauseWatcher = this.AddNode<PauseWatcher>();
//        _loadingScreen.Visible = false;
    }
    public override void _Ready()
    {
        // Hook up events
        Monitor.OnGameStateChanged += HandleGameStateRequest;
        PackManager.OnPackLoaded += HandlePackLoaded;
//        _mainMenu.OnStartGame += HandleStartGame;
        _pauseWatcher.OnTogglePause += HandleTogglePause;
        // ! Debug
        HandleStartGame(PackManager.GamePacks[0]);
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
                _LoadedPackedScene?.QueueFree();
                _LoadedPackedScene = null;
                _loadingScreen.Visible = false;
                break;
            case GameState.GameMenu:
                break;
            case GameState.InGame:
//                _loadingScreen.Visible = false;
                break;
            case GameState.Paused:
                HandleTogglePause();
                break;
            case GameState.GameOver:
                break;
            case GameState.Loading:
//                _loadingScreen.Visible = true;
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
        _LoadedPackedScene =  pack.GameScene.Instantiate() as Node2D;
        _gameScreen.AddChild(_LoadedPackedScene);
        _LoadedPackedScene.Scale = new Vector2(1.78f, 1.78f);
    }
    /// <summary>
    /// Handles starting a new game based on the selected game type.
    /// </summary>
    private static void HandleStartGame(GamePack selection)
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
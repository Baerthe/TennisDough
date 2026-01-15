namespace BlockGame;

using Common;
using Godot;
using System;
using System.Reflection.Metadata;

/// <summary>
/// Main game controller for BlockGame. BlockGame is a breakout-style game, so we will have MainBlock being the controller and orchestrator of the game.
/// </summary>
public sealed partial class MainBlock : Node2D
{
    [ExportGroup("References")]
    [Export] public BallBlock Ball { get; private set; }
    [Export] public BlockCollection BlockCollection { get; private set; }
    [Export] public Timer GameTimer {get; private set; }
    [Export] public LevelData TestLevel { get; private set; }
    // [Export] public Paddle PaddleP1 { get; private set; }
    [ExportGroup("Sounds")]
    [Export] public AudioStream AudioBlockHit { get; private set; }
    [Export] public AudioStream AudioBlockDestroy { get; private set; }
    [Export] public AudioStream AudioOutOfBounds { get; private set; }
    [ExportGroup("Rects")]
    [Export] public ColorRect CrossRect { get; private set; }
    [Export] public ColorRect LeftWallRect { get; private set; }
    [Export] public ColorRect RightWallRect { get; private set; }
    [ExportGroup("HUD Properties")]
    [Export] public Label ScoreLabel { get; private set; }
    [Export] public Label TimerLabel { get; private set; }
    [Export] public Label MiddleScreenLabel { get; private set; }
    // -> Switches
    private bool _isGameOver = false;
    private bool _isPaused = false;
    private bool _isRainbowEffectActive = false;
    // -> Components
    private AudioManager _audioManager;
    private PauseWatcher _pauseWatcher;
    private Score _score;
    // -> Fields
    private int _timeInSeconds = 0;
    private int _maxTimeInSeconds = 9999;
    private byte _maxScore = 255;
    // -> Godot Overrides
    public override void _EnterTree()
    {
        _audioManager = this.AddNode<AudioManager>();
        _pauseWatcher = this.AddNode<PauseWatcher>();
        //_score = new Score(ScoreLabel);
    }
    public override void _Ready()
    {
        Ball.Inject(_audioManager);
        _audioManager.AddAudioClip("block_hit", AudioBlockHit);
        _audioManager.AddAudioClip("block_destroy", AudioBlockDestroy);
        _audioManager.AddAudioClip("out_of_bounds", AudioOutOfBounds);
        Ball.OnBlockHit += HandleBlockHit;
        Ball.OnOutOfBounds += HandleBallOutOfBounds;
        BlockCollection.GenerateLevel(TestLevel);
    }
    // -> Game State Functions
    /// <summary>
    /// Pauses or unpauses the current game.
    /// </summary>
    private void GamePause()
    {
        if (_isGameOver)
            return;
        if (_isPaused)
        {
            if (GameTimer.IsStopped())
                GameTimer.Start();
            GameTimer.Paused = false;
            Ball.ToggleEnable();
           // Menu.Visible = false;
            _isPaused = false;
        }
        else
        {
            GameTimer.Paused = true;
            Ball.ToggleEnable();
           // Menu.Visible = true;
            _isPaused = true;
        }
    }
    // -> Event Handlers
    private void HandleBlockHit(Block block)
    {
        _audioManager.PlayAudioClip("block_hit");
        block.OnBlockHit();
    }
    private void HandleBallOutOfBounds() => _audioManager.PlayAudioClip("out_of_bounds");
}
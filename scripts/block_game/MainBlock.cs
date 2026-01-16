namespace BlockGame;

using Common;
using Godot;
/// <summary>
/// Main game controller for BlockGame. BlockGame is a breakout-style game, so we will have MainBlock being the controller and orchestrator of the game.
/// </summary>
public sealed partial class MainBlock : Node2D
{
    [ExportGroup("References")]
    [Export] private MenuBlock _menu;
    [Export] private BallBlock _ball;
    [Export] private BlockCollection _blockCollection;
    [Export] private Timer _gameTimer;
    [Export] private LevelData _testLevel;
    [Export] public PaddleBlock _paddle;
    [ExportGroup("Sounds")]
    [Export] private AudioStream _audioBlockHit;
    [Export] private AudioStream _audioBlockDestroy;
    [Export] private AudioStream _audioOutOfBounds;
    [Export] private AudioStream _sfxButtonPress;
    [Export] private AudioStream _sfxMenuOpen;
    [Export] private AudioStream _sfxMenuClose;
    [Export] private AudioStream _sfxGameOver;
    [ExportGroup("Rects")]
    [Export] private ColorRect _crossRect;
    [Export] private ColorRect _leftWallRect;
    [Export] private ColorRect _rightWallRect;
    [ExportGroup("HUD Properties")]
    [Export] private Label _scoreLabel;
    [Export] private Label _timerLabel;
    [Export] private Label _middleScreenLabel;
    // *-> Switches
    private bool _isGameOver = false;
    private bool _isPaused = false;
    private bool _isRainbowEffectActive = false;
    // *-> Components
    private AudioManager _audioManager;
    private PauseWatcher _pauseWatcher;
    private Score _score;
    // *-> Fields
    private int _timeInSeconds = 0;
    private int _maxTimeInSeconds = 9999;
    private byte _maxScore = 255;
    // *-> Godot Overrides
    public override void _EnterTree()
    {
        _audioManager = this.AddNode<AudioManager>();
        _pauseWatcher = this.AddNode<PauseWatcher>();
    }
    public override void _Ready()
    {
        _score = new Score(_scoreLabel);
        _ball.Inject(_audioManager);
        // Setup AudioManager
        _audioManager.AddAudioClip("block_hit", _audioBlockHit);
        _audioManager.AddAudioClip("block_destroy", _audioBlockDestroy);
        _audioManager.AddAudioClip("out_of_bounds", _audioOutOfBounds);
        _audioManager.AddAudioClip("button_press", _sfxButtonPress);
        _audioManager.AddAudioClip("menu_open", _sfxMenuOpen);
        _audioManager.AddAudioClip("menu_close", _sfxMenuClose);
        _audioManager.AddAudioClip("game_over", _sfxGameOver);
        // Connect Events
        _ball.OnBlockHit += HandleBlockHit;
        _ball.OnOutOfBounds += HandleBallOutOfBounds;
        _gameTimer.Timeout += HandleTimerUpdate;
        _pauseWatcher.OnTogglePause += GamePause;
        // ! Debug init
        _blockCollection.GenerateLevel();
        // ! End Debug init
    }
    // *-> Game State Functions
    /// <summary>
    /// Pauses or unpauses the current game.
    /// </summary>
    private void GamePause()
    {
        if (_isGameOver)
            return;
        if (_isPaused)
        {
            if (_gameTimer.IsStopped())
                _gameTimer.Start();
            _gameTimer.Paused = false;
            _ball.ToggleEnable();
            _menu.Visible = false;
            _isPaused = false;
        }
        else
        {
            _gameTimer.Paused = true;
            _ball.ToggleEnable();
            _menu.Visible = true;
            _isPaused = true;
        }
    }
    // *-> Event Handlers
    /// <summary>
    /// Handles the block being hit by the ball.
    /// </summary>
    /// <param name="block"></param>
    private void HandleBlockHit(Block block)
    {
        _audioManager.PlayAudioClip("block_hit");
        block.OnBlockHit();
        _score.AddPoint();
    }
    /// <summary>
    /// Handles the ball going out of bounds.
    /// </summary>
    private void HandleBallOutOfBounds() => _audioManager.PlayAudioClip("out_of_bounds");
    /// <summary>
    /// Updates the game timer each second. Calls GameOver if the max time (or score) is reached.
    /// </summary>
    private async void HandleTimerUpdate()
    {
        if (_timeInSeconds < _maxTimeInSeconds)
        {
            _timeInSeconds++;
            int time = _maxTimeInSeconds - _timeInSeconds;
            _timerLabel.Text = time.ToString("D4");
        } //else
            //GameOver();
    }
}
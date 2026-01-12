namespace TennisGame;

using Common;
using Godot;
using System;
/// <summary>
/// Main game controller for tennis_game. tennis_game is a pretty simple game, so we will have Main being the controller and orchestrator of the game.
/// It will manage the paddles and the ball, and handle the game logic.
/// </summary>
public partial class Main : Node2D
{
    [ExportGroup("References")]
    [Export] public Menu Menu { get; private set; }
    [Export] public Timer GameTimer {get; private set; }
    [Export] public Paddle PaddleP1 { get; private set; }
    [Export] public Paddle PaddleP2 { get; private set; }
    [Export] public Ball Ball { get; private set; }
    [ExportGroup("Rects")]
    [Export] public ColorRect CrossRect { get; private set; }
    [Export] public ColorRect DividerRect { get; private set; }
    [ExportGroup("HUD Properties")]
    [Export] public Label ScoreP1Label { get; private set; }
    [Export] public Label ScoreP2Label { get; private set; }
    [Export] public Label TimerLabel { get; private set; }
    [Export] public Label MiddleScreenLabel { get; private set; }
    private bool _isGameOver = true;
    private bool _isPaused = false;
    private bool _isRainbowEffectActive = false;
    private AudioManager _audioManager;
    private PauseWatcher _pauseWatcher;
    private IController _controller1;
    private IController _controller2;
    private Score _scoreP1;
    private Score _scoreP2;
    private int _timeInSeconds = 0;
    private int _maxTimeInSeconds = 9999;
    private byte _maxScore = 255;
    // -> Godot Overrides
    public override void _EnterTree()
    {
        _audioManager = this.AddNode<AudioManager>();
        _pauseWatcher = this.AddNode<PauseWatcher>();
    }
    public override void _Ready()
    {
        Ball.Inject(_audioManager);
        Menu.Inject(_audioManager);
        _scoreP1 = new Score(ScoreP1Label);
        _scoreP2 = new Score(ScoreP2Label);
        Menu.OnGameCancel += GamePause;
        Menu.OnGameStart += GameStart;
        GameTimer.Timeout += TimerUpdate;
        _pauseWatcher.OnTogglePause += GamePause;
    }
    public override void _Process(double delta)
    {
        if (_isGameOver || _isPaused)
            return;
        _controller1.Update();
        _controller2.Update();
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
            Menu.Visible = false;
            _isPaused = false;
        }
        else
        {
            GameTimer.Paused = true;
            Ball.ToggleEnable();
            Menu.Visible = true;
            _isPaused = true;
        }
    }
    /// <summary>
    /// Ends the current game.
    /// </summary>
    private async void GameOver()
    {
        _isGameOver = true;
        MiddleScreenLabel.Text =
            _scoreP1.CurrentScore > _scoreP2.CurrentScore ?
            "Player 1 Wins!" :
            _scoreP2.CurrentScore > _scoreP1.CurrentScore ?
            "Player 2 Wins!" :
            "It's a Tie!";
        MiddleScreenLabel.Visible = true;
        ToggleRainbowColorEffect();
        Menu.ToggleButtons();
        Ball.ToggleEnable();
        GameTimer.Stop();
        MiddleScreenLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(6.0), "timeout");
        MiddleScreenLabel.Visible = false;
        Menu.Visible = true;
        ToggleRainbowColorEffect();
        GameReset();
    }
    /// <summary>
    /// Resets the current game to initial state.
    /// </summary>
    private void GameReset()
    {
        GameTimer.Stop();
        PaddleP1.ResetPosition();
        PaddleP2.ResetPosition();
        Ball.ResetBall();
        _scoreP1.Reset();
        _scoreP2.Reset();
        _timeInSeconds = 0;
        if (_isPaused)
            _isPaused = false;
    }
    /// <summary>
    /// Starts a new game with the given parameters sent from the Menu.
    /// </summary>
    /// <param name="player1Type"></param>
    /// <param name="player2Type"></param>
    /// <param name="ballSize"></param>
    /// <param name="paddle1Size"></param>
    /// <param name="paddle2Size"></param>
    /// <param name="paddle1Speed"></param>
    /// <param name="paddle2Speed"></param>
    /// <param name="paddle1Color"></param>
    /// <param name="paddle2Color"></param>
    /// <param name="gameTime"></param>
    /// <param name="maxScore"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void GameStart(PlayerType player1Type, PlayerType player2Type, int ballSize, int paddle1Size, int paddle2Size, int paddle1Speed, int paddle2Speed, Color paddle1Color, Color paddle2Color, Color ballColor, int gameTime, int maxScore)
    {
        if (_controller1 != null)
            _controller1.Detach();
        if (_controller2 != null)
            _controller2.Detach();
        _controller1 = player1Type switch
        {
            PlayerType.Player1 => new PaddlePlayer(PaddleP1, _scoreP1, true, true),
            PlayerType.Player2 => new PaddlePlayer(PaddleP1, _scoreP1, true, false),
            PlayerType.AI => new PaddleAI(PaddleP1, _scoreP1, true),
            _ => throw new ArgumentOutOfRangeException(nameof(player1Type), "Invalid player type")
        };
        GD.Print("Controller 1 created.");
        _controller2 = player2Type switch
        {
            PlayerType.Player1 => new PaddlePlayer(PaddleP2, _scoreP2, false, true),
            PlayerType.Player2 => new PaddlePlayer(PaddleP2, _scoreP2, false, false),
            PlayerType.AI => new PaddleAI(PaddleP2, _scoreP2, false),
            _ => throw new ArgumentOutOfRangeException(nameof(player2Type), "Invalid player type")
        };
        GD.Print("Controller 2 created.");
        _controller1.Attach();
        _controller2.Attach();
        if (_isGameOver)
            _isGameOver = false;
        GD.Print("Controllers attached.");
        MiddleScreenLabel.Visible = false;
        Ball.AdjustSize((byte)ballSize);
        Ball.AdjustColor(ballColor);
        GD.Print("Ball adjusted.");
        PaddleP1.Resize((byte)paddle1Size);
        PaddleP2.Resize((byte)paddle2Size);
        GD.Print("Paddles resized.");
        PaddleP1.ChangeSpeed((uint)paddle1Speed);
        PaddleP2.ChangeSpeed((uint)paddle2Speed);
        GD.Print("Paddles speed changed.");
        PaddleP1.AdjustColor(paddle1Color);
        PaddleP2.AdjustColor(paddle2Color);
        GD.Print("Paddles color adjusted.");
        _maxTimeInSeconds = gameTime;
        GD.Print("Game time set.");
        _maxScore = (byte)maxScore;
        GD.Print("Max score set.");
        if (_isPaused)
        {
            GamePause();
            return;
        }
        if (!_isGameOver)
        {
            GD.Print("Game already in progress.");
            Menu.ToggleButtons();
            GD.Print("Buttons toggled.");
            Ball.ToggleEnable();
            GD.Print("Ball enabled.");
            GameTimer.WaitTime = 1.0;
            _timeInSeconds = 0;
            GameTimer.Start();
            GD.Print("Game timer started.");
            Ball.ResetBall();
            _scoreP1.Reset();
            _scoreP2.Reset();
            GD.Print("Ball and Score reset.");
        }
    }
    /// <summary>
    /// Updates the game timer each second. Calls GameOver if the max time (or score) is reached.
    /// </summary>
    private async void TimerUpdate()
    {
        if (_scoreP1.CurrentScore >= _maxScore || _scoreP2.CurrentScore >= _maxScore)
            GameOver();
        if (_timeInSeconds < _maxTimeInSeconds)
        {
            _timeInSeconds++;
            int time = _maxTimeInSeconds - _timeInSeconds;
            TimerLabel.Text = time.ToString("D4");
        } else
            GameOver();
    }
    /// <summary>
    /// Applies a rainbow color effect to various game elements.
    /// </summary>
    private async void ToggleRainbowColorEffect()
    {
        _isRainbowEffectActive = !_isRainbowEffectActive;
        while (_isRainbowEffectActive)
        {
            Color color = new Color(GD.Randf(), GD.Randf(), GD.Randf());
            CrossRect.Color = color;
            DividerRect.Color = color;
            ScoreP1Label.AddThemeColorOverride("font_color", color);
            ScoreP2Label.AddThemeColorOverride("font_color", color);
            TimerLabel.AddThemeColorOverride("font_color", color);
            MiddleScreenLabel.AddThemeColorOverride("font_color", color);
            PaddleP1.AdjustColor(color);
            PaddleP2.AdjustColor(color);
            Ball.AdjustColor(color);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }
        Color white = new Color(1, 1, 1);
        CrossRect.Color = white;
        DividerRect.Color = white;
        ScoreP1Label.AddThemeColorOverride("font_color", white);
        ScoreP2Label.AddThemeColorOverride("font_color", white);
        TimerLabel.AddThemeColorOverride("font_color", white);
        MiddleScreenLabel.AddThemeColorOverride("font_color", white);
        PaddleP1.AdjustColor(white);
        PaddleP2.AdjustColor(white);
        Ball.AdjustColor(white);
    }
}
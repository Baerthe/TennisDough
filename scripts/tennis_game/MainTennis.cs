namespace TennisGame;

using Common;
using Godot;
using System;
/// <summary>
/// Main game controller for tennis_game. tennis_game is a pretty simple game, so we will have Main being the controller and orchestrator of the game.
/// It will manage the paddles and the ball, and handle the game logic.
/// </summary>
public sealed partial class MainTennis : Node2D
{
    [ExportGroup("References")]
    [Export] private MenuTennis _menu;
    [Export] private Timer _gameTimer;
    [Export] private Paddle _paddleP1;
    [Export] private Paddle _paddleP2;
    [Export] private BallTennis _ball;
    [ExportGroup("Rects")]
    [Export] private ColorRect _crossRect;
    [Export] private ColorRect _dividerRect;
    [ExportGroup("HUD Properties")]
    [Export] private Label _scoreP1Label;
    [Export] private Label _scoreP2Label;
    [Export] private Label _timerLabel;
    [Export] private Label _middleScreenLabel;
    // *-> Switches
    private bool _isRainbowEffectActive = false;
    // *-> Components
    private IController _controller1;
    private IController _controller2;
    private Score _scoreP1;
    private Score _scoreP2;
    // *-> Fields
    private int _timeInSeconds = 0;
    private int _maxTimeInSeconds = 9999;
    private byte _maxScore = 255;
    // *-> Singleton Access
    private readonly AudioManager _audioManager = GameManager.Audio;
    private readonly GameMonitor _monitor = GameManager.Monitor;
    // *-> Godot Overrides
    public override void _Ready()
    {
        // TODO: Really should remove injection and call the global singleton, but it works for now so eh?
        _ball.Inject(_audioManager);
        _menu.Inject(_audioManager);
        _scoreP1 = new Score(_scoreP1Label);
        _scoreP2 = new Score(_scoreP2Label);
        _menu.OnGameCancel += GamePause;
        _menu.OnGameStart += GameStart;
        _gameTimer.Timeout += TimerUpdate;
    }
    public override void _Process(double delta)
    {
        if (_monitor.CurrentState != GameState.InGame)
            return;
        _controller1.Update();
        _controller2.Update();
    }
    public override void _ExitTree()
    {
        _controller1?.Detach();
        _controller2?.Detach();
        _menu.OnGameCancel -= GamePause;
        _menu.OnGameStart -= GameStart;
        _gameTimer.Timeout -= TimerUpdate;
    }
    // *-> Game State Functions
    /// <summary>
    /// Pauses or unpauses the current game.
    /// </summary>
    private void GamePause()
    {
        if (_monitor.CurrentState == GameState.GameOver)
            return;
        if (_monitor.CurrentState == GameState.Paused)
        {
            if (_gameTimer.IsStopped())
                _gameTimer.Start();
            _gameTimer.Paused = false;
            _ball.ToggleEnable();
            _menu.Visible = false;
            _monitor.ChangeState(GameState.InGame);
        }
        else
        {
            _gameTimer.Paused = true;
            _ball.ToggleEnable();
            _menu.Visible = true;
            _monitor.ChangeState(GameState.Paused);
        }
    }
    /// <summary>
    /// Ends the current game.
    /// </summary>
    private async void GameOver()
    {
        _monitor.ChangeState(GameState.GameOver);
        _middleScreenLabel.Text =
            _scoreP1.CurrentScore > _scoreP2.CurrentScore ?
            "Player 1 Wins!" :
            _scoreP2.CurrentScore > _scoreP1.CurrentScore ?
            "Player 2 Wins!" :
            "It's a Tie!";
        _middleScreenLabel.Visible = true;
        ToggleRainbowColorEffect();
        _menu.ToggleButtons();
        _ball.ToggleEnable();
        _gameTimer.Stop();
        _middleScreenLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(6.0), "timeout");
        _middleScreenLabel.Visible = false;
        _menu.Visible = true;
        ToggleRainbowColorEffect();
        GameReset();
    }
    /// <summary>
    /// Resets the current game to initial state.
    /// </summary>
    private void GameReset()
    {
        _gameTimer.Stop();
        _paddleP1.ResetPosition();
        _paddleP2.ResetPosition();
        _ball.ResetBall();
        _scoreP1.Reset();
        _scoreP2.Reset();
        _timeInSeconds = 0;
        _monitor.ChangeState(GameState.InGame);
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
        _controller1?.Detach();
        _controller2?.Detach();
        _controller1 = player1Type switch
        {
            PlayerType.Player1 => new PaddlePlayer(_paddleP1, _ball, _scoreP1, true, true),
            PlayerType.Player2 => new PaddlePlayer(_paddleP1, _ball, _scoreP1, true, false),
            PlayerType.AI => new PaddleAI(_paddleP1, _ball, _scoreP1, true),
            _ => throw new ArgumentOutOfRangeException(nameof(player1Type), "Invalid player type")
        };
        GD.Print("Controller 1 created.");
        _controller2 = player2Type switch
        {
            PlayerType.Player1 => new PaddlePlayer(_paddleP2, _ball, _scoreP2, false, true),
            PlayerType.Player2 => new PaddlePlayer(_paddleP2, _ball, _scoreP2, false, false),
            PlayerType.AI => new PaddleAI(_paddleP2, _ball, _scoreP2, false),
            _ => throw new ArgumentOutOfRangeException(nameof(player2Type), "Invalid player type")
        };
        GD.Print("Controller 2 created.");
        _controller1.Attach();
        _controller2.Attach();
        GD.Print("Controllers attached.");
        _middleScreenLabel.Visible = false;
        _ball.AdjustSize((byte)ballSize);
        _ball.AdjustColor(ballColor);
        GD.Print("Ball adjusted.");
        _paddleP1.Resize((byte)paddle1Size);
        _paddleP2.Resize((byte)paddle2Size);
        GD.Print("Paddles resized.");
        _paddleP1.ChangeSpeed((uint)paddle1Speed);
        _paddleP2.ChangeSpeed((uint)paddle2Speed);
        GD.Print("Paddles speed changed.");
        _paddleP1.AdjustColor(paddle1Color);
        _paddleP2.AdjustColor(paddle2Color);
        GD.Print("Paddles color adjusted.");
        _maxTimeInSeconds = gameTime;
        GD.Print("Game time set.");
        _maxScore = (byte)maxScore;
        GD.Print("Max score set.");
        if (_monitor.CurrentState == GameState.Paused)
        {
            GamePause();
            return;
        }
        if (_monitor.CurrentState == GameState.InGame)
        {
            GD.Print("Game already in progress.");
            _menu.ToggleButtons();
            GD.Print("Buttons toggled.");
            _ball.ToggleEnable();
            GD.Print("Ball enabled.");
            _gameTimer.WaitTime = 1.0;
            _timeInSeconds = 0;
            _gameTimer.Start();
            GD.Print("Game timer started.");
            _ball.ResetBall();
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
            _timerLabel.Text = time.ToString("D4");
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
            Color color = new(GD.Randf(), GD.Randf(), GD.Randf());
            _crossRect.Color = color;
            _dividerRect.Color = color;
            _scoreP1Label.AddThemeColorOverride("font_color", color);
            _scoreP2Label.AddThemeColorOverride("font_color", color);
            _timerLabel.AddThemeColorOverride("font_color", color);
            _middleScreenLabel.AddThemeColorOverride("font_color", color);
            _paddleP1.AdjustColor(color);
            _paddleP2.AdjustColor(color);
            _ball.AdjustColor(color);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }
        Color white = new(1, 1, 1);
        _crossRect.Color = white;
        _dividerRect.Color = white;
        _scoreP1Label.AddThemeColorOverride("font_color", white);
        _scoreP2Label.AddThemeColorOverride("font_color", white);
        _timerLabel.AddThemeColorOverride("font_color", white);
        _middleScreenLabel.AddThemeColorOverride("font_color", white);
        _paddleP1.AdjustColor(white);
        _paddleP2.AdjustColor(white);
        _ball.AdjustColor(white);
    }
}
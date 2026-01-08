namespace Pong;

using Godot;
using System;
/// <summary>
/// Main game controller for Pong game. Pong is a pretty simple game, so we will have Main being the controller and orchestrator of the game.
/// It will manage the paddles and the ball, and handle the game logic.
/// </summary>
public partial class Main : Node2D
{
    [ExportGroup("References")]
    [Export] public Menu Menu { get; private set; }
    [Export] public PauseWatcher PauseWatcher { get; private set; }
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
    private IController _controller1;
    private IController _controller2;
    private Score _scoreP1;
    private Score _scoreP2;
    private int _timeInSeconds = 0;
    private int _maxTimeInSeconds = 9999;
    private byte _maxScore = 255;
    // -> Godot Overrides
    public override void _Ready()
    {
        _scoreP1 = new Score(ScoreP1Label);
        _scoreP2 = new Score(ScoreP2Label);
        Menu.OnGameCancel += GamePause;
        Menu.OnGameReset += GameReset;
        Menu.OnGameStart += GameStart;
        GameTimer.Timeout += TimerUpdate;
        PauseWatcher.OnTogglePause += GamePause;
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
        GameTimer.Stop();
        MiddleScreenLabel.Text = "Game Over, Returning to Menu...";
        Menu.ToggleButtons();
        Ball.ToggleEnable();
        _isGameOver = true;
        await ToSignal(GetTree().CreateTimer(6.0), "timeout");
        MiddleScreenLabel.Visible = false;
        Menu.Visible = true;
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
        _scoreP1.Reset();
        _scoreP2.Reset();
        Ball.ToggleEnable();
        TimerLabel.Text = "0000";
        _timeInSeconds = 0;
        _isGameOver = false;
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
    private void GameStart(PlayerType player1Type, PlayerType player2Type, int ballSize, int paddle1Size, int paddle2Size, int paddle1Speed, int paddle2Speed, Color paddle1Color, Color paddle2Color, int gameTime, int maxScore)
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

        _controller2 = player2Type switch
        {
            PlayerType.Player1 => new PaddlePlayer(PaddleP2, _scoreP2, false, true),
            PlayerType.Player2 => new PaddlePlayer(PaddleP2, _scoreP2, false, false),
            PlayerType.AI => new PaddleAI(PaddleP2, _scoreP2, false),
            _ => throw new ArgumentOutOfRangeException(nameof(player2Type), "Invalid player type")
        };
        _controller1.Attach();
        _controller2.Attach();
        MiddleScreenLabel.Visible = false;
        Ball.AdjustSize((byte)ballSize);
        PaddleP1.Resize((byte)paddle1Size);
        PaddleP2.Resize((byte)paddle2Size);
        PaddleP1.ChangeSpeed((uint)paddle1Speed);
        PaddleP2.ChangeSpeed((uint)paddle2Speed);
        PaddleP1.ChangeColor(paddle1Color);
        PaddleP2.ChangeColor(paddle2Color);
        _maxTimeInSeconds = gameTime;
        _maxScore = (byte)maxScore;
        if (_isPaused)
        {
            GamePause();
            return;
        }
        if (!_isGameOver)
        {
            Menu.ToggleButtons();
            TimerLabel.Text = "0000";
            GameTimer.WaitTime = 1.0;
            _timeInSeconds = 0;
            GameTimer.Start();
        }
    }
    /// <summary>
    /// Updates the game timer each second. Calls GameOver if the max time (or score) is reached.
    /// </summary>
    private async void TimerUpdate()
    {
        if (_scoreP1.CurrentScore >= _maxScore || _scoreP2.CurrentScore >= _maxScore)
        {
            MiddleScreenLabel.Text = $"Player {( _scoreP1.CurrentScore >= _maxScore ? "1" : "2" )} Wins!";
            MiddleScreenLabel.Visible = true;
            RainbowColorEffect(true);
            await ToSignal(GetTree().CreateTimer(8.0), "timeout");
            RainbowColorEffect(false);
            GameOver();
            return;
        }
        if (_timeInSeconds < _maxTimeInSeconds)
        {
            _timeInSeconds = int.Parse(TimerLabel.Text);
            _timeInSeconds++;
            TimerLabel.Text = _timeInSeconds.ToString("D4");
        } else
            GameOver();
    }
    /// <summary>
    /// Applies a rainbow color effect to various game elements.
    /// </summary>
    private async void RainbowColorEffect(bool toggle)
    {
        if (!toggle)
        {
            CrossRect.AddThemeColorOverride("color", Colors.White);
            DividerRect.AddThemeColorOverride("color", Colors.White);
            ScoreP1Label.AddThemeColorOverride("font_color", Colors.White);
            ScoreP2Label.AddThemeColorOverride("font_color", Colors.White);
            TimerLabel.AddThemeColorOverride("font_color", Colors.White);
            MiddleScreenLabel.AddThemeColorOverride("font_color", Colors.White);
            PaddleP1.ChangeColor(Colors.White);
            PaddleP2.ChangeColor(Colors.White);
            return;
        }
        while (toggle)
        {
            Color color = new Color(GD.Randf(), GD.Randf(), GD.Randf());
            CrossRect.AddThemeColorOverride("color", color);
            DividerRect.AddThemeColorOverride("color", color);
            ScoreP1Label.AddThemeColorOverride("font_color", color);
            ScoreP2Label.AddThemeColorOverride("font_color", color);
            TimerLabel.AddThemeColorOverride("font_color", color);
            MiddleScreenLabel.AddThemeColorOverride("font_color", color);
            PaddleP1.ChangeColor(color);
            PaddleP2.ChangeColor(color);
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
        }
    }
}
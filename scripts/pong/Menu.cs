namespace Pong;

using System;
using Godot;
public partial class Menu : Control
{
    public event Action<PlayerType, PlayerType, int, int, int, int, int, Color, Color> OnGameStart;
    public event Action OnGameCancel;
    public event Action OnGameReset;
    [Export] public Button ButtonReset { get; private set; }
    [Export] public Button ButtonCancel { get; private set; }
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonQuit;
    [Export] private OptionButton _optionPaddle1;
    [Export] private OptionButton _optionPaddle2;
    [Export] private HSlider _gameTimeSlider;
    [Export] private Label _labelGameTime;
    [Export] private HSlider _sliderBall;
    [Export] private Label _labelBallSpeed;
    [Export] private HSlider _sliderPaddle1Size;
    [Export] private Label _labelPaddle1Size;
    [Export] private HSlider _sliderPaddle2Size;
    [Export] private Label _labelPaddle2Size;
    [Export] private HSlider _sliderPaddle1Speed;
    [Export] private Label _labelPaddle1Speed;
    [Export] private HSlider _sliderPaddle2Speed;
    [Export] private Label _labelPaddle2Speed;
    [Export] private ColorPickerButton _colorPickerPaddle1;
    [Export] private ColorPickerButton _colorPickerPaddle2;
    public override void _Ready()
    {
        _buttonPlay.Pressed += OnButtonPlayPressed;
        ButtonReset.Visible = false;
        ButtonReset.Pressed += () => OnGameReset?.Invoke();
        _buttonQuit.Pressed += () => GetTree().Quit();
        ButtonCancel.Visible = false;
        ButtonCancel.Pressed += () => Visible = false;
        ButtonCancel.Pressed += () => OnGameCancel?.Invoke();
        _sliderBall.ValueChanged += (double value) => _labelBallSpeed.Text = ((int)value).ToString("D2");
        _sliderPaddle1Size.ValueChanged += (double value) => _labelPaddle1Size.Text = ((int)value).ToString("D2");
        _sliderPaddle2Size.ValueChanged += (double value) => _labelPaddle2Size.Text = ((int)value).ToString("D2");
        _sliderPaddle1Speed.ValueChanged += (double value) => _labelPaddle1Speed.Text = ((int)value).ToString("D2");
        _sliderPaddle2Speed.ValueChanged += (double value) => _labelPaddle2Speed.Text = ((int)value).ToString("D2");
        _labelBallSpeed.Text = ((int)_sliderBall.Value).ToString("D2");
        _labelPaddle1Size.Text = ((int)_sliderPaddle1Size.Value).ToString("D2");
        _labelPaddle2Size.Text = ((int)_sliderPaddle2Size.Value).ToString("D2");
        _labelPaddle1Speed.Text = ((int)_sliderPaddle1Speed.Value).ToString("D2");
        _labelPaddle2Speed.Text = ((int)_sliderPaddle2Speed.Value).ToString("D2");
    }
    private void OnButtonPlayPressed()
    {
        GD.Print("Play button pressed");
        Visible = false;
        OnGameStart?.Invoke(
            (PlayerType)_optionPaddle1.GetSelectedId(),
            (PlayerType)_optionPaddle2.GetSelectedId(),
            (int)_sliderBall.Value,
            (int)_sliderPaddle1Size.Value,
            (int)_sliderPaddle2Size.Value,
            (int)_sliderPaddle1Speed.Value,
            (int)_sliderPaddle2Speed.Value,
            _colorPickerPaddle1.Color,
            _colorPickerPaddle2.Color
        );
        GD.Print($"Controllers selected: P1 - {(PlayerType)_optionPaddle1.GetSelectedId()}, P2 - {(PlayerType)_optionPaddle2.GetSelectedId()}");
    }
}
namespace Pong;

using System;
using Godot;
public partial class Menu : Control
{
    public event Action<PlayerType, PlayerType, int, int, int, int, int, Color, Color> OnGameStart;
    public event Action OnGameReset;
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonReset;
    [Export] private Button _buttonQuit;
    [Export] private Button _buttonCancel;
    [Export] private OptionButton _optionPaddle1;
    [Export] private OptionButton _optionPaddle2;
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
    [Export] private Label _labelPaddle1Color;
    [Export] private ColorPickerButton _colorPickerPaddle2;
    [Export] private Label _labelPaddle2Color;
    public override void _Ready()
    {
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _buttonReset.Pressed += () => OnGameReset?.Invoke();
        _buttonQuit.Pressed += OnButtonQuitPressed;
        _buttonCancel.Pressed += OnButtonCancelPressed;
        _sliderBall.ValueChanged += OnSliderBallValueChanged;
        _sliderPaddle1Size.ValueChanged += OnSliderPaddle1SizeValueChanged;
        _sliderPaddle2Size.ValueChanged += OnSliderPaddle2SizeValueChanged;
        _sliderPaddle1Speed.ValueChanged += OnSliderPaddle1SpeedValueChanged;
        _sliderPaddle2Speed.ValueChanged += OnSliderPaddle2SpeedValueChanged;
        _colorPickerPaddle1.ColorChanged += OnColorPickerPaddle1ColorChanged;
        _colorPickerPaddle2.ColorChanged += OnColorPickerPaddle2ColorChanged;
        _labelBallSpeed.Text = ((int)_sliderBall.Value).ToString("D2");
        _labelPaddle1Size.Text = ((int)_sliderPaddle1Size.Value).ToString("D2");
        _labelPaddle2Size.Text = ((int)_sliderPaddle2Size.Value).ToString("D2");
        _labelPaddle1Speed.Text = ((int)_sliderPaddle1Speed.Value).ToString("D2");
        _labelPaddle2Speed.Text = ((int)_sliderPaddle2Speed.Value).ToString("D2");
        Color color1 = _colorPickerPaddle1.Color;
        Color color2 = _colorPickerPaddle2.Color;
        _labelPaddle1Color.Text = $"R:{(int)(color1.R * 255)} G:{(int)(color1.G * 255)} B:{(int)(color1.B * 255)}";
        _labelPaddle2Color.Text = $"R:{(int)(color2.R * 255)} G:{(int)(color2.G * 255)} B:{(int)(color2.B * 255)}";
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
    private void OnButtonQuitPressed() => GetTree().Quit();
    private void OnButtonCancelPressed() => Visible = false;
    private void OnSliderBallValueChanged(double value) => _labelBallSpeed.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle1SizeValueChanged(double value) => _labelPaddle1Size.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle2SizeValueChanged(double value) => _labelPaddle2Size.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle1SpeedValueChanged(double value) => _labelPaddle1Speed.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle2SpeedValueChanged(double value) => _labelPaddle2Speed.Text = ((int)value).ToString("D2");
    private void OnColorPickerPaddle1ColorChanged(Color color) => _labelPaddle1Color.Text = $"R:{(int)(color.R * 255)} G:{(int)(color.G * 255)} B:{(int)(color.B * 255)}";
    private void OnColorPickerPaddle2ColorChanged(Color color) => _labelPaddle2Color.Text = $"R:{(int)(color.R * 255)} G:{(int)(color.G * 255)} B:{(int)(color.B * 255)}";
}
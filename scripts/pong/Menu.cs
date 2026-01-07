namespace Pong;

using System;
using Godot;
public partial class Menu : Control
{
    public event Action<PlayerType, PlayerType, int, int, int> OnGameStart;
    [Export] private Button _buttonPlay;
    [Export] private OptionButton _optionPaddle1;
    [Export] private OptionButton _optionPaddle2;
    [Export] private HSlider _sliderBall;
    [Export] private Label _labelBallSpeed;
    [Export] private HSlider _sliderPaddle1Size;
    [Export] private Label _labelPaddle1Size;
    [Export] private HSlider _sliderPaddle2Size;
    [Export] private Label _labelPaddle2Size;
    public override void _Ready()
    {
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _sliderBall.ValueChanged += OnSliderBallValueChanged;
        _sliderPaddle1Size.ValueChanged += OnSliderPaddle1SizeValueChanged;
        _sliderPaddle2Size.ValueChanged += OnSliderPaddle2SizeValueChanged;
        _labelBallSpeed.Text = ((int)_sliderBall.Value).ToString("D2");
    }
    private void OnSliderBallValueChanged(double value) => _labelBallSpeed.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle1SizeValueChanged(double value) => _labelPaddle1Size.Text = ((int)value).ToString("D2");
    private void OnSliderPaddle2SizeValueChanged(double value) => _labelPaddle2Size.Text = ((int)value).ToString("D2");
    private void OnButtonPlayPressed()
    {
        GD.Print("Play button pressed");
        Visible = false;
        OnGameStart?.Invoke(
            (PlayerType)_optionPaddle1.GetSelectedId(),
            (PlayerType)_optionPaddle2.GetSelectedId(),
            (int)_sliderBall.Value,
            (int)_sliderPaddle1Size.Value,
            (int)_sliderPaddle2Size.Value
        );
        GD.Print($"Controllers selected: P1 - {(PlayerType)_optionPaddle1.GetSelectedId()}, P2 - {(PlayerType)_optionPaddle2.GetSelectedId()}");
    }
}
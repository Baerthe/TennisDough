namespace Pong;

using System;
using Godot;
public partial class Menu : Control
{
    public event Action<PlayerType, PlayerType, int> OnGameStart;
    [Export] private Button _buttonPlay;
    [Export] private OptionButton _optionPaddle1;
    [Export] private OptionButton _optionPaddle2;
    [Export] private HSlider _sliderBall;
    [Export] private Label _labelBallSpeed;
    public override void _Ready()
    {
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _sliderBall.ValueChanged += OnSliderBallValueChanged;
        _labelBallSpeed.Text = ((int)_sliderBall.Value).ToString("D2");
    }
    private void OnSliderBallValueChanged(Double value)
    {
        GD.Print($"Ball speed slider changed to {_sliderBall.Value}");
        _labelBallSpeed.Text = ((int)value).ToString("D2");
    }
    private void OnButtonPlayPressed()
    {
        GD.Print("Play button pressed");
        Visible = false;
        OnGameStart?.Invoke(
            (PlayerType)_optionPaddle1.GetSelectedId(),
            (PlayerType)_optionPaddle2.GetSelectedId(),
            (int)_sliderBall.Value
        );
        GD.Print($"Controllers selected: P1 - {(PlayerType)_optionPaddle1.GetSelectedId()}, P2 - {(PlayerType)_optionPaddle2.GetSelectedId()}");
    }
}
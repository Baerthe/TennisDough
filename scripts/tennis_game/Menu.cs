namespace tennis_game;

using System;
using Godot;
public partial class Menu : Control
{
    public event Action<PlayerType, PlayerType, int, int, int, int, int, Color, Color, int, int> OnGameStart;
    public event Action OnGameCancel;
    public event Action OnGameReset;
    private AudioStreamPlayer _audioPlayer;
    [ExportGroup("Sound Effects")]
    [Export] private AudioStream _sfxButtonPress;
    [Export] private AudioStream _sfxMenuOpen;
    [ExportGroup("Buttons")]
    [Export] private Button _buttonReset;
    [Export] private Button _buttonCancel;
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonQuit;
    [Export] private OptionButton _optionPaddle1;
    [Export] private OptionButton _optionPaddle2;
    [ExportGroup("Settings Sliders")]
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
    [Export] private HSlider _gameTimeSlider;
    [Export] private Label _labelGameTime;
    [Export] private HSlider _maxScoreSlider;
    [Export] private Label _labelMaxScore;
    public override void _Ready()
    {
        _audioPlayer = new AudioStreamPlayer();
        AddChild(_audioPlayer);
        _buttonReset.Visible = false;
        _buttonCancel.Visible = false;
        // Connect button signals
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _buttonQuit.Pressed += () => GetTree().Quit();
        _buttonCancel.Pressed += () => Visible = false;
        _buttonReset.Pressed += () => OnGameReset?.Invoke();
        _buttonCancel.Pressed += () => OnGameCancel?.Invoke();
        // Connect sound effects
        _buttonPlay.Pressed += () => PlaySfx(_sfxButtonPress);
        _buttonQuit.Pressed += () => PlaySfx(_sfxButtonPress);
        _buttonCancel.Pressed += () => PlaySfx(_sfxButtonPress);
        _buttonReset.Pressed += () => PlaySfx(_sfxButtonPress);
        VisibilityChanged += () =>
        {
            if (Visible)
                PlaySfx(_sfxMenuOpen);
        };
        // Connect slider signals to update labels
        _gameTimeSlider.ValueChanged += (double value) => _labelGameTime.Text = ((int)value).ToString("D4");
        _sliderBall.ValueChanged += (double value) => _labelBallSpeed.Text = ((int)value).ToString("D2");
        _sliderPaddle1Size.ValueChanged += (double value) => _labelPaddle1Size.Text = ((int)value).ToString("D2");
        _sliderPaddle2Size.ValueChanged += (double value) => _labelPaddle2Size.Text = ((int)value).ToString("D2");
        _sliderPaddle1Speed.ValueChanged += (double value) => _labelPaddle1Speed.Text = ((int)value).ToString("D2");
        _sliderPaddle2Speed.ValueChanged += (double value) => _labelPaddle2Speed.Text = ((int)value).ToString("D2");
        _maxScoreSlider.ValueChanged += (double value) => _labelMaxScore.Text = ((int)value).ToString("D2");
        // Initialize labels
        _labelGameTime.Text = ((int)_gameTimeSlider.Value).ToString("D4");
        _labelBallSpeed.Text = ((int)_sliderBall.Value).ToString("D2");
        _labelPaddle1Size.Text = ((int)_sliderPaddle1Size.Value).ToString("D2");
        _labelPaddle2Size.Text = ((int)_sliderPaddle2Size.Value).ToString("D2");
        _labelPaddle1Speed.Text = ((int)_sliderPaddle1Speed.Value).ToString("D2");
        _labelPaddle2Speed.Text = ((int)_sliderPaddle2Speed.Value).ToString("D2");
        _labelMaxScore.Text = ((int)_maxScoreSlider.Value).ToString("D2");
        // Configure color pickers
        ConfigureColorPicker(_colorPickerPaddle1);
        ConfigureColorPicker(_colorPickerPaddle2);
    }
    /// <summary>
    /// Toggles the visibility of the Reset and Cancel buttons.
    /// </summary>
    public void ToggleButtons()
    {
        _buttonReset.Visible = !_buttonReset.Visible;
        _buttonCancel.Visible = !_buttonCancel.Visible;
    }
    /// <summary>
    /// Handles the Play button press event. Sends the menu settings to start a new game.
    /// </summary>
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
            _colorPickerPaddle2.Color,
            (int)_gameTimeSlider.Value,
            (int)_maxScoreSlider.Value
        );
        GD.Print($"Controllers selected: P1 - {(PlayerType)_optionPaddle1.GetSelectedId()}, P2 - {(PlayerType)_optionPaddle2.GetSelectedId()}");
    }
    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="sfx"></param>
    private void PlaySfx(AudioStream sfx)
    {
        _audioPlayer.Stream = sfx;
        _audioPlayer.Play();
    }
    /// <summary>
    /// Configures the appearance and behavior of a ColorPickerButton.
    /// </summary>
    private void ConfigureColorPicker(ColorPickerButton button)
    {
        ColorPicker picker = button.GetPicker();
        picker.AddThemeConstantOverride("sv_width", 100);
        picker.AddThemeConstantOverride("sv_height", 100);
        picker.PresetsVisible = false;
        picker.CanAddSwatches = false;
        picker.SamplerVisible = false;
        picker.ColorModesVisible = false;
        picker.HexVisible = false;
    }
}
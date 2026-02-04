namespace BlockGame;

using Common;
using Godot;
using System;
using System.Linq;

/// <summary>
/// Menu UI for BlockGame.
/// </summary>
public sealed partial class MenuBlock : Control
{
    // *-> Events
    public event Action OnGameStart;
    public event Action OnGameCancel;
    public event Action OnGameReset;
    public event Action OnMenuOpen;
    // *-> Exports
    [ExportGroup("Buttons")]
    [Export] private Button _buttonCancel;
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonQuit;
    [Export] private OptionButton _optionPaddle;
    [Export] private OptionButton _optionLevel;
    [ExportGroup("Sound Effects")]
    [Export] private AudioEvent _sfxButtonPress;
    [Export] private AudioEvent _sfxMenuOpen;
    // *-> Dependencies
    private AudioManager _audioManager;

    // *-> Godot Overrides
    public override void _Ready()
    {
        _buttonCancel.Visible = false;
        // Connect button signals
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _buttonQuit.Pressed += () => GetTree().Quit();
        _buttonCancel.Pressed += () => Visible = false;
        _buttonCancel.Pressed += () => OnGameCancel?.Invoke();
        foreach (Button button in GetChildren().Cast<Button>())
            button.Pressed += OnAnyButtonPressed;
    }
    public void Inject(AudioManager audioManager) => _audioManager = audioManager;
    /// <summary>
    /// Handles the Play button press event.
    /// </summary>
    private void OnButtonPlayPressed()
    {
        OnGameStart?.Invoke();
    }
    /// <summary>
    /// Handles any button press to play a sound effect.
    /// </summary>
    private void OnAnyButtonPressed() => _audioManager.PlayAudioClip(_sfxButtonPress);
}
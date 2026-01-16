namespace BlockGame;

using Godot;
using System;
/// <summary>
/// Menu UI for BlockGame.
/// </summary>
public sealed partial class MenuBlock : Control
{
    public event Action OnGameStart;
    public event Action OnGameCancel;
    public event Action OnGameReset;
    public event Action OnMenuOpen;
    public event Action OnAnyButtonPress;
    [ExportGroup("Buttons")]
    [Export] private Button _buttonCancel;
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonQuit;
    [Export] private OptionButton _optionPaddle;
    [Export] private OptionButton _optionLevel;
    // -> Godot Overrides
    public override void _Ready()
    {
        _buttonCancel.Visible = false;
        // Connect button signals
        _buttonPlay.Pressed += () => OnAnyButtonPress?.Invoke();
        _buttonPlay.Pressed += OnButtonPlayPressed;
        _buttonQuit.Pressed += () => OnAnyButtonPress?.Invoke();
        _buttonQuit.Pressed += () => GetTree().Quit();
        _buttonCancel.Pressed += () => Visible = false;
        _buttonCancel.Pressed += () => OnGameCancel?.Invoke();
    }
    //TODO: Add a binder to the util so make dual eventing easier....
    /// <summary>
    /// Handles the Play button press event.
    /// </summary>
    private void OnButtonPlayPressed()
    {
        OnGameStart?.Invoke();
    }
}
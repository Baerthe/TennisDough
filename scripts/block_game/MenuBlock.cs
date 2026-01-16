namespace BlockGame;

using Godot;
using System;
/// <summary>
/// Menu UI for BlockGame.
/// </summary>
public sealed partial class MenuBlock : Control
{
    public event Action OnGameCancel;
    public event Action OnGameReset;
    [ExportGroup("Buttons")]
    [Export] private Button _buttonCancel;
    [Export] private Button _buttonPlay;
    [Export] private Button _buttonQuit;
    [Export] private OptionButton _optionPaddle;
    [Export] private OptionButton _optionLevel;
    // -> Godot Overrides
    public override void _Ready(){}
}
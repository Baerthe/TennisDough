namespace Common;

using Godot;
using System;
public sealed partial class MainMenu : Control
{
    // TODO
    public event Action<GameSelection> OnStartGame;
    public event Action OnQuitGame;
}
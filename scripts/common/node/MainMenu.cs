namespace Common;

using Godot;
using System;
public sealed partial class MainMenu : Control
{
    // TODO
    public event Action<GamePack> OnStartGame;
    public event Action OnQuitGame;
}
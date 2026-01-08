namespace tennis_game;

/// <summary>
/// Watches for pause input and triggers pause events.
/// </summary>
using System;
using Godot;
[GlobalClass]
public sealed partial class PauseWatcher : Node
{
    public event Action OnTogglePause;
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause_game"))
        {
            OnTogglePause?.Invoke();
        }
    }
}
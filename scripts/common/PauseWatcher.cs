namespace Common;

using System;
using Godot;
[GlobalClass]
public sealed partial class PauseWatcher : Node
{
    public event Action OnTogglePause;
    private bool _isPaused = false;
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("pause_game"))
        {
            _isPaused = !_isPaused;
            OnTogglePause?.Invoke();
        }
    }
}
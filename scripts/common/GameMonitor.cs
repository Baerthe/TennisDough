namespace Common;

using System;
using Godot;

/// <summary>
/// Monitors the current game state and transitions between states, independently from scene.
/// Lets higher level systems talk to the GameManager without direct references.
/// </summary>
public sealed class GameMonitor
{
    public event Action<GameState> OnGameStateChanged;
    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public GameState PriorState { get; private set; } = GameState.MainMenu;
    public GameMonitor()
    {
        GD.Print("GameMonitor: Initialized");
    }
    /// <summary>
    /// Changes the current game state and notifies listeners.
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;
        PriorState = CurrentState;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(CurrentState);
    }
}
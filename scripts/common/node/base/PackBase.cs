namespace Common;

using Common;
using Godot;
using System;
public abstract partial class PackBase : Node2D
{
    public abstract event Action<string, uint> OnScoreSubmission;
    public readonly AudioManager AudioManager = GameManager.Audio;
    public readonly GameMonitor Monitor = GameManager.Monitor;
}
namespace Common;

using Godot;
using System;
public abstract partial class PackBase : Node2D
{
    public abstract event Action<string, uint> OnScoreSubmission;
}
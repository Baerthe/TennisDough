namespace BlockGame;

using Common;
using Godot;
public partial class Block : Area2D
{
    [Export] public AudioStream AudioBlockHit { get; private set; }
    public AudioManager AudioManager { get; private set; }
}
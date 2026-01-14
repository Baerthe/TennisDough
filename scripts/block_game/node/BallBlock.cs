namespace BlockGame;

using Common;
using Godot;
using System;
/// <summary>
/// Ball node for BlockGame.
/// </summary>
[GlobalClass]
public sealed partial class BallBlock : BallBase
{
    [Export] public AudioStream AudioOutOfBounds { get; private set; }
    public event Action<Block> OnBlockHit;
    public event Action OnOutOfBounds;
    public override void _Ready()
    {
        base._Ready();
        AudioManager.AddAudioClip("out_of_bounds", AudioOutOfBounds);
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled)
            return;
        Velocity = Velocity.Clamp(new Vector2(-12000,-12000), new Vector2(12000, 12000));
        var collision = MoveAndCollide(Velocity * (float)delta * SpeedFactor);
        if (collision != null)
        {
            if (collision.GetCollider() is Block block)
                OnBlockHit?.Invoke(block);
            else
                AudioManager.PlayAudioClip("hit");
            var normal = collision.GetNormal();
            Velocity = Velocity.Bounce(normal);
            SpeedFactor += SpeedFactor * (Acceleration / 200.0f);
            SpeedFactor = Mathf.Clamp(SpeedFactor, 0f, 0.8f);
            if (Velocity.X > -256 && Velocity.X < 256)
                Velocity = new Vector2(GD.RandRange(-512, 512), Velocity.Y);
        }
    }
    /// <summary>
    /// Injects the AudioManager dependency for playing sound effects.
    /// </summary>
    /// <param name="audioManager"></param>
    public override void Inject(AudioManager audioManager)
    {
        base.Inject(audioManager);
        AudioManager.AddAudioClip("out_of_bounds", AudioOutOfBounds);
    }
    /// <summary>
    /// Resets the ball position and velocity.
    /// </summary>
    public override void ResetBall()
    {
        Velocity = Vector2.Zero;
        GlobalPosition = InitialPosition;
        SpeedFactor = 0.05f;
        if (GlobalPosition.Y < -640)
        {
            AudioManager.PlayAudioClip("out_of_bounds");
            OnOutOfBounds?.Invoke();
        }
    }
}
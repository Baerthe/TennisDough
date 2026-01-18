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
    public event Action<Block> OnBlockHit;
    public event Action OnOutOfBounds;
    public override void _Ready()
    {
        base._Ready();
        Velocity = new Vector2( GD.RandRange(-512, 512), 12000);
        ToggleEnable();
        SpeedFactor = 0.005f;
    }
    public override void _PhysicsProcess(double delta)
    {
        // if (!IsEnabled)
        //     return;
        Velocity = Velocity.Clamp(new Vector2(-8000,-8000), new Vector2(8000, 8000));
        var collision = MoveAndCollide(Velocity * (float)delta * SpeedFactor);
        if (collision != null)
        {
            if (collision.GetCollider() is Block block)
                OnBlockHit?.Invoke(block);
            var normal = collision.GetNormal();
            Velocity = Velocity.Bounce(normal);
            SpeedFactor += SpeedFactor * (Acceleration / 400.0f);
            SpeedFactor = Mathf.Clamp(SpeedFactor, 0f, 0.8f);
            if (Velocity.X > -256 && Velocity.X < 256)
                Velocity = new Vector2(GD.RandRange(-512, 512), Velocity.Y);
        }
    }
    /// <summary>
    /// Resets the ball position and velocity.
    /// </summary>
    public override void ResetBall()
    {
        Velocity = Vector2.Zero;
        GlobalPosition = InitialPosition;
        SpeedFactor = 0.005f;
        if (GlobalPosition.Y < -640)
        {
            OnOutOfBounds?.Invoke();
        }
    }
}
namespace TennisGame;

using Common;
using Godot;
using System;
/// <summary>
/// Represents the ball in the tennis_game.
/// Handles movement, collisions, scoring, and visual effects.
/// </summary>
[GlobalClass]
public sealed partial class Ball : BallBase
{
    public event Action<bool> OnOutOfBounds;
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("ball");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled)
            return;
        base._PhysicsProcess(delta);
        Velocity = Velocity.Clamp(new Vector2(-12000,-12000), new Vector2(12000, 12000));
        var collision = MoveAndCollide(Velocity * (float)delta * SpeedFactor);
        if (collision != null)
        {
            AudioManager.PlayAudioClip("hit");
            var normal = collision.GetNormal();
            Velocity = Velocity.Bounce(normal);
            SpeedFactor += SpeedFactor * (Acceleration / 200.0f);
            SpeedFactor = Mathf.Clamp(SpeedFactor, 0f, 1.2f);
            if (Velocity.Y > -128 && Velocity.Y < 128)
            {
                Velocity = new Vector2(Velocity.X, GD.RandRange(-512, 512));
            }
        }
    }
    /// <summary>
    /// Resets the ball position and velocity when it goes out of bounds.
    /// </summary>
    public override void ResetBall()
    {
        Velocity = Vector2.Zero;
        GlobalPosition = InitialPosition;
        SpeedFactor = 0.05f;
        var flip = GD.Randf() < 0.5f ? -1 : 1;
        if (GlobalPosition.X == 0)
        {
            if (flip < 0)
                Velocity = new Vector2( -8000, GD.RandRange(-512, 512));
            else
                Velocity = new Vector2( 8000, GD.RandRange(-512, 512));
            return;
        }
        if (GlobalPosition.X != 0)
        {
            var winner = GlobalPosition.X < 0 ? false : true;
            AudioManager.PlayAudioClip("score");
            OnOutOfBounds?.Invoke(winner);
            if (winner)
                Velocity = new Vector2( -8000, GD.RandRange(-512, 512));
            else
                Velocity = new Vector2( 8000, GD.RandRange(-512, 512));
        }
    }
}
namespace TennisGame;

using Common;
using Godot;
using System;
/// <summary>
/// Represents the ball in the tennis_game.
/// Handles movement, collisions, scoring, and visual effects.
/// </summary>
[GlobalClass]
/// TODO: Interface instead.
public sealed partial class Ball : BallBase
{
    public override void _Ready()
    {
        base._Ready();
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        Velocity = Velocity.Clamp(new Vector2(-12000,-12000), new Vector2(12000, 12000));
        var collision = MoveAndCollide(Velocity * (float)delta * _speedFactor);
        if (collision != null)
        {
            _audioManager.PlayAudioClip("hit");
            var normal = collision.GetNormal();
            Velocity = Velocity.Bounce(normal);
            _speedFactor += _speedFactor * (Acceleration / 200.0f);
            _speedFactor = Mathf.Clamp(_speedFactor, 0f, 1.2f);
            if (Velocity.Y > -128 && Velocity.Y < 128)
            {
                Velocity = new Vector2(Velocity.X, GD.RandRange(-512, 512));
            }
        }
    }
    /// <summary>
    /// Injects the AudioManager dependency for playing sound effects.
    /// </summary>
    /// <param name="audioManager"></param>
    public void Inject(AudioManager audioManager)
    {
        _audioManager = audioManager;
        _audioManager.AddAudioClip("hit", AudioHit);
        _audioManager.AddAudioClip("score", AudioScore);
    }
    /// <summary>
    /// Adjusts the size of the ball and updates related components.
    /// </summary>
    /// <param name="size"></param>
    public void AdjustSize(byte size)
    {
        Size = (byte)Mathf.Clamp(size, 8, 32);
        _collisionShape.Shape = new CircleShape2D() { Radius = Size / 2 };
        _colorRect.Size = new Vector2(Size, Size);
        _colorRect.Position = new Vector2(-Size / 2, -Size / 2);
        _trailParticles.ProcessMaterial.Set("scale_max", Size);
        _trailParticles.ProcessMaterial.Set("scale_min", Size);
    }
    /// <summary>
    /// Adjusts the color of the ball and its trail effect.
    /// </summary>
    /// <param name="color"></param>
    public void AdjustColor(Color color)
    {
        _colorRect.Color = color;
        _trailParticles.ProcessMaterial.Set("color", color);
    }
    /// <summary>
    /// Resets the ball position and velocity when it goes out of bounds.
    /// </summary>
    public void ResetBall()
    {
        Velocity = Vector2.Zero;
        GlobalPosition = _initialPosition;
        _speedFactor = 0.05f;
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
            _audioManager.PlayAudioClip("score");
            OnOutOfBounds?.Invoke();
            if (winner)
                Velocity = new Vector2( -8000, GD.RandRange(-512, 512));
            else
                Velocity = new Vector2( 8000, GD.RandRange(-512, 512));
        }
    }
    /// <summary>
    /// Toggles whether the ball is enabled (moving) or not.
    /// </summary>
    public void ToggleEnable() => _isEnabled = !_isEnabled;
}
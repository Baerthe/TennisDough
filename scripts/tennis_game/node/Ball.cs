namespace tennis_game;

using System;
using Godot;
/// <summary>
/// Represents the ball in the tennis_game.
/// Handles movement, collisions, scoring, and visual effects.
/// </summary>
[GlobalClass]
public sealed partial class Ball : CharacterBody2D
{
    public event Action<bool> OnOutOfBounds;
    [ExportGroup("Properties")]
    [Export] AudioStream AudioHit;
    [Export] AudioStream AudioScore;
    [Export(PropertyHint.Range, "1,100")] public byte Acceleration { get; private set; } = 25;
    [Export(PropertyHint.Range, "1,100")] public byte Size { get; private set; } = 8;
    private AudioManager _audioManager;
    private bool _isEnabled = false;
    private CollisionShape2D _collisionShape;
    private ColorRect _colorRect;
    private GpuParticles2D _trailParticles;
    private Vector2 _initialPosition;
    private VisibleOnScreenNotifier2D _visibleNotifier;
    private float _speedFactor;
    public override void _Ready()
    {
        _collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
        _colorRect = GetNode<ColorRect>("ColorRect");
        _trailParticles = GetNode<GpuParticles2D>("GPUParticles2D");
        _visibleNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        AdjustSize(Size);
        _visibleNotifier.ScreenExited += ResetBall;
        _initialPosition = GlobalPosition;
        AddToGroup("ball");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!_isEnabled)
            return;
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
            OnOutOfBounds?.Invoke(winner);
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
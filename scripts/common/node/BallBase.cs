namespace Common;

using Godot;
using System;
public abstract partial class BallBase : CharacterBody2D
{
    public event Action OnOutOfBounds;
    [ExportGroup("Properties")]
    [Export] AudioStream AudioHit;
    [Export] AudioStream AudioScore;
    [Export(PropertyHint.Range, "1,100")] public byte Acceleration { get; private set; } = 25;
    [Export(PropertyHint.Range, "1,100")] public byte Size { get; private set; } = 8;
    [ExportGroup("Components")]
    [Export] public CollisionShape2D CollisionShape { get; private set; }
    [Export] public ColorRect ColorRect { get; private set; }
    [Export] public GpuParticles2D TrailParticles { get; private set; }
    [Export] public VisibleOnScreenNotifier2D VisibleNotifier { get; private set; }
    public bool IsEnabled { get; private set; } = false;
    private AudioManager _audioManager;
    private Vector2 _initialPosition;
    private VisibleOnScreenNotifier2D _visibleNotifier;
    private float _speedFactor;
    public override void _Ready()
    {
        AdjustSize(Size);
        _visibleNotifier.ScreenExited += ResetBall;
        _initialPosition = GlobalPosition;
        AddToGroup("ball");
    }
    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled)
            return;
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
        CollisionShape.Shape = new CircleShape2D() { Radius = Size / 2 };
        ColorRect.Size = new Vector2(Size, Size);
        ColorRect.Position = new Vector2(-Size / 2, -Size / 2);
        TrailParticles.ProcessMaterial.Set("scale_max", Size);
        TrailParticles.ProcessMaterial.Set("scale_min", Size);
    }
    /// <summary>
    /// Adjusts the color of the ball and its trail effect.
    /// </summary>
    /// <param name="color"></param>
    public void AdjustColor(Color color)
    {
        ColorRect.Color = color;
        TrailParticles.ProcessMaterial.Set("color", color);
    }
    /// <summary>
    /// Resets the ball position and velocity when it goes out of bounds.
    /// </summary>
    public void ResetBall()
    {
        Velocity = Vector2.Zero;
        GlobalPosition = _initialPosition;
    }
    /// <summary>
    /// Toggles whether the ball is enabled (moving) or not.
    /// </summary>
    public void ToggleEnable() => _isEnabled = !_isEnabled;
}
namespace BlockGame;

using Godot;
/// <summary>
/// Represents a block in the BlockGame that can be hit by the ball.
/// Handles hit points, color changes, and destruction effects. Blocks must be 24 x 18 px.
/// </summary>
public sealed partial class Block : Area2D
{
	[Export] public CollisionShape2D CollisionShape { get; private set; }
	[Export] public ColorRect ColorRect { get; private set; }
	[Export] public GpuParticles2D HitParticles { get; private set; }
	public byte HitPoints { get; private set; } = 1;
	private bool _isDestroyed = false;
	private float _delta;
	private static readonly BlockColorMap _blockColorMap = BlockColorMap.Instance;
	public override void _Ready()
	{
		AddToGroup("block");
		ColorRect.Color = _blockColorMap.ColorMap[HitPoints];
	}
	public override void _Process(double delta)
	{
		if (!_isDestroyed)
			return;
		var duration = 0.05f;
		_delta += (float)delta;
		if (_delta >= duration)
		{
			_delta = 0;
			ColorRect.Color = new Color(ColorRect.Color, ColorRect.Color.A - 0.1f);
			if (ColorRect.Color.A <= 0)
				QueueFree();
		}
	}
	public override void _ExitTree()
	{
		RemoveFromGroup("block");
		ColorRect.Color = Colors.Transparent;
	}
	/// <summary>
	/// Handles the block being hit by the ball.
	/// </summary>
	public void OnBlockHit()
	{
		HitPoints--;
		HitParticles.Emitting = true;
		if (HitPoints <= 0)
			_isDestroyed = true;
		else
			ColorRect.Color = _blockColorMap.ColorMap[HitPoints];
	}
	/// <summary>
	/// Sets the hit points of the block and updates its color.
	/// </summary>
	/// <param name="hitPoints"></param>
	public void SetHitPoints(byte hitPoints)
	{
		HitPoints = hitPoints;
		ColorRect.Color = _blockColorMap.ColorMap[HitPoints];
	}
}

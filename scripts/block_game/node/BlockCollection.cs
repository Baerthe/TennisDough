namespace BlockGame;

using Common;
using Godot;
using System;
/// <summary>
/// Manages a collection of blocks in the BlockGame.
/// </summary>
[GlobalClass]
public sealed partial class BlockCollection : Node2D
{
    [Export] public PackedScene BlockScene { get; private set; }
    public Vector2 BlockSize { get; private set; } = new Vector2(24, 18);
    public Block[,] BlockArray { get; private set; }
    public Vector2 Spacing { get; private set; } = new Vector2(2, 2);
    private LevelData _currentLevel;
    private bool _debugMode = true;
    public override void _Process(double delta)
    {
        if (_debugMode)
            DebugDraw();
    }
    /// <summary>
    /// Cleans up all blocks from the current level.
    /// </summary>
    public void ClearLevel()
    {
        foreach (var block in GetTree().GetNodesInGroup("block"))
        {
            if (block is Block b)
                b.QueueFree();
        }
        BlockArray = new Block[0, 0];
        _currentLevel = null;
    }
    /// <summary>
    /// Generates a level based on the provided LevelData. It adds them within the BlockCollection node space.
    /// Leave level as null to generate a random level.
    /// </summary>
    /// <param name="level"></param>
    public void GenerateLevel(LevelData level = null)
    {
        _currentLevel = level;
        var lines = null as string[];
        var width = 24;
        if (level == null)
        {
            byte randLines = (byte)GD.RandRange(5, 15);
            lines = new string[randLines];
            for (byte i = 0; i < randLines; i++)
            {
                string line = "";
                for (byte j = 0; j < width; j++)
                {
                     line += GD.RandRange(0, 4).ToString();
                }
                lines[i] = line;
            }
        }
        else
            lines = level.LevelGrid.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var height = lines.Length;
        BlockArray = new Block[width, height];
        for (int y = 0; y < height; y++)
        {
            var line = lines[y];
            for (int x = 0; x < width; x++)
            {
                if (x < line.Length)
                {
                    var charValue = line[x];
                    if (charValue >= '1' && charValue <= '9')
                    {
                        var hitPoints = (byte)(charValue - '0');
                        var blockInstance = BlockScene.Instantiate<Block>();
                        blockInstance.Position = new Vector2(x * (BlockSize.X + Spacing.X), y * (BlockSize.Y + Spacing.Y));
                        AddChild(blockInstance);
                        blockInstance.SetHitPoints(hitPoints);
                        BlockArray[x, y] = blockInstance;
                    }
                }
            }
        }
    }
    /// <summary>
    /// Resets the current level by clearing and regenerating it.
    /// </summary>
    public void ResetLevel()
    {
        if (_currentLevel != null)
        {
            ClearLevel();
            GenerateLevel(_currentLevel);
        }
    }
    /// <summary>
    /// Debug function to randomly hit blocks and regenerate level if all blocks are destroyed. Used to make sure both this and blocks logic worked.
    /// </summary>
    private void DebugDraw()
    {
        var rand = GD.RandRange(0, 1);
        if (rand > 0.999f)
        {
            GetTree().GetNodesInGroup("block")[GD.RandRange(0, GetTree().GetNodesInGroup("block").Count)].Call("OnBlockHit");
        }
        if (GetTree().GetNodesInGroup("block").Count == 0)
            GenerateLevel();
    }
}
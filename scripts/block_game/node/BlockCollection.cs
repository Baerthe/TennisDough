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
    public Vector2 BlockSize { get; private set; } = new Vector2(20, 14);
    public Block[,] BlockArray { get; private set; }
    public Vector2 Spacing { get; private set; } = new Vector2(2, 2);
    private LevelData _currentLevel;
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
    /// </summary>
    /// <param name="level"></param>
    public void GenerateLevel(LevelData level)
    {
        _currentLevel = level;
        BlockArray = new Block[0, 0];
        var lines = level.LevelGrid.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var height = lines.Length;
        var width = 20;
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
                        blockInstance.SetHitPoints(hitPoints);
                        blockInstance.Position = new Vector2(x * (BlockSize.X + Spacing.X), y * (BlockSize.Y + Spacing.Y));
                        AddChild(blockInstance);
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
}
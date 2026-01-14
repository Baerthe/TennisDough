namespace BlockGame;

using Godot;
using System.Collections.Generic;
/// <summary>
/// Class to hold block color mappings.
/// </summary>
public sealed class BlockColorMap
{
    public static BlockColorMap Instance { get; } = new BlockColorMap();
    public Dictionary<byte, Color> ColorMap { get; private set; }
    public BlockColorMap()
    {
        ColorMap = new Dictionary<byte, Color>
        {
            { 1, new Color(1, 0, 0) },
            { 2, new Color(0, 1, 0) },
            { 3, new Color(0, 0, 1) },
            { 4, new Color(1, 1, 0) }
        };
    }
}
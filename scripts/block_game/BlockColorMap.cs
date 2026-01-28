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
    public Dictionary<byte, ParticleProcessMaterial> ParticleColorMap { get; private set; }
    public BlockColorMap()
    {
        ColorMap = new Dictionary<byte, Color>
        {
            { 1, new Color(0.2f, 0.2f, 1) },
            { 2, new Color(0.4f, 0.4f, 1) },
            { 3, new Color(0.6f, 0.6f, 1) },
            { 4, new Color(0.8f, 0.8f, 1) }
        };
        ParticleColorMap = [];
    }
    public void AddParticleMaterial(byte hitPoints, ParticleProcessMaterial material)
    {
        var copy = material.Duplicate() as ParticleProcessMaterial;
        copy.Set("color", ColorMap[hitPoints]);
        if (!ParticleColorMap.ContainsKey(hitPoints))
            ParticleColorMap[hitPoints] = copy;
    }
}
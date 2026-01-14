namespace BlockGame;

using Godot;
/// <summary>
/// Resource representing level data for BlockGame.
/// </summary>
[GlobalClass]
public sealed partial class LevelData : Resource
{
    [Export(PropertyHint.MultilineText)] public string LevelGrid { get; set; } = "";
}
namespace Common;
/// <summary>
/// Direction enum for paddle movement.
/// </summary>
public enum Direction : byte
{
    None = 0,
    Up = 1,
    Down = 2
}
/// <summary>
/// Player type enum for game mode selection.
/// </summary>
public enum PlayerType : byte
{
    Player1 = 0,
    Player2 = 1,
    AI = 2
}
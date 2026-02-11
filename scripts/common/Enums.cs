namespace Common;
/// <summary>
/// Direction enum for player movement.
/// </summary>
public enum Direction : byte
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
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
/// <summary>
/// Game States enum for tracking.
/// </summary>
public enum GameState : byte
{
    MainMenu = 0,
    GameMenu = 1,
    InGame = 2,
    Paused = 3,
    GameOver = 4,
    Loading = 5
}
/// <summary>
/// Game Settings section enum
/// </summary>
public enum Sectional : byte
{
    Audio,
}
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
/// Game selection for menu.
/// </summary>
public enum GameSelection : byte
{
    None = 0,
    BlockGame = 1,
    TennisGame = 2
}
/// <summary>
/// Game States.
/// </summary>
public enum GameState : byte
{
    MainMenu = 0,
    InGame = 1,
    Paused = 2,
    GameOver = 3
}
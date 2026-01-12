namespace TennisGame;

using Common;
using Godot;
/// <summary>
/// AI controller for the paddle in the tennis_game.
/// Determines paddle movement based on ball position.
/// </summary>
public class PaddlePlayer : IController
{
    public Ball Ball { get; private set; }
    public PlayerType PlayerType { get; private set; }
    public Paddle Paddle { get; private set; }
    public Score Score { get; private set; }
    private string _inputPrefix;
    public PaddlePlayer(Paddle paddle, Score score, PlayerType playerType)
    {
        PlayerType = playerType;
        _inputPrefix = playerType == PlayerType.Player1 ? "p1_" : "p2_";
        Paddle = paddle;
        Score = score;
        Ball = Paddle.GetTree().GetNodesInGroup("ball")[0] as Ball;
        GD.Print($"PaddlePlayer created for {(playerType == PlayerType.Player1 ? "Player 1" : "Player 2")}");
    }
    public Direction GetInputDirection()
    {
        Direction direction = Direction.None;
        if (Input.IsActionPressed($"{_inputPrefix}move_up"))
            direction = Direction.Up;
        if (Input.IsActionPressed($"{_inputPrefix}move_down"))
            direction = Direction.Down;
        return direction;
    }
}
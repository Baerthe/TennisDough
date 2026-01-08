namespace Pong;


using Godot;
public class PaddlePlayer : IController
{
    public Ball Ball { get; private set; }
    public bool IsLeftSide { get; private set; }
    public Paddle Paddle { get; private set; }
    public Score Score { get; private set; }
    private string _inputPrefix;
    public PaddlePlayer(Paddle paddle, Score score, bool isLeftSide, bool isPlayer1)
    {
        IsLeftSide = isLeftSide;
        _inputPrefix = isPlayer1 ? "p1_" : "p2_";
        Paddle = paddle;
        Score = score;
        Ball = Paddle.GetTree().GetNodesInGroup("ball")[0] as Ball;
        GD.Print($"PaddlePlayer created for {(IsLeftSide ? "Player 1" : "Player 2")}");
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
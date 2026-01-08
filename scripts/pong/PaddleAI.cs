namespace Pong;


using Godot;
public class PaddleAI : IController
{
    public Ball Ball { get; private set; }
    public bool IsLeftSide { get; private set; }
    public Paddle Paddle { get; private set; }
    public Score Score { get; private set; }
    private Direction _lastDirection = Direction.None;
    public PaddleAI(Paddle paddle, Score score, bool isLeftSide)
    {
        IsLeftSide = isLeftSide;
        Paddle = paddle;
        Score = score;
        Ball = Paddle.GetTree().GetNodesInGroup("ball")[0] as Ball;
        GD.Print($"PaddleAI created for {(IsLeftSide ? "Player 1" : "Player 2")}");
    }
    public Direction GetInputDirection()
    {
        int flip = GD.RandRange(0, 100);
        var detectionZone = (Paddle.Size + GD.RandRange(-18, 12)) / 4;
        Direction direction = Direction.None;
        if (flip < 20)
        {
            if (Ball.GlobalPosition.Y < Paddle.GlobalPosition.Y - detectionZone)
                direction = Direction.Up;
            else if (Ball.GlobalPosition.Y > Paddle.GlobalPosition.Y + detectionZone)
                direction = Direction.Down;
        }
        switch (_lastDirection)
        {
            case Direction.Up:
                if (flip < 8)
                     break;
                direction = Direction.Up;
                break;
            case Direction.Down:
                if (flip < 8)
                    break;
                direction = Direction.Down;
                break;
            case Direction.None:
                break;
        }
        _lastDirection = direction;
        return direction;
    }
}
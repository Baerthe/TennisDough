namespace BlockGame;

using Common;
/// <summary>
/// Interface for a controller that manages paddle movement and scoring.
/// </summary>
public interface IController
{
    BallBlock Ball { get; }
    PaddleBlock Paddle { get; }
    Score Score { get; }
    Direction GetInputDirection();
    public void Update()
    {
        Direction direction = GetInputDirection();
        if (direction == Direction.None)
            return;
        Paddle.Move(direction);
    }
}
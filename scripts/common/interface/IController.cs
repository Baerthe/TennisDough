namespace Common;

using TennisGame;
/// <summary>
/// Interface for a controller that manages paddle movement and scoring.
/// </summary>
public interface IController
{
    Ball Ball { get; }
    PlayerType PlayerType { get; }
    Paddle Paddle { get; }
    Score Score { get; }
    Direction GetInputDirection();
    public void Attach()
    {
        Ball.OnOutOfBounds += OnPointScore;
    }
    public void Detach()
    {
        Ball.OnOutOfBounds -= OnPointScore;
    }
    public void OnPointScore()
    {
        if (PlayerType == PlayerType.Player1 && Ball.GlobalPosition.X > 0 ||
            PlayerType == PlayerType.Player2 && Ball.GlobalPosition.X < 0)
            Score.AddPoint();
    }
    public void Update()
    {
        Direction direction = GetInputDirection();
        if (direction == Direction.None)
            return;
        Paddle.Move(direction);
    }
}
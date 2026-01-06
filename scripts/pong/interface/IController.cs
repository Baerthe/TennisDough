namespace Pong;

using Common;
using Godot;
public interface IController
{
    Ball Ball { get; }
    bool IsLeftSide { get; }
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
    public void OnPointScore(bool isLeftSide)
    {
        if (isLeftSide && IsLeftSide || !isLeftSide && !IsLeftSide)
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
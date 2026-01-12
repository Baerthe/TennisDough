namespace Common;

using Godot;
/// <summary>
/// Manages the score data for an IController player.
/// </summary>
public sealed class Score
{
    public byte CurrentScore { get { return _points; }}
    private Label _scoreLabel;
    private byte _points;
    public Score(Label scoreLabel)
    {
        _scoreLabel = scoreLabel;
        _points = 0;
        UpdateLabel();
    }
    public void AddPoint()
    {
        _points++;
        UpdateLabel();
    }
    public void Reset()
    {
        _points = 0;
        UpdateLabel();
    }
    private void UpdateLabel()
    {
        _scoreLabel.Text = _points.ToString("D8");
    }
}
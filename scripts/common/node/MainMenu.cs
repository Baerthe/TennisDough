namespace Common;

using Godot;
using System;
/// <summary>
/// Main menu controller.
/// </summary>
public sealed partial class MainMenu : Control
{
    // TODO: Setup more akin to other menus with up-sent events instead of direct logic?
    public event Action<GamePack> OnStartGame;
    public event Action OnQuitGame;
    [ExportGroup("References")]
    [Export] private AudioStream _menuTheme;
    [ExportGroup("Buttons")]
    [Export] private Button _quitButton;
    // *-> Singleton References
    private AudioManager _audioManager;
    private GameMonitor _gameMonitor;
    // *-> Godot Overrides
    public override void _Ready()
    {
        _audioManager = GameManager.Audio;
        _gameMonitor = GameManager.Monitor;
        _audioManager.AddMusicTrack("menu_theme", _menuTheme);
        _quitButton.Pressed += () => OnQuitGame?.Invoke();
        BootSequence();
    }
    // *-> Private Methods
    /// <summary>
    /// Boots the main menu sequence.
    /// </summary>
    private void BootSequence()
    {
        _gameMonitor.ChangeState(GameState.MainMenu);
        _audioManager.PlayMusicTrack("menu_theme");
    }
}
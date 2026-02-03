namespace Common;

using Godot;
using System;
/// <summary>
/// Main menu controller.
/// Unlike the pack minigames which have their menu logic handled by their main class,
/// the main menu handles itself as to not clutter the game manager.
/// </summary>
public sealed partial class MainMenu : Control
{
    public event Action OnBootSequence;
    public event Action<GamePack> OnStartGame;
    public event Action OnQuitGame;
    [ExportGroup("References")]
    [Export] private AudioStream _menuBootUpSound;
    [Export] private AudioStream _menuTheme;
    [Export] private HBoxContainer _packButtonContainer;
    [ExportGroup("Buttons")]
    [Export] private Button _quitButton;
    // *-> Singleton References
    private AudioManager _audioManager;
    private GameMonitor _gameMonitor;
    private PackManager _packManager;
    // *-> Godot Overrides
    public override void _Ready()
    {
        _audioManager = GameManager.Audio;
        _gameMonitor = GameManager.Monitor;
        _packManager = GameManager.PackManager;
        _audioManager.AddAudioClip("menu_bootup", _menuBootUpSound);
        _audioManager.AddMusicTrack("menu_theme", _menuTheme);
        _quitButton.Pressed += () => OnQuitGame?.Invoke();
        BootSequence();
        LoadPackButtons();
    }
    // *-> Private Methods
    /// <summary>
    /// Boots the main menu sequence.
    /// </summary>
    private void BootSequence()
    {
        _gameMonitor.ChangeState(GameState.MainMenu);
        _audioManager.PlayAudioClip("menu_bootup");
        _audioManager.PlayMusicTrack("menu_theme");
        OnBootSequence?.Invoke();
    }
    /// <summary>
    /// Loads buttons for each available game pack into the main menu.
    /// </summary>
    private void LoadPackButtons()
    {
        foreach (var packEntry in _packManager.GamePacks)
        {
            Button packButton = _packButtonContainer.AddNode<Button>();
            packButton.Text = packEntry.Key;
            packButton.Pressed += () =>
            {
                GamePack selectedPack = OnPackSelected(packEntry.Key);
                if (selectedPack != null)
                    OnStartGame?.Invoke(selectedPack);
            };
        }
    }
    /// <summary>
    /// Handles when a pack is selected from the menu.
    /// </summary>
    /// <param name="packName"></param>
    /// <returns></returns>
    private GamePack OnPackSelected(string packName)
    {
        if (_packManager.GamePacks.TryGetValue(packName, out var pack))
        {
            GD.Print($"MainMenu: Pack selected: {packName}");
            return pack;
        } else
        {
            GD.PrintErr($"MainMenu: Pack not found: {packName}");
            return null;
        }
    }
}
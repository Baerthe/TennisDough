# TennisDough Architecture

## Overview

TennisDough is a Godot 4.5+ C# minigame collection that recreates classic Atari games (Pong, Breakout) with modern features and extensibility. The project emphasizes clean architecture with reusable components and dynamic game management.

## Current Architecture

### Core Components

#### 1. Game Manager (`GameManager.cs`)
The central orchestrator responsible for:
- Managing global game state transitions
- Loading and instantiating game scenes
- Coordinating between main menu and game instances
- Handling pause/unpause functionality

Currently uses an enum-based approach (`GameSelection`) to identify and load games:
- `GameSelection.TennisGame`
- `GameSelection.BlockGame`

#### 2. Game Monitor (`GameMonitor.cs`)
A singleton that tracks:
- Current game state (`MainMenu`, `InGame`, `Paused`, `GameOver`)
- State change events
- Global game status

#### 3. Controller System (`IController`)
An abstraction pattern enabling:
- Dynamic switching between human and AI players
- Unified control interface for game entities
- Extensible player behavior implementations

#### 4. Resource Pattern (`LevelData.cs`)
Currently used in BlockGame to:
- Store level configuration data as Godot Resources
- Enable designer-friendly content creation
- Separate data from logic

### Current Limitations

1. **Hardcoded Game Enumeration**: The `GameSelection` enum requires code changes when adding/removing games
2. **Manual Scene References**: Each game requires an explicit `[Export] PackedScene` field in GameManager
3. **Inflexible Menu Population**: Main menu must be manually updated for new games
4. **Tight Coupling**: Game identification tightly coupled to enum values

---

## Proposed: Resource Registry System for Minigames

### Overview

A dynamic resource-based system that automatically discovers and registers available minigames at runtime, eliminating hardcoded enums and improving extensibility.

### Design Principles

1. **Convention over Configuration**: Games auto-discovered from standardized folder structure
2. **Data-Driven**: Game metadata stored in resources, not code
3. **Extensible**: Add new games by creating resources, not modifying code
4. **Designer-Friendly**: All game properties configurable in Godot Editor

### Architecture Components

#### 1. GameResource (`GameResource.cs`)

A Godot Resource class that encapsulates all minigame metadata:

```csharp
[GlobalClass]
public partial class GameResource : Resource
{
    /// <summary>
    /// Unique identifier for the game (derived from resource filename)
    /// </summary>
    [Export] public string GameId { get; set; }
    
    /// <summary>
    /// Display name shown in menus
    /// </summary>
    [Export] public string DisplayName { get; set; }
    
    /// <summary>
    /// Short description of the game
    /// </summary>
    [Export(PropertyHint.MultilineText)] public string Description { get; set; }
    
    /// <summary>
    /// Icon texture for menu display
    /// </summary>
    [Export] public Texture2D Icon { get; set; }
    
    /// <summary>
    /// Reference to the game's main scene
    /// </summary>
    [Export] public PackedScene GameScene { get; set; }
    
    /// <summary>
    /// Optional: Category for grouping games
    /// </summary>
    [Export] public string Category { get; set; } = "Arcade";
    
    /// <summary>
    /// Optional: Sort order for menu display
    /// </summary>
    [Export] public int SortOrder { get; set; } = 0;
}
```

**Key Features:**
- `[GlobalClass]` attribute makes it available in Godot Editor
- Resource filename serves as the `GameId` (e.g., `tennis_game.tres` → `"tennis_game"`)
- All properties exposed to Godot Inspector for easy editing
- Includes scene reference, metadata, and UI assets

#### 2. GameRegistry (`GameRegistry.cs`)

A singleton service that manages game resource discovery and access:

```csharp
public sealed class GameRegistry
{
    private static GameRegistry _instance;
    public static GameRegistry Instance => _instance ??= new GameRegistry();
    
    private Dictionary<string, GameResource> _games = new();
    private const string GAMES_RESOURCE_PATH = "res://resources/games/";
    
    /// <summary>
    /// Loads all GameResource files from the designated folder
    /// </summary>
    public void LoadGameResources()
    {
        _games.Clear();
        
        // Use Godot's DirAccess to scan for .tres files
        using var dir = DirAccess.Open(GAMES_RESOURCE_PATH);
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    string resourcePath = GAMES_RESOURCE_PATH + fileName;
                    var gameResource = GD.Load<GameResource>(resourcePath);
                    
                    if (gameResource != null)
                    {
                        // Use filename (without extension) as GameId
                        string gameId = fileName.Replace(".tres", "");
                        gameResource.GameId = gameId;
                        _games[gameId] = gameResource;
                        GD.Print($"Registered game: {gameId}");
                    }
                }
                fileName = dir.GetNext();
            }
            dir.ListDirEnd();
        }
    }
    
    /// <summary>
    /// Retrieves a game resource by ID
    /// </summary>
    public GameResource GetGame(string gameId) => 
        _games.TryGetValue(gameId, out var game) ? game : null;
    
    /// <summary>
    /// Returns all registered games
    /// </summary>
    public IEnumerable<GameResource> GetAllGames() => 
        _games.Values.OrderBy(g => g.SortOrder);
    
    /// <summary>
    /// Returns games filtered by category
    /// </summary>
    public IEnumerable<GameResource> GetGamesByCategory(string category) =>
        _games.Values.Where(g => g.Category == category).OrderBy(g => g.SortOrder);
}
```

**Key Features:**
- Singleton pattern for global access
- Uses `DirAccess` (Godot's filesystem API) to scan resource folder
- Automatic resource loading and registration
- Dictionary-based lookup by string ID
- Supports filtering and sorting

#### 3. Updated GameManager

The GameManager is modified to use the registry instead of enums:

```csharp
public sealed partial class GameManager : Node
{
    public static GameMonitor Monitor { get; private set; } = new GameMonitor();
    
    // No more individual PackedScene exports!
    // No more GameSelection enum handling!
    
    private MainMenu _mainMenu;
    private PauseWatcher _pauseWatcher;
    private Node _currentGameInstance;
    
    public override void _Ready()
    {
        // Initialize registry
        GameRegistry.Instance.LoadGameResources();
        
        // Load main menu (now receives list of games from registry)
        _mainMenu = GetNode<MainMenu>("MainMenu");
        _mainMenu.OnStartGame += HandleStartGame;
        
        _pauseWatcher = this.AddNode<PauseWatcher>();
        _pauseWatcher.OnTogglePause += HandleTogglePause;
    }
    
    /// <summary>
    /// Handles starting a game by string ID instead of enum
    /// </summary>
    private void HandleStartGame(string gameId)
    {
        var gameResource = GameRegistry.Instance.GetGame(gameId);
        if (gameResource == null)
        {
            GD.PrintErr($"Game not found: {gameId}");
            return;
        }
        
        GD.Print($"Starting game: {gameResource.DisplayName}");
        
        // Clean up previous game if exists
        _currentGameInstance?.QueueFree();
        
        // Instantiate the game scene
        _currentGameInstance = gameResource.GameScene.Instantiate();
        AddChild(_currentGameInstance);
        
        Monitor.ChangeState(GameState.InGame);
    }
}
```

**Key Changes:**
- No hardcoded `PackedScene` exports
- No enum-based switch statements
- Game identified by string ID
- Dynamic scene instantiation from registry

#### 4. Updated MainMenu

The main menu automatically populates from available games:

```csharp
public sealed partial class MainMenu : Control
{
    [Signal] public delegate void OnStartGameEventHandler(string gameId);
    
    public override void _Ready()
    {
        PopulateGameList();
    }
    
    private void PopulateGameList()
    {
        var gamesContainer = GetNode<VBoxContainer>("GamesContainer");
        
        // Clear existing buttons
        foreach (var child in gamesContainer.GetChildren())
            child.QueueFree();
        
        // Create button for each registered game
        foreach (var game in GameRegistry.Instance.GetAllGames())
        {
            var button = new Button
            {
                Text = game.DisplayName,
                Icon = game.Icon
            };
            button.Pressed += () => EmitSignal(SignalName.OnStartGame, game.GameId);
            gamesContainer.AddChild(button);
        }
    }
}
```

**Key Features:**
- Dynamically generates menu from registry
- No hardcoded game references
- Automatically reflects new games without code changes

### File Structure

```
TennisDough/
├── scripts/
│   └── common/
│       ├── resource/
│       │   └── GameResource.cs          # New: Resource class definition
│       ├── GameRegistry.cs              # New: Registry singleton
│       └── node/
│           ├── GameManager.cs           # Modified: Uses registry
│           └── MainMenu.cs              # Modified: Populates from registry
├── resources/
│   └── games/                           # New: Game resource folder
│       ├── tennis_game.tres             # Tennis game metadata
│       └── block_game.tres              # Block game metadata
└── scenes/
    ├── tennis_game/
    │   └── main_tennis.tscn
    └── block_game/
        └── main_block.tscn
```

### Example Resource Files

**`resources/games/tennis_game.tres`:**
```
[gd_resource type="Resource" script_class="GameResource" load_steps=3 format=3]

[ext_resource type="Script" path="res://scripts/common/resource/GameResource.cs" id="1"]
[ext_resource type="Texture2D" path="res://assets/icons/tennis_icon.png" id="2"]
[ext_resource type="PackedScene" path="res://scenes/tennis_game/main_tennis.tscn" id="3"]

[resource]
script = ExtResource("1")
GameId = "tennis_game"
DisplayName = "Tennis"
Description = "Classic Pong-style tennis game with configurable paddles, ball, and AI opponents."
Icon = ExtResource("2")
GameScene = ExtResource("3")
Category = "Arcade"
SortOrder = 1
```

---

## Reasons for Dropping Game Enums

### 1. **Inflexibility**
- **Current**: Adding a new game requires:
  1. Modifying `Enums.cs` to add enum value
  2. Modifying `GameManager.cs` to add PackedScene export
  3. Modifying menu code to handle new enum
  4. Recompiling entire project
- **Proposed**: Adding a new game requires:
  1. Creating a `.tres` resource file
  2. Automatic discovery at runtime

### 2. **Maintenance Burden**
- Enums create cascading changes across multiple files
- Removing a game leaves orphaned enum values
- Renaming requires coordinated updates
- Merge conflicts in shared enum files

### 3. **Designer Accessibility**
- Current system requires programmer intervention
- Resource system enables designers to add games independently
- All configuration in Godot Editor, no code changes needed

### 4. **Runtime Flexibility**
- Enums are compile-time constructs
- Resources support runtime discovery and hot-reloading
- Enables modding support and DLC extensions
- Facilitates A/B testing of game variants

### 5. **Scalability**
- As game count grows, enum switches become unwieldy
- Resource registry scales to hundreds of games
- Supports categorization and filtering
- Enables lazy loading for performance

### 6. **Consistency with Godot Patterns**
- Aligns with Godot's resource-based workflow
- Leverages `[GlobalClass]` for editor integration
- Matches pattern used in `LevelData.cs`
- Follows Godot best practices

---

## Integration with Existing Architecture

### Phase 1: Parallel Implementation (Non-Breaking)

1. **Create new resource infrastructure** without removing existing enums
2. **Add GameRegistry** as an optional system
3. **Implement resource loading** alongside existing enum-based loading
4. **Update GameManager** to support both approaches

**Benefits:**
- Zero risk to existing functionality
- Gradual migration path
- Easy rollback if issues arise

### Phase 2: Menu Migration

1. **Update MainMenu** to read from GameRegistry
2. **Keep enum-based signal** for backward compatibility
3. **Add mapping layer** from resource IDs to enums

**Benefits:**
- Menu automatically reflects new games
- Existing game code unchanged
- Incremental testing

### Phase 3: Complete Migration

1. **Remove PackedScene exports** from GameManager
2. **Deprecate GameSelection enum** (mark as obsolete)
3. **Update signals** to use string IDs
4. **Remove enum-based logic**

**Benefits:**
- Clean architecture
- No technical debt
- Full flexibility achieved

### Backward Compatibility Strategy

If needed, maintain a bridge between old and new systems:

```csharp
// Temporary mapping for migration period
private static readonly Dictionary<GameSelection, string> EnumToId = new()
{
    { GameSelection.TennisGame, "tennis_game" },
    { GameSelection.BlockGame, "block_game" }
};

private static readonly Dictionary<string, GameSelection> IdToEnum = new()
{
    { "tennis_game", GameSelection.TennisGame },
    { "block_game", GameSelection.BlockGame }
};
```

---

## Benefits Summary

### For Developers
- ✅ Less boilerplate code
- ✅ Fewer merge conflicts
- ✅ Easier to add/remove games
- ✅ Better separation of concerns
- ✅ More maintainable codebase

### For Designers
- ✅ No code changes needed
- ✅ All configuration in Godot Editor
- ✅ Immediate visual feedback
- ✅ Can manage game library independently
- ✅ Safe iteration without breaking builds

### For the Project
- ✅ Improved extensibility
- ✅ Better scalability
- ✅ Mod support foundation
- ✅ Consistent with Godot patterns
- ✅ Future-proof architecture

---

## Conclusion

The Resource Registry System represents a significant architectural improvement that aligns TennisDough with Godot best practices while dramatically improving extensibility and maintainability. By eliminating hardcoded enums and embracing data-driven design, the project gains flexibility without sacrificing simplicity.

The proposed phased migration ensures a safe transition path, allowing the team to validate each step before proceeding. Once complete, adding new minigames becomes a matter of creating a single resource file rather than touching multiple code files—a true win for both developers and designers.

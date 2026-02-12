# TennisDough

TennisDough is an open-source project, created as a means to learn Godot 4.5 (Godot.NET.Sdk/4.5.1, .NET 9.0) by recreating Atari's Pong, and then Atari's Breakout, but then I accidentally slipped and fell and now we are making a few more little minigames along the way... The game is named after a combination of Tennis For Two and Godot (GAH'DOUGH).

## License & Acknowledgements

This project is under GNU Affero General Public License v3.0 [License](LICENSE.txt) and uses assets from Kenney.nl under the CC0 1.0 Universal (CC0 1.0) Public Domain Dedication [Kenney Assets License](Asset_License.txt).

- This is inspired by the ["20 Game Challenge"](https://20_games_challenge.gitlab.io/), but is not affiliated with it in any way.
- Though edited, some graphics, like menu, created by [piiixl](https://piiixl.itch.io/game-ui) under License: [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/deed.en).
- Incorporates a (work in progress) modification of the [space worm theme by softwave](https://softwave.itch.io/godot-retro-theme-space-worm) under the Unlicense.

---

## About TennisDough

TennisDough is a collection of classic arcade-style minigames built with Godot 4 and C#. Currently, it includes recreations of **Pong** (Tennis) and **Breakout** (Block), both featuring customizable gameplay options. The project is designed with a modular "Game Pack" architecture, allowing multiple minigames to be loaded dynamically from a central main menu.

This game is provided as is, without warranty of any kind, and is free to use and modify under the terms of the AGPLv3 license. Feel free to learn from it, modify it, and share it with others!

### Current Development Status

| Game Pack | Status | Description |
|-----------|--------|-------------|
| **Main Menu** | Work in Progress | Central hub for game selection; functional but minimal |
| **Tennis** | Feature Complete | Pong-style game with full gameplay, AI, and customization |
| **Block** | Mostly Complete | Breakout-style game; core gameplay works, menu integration pending |

---

## Project Architecture

### Technology Stack
- **Engine**: Godot 4.5 (4.6 compatible)
- **Language**: C# (.NET 9.0)
- **Resolution**: 1920×1080, viewport stretch mode

### Directory Structure

```
TennisDough/
├── assets/                    # Art, audio, shaders, themes
│   ├── fonts/                 # Kenney Future fonts
│   ├── interface/             # UI sprites and icons
│   ├── resources/             # Game packs, audio events, level data
│   ├── shaders/               # Background and CRT effect shaders
│   ├── sounds/                # Sound effects (OGG format)
│   └── themes/                # Godot UI themes (Space Worm)
├── scenes/                    # Scene files (.tscn)
│   ├── main.tscn              # Root scene (GameManager)
│   ├── tennis_game/           # Tennis game scenes
│   └── block_game/            # Block game scenes
└── scripts/                   # C# source code
    ├── common/                # Shared systems and utilities
    ├── tennis_game/           # Tennis-specific scripts
    └── block_game/            # Block-specific scripts
```

---

## Core Systems

### GameManager (`scripts/common/node/GameManager.cs`)

The `GameManager` is the root `Control` node and global entry point. It initializes and provides static access to core subsystems:

| Subsystem | Class | Purpose |
|-----------|-------|---------|
| `Audio` | `AudioManager` | Multi-channel audio playback with mute/volume control |
| `Monitor` | `GameMonitor` | Game state tracking with event-driven state changes |
| `Package` | `PackManager` | Dynamic loading of game packs from resources |
| `Settings` | `SettingsManager` | User preferences (audio, etc.) |

The GameManager also manages:
- CRT overlay shaders (default, paused, boot effects)
- Scene transitions between main menu and game packs
- Global pause handling via `PauseWatcher`

### Game State System (`scripts/common/GameMonitor.cs`)

Centralized state machine with the following states:

```
MainMenu → Loading → InGame ↔ Paused → GameOver
```

States are defined in `Enums.cs`:
- `MainMenu` - Main menu is displayed
- `GameMenu` - In-game menu/settings overlay
- `InGame` - Active gameplay
- `Paused` - Game paused
- `GameOver` - Game ended, showing results
- `Loading` - Transitioning between scenes

### Pack System (Game Packs)

Games are loaded as "packs" - self-contained game modules defined by a `GamePack` resource:

**GamePack Resource** (`scripts/common/resource/GamePack.cs`):
- `GameIcon` - Texture for menu display
- `GameName` - Display name
- `GameScene` - PackedScene to instantiate
- `GameDescription` - Description text

**PackBase** (`scripts/common/node/base/PackBase.cs`):
Abstract base class that all game main scenes must extend. Provides:
- Access to `AudioManager` and `GameMonitor`
- `OnScoreSubmission` event for high score integration

**PackManager** (`scripts/common/PackManager.cs`):
- Scans `res://assets/resources/packs/` for `.tres` GamePack files
- Maintains a dictionary of available packs
- Handles instantiation and loading via `OnPackLoaded` event

### Audio System (`scripts/common/AudioManager.cs`)

Three-channel audio system:
- **Channel 1 & 2**: Sound effects (can play simultaneously)
- **Channel Music**: Background music

Features:
- Per-channel mute toggle
- Volume control with linear-to-dB conversion
- `AudioEvent` resource type for defining clips

### Score System

**Score** (`scripts/common/Score.cs`):
- Tracks points for individual controllers
- Updates a linked Label for HUD display

**ScoreManager** (`scripts/common/ScoreManager.cs`):
- Persists high scores to `.scores` config files
- Stores under `user://saves/{pack_name}/`
- Default high score table with placeholder names

### Input System

Configured in `project.godot`:

| Action | Keys |
|--------|------|
| `p1_move_up` | W, A, Up Arrow |
| `p1_move_down` | S, D, Down Arrow |
| `p2_move_up` | I, J, Numpad 8 |
| `p2_move_down` | K, L, Numpad 2 |
| `pause_game` | ESC |

### Utility Extensions (`scripts/common/Utils.cs`)

- `AddNode<T>()` - Creates and adds a child node of type T
- `InstanceScene()` - Instantiates a PackedScene as a child

---

## Tennis Game (Pong Clone)

A two-player tennis/pong game with AI support and extensive customization.

### Components

**MainTennis** (`scripts/tennis_game/MainTennis.cs`) - `PackBase`
- Orchestrates game state (start, pause, reset, game over)
- Manages controllers, timers, and scoring
- Handles rainbow victory effect animation
- Emits scores on game completion

**MenuTennis** (`scripts/tennis_game/MenuTennis.cs`) - `Control`
- Full settings UI with sliders and color pickers
- Player type selection (Human P1, Human P2, AI)
- Paddle size/speed configuration per player
- Ball speed, max score, and time limit settings

**Paddle** (`scripts/tennis_game/node/Paddle.cs`) - `CharacterBody2D`
- Physics-based movement with configurable friction
- Dynamic resizing (affects collision and visual)
- Color customization

**BallTennis** (`scripts/tennis_game/node/BallTennis.cs`) - `BallBase`
- Bounce physics with speed acceleration over time
- Particle trail effect
- Out-of-bounds detection for scoring
- Audio feedback on collisions

### Controller Pattern

**IController** (`scripts/tennis_game/interface/IController.cs`):
```csharp
interface IController {
    Direction GetInputDirection();
    void Update();
    void Attach() / void Detach();  // Score event subscription
}
```

**Implementations**:
- `PaddlePlayer` - Reads from input actions based on player prefix (`p1_` or `p2_`)
- `PaddleAI` - Tracks ball position with randomized delay/error margin for imperfect AI

### Game Flow

1. Menu starts visible with configuration options
2. Player selects controller types and settings, presses Play
3. `OnGameStart` fires with all parameters
4. `MainTennis` creates controllers, configures entities, starts timer
5. Game runs until max score or time reached
6. Victory screen displays, game resets after delay

---

## Block Game (Breakout Clone)

A single-player breakout-style game with destructible blocks.

### Components

**MainBlock** (`scripts/block_game/MainBlock.cs`) - `PackBase`
- Manages paddle controller and ball
- Handles block hit events and scoring
- Supports both random and designed levels

**PaddleBlock** (`scripts/block_game/node/PaddleBlock.cs`) - `CharacterBody2D`
- Horizontal movement (left/right)
- Similar physics to Tennis paddle

**BallBlock** (`scripts/block_game/node/BallBlock.cs`) - `BallBase`
- Bounce physics optimized for breakout gameplay
- Emits `OnBlockHit` with the struck block
- Tracks out-of-bounds (ball lost)

**Block** (`scripts/block_game/node/Block.cs`) - `StaticBody2D`
- Configurable hit points (1-4)
- Color changes based on remaining HP
- Particle effects on hit and destruction
- Fade-out animation on destruction

**BlockCollection** (`scripts/block_game/node/BlockCollection.cs`) - `Node2D`
- Manages all blocks in a level
- Generates levels from `LevelData` resource or randomly
- Block grid: 24 columns, variable rows
- Block size: 24×18 pixels with 2px spacing

**BlockColorMap** (`scripts/block_game/BlockColorMap.cs`):
- Singleton mapping hit points to colors
- Dynamically generates particle materials per HP value

### Level Data (`scripts/block_game/resource/LevelData.cs`)

Resource for designing levels as string grids:
- 24 characters per row
- `0` = empty, `1-4` = block with that many hit points
- String is newline-separated for multiple rows

### Controller Pattern

**IController** (`scripts/block_game/interface/IController.cs`):
Simplified compared to Tennis (no score events for now):
```csharp
interface IController {
    Direction GetInputDirection();
    void Update();
}
```

**Implementation**:
- `PaddlePlayer` - Maps `p1_move_up/down` to left/right movement

---

## Visual Effects

### CRT Shader System

Three shader materials for the CRT overlay:
- **Default** - Standard scanline effect during gameplay
- **Paused** - Modified effect when game is paused
- **Boot** - Startup/main menu effect

### Background Shader

Animated procedural background with:
- Configurable colors (main, secondary, back)
- Spin and movement animation
- Pixel filtering for retro aesthetic

### Particle Effects

- **Ball trails** - GPU particles following the ball
- **Block hit effects** - Burst particles on block damage
- Color-matched to entity colors

---

## Themes

Uses a modified "Space Worm" theme providing consistent styling for:
- Buttons (normal, hover, pressed states)
- Checkboxes and sliders
- Panels and separators
- Scroll bars
- Text styling and fonts (Kenney Future family)
# TennisDough - Architectural Documentation

## Table of Contents
1. [Overview](#overview)
2. [Overall Architectural Layout](#overall-architectural-layout)
3. [Game Logic Flow](#game-logic-flow)
4. [Key Modules and Their Interactions](#key-modules-and-their-interactions)
5. [Setup and Configuration](#setup-and-configuration)
6. [System Structure Layout](#system-structure-layout)
7. [Technical Implementation Details](#technical-implementation-details)
8. [Extending the Architecture](#extending-the-architecture)

---

## Overview

**TennisDough** is an educational game project built with **Godot 4.6** (C# with .NET 8.0/9.0) that recreates classic arcade games. The project currently includes two game modes:

- **Tennis Game**: A Pong-inspired two-player tennis game with configurable paddles, ball, and game rules
- **Block Game**: A Breakout-style single-player game (partially implemented)

### Key Characteristics
- **Technology Stack**: Godot 4.6, C# (.NET 8.0/9.0), Godot.NET.Sdk/4.6.0
- **Resolution**: 1920x1080 (viewport with keep_height aspect ratio)
- **Rendering**: GL Compatibility mode with CRT shader effects
- **Architecture Pattern**: Component-based with event-driven communication
- **License**: GNU Affero General Public License v3.0

---

## Overall Architectural Layout

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         main.tscn                            │
│                    (Entry Point Scene)                       │
│  ┌────────────────────────────────────────────────────────┐ │
│  │              GameManager (Singleton)                    │ │
│  │  - GameMonitor (State Management)                       │ │
│  │  - PauseWatcher (Input Handling)                        │ │
│  │  - MainMenu (Scene Management)                          │ │
│  └────────────────────────────────────────────────────────┘ │
│           │                                │                 │
│           ▼                                ▼                 │
│  ┌─────────────────┐           ┌─────────────────┐         │
│  │  TennisGame     │           │   BlockGame     │         │
│  │  Main Scene     │           │   Main Scene    │         │
│  └─────────────────┘           └─────────────────┘         │
└─────────────────────────────────────────────────────────────┘
```

### Architectural Layers

1. **Entry Layer**: `main.tscn` - The root scene that initializes GameManager
2. **Management Layer**: `GameManager` and `GameMonitor` - Handle state and scene transitions
3. **Game Layer**: `MainTennis` and `MainBlock` - Game-specific controllers
4. **UI Layer**: `MenuTennis` and `MenuBlock` - Configuration and game settings
5. **Entity Layer**: `Paddle`, `Ball`, `Block` - Game entities with physics
6. **Common Layer**: Shared utilities, audio, scoring, and pause management

### Architectural Patterns Used

| Pattern | Implementation | Purpose |
|---------|---------------|---------|
| **Singleton** | `GameMonitor` via `GameManager.Monitor` | Centralized game state management |
| **Dependency Injection** | Constructor parameters (e.g., `AudioManager`) | Loose coupling between components |
| **Interface Segregation** | `IController` (separate for Tennis and Block) | Abstracted player control |
| **Observer Pattern** | C# events and Godot signals | Event-driven communication |
| **Composition** | Components like `Score`, `AudioManager` | Flexible component assembly |
| **Factory (Implicit)** | Controller creation in `GameStart()` | Runtime polymorphism |

---

## Game Logic Flow

### 1. Application Initialization Flow

```
Application Start
    ↓
main.tscn loads
    ↓
GameManager._Ready()
    ├→ Initialize GameMonitor (Singleton)
    ├→ Instantiate MainMenu scene
    ├→ Create PauseWatcher
    └→ Subscribe to OnStartGame event
        ↓
MainMenu displays
    ├→ User selects game mode (Tennis/Block)
    └→ Emits OnStartGame(GameSelection)
        ↓
GameManager.HandleStartGame()
    └→ (TODO) Load selected game scene
```

### 2. Tennis Game Initialization Flow

```
TennisGameMain scene loads
    ↓
MainTennis._EnterTree()
    ├→ Create AudioManager
    └→ Create PauseWatcher
        ↓
MainTennis._Ready()
    ├→ Inject AudioManager into Ball and Menu
    ├→ Create Score objects (Player 1 & 2)
    ├→ Subscribe to events:
    │   ├─ Menu.OnGameStart → MainTennis.GameStart()
    │   ├─ Menu.OnGameCancel → MainTennis.GamePause()
    │   ├─ PauseWatcher.OnTogglePause → MainTennis.GamePause()
    │   └─ GameTimer.Timeout → MainTennis.TimerUpdate()
    └→ Wait for menu input (game in paused state)
        ↓
User configures game in menu and clicks "Start"
    ↓
MenuTennis.OnGameStart emits with parameters:
    - Player types (Human/AI for each paddle)
    - Paddle sizes, speeds, colors
    - Ball size, speed, color
    - Max score and time limit
        ↓
MainTennis.GameStart() executes
    ├→ Detach existing controllers (if any)
    ├→ Create new controllers based on player types:
    │   ├─ PaddlePlayer (for human players)
    │   └─ PaddleAI (for AI players)
    ├→ Attach controllers to Ball.OnOutOfBounds event
    ├→ Configure paddle properties (size, speed, color)
    ├→ Configure ball properties (size, color)
    ├→ Set game rules (max score, time limit)
    ├→ Reset game state (scores, timer, positions)
    ├→ Start game timer
    └→ Enable ball physics
```

### 3. Tennis Game Loop (In-Game Mechanics)

```
MainTennis._Process(delta) - Called every frame
    ↓
Check game state
    ├→ If GameOver or Paused: Skip processing
    └→ If Playing:
        ↓
    Controller1.Update()
        ├→ GetInputDirection() (from PaddlePlayer or PaddleAI)
        └→ Paddle.Move(direction)
            ↓
    Controller2.Update()
        ├→ GetInputDirection()
        └→ Paddle.Move(direction)
            ↓
BallTennis._PhysicsProcess(delta) - Godot physics
    ├→ Apply velocity and friction
    ├→ MoveAndSlide() (built-in Godot physics)
    └→ Check collisions:
        ├─ If collides with Paddle:
        │   ├─ Bounce ball
        │   ├─ Increase speed (up to 0.8x multiplier)
        │   └─ Play "hit" sound
        └─ If collides with Wall:
            ├─ Bounce ball
            └─ Play "hit" sound
                ↓
Ball.CheckBoundary() - Called each frame
    ↓
Check if ball is outside viewport
    ├→ If out of left side:
    │   └─ Emit OnOutOfBounds(isLeftSide: true)
    └→ If out of right side:
        └─ Emit OnOutOfBounds(isLeftSide: false)
            ↓
IController.OnPointScore(isLeftSide)
    ├→ If point scored against opponent:
    │   ├─ Score.AddPoint()
    │   └─ Update score display
    └→ Reset ball to center
        ↓
Timer updates every second
    ↓
MainTennis.TimerUpdate()
    ├→ Check if max score reached
    ├→ Check if time expired
    └→ If either condition met:
        └→ Call GameOver()
```

### 4. Win/Lose Conditions

#### Tennis Game Win Conditions
- **Score-based**: First player to reach `maxScore` (default: 255)
- **Time-based**: Player with highest score when `maxTimeInSeconds` expires (default: 9999)
- **Tie**: If scores are equal when time expires

#### Block Game Win/Lose Conditions (Partially Implemented)
- **Win**: All blocks destroyed
- **Lose**: Ball goes out of bounds (bottom) multiple times
- **Current Status**: Only block hit detection is implemented

### 5. Game Over Flow

```
GameOver() triggers
    ↓
Set _isGameOver = true
    ↓
Determine winner
    ├→ Compare scores
    └→ Set middleScreenLabel text:
        ├─ "Player 1 Wins!"
        ├─ "Player 2 Wins!"
        └─ "It's a Tie!"
            ↓
Toggle rainbow effect (6 seconds)
    ├→ Randomly change colors every 0.1s
    │   ├─ Paddles
    │   ├─ Ball
    │   ├─ UI elements (scores, timer, labels)
    │   └─ Background elements
    └→ After 6 seconds, restore white colors
        ↓
Show final message for 6 seconds
    ↓
Hide message and show menu
    ↓
GameReset()
    ├→ Stop timer
    ├→ Reset paddle positions
    ├→ Reset ball position and velocity
    ├→ Reset scores to 0
    └→ Reset time to 0
```

### 6. Pause Flow

```
User presses ESC key
    ↓
PauseWatcher detects input
    ↓
Emit OnTogglePause event
    ↓
MainTennis.GamePause()
    ↓
Check current state
    ├→ If currently playing:
    │   ├─ Pause game timer
    │   ├─ Disable ball physics
    │   ├─ Show menu
    │   └─ Set _isPaused = true
    └→ If currently paused:
        ├─ Resume game timer
        ├─ Enable ball physics
        ├─ Hide menu
        └─ Set _isPaused = false
```

---

## Key Modules and Their Interactions

### Module Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     Common Modules                           │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │GameMonitor  │  │ AudioManager │  │    Score     │       │
│  │  (State)    │  │   (Sound)    │  │  (Display)   │       │
│  └─────────────┘  └──────────────┘  └──────────────┘       │
│  ┌─────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │PauseWatcher │  │    Utils     │  │    Enums     │       │
│  │  (Input)    │  │  (Helpers)   │  │    (Types)   │       │
│  └─────────────┘  └──────────────┘  └──────────────┘       │
└─────────────────────────────────────────────────────────────┘
                            │
          ┌─────────────────┴─────────────────┐
          ▼                                    ▼
┌──────────────────────┐          ┌──────────────────────┐
│   Tennis Game        │          │    Block Game        │
│   ┌──────────────┐   │          │   ┌──────────────┐   │
│   │ MainTennis   │   │          │   │  MainBlock   │   │
│   │ (Controller) │   │          │   │ (Controller) │   │
│   └──────────────┘   │          │   └──────────────┘   │
│   ┌──────────────┐   │          │   ┌──────────────┐   │
│   │ MenuTennis   │   │          │   │  MenuBlock   │   │
│   │    (UI)      │   │          │   │    (UI)      │   │
│   └──────────────┘   │          │   └──────────────┘   │
│   ┌──────────────┐   │          │   ┌──────────────┐   │
│   │ IController  │   │          │   │ IController  │   │
│   │ (Interface)  │   │          │   │ (Interface)  │   │
│   │ ├─PaddlePlayer│  │          │   │ └─PaddlePlayer│  │
│   │ └─PaddleAI   │   │          │   └──────────────┘   │
│   └──────────────┘   │          │   ┌──────────────┐   │
│   ┌──────────────┐   │          │   │    Block     │   │
│   │   Paddle     │   │          │   │ BlockCollect │   │
│   │  BallTennis  │   │          │   │ PaddleBlock  │   │
│   │  (Entities)  │   │          │   │  BallBlock   │   │
│   └──────────────┘   │          │   └──────────────┘   │
└──────────────────────┘          └──────────────────────┘
```

### Module Descriptions

#### 1. GameMonitor (Common)
**Purpose**: Centralized game state management independent of scene tree

**Properties**:
- `CurrentState`: Current game state (MainMenu, InGame, Paused, GameOver)
- `PriorState`: Previous game state for state restoration

**Events**:
- `OnGameStateChanged`: Fired when state changes

**Methods**:
- `ChangeState(GameState)`: Transitions between states and notifies listeners

**Interactions**:
- Accessed via `GameManager.Monitor` singleton
- Observed by UI components for state-based visibility
- Updated by GameManager on pause toggle

---

#### 2. GameManager (Common)
**Purpose**: Scene orchestrator and entry point controller

**Properties**:
- `Monitor`: Static GameMonitor singleton
- `_mainMenuScene`, `_blockGameScene`, `_tennisGameScene`: Packed scene references

**Events Subscribed**:
- `MainMenu.OnStartGame`: Handles game mode selection
- `PauseWatcher.OnTogglePause`: Manages pause state

**Methods**:
- `HandleStartGame(GameSelection)`: Loads selected game scene (TODO)
- `HandleTogglePause()`: Toggles pause if not in MainMenu

**Interactions**:
- Root manager for all game modes
- Instantiates MainMenu on ready
- Provides GameMonitor singleton to all components

---

#### 3. AudioManager (Common)
**Purpose**: Centralized audio playback with dual-channel support

**Properties**:
- `_audioPlayerChannel1`, `_audioPlayerChannel2`: AudioStreamPlayer nodes
- `_audioClips`: Dictionary of registered audio clips
- Volume controls for each channel

**Methods**:
- `AddAudioClip(name, AudioStream)`: Register audio clip
- `PlayAudioClip(name, channel)`: Play sound on specified channel
- `SetChannelVolume(channel, volume)`: Adjust volume
- `LinearToDb(linear)`: Convert linear volume to decibels

**Registered Clips** (Tennis):
- `hit`: Ball-paddle collision
- `score`: Point scored
- `out_of_bounds`: Ball exit

**Interactions**:
- Created in each game's `_EnterTree()`
- Injected into Ball and Menu via `Inject(AudioManager)`
- Singleton pattern enforced (throws if multiple instances)

---

#### 4. Score (Common)
**Purpose**: Score management and display binding

**Properties**:
- `CurrentScore`: Current score value (byte, max 255)
- `_scoreLabel`: Godot Label reference

**Methods**:
- `AddPoint()`: Increment score and update display
- `Reset()`: Set score to 0

**Display Format**: `D8` (8-digit zero-padded, e.g., "00000042")

**Interactions**:
- Created per player in MainTennis (2 instances)
- Created once in MainBlock (single player)
- Updated by IController implementations when points scored

---

#### 5. PauseWatcher (Common)
**Purpose**: Global input handler for pause functionality

**Events**:
- `OnTogglePause`: Fired when ESC key pressed

**Input Actions**:
- `pause_game`: Mapped to ESC key (physical keycode 4194305)

**Interactions**:
- Created in GameManager and each game main
- Subscribed to by MainTennis and MainBlock
- Independent from menu system

---

#### 6. Utils (Common)
**Purpose**: Extension methods and helper utilities

**Methods**:
- `AddNode<T>()`: Generic node instantiation and parenting
- `InstantScene(PackedScene)`: Scene instantiation with automatic parenting

**Usage Examples**:
```csharp
_audioManager = this.AddNode<AudioManager>();
_mainMenu = this.InstantScene(_mainMenuScene) as MainMenu;
```

---

#### 7. MainTennis (Tennis Game)
**Purpose**: Main game controller for Tennis mode

**Responsibilities**:
1. Game state management (playing, paused, game over)
2. Controller instantiation (Player/AI for each paddle)
3. Game timer and score tracking
4. Win condition checking
5. Visual effects (rainbow on game over)

**Key Fields**:
- `_menu`: MenuTennis reference
- `_paddleP1`, `_paddleP2`: Paddle entity references
- `_ball`: BallTennis entity reference
- `_controller1`, `_controller2`: IController implementations
- `_scoreP1`, `_scoreP2`: Score objects
- `_maxScore`, `_maxTimeInSeconds`: Win condition parameters

**Event Subscriptions**:
- `Menu.OnGameStart`: Configure and start game
- `Menu.OnGameCancel`: Pause game
- `PauseWatcher.OnTogglePause`: Toggle pause state
- `GameTimer.Timeout`: Update timer each second

**Game Loop**:
- Calls `_controller1.Update()` and `_controller2.Update()` each frame
- Controllers read input and move paddles
- Ball physics handled by BallTennis entity

---

#### 8. MenuTennis (Tennis Game)
**Purpose**: Configuration UI for Tennis mode

**Events Emitted**:
- `OnGameStart`: With 12 parameters (player types, sizes, speeds, colors, rules)
- `OnGameCancel`: Pause request

**Configuration Options**:
- **Player Selection**: Player1, Player2, or AI for each paddle
- **Paddle Properties**: Size (1-255), Speed (100-1000), Color (RGB picker)
- **Ball Properties**: Size (1-255), Speed (0-1000), Color (RGB picker)
- **Game Rules**: Max Score (1-255), Time Limit (1-9999 seconds)

**UI Elements**:
- Sliders for numeric values
- Color pickers for entity colors
- Dropdown for player type selection
- Buttons: Start, Resume, Reset, Quit

**Audio Integration**:
- Receives AudioManager via `Inject()`
- Plays button press sounds

---

#### 9. IController (Tennis Game)
**Purpose**: Abstract interface for paddle control

**Properties**:
- `Paddle`: Controlled paddle reference
- `BallTennis`: Ball reference for event subscription
- `Score`: Score object reference
- `IsLeftSide`: Left/right paddle identification

**Methods**:
- `GetInputDirection()`: Abstract method for input (implemented by PaddlePlayer/PaddleAI)
- `Update()`: Default implementation - calls GetInputDirection() and moves paddle
- `Attach()`: Subscribe to Ball.OnOutOfBounds event
- `Detach()`: Unsubscribe from Ball.OnOutOfBounds event
- `OnPointScore(isLeftSide)`: Score point if opponent side

**Implementations**:

**PaddlePlayer**:
- Reads Godot input actions (`p1_move_up`, `p1_move_down`, `p2_move_up`, `p2_move_down`)
- Supports two player types (Player1 or Player2 input scheme)
- Returns Direction enum based on input

**PaddleAI**:
- Tracks ball Y position
- Compares to paddle Y position with error margin
- Adds randomized delay for "imperfect" play
- Returns Direction.Up or Direction.Down to follow ball

---

#### 10. Paddle (Tennis Game)
**Purpose**: Physics-based paddle entity

**Type**: `CharacterBody2D` (Godot physics node)

**Properties**:
- `_speed`: Movement speed (default 700, configurable)
- `_size`: Paddle height multiplier (1-255)
- `_friction`: Velocity dampening (0.9)

**Methods**:
- `Move(Direction)`: Apply velocity in given direction
- `Resize(byte)`: Adjust paddle height
- `ChangeSpeed(uint)`: Adjust movement speed
- `AdjustColor(Color)`: Change paddle color
- `ResetPosition()`: Return to starting position

**Physics**:
- Uses `MoveAndSlide()` for collision detection
- Applies friction each physics frame
- Clamped to viewport boundaries

---

#### 11. BallTennis (Tennis Game)
**Purpose**: Ball entity with bouncing physics

**Type**: `CharacterBody2D` (Godot physics node)

**Properties**:
- `_initialSpeed`: Starting velocity (default 300)
- `_speedFactor`: Acceleration multiplier (increases on paddle hit, max 0.8)
- `_maxSpeed`: Velocity clamp (12000)

**Events Emitted**:
- `OnOutOfBounds(bool isLeftSide)`: Ball exits viewport

**Methods**:
- `ResetBall()`: Return to center with random angle
- `AdjustSize(byte)`: Scale ball sprite
- `AdjustColor(Color)`: Change ball color
- `ToggleEnable()`: Enable/disable physics processing
- `Inject(AudioManager)`: Receive audio manager

**Physics**:
- Random initial direction (-135° to 135°, excluding -45° to 45°)
- Bounces off walls and paddles
- Increases speed on paddle collision
- Plays sounds on collision

**Boundary Detection**:
- Checks if X position is outside viewport each frame
- Emits `OnOutOfBounds` to trigger scoring

---

#### 12. MainBlock (Block Game)
**Purpose**: Main game controller for Block mode (Partial Implementation)

**Current State**: Basic structure with limited functionality

**Implemented**:
- AudioManager integration
- PauseWatcher creation
- Block hit detection
- Score tracking
- Level generation (random or from LevelData resource)

**Not Implemented**:
- Full menu integration
- Out of bounds handling
- Timer system
- Game over conditions
- Menu event handlers

**Event Subscriptions**:
- `Ball.OnBlockHit`: Handles block destruction and scoring

---

#### 13. Block Entities (Block Game)

**Block**:
- `CharacterBody2D` with collision detection
- `HitCount`: Durability (hits required to destroy)
- `OnBlockHit()`: Decrements hit count, queues for deletion if 0
- Color-coded by hit count (via BlockColorMap)

**BlockCollection**:
- Manages grid of blocks
- `GenerateLevel()`: Creates random block layout
- `GenerateLevel(LevelData)`: Creates blocks from resource definition
- Grid positioning and spacing

**PaddleBlock**:
- Similar to Tennis Paddle but horizontal movement only
- Single player control (no left/right side distinction)

**BallBlock**:
- Similar to BallTennis but with automatic downward start
- Emits `OnBlockHit(Block)` on collision
- Lower speed factor (0.015 vs 0.04)

---

### Module Interaction Diagram

```
User Input (ESC) → PauseWatcher → MainTennis.GamePause()
                                          ↓
                                  Toggle menu visibility
                                  Toggle ball physics
                                  Pause/resume timer

User Input (Menu) → MenuTennis.OnGameStart → MainTennis.GameStart()
                                                      ↓
                                          Create Controllers
                                          Configure entities
                                          Start game timer

Ball Movement → Collision → AudioManager.PlayAudioClip()
                    ↓
            Out of Bounds → Ball.OnOutOfBounds
                    ↓
            IController.OnPointScore
                    ↓
            Score.AddPoint()
                    ↓
            MainTennis.TimerUpdate() checks score
                    ↓
            GameOver() if condition met
```

---

## Setup and Configuration

### Prerequisites

1. **Godot Engine**: Version 4.6 or later
   - Download from: https://godotengine.org/
   - Must support .NET/C# (download the .NET version)

2. **.NET SDK**: Version 8.0 or 9.0
   - Download from: https://dotnet.microsoft.com/download
   - Required for C# script compilation

3. **Operating System**: Windows, Linux, or macOS
   - Cross-platform compatibility via Godot and .NET

### Installation Steps

#### 1. Clone the Repository
```bash
git clone https://github.com/Baerthe/TennisDough.git
cd TennisDough
```

#### 2. Open in Godot Editor
```bash
# Open Godot and use "Import" button
# Navigate to project.godot file
# Click "Import & Edit"
```

Or via command line:
```bash
godot --path . --editor
```

#### 3. Build C# Project
The project uses Godot.NET.Sdk/4.6.0. Building happens automatically in Godot, but you can also build manually:

```bash
dotnet build TennisDough.csproj
```

#### 4. Run the Game
- In Godot Editor: Press F5 or click "Run Project" button
- From command line:
```bash
godot --path .
```

### Configuration Files

#### project.godot
Main project configuration file. Key settings:

```ini
[application]
config/name="TennisDough"
run/main_scene="uid://c3lck2cuuntgh"  # scenes/main.tscn
config/features=PackedStringArray("4.6", "C#", "GL Compatibility")

[display]
window/size/viewport_width=1920
window/size/viewport_height=1080
window/size/borderless=true
window/stretch/mode="viewport"
window/stretch/aspect="keep_height"

[input]
# Player 1 controls: W/A or Up Arrow
p1_move_up={...}
p1_move_down={...}
# Player 2 controls: I/J or Numpad 8
p2_move_up={...}
p2_move_down={...}
# System controls
pause_game={...}  # ESC key
```

#### TennisDough.csproj
.NET project configuration:

```xml
<Project Sdk="Godot.NET.Sdk/4.6.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <TargetFramework Condition=" '$(GodotTargetPlatform)' == 'android' ">net9.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
  </PropertyGroup>
</Project>
```

### Input Mapping

Configure in Godot: Project → Project Settings → Input Map

| Action | Keys | Purpose |
|--------|------|---------|
| `p1_move_up` | W, A, Up Arrow | Player 1 move up |
| `p1_move_down` | S, D, Down Arrow | Player 1 move down |
| `p2_move_up` | I, J, Numpad 8 | Player 2 move up |
| `p2_move_down` | K, L, Numpad 2 | Player 2 move down |
| `pause_game` | ESC | Pause/unpause game |

**Edge Case**: Player 1 has two input schemes (WASD or Arrows) to support different keyboard layouts.

### Physics Layers

Configure in Godot: Project → Project Settings → Layer Names → 2D Physics

| Layer | Name | Usage |
|-------|------|-------|
| 1 | Foreground 1 | Primary entities (paddles, ball) |
| 2 | Foreground 2 | Secondary entities |
| 3 | Objects | Interactive objects (blocks) |

### Asset Dependencies

Assets are included in the repository under `assets/`:
- **Fonts**: Custom fonts for UI
- **Sounds**: Audio clips for game events
- **Shaders**: CRT effect shader for retro aesthetic
- **Themes**: UI theme resources
- **Interface**: UI sprites and textures

**External Assets**:
- Kenney.nl assets (CC0 1.0 Universal)
- piiixl game UI (CC BY 4.0)
- softwave space worm theme (Unlicense)

### Edge Cases and Common Issues

#### 1. C# Build Errors
**Issue**: "Could not find .NET SDK"

**Solution**:
- Ensure .NET 8.0/9.0 SDK is installed
- Restart Godot after installing .NET
- Check Godot → Editor → Editor Settings → Mono → Builds → Build Tool

#### 2. Input Not Responding
**Issue**: Paddle doesn't move with keyboard input

**Solution**:
- Verify input actions in Project Settings
- Check that controller is created (MainTennis.GameStart must be called)
- Ensure game is not paused (_isPaused = false)

#### 3. Audio Not Playing
**Issue**: No sound on collisions

**Solution**:
- Verify AudioManager is created in _EnterTree()
- Check audio clips are registered via AddAudioClip()
- Verify audio files are imported correctly in Godot

#### 4. Scene Loading Issues
**Issue**: Game doesn't start or blank screen

**Solution**:
- Verify main_scene UID in project.godot matches main.tscn
- Check scene dependencies are not broken
- Rebuild C# project

#### 5. Performance Issues
**Issue**: Game runs slowly or stutters

**Solution**:
- Ensure GL Compatibility mode is enabled (not Forward+)
- Disable CRT shader if hardware is limited
- Check for infinite loops in custom code

---

## System Structure Layout

### Directory Structure

```
TennisDough/
├── assets/                     # Game assets
│   ├── fonts/                  # Custom fonts
│   ├── interface/              # UI sprites
│   ├── resources/              # Game resources (LevelData, etc.)
│   ├── shaders/                # Visual effects shaders
│   ├── sounds/                 # Audio files
│   └── themes/                 # UI themes
├── scenes/                     # Godot scene files
│   ├── main.tscn               # Entry point scene
│   ├── tennis_game/            # Tennis game scenes
│   │   ├── tennis_game_main.tscn
│   │   ├── tennis_game_menu.tscn
│   │   ├── tennis_game_paddle.tscn
│   │   └── tennis_game_ball.tscn
│   └── block_game/             # Block game scenes
│       ├── block_game_main.tscn
│       ├── block_game_menu.tscn
│       ├── block_game_paddle.tscn
│       ├── block_game_ball.tscn
│       └── block_game_block.tscn
├── scripts/                    # C# scripts
│   ├── common/                 # Shared scripts
│   │   ├── node/               # Node-based components
│   │   │   ├── AudioManager.cs
│   │   │   ├── GameManager.cs
│   │   │   ├── MainMenu.cs
│   │   │   └── PauseWatcher.cs
│   │   ├── Enums.cs            # Game enumerations
│   │   ├── GameMonitor.cs      # State management
│   │   ├── Score.cs            # Score component
│   │   └── Utils.cs            # Helper utilities
│   ├── tennis_game/            # Tennis game scripts
│   │   ├── interface/
│   │   │   └── IController.cs  # Controller interface
│   │   ├── node/
│   │   │   ├── BallTennis.cs
│   │   │   └── Paddle.cs
│   │   ├── MainTennis.cs       # Tennis game controller
│   │   ├── MenuTennis.cs       # Tennis menu UI
│   │   ├── PaddleAI.cs         # AI implementation
│   │   └── PaddlePlayer.cs     # Player implementation
│   └── block_game/             # Block game scripts
│       ├── interface/
│       │   └── IController.cs  # Controller interface
│       ├── node/
│       │   ├── BallBlock.cs
│       │   ├── Block.cs
│       │   ├── BlockCollection.cs
│       │   └── PaddleBlock.cs
│       ├── resource/
│       │   └── LevelData.cs    # Level definition resource
│       ├── BlockColorMap.cs    # Block color mapping
│       ├── MainBlock.cs        # Block game controller
│       ├── MenuBlock.cs        # Block menu UI
│       ├── PaddleAI.cs         # AI implementation (TODO)
│       └── PaddlePlayer.cs     # Player implementation
├── .gitignore                  # Git ignore file
├── .editorconfig               # Editor configuration
├── icon.svg                    # Project icon
├── project.godot               # Godot project configuration
├── TennisDough.csproj          # .NET project file
├── TennisDough.sln             # Visual Studio solution
├── LICENSE.txt                 # License file
├── Asset_License.txt           # Asset licenses
└── README.md                   # Project readme
```

### Scene Hierarchy

#### main.tscn (Entry Point)
```
Control (Main)
├── SubViewportContainer (CRT effect container)
│   ├── SubViewport
│   │   └── BlockGameMain (current active game)
│   └── ColorRect (CRT overlay with shader)
└── CanvasLayer (UI layer for overlays)
```

#### tennis_game_main.tscn
```
Node2D (MainTennis)
├── Camera2D (Game camera)
├── Background (ColorRect - game area)
├── Walls
│   ├── TopWall (StaticBody2D with collision)
│   └── BottomWall (StaticBody2D with collision)
├── Divider (ColorRect - center line)
├── CrossRect (ColorRect - decorative)
├── Paddle1 (TennisGamePaddle scene instance)
├── Paddle2 (TennisGamePaddle scene instance)
├── Ball (TennisGameBall scene instance)
├── HUD (CanvasLayer)
│   ├── ScoreP1 (Label)
│   ├── ScoreP2 (Label)
│   ├── Timer (Label)
│   └── MiddleScreenLabel (Label - game over message)
├── Menu (TennisGameMenu scene instance)
└── GameTimer (Timer node)
```

#### tennis_game_paddle.tscn
```
CharacterBody2D (Paddle)
├── CollisionShape2D (paddle hitbox)
└── Sprite2D (paddle visual)
```

#### tennis_game_ball.tscn
```
CharacterBody2D (BallTennis)
├── CollisionShape2D (ball hitbox)
└── Sprite2D (ball visual)
```

#### tennis_game_menu.tscn
```
Control (MenuTennis)
└── Panel (menu background)
    ├── TabContainer (settings tabs)
    │   ├── Players (player configuration)
    │   │   ├── Player1Type (OptionButton)
    │   │   └── Player2Type (OptionButton)
    │   ├── Paddles (paddle settings)
    │   │   ├── Paddle1Size (HSlider)
    │   │   ├── Paddle1Speed (HSlider)
    │   │   ├── Paddle1Color (ColorPickerButton)
    │   │   ├── Paddle2Size (HSlider)
    │   │   ├── Paddle2Speed (HSlider)
    │   │   └── Paddle2Color (ColorPickerButton)
    │   ├── Ball (ball settings)
    │   │   ├── BallSize (HSlider)
    │   │   ├── BallSpeed (HSlider)
    │   │   └── BallColor (ColorPickerButton)
    │   └── Rules (game rules)
    │       ├── MaxScore (HSlider)
    │       └── TimeLimit (HSlider)
    └── Buttons (action buttons)
        ├── StartButton (Button)
        ├── ResumeButton (Button)
        ├── ResetButton (Button)
        └── QuitButton (Button)
```

#### block_game_main.tscn
```
Node2D (MainBlock)
├── Background (ColorRect)
├── Walls
│   ├── TopWall (StaticBody2D)
│   ├── LeftWall (StaticBody2D)
│   └── RightWall (StaticBody2D)
├── Paddle (BlockGamePaddle scene instance)
├── Ball (BlockGameBall scene instance)
├── BlockCollection (Node2D - manages block grid)
├── HUD (CanvasLayer)
│   ├── Score (Label)
│   ├── Timer (Label)
│   └── MiddleScreenLabel (Label)
├── Menu (BlockGameMenu scene instance)
└── GameTimer (Timer node)
```

### Signal Flow Diagram

```
┌───────────────────────────────────────────────────────────────┐
│                     Signal/Event Flow                          │
└───────────────────────────────────────────────────────────────┘

User Input (Keyboard)
    ↓
Input.is_action_pressed("p1_move_up")
    ↓
PaddlePlayer.GetInputDirection()
    ↓
IController.Update() calls Paddle.Move()
    ↓
Paddle._PhysicsProcess() applies velocity

---

Ball Physics
    ↓
BallTennis._PhysicsProcess()
    ↓
MoveAndSlide() → Collision detection
    ↓
get_slide_collision() → GetCollider()
    ↓
If Paddle: Bounce + speed increase + AudioManager.PlayAudioClip("hit")
If Wall: Bounce + AudioManager.PlayAudioClip("hit")

---

Ball Out of Bounds
    ↓
BallTennis.CheckBoundary()
    ↓
BallTennis.OnOutOfBounds.Emit(isLeftSide)
    ↓
IController.OnPointScore(isLeftSide)
    ↓
Score.AddPoint()
    ↓
Score._scoreLabel.Text updated

---

Menu Interaction
    ↓
User clicks "Start" button
    ↓
Button.Pressed signal → MenuTennis._OnStartButtonPressed()
    ↓
MenuTennis.OnGameStart.Emit(...parameters)
    ↓
MainTennis.GameStart() receives event
    ↓
Create controllers, configure entities, start game

---

Pause System
    ↓
User presses ESC
    ↓
Input.is_action_just_pressed("pause_game")
    ↓
PauseWatcher._Process() detects input
    ↓
PauseWatcher.OnTogglePause.Emit()
    ↓
MainTennis.GamePause() receives event
    ↓
Toggle menu visibility, ball physics, timer

---

Timer System
    ↓
GameTimer.timeout signal fires (every 1 second)
    ↓
MainTennis.TimerUpdate()
    ↓
Check scores and time
    ↓
If condition met: MainTennis.GameOver()
    ↓
Display winner, rainbow effect, reset game
```

### Data Flow Diagram

```
┌──────────────┐
│  User Input  │
└──────┬───────┘
       │
       ▼
┌──────────────────┐
│  IController     │
│  (Player/AI)     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐     ┌──────────────────┐
│     Paddle       │────▶│  CharacterBody2D │
│  (Entity)        │     │   (Physics)      │
└──────────────────┘     └──────────────────┘
                                  │
                         Collision Detection
                                  │
       ┌──────────────────────────┴──────────────────────────┐
       ▼                                                       ▼
┌──────────────────┐                              ┌──────────────────┐
│      Ball        │                              │      Wall        │
│   (Entity)       │                              │   (StaticBody2D) │
└──────┬───────────┘                              └──────────────────┘
       │
       │ Bounce + Audio
       ▼
┌──────────────────┐
│  AudioManager    │
│  (Sound)         │
└──────────────────┘

       │ Out of Bounds Event
       ▼
┌──────────────────┐
│  IController     │
│  (Scoring)       │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│      Score       │
│  (Display)       │
└──────────────────┘
       │
       ▼
┌──────────────────┐
│   MainTennis     │
│  (Check Win)     │
└──────┬───────────┘
       │
       ▼
┌──────────────────┐
│    Game Over     │
│  (UI Display)    │
└──────────────────┘
```

---

## Technical Implementation Details

### C# Namespaces

```csharp
Common          // Shared utilities and components
TennisGame      // Tennis-specific classes
BlockGame       // Block-specific classes
```

### Key C# Features Used

1. **Partial Classes**: All Godot node classes use `partial` for code generation
2. **Events**: C# events for custom game logic communication
3. **Interfaces**: `IController` for polymorphic behavior
4. **Extension Methods**: `Utils.cs` provides extension methods for Node operations
5. **Async/Await**: Used in `GameOver()` and `ToggleRainbowColorEffect()` for timed sequences
6. **Default Interface Methods**: `IController` provides default implementations for `Update()`, `Attach()`, etc.
7. **Switch Expressions**: Used in `MainTennis.GameStart()` for controller creation
8. **Sealed Classes**: Most classes are sealed to prevent inheritance

### Godot-Specific Features

1. **Signals**: Used for UI interactions (Button.Pressed, Timer.Timeout)
2. **Export Variables**: `[Export]` attribute for inspector visibility
3. **CharacterBody2D**: Physics body for entities with collision
4. **MoveAndSlide()**: Built-in collision-aware movement
5. **PackedScene**: Scene resources for instantiation
6. **AudioStreamPlayer**: Audio playback nodes
7. **CanvasLayer**: UI layer for HUD elements
8. **Timer**: Built-in timer node for game timing
9. **Shader Materials**: CRT effect applied via shader
10. **Input Actions**: Godot's input mapping system

### Physics Implementation

**Tennis Ball Physics**:
```csharp
// Initialization
Vector2 velocity = new Vector2(
    Mathf.Cos(angle) * _initialSpeed,
    Mathf.Sin(angle) * _initialSpeed
);

// Physics step
Velocity = velocity;
MoveAndSlide();

// Collision handling
for (int i = 0; i < GetSlideCollisionCount(); i++)
{
    var collision = GetSlideCollision(i);
    velocity = velocity.Bounce(collision.GetNormal());
    
    if (collision.GetCollider() is Paddle)
    {
        _speedFactor += 0.04f; // Accelerate
        _speedFactor = Mathf.Clamp(_speedFactor, 0.0f, 0.8f);
        AudioManager.PlayAudioClip("hit");
    }
}
```

**Paddle Movement**:
```csharp
// Apply input direction
Velocity = new Vector2(0, direction * _speed);

// Apply friction
Velocity *= _friction;

// Move with collision
MoveAndSlide();

// Clamp to viewport
Position = new Vector2(
    Mathf.Clamp(Position.X, 0, viewportSize.X),
    Mathf.Clamp(Position.Y, minY, maxY)
);
```

### AI Implementation (Tennis)

```csharp
public Direction GetInputDirection()
{
    // Track ball position with error margin
    float targetY = _ball.Position.Y;
    float paddleY = _paddle.Position.Y;
    float errorMargin = 50.0f; // Imperfect tracking
    
    // Add random delay
    if (_delayTimer > 0)
    {
        _delayTimer -= GetProcessDeltaTime();
        return Direction.None;
    }
    
    // Move towards ball
    if (targetY < paddleY - errorMargin)
        return Direction.Up;
    else if (targetY > paddleY + errorMargin)
        return Direction.Down;
    else
        return Direction.None;
}
```

### Score Display Implementation

```csharp
public class Score
{
    private byte _currentScore = 0;
    private readonly Label _scoreLabel;
    
    public Score(Label label)
    {
        _scoreLabel = label;
        UpdateDisplay();
    }
    
    public void AddPoint()
    {
        if (_currentScore < 255)
            _currentScore++;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        _scoreLabel.Text = _currentScore.ToString("D8");
    }
}
```

### Audio Management Implementation

```csharp
// Registration (in _Ready)
_audioManager.AddAudioClip("hit", _audioHit);
_audioManager.AddAudioClip("score", _audioScore);

// Playback (anywhere)
_audioManager.PlayAudioClip("hit", channel: 1);

// Volume control
_audioManager.SetChannelVolume(1, 0.8f); // 80% volume
```

### State Management Pattern

```csharp
// Singleton access
private readonly GameMonitor _monitor = GameManager.Monitor;

// State changes
_monitor.ChangeState(GameState.InGame);

// State checking
if (_monitor.CurrentState == GameState.Paused)
    return;

// State observation
_monitor.OnGameStateChanged += (newState) => {
    GD.Print($"State changed to: {newState}");
};
```

---

## Extending the Architecture

### Adding a New Game Mode

**Steps**:

1. **Create Scene Directory**:
```
scenes/
└── new_game/
    ├── new_game_main.tscn
    ├── new_game_menu.tscn
    └── new_game_entity.tscn (paddles, balls, etc.)
```

2. **Create Script Directory**:
```
scripts/
└── new_game/
    ├── MainNewGame.cs
    ├── MenuNewGame.cs
    ├── interface/
    │   └── IController.cs
    └── node/
        └── Entity.cs
```

3. **Implement Main Controller**:
```csharp
namespace NewGame;

using Common;
using Godot;

public sealed partial class MainNewGame : Node2D
{
    private AudioManager _audioManager;
    private PauseWatcher _pauseWatcher;
    private Score _score;
    
    public override void _EnterTree()
    {
        _audioManager = this.AddNode<AudioManager>();
        _pauseWatcher = this.AddNode<PauseWatcher>();
    }
    
    public override void _Ready()
    {
        // Setup game
    }
    
    public override void _Process(double delta)
    {
        // Game loop
    }
}
```

4. **Register in GameManager**:
```csharp
[Export] private PackedScene _newGameScene;

private void HandleStartGame(GameSelection selection)
{
    switch (selection)
    {
        case GameSelection.NewGame:
            this.InstantScene(_newGameScene);
            break;
    }
}
```

5. **Add to Enums.cs**:
```csharp
public enum GameSelection : byte
{
    None = 0,
    BlockGame = 1,
    TennisGame = 2,
    NewGame = 3  // Add new entry
}
```

### Adding New Entity Types

**Example: Power-up Entity**

1. **Create Scene**: `scenes/tennis_game/tennis_game_powerup.tscn`

2. **Create Script**:
```csharp
namespace TennisGame;

using Godot;

public sealed partial class PowerUp : Area2D
{
    [Signal]
    public delegate void OnCollectedEventHandler(PowerUp powerUp);
    
    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }
    
    private void OnBodyEntered(Node2D body)
    {
        if (body is Paddle)
        {
            EmitSignal(SignalName.OnCollected, this);
            QueueFree();
        }
    }
}
```

3. **Integrate in MainTennis**:
```csharp
[Export] private PackedScene _powerUpScene;

private void SpawnPowerUp()
{
    var powerUp = this.InstantScene(_powerUpScene) as PowerUp;
    powerUp.OnCollected += HandlePowerUpCollected;
}

private void HandlePowerUpCollected(PowerUp powerUp)
{
    // Apply power-up effect
    _audioManager.PlayAudioClip("powerup");
}
```

### Adding New Controller Types

**Example: Network Player Controller**

```csharp
namespace TennisGame;

using Common;

public sealed class PaddleNetwork : IController
{
    public BallTennis BallTennis { get; }
    public bool IsLeftSide { get; }
    public Paddle Paddle { get; }
    public Score Score { get; }
    
    private NetworkClient _networkClient;
    
    public PaddleNetwork(Paddle paddle, BallTennis ball, 
                        Score score, bool isLeftSide, 
                        NetworkClient client)
    {
        Paddle = paddle;
        BallTennis = ball;
        Score = score;
        IsLeftSide = isLeftSide;
        _networkClient = client;
    }
    
    public Direction GetInputDirection()
    {
        // Receive input from network
        return _networkClient.GetRemoteInput();
    }
}
```

### Adding Audio Clips

**Steps**:

1. **Import Audio File**: Place in `assets/sounds/`

2. **Reference in Scene**: Add as `[Export]` in main controller

3. **Register in AudioManager**:
```csharp
public override void _Ready()
{
    _audioManager.AddAudioClip("new_sound", _audioNewSound);
}
```

4. **Play Sound**:
```csharp
_audioManager.PlayAudioClip("new_sound", channel: 2);
```

### Customizing Win Conditions

**Example: Best of 3 Rounds**

```csharp
private int _player1Rounds = 0;
private int _player2Rounds = 0;
private const int MAX_ROUNDS = 3;

private void CheckRoundWin()
{
    if (_scoreP1.CurrentScore >= _maxScore)
    {
        _player1Rounds++;
        ResetRound();
    }
    else if (_scoreP2.CurrentScore >= _maxScore)
    {
        _player2Rounds++;
        ResetRound();
    }
    
    if (_player1Rounds >= 2 || _player2Rounds >= 2)
        GameOver();
}

private void ResetRound()
{
    _scoreP1.Reset();
    _scoreP2.Reset();
    _ball.ResetBall();
}
```

### Adding UI Elements

**Example: Lives Display**

1. **Add to Scene**: Create Label node in HUD CanvasLayer

2. **Reference in Script**:
```csharp
[Export] private Label _livesLabel;
private int _lives = 3;
```

3. **Update Display**:
```csharp
private void UpdateLivesDisplay()
{
    _livesLabel.Text = $"Lives: {_lives}";
}

private void HandleBallOutOfBounds()
{
    _lives--;
    UpdateLivesDisplay();
    
    if (_lives <= 0)
        GameOver();
    else
        _ball.ResetBall();
}
```

### Performance Optimization Tips

1. **Object Pooling**: Reuse frequently instantiated objects (balls, blocks)
```csharp
private Queue<Ball> _ballPool = new Queue<Ball>();

private Ball GetBall()
{
    if (_ballPool.Count > 0)
        return _ballPool.Dequeue();
    else
        return this.InstantScene(_ballScene) as Ball;
}

private void ReturnBall(Ball ball)
{
    ball.Visible = false;
    _ballPool.Enqueue(ball);
}
```

2. **Reduce Signal Emissions**: Cache frequently accessed values

3. **Use `_PhysicsProcess` for Physics**: Keep physics calculations in physics frame

4. **Batch Scene Changes**: Use `call_deferred` for scene tree modifications

5. **Profile Performance**: Use Godot's built-in profiler (Debug → Profiler)

---

## Additional Information

### Known Issues and Limitations

1. **Block Game Incomplete**: 
   - Menu integration not finished
   - Game over conditions not implemented
   - Timer system not connected

2. **No Main Menu**:
   - GameManager has menu scene reference but not fully implemented
   - Direct scene loading instead of menu selection

3. **AI Limitations**:
   - Tennis AI is reactive only (no prediction)
   - Block AI not implemented

4. **No Persistence**:
   - Settings not saved between sessions
   - No high scores or statistics

5. **Limited Audio Channels**:
   - Only 2 audio channels (can overlap sounds)
   - No audio mixing or priority system

### Testing Recommendations

1. **Unit Testing**: Not currently implemented, but consider testing:
   - Score arithmetic
   - Controller direction logic
   - Ball boundary detection
   - State transitions in GameMonitor

2. **Integration Testing**:
   - Test menu → game start flow
   - Test pause/resume functionality
   - Test game over conditions
   - Test controller switching (Player ↔ AI)

3. **Manual Testing Checklist**:
   - [ ] Player 1 controls work (WASD and Arrows)
   - [ ] Player 2 controls work (IJKL and Numpad)
   - [ ] AI follows ball correctly
   - [ ] Ball bounces off paddles and walls
   - [ ] Score increments correctly
   - [ ] Timer counts down
   - [ ] Game over triggers at max score
   - [ ] Game over triggers at time limit
   - [ ] Pause/resume works (ESC key)
   - [ ] Menu settings apply correctly
   - [ ] Audio plays on collisions
   - [ ] Rainbow effect works on game over

### Future Enhancement Ideas

1. **Gameplay**:
   - Power-ups (speed boost, size change, multi-ball)
   - Different ball types (curve ball, heavy ball)
   - Special abilities for paddles
   - Obstacles in play field
   - Different game modes (survival, time attack)

2. **AI**:
   - Predictive AI (anticipates ball trajectory)
   - Difficulty levels
   - Learning AI (adapts to player style)

3. **Multiplayer**:
   - Local multiplayer (already supported)
   - Online multiplayer (requires networking)
   - Spectator mode

4. **UI/UX**:
   - Main menu with game selection
   - Settings persistence
   - High score tracking
   - Replay system
   - Better visual effects (particles, shaders)

5. **Content**:
   - More levels for Block game
   - Level editor
   - Custom themes and skins
   - Achievements

6. **Technical**:
   - Unit tests for core logic
   - Automated UI tests
   - Performance profiling
   - Mobile support (touch controls)
   - Gamepad support

### Contributing Guidelines

If you want to extend this project:

1. **Follow Existing Patterns**: Use the same architectural patterns demonstrated
2. **Namespace Organization**: Keep scripts in appropriate namespace (Common, TennisGame, BlockGame)
3. **Event-Driven Communication**: Use events for decoupled communication
4. **Sealed Classes**: Mark classes as `sealed` unless inheritance is intended
5. **Comments**: Add XML documentation comments for public APIs
6. **Testing**: Manually test all changes before committing
7. **License**: All contributions must be compatible with AGPLv3

### Resources and References

**Godot Documentation**:
- Godot 4.x Docs: https://docs.godotengine.org/en/stable/
- C# in Godot: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/

**Project References**:
- 20 Games Challenge: https://20_games_challenge.gitlab.io/
- Kenney Assets: https://kenney.nl/
- Godot Community: https://godotengine.org/community

**Tools**:
- Godot Engine: https://godotengine.org/
- .NET SDK: https://dotnet.microsoft.com/
- Visual Studio Code: https://code.visualstudio.com/
- Rider (C# IDE): https://www.jetbrains.com/rider/

---

## Conclusion

This architectural document provides a comprehensive overview of the TennisDough game project. The architecture emphasizes:

- **Modularity**: Clear separation between game modes and common components
- **Extensibility**: Easy to add new game modes, entities, and features
- **Maintainability**: Event-driven design with clear responsibilities
- **Flexibility**: Interface-based controllers allow for different player types

The project serves as an excellent learning resource for Godot C# development, demonstrating fundamental game development concepts, physics implementation, UI management, and architectural patterns.

For questions or contributions, refer to the project repository: https://github.com/Baerthe/TennisDough

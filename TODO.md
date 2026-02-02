# TennisDough - Project TODO List

> **Last Updated:** February 02, 2026
> **Project Status:** In Development

---

## 📋 Overview

This document tracks the remaining work items for the TennisDough project, which contains two mini-games:
- **BlockGame** - A breakout-style game
- **TennisGame** - A Pong-style game

Both games need to be integrated into a unified system with a main menu, game pack loading, and shared state management.

### Further Mini-Games
- **FrogGame** - A Frogger-style game (planned, not started)
- **SpaceGame** - A floaty space-style game (planned, not started)
- **PacGame** - A Pacman-style game (planned, not started)
- **BrotherGame** - A Donkey Kong-style game (planned, not started)
- **WormyGame** - A Worms-style game (planned, not started)

*MAYBE 3D concepts, maybe.*
- **FoxGame** - StarFox-style game (planned, not started)
- **WolfGame** - A German-shooting style game (planned, not started)

At some point want to add the background elements to the main menu screen, where the monitor is, to show a stack of game carts for quick selection. Maybe a window?

---

## 🔴 Critical / High Priority

### 1. GameManager - Core System Integration
- [x] **Implement `HandleStartGame` logic** - Currently just prints a debug message
- [x] **Scene loading/unloading** - Load game scenes based on `GamePack` selection
- [x] **State transitions** - Wire `GameMonitor.ChangeState()` calls throughout game lifecycle
- [ ] **Return to main menu** - Handle game completion → main menu transition
- [ ] **Cleanup on game exit** - Properly dispose of game scenes when returning to menu

### 2. MainMenu - UI Implementation
- [ ] **Build MainMenu scene** - Scene file needs to be created
- [ ] **Display available GamePacks** - List loaded packs with icons/descriptions
- [ ] **Game selection UI** - Allow user to select and start a game
- [ ] **Settings button** - Global audio/video settings
- [ ] **Quit button implementation** - Wire `OnQuitGame` event
- [ ] **Inject GamePacks from GameManager** - Pass loaded packs to menu for display

### 3. Global Game State Management
- [x] **Integrate `GameMonitor` into both games** - BlockGame partially done, TennisGame needs update
- [x] **Pause state handling** - `GameState.Paused` should propagate to active game
- [x] **Game over state** - `GameState.GameOver` should trigger return flow
- [x] **State change events** - Games should subscribe to `OnGameStateChanged`

---

## 🟡 Medium Priority

### 4. BlockGame - Finishing Touches
- [ ] **Uncomment event connections in `_Ready()`:**
  - `_ball.OnOutOfBounds += HandleBallOutOfBounds`
  - `_gameTimer.Timeout += HandleTimerUpdate`
  - `_pauseWatcher.OnTogglePause += GamePause`
- [ ] **Implement `GameOver()` method** - Currently commented out in `HandleTimerUpdate`
- [ ] **Wire menu audio clips** - `button_press`, `menu_open`, `menu_close`, `game_over` are commented
- [ ] **Inject AudioManager into MenuBlock** - `_menu.Inject(_audioManager)` is commented
- [ ] **Fix particle rendering** - GPUParticles2D z_index issue when embedded in main scene
- [ ] **Add win condition** - Detect when all blocks are destroyed
- [ ] **Level progression** - Support multiple levels or endless mode
- [ ] **Implement `OnOutOfBounds` logic** - Ball reset and life/score penalty

### 5. TennisGame - System Migration
- [ ] **Migrate to use `GameManager.Monitor`** - Currently uses local state booleans
- [ ] **Remove local `_isGameOver`/`_isPaused`** - Use centralized `GameMonitor` state
- [ ] **Subscribe to `OnGameStateChanged`** - React to global state changes
- [ ] **Update pause handling** - Should work with global pause system
- [ ] **Standardize controller interface** - Align with BlockGame's `IController`

### 6. GamePack System
- [ ] **Create `resources/packs/` directory** - For storing game pack `.tres` files
- [ ] **Create BlockGame.tres pack** - GamePack resource for BlockGame
- [ ] **Create TennisGame.tres pack** - GamePack resource for TennisGame
- [ ] **Add pack icons** - Create/assign `GameIcon` textures
- [ ] **Write pack descriptions** - Fill in `GameDescription` for each pack

---

## 🟢 Low Priority / Polish

### 7. Menu Systems Unification
- [ ] **Create base Menu class** - Extract common functionality from `MenuBlock`/`MenuTennis`
- [ ] **Standardize button layouts** - Consistent UI across both games
- [ ] **Add keyboard/controller navigation** - Menu accessibility

### 8. Audio System Improvements
- [ ] **Global volume settings** - Persist across game sessions
- [ ] **Music support** - Background music tracks
- [ ] ? Music Player ?
- [ ] **Audio bus configuration** - Separate SFX/Music buses

### 9. Visual Polish
- [ ] **CRT shader refinement** - Tune parameters for better effect
- [ ] ? CRT Shader options and effects related to loading, gameover, etc.
- [ ] **Loading transitions** - Fade in/out between scenes
- [ ] **Rainbow effect** - Implement `_isRainbowEffectActive` in BlockGame (already in TennisGame)
- [ ] **Particle system fixes** - Ensure trail effects work in all contexts

### 10. Code Quality
- [ ] **Remove debug flags** - `_isRandomLevel = true` should be configurable
- [ ] **Consolidate IController interfaces** - BlockGame and TennisGame have separate definitions
- [ ] **Add null checks** - AudioManager calls can fail if not injected
- [ ] **Error handling** - GamePack loading failures

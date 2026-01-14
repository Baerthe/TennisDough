TennisDough is an open-source project, created as a means to learn Godot 4.5 (Godot.NET.Sdk/4.5.1, .net9.0) by recreating Atari's Pong.

# License & Acknowledgements
This project is under GNU Affero General Public License v3.0 [License](LICENSE.txt) and uses assets from Kenney.nl under the CC0 1.0 Universal (CC0 1.0) Public Domain Dedication [Kenney Assets License](Asset_License.txt).

This is inspired by the ["20 Game Challenge"](https://20_games_challenge.gitlab.io/), but is not affiliated with it in any way.

# About TennisDough
This is a tennis style game inspired by the classic Atari Pong. It features both single and multiplayer modes, including with any configuration of human or AI players. The game is designed to be rather simple and fun but provides a variety of configuration options to customize the game. The game features configurations for paddle speed and size, ball speed, max score, max time, and color customization for paddles and ball. This game is provided as is, without warranty of any kind, and is free to use and modify under the terms of the AGPLv3 license. Feel free to learn from it, modify it, and share it with others!

## Progess
The game is feature complete but may need polish or bug fixes.

### Core Components
#### Game Loop & Management (`MainTennis.cs`)
The `Main` class serves as the central orchestrator. It is a `Node2D` that:
- Manages the game state (`Playing`, `Paused`, `GameOver`).
- Instantiates and updates Player/AI controllers.
- Connects signals from the `Menu` and `PauseWatcher`.
- Handles the core game timer and score tracking using `Score` objects.

#### Menu System (`MenuTennis.cs`)
The `Menu` class (`Control`) handles all UI interactions and game configuration. Pressing `ESC` opens this menu. It allows users to set:
- Player Types (Human vs AI).
- Paddle properties (Speed, Size, Color).
- Ball properties (Speed, Size, Color).
- Game rules (Max Score, Time Limit).
- Reset the game, quit the game.

It emits events like `OnGameStart`, `OnGameReset`, and `OnGameCancel` which `Main` subscribes to.

#### Entities (`Paddle.cs`, `BallTennis.cs`)
- **Paddle**: A `CharacterBody2D` that handles physics-based movement with friction. It supports dynamic resizing and color changes.
- **Ball**: A `CharacterBody2D` that handles bouncing physics, speed acceleration over time, and audio feedback. It detects out-of-bounds events to trigger scoring.

### Controller System
The game uses an `IController` interface to abstract paddle control, allowing seamless switching between human and AI players `Main` dynamically assigns these at game start (or during play from the menu).

- **`IController`**: Interface defining `Update()` and `GetInputDirection()`. Handles score accumulation logic via default interface methods.
- **`PaddlePlayer`**: Implementation for human players. Reads input actions (`p1_move_up`, etc.).
- **`PaddleAI`**: Implementation for AI. Uses a reactive tracking system with randomized delay/error margin to simulate imperfect play.

### Input Map
The scripts rely on the following Input Map actions (configured in Project Settings):
- **Player 1**: `p1_move_up`, `p1_move_down`
    - WASD or Arrow Keys
- **Player 2**: `p2_move_up`, `p2_move_down`
    - IJKL or Numpad Keys
- **System**: `pause_game`
    - ESC Key
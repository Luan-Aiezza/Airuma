# Airumã danmaku

A *danmaku* (bullet hell) game inspired by Touhou and South American mythologies and cultures. Dodge dense bullet patterns, shoot back and survive a six-phase boss fight while the wind pushes every bullet around the screen. Built with **Godot 4** and **C#**.

> The project is in early development. The bullet art is still a placeholder icon, and there is one boss fight so far.

## Features

- **Player:** move with the arrow keys, shoot with **Z**, and activate a **bomb** with **X**, an aura of light around the player that destroys boss bullets touching it for a few seconds. The player has 3 lives, a weapon power level and a limited number of bombs.
- **Six-phase boss:** each of the boss's 6 lives is a different attack pattern, with a transition between phases:
  1. A classic ring of bullets
  2. An aimed shotgun
  3. Double side spinners
  4. A homing lotus flower, with negative acceleration and curving bullets
  5. A giant snake with mixed bullet sizes
  6. A screen-wrap dash that leaves a trail of bullets
- **Advanced bullet patterns:** per-bullet acceleration, curve rate, scale and behaviors such as stop-and-aim.
- **Wind system:** a random wind (north, south, east or west) with no, weak, medium or strong force pushes bullets, and affects the player's bullets more than the boss's.
- **HUD and end screen:** lives, power, bombs, wind and boss health, plus a restart button.
- **Exports:** presets for macOS, Windows and Web.

## Architecture

The game uses an **ECS (entity–component–system)** design in C#. Bullets live in a fixed pool of 1,000 structs, which keeps updates cache-friendly and avoids allocations during play.

| Folder | Contents |
| --- | --- |
| `Scripts/ECS/Core` | `ECSManager`, the main node that owns the entities, the bullet pool and the game loop |
| `Scripts/ECS/Entities` | `PlayerEntity` and `BossEntity` |
| `Scripts/ECS/Components` | Core components (transform, velocity, hitbox, health, render) and combat components (weapons, bombs, bullet emitters) |
| `Scripts/ECS/Systems` | Movement, bullets, combat, bombs, boss phases, wind, rendering and UI systems |
| `Scripts/Data` | `BulletData`, the struct stored in the pool |
| `Scenes` | The main scene |

## Tech stack

![Godot](https://img.shields.io/badge/Godot-4.6-478CBF?style=for-the-badge&logo=godotengine&logoColor=white) ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white) ![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

## Running the project

Requirements: the **.NET** edition of Godot 4.6 and the .NET SDK.

1. Clone the repository:
   ```bash
   git clone https://github.com/Luan-Aiezza/Airuma.git
   ```
2. Open `project.godot` in Godot (.NET).
3. Press **F5** to build and run the main scene.

### Controls

| Key | Action |
| --- | --- |
| Arrow keys | Move |
| Z | Shoot |
| X | Bomb |

## Author

[Luan Aiezza](https://github.com/Luan-Aiezza)

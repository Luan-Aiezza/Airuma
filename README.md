# Airumã

A *danmaku* (bullet hell) for the desktop (Windows and macOS), inspired by Touhou and the myths and cultures of South America. You
are Atli, a boy lost on a river at night, and Airu, the fairy of his lantern. Gather the light of the forest spirits, choose cards,
dodge the bullets of the first boss, a woman of fire who turns into a giant serpent, while the wind pushes everything around. Built with
**Godot 4.7** and **C#**.

Everything you see and hear is made in code: the pixel art, the shaders, the music and the sound effects. There are no image or audio
files in the project (apart from the game's icon).

## Playing

| Key | Action |
| --- | --- |
| Arrow keys | Move |
| Z | Shoot (and confirm in menus and talks) |
| X | Bomb (and go back, or skip a talk) |
| Esc | Pause |
| F11, Alt+Enter, Cmd+Ctrl+F | Full screen |

- **Start** begins a run on Easy, Normal or Hard (the difficulty changes the number of bullets and the strength of the wind).
- **Boss Practice** rehearses the fight with the boss. The other bosses are locked until they are made.
- Light dropped by the spirits raises the weapon's level; a card is chosen at the start of the stage and before the boss; a spare
  lantern (a bomb) and a spare boat (a life) fall from the sky when the score reaches certain heights.

## How the project is organised

The simulation follows the **ECS** pattern (entities, components, systems) and knows nothing about pictures or sound. The rest of the game
is *data* that the simulation is given, or *views* that show what the simulation says.

```
Scripts/
  Ecs/                the simulation (no Godot nodes)
    Components/       plain data: transform, health, hitbox, weapon, bomb, bullet emitter, bullet, light, wave progress, score...
    Entities/         PlayerEntity, BossEntity, EnemyEntity: bags of components
    Systems/          the rules, as static functions: movement, bullets, combat, bomb, enemies, waves, boss attacks,
                      lights, power, score and prizes, cards, wind
    Cards/            the cards' effects, the run's stack of cards, the offers
    Events/           WorldEvent: what the simulation tells the views
    World.cs          owns the entities and runs the systems in order, once per step (World.Testing.cs: hooks for the tests)
  Content/            what changes from stage to stage: the data the simulation runs
    Stages/           StageDefinition, the stage list, and the waves of stage 1
    Bosses/           BossDefinition / BossPhase, and Iryssh, the boss of stage 1
    Spirits/          the table of the forest spirits (health, hitbox, lights, points)
    Cards/            titles, rarities and limits of the cards
    Text/             every text of the game (UIText) and the talks (Dialogue)
  View/               Godot nodes that show the simulation
    Game/             GameScene (split in partial files: sequences, rendering, events, input), input
    Hud/              the side panel, dialogue box, card picker, pause menu, title card
    Menu/             the title menu and its backdrop
    Backdrop/         the river (with its water shader), the scrolling banks, the pillars at the sides of the screen
    Effects/          bomb, bursts, wake, wind
    Boss/             the serpent's body and flames
  Art/                pixel art drawn pixel by pixel in code (sprites, tiles, HUD, fonts)
  Audio/              the synthesiser, the songs, the sound effects and their players
  App/                Main (menu <-> game), the layout of the screen, the high scores
  Tests/              SelfTest: the logic tests (no window needed)
Shaders/              water, menu water, and the serpent's flames
```

### Adding to the game

- **A stage:** write a `StageDefinition` (title, music, waves, boss) in `Content/Stages` and list it in `Stages`. `World` takes it as an
  argument; nothing in `Ecs/` has to change.
- **A boss:** a `BossDefinition` is a list of `BossPhase`s: each one has the attack's name, the shape and hitbox, how it moves, and its
  bullet emitters (plus an extra emitter for Hard). `Content/Bosses/IrysshBoss.cs` is the model.
- **A spirit:** add a value to `SpiritKind` and a row to `SpiritCatalog`; the paths and shooting patterns of a wave are in
  `Content/Stages/Stage1Waves.cs` (`EnemyPath`, `FirePattern`).
- **A card:** add a value to `Card`, its texts to `UIText`, its rarity and limit to `CardInfo`, and its effect to `CardEffects`.

## Running

Requirements: the **.NET edition of Godot 4.7** and the .NET 10 SDK. `nuget.config` points to the packages bundled with the Godot editor
(for offline builds); change it if your editor lives elsewhere.

1. Open `project.godot` in Godot (.NET) and press **F5**, or run `Godot --path .`.
2. The window is 1378x775 (the game is 1110x775 and the pillars of ancient stone fill the sides); on wider screens in full screen the
   pillars and the jungle wall grow to fill the space.

Flags after `--`:

| Flag | What it does |
| --- | --- |
| `--game` | Skip the menu and start a run (`--difficulty=easy\|normal\|hard`) |
| `--practice=N` | Start boss practice at attack N (1 = the whole fight) |
| `--autoplay` | A test pilot plays |
| `--shot=file.png` | Save a picture of the screen after a moment and quit |
| `--export-music=dir`, `--export-sfx=dir` | Write the songs and the sound effects as WAV files |
| `--export-boss=dir`, `--export-pillar=dir`, `--export-icon=file` | Write art for a look |

## Tests

```bash
Godot --headless --path . res://Scenes/selftest.tscn   # the exit code is the number of failures
```

The tests (`Scripts/Tests`) run the simulation without a window: movement and bullets, the spirits and their waves, the bomb, the cards,
the score and prizes, the difficulties, the stage flow (including a stage made on the spot), the audio and the texts.

## Exporting

The presets for macOS and Windows are in `export_presets.cfg`; they need the export templates installed (Editor > Manage Export Templates).

## Author

[Luan Aiezza](https://github.com/Luan-Aiezza)

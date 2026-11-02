# Airumã

A *danmaku* (bullet hell) for the desktop (Windows and macOS), inspired by Touhou and the myths and cultures of South America. You
are Atli, a boy lost on a river at night, and Airu, the fairy of his lantern. Gather the light of the forest spirits, choose cards,
dodge the bullets of the bosses (so far a woman of fire, and a dancer of the river parties) while the wind pushes everything around. Built with
**Godot 4.7** and **C#**.

Everything you see and hear is made in code: the pixel art, the shaders, the music and the sound effects. There are no image or audio
files in the project (apart from the game's icon).

## Playing

| Key | Action |
| --- | --- |
| Arrow keys | Move |
| Z | Shoot (and confirm in menus and talks) |
| Shift | Focus: the boat moves slowly, the shot narrows and hits harder, and the hitbox shows as a small dot |
| X | Bomb (and go back, or skip a talk) |
| Esc | Pause |
| F11, Alt+Enter, Cmd+Ctrl+F | Full screen |

- **Start** begins a run on Easy, Normal or Hard (the difficulty changes the number of bullets and the strength of the wind).
- **Practice** rehearses a whole stage (not recorded) or the fight with a boss; the bosses that are not made yet are locked. In practice the
  boat never loses a life.
- Losing a life fills the bag of bombs again.
- Light dropped by the spirits raises the weapon's level; one card is chosen at the start of the run and one after each boss (six in all; a boss may leave a special card of its theme, likelier the better the stage score); a spare
  bomb and a spare boat (a life) fall from the sky when the score reaches certain heights.

## How the project is organised

The simulation follows the **ECS** pattern (entities, components, systems) and knows nothing about pictures or sound. The rest of the game
is *data* that the simulation is given, or *views* that show what the simulation says.

```
Scripts/
  Ecs/                the simulation (no Godot nodes)
    Components/       plain data: transform, health, hitbox, weapon, bomb, bullet emitter, bullet, light, wave progress, score...
    Entities/         PlayerEntity, BossEntity, EnemyEntity: bags of components; BossBrains.cs: the state of each way a boss moves
    Systems/          the rules, as static functions: movement (Movement/: one file per way a boss moves), bullets, combat, bomb,
                      enemies, waves, phases, lights, power, score and prizes, cards, wind
    Cards/            the cards' effects, the run's stack of cards, the offers
    Events/           WorldEvent: what the simulation tells the views
    World.cs          owns the entities and runs the systems in order, once per step; RunCarry.cs: what goes from one stage to the next
  Content/            what changes from stage to stage: the data the simulation runs
    Stages/           StageDefinition (waves, boss, talks, scenery, HUD), the stage list, WaveBuilder, the waves of each stage
    Bosses/           BossDefinition / BossPhase / BossBuilder, one file per boss, Attacks/ (the library, one file per theme)
    Spirits/          the table of the forest spirits (health, hitbox, lights, points)
    Cards/            titles, rarities and limits of the cards
    Text/             every text (UIText, Loc, the tables of each language) and the talks (Dialogue)
  View/               Godot nodes that show the simulation
    Game/             GameScene (split in partial files: sequences, rendering, events, input), input
    Boss/             BossView (the boss: sprite, glow, reflection) and the parts that show what it does: spin, rage, dive, jet
    Hud/              the side panel, dialogue box, card picker, pause menu, title card
    Menu/             the title menu and its backdrop
    Backdrop/         the river (with its water shader), the scrolling banks, the pillars at the sides of the screen
    Effects/          bomb, bursts, wake, wind
    ViewKit.cs        helpers shared by the views (z-order, tinted and additive materials, reflections)
  Art/                pixel art drawn pixel by pixel in code (sprites, tiles, HUD, fonts); Retired/: art kept for later (serpent, dolphin)
  Audio/              the synthesiser, the songs, the sound effects and their players
  App/                Main (menu <-> game), LaunchOptions (the command line), DevTools (art, sounds and music to files), layout, high scores
  Tests/              SelfTest: the logic tests (no window needed)
Shaders/              water, menu water
```

### Where things are decided

- **What a stage is:** a `StageDefinition` (`Content/Stages/Stages.cs`): its waves, its boss, its scenery (`Scenery`), its HUD (`Hud`), its
  title and music, and the talks (`Intro`, `BossArrival`, `BossEncounter`, `Ending`).
- **What a boss is:** a list of attacks (`BossBuilder`: calm ones first, enraged ones last) and a `BossStyle` (`Art/BossStyle.cs`: the sprite and
  its size, glow, whether it spins, the colours of its rage). `BossView` draws any of them; it knows no boss by name.
- **How a boss moves:** `BossMovementType` plus a file `Ecs/Systems/Movement/MovementSystem.<Name>.cs` and a brain in `BossBrains.cs`.
- **How bullets are fired:** one function, `BulletSystem.FireBullet`, for the emitters of the bosses and for the patterns of the spirits
  (`FirePattern` can be `Styled(...)` and `Dancing(...)`, like a boss's emitter).
- **The bullets a boss can fire** (`BulletEmitterComponent` / `BulletBehavior`): plain volleys and spirals; `Dance` (sway on a half-second beat); `Zigzag`
  (lightning); `Constellation` (stars that fly out, hang linked by thin lines that hurt, and fall; `Link`, `LinkSkip`); and the **curtain**: a row
  of bullets across the field with a gap that swings from side to side, which makes the boat move in small steps (the micro-movement of the
  later bosses; the gap is never faster than the boat while focusing).
- **Every text:** by key, in the tables; a talk's lines hold keys (`DialogueLine.Key`), so adding a line is a line of code and two table entries.

### Languages

Every text of the game is looked up by a **key** in a table for the current language (`Content/Text/Loc.cs`). The tables are
`Content/Text/Texts.PtBr.cs` (Brazilian Portuguese, the default) and `Texts.En.cs` (English); Spanish is still to be written. The key of
a text is the name of its property in `UIText` (`MenuStart`...), a line of a talk (`Intro01`, `S2Meet03`...), a card (`CardFervor.Title`, `CardFervor.Detail`) or of a boss
attack (`AttackRingOfEmbers`). A text that a language lacks is shown in English. The game draws its text with its own pixel font, so
Godot's translation of labels does not apply; `Loc` does the same job. The tests check that every language has every key and that the
font has every letter that the texts use.

To add a language: add it to `Language`, write `TextsXx.cs` with the same keys as `TextsEn.cs`, and register it in `Loc.Tables`. The
language is read from `user://settings.cfg` (`Loc.Load`, called at start-up) and saved by `Loc.Choose` (for the options menu to use).

### Adding to the game

- **A stage** (checklist; the architecture tests fail while something is missing):
  1. `Content/Stages/StageNWaves.cs`: the waves, from the blocks of `WaveBuilder`. Each stage has more spirits than the last; from stage 3 all
     three kinds come. A spirit's attacks are its own: `FirePattern`, optionally `.Styled(...)` and `.Dancing(...)`.
  2. A boss file in `Content/Bosses/` (below).
  3. In `Stages.cs`: a `StageDefinition` with the title and music keys, the waves, the boss, `Scenery`, `Hud`, and the talks (functions that
     return lines of keys); add it to `Stages.All`. `World` takes it as an argument; nothing in `Ecs/` has to change.
  4. Texts for every key (title, talks, attack names, boss name) in `Texts.PtBr.cs` and `Texts.En.cs`.
  5. Art: a scenery in `RiverBackground` (a `Scenery` value and its art, see `FestivalArt`), a HUD theme in `HudTheme`.
  `--stage=N` starts a given stage; the practice menu lists it by itself.
- **A boss:** a file in `Content/Bosses/` with a `BossBuilder` (name key, hitbox = a third of the sprite's width, a `BossStyle`) and its attacks:
  `Builder.Calm(Attacks.X())` ... `Builder.Enraged(Attacks.Y())`. Bosses have, in order of stage, 2+2, 3+2, 4+2, 4+3, 5+3 and 6+3 attacks
  (calm + enraged). Attacks live in `Content/Bosses/Attacks/` (one file per theme, with the spare ones kept for later bosses); reuse one only if
  it changes something. A new way of moving is a `BossMovementType` with its own file in `Ecs/Systems/Movement/` and its brain in `BossBrains.cs`.
  The boss's sprite is a class in `Art/` (4 frames) and a `BossStyle` in `BossStyles`. `IrysshBoss.cs` and `BayloBoss.cs` are the models.
- **A spirit:** add a value to `SpiritKind` and a row to `SpiritCatalog`.
- **A card:** add a value to `Card`, its texts to the tables, its rarity and limit to `CardInfo`, and its effect to `CardEffects`.

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
| `--practice=N` | Start boss practice at attack N of the boss of `--stage=N` (1 = the whole fight) |
| `--autoplay` | A test pilot plays |
| `--shot=file.png` | Save a picture of the screen after a moment and quit |
| `--export-music=dir`, `--export-sfx=dir` | Write the songs and the sound effects as WAV files |
| `--stage=N` | The stage to start in (with `--game` or `--practice`) |
| `--shot-frames=N` | Frames to wait before `--shot` (90 by default) |
| `--export-baylo=file`, `--export-iryssh=file`, `--export-retired=dir`, `--export-pillar=file`, `--export-icon=file` | Write art for a look |

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

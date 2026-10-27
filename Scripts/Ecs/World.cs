using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>The run has two parts: the waves of spirits, then the boss.</summary>
public enum StagePart { Waves, Boss }

/// <summary>
/// The simulation of a run: owns the entities and the state of the stage, and each step runs the systems on them in order.
/// It knows nothing of pictures or sounds: it only tells, through <see cref="TakeEvents"/>, what happened. Which spirits come and which
/// boss fights is not in here but in the <see cref="StageDefinition"/> it is given.
/// </summary>
public partial class World
{
    public static readonly Rect2 PlayArea = new Rect2(0, 0, 600, 775);
    public const int MaxLives = 3;

    // The stage
    public readonly StageDefinition Definition;
    public StagePart Part { get; private set; } = StagePart.Waves;
    /// <summary>In boss practice, the boss attack (1...) being rehearsed; null in a normal run.</summary>
    public readonly int? PracticePhase;
    /// <summary>How many bullets the enemies fire, and how hard the wind blows (see <see cref="global::Difficulty"/>).</summary>
    public readonly Difficulty Difficulty;
    /// <summary>Boss lives at the start of the run: all of them in a normal run, fewer when practising a later attack.</summary>
    public int StartingBossLives => Definition.Boss.Lives - (PracticePhase ?? 1) + 1;
    /// <summary>Normal runs offer cards; boss practice never does.</summary>
    public readonly bool UsesCards;

    // Entities and pools
    public PlayerEntity Player { get; private set; }
    public BossEntity Boss { get; private set; }
    public List<EnemyEntity> Enemies { get; private set; } = new List<EnemyEntity>();
    public BulletData[] Bullets { get; private set; } = BulletSystem.NewPool();
    /// <summary>Little lights falling through the picture, for the player to gather.</summary>
    public LightItem[] Lights { get; private set; } = LightSystem.NewPool();

    // State of the run
    public WindState Wind { get; private set; }
    public GameResult? Result { get; private set; }
    public int Score => _score.Points;
    private ScoreBoard _score;
    private WaveProgress _waves = new WaveProgress();
    /// <summary>Index of the wave in progress, or the number of waves once every wave is done.</summary>
    public int WaveIndex => _waves.Index;
    private List<WorldEvent> _events = new List<WorldEvent>();

    // Cards
    /// <summary>The cards taken so far in this run. Lost with the run: a new run starts empty.</summary>
    public RunState Run { get; private set; } = new RunState();
    public CardEffects Effects { get; private set; } = new CardEffects();
    /// <summary>A choice the player has to make; the world stands still until they do.</summary>
    public Draft Draft { get; private set; }
    private readonly SeededGenerator _cardRandom;

    public World(int? practicePhase = null, bool withCards = true, ulong? seed = null, Difficulty difficulty = Difficulty.Normal, StageDefinition stage = null)
    {
        Definition = stage ?? Stages.One;
        Difficulty = difficulty;
        PracticePhase = practicePhase.HasValue ? Math.Max(1, Math.Min(Definition.Boss.Lives, practicePhase.Value)) : (int?)null;
        UsesCards = withCards && !practicePhase.HasValue;
        _cardRandom = new SeededGenerator(seed ?? ((ulong)Random.Shared.NextInt64() << 1) ^ (ulong)Random.Shared.NextInt64());
        Reset();
    }

    /// <summary>What happened since the last call.</summary>
    public List<WorldEvent> TakeEvents()
    {
        var taken = _events;
        _events = new List<WorldEvent>();
        return taken;
    }

    /// <summary>Starts the run again from the beginning.</summary>
    public void Reset()
    {
        Player = PlayerEntity.Create();
        Boss = BossEntity.Create(Definition.Boss);
        Boss.Health.Lives = StartingBossLives;
        Bullets = BulletSystem.NewPool();
        Enemies = new List<EnemyEntity>();
        Lights = LightSystem.NewPool();
        Result = null;
        _score = new ScoreBoard();
        _waves = new WaveProgress();
        _events = new List<WorldEvent>();
        Run = new RunState();
        Effects = new CardEffects();
        CardSystem.Apply(Player, Effects);
        Draft = null;
        Player.Energy = PowerCurve.StartingEnergy(PracticePhase.HasValue);
        Player.Weapon.PowerLevel = PowerCurve.Level(Player.Energy);
        RandomizeWind();
        if (!PracticePhase.HasValue)
        {
            // The boss waits off-screen until the spirits are dealt with.
            Part = StagePart.Waves;
            Boss.Transform.Position = new Vector2(300, -400);
            Boss.MovementType = BossMovementType.Stationary;
        }
        else
        {
            Part = StagePart.Boss;
            PhaseManagerSystem.ChangePhase(Boss, Boss.Health.Lives, Difficulty);
            PhaseManagerSystem.BeginEntrance(Boss);
        }
        RandomizeWind();
        if (UsesCards) Draft = CardSystem.MakeDraft(DraftMoment.StageStart, Run, Player.Bomb.BombsLeft, _cardRandom);
    }

    private void RandomizeWind()
    {
        var w = Wind;
        WindSystem.Randomize(ref w);
        Wind = w;
    }

    private void AddScore(int points) => ScoreSystem.Add(ref _score, points, Effects.ScoreMultiplier, Lights);

    private void StartBoss()
    {
        Part = StagePart.Boss;
        BulletSystem.ClearBossBullets(Bullets);
        Boss.Transform.Position = new Vector2(300, -160);
        PhaseManagerSystem.ChangePhase(Boss, Boss.Health.Lives, Difficulty);
        PhaseManagerSystem.BeginEntrance(Boss);
        RandomizeWind();
        _events.Add(WorldEvent.BossEntered());
    }

    /// <summary>Takes the card at index of the current offer, and goes on.</summary>
    public void Choose(int index)
    {
        var current = Draft;
        if (current == null || index < 0 || index >= current.Offers.Count) return;
        Card card = current.Offers[index];
        Run.Add(card);
        Effects = Run.Effects;
        CardSystem.Take(Player, card);
        CardSystem.Apply(Player, Effects);
        _events.Add(WorldEvent.CardTaken(card));
        Draft = null;
        if (current.Moment == DraftMoment.BeforeBoss) StartBoss();
    }

    /// <summary>Advances the simulation by delta seconds. Does nothing once the game has ended.</summary>
    public void Step(float delta, PlayerInputComponent input)
    {
        if (Result.HasValue || Draft != null) return;

        bool bossActive = Part == StagePart.Boss;
        if (Part == StagePart.Waves && WaveSystem.Update(Definition.Waves, _waves, Enemies, delta, _events))
        {
            Draft offer = UsesCards ? CardSystem.MakeDraft(DraftMoment.BeforeBoss, Run, Player.Bomb.BombsLeft, _cardRandom) : null;
            if (offer != null) Draft = offer; else StartBoss();
        }

        // 1. Input and spawners
        Player.Input = input;
        BulletSystem.UpdatePlayerWeapons(Player, Bullets, delta, Effects);
        if (bossActive) BulletSystem.UpdateBossWeapons(Boss, Player, Bullets, delta);
        foreach (var enemy in Enemies) EnemySystem.Fire(enemy, Player.Transform.Position, Bullets, PlayArea, delta, Difficulty);

        // 2. Movement
        Vector2 windVector = Wind.CurrentWindVector * Difficulty.WindFactor();
        MovementSystem.MovePlayer(Player, PlayArea, delta, windVector);
        if (bossActive) MovementSystem.MoveBoss(Boss, PlayArea, delta);
        foreach (var enemy in Enemies) EnemySystem.Move(enemy, delta);
        GatherLights(delta);
        CardSystem.MendWard(Player, Effects, delta);
        Enemies.RemoveAll(e => EnemySystem.IsOffScreen(e, PlayArea));
        BulletSystem.MoveBullets(Bullets, PlayArea, delta, windVector, Player, BulletSystem.HomingTargets(Effects, Enemies, Boss, bossActive));

        // 3. Bomb, after everything has moved
        bool bombWasLit = Player.Bomb.IsActive;
        BombSystem.Process(Player, Bullets, delta);
        if (!bombWasLit && Player.Bomb.IsActive) _events.Add(WorldEvent.BombFired());

        // 4. Collisions and health
        DefeatSpirits(delta, bossActive);
        int bossHits = CombatSystem.ProcessCollisions(Bullets, Player, Boss, bossActive, Enemies);
        if (bossHits > 0) _events.Add(WorldEvent.BossHit());
        AddScore(bossHits * ScoreSystem.PointsPerHit);
        if (Player.ShieldBroke)
        {
            Player.ShieldBroke = false;
            _events.Add(WorldEvent.WardBroke());
        }
        UpdateHealth(delta, bossActive);
        BulletSystem.ReportFresh(Bullets, _events);

        if (Player.Health.Lives <= 0)
            Result = GameResult.Defeat;
        else if (bossActive && (Boss.Health.Lives <= 0 || (Boss.Health.Lives < StartingBossLives && PracticePhase > 1)))
            Result = GameResult.Victory;
    }

    /// <summary>The boat gathers the lights that fall to it: energy for the lantern, and the prizes of the score.</summary>
    private void GatherLights(float delta)
    {
        float gathered = LightSystem.Update(Lights, Player.Transform.Position, PlayArea, delta, out int bombs, out int lives, Effects.MagnetMultiplier);
        if (gathered > 0) _events.Add(WorldEvent.LightGathered());
        float spare = PowerSystem.AddEnergy(Player, gathered * Effects.LightValueMultiplier, _events);
        if (spare > 0) AddScore((int)spare * ScoreSystem.PointsPerSpareLight);
        for (int r = 0; r < bombs; r++)
        {
            if (Player.Bomb.BombsLeft < 3) Player.Bomb.BombsLeft++; else AddScore(ScoreSystem.PointsPerSpareBomb);
            _events.Add(WorldEvent.BombRefilled());
        }
        for (int r = 0; r < lives; r++)
        {
            if (Player.Health.Lives < MaxLives) Player.Health.Lives++; else AddScore(ScoreSystem.PointsPerSpareLife);
            _events.Add(WorldEvent.LifeRefilled());
        }
    }

    /// <summary>The bomb and the player's shots hurt the spirits; those that fall let go of their lights.</summary>
    private void DefeatSpirits(float delta, bool bossActive)
    {
        var defeated = CombatSystem.ProcessBombDamage(Player, Enemies, Boss, bossActive, delta, Effects.BombDamageMultiplier);
        var spirits = CombatSystem.ProcessEnemyHits(Bullets, Enemies);
        AddScore(spirits.Hits * ScoreSystem.PointsPerSpiritHit);
        defeated.AddRange(spirits.Defeated);
        // A shot that hurt a spirit without finishing it is heard (one event per kind per step).
        foreach (var kind in spirits.Survivors.Distinct()) _events.Add(WorldEvent.EnemyHit(kind));
        foreach (var defeat in defeated)
        {
            AddScore(defeat.kind.Points());
            _events.Add(WorldEvent.EnemyDefeated(defeat.kind, defeat.position));
            for (int k = 0; k < defeat.kind.Drops(); k++) LightSystem.Spawn(Lights, defeat.position, 1);
        }
        Enemies.RemoveAll(e => e.Health <= 0);
    }

    /// <summary>Hurts and kills the boat, and moves the boss on to its next attack when it has lost a life.</summary>
    private void UpdateHealth(float delta, bool bossActive)
    {
        int livesBefore = Player.Health.Lives;
        Vector2 placeBefore = Player.Transform.Position;
        if (CombatSystem.ProcessHealth(Player, Boss, delta, bossActive))
        {
            AddScore(ScoreSystem.PointsPerPhase);
            _events.Add(WorldEvent.BossBroken());
            BulletSystem.ClearBossBullets(Bullets);
            PhaseManagerSystem.ChangePhase(Boss, Boss.Health.Lives, Difficulty);
            RandomizeWind();
        }
        if (Player.Health.Lives < livesBefore)
        {
            PowerSystem.LoseEnergy(Player, Lights, placeBefore);
            _events.Add(WorldEvent.PlayerHit());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the cards.</summary>
public static partial class SelfTest
{
    private static void RegisterCards(Action<string, Action> T)
    {
        T("AnOfferHasThreeDifferentCards", AnOfferHasThreeDifferentCards);
        T("CardsAtTheirLimitAreNotOffered", CardsAtTheirLimitAreNotOffered);
        T("EveryCardCanShowUp", EveryCardCanShowUp);
        T("CommonCardsComeUpMoreThanRareOnes", CommonCardsComeUpMoreThanRareOnes);
        T("StacksBuildTheEffects", StacksBuildTheEffects);
        T("EveryCardHasATitleAndText", EveryCardHasATitleAndText);
        T("ARunOpensWithACardChoiceAndWaitsForIt", ARunOpensWithACardChoiceAndWaitsForIt);
        T("AnotherChoiceComesBeforeTheBoss", AnotherChoiceComesBeforeTheBoss);
        T("LosingTheRunLosesTheCards", LosingTheRunLosesTheCards);
        T("BossPracticeHasNoCards", BossPracticeHasNoCards);
        T("TakingACardIsReported", TakingACardIsReported);
        T("AWardTakesOneHitInsteadOfALife", AWardTakesOneHitInsteadOfALife);
        T("AMendingWardComesBack", AMendingWardComesBack);
        T("DamageCardsHitHarder", DamageCardsHitHarder);
        T("PiercingBoltsGoThroughSeveralSpirits", PiercingBoltsGoThroughSeveralSpirits);
        T("BurstsHurtTheSpiritsAround", BurstsHurtTheSpiritsAround);
        T("FansAndQuickWicksFireMore", FansAndQuickWicksFireMore);
        T("WispsTurnTowardsTheNearestFoe", WispsTurnTowardsTheNearestFoe);
        T("WispsAreFiredByTheWispCard", WispsAreFiredByTheWispCard);
        T("BombCardsReshapeTheBomb", BombCardsReshapeTheBomb);
        T("ASpareLanternRefillsOneBombButNoMoreThanThree", ASpareLanternRefillsOneBombButNoMoreThanThree);
        T("SmallCardsChangeTheBoat", SmallCardsChangeTheBoat);
        T("LightAndScoreCards", LightAndScoreCards);
    }

    private static void AnOfferHasThreeDifferentCards()
    {
        for (ulong seed = 0; seed < 50; seed++)
        {
            var offers = CardDeck.Offers(new RunState(), 2, new SeededGenerator(seed));
            Eq(offers.Count, 3);
            Eq(offers.Distinct().Count(), 3);
        }
    }

    private static void CardsAtTheirLimitAreNotOffered()
    {
        var run = new RunState();
        foreach (var card in Enum.GetValues<Card>().Where(c => c != Card.SpareLantern))
            for (int i = 0; i < CardInfo.MaxStacks(card); i++) run.Add(card);
        var rng = new SeededGenerator(1);
        True(CardDeck.Offers(run, 1, rng).SequenceEqual(new[] { Card.SpareLantern }));
        Eq(CardDeck.Offers(run, 3, rng).Count, 0, "a full lantern needs no spare");
        run.Add(Card.SlimWake);
        Eq(run.Count(Card.SlimWake), 1, "a card cannot be stacked past its limit");
    }

    private static void EveryCardCanShowUp()
    {
        var seen = new HashSet<Card>();
        var rng = new SeededGenerator(7);
        for (int i = 0; i < 400; i++) foreach (var c in CardDeck.Offers(new RunState(), 1, rng)) seen.Add(c);
        Eq(seen.Count, Enum.GetValues<Card>().Length);
    }

    private static void CommonCardsComeUpMoreThanRareOnes()
    {
        var rng = new SeededGenerator(3);
        var counts = new Dictionary<CardRarity, int>();
        for (int i = 0; i < 600; i++)
            foreach (var c in CardDeck.Offers(new RunState(), 1, rng))
            {
                var r = CardInfo.Rarity(c);
                counts[r] = (counts.TryGetValue(r, out int v) ? v : 0) + 1;
            }
        double PerCard(CardRarity r) => (counts.TryGetValue(r, out int v) ? v : 0) / (double)Enum.GetValues<Card>().Count(c => CardInfo.Rarity(c) == r);
        Greater((float)PerCard(CardRarity.Common), (float)PerCard(CardRarity.Rare));
    }

    private static void StacksBuildTheEffects()
    {
        var run = new RunState();
        run.Add(Card.Fervor); run.Add(Card.Fervor); run.Add(Card.TwinFan); run.Add(Card.CalmSails);
        Near(run.Effects.DamageMultiplier, 1.5f, 0.001f);
        Eq(run.Effects.FanPairs, 1);
        Near(run.Effects.WindMultiplier, 0.6f, 0.001f);
        True(new RunState().Effects == new CardEffects());
        var owned = run.Owned;
        Eq(owned[0].card, Card.Fervor);
        Eq(owned[0].count, 2);
    }

    private static void EveryCardHasATitleAndText()
    {
        foreach (var card in Enum.GetValues<Card>())
        {
            False(string.IsNullOrEmpty(CardInfo.Title(card)));
            False(string.IsNullOrEmpty(CardInfo.Detail(card)));
        }
    }

    private static void ARunOpensWithACardChoiceAndWaitsForIt()
    {
        var world = new World(seed: 1);
        Eq(world.Draft?.Moment, (DraftMoment?)DraftMoment.StageStart);
        Eq(world.Draft?.Offers.Count, (int?)3);
        var input = Idle(); input.Direction = V(1, 0);
        float x = world.Player.Transform.Position.X;
        StepN(world, 30, input);
        Eq(world.Player.Transform.Position.X, x, "nothing moves while the player chooses");
        world.Choose(1);
        True(world.Draft == null);
        Eq(world.Run.TotalCards, 1);
        StepN(world, 30, input);
        Greater(world.Player.Transform.Position.X, x);
    }

    private static void AnotherChoiceComesBeforeTheBoss()
    {
        var world = new World(seed: 2);
        world.Choose(0);
        world.CalmWindForTesting();
        world.FinishWavesForTesting();
        StepN(world, 5);
        Eq(world.Draft?.Moment, (DraftMoment?)DraftMoment.BeforeBoss);
        Eq(world.Part, StagePart.Waves, "the boss waits for the choice");
        world.Choose(0);
        Eq(world.Part, StagePart.Boss);
        Eq(world.Run.TotalCards, 2, "cards pile up");
    }

    private static void LosingTheRunLosesTheCards()
    {
        var world = new World(seed: 4);
        world.GiveForTesting(Card.Fervor);
        world.GiveForTesting(Card.SpiritWard);
        Eq(world.Run.TotalCards, 2);
        world.Reset();
        Eq(world.Run.TotalCards, 0);
        True(world.Effects == new CardEffects());
        Eq(world.Player.Shield, 0);
        Eq(world.Draft?.Moment, (DraftMoment?)DraftMoment.StageStart, "a new run starts with a new choice");
    }

    private static void BossPracticeHasNoCards()
    {
        var world = new World(practicePhase: 2);
        True(world.Draft == null);
        world.Step(Dt, Idle());
        Eq(world.Run.TotalCards, 0);
    }

    private static void TakingACardIsReported()
    {
        var world = new World(seed: 5);
        var offered = world.Draft.Offers[2];
        world.Choose(2);
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.CardTaken && e.Card == offered));
    }

    private static World Armed(params Card[] cards)
    {
        var world = new World(withCards: false);
        world.CalmWindForTesting();
        world.HoldWavesForTesting();
        foreach (var c in cards) world.GiveForTesting(c);
        return world;
    }

    private static void AWardTakesOneHitInsteadOfALife()
    {
        var world = Armed(Card.SpiritWard);
        Eq(world.Player.Shield, 1);
        BulletSystem.Spawn(world.Bullets, world.Player.Transform.Position, Vector2.Zero, Faction.Boss, BulletStyle.OrbRed);
        world.Step(Dt, Idle());
        Eq(world.Player.Health.Lives, 3);
        Eq(world.Player.Shield, 0);
        True(Has(world.TakeEvents(), WorldEventKind.WardBroke));
        // Now it is gone.
        StepN(world, 80);
        BulletSystem.Spawn(world.Bullets, world.Player.Transform.Position, Vector2.Zero, Faction.Boss, BulletStyle.OrbRed);
        world.Step(Dt, Idle());
        Eq(world.Player.Health.Lives, 2);
    }

    private static void AMendingWardComesBack()
    {
        var world = Armed(Card.MendingWard);
        world.Player.Shield = 0;
        StepN(world, (int)(CardSystem.WardMendTime * 60) + 5);
        Eq(world.Player.Shield, 1);
    }

    private static void DamageCardsHitHarder()
    {
        int Lost(params Card[] cards)
        {
            var w = new World(practicePhase: 1);
            foreach (var c in cards) w.GiveForTesting(c);
            w.CalmWindForTesting();
            w.Boss.Transform.Position = V(300, Layout.BossY);
            w.Boss.MovementType = BossMovementType.Stationary;
            w.Boss.EmitterMain.IsActive = false; w.Boss.EmitterSub.IsActive = false;
            int before = w.Boss.Health.CurrentHealth;
            for (int i = 0; i < 2; i++)
                BulletSystem.Spawn(w.Bullets, w.Boss.Transform.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt, power: w.Effects.DamageMultiplier);
            w.Step(Dt, Idle());
            return before - w.Boss.Health.CurrentHealth;
        }
        Eq(Lost(), 2);
        Eq(Lost(Card.Fervor, Card.Fervor, Card.Fervor), 3);   // x1.75 of two shots
    }

    private static void PiercingBoltsGoThroughSeveralSpirits()
    {
        var world = Armed(Card.PiercingLight, Card.PiercingLight);
        var a = Spirit(V(300, 300), 501); var b = Spirit(V(300, 280), 502);
        world.AddForTesting(a); world.AddForTesting(b);
        BulletSystem.Spawn(world.Bullets, V(300, 306), V(0, -800), Faction.Player, BulletStyle.LightBolt, pierce: world.Effects.Pierce);
        StepN(world, 12);
        Eq(a.Health, SpiritKind.Heavy.MaxHealth() - 1, "hit once, not on every step");
        Eq(b.Health, SpiritKind.Heavy.MaxHealth() - 1);
    }

    private static void BurstsHurtTheSpiritsAround()
    {
        var world = Armed(Card.BurstingBolts);
        var a = Spirit(V(300, 300), 501); var b = Spirit(V(330, 300), 502);
        world.AddForTesting(a); world.AddForTesting(b);
        BulletSystem.Spawn(world.Bullets, a.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt, power: 2, burstRadius: world.Effects.BurstRadius);
        world.Step(Dt, Idle());
        Less(b.Health, SpiritKind.Heavy.MaxHealth(), "the neighbour was scorched");
        Less(a.Health, b.Health);
    }

    private static void FansAndQuickWicksFireMore()
    {
        int Volley(CardEffects effects)
        {
            var pool = Pool(100);
            var p = new PlayerEntity();
            p.Input.IsShooting = true;
            p.Weapon.TimeSinceLastFire = 1;
            BulletSystem.UpdatePlayerWeapons(p, pool, 0.1f, effects);
            return ActiveCount(pool);
        }
        var run = new RunState(); run.Add(Card.TwinFan);
        Eq(Volley(new CardEffects()), 1);
        Eq(Volley(run.Effects), 3);
        var world = Armed(Card.QuickWick, Card.QuickWick);
        Near(world.Player.Weapon.FireRate, 0.1f / 1.4f, 0.0001f);
    }

    private static void WispsTurnTowardsTheNearestFoe()
    {
        var pool = Pool(10);
        BulletSystem.Spawn(pool, V(300, 500), V(0, -420), Faction.Player, BulletStyle.Wisp, behavior: BulletBehavior.Homing);
        var player = new PlayerEntity();
        for (int i = 0; i < 60; i++)
            BulletSystem.MoveBullets(pool, World.PlayArea, 1f / 60, Vector2.Zero, player, new List<Vector2> { V(500, 400) });
        Greater(pool[0].Position.X, 330, "it swung towards the foe on the right");
    }

    private static void WispsAreFiredByTheWispCard()
    {
        var world = Armed(Card.HomingWisp);
        var input = Idle(); input.IsShooting = true;
        StepN(world, 40, input);
        True(world.Bullets.Any(b => b.IsActive && b.Style == BulletStyle.Wisp));
    }

    private static void BombCardsReshapeTheBomb()
    {
        var world = Armed(Card.BrighterLight, Card.LongBurn, Card.SearingLight);
        Near(world.Player.Bomb.Radius, 300, 0.01f);
        Near(world.Player.Bomb.Duration, 6f, 0.01f);
        Near(world.Effects.BombDamageMultiplier, 1.5f, 0.001f);
        var plain = Armed();
        var strong = Armed(Card.SearingLight, Card.SearingLight); // x2
        foreach (var w in new[] { plain, strong })
        {
            w.Player.Bomb.IsActive = true;
            w.Player.Bomb.Timer = 5;
            var e = Spirit(w.Player.Transform.Position + V(0, -50)); w.AddForTesting(e);
            StepN(w, 30);
        }
        Less(strong.Enemies.Count > 0 ? strong.Enemies[0].Health : 0, plain.Enemies.Count > 0 ? plain.Enemies[0].Health : 99);
    }

    private static void ASpareLanternRefillsOneBombButNoMoreThanThree()
    {
        var world = Armed();
        world.Player.Bomb.BombsLeft = 1;
        world.GiveForTesting(Card.SpareLantern);
        Eq(world.Player.Bomb.BombsLeft, 2);
        world.GiveForTesting(Card.SpareLantern); world.GiveForTesting(Card.SpareLantern);
        Eq(world.Player.Bomb.BombsLeft, 3);
    }

    private static void SmallCardsChangeTheBoat()
    {
        var world = Armed(Card.SlimWake, Card.CalmSails);
        Eq(world.Player.Hitbox.Radius, 3f);
        Near(world.Player.WindModifier.Value, 0.6f, 0.001f);
    }

    private static void LightAndScoreCards()
    {
        var plain = Armed();
        var rich = Armed(Card.HungryLantern, Card.HungryLantern, Card.GoldenSpirits);
        foreach (var w in new[] { plain, rich })
        {
            var items = LightSystem.NewPool();
            items[0] = new LightItem { IsActive = true, Position = w.Player.Transform.Position, Velocity = Vector2.Zero, Value = 2, Age = 1 };
            w.SetLightsForTesting(items);
            w.Step(Dt, Idle());
        }
        Eq(plain.Player.Energy, 2f);
        Eq(rich.Player.Energy, 4f);
        Near(rich.Effects.ScoreMultiplier, 1.25f, 0.001f);
    }
}

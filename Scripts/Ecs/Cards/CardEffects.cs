using System;
using System.Collections.Generic;

/// <summary>What the cards together do to the player. Read by the weapon, the bomb, the lights and the score.</summary>
public struct CardEffects : IEquatable<CardEffects>
{
    // Weapon
    public int HomingWisps;
    public int FanPairs;
    public int Pierce;
    public float DamageMultiplier;
    public float FireRateMultiplier;
    public float BurstRadius;
    // Defence
    public bool MendingWard;
    public float HitboxRadius;
    public float WindMultiplier;
    // Bomb
    public float BombRadiusMultiplier;
    public float BombDurationBonus;
    public float BombDamageMultiplier;
    // Gathering and scoring
    public float LightValueMultiplier;
    public float MagnetMultiplier;
    public float ScoreMultiplier;

    public CardEffects()
    {
        HomingWisps = 0; FanPairs = 0; Pierce = 0; DamageMultiplier = 1; FireRateMultiplier = 1; BurstRadius = 0;
        MendingWard = false; HitboxRadius = 4; WindMultiplier = 1;
        BombRadiusMultiplier = 1; BombDurationBonus = 0; BombDamageMultiplier = 1;
        LightValueMultiplier = 1; MagnetMultiplier = 1; ScoreMultiplier = 1;
    }

    public CardEffects(Dictionary<Card, int> stacks) : this()
    {
        float n(Card card) => stacks.TryGetValue(card, out int v) ? v : 0;
        HomingWisps = (int)n(Card.HomingWisp);
        FanPairs = (int)n(Card.TwinFan);
        Pierce = (int)n(Card.PiercingLight);
        DamageMultiplier = 1 + 0.25f * n(Card.Fervor);
        FireRateMultiplier = 1 + 0.2f * n(Card.QuickWick);
        BurstRadius = n(Card.BurstingBolts) > 0 ? 34 + 14 * n(Card.BurstingBolts) : 0;
        MendingWard = n(Card.MendingWard) > 0;
        HitboxRadius = n(Card.SlimWake) > 0 ? 3 : 4;
        WindMultiplier = MathF.Max(0.1f, 1 - 0.4f * n(Card.CalmSails));
        BombRadiusMultiplier = 1 + 0.2f * n(Card.BrighterLight);
        BombDurationBonus = 1.0f * n(Card.LongBurn);
        BombDamageMultiplier = 1 + 0.5f * n(Card.SearingLight);
        LightValueMultiplier = 1 + 0.5f * n(Card.HungryLantern);
        MagnetMultiplier = 1 + 0.4f * n(Card.WideNet);
        ScoreMultiplier = 1 + 0.25f * n(Card.GoldenSpirits);
    }

    public bool Equals(CardEffects o) =>
        HomingWisps == o.HomingWisps && FanPairs == o.FanPairs && Pierce == o.Pierce && DamageMultiplier == o.DamageMultiplier
        && FireRateMultiplier == o.FireRateMultiplier && BurstRadius == o.BurstRadius && MendingWard == o.MendingWard
        && HitboxRadius == o.HitboxRadius && WindMultiplier == o.WindMultiplier && BombRadiusMultiplier == o.BombRadiusMultiplier
        && BombDurationBonus == o.BombDurationBonus && BombDamageMultiplier == o.BombDamageMultiplier
        && LightValueMultiplier == o.LightValueMultiplier && MagnetMultiplier == o.MagnetMultiplier && ScoreMultiplier == o.ScoreMultiplier;
    public override bool Equals(object obj) => obj is CardEffects o && Equals(o);
    public override int GetHashCode() => HashCode.Combine(HomingWisps, FanPairs, Pierce, DamageMultiplier, FireRateMultiplier, BurstRadius, MendingWard);
    public static bool operator ==(CardEffects a, CardEffects b) => a.Equals(b);
    public static bool operator !=(CardEffects a, CardEffects b) => !a.Equals(b);
}

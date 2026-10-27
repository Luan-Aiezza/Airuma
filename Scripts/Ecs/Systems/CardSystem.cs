using System;

/// <summary>What the cards do to the player once taken.</summary>
public static class CardSystem
{
    /// <summary>Seconds a mending ward takes to mend itself.</summary>
    public const float WardMendTime = 45;

    /// <summary>The one-off effect of taking a card (a ward to stand in front, a spare lantern); the lasting ones are in the effects.</summary>
    public static void Take(PlayerEntity player, Card card)
    {
        switch (card)
        {
            case Card.SpiritWard: case Card.MendingWard: player.Shield += 1; break;
            case Card.SpareLantern: player.Bomb.BombsLeft = Math.Min(3, player.Bomb.BombsLeft + 1); break;
        }
    }

    /// <summary>Puts the lasting effects of the cards on the player.</summary>
    public static void Apply(PlayerEntity player, CardEffects effects)
    {
        player.Weapon.FireRate = 0.1f / effects.FireRateMultiplier;
        player.Hitbox.Radius = effects.HitboxRadius;
        player.WindModifier.Value = effects.WindMultiplier;
        player.Bomb.Radius = 250 * effects.BombRadiusMultiplier;
        player.Bomb.Duration = 5 + effects.BombDurationBonus;
    }

    /// <summary>A mending ward that has been used mends itself after a while.</summary>
    public static void MendWard(PlayerEntity player, CardEffects effects, float delta)
    {
        if (!(effects.MendingWard && player.Shield == 0))
        {
            player.WardClock = 0;
            return;
        }
        player.WardClock += delta;
        if (player.WardClock >= WardMendTime)
        {
            player.WardClock = 0;
            player.Shield = 1;
        }
    }

    /// <summary>Three cards to choose from, or null if there are none worth offering.</summary>
    public static Draft MakeDraft(DraftMoment moment, RunState run, int bombsLeft, SeededGenerator rng)
    {
        var offers = CardDeck.Offers(run, bombsLeft, rng);
        return offers.Count == 0 ? null : new Draft(moment, offers);
    }
}

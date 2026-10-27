using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>The cards taken so far in this run.</summary>
public class RunState
{
    private readonly Dictionary<Card, int> _stacks = new Dictionary<Card, int>();

    public CardEffects Effects => new CardEffects(_stacks);
    public int TotalCards => _stacks.Values.Sum();

    /// <summary>The most cards a whole run can gather (the HUD keeps this many places for them).</summary>
    public const int MaxCards = 12;

    private readonly List<Card> _history = new List<Card>();
    /// <summary>The cards in the order they were taken (one entry for each time a card was taken).</summary>
    public IReadOnlyList<Card> History => _history;

    public int Count(Card card) => _stacks.TryGetValue(card, out int v) ? v : 0;

    /// <summary>Cards in the order they would be listed: the most stacked first.</summary>
    public List<(Card card, int count)> Owned =>
        _stacks.Select(kv => (card: kv.Key, count: kv.Value))
               .OrderByDescending(t => t.count).ThenBy(t => CardInfo.CaseName(t.card), StringComparer.Ordinal).ToList();

    public void Add(Card card)
    {
        int before = Count(card);
        _stacks[card] = Math.Min(CardInfo.MaxStacks(card), before + 1);
        if (_stacks[card] > before) _history.Add(card);
    }

    public override bool Equals(object obj) =>
        obj is RunState o && o._stacks.Count == _stacks.Count && _stacks.All(kv => o.Count(kv.Key) == kv.Value);
    public override int GetHashCode() => _stacks.Count;
}

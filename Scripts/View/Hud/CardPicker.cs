using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>The screen where the player chooses one of three cards. Covers the river; the side panel stays visible.
/// Screen space (Y down); add it at the origin with <c>area = (Layout.PanelX, Layout.Height)</c>. ZIndex 150.</summary>
public partial class CardPicker : Node2D
{
    public static readonly Vector2 CardSize = new Vector2(CardArt.Width * CardArt.PixelScale, CardArt.Height * CardArt.PixelScale);
    public const float Spacing = 224;
    /// <summary>Letters per line on a card.</summary>
    public const int LineLength = 13;

    private readonly ColorRect _shade = new ColorRect { Color = new Color(0.02f, 0.04f, 0.08f, 0.78f), MouseFilter = Godot.Control.MouseFilterEnum.Ignore };
    private readonly PixelLabel _heading = new(PixelLabel.Style.MenuHeading, 4, PixelLabel.Align.Center);
    private readonly PixelLabel _subheading = new(PixelLabel.Style.MenuDim, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _hint = new(PixelLabel.Style.MenuDim, 2, PixelLabel.Align.Center);
    private readonly PixelLabel _ownedTitle = new(PixelLabel.Style.MenuDim, 2, PixelLabel.Align.Left);
    private readonly List<Node2D> _cardNodes = new();
    private readonly List<Node2D> _ownedNodes = new();
    public List<Card> Offers { get; private set; } = new();
    public int Selected { get; private set; }

    private readonly float _areaWidth, _areaHeight;

    public bool IsShown => Visible && !_closing;

    // The picker eases in (the shade darkens, the title fades in and the cards rise one after another) and eases out.
    private float _appear;
    private bool _closing;
    private float _closeTime;
    private const float AppearTime = 0.35f, CloseTime = 0.22f, CardStagger = 0.09f, CardDelay = 0.12f;

    private static int Z(float z) => (int)Math.Round(z * 10);

    public CardPicker(Vector2 area)
    {
        _areaWidth = area.X;
        _areaHeight = area.Y;
        ZIndex = Z(15);
        Visible = false;

        _shade.Size = area;
        AddChild(_shade);

        _heading.Position = new Vector2(area.X / 2, 70);
        _subheading.Position = new Vector2(area.X / 2, 118);
        foreach (var label in new[] { _heading, _subheading })
        {
            label.ZIndex = Z(1);
            AddChild(label);
        }
        _hint.Position = new Vector2(area.X / 2, area.Y - 28);
        _hint.ZIndex = Z(1);
        _hint.SetText(UIText.CardsHint);
        AddChild(_hint);
        _ownedTitle.Position = new Vector2(28, area.Y - 92);
        _ownedTitle.ZIndex = Z(1);
        AddChild(_ownedTitle);
    }

    private float CardCentreX(int index, int count) => _areaWidth / 2 + (index - (count - 1) / 2f) * Spacing;

    private float CardTop => 160;

    private static int Len(string s) => new StringInfo(s).LengthInTextElements;

    /// <summary>Words wrapped to lines of at most <paramref name="limit"/> letters.</summary>
    public static List<string> Wrap(string text, int limit = LineLength)
    {
        var lines = new List<string>();
        string line = "";
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length == 0) line = word;
            else if (Len(line) + 1 + Len(word) <= limit) line += " " + word;
            else { lines.Add(line); line = word; }
        }
        if (line.Length > 0) lines.Add(line);
        return lines;
    }

    public void Show(Draft draft, IEnumerable<(Card card, int count)> owned)
    {
        var ownedList = owned.ToList();
        Offers = draft.Offers;
        Selected = 0;
        _heading.SetText(UIText.ChooseCard);
        _subheading.SetText(draft.Moment == DraftMoment.StageStart ? UIText.ChooseCardStart : UIText.ChooseCardBoss);

        foreach (var n in _cardNodes) n.QueueFree();
        _cardNodes.Clear();
        for (int index = 0; index < Offers.Count; index++)
        {
            var node = MakeCard(Offers[index]);
            node.Position = new Vector2(CardCentreX(index, Offers.Count), CardTop);
            node.ZIndex = Z(1);
            AddChild(node);
            _cardNodes.Add(node);
        }

        foreach (var n in _ownedNodes) n.QueueFree();
        _ownedNodes.Clear();
        _ownedTitle.SetText(ownedList.Count == 0 ? "" : UIText.YourCards);
        for (int i = 0; i < Math.Min(14, ownedList.Count); i++)
        {
            var entry = ownedList[i];
            var chip = new Node2D();
            chip.AddChild(new Sprite2D { Texture = CardArt.Chip(entry.card).ToTexture(), Scale = new Vector2(3, 3) });
            if (entry.count > 1)
            {
                var count = new PixelLabel(PixelLabel.Style.Gold, 2, PixelLabel.Align.Right);
                count.SetText(entry.count.ToString(CultureInfo.InvariantCulture));
                count.Position = new Vector2(24, 4);
                count.ZIndex = Z(1);
                chip.AddChild(count);
            }
            chip.Position = new Vector2(52 + i * 54, _areaHeight - 52);
            chip.ZIndex = Z(1);
            AddChild(chip);
            _ownedNodes.Add(chip);
        }

        Visible = true;
        _appear = 0;
        _closing = false;
        Highlight();
        ApplyAppearance();
    }

    /// <summary>Fades the picker out.</summary>
    public void Hide()
    {
        if (!Visible || _closing) return;
        _closing = true;
        _closeTime = 0;
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        float dt = (float)Math.Min(delta, 0.1);
        if (_closing)
        {
            _closeTime += dt;
            if (_closeTime >= CloseTime) { Visible = false; _closing = false; return; }
        }
        else _appear += dt;
        ApplyAppearance();
    }

    private void ApplyAppearance()
    {
        float global = _closing ? 1 - _closeTime / CloseTime : Mathf.Clamp(_appear / AppearTime, 0, 1);
        _shade.Modulate = new Color(1, 1, 1, global);
        foreach (var label in new CanvasItem[] { _heading, _subheading, _hint, _ownedTitle }) label.Modulate = new Color(1, 1, 1, global);
        foreach (var chip in _ownedNodes) chip.Modulate = new Color(1, 1, 1, global);
        for (int i = 0; i < _cardNodes.Count; i++)
        {
            float u = Mathf.Clamp((_appear - CardDelay - i * CardStagger) / 0.35f, 0, 1);
            float eased = 1 - (1 - u) * (1 - u) * (1 - u);
            float alpha = (i == Selected ? 1f : 0.7f) * eased * (_closing ? global : 1);
            _cardNodes[i].Modulate = new Color(1, 1, 1, alpha);
            _cardNodes[i].Position = new Vector2(CardCentreX(i, _cardNodes.Count), CardTop + (1 - eased) * 70);
        }
    }

    private Node2D MakeCard(Card card)
    {
        var node = new Node2D();
        var tex = CardArt.Face(card).ToTexture();
        node.AddChild(new Sprite2D
        {
            Texture = tex, Centered = false, Scale = new Vector2(CardArt.PixelScale, CardArt.PixelScale),
            Offset = new Vector2(-tex.GetWidth() / 2f, 0),
        });

        float top = 49 * CardArt.PixelScale; // under the icon's window
        foreach (var line in Wrap(CardInfo.Title(card)))
        {
            var label = new PixelLabel(PixelLabel.Style.Gold, 2, PixelLabel.Align.Center);
            label.SetText(line);
            label.Position = new Vector2(0, top);
            label.ZIndex = Z(1);
            node.AddChild(label);
            top += 20;
        }
        top += 8;
        foreach (var line in Wrap(CardInfo.Detail(card)))
        {
            var label = new PixelLabel(PixelLabel.Style.Label, 2, PixelLabel.Align.Center);
            label.SetText(line);
            label.Position = new Vector2(0, top);
            label.ZIndex = Z(1);
            node.AddChild(label);
            top += 20;
        }
        return node;
    }

    /// <summary>The raised card is the one that will be taken.</summary>
    private void Highlight()
    {
        for (int i = 0; i < _cardNodes.Count; i++)
        {
            bool chosen = i == Selected;
            var node = _cardNodes[i];
            node.Scale = chosen ? new Vector2(1.06f, 1.06f) : new Vector2(0.96f, 0.96f);
            node.ZIndex = Z(chosen ? 2 : 1);
        }
    }

    public void Move(int step)
    {
        if (Offers.Count == 0) return;
        Selected = ((Selected + step) % Offers.Count + Offers.Count) % Offers.Count;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove);
        Highlight();
    }

    public void Select(int index)
    {
        if (index < 0 || index >= Offers.Count) return;
        Selected = index;
        Highlight();
    }

    /// <summary>The card under a point of the screen (picker coordinates), if any.</summary>
    public int? CardIndexAt(Vector2 point)
    {
        for (int i = 0; i < _cardNodes.Count; i++)
        {
            var local = point - _cardNodes[i].Position;
            if (Math.Abs(local.X) <= CardSize.X / 2 && local.Y >= 0 && local.Y <= CardSize.Y) return i;
        }
        return null;
    }
}

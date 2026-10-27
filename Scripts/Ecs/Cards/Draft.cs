using System.Collections.Generic;

public enum DraftMoment { StageStart, BeforeBoss }

/// <summary>A choice of cards the player has to make before the game goes on.</summary>
public class Draft
{
    public DraftMoment Moment;
    public List<Card> Offers;
    public Draft(DraftMoment moment, List<Card> offers) { Moment = moment; Offers = offers; }
}

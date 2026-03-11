namespace RPSPS.Models;

public sealed class Round
{
    public Move HomeMove { get; }
    public Move AwayMove { get; }
    public RoundResult Result { get; }

    public Round(Move homeMove, Move awayMove)
    {
        HomeMove = homeMove;
        AwayMove = awayMove;
        Result = Resolve(homeMove, awayMove);
    }

    private static RoundResult Resolve(Move home, Move away)
    {
        if (home == away) return RoundResult.Draw;
        return home.Beats(away) ? RoundResult.HomeWin : RoundResult.AwayWin;
    }
}

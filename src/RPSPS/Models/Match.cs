namespace RPSPS.Models;

using RPSPS.Players;

public sealed class Match
{
    private const int WinsNeeded = 3;

    public Player Home { get; }
    public Player Away { get; }
    public List<Round> Rounds { get; } = new();

    public Match(Player home, Player away)
    {
        Home = home;
        Away = away;
    }

    public MatchResult Play()
    {
        int homeWins = 0, awayWins = 0, draws = 0;

        while (homeWins < WinsNeeded && awayWins < WinsNeeded)
        {
            var homeMove = Home.ChooseMove();
            var awayMove = Away.ChooseMove();

            var round = new Round(homeMove, awayMove);
            Rounds.Add(round);

            switch (round.Result)
            {
                case RoundResult.Draw:
                    draws++;
                    break;
                case RoundResult.HomeWin:
                    homeWins++;
                    break;
                case RoundResult.AwayWin:
                    awayWins++;
                    break;
            }

            Home.RecordOpponentMove(awayMove);
            Away.RecordOpponentMove(homeMove);
        }

        return new MatchResult(Home.Name, Away.Name, homeWins, awayWins, draws, Rounds);
    }

    // Convenience for callers that don't need the Match instance
    public static MatchResult Play(Player home, Player away) => new Match(home, away).Play();
}

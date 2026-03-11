namespace RPSPS.Models;

using RPSPS.Players;

public sealed class Match
{
    private const int WinsNeeded = 3;

    public static MatchResult Play(Player home, Player away)
    {
        int homeWins = 0, awayWins = 0, draws = 0, roundCount = 0;

        home.Reset();
        away.Reset();

        while (homeWins < WinsNeeded && awayWins < WinsNeeded)
        {
            var homeMove = home.ChooseMove();
            var awayMove = away.ChooseMove();

            roundCount++;

            // Inline resolution — avoid Round allocation in hot path
            if (homeMove == awayMove)
            {
                draws++;
            }
            else if (homeMove.Beats(awayMove))
            {
                homeWins++;
            }
            else
            {
                awayWins++;
            }

            // Record opponent moves for strategy tracking
            home.RecordOpponentMove(awayMove);
            away.RecordOpponentMove(homeMove);
        }

        return new MatchResult(home.Name, away.Name, homeWins, awayWins, draws, roundCount);
    }
}

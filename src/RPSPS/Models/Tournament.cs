namespace RPSPS.Models;

using RPSPS.Players;

public sealed class TournamentResult
{
    public int MatchCount { get; }
    public int TotalRounds { get; }
    public List<MatchResult> MatchResults { get; }
    public PlayerStanding[] Standings { get; }

    public TournamentResult(int matchCount, int totalRounds, List<MatchResult> matchResults, PlayerStanding[] standings)
    {
        MatchCount = matchCount;
        TotalRounds = totalRounds;
        MatchResults = matchResults;
        Standings = standings;
    }
}

public sealed class PlayerStanding
{
    public string PlayerName { get; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GameDifferential => GamesWon - GamesLost;
    public double WinRate => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) : 0;

    public PlayerStanding(string playerName)
    {
        PlayerName = playerName;
    }
}

public sealed class Tournament
{
    public static TournamentResult Run(Player[] players)
    {
        var standings = new PlayerStanding[players.Length];
        for (int i = 0; i < players.Length; i++)
            standings[i] = new PlayerStanding(players[i].Name);

        var matchResults = new List<MatchResult>();
        int totalRounds = 0;

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                // Fresh player clones per match — no history leaks
                var home = players[i].Clone();
                var away = players[j].Clone();

                var match = new Match(home, away);
                var result = match.Play();
                matchResults.Add(result);
                totalRounds += result.RoundCount;

                if (result.HomeWins > result.AwayWins)
                {
                    standings[i].Wins++;
                    standings[j].Losses++;
                }
                else
                {
                    standings[j].Wins++;
                    standings[i].Losses++;
                }

                standings[i].GamesWon += result.HomeWins;
                standings[i].GamesLost += result.AwayWins;
                standings[j].GamesWon += result.AwayWins;
                standings[j].GamesLost += result.HomeWins;
            }
        }

        return new TournamentResult(matchResults.Count, totalRounds, matchResults, standings);
    }
}

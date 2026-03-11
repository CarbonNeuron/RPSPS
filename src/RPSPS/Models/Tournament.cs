namespace RPSPS.Models;

using RPSPS.Players;

public sealed class TournamentResult
{
    public int MatchCount { get; }
    public int TotalRounds { get; }
    public PlayerStanding[] Standings { get; }

    public TournamentResult(int matchCount, int totalRounds, PlayerStanding[] standings)
    {
        MatchCount = matchCount;
        TotalRounds = totalRounds;
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

        int matchCount = 0;
        int totalRounds = 0;

        for (int i = 0; i < players.Length; i++)
        {
            for (int j = i + 1; j < players.Length; j++)
            {
                var result = Match.Play(players[i], players[j]);
                matchCount++;
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

        return new TournamentResult(matchCount, totalRounds, standings);
    }
}

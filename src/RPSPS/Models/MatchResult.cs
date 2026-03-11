namespace RPSPS.Models;

public sealed class MatchResult
{
    public string HomePlayerName { get; }
    public string AwayPlayerName { get; }
    public int HomeWins { get; }
    public int AwayWins { get; }
    public int Draws { get; }
    public int RoundCount { get; }
    public List<Round> Rounds { get; }
    public string WinnerName => HomeWins > AwayWins ? HomePlayerName : AwayPlayerName;

    public MatchResult(string homePlayerName, string awayPlayerName, int homeWins, int awayWins, int draws, List<Round> rounds)
    {
        HomePlayerName = homePlayerName;
        AwayPlayerName = awayPlayerName;
        HomeWins = homeWins;
        AwayWins = awayWins;
        Draws = draws;
        Rounds = rounds;
        RoundCount = rounds.Count;
    }
}

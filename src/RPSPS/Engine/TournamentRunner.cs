namespace RPSPS.Engine;

using RPSPS.Models;
using RPSPS.Players;

public sealed class TournamentRunner
{
    private readonly int _baseSeed;
    private readonly Player[] _players;

    public TournamentRunner(int baseSeed)
    {
        _baseSeed = baseSeed;
        _players = CreatePlayers(baseSeed);
    }

    public TournamentResult RunTournament(int iteration)
    {
        return Tournament.Run(_players);
    }

    public static Player[] CreatePlayers(int seed)
    {
        return
        [
            new RandomPlayer(seed),
            new PatternPlayer(seed + 1),
            new FrequencyPlayer(seed + 2),
            new MarkovPlayer(seed + 3)
        ];
    }
}

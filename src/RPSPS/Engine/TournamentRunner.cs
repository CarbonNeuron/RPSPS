namespace RPSPS.Engine;

using RPSPS.Models;
using RPSPS.Players;

public sealed class TournamentRunner
{
    private readonly int _baseSeed;
    private readonly Player[] _players;

    public TournamentRunner(int baseSeed, GameMode gameMode = GameMode.Classic)
    {
        _baseSeed = baseSeed;
        _players = CreatePlayers(baseSeed, gameMode);
    }

    public TournamentResult RunTournament(int iteration)
    {
        return Tournament.Run(_players);
    }

    public static Player[] CreatePlayers(int seed, GameMode gameMode = GameMode.Classic)
    {
        int moveCount = gameMode.MoveCount();
        return
        [
            new RandomPlayer(seed, moveCount),
            new PatternPlayer(seed + 1, moveCount),
            new FrequencyPlayer(seed + 2, moveCount),
            new MarkovPlayer(seed + 3, moveCount)
        ];
    }
}

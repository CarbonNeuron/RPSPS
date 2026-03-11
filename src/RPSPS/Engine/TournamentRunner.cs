namespace RPSPS.Engine;

using RPSPS.Models;
using RPSPS.Players;

public sealed class TournamentRunner
{
    private readonly Player[] _templatePlayers;

    public TournamentRunner(int baseSeed, GameMode gameMode = GameMode.Classic)
    {
        _templatePlayers = CreatePlayers(baseSeed, gameMode);
    }

    public TournamentResult RunTournament(int iteration)
    {
        // Fresh player instances each tournament — cloned from templates
        var players = new Player[_templatePlayers.Length];
        for (int i = 0; i < _templatePlayers.Length; i++)
            players[i] = _templatePlayers[i].Clone();

        return Tournament.Run(players);
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

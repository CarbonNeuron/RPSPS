using BenchmarkDotNet.Attributes;
using RPSPS.Engine;
using RPSPS.Models;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class TournamentBenchmarks
{
    [Params("classic", "spock")]
    public string GameModeName { get; set; } = "classic";

    private GameMode _gameMode;

    [GlobalSetup]
    public void Setup()
    {
        _gameMode = GameModeName == "spock" ? GameMode.Spock : GameMode.Classic;
    }

    [Benchmark]
    public TournamentResult RunSingleTournament()
    {
        var players = TournamentRunner.CreatePlayers(42, _gameMode);
        return Tournament.Run(players);
    }
}

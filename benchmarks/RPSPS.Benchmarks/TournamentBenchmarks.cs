using BenchmarkDotNet.Attributes;
using RPSPS.Engine;
using RPSPS.Models;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class TournamentBenchmarks
{
    [Benchmark]
    public TournamentResult RunSingleTournament()
    {
        var players = TournamentRunner.CreatePlayers(42);
        return Tournament.Run(players);
    }
}

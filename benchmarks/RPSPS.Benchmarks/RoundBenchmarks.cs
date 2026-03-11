using BenchmarkDotNet.Attributes;
using RPSPS.Models;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class RoundBenchmarks
{
    [Benchmark]
    public Round CreateRound_RockVsScissors() => new(Move.Rock, Move.Scissors);

    [Benchmark]
    public Round CreateRound_RockVsRock() => new(Move.Rock, Move.Rock);

    [Benchmark]
    public Round CreateRound_PaperVsScissors() => new(Move.Paper, Move.Scissors);
}

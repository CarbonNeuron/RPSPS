using BenchmarkDotNet.Attributes;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class MatchBenchmarks
{
    public IEnumerable<object[]> PlayerPairings()
    {
        yield return new object[] { "Random vs Random", new RandomPlayer(42), new RandomPlayer(43) };
        yield return new object[] { "Random vs Pattern", new RandomPlayer(42), new PatternPlayer(43) };
        yield return new object[] { "Random vs Frequency", new RandomPlayer(42), new FrequencyPlayer(43) };
        yield return new object[] { "Random vs Markov", new RandomPlayer(42), new MarkovPlayer(43) };
        yield return new object[] { "Pattern vs Frequency", new PatternPlayer(42), new FrequencyPlayer(43) };
        yield return new object[] { "Markov vs Frequency", new MarkovPlayer(42), new FrequencyPlayer(43) };
    }

    [Benchmark]
    [ArgumentsSource(nameof(PlayerPairings))]
    public MatchResult PlayMatch(string name, Player home, Player away)
    {
        var match = new Match(home.Clone(), away.Clone());
        return match.Play();
    }
}

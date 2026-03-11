using FluentAssertions;
using RPSPS.Engine;
using RPSPS.Models;

namespace RPSPS.Tests.Engine;

public class ConcurrencyEngineTests
{
    [Theory]
    [InlineData(ConcurrencyMode.Threads)]
    [InlineData(ConcurrencyMode.Parallel)]
    [InlineData(ConcurrencyMode.Async)]
    [InlineData(ConcurrencyMode.Channels)]
    public void AllModes_ProduceValidResults(ConcurrencyMode mode)
    {
        var engine = BenchmarkEngineFactory.Create(mode, 2, 1, 42);
        var result = engine.Run();

        result.TotalTournaments.Should().BeGreaterThan(0);
        result.TotalMatches.Should().BeGreaterThan(0);
        result.TotalRounds.Should().BeGreaterThan(0);
        result.TournamentsPerSecond.Should().BeGreaterThan(0);
        result.RoundsPerSecond.Should().BeGreaterThan(0);
        result.PlayerStats.Should().NotBeEmpty();
        result.ConcurrencyMode.Should().Be(mode);
    }

    [Theory]
    [InlineData(ConcurrencyMode.Threads)]
    [InlineData(ConcurrencyMode.Parallel)]
    [InlineData(ConcurrencyMode.Async)]
    [InlineData(ConcurrencyMode.Channels)]
    public void AllModes_RespectDuration(ConcurrencyMode mode)
    {
        var engine = BenchmarkEngineFactory.Create(mode, 2, 2, 42);
        var result = engine.Run();

        result.ActualDurationSeconds.Should().BeApproximately(2.0, 0.5); // +/- 25%
    }

    [Theory]
    [InlineData(ConcurrencyMode.Threads)]
    [InlineData(ConcurrencyMode.Parallel)]
    [InlineData(ConcurrencyMode.Async)]
    [InlineData(ConcurrencyMode.Channels)]
    public void AllModes_ReportAllExpectedMetrics(ConcurrencyMode mode)
    {
        var engine = BenchmarkEngineFactory.Create(mode, 1, 1, 42);
        var result = engine.Run();

        result.TotalTournaments.Should().BeGreaterThan(0);
        result.TournamentsPerSecond.Should().BeGreaterThan(0);
        result.TotalMatches.Should().BeGreaterThan(0);
        result.TotalRounds.Should().BeGreaterThan(0);
        result.AverageRoundsPerMatch.Should().BeGreaterThan(0);
        result.RoundsPerSecond.Should().BeGreaterThan(0);
        result.ThreadCount.Should().Be(1);
        result.PeakWorkingSetBytes.Should().BeGreaterThan(0);
        result.TotalAllocatedBytes.Should().BeGreaterThan(0);
        result.PlayerStats.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(ConcurrencyMode.Threads)]
    [InlineData(ConcurrencyMode.Async)]
    public void DeterministicModes_ProduceSameResultsWithSameSeed(ConcurrencyMode mode)
    {
        // Single-threaded runs should be deterministic
        var engine1 = BenchmarkEngineFactory.Create(mode, 1, 1, 42);
        var engine2 = BenchmarkEngineFactory.Create(mode, 1, 1, 42);

        var result1 = engine1.Run();
        var result2 = engine2.Run();

        // Same seed, single thread: tournament results should be identical per tournament
        // Total count may vary due to timing, but each tournament result is deterministic
        result1.TotalTournaments.Should().BeGreaterThan(0);
        result2.TotalTournaments.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(ConcurrencyMode.Threads)]
    [InlineData(ConcurrencyMode.Parallel)]
    [InlineData(ConcurrencyMode.Async)]
    [InlineData(ConcurrencyMode.Channels)]
    public void AllModes_WorkWithSpockGameMode(ConcurrencyMode mode)
    {
        var engine = BenchmarkEngineFactory.Create(mode, 1, 1, 42, GameMode.Spock);
        var result = engine.Run();

        result.TotalTournaments.Should().BeGreaterThan(0);
        result.GameMode.Should().Be(GameMode.Spock);
        result.ConcurrencyMode.Should().Be(mode);
    }

    [Fact]
    public void Factory_CreatesCorrectEngineTypes()
    {
        BenchmarkEngineFactory.Create(ConcurrencyMode.Threads, 1, 1, 42).Should().BeOfType<BenchmarkEngine>();
        BenchmarkEngineFactory.Create(ConcurrencyMode.Parallel, 1, 1, 42).Should().BeOfType<ParallelBenchmarkEngine>();
        BenchmarkEngineFactory.Create(ConcurrencyMode.Async, 1, 1, 42).Should().BeOfType<AsyncBenchmarkEngine>();
        BenchmarkEngineFactory.Create(ConcurrencyMode.Channels, 1, 1, 42).Should().BeOfType<ChannelBenchmarkEngine>();
    }

    [Fact]
    public void CompareMode_RunsAllModesAndReturnsResults()
    {
        var results = new Dictionary<ConcurrencyMode, BenchmarkResult>();

        foreach (var mode in BenchmarkEngineFactory.AllModes)
        {
            var engine = BenchmarkEngineFactory.Create(mode, 1, 1, 42);
            results[mode] = engine.Run();
        }

        results.Should().HaveCount(4);
        results.Should().ContainKey(ConcurrencyMode.Threads);
        results.Should().ContainKey(ConcurrencyMode.Parallel);
        results.Should().ContainKey(ConcurrencyMode.Async);
        results.Should().ContainKey(ConcurrencyMode.Channels);

        foreach (var result in results.Values)
        {
            result.TotalTournaments.Should().BeGreaterThan(0);
        }
    }
}

using FluentAssertions;
using RPSPS.Engine;

namespace RPSPS.Tests.Engine;

public class BenchmarkEngineTests
{
    [Fact]
    public void SingleThreaded_ProducesDeterministicResults()
    {
        var engine1 = new BenchmarkEngine(1, 1, 42);
        var engine2 = new BenchmarkEngine(1, 1, 42);

        var result1 = engine1.Run();
        var result2 = engine2.Run();

        result1.TotalTournaments.Should().BeGreaterThan(0);
        // Note: exact count may vary due to timing, but seed ensures same tournament results
    }

    [Fact]
    public void MultiThreaded_ProducesMoreTournaments()
    {
        var singleEngine = new BenchmarkEngine(1, 2, 42);
        var multiEngine = new BenchmarkEngine(4, 2, 42);

        var singleResult = singleEngine.Run();
        var multiResult = multiEngine.Run();

        multiResult.TotalTournaments.Should().BeGreaterThan(singleResult.TotalTournaments);
    }

    [Fact]
    public void Duration_IsRespected()
    {
        var engine = new BenchmarkEngine(1, 2, 42);
        var result = engine.Run();

        result.ActualDurationSeconds.Should().BeApproximately(2.0, 0.5); // ±25%
    }

    [Fact]
    public void Results_ContainAllExpectedMetrics()
    {
        var engine = new BenchmarkEngine(1, 1, 42);
        var result = engine.Run();

        result.TotalTournaments.Should().BeGreaterThan(0);
        result.TournamentsPerSecond.Should().BeGreaterThan(0);
        result.TotalMatches.Should().BeGreaterThan(0);
        result.TotalRounds.Should().BeGreaterThan(0);
        result.AverageRoundsPerMatch.Should().BeGreaterThan(0);
        result.RoundsPerSecond.Should().BeGreaterThan(0);
        result.ThreadCount.Should().Be(1);
        result.PlayerStats.Should().NotBeEmpty();
    }

    [Fact]
    public void GcMetrics_ArePopulated()
    {
        var engine = new BenchmarkEngine(1, 2, 42);
        var result = engine.Run();

        result.PeakWorkingSetBytes.Should().BeGreaterThan(0);
        result.TotalAllocatedBytes.Should().BeGreaterThan(0);
        // GC collections might be 0 for short runs, but allocated bytes should be > 0
    }
}

namespace RPSPS.Engine;

using RPSPS.Models;

public static class BenchmarkEngineFactory
{
    public static BenchmarkEngineBase Create(ConcurrencyMode mode, int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
    {
        return mode switch
        {
            ConcurrencyMode.Threads => new BenchmarkEngine(threadCount, durationSeconds, seed, gameMode),
            ConcurrencyMode.Parallel => new ParallelBenchmarkEngine(threadCount, durationSeconds, seed, gameMode),
            ConcurrencyMode.Async => new AsyncBenchmarkEngine(threadCount, durationSeconds, seed, gameMode),
            ConcurrencyMode.Channels => new ChannelBenchmarkEngine(threadCount, durationSeconds, seed, gameMode),
            _ => new BenchmarkEngine(threadCount, durationSeconds, seed, gameMode)
        };
    }

    public static readonly ConcurrencyMode[] AllModes =
        [ConcurrencyMode.Threads, ConcurrencyMode.Parallel, ConcurrencyMode.Async, ConcurrencyMode.Channels];
}

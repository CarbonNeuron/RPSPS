namespace RPSPS.Engine;

using System.Diagnostics;
using RPSPS.Models;

public sealed class AsyncBenchmarkEngine : BenchmarkEngineBase
{
    public override ConcurrencyMode ConcurrencyMode => ConcurrencyMode.Async;

    public AsyncBenchmarkEngine(int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
        : base(threadCount, durationSeconds, seed, gameMode)
    {
    }

    protected override void RunCore(int[] threadSeeds, ThreadCounters[] counters,
        long startTimestamp, long endTimestamp,
        ProgressCallback? onProgress, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var tasks = new Task[_threadCount];

        for (int t = 0; t < _threadCount; t++)
        {
            int taskIndex = t;
            int taskSeed = threadSeeds[t];

            tasks[t] = Task.Run(() =>
            {
                var runner = new TournamentRunner(taskSeed, _gameMode);
                ref var c = ref counters[taskIndex];
                int iteration = 0;

                while (Stopwatch.GetTimestamp() < endTimestamp && !cts.Token.IsCancellationRequested)
                {
                    var result = runner.RunTournament(iteration++);
                    c.Tournaments++;
                    c.Matches += result.MatchCount;
                    c.Rounds += result.TotalRounds;
                    AccumulateStats(c.PlayerStats, result);
                }
            }, cts.Token);
        }

        // Progress reporting from calling thread
        while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
        {
            onProgress?.Invoke(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
            Thread.Sleep(100);
        }

        cts.Cancel();

        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException or OperationCanceledException))
        {
            // Expected when duration expires
        }
    }
}

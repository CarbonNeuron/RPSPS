namespace RPSPS.Engine;

using System.Diagnostics;
using RPSPS.Models;

public sealed class ParallelBenchmarkEngine : BenchmarkEngineBase
{
    public override ConcurrencyMode ConcurrencyMode => ConcurrencyMode.Parallel;

    public ParallelBenchmarkEngine(int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
        : base(threadCount, durationSeconds, seed, gameMode)
    {
    }

    protected override void RunCore(int[] threadSeeds, ThreadCounters[] counters,
        long startTimestamp, long endTimestamp,
        ProgressCallback? onProgress, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start a timer to cancel after duration
        var timerThread = new Thread(() =>
        {
            while (Stopwatch.GetTimestamp() < endTimestamp && !cts.Token.IsCancellationRequested)
            {
                onProgress?.Invoke(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
                Thread.Sleep(100);
            }
            cts.Cancel();
        })
        { IsBackground = true, Name = "RPSPS-Timer" };
        timerThread.Start();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = _threadCount,
            CancellationToken = cts.Token
        };

        try
        {
            // Run an effectively infinite number of iterations, relying on cancellation to stop
            Parallel.For(0, int.MaxValue, options, (i, state) =>
            {
                if (cts.Token.IsCancellationRequested)
                {
                    state.Stop();
                    return;
                }

                int threadIndex = Thread.CurrentThread.ManagedThreadId % _threadCount;
                // Use deterministic seed based on iteration
                var runner = new TournamentRunner(threadSeeds[threadIndex % threadSeeds.Length] + i, _gameMode);
                var result = runner.RunTournament(i);

                ref var c = ref counters[threadIndex];
                c.Tournaments++;
                c.Matches += result.MatchCount;
                c.Rounds += result.TotalRounds;
                AccumulateStats(c.PlayerStats, result);
            });
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Duration expired — this is expected
        }

        timerThread.Join();
    }
}

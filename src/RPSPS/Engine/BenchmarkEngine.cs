namespace RPSPS.Engine;

using System.Diagnostics;
using RPSPS.Models;

public sealed class BenchmarkEngine : BenchmarkEngineBase
{
    public override ConcurrencyMode ConcurrencyMode => ConcurrencyMode.Threads;

    public BenchmarkEngine(int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
        : base(threadCount, durationSeconds, seed, gameMode)
    {
    }

    protected override void RunCore(int[] threadSeeds, ThreadCounters[] counters,
        long startTimestamp, long endTimestamp,
        ProgressCallback? onProgress, CancellationToken cancellationToken)
    {
        if (_threadCount == 1)
        {
            var runner = new TournamentRunner(threadSeeds[0], _gameMode);
            ref var c = ref counters[0];
            int iteration = 0;

            while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
            {
                var result = runner.RunTournament(iteration++);
                c.Tournaments++;
                c.Matches += result.MatchCount;
                c.Rounds += result.TotalRounds;
                AccumulateStats(c.PlayerStats, result);

                if (onProgress != null && (iteration & 0xFF) == 0)
                    onProgress(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
            }
        }
        else
        {
            var threads = new Thread[_threadCount];

            for (int t = 0; t < _threadCount; t++)
            {
                int threadIndex = t;
                int threadSeed = threadSeeds[t];

                threads[t] = new Thread(() =>
                {
                    var runner = new TournamentRunner(threadSeed, _gameMode);
                    ref var c = ref counters[threadIndex];
                    int iteration = 0;

                    while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
                    {
                        var result = runner.RunTournament(iteration++);
                        c.Tournaments++;
                        c.Matches += result.MatchCount;
                        c.Rounds += result.TotalRounds;
                        AccumulateStats(c.PlayerStats, result);
                    }
                })
                {
                    IsBackground = true,
                    Name = $"RPSPS-Worker-{t}"
                };
            }

            foreach (var thread in threads)
                thread.Start();

            while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
            {
                onProgress?.Invoke(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
                Thread.Sleep(100);
            }

            foreach (var thread in threads)
                thread.Join();
        }
    }
}

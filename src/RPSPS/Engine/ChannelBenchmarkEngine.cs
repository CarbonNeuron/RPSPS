namespace RPSPS.Engine;

using System.Diagnostics;
using System.Threading.Channels;
using RPSPS.Models;

public sealed class ChannelBenchmarkEngine : BenchmarkEngineBase
{
    public override ConcurrencyMode ConcurrencyMode => ConcurrencyMode.Channels;

    public ChannelBenchmarkEngine(int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
        : base(threadCount, durationSeconds, seed, gameMode)
    {
    }

    protected override void RunCore(int[] threadSeeds, ThreadCounters[] counters,
        long startTimestamp, long endTimestamp,
        ProgressCallback? onProgress, CancellationToken cancellationToken)
    {
        // Send full TournamentResult objects through the channel — real object graph pressure
        var channel = Channel.CreateBounded<TournamentResult>(new BoundedChannelOptions(1024)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Producer tasks
        var producers = new Task[_threadCount];
        for (int t = 0; t < _threadCount; t++)
        {
            int taskIndex = t;
            int taskSeed = threadSeeds[t];

            producers[t] = Task.Run(async () =>
            {
                var runner = new TournamentRunner(taskSeed, _gameMode);
                int iteration = 0;

                while (Stopwatch.GetTimestamp() < endTimestamp && !cts.Token.IsCancellationRequested)
                {
                    var result = runner.RunTournament(iteration++);

                    try
                    {
                        await channel.Writer.WriteAsync(result, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cts.Token);
        }

        // Consumer task — aggregates into counters[0]
        var consumer = Task.Run(async () =>
        {
            var playerStats = counters[0].PlayerStats;
            var reader = channel.Reader;

            try
            {
                await foreach (var result in reader.ReadAllAsync(cts.Token))
                {
                    counters[0].Tournaments++;
                    counters[0].Matches += result.MatchCount;
                    counters[0].Rounds += result.TotalRounds;

                    var standings = result.Standings;
                    for (int i = 0; i < standings.Length; i++)
                    {
                        playerStats[i].Wins += standings[i].Wins;
                        playerStats[i].Losses += standings[i].Losses;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }, cts.Token);

        // Progress reporting from calling thread
        while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
        {
            onProgress?.Invoke(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
            Thread.Sleep(100);
        }

        cts.Cancel();
        channel.Writer.TryComplete();

        try
        {
            Task.WaitAll([.. producers, consumer]);
        }
        catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is TaskCanceledException or OperationCanceledException))
        {
            // Expected when duration expires
        }
    }
}

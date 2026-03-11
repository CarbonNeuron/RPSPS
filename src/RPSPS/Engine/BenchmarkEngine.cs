namespace RPSPS.Engine;

using System.Diagnostics;
using RPSPS.Models;

public sealed class BenchmarkEngine
{
    private readonly int _threadCount;
    private readonly double _durationSeconds;
    private readonly int _seed;

    public BenchmarkEngine(int threadCount, double durationSeconds, int seed)
    {
        _threadCount = threadCount;
        _durationSeconds = durationSeconds;
        _seed = seed;
    }

    public delegate void ProgressCallback(double elapsedSeconds, double totalSeconds);

    // Per-thread counters — padded to avoid false sharing between cache lines
    private struct ThreadCounters
    {
        public long Tournaments;
        public long Matches;
        public long Rounds;
        public PlayerStats[] PlayerStats; // indexed by player index (0-3)
    }

    public BenchmarkResult Run(ProgressCallback? onProgress = null, CancellationToken cancellationToken = default)
    {
        int gen0Before = GC.CollectionCount(0);
        int gen1Before = GC.CollectionCount(1);
        int gen2Before = GC.CollectionCount(2);
        long allocBefore = GC.GetTotalAllocatedBytes(precise: false);

        // Derive per-thread seeds deterministically from the master seed
        var seedRng = new Random(_seed);
        var threadSeeds = new int[_threadCount];
        for (int i = 0; i < _threadCount; i++)
            threadSeeds[i] = seedRng.Next();

        var counters = new ThreadCounters[_threadCount];
        for (int i = 0; i < _threadCount; i++)
            counters[i].PlayerStats = [new(), new(), new(), new()];

        long startTimestamp = Stopwatch.GetTimestamp();
        long endTimestamp = startTimestamp + (long)(_durationSeconds * Stopwatch.Frequency);

        if (_threadCount == 1)
        {
            var runner = new TournamentRunner(threadSeeds[0]);
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
                    var runner = new TournamentRunner(threadSeed);
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

            // Progress reporting from main thread
            while (Stopwatch.GetTimestamp() < endTimestamp && !cancellationToken.IsCancellationRequested)
            {
                onProgress?.Invoke(Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds, _durationSeconds);
                Thread.Sleep(100);
            }

            foreach (var thread in threads)
                thread.Join();
        }

        cancellationToken.ThrowIfCancellationRequested();

        double actualDuration = Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds;
        long allocAfter = GC.GetTotalAllocatedBytes(precise: false);
        var process = Process.GetCurrentProcess();

        // Aggregate counters across threads
        long totalTournaments = 0, totalMatches = 0, totalRounds = 0;
        var globalPlayerStats = new Dictionary<string, PlayerStats>();

        // Get player names from a temp runner
        var playerNames = GetPlayerNames();

        for (int t = 0; t < _threadCount; t++)
        {
            ref var c = ref counters[t];
            totalTournaments += c.Tournaments;
            totalMatches += c.Matches;
            totalRounds += c.Rounds;

            for (int p = 0; p < c.PlayerStats.Length; p++)
            {
                var name = playerNames[p];
                if (!globalPlayerStats.TryGetValue(name, out var gps))
                {
                    gps = new PlayerStats();
                    globalPlayerStats[name] = gps;
                }
                gps.Wins += c.PlayerStats[p].Wins;
                gps.Losses += c.PlayerStats[p].Losses;
            }
        }

        return new BenchmarkResult
        {
            TotalTournaments = totalTournaments,
            TournamentsPerSecond = totalTournaments / actualDuration,
            TotalMatches = totalMatches,
            TotalRounds = totalRounds,
            AverageRoundsPerMatch = totalMatches > 0 ? (double)totalRounds / totalMatches : 0,
            RoundsPerSecond = totalRounds / actualDuration,
            ActualDurationSeconds = actualDuration,
            ThreadCount = _threadCount,
            Seed = _seed,
            PeakWorkingSetBytes = process.PeakWorkingSet64,
            TotalAllocatedBytes = allocAfter - allocBefore,
            AllocationRateBytesPerSecond = (allocAfter - allocBefore) / actualDuration,
            GcGen0Collections = GC.CollectionCount(0) - gen0Before,
            GcGen1Collections = GC.CollectionCount(1) - gen1Before,
            GcGen2Collections = GC.CollectionCount(2) - gen2Before,
            PlayerStats = globalPlayerStats
        };
    }

    private static void AccumulateStats(PlayerStats[] stats, TournamentResult result)
    {
        var standings = result.Standings;
        for (int i = 0; i < standings.Length; i++)
        {
            stats[i].Wins += standings[i].Wins;
            stats[i].Losses += standings[i].Losses;
        }
    }

    private static string[] GetPlayerNames() =>
        ["RandomPlayer", "PatternPlayer", "FrequencyPlayer", "MarkovPlayer"];
}

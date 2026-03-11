namespace RPSPS.Engine;

using System.Diagnostics;
using RPSPS.Models;

public abstract class BenchmarkEngineBase
{
    protected readonly int _threadCount;
    protected readonly double _durationSeconds;
    protected readonly int _seed;
    protected readonly GameMode _gameMode;

    public delegate void ProgressCallback(double elapsedSeconds, double totalSeconds);

    protected struct ThreadCounters
    {
        public long Tournaments;
        public long Matches;
        public long Rounds;
        public PlayerStats[] PlayerStats;
    }

    protected BenchmarkEngineBase(int threadCount, double durationSeconds, int seed, GameMode gameMode = GameMode.Classic)
    {
        _threadCount = threadCount;
        _durationSeconds = durationSeconds;
        _seed = seed;
        _gameMode = gameMode;
    }

    public abstract ConcurrencyMode ConcurrencyMode { get; }

    public BenchmarkResult Run(ProgressCallback? onProgress = null, CancellationToken cancellationToken = default)
    {
        int gen0Before = GC.CollectionCount(0);
        int gen1Before = GC.CollectionCount(1);
        int gen2Before = GC.CollectionCount(2);
        long allocBefore = GC.GetTotalAllocatedBytes(precise: false);

        var seedRng = new Random(_seed);
        var threadSeeds = new int[_threadCount];
        for (int i = 0; i < _threadCount; i++)
            threadSeeds[i] = seedRng.Next();

        var counters = new ThreadCounters[_threadCount];
        for (int i = 0; i < _threadCount; i++)
            counters[i].PlayerStats = [new(), new(), new(), new()];

        long startTimestamp = Stopwatch.GetTimestamp();
        long endTimestamp = startTimestamp + (long)(_durationSeconds * Stopwatch.Frequency);

        RunCore(threadSeeds, counters, startTimestamp, endTimestamp, onProgress, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        double actualDuration = Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds;
        long allocAfter = GC.GetTotalAllocatedBytes(precise: false);
        var process = Process.GetCurrentProcess();

        long totalTournaments = 0, totalMatches = 0, totalRounds = 0;
        var globalPlayerStats = new Dictionary<string, PlayerStats>();

        var playerNames = TournamentRunner.CreatePlayers(_seed, _gameMode)
            .Select(p => p.Name).ToArray();

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
            GameMode = _gameMode,
            ConcurrencyMode = ConcurrencyMode,
            PeakWorkingSetBytes = process.PeakWorkingSet64,
            TotalAllocatedBytes = allocAfter - allocBefore,
            AllocationRateBytesPerSecond = (allocAfter - allocBefore) / actualDuration,
            GcGen0Collections = GC.CollectionCount(0) - gen0Before,
            GcGen1Collections = GC.CollectionCount(1) - gen1Before,
            GcGen2Collections = GC.CollectionCount(2) - gen2Before,
            PlayerStats = globalPlayerStats
        };
    }

    protected abstract void RunCore(int[] threadSeeds, ThreadCounters[] counters,
        long startTimestamp, long endTimestamp,
        ProgressCallback? onProgress, CancellationToken cancellationToken);

    protected static void AccumulateStats(PlayerStats[] stats, TournamentResult result)
    {
        var standings = result.Standings;
        for (int i = 0; i < standings.Length; i++)
        {
            stats[i].Wins += standings[i].Wins;
            stats[i].Losses += standings[i].Losses;
        }
    }
}

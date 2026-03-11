namespace RPSPS.Engine;

using System.Text.Json.Serialization;
using RPSPS.Models;

public sealed class BenchmarkResult
{
    public long TotalTournaments { get; set; }
    public double TournamentsPerSecond { get; set; }
    public long TotalMatches { get; set; }
    public long TotalRounds { get; set; }
    public double AverageRoundsPerMatch { get; set; }
    public double RoundsPerSecond { get; set; }
    public double ActualDurationSeconds { get; set; }
    public int ThreadCount { get; set; }
    public int? Seed { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<GameMode>))]
    public GameMode GameMode { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<ConcurrencyMode>))]
    public ConcurrencyMode ConcurrencyMode { get; set; }

    // Memory metrics
    public long PeakWorkingSetBytes { get; set; }
    public long TotalAllocatedBytes { get; set; }
    public double AllocationRateBytesPerSecond { get; set; }
    public int GcGen0Collections { get; set; }
    public int GcGen1Collections { get; set; }
    public int GcGen2Collections { get; set; }

    // Player stats
    public Dictionary<string, PlayerStats> PlayerStats { get; set; } = new();
}

public sealed class PlayerStats
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) : 0;
}

public sealed class ComparisonResult
{
    public Dictionary<string, BenchmarkResult> Results { get; set; } = new();
}

[JsonSerializable(typeof(BenchmarkResult))]
[JsonSerializable(typeof(PlayerStats))]
[JsonSerializable(typeof(Dictionary<string, PlayerStats>))]
[JsonSerializable(typeof(ComparisonResult))]
[JsonSerializable(typeof(Dictionary<string, BenchmarkResult>))]
[JsonSerializable(typeof(BenchmarkResult[]))]
public partial class BenchmarkResultJsonContext : JsonSerializerContext
{
}

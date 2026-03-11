using BenchmarkDotNet.Attributes;
using RPSPS.Engine;
using RPSPS.Models;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class EngineBenchmarks
{
    [Params(1, 2, 4, 8, 16)]
    public int ThreadCount { get; set; }

    [Params("classic", "spock")]
    public string GameModeName { get; set; } = "classic";

    [Params("threads", "parallel", "async", "channels")]
    public string ConcurrencyModeName { get; set; } = "threads";

    private GameMode _gameMode;
    private ConcurrencyMode _concurrencyMode;

    [GlobalSetup]
    public void Setup()
    {
        _gameMode = GameModeName == "spock" ? GameMode.Spock : GameMode.Classic;
        _concurrencyMode = ConcurrencyModeName switch
        {
            "parallel" => ConcurrencyMode.Parallel,
            "async" => ConcurrencyMode.Async,
            "channels" => ConcurrencyMode.Channels,
            _ => ConcurrencyMode.Threads
        };
    }

    [Benchmark]
    public BenchmarkResult RunEngine()
    {
        var engine = BenchmarkEngineFactory.Create(_concurrencyMode, ThreadCount, 1, 42, _gameMode);
        return engine.Run();
    }
}

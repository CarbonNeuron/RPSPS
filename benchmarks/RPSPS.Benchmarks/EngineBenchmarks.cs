using BenchmarkDotNet.Attributes;
using RPSPS.Engine;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class EngineBenchmarks
{
    [Params(1, 2, 4, 8, 16)]
    public int ThreadCount { get; set; }

    [Benchmark]
    public BenchmarkResult RunEngine()
    {
        var engine = new BenchmarkEngine(ThreadCount, 1, 42);
        return engine.Run();
    }
}

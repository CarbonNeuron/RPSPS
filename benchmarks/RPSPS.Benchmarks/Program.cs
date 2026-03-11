using BenchmarkDotNet.Running;
using RPSPS.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(PlayerBenchmarks).Assembly).Run(args);

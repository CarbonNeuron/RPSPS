using BenchmarkDotNet.Attributes;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class PlayerBenchmarks
{
    [Params(0, 10, 100, 1000)]
    public int HistorySize { get; set; }

    private RandomPlayer _randomPlayer = null!;
    private PatternPlayer _patternPlayer = null!;
    private FrequencyPlayer _frequencyPlayer = null!;
    private MarkovPlayer _markovPlayer = null!;
    private List<Move> _history = null!;

    [GlobalSetup]
    public void Setup()
    {
        _randomPlayer = new RandomPlayer(42);
        _patternPlayer = new PatternPlayer(42);
        _frequencyPlayer = new FrequencyPlayer(42);
        _markovPlayer = new MarkovPlayer(42);

        _history = new List<Move>(HistorySize);
        var rng = new Random(42);
        for (int i = 0; i < HistorySize; i++)
        {
            var move = (Move)rng.Next(3);
            _history.Add(move);
            _randomPlayer.RecordOpponentMove(move);
            _patternPlayer.RecordOpponentMove(move);
            _frequencyPlayer.RecordOpponentMove(move);
            _markovPlayer.RecordOpponentMove(move);
        }
    }

    [Benchmark(Baseline = true)]
    public Move RandomPlayer_ChooseMove() => _randomPlayer.ChooseMove();

    [Benchmark]
    public Move PatternPlayer_ChooseMove() => _patternPlayer.ChooseMove();

    [Benchmark]
    public Move FrequencyPlayer_ChooseMove() => _frequencyPlayer.ChooseMove();

    [Benchmark]
    public Move MarkovPlayer_ChooseMove() => _markovPlayer.ChooseMove();
}

using BenchmarkDotNet.Attributes;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Benchmarks;

[MemoryDiagnoser]
public class PlayerBenchmarks
{
    [Params(0, 10, 100, 1000)]
    public int HistorySize { get; set; }

    [Params("classic", "spock")]
    public string GameModeName { get; set; } = "classic";

    private RandomPlayer _randomPlayer = null!;
    private PatternPlayer _patternPlayer = null!;
    private FrequencyPlayer _frequencyPlayer = null!;
    private MarkovPlayer _markovPlayer = null!;

    [GlobalSetup]
    public void Setup()
    {
        int moveCount = GameModeName == "spock" ? 5 : 3;

        _randomPlayer = new RandomPlayer(42, moveCount);
        _patternPlayer = new PatternPlayer(42, moveCount);
        _frequencyPlayer = new FrequencyPlayer(42, moveCount);
        _markovPlayer = new MarkovPlayer(42, moveCount);

        var rng = new Random(42);
        for (int i = 0; i < HistorySize; i++)
        {
            var move = (Move)rng.Next(moveCount);
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

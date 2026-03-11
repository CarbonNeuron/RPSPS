namespace RPSPS.Players;

using RPSPS.Models;

public sealed class RandomPlayer : Player
{
    private readonly Random _rng;
    private readonly int _moveCount;

    public override string Name => "RandomPlayer";

    public RandomPlayer(int seed, int moveCount = 3)
    {
        _rng = new Random(seed);
        _moveCount = moveCount;
    }

    public override Move ChooseMove()
    {
        return (Move)_rng.Next(_moveCount);
    }

    public override Player Clone() => new RandomPlayer(_rng.Next(), _moveCount);
}

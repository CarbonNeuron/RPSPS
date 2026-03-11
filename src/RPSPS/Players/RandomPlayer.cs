namespace RPSPS.Players;

using RPSPS.Models;

public sealed class RandomPlayer : Player
{
    private readonly Random _rng;

    public override string Name => "RandomPlayer";

    public RandomPlayer(int seed)
    {
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        return (Move)_rng.Next(3);
    }
}

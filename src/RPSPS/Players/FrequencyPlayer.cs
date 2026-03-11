namespace RPSPS.Players;

using RPSPS.Models;

public sealed class FrequencyPlayer : Player
{
    // Flat array instead of Dictionary<Move, int>
    private readonly int[] _frequency = new int[3];
    private readonly Random _rng;

    public override string Name => "FrequencyPlayer";

    public FrequencyPlayer(int seed)
    {
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        if (OpponentHistoryCount == 0)
            return (Move)_rng.Next(3);

        // Find the most frequent move
        int bestIdx = 0;
        int bestCount = _frequency[0];

        if (_frequency[1] > bestCount)
        {
            bestIdx = 1;
            bestCount = _frequency[1];
        }
        if (_frequency[2] > bestCount)
        {
            bestIdx = 2;
        }

        return ((Move)bestIdx).GetCounter();
    }

    protected override void OnOpponentMove(Move move)
    {
        _frequency[(int)move]++;
    }

    public override void Reset()
    {
        base.Reset();
        _frequency[0] = 0;
        _frequency[1] = 0;
        _frequency[2] = 0;
    }
}

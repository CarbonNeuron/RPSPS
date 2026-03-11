namespace RPSPS.Players;

using RPSPS.Models;

public sealed class FrequencyPlayer : Player
{
    private readonly int[] _frequency;
    private readonly Random _rng;
    private readonly int _moveCount;

    public override string Name => "FrequencyPlayer";

    public FrequencyPlayer(int seed, int moveCount = 3)
    {
        _moveCount = moveCount;
        _frequency = new int[moveCount];
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        if (OpponentHistoryCount == 0)
            return (Move)_rng.Next(_moveCount);

        // Find the most frequent move
        int bestIdx = 0;
        int bestCount = _frequency[0];

        for (int i = 1; i < _moveCount; i++)
        {
            if (_frequency[i] > bestCount)
            {
                bestIdx = i;
                bestCount = _frequency[i];
            }
        }

        return _moveCount > 3 ? ((Move)bestIdx).GetCounter(_rng) : ((Move)bestIdx).GetCounter();
    }

    protected override void OnOpponentMove(Move move)
    {
        _frequency[(int)move]++;
    }

    public override void Reset()
    {
        base.Reset();
        Array.Clear(_frequency);
    }
}

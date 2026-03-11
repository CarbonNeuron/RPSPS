namespace RPSPS.Players;

using RPSPS.Models;

public sealed class PatternPlayer : Player
{
    private readonly int _windowSize;
    private readonly Random _rng;

    public override string Name => "PatternPlayer";

    public PatternPlayer(int seed, int windowSize = 5)
    {
        _windowSize = windowSize;
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        var history = OpponentHistory;
        if (history.Length < _windowSize * 2)
            return (Move)_rng.Next(3);

        // Look for repeating pattern in the last windowSize moves
        int start = history.Length - _windowSize;
        for (int patternLen = 1; patternLen <= _windowSize / 2; patternLen++)
        {
            bool isPattern = true;

            for (int i = start; i < history.Length; i++)
            {
                if (history[i] != history[start + (i - start) % patternLen])
                {
                    isPattern = false;
                    break;
                }
            }

            if (isPattern)
            {
                int nextIdx = (history.Length - start) % patternLen;
                Move predicted = history[start + nextIdx];
                return predicted.GetCounter();
            }
        }

        return (Move)_rng.Next(3);
    }
}

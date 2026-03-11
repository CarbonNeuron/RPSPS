namespace RPSPS.Players;

using RPSPS.Models;

public sealed class MarkovPlayer : Player
{
    private readonly int[] _transitions;
    private readonly int _moveCount;
    private int _lastOpponentMove = -1;
    private readonly Random _rng;

    public override string Name => "MarkovPlayer";

    public MarkovPlayer(int seed, int moveCount = 3)
    {
        _moveCount = moveCount;
        _transitions = new int[moveCount * moveCount];
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        if (_lastOpponentMove < 0 || OpponentHistoryCount < 2)
            return (Move)_rng.Next(_moveCount);

        int baseIdx = _lastOpponentMove * _moveCount;

        // Find the most likely next move based on transitions
        int maxWeight = _transitions[baseIdx];
        int predicted = 0;

        for (int i = 1; i < _moveCount; i++)
        {
            if (_transitions[baseIdx + i] > maxWeight)
            {
                maxWeight = _transitions[baseIdx + i];
                predicted = i;
            }
        }

        if (maxWeight <= 0)
            return (Move)_rng.Next(_moveCount);

        return _moveCount > 3 ? ((Move)predicted).GetCounter(_rng) : ((Move)predicted).GetCounter();
    }

    protected override void OnOpponentMove(Move move)
    {
        if (_lastOpponentMove >= 0)
        {
            _transitions[_lastOpponentMove * _moveCount + (int)move]++;
        }
        _lastOpponentMove = (int)move;
    }

    public override Player Clone() => new MarkovPlayer(_rng.Next(), _moveCount);
}

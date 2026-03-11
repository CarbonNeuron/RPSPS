namespace RPSPS.Players;

using RPSPS.Models;

public sealed class MarkovPlayer : Player
{
    // Transition matrix: _transitions[lastMove * 3 + nextMove] = count
    // Flat array avoids 2D array bounds checking overhead
    private readonly int[] _transitions = new int[9];
    private int _lastOpponentMove = -1;
    private readonly Random _rng;

    public override string Name => "MarkovPlayer";

    public MarkovPlayer(int seed)
    {
        _rng = new Random(seed);
    }

    public override Move ChooseMove()
    {
        if (_lastOpponentMove < 0 || OpponentHistoryCount < 2)
            return (Move)_rng.Next(3);

        int baseIdx = _lastOpponentMove * 3;

        // Find the most likely next move based on transitions
        int maxWeight = _transitions[baseIdx];
        int predicted = 0;

        if (_transitions[baseIdx + 1] > maxWeight)
        {
            maxWeight = _transitions[baseIdx + 1];
            predicted = 1;
        }
        if (_transitions[baseIdx + 2] > maxWeight)
        {
            maxWeight = _transitions[baseIdx + 2];
            predicted = 2;
        }

        if (maxWeight <= 0)
            return (Move)_rng.Next(3);

        return ((Move)predicted).GetCounter();
    }

    protected override void OnOpponentMove(Move move)
    {
        if (_lastOpponentMove >= 0)
        {
            _transitions[_lastOpponentMove * 3 + (int)move]++;
        }
        _lastOpponentMove = (int)move;
    }

    public override void Reset()
    {
        base.Reset();
        Array.Clear(_transitions);
        _lastOpponentMove = -1;
    }
}

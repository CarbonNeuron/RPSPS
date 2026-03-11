namespace RPSPS.Players;

using RPSPS.Models;

public abstract class Player
{
    public abstract string Name { get; }

    // Pre-allocated history buffer — matches are short (typically <10 rounds)
    private Move[] _opponentHistory = new Move[16];
    private int _opponentHistoryCount;

    public abstract Move ChooseMove();

    public void RecordOpponentMove(Move move)
    {
        if (_opponentHistoryCount == _opponentHistory.Length)
            Array.Resize(ref _opponentHistory, _opponentHistory.Length * 2);
        _opponentHistory[_opponentHistoryCount++] = move;
        OnOpponentMove(move);
    }

    protected virtual void OnOpponentMove(Move move) { }

    protected ReadOnlySpan<Move> OpponentHistory => _opponentHistory.AsSpan(0, _opponentHistoryCount);
    protected int OpponentHistoryCount => _opponentHistoryCount;

    public virtual void Reset()
    {
        _opponentHistoryCount = 0;
    }
}

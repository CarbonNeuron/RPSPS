namespace RPSPS.Players;

using RPSPS.Models;

public abstract class Player
{
    public abstract string Name { get; }

    private readonly List<Move> _opponentHistory = new();

    public abstract Move ChooseMove();

    public abstract Player Clone();

    public void RecordOpponentMove(Move move)
    {
        _opponentHistory.Add(move);
        OnOpponentMove(move);
    }

    protected virtual void OnOpponentMove(Move move) { }

    protected IReadOnlyList<Move> OpponentHistory => _opponentHistory;
    protected int OpponentHistoryCount => _opponentHistory.Count;
}

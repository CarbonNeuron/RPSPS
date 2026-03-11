namespace RPSPS.Models;

public enum Move
{
    Rock = 0,
    Paper = 1,
    Scissors = 2
}

public static class MoveExtensions
{
    // Lookup tables — no branching
    private static readonly Move[] Counters = [Move.Paper, Move.Scissors, Move.Rock];

    // WinsTable[attacker, defender] = true if attacker wins
    private static readonly bool[,] WinsTable =
    {
        { false, false, true },  // Rock beats Scissors
        { true, false, false },  // Paper beats Rock
        { false, true, false }   // Scissors beats Paper
    };

    public static Move GetCounter(this Move move) => Counters[(int)move];

    public static bool Beats(this Move attacker, Move defender) =>
        WinsTable[(int)attacker, (int)defender];
}

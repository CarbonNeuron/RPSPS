namespace RPSPS.Models;

public enum Move
{
    Rock = 0,
    Paper = 1,
    Scissors = 2,
    Lizard = 3,
    Spock = 4
}

public static class MoveExtensions
{
    // 5x5 WinsTable — works for both Classic (3x3 subset) and Spock (full 5x5)
    // WinsTable[attacker, defender] = true if attacker wins
    //
    // Rock     beats Scissors, Lizard
    // Paper    beats Rock, Spock
    // Scissors beats Paper, Lizard
    // Lizard   beats Spock, Paper
    // Spock    beats Scissors, Rock
    private static readonly bool[,] WinsTable =
    {
        //              Rock   Paper  Scissors Lizard Spock
        /* Rock */    { false, false, true,    true,  false },
        /* Paper */   { true,  false, false,   false, true  },
        /* Scissors */{ false, true,  false,   true,  false },
        /* Lizard */  { false, true,  false,   false, true  },
        /* Spock */   { true,  false, true,    false, false }
    };

    // First counter for each move (used by GetCounter for single-counter path)
    // In Classic mode these are the only counters; in Spock mode there's a second.
    private static readonly Move[] FirstCounters =
        [Move.Paper, Move.Scissors, Move.Rock, Move.Rock, Move.Paper];

    // Second counter (only meaningful in Spock mode)
    private static readonly Move[] SecondCounters =
        [Move.Spock, Move.Lizard, Move.Spock, Move.Scissors, Move.Lizard];

    public static Move GetCounter(this Move move) => FirstCounters[(int)move];

    public static Move GetCounter(this Move move, Random rng) =>
        rng.Next(2) == 0 ? FirstCounters[(int)move] : SecondCounters[(int)move];

    public static bool Beats(this Move attacker, Move defender) =>
        WinsTable[(int)attacker, (int)defender];
}

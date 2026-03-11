using FluentAssertions;
using RPSPS.Engine;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Models;

public class SpockRoundTests
{
    // All 25 Spock mode combinations
    [Theory]
    // Rock beats Scissors, Lizard
    [InlineData(Move.Rock, Move.Scissors, RoundResult.HomeWin)]
    [InlineData(Move.Rock, Move.Lizard, RoundResult.HomeWin)]
    // Paper beats Rock, Spock
    [InlineData(Move.Paper, Move.Rock, RoundResult.HomeWin)]
    [InlineData(Move.Paper, Move.Spock, RoundResult.HomeWin)]
    // Scissors beats Paper, Lizard
    [InlineData(Move.Scissors, Move.Paper, RoundResult.HomeWin)]
    [InlineData(Move.Scissors, Move.Lizard, RoundResult.HomeWin)]
    // Lizard beats Spock, Paper
    [InlineData(Move.Lizard, Move.Spock, RoundResult.HomeWin)]
    [InlineData(Move.Lizard, Move.Paper, RoundResult.HomeWin)]
    // Spock beats Scissors, Rock
    [InlineData(Move.Spock, Move.Scissors, RoundResult.HomeWin)]
    [InlineData(Move.Spock, Move.Rock, RoundResult.HomeWin)]
    // Reverse losses
    [InlineData(Move.Scissors, Move.Rock, RoundResult.AwayWin)]
    [InlineData(Move.Lizard, Move.Rock, RoundResult.AwayWin)]
    [InlineData(Move.Rock, Move.Paper, RoundResult.AwayWin)]
    [InlineData(Move.Spock, Move.Paper, RoundResult.AwayWin)]
    [InlineData(Move.Paper, Move.Scissors, RoundResult.AwayWin)]
    [InlineData(Move.Lizard, Move.Scissors, RoundResult.AwayWin)]
    [InlineData(Move.Spock, Move.Lizard, RoundResult.AwayWin)]
    [InlineData(Move.Paper, Move.Lizard, RoundResult.AwayWin)]
    [InlineData(Move.Scissors, Move.Spock, RoundResult.AwayWin)]
    [InlineData(Move.Rock, Move.Spock, RoundResult.AwayWin)]
    // Draws
    [InlineData(Move.Rock, Move.Rock, RoundResult.Draw)]
    [InlineData(Move.Paper, Move.Paper, RoundResult.Draw)]
    [InlineData(Move.Scissors, Move.Scissors, RoundResult.Draw)]
    [InlineData(Move.Lizard, Move.Lizard, RoundResult.Draw)]
    [InlineData(Move.Spock, Move.Spock, RoundResult.Draw)]
    public void SpockMode_ResolvesAll25CombinationsCorrectly(Move home, Move away, RoundResult expected)
    {
        var round = new Round(home, away);
        round.Result.Should().Be(expected);
    }

    [Fact]
    public void EachMove_BeatsExactly2Others()
    {
        var allMoves = new[] { Move.Rock, Move.Paper, Move.Scissors, Move.Lizard, Move.Spock };

        foreach (var attacker in allMoves)
        {
            int wins = allMoves.Count(defender => attacker.Beats(defender));
            wins.Should().Be(2, because: $"{attacker} should beat exactly 2 other moves");
        }
    }

    [Fact]
    public void EachMove_LosesToExactly2Others()
    {
        var allMoves = new[] { Move.Rock, Move.Paper, Move.Scissors, Move.Lizard, Move.Spock };

        foreach (var defender in allMoves)
        {
            int losses = allMoves.Count(attacker => attacker.Beats(defender));
            losses.Should().Be(2, because: $"{defender} should lose to exactly 2 other moves");
        }
    }

    [Fact]
    public void SpockMode_PlayerStrategiesWorkWith5Moves()
    {
        var players = TournamentRunner.CreatePlayers(42, GameMode.Spock);
        var result = Tournament.Run(players);

        result.MatchCount.Should().Be(6); // C(4,2) = 6
        result.TotalRounds.Should().BeGreaterThan(0);
        result.Standings.Should().HaveCount(4);
    }

    [Fact]
    public void SpockMode_MarkovPlayerBuilds5x5Matrix()
    {
        var player = new MarkovPlayer(42, moveCount: 5);

        // Feed moves including Lizard and Spock
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Lizard);
        player.RecordOpponentMove(Move.Spock);
        player.RecordOpponentMove(Move.Paper);
        player.RecordOpponentMove(Move.Scissors);
        player.RecordOpponentMove(Move.Lizard);

        var move = player.ChooseMove();
        // Should be a valid Spock-mode move
        ((int)move).Should().BeInRange(0, 4);
    }

    [Fact]
    public void SpockMode_FrequencyPlayerCountersMostCommon()
    {
        var player = new FrequencyPlayer(42, moveCount: 5);

        // Opponent plays mostly Lizard
        for (int i = 0; i < 10; i++)
            player.RecordOpponentMove(Move.Lizard);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);

        var move = player.ChooseMove();
        // Most frequent is Lizard. Counters to Lizard are Rock and Scissors.
        move.Should().BeOneOf(Move.Rock, Move.Scissors);
    }

    [Fact]
    public void SpockMode_RandomPlayerGeneratesAll5Moves()
    {
        var player = new RandomPlayer(42, moveCount: 5);
        var seen = new HashSet<Move>();

        for (int i = 0; i < 1000; i++)
            seen.Add(player.ChooseMove());

        seen.Should().Contain(Move.Rock);
        seen.Should().Contain(Move.Paper);
        seen.Should().Contain(Move.Scissors);
        seen.Should().Contain(Move.Lizard);
        seen.Should().Contain(Move.Spock);
    }

    [Fact]
    public void GetCounter_ReturnsMoveThatBeatsTarget()
    {
        var allMoves = new[] { Move.Rock, Move.Paper, Move.Scissors, Move.Lizard, Move.Spock };

        foreach (var target in allMoves)
        {
            var counter = target.GetCounter();
            counter.Beats(target).Should().BeTrue(
                because: $"GetCounter({target}) = {counter} should beat {target}");
        }
    }

    [Fact]
    public void GetCounterWithRng_ReturnsMoveThatBeatsTarget()
    {
        var rng = new Random(42);
        var allMoves = new[] { Move.Rock, Move.Paper, Move.Scissors, Move.Lizard, Move.Spock };

        for (int trial = 0; trial < 100; trial++)
        {
            foreach (var target in allMoves)
            {
                var counter = target.GetCounter(rng);
                counter.Beats(target).Should().BeTrue(
                    because: $"GetCounter({target}, rng) = {counter} should beat {target}");
            }
        }
    }
}

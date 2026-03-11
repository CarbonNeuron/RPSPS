using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Players;

public class RandomPlayerTests
{
    [Fact]
    public void ChooseMove_ReturnsValidMove()
    {
        var player = new RandomPlayer(42);
        var move = player.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }

    [Fact]
    public void ChooseMove_WithSameSeed_IsDeterministic()
    {
        var player1 = new RandomPlayer(42);
        var player2 = new RandomPlayer(42);

        for (int i = 0; i < 100; i++)
        {
            player1.ChooseMove().Should().Be(player2.ChooseMove());
        }
    }

    [Fact]
    public void ChooseMove_IsRoughlyUniform()
    {
        var player = new RandomPlayer(42);
        var counts = new Dictionary<Move, int>
        {
            { Move.Rock, 0 }, { Move.Paper, 0 }, { Move.Scissors, 0 }
        };

        for (int i = 0; i < 10000; i++)
        {
            counts[player.ChooseMove()]++;
        }

        // Each should be roughly 33% (allow 25-41%)
        foreach (var count in counts.Values)
        {
            count.Should().BeGreaterThan(2500);
            count.Should().BeLessThan(4100);
        }
    }
}

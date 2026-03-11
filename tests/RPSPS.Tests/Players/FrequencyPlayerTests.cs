using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Players;

public class FrequencyPlayerTests
{
    [Fact]
    public void ChooseMove_ReturnsValidMove()
    {
        var player = new FrequencyPlayer(42);
        var move = player.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }

    [Fact]
    public void ChooseMove_CountersMostFrequentMove()
    {
        var player = new FrequencyPlayer(42);
        // Opponent plays mostly Rock
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);
        player.RecordOpponentMove(Move.Scissors);

        var move = player.ChooseMove();
        // Most frequent is Rock, counter is Paper
        move.Should().Be(Move.Paper);
    }

    [Fact]
    public void Clone_ProducesFreshPlayer()
    {
        var player = new FrequencyPlayer(42);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Rock);

        var clone = (FrequencyPlayer)player.Clone();

        // Clone has no history — falls back to random
        clone.ChooseMove().Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }
}

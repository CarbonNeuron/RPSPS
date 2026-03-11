using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Players;

public class MarkovPlayerTests
{
    [Fact]
    public void ChooseMove_ReturnsValidMove()
    {
        var player = new MarkovPlayer(42);
        var move = player.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }

    [Fact]
    public void ChooseMove_PredictionsConvergeWithData()
    {
        var player = new MarkovPlayer(42);
        // Feed a pattern: Rock always followed by Scissors
        for (int i = 0; i < 20; i++)
        {
            player.RecordOpponentMove(Move.Rock);
            player.RecordOpponentMove(Move.Scissors);
        }

        // After Rock, opponent should play Scissors
        // Record one more Rock so last move is Rock
        player.RecordOpponentMove(Move.Rock);

        var move = player.ChooseMove();
        // Predicted: Scissors (follows Rock), Counter: Rock
        move.Should().Be(Move.Rock);
    }

    [Fact]
    public void Clone_ProducesFreshPlayer()
    {
        var player = new MarkovPlayer(42);
        for (int i = 0; i < 10; i++)
            player.RecordOpponentMove(Move.Rock);

        var clone = (MarkovPlayer)player.Clone();

        // Clone has no history — falls back to random
        var move = clone.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }

    [Fact]
    public void Players_MaintainIndependentState()
    {
        var player1 = new MarkovPlayer(42);
        var player2 = new MarkovPlayer(42);

        player1.RecordOpponentMove(Move.Rock);
        player1.RecordOpponentMove(Move.Rock);

        // player2 has no history, its ChooseMove should be independent of player1
        var move2 = player2.ChooseMove();
        move2.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }
}

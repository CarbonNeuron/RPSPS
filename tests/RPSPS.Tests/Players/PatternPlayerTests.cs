using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Players;

public class PatternPlayerTests
{
    [Fact]
    public void ChooseMove_ReturnsValidMove()
    {
        var player = new PatternPlayer(42);
        var move = player.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }

    [Fact]
    public void ChooseMove_DetectsSimplePattern()
    {
        var player = new PatternPlayer(42, windowSize: 4);
        // Feed a repeating pattern: Rock, Paper, Rock, Paper, Rock, Paper, Rock, Paper
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);

        var move = player.ChooseMove();
        // Pattern is R,P repeating. Next predicted: R (index 8 % 2 = 0 -> Rock)
        // Counter to Rock is Paper
        move.Should().Be(Move.Paper);
    }

    [Fact]
    public void ChooseMove_FallsBackToRandomWithNoPattern()
    {
        var player = new PatternPlayer(42);
        player.RecordOpponentMove(Move.Rock);
        player.RecordOpponentMove(Move.Paper);
        var move = player.ChooseMove();
        move.Should().BeOneOf(Move.Rock, Move.Paper, Move.Scissors);
    }
}

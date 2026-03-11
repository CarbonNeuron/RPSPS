using FluentAssertions;
using RPSPS.Models;

namespace RPSPS.Tests.Models;

public class RoundTests
{
    [Theory]
    [InlineData(Move.Rock, Move.Scissors, RoundResult.HomeWin)]
    [InlineData(Move.Scissors, Move.Paper, RoundResult.HomeWin)]
    [InlineData(Move.Paper, Move.Rock, RoundResult.HomeWin)]
    [InlineData(Move.Scissors, Move.Rock, RoundResult.AwayWin)]
    [InlineData(Move.Paper, Move.Scissors, RoundResult.AwayWin)]
    [InlineData(Move.Rock, Move.Paper, RoundResult.AwayWin)]
    [InlineData(Move.Rock, Move.Rock, RoundResult.Draw)]
    [InlineData(Move.Paper, Move.Paper, RoundResult.Draw)]
    [InlineData(Move.Scissors, Move.Scissors, RoundResult.Draw)]
    public void Round_ResolvesCorrectly(Move home, Move away, RoundResult expected)
    {
        var round = new Round(home, away);
        round.Result.Should().Be(expected);
    }

    [Fact]
    public void Draw_DoesNotCountAsWin()
    {
        var round = new Round(Move.Rock, Move.Rock);
        round.Result.Should().Be(RoundResult.Draw);
        round.Result.Should().NotBe(RoundResult.HomeWin);
        round.Result.Should().NotBe(RoundResult.AwayWin);
    }
}

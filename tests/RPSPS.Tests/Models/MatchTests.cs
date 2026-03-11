using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;

namespace RPSPS.Tests.Models;

public class MatchTests
{
    [Fact]
    public void Match_EndsWhenPlayerReaches3Wins()
    {
        var home = new RandomPlayer(42);
        var away = new RandomPlayer(43);
        var result = Match.Play(home, away);

        Math.Max(result.HomeWins, result.AwayWins).Should().Be(3);
    }

    [Fact]
    public void Match_NeverExceeds5NonDrawRounds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var home = new RandomPlayer(seed);
            var away = new RandomPlayer(seed + 1000);
            var result = Match.Play(home, away);

            int nonDrawRounds = result.HomeWins + result.AwayWins;
            nonDrawRounds.Should().BeLessThanOrEqualTo(5);
            nonDrawRounds.Should().BeGreaterThanOrEqualTo(3); // minimum 3-0
        }
    }

    [Fact]
    public void Match_ProducesValidMatchResult()
    {
        var home = new RandomPlayer(42);
        var away = new RandomPlayer(43);
        var result = Match.Play(home, away);

        result.HomePlayerName.Should().Be("RandomPlayer");
        result.AwayPlayerName.Should().Be("RandomPlayer");
        result.RoundCount.Should().BeGreaterThan(0);
        result.WinnerName.Should().Be("RandomPlayer");
    }
}

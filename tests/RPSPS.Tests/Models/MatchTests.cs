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
        var match = new Match(home, away);
        var result = match.Play();

        Math.Max(result.HomeWins, result.AwayWins).Should().Be(3);
    }

    [Fact]
    public void Match_NeverExceeds5NonDrawRounds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var home = new RandomPlayer(seed);
            var away = new RandomPlayer(seed + 1000);
            var match = new Match(home, away);
            var result = match.Play();

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
        var match = new Match(home, away);
        var result = match.Play();

        result.HomePlayerName.Should().Be("RandomPlayer");
        result.AwayPlayerName.Should().Be("RandomPlayer");
        result.RoundCount.Should().BeGreaterThan(0);
        result.WinnerName.Should().Be("RandomPlayer");
    }

    [Fact]
    public void Match_StoresRoundHistory()
    {
        var home = new RandomPlayer(42);
        var away = new RandomPlayer(43);
        var match = new Match(home, away);
        var result = match.Play();

        result.Rounds.Should().NotBeEmpty();
        result.Rounds.Count.Should().Be(result.RoundCount);
        match.Rounds.Should().BeSameAs(result.Rounds);

        foreach (var round in result.Rounds)
        {
            round.Should().NotBeNull();
            ((int)round.HomeMove).Should().BeInRange(0, 2);
            ((int)round.AwayMove).Should().BeInRange(0, 2);
        }
    }

    [Fact]
    public void Match_FreshPlayersPerMatch()
    {
        var player1 = new RandomPlayer(42);
        var player2 = new RandomPlayer(43);

        // Clone simulates what Tournament does
        var clone1a = player1.Clone();
        var clone1b = player1.Clone();

        // Clones should be independent instances
        clone1a.Should().NotBeSameAs(clone1b);
    }
}

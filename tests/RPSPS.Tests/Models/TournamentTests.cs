using FluentAssertions;
using RPSPS.Models;
using RPSPS.Players;
using RPSPS.Engine;

namespace RPSPS.Tests.Models;

public class TournamentTests
{
    [Fact]
    public void Tournament_Runs6MatchesFor4Players()
    {
        var players = TournamentRunner.CreatePlayers(42);
        var result = Tournament.Run(players);

        result.MatchCount.Should().Be(6); // C(4,2) = 6
    }

    [Fact]
    public void Tournament_StandingsAreConsistent()
    {
        var players = TournamentRunner.CreatePlayers(42);
        var result = Tournament.Run(players);

        int totalWins = result.Standings.Sum(s => s.Wins);
        int totalLosses = result.Standings.Sum(s => s.Losses);

        totalWins.Should().Be(totalLosses); // Every match has one winner and one loser
        totalWins.Should().Be(6); // 6 matches = 6 wins
    }

    [Fact]
    public void Tournament_AllPlayersHaveStandings()
    {
        var players = TournamentRunner.CreatePlayers(42);
        var result = Tournament.Run(players);

        result.Standings.Should().HaveCount(4);
        result.Standings.Should().Contain(s => s.PlayerName == "RandomPlayer");
        result.Standings.Should().Contain(s => s.PlayerName == "PatternPlayer");
        result.Standings.Should().Contain(s => s.PlayerName == "FrequencyPlayer");
        result.Standings.Should().Contain(s => s.PlayerName == "MarkovPlayer");
    }

    [Fact]
    public void Tournament_TotalRoundsIsPositive()
    {
        var players = TournamentRunner.CreatePlayers(42);
        var result = Tournament.Run(players);

        result.TotalRounds.Should().BeGreaterThan(0);
    }
}

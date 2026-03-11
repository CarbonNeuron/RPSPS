namespace RPSPS.Display;

using RPSPS.Engine;
using RPSPS.Update;
using Spectre.Console;

public static class ResultsDisplay
{
    private static readonly string[] PlayerColors = ["red", "blue", "green", "yellow", "magenta", "cyan"];

    public static void ShowResults(BenchmarkResult result, bool verbose)
    {
        AnsiConsole.WriteLine();

        // ── Big hero number ──
        var rpsps = result.TournamentsPerSecond;
        var heroColor = rpsps switch
        {
            > 1_000_000 => "bold green",
            > 100_000 => "bold cyan",
            > 10_000 => "bold yellow",
            _ => "bold red"
        };
        AnsiConsole.Write(new Rule("[bold magenta]:bar_chart: Results[/]").RuleStyle("magenta"));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"  [{heroColor}]{rpsps:N0}[/] [dim]tournaments/sec[/]");
        AnsiConsole.WriteLine();

        // ── Throughput metrics as colored key-value lines ──
        WriteMetric("fuchsia", "Tournaments", result.TotalTournaments.ToString("N0"));
        WriteMetric("cyan", "Total Matches", result.TotalMatches.ToString("N0"));
        WriteMetric("cyan", "Total Rounds", result.TotalRounds.ToString("N0"));
        WriteMetric("deepskyblue1", "Avg Rounds/Match", result.AverageRoundsPerMatch.ToString("F2"));
        WriteMetric("deepskyblue1", "Rounds/sec", result.RoundsPerSecond.ToString("N0"));
        WriteMetric("grey", "Duration", $"{result.ActualDurationSeconds:F2}s");
        AnsiConsole.WriteLine();

        // ── Memory section ──
        AnsiConsole.Write(new Rule("[bold yellow]:floppy_disk: Memory[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();

        WriteMetric("orange1", "Peak Working Set", FormatBytes(result.PeakWorkingSetBytes));
        WriteMetric("orange1", "Total Allocated", FormatBytes(result.TotalAllocatedBytes));
        WriteMetric("gold1", "Alloc Rate", $"{FormatBytes((long)result.AllocationRateBytesPerSecond)}/s");

        var gcColor0 = result.GcGen0Collections > 1000 ? "red" : result.GcGen0Collections > 100 ? "yellow" : "green";
        var gcColor1 = result.GcGen1Collections > 100 ? "red" : result.GcGen1Collections > 10 ? "yellow" : "green";
        var gcColor2 = result.GcGen2Collections > 0 ? "red" : "green";

        WriteMetric(gcColor0, "GC Gen0", result.GcGen0Collections.ToString("N0"));
        WriteMetric(gcColor1, "GC Gen1", result.GcGen1Collections.ToString("N0"));
        WriteMetric(gcColor2, "GC Gen2", result.GcGen2Collections.ToString("N0"));
        AnsiConsole.WriteLine();

        // ── Player leaderboard ──
        AnsiConsole.Write(new Rule("[bold green]:trophy: Leaderboard[/]").RuleStyle("green"));
        AnsiConsole.WriteLine();

        var sorted = result.PlayerStats
            .OrderByDescending(p => p.Value.WinRate)
            .ToList();

        string[] medals = [":1st_place_medal:", ":2nd_place_medal:", ":3rd_place_medal:", "  "];
        var barWidth = 30;

        for (int i = 0; i < sorted.Count; i++)
        {
            var (name, stats) = sorted[i];
            var medal = i < medals.Length ? medals[i] : "  ";
            var color = PlayerColors[i % PlayerColors.Length];
            var pct = stats.WinRate * 100;
            var filled = (int)(pct / 100.0 * barWidth);
            var empty = barWidth - filled;

            var bar = new string('\u2588', filled) + new string('\u2591', empty);

            AnsiConsole.MarkupLine($"  {medal} [{color} bold]{name,-18}[/] [{color}]{bar}[/] [{color} bold]{pct:F1}%[/]  [dim]{stats.Wins:N0}W / {stats.Losses:N0}L[/]");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("dim"));
    }

    public static void ShowHeader()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("RPSPS")
            .Color(Color.Fuchsia));
        AnsiConsole.MarkupLine($"  [dim italic]Rock Paper Scissors Per Second[/]  [grey]v{Updater.CurrentVersion}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("fuchsia"));
        AnsiConsole.WriteLine();
    }

    public static void ShowConfiguration(int threads, double duration, int? seed)
    {
        AnsiConsole.MarkupLine("[bold fuchsia]:gear: Configuration[/]");
        WriteMetric("cyan", "Threads", threads.ToString());
        WriteMetric("cyan", "Duration", $"{duration}s");
        WriteMetric("cyan", "Seed", seed?.ToString() ?? "random");
        AnsiConsole.WriteLine();
    }

    private static void WriteMetric(string color, string label, string value)
    {
        AnsiConsole.MarkupLine($"  [dim]|[/] {label,-20} [{color} bold]{value}[/]");
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        int suffixIndex = 0;

        while (value >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            value /= 1024;
            suffixIndex++;
        }

        return $"{value:F1} {suffixes[suffixIndex]}";
    }
}

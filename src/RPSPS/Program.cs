using System.Text;
using RPSPS.Display;
using RPSPS.Engine;
using RPSPS.Models;
using RPSPS.Update;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

return EntryPoint.Main(args);

internal static class EntryPoint
{
    // Preserve types that Spectre.Console.Cli discovers via reflection at runtime
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BenchmarkCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BenchmarkSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(UpdateCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.EmptyCommandSettings", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand+Settings", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.OpenCliGeneratorCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.DelegateCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "OpenCli.OpenCliCommand", "Spectre.Console.Cli")]
    public static int Main(string[] args)
    {
#pragma warning disable IL3050 // RequiresDynamicCode — preserved via DynamicDependency above
        var app = new CommandApp<BenchmarkCommand>();
#pragma warning restore IL3050
        app.Configure(config =>
        {
            config.SetApplicationName("rpsps");
            config.SetApplicationVersion(Updater.CurrentVersion);
            config.AddCommand<UpdateCommand>("update")
                .WithDescription("Check for updates and install the latest version");
        });
        return app.Run(args);
    }
}

internal sealed class BenchmarkSettings : CommandSettings
{
    [CommandOption("-t|--threads")]
    [Description("Number of threads/tasks (0 = all cores)")]
    [DefaultValue(1)]
    public int Threads { get; set; }

    [CommandOption("-d|--duration")]
    [Description("Benchmark duration in seconds")]
    [DefaultValue(10)]
    public int Duration { get; set; }

    [CommandOption("-s|--seed")]
    [Description("RNG seed for reproducibility")]
    public int? Seed { get; set; }

    [CommandOption("-g|--game")]
    [Description("Game variant: classic, spock")]
    [DefaultValue("classic")]
    public string Game { get; set; } = "classic";

    [CommandOption("-c|--concurrency")]
    [Description("Concurrency model: threads, parallel, async, channels")]
    [DefaultValue("threads")]
    public string Concurrency { get; set; } = "threads";

    [CommandOption("--compare")]
    [Description("Run all concurrency modes and show comparison")]
    public bool Compare { get; set; }

    [CommandOption("--no-color")]
    [Description("Disable colored output")]
    public bool NoColor { get; set; }

    [CommandOption("--json")]
    [Description("Output results as JSON")]
    public bool Json { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("Show per-player stats and match breakdowns")]
    public bool Verbose { get; set; }

    [CommandOption("--nologo")]
    [Description("Suppress the banner header")]
    public bool NoLogo { get; set; }

    public GameMode ParsedGameMode => Game.ToLowerInvariant() switch
    {
        "spock" => GameMode.Spock,
        _ => GameMode.Classic
    };

    public ConcurrencyMode ParsedConcurrencyMode => Concurrency.ToLowerInvariant() switch
    {
        "parallel" => ConcurrencyMode.Parallel,
        "async" => ConcurrencyMode.Async,
        "channels" => ConcurrencyMode.Channels,
        _ => ConcurrencyMode.Threads
    };

    public override ValidationResult Validate()
    {
        if (Threads < 0)
            return ValidationResult.Error("Thread count must be 0 or greater");
        if (Duration <= 0)
            return ValidationResult.Error("Duration must be greater than 0");

        var validGames = new[] { "classic", "spock" };
        if (!validGames.Contains(Game.ToLowerInvariant()))
            return ValidationResult.Error($"Invalid game mode '{Game}'. Valid options: classic, spock");

        var validConcurrency = new[] { "threads", "parallel", "async", "channels" };
        if (!validConcurrency.Contains(Concurrency.ToLowerInvariant()))
            return ValidationResult.Error($"Invalid concurrency mode '{Concurrency}'. Valid options: threads, parallel, async, channels");

        return ValidationResult.Success();
    }
}

internal sealed class BenchmarkCommand : Command<BenchmarkSettings>
{
    public override int Execute(CommandContext context, BenchmarkSettings settings, CancellationToken cancellation)
    {
        if (settings.NoColor)
            AnsiConsole.Profile.Capabilities.Ansi = false;

        int threads = settings.Threads == 0 ? Environment.ProcessorCount : settings.Threads;
        int actualSeed = settings.Seed ?? Random.Shared.Next();
        var gameMode = settings.ParsedGameMode;
        var concurrencyMode = settings.ParsedConcurrencyMode;

        if (settings.Compare)
            return RunComparison(settings, threads, actualSeed, gameMode, cancellation);

        if (!settings.Json)
        {
            if (!settings.NoLogo)
                ResultsDisplay.ShowHeader();
            ResultsDisplay.ShowConfiguration(threads, settings.Duration, actualSeed, gameMode, concurrencyMode);
        }

        var engine = BenchmarkEngineFactory.Create(concurrencyMode, threads, settings.Duration, actualSeed, gameMode);

        BenchmarkResult result;

        try
        {
            if (!settings.Json)
            {
                result = AnsiConsole.Progress()
                    .AutoRefresh(true)
                    .HideCompleted(false)
                    .Columns(
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn(),
                        new PercentageColumn(),
                        new ElapsedTimeColumn(),
                        new SpinnerColumn())
                    .Start(ctx =>
                    {
                        var task = ctx.AddTask("Running benchmark", maxValue: settings.Duration);

                        var benchResult = engine.Run((elapsed, total) =>
                        {
                            task.Value = Math.Min(elapsed, total);
                        }, cancellation);

                        task.Value = settings.Duration;
                        return benchResult;
                    });

                ResultsDisplay.ShowResults(result, settings.Verbose);
            }
            else
            {
                result = engine.Run(cancellationToken: cancellation);
                var jsonStr = JsonSerializer.Serialize(result, BenchmarkResultJsonContext.Default.BenchmarkResult);
                Console.WriteLine(jsonStr);
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Cancelled[/]");
            return 1;
        }

        return 0;
    }

    private static int RunComparison(BenchmarkSettings settings, int threads, int seed, GameMode gameMode, CancellationToken cancellation)
    {
        var results = new Dictionary<ConcurrencyMode, BenchmarkResult>();

        if (!settings.Json)
        {
            if (!settings.NoLogo)
                ResultsDisplay.ShowHeader();
            AnsiConsole.MarkupLine($"[bold fuchsia]:bar_chart: Running comparison ({threads} threads, {gameMode.DisplayName().ToLowerInvariant()}, {settings.Duration}s each)[/]");
            AnsiConsole.WriteLine();
        }

        try
        {
            foreach (var mode in BenchmarkEngineFactory.AllModes)
            {
                if (!settings.Json)
                {
                    var result = AnsiConsole.Progress()
                        .AutoRefresh(true)
                        .HideCompleted(false)
                        .Columns(
                            new TaskDescriptionColumn(),
                            new ProgressBarColumn(),
                            new PercentageColumn(),
                            new ElapsedTimeColumn(),
                            new SpinnerColumn())
                        .Start(ctx =>
                        {
                            var task = ctx.AddTask($"[cyan]{mode.ToString().ToLowerInvariant()}[/]", maxValue: settings.Duration);
                            var engine = BenchmarkEngineFactory.Create(mode, threads, settings.Duration, seed, gameMode);

                            var benchResult = engine.Run((elapsed, total) =>
                            {
                                task.Value = Math.Min(elapsed, total);
                            }, cancellation);

                            task.Value = settings.Duration;
                            return benchResult;
                        });

                    results[mode] = result;
                }
                else
                {
                    var engine = BenchmarkEngineFactory.Create(mode, threads, settings.Duration, seed, gameMode);
                    results[mode] = engine.Run(cancellationToken: cancellation);
                }
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Cancelled[/]");
            return 1;
        }

        if (settings.Json)
        {
            var jsonResults = results.Values.ToArray();
            var jsonStr = JsonSerializer.Serialize(jsonResults, BenchmarkResultJsonContext.Default.BenchmarkResultArray);
            Console.WriteLine(jsonStr);
        }
        else
        {
            ResultsDisplay.ShowComparisonResults(results, threads, gameMode, settings.Duration);
        }

        return 0;
    }
}

internal sealed class UpdateCommand : AsyncCommand<EmptyCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellation)
    {
        return await Updater.RunAsync(cancellation);
    }
}

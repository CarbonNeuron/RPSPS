using System.Text;
using RPSPS.Display;
using RPSPS.Engine;
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
    [Description("Number of threads (0 = all cores)")]
    [DefaultValue(1)]
    public int Threads { get; set; }

    [CommandOption("-d|--duration")]
    [Description("Benchmark duration in seconds")]
    [DefaultValue(10)]
    public int Duration { get; set; }

    [CommandOption("-s|--seed")]
    [Description("RNG seed for reproducibility")]
    public int? Seed { get; set; }

    [CommandOption("--no-color")]
    [Description("Disable colored output")]
    public bool NoColor { get; set; }

    [CommandOption("--json")]
    [Description("Output results as JSON")]
    public bool Json { get; set; }

    [CommandOption("-v|--verbose")]
    [Description("Show per-player stats and match breakdowns")]
    public bool Verbose { get; set; }

    public override ValidationResult Validate()
    {
        if (Threads < 0)
            return ValidationResult.Error("Thread count must be 0 or greater");
        if (Duration <= 0)
            return ValidationResult.Error("Duration must be greater than 0");
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

        if (!settings.Json)
        {
            ResultsDisplay.ShowHeader();
            ResultsDisplay.ShowConfiguration(threads, settings.Duration, actualSeed);
        }

        var engine = new BenchmarkEngine(threads, settings.Duration, actualSeed);

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
}

internal sealed class UpdateCommand : AsyncCommand<EmptyCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellation)
    {
        return await Updater.RunAsync(cancellation);
    }
}

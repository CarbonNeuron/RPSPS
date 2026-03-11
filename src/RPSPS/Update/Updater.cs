namespace RPSPS.Update;

using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Spectre.Console;

public static class Updater
{
    private const string GitHubOwner = "CarbonNeuron";
    private const string GitHubRepo = "RPSPS";

    public static string CurrentVersion { get; } =
        typeof(Updater).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion.Split('+')[0] // strip build metadata
        ?? typeof(Updater).Assembly.GetName().Version?.ToString(3)
        ?? "0.0.0";

    public static async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        AnsiConsole.MarkupLine($"[dim]Current version:[/] [bold]v{CurrentVersion}[/]");
        AnsiConsole.MarkupLine("[dim]Checking for updates...[/]");

        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd($"RPSPS/{CurrentVersion}");

        GitHubRelease? release;
        try
        {
            release = await http.GetFromJsonAsync(
                $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest",
                UpdateJsonContext.Default.GitHubRelease,
                cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to check for updates:[/] {ex.Message}");
            return 1;
        }

        if (release is null || string.IsNullOrEmpty(release.TagName))
        {
            AnsiConsole.MarkupLine("[red]Could not determine latest version.[/]");
            return 1;
        }

        var latestVersion = release.TagName.TrimStart('v');
        if (latestVersion == CurrentVersion)
        {
            AnsiConsole.MarkupLine("[green]Already up to date.[/]");
            return 0;
        }

        AnsiConsole.MarkupLine($"[yellow]New version available:[/] [bold]v{latestVersion}[/]");

        var rid = GetCurrentRid();
        if (rid is null)
        {
            AnsiConsole.MarkupLine($"[red]Unsupported platform:[/] {RuntimeInformation.OSDescription} / {RuntimeInformation.OSArchitecture}");
            AnsiConsole.MarkupLine($"[dim]Download manually from:[/] {release.HtmlUrl}");
            return 1;
        }

        var extension = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".zip" : ".tar.gz";
        var assetName = $"rpsps-{rid}{extension}";
        var asset = release.Assets?.FirstOrDefault(a => a.Name == assetName);

        if (asset is null)
        {
            AnsiConsole.MarkupLine($"[red]No binary found for[/] [bold]{rid}[/]");
            AnsiConsole.MarkupLine($"[dim]Download manually from:[/] {release.HtmlUrl}");
            return 1;
        }

        var currentExe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(currentExe))
        {
            AnsiConsole.MarkupLine("[red]Cannot determine current executable path.[/]");
            return 1;
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"rpsps-update-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var archivePath = Path.Combine(tempDir, assetName);

            await AnsiConsole.Progress()
                .AutoRefresh(true)
                .Columns(
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new TransferSpeedColumn(),
                    new DownloadedColumn())
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask($"Downloading v{latestVersion}", autoStart: true);

                    using var response = await http.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1;
                    if (totalBytes > 0)
                        task.MaxValue = totalBytes;

                    await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    await using var fileStream = File.Create(archivePath);

                    var buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                        task.Increment(bytesRead);
                    }

                    task.Value = task.MaxValue;
                });

            // Extract
            var binaryName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "RPSPS.exe" : "RPSPS";
            var extractedPath = Path.Combine(tempDir, binaryName);

            if (archivePath.EndsWith(".tar.gz"))
            {
                var psi = new ProcessStartInfo("tar", $"xzf \"{archivePath}\" -C \"{tempDir}\"")
                {
                    RedirectStandardError = true
                };
                var proc = Process.Start(psi)!;
                await proc.WaitForExitAsync(cancellationToken);
                if (proc.ExitCode != 0)
                {
                    AnsiConsole.MarkupLine("[red]Failed to extract archive.[/]");
                    return 1;
                }
            }
            else
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(archivePath, tempDir);
            }

            if (!File.Exists(extractedPath))
            {
                AnsiConsole.MarkupLine($"[red]Expected binary not found in archive:[/] {binaryName}");
                return 1;
            }

            // Replace current binary
            var backupPath = currentExe + ".bak";
            try
            {
                // Move current to backup, move new to current
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(currentExe, backupPath);
                File.Move(extractedPath, currentExe);

                // Preserve executable permission on Unix
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    File.SetUnixFileMode(currentExe, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                                                      UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                                                      UnixFileMode.OtherRead | UnixFileMode.OtherExecute);

                // Clean up backup
                try { File.Delete(backupPath); } catch { /* best effort */ }

                AnsiConsole.MarkupLine($"[green]Updated to v{latestVersion}[/]");
                return 0;
            }
            catch
            {
                // Restore backup on failure
                if (File.Exists(backupPath) && !File.Exists(currentExe))
                    File.Move(backupPath, currentExe);
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            AnsiConsole.MarkupLine("[bold yellow]Update cancelled.[/]");
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Update failed:[/] {ex.Message}");
            return 1;
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { /* best effort */ }
        }
    }

    private static string? GetCurrentRid()
    {
        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => null
        };

        if (arch is null) return null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return $"linux-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return $"osx-{arch}";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return $"win-{arch}";

        return null;
    }
}

public sealed class GitHubRelease
{
    [JsonPropertyName("tag_name")]
    public string? TagName { get; set; }

    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }

    [JsonPropertyName("assets")]
    public List<GitHubAsset>? Assets { get; set; }
}

public sealed class GitHubAsset
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadUrl { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }
}

[JsonSerializable(typeof(GitHubRelease))]
[JsonSerializable(typeof(GitHubAsset))]
[JsonSerializable(typeof(List<GitHubAsset>))]
public partial class UpdateJsonContext : JsonSerializerContext
{
}

# RPSPS — Rock Paper Scissors Per Second

A novelty benchmark that measures how many Rock-Paper-Scissors tournament games per second your machine can run. Intentionally OOP-heavy to benchmark real-world object allocation, virtual dispatch, GC pressure, and collection usage — not just raw math.

## Quick Start

```bash
# Run with defaults (1 thread, 10 seconds)
dotnet run -c Release --project src/RPSPS

# All cores, 30 seconds, fixed seed
dotnet run -c Release --project src/RPSPS -- -t 0 -d 30 -s 42

# JSON output
dotnet run -c Release --project src/RPSPS -- -t 0 -d 10 --json
```

### Pre-built Binary (Native AOT)

Download a self-contained binary from [Releases](https://github.com/CarbonNeuron/RPSPS/releases) — no .NET runtime required.

```bash
./RPSPS -t 0 -d 10 -s 42
```

## Sample Output

```
  ____    ____    ____    ____    ____
 |  _ \  |  _ \  / ___|  |  _ \  / ___|
 | |_) | | |_) | \___ \  | |_) | \___ \
 |  _ <  |  __/   ___) | |  __/   ___) |
 |_| \_\ |_|     |____/  |_|     |____/

  Rock Paper Scissors Per Second  v1.0

────────────────────────────────────────────────

Configuration
  │ Threads   24
  │ Duration  10s
  │ Seed      42

─────────────── Results ────────────────────────

  1,284,729 tournaments/sec

  │ Tournaments          12,847,291
  │ Total Matches        77,083,746
  │ Total Rounds         289,063,547
  │ Avg Rounds/Match     5.95
  │ Rounds/sec           28,906,354
  │ Duration             10.00s

──────────────── Memory ────────────────────────

  │ Peak Working Set     62 MB
  │ Total Allocated      14.2 GB
  │ Alloc Rate           1.4 GB/s
  │ GC Gen0              847
  │ GC Gen1              12
  │ GC Gen2              0

─────────────── Leaderboard ────────────────────

  MarkovPlayer       ██████████████████░░░░  58.1%
  PatternPlayer      ████████████████░░░░░░  55.9%
  RandomPlayer       █████████████░░░░░░░░░  44.3%
  FrequencyPlayer    ████████████░░░░░░░░░░  41.7%
```

## CLI Options

```
USAGE:
    rpsps [OPTIONS]

OPTIONS:
    -h, --help                   Prints help information
    -t, --threads     1          Number of threads (0 = all cores)
    -d, --duration    10         Benchmark duration in seconds
    -s, --seed                   RNG seed for reproducibility
        --no-color               Disable colored output
        --json                   Output results as JSON
    -v, --verbose                Show per-player stats and match breakdowns
```

## How It Works

### Tournament Structure
- **4 player strategies** compete in a round-robin (6 matches per tournament)
- Each **match** is best-of-5 (first to 3 wins, draws replayed)
- One complete tournament = one "game" for the benchmark score

### Player Strategies

| Player | Strategy | Cost |
|--------|----------|------|
| **RandomPlayer** | Uniform random via `System.Random` | Lightest |
| **PatternPlayer** | Sliding window pattern detection, counters predicted move | Light |
| **FrequencyPlayer** | Tracks opponent move frequency, counters most common | Light |
| **MarkovPlayer** | Transition probability matrix, predicts next move from last | Heaviest |

### Metrics Reported
- **RPSPS** — tournaments per second (the headline number)
- Rounds/sec, total matches, average rounds per match
- GC collections (Gen0/1/2), peak working set, total bytes allocated, allocation rate
- Per-player win rates across all tournaments

## Project Structure

```
RPSPS/
├── RPSPS.slnx
├── src/RPSPS/              # Main benchmark tool
│   ├── Models/             # Move, Round, Match, Tournament
│   ├── Players/            # Abstract Player + 4 strategies
│   ├── Engine/             # BenchmarkEngine, TournamentRunner
│   └── Display/            # Spectre.Console rich output
├── tests/RPSPS.Tests/      # xUnit + FluentAssertions
└── benchmarks/RPSPS.Benchmarks/  # BenchmarkDotNet suite
```

## Building

```bash
# Build all projects
dotnet build

# Run tests
dotnet test

# Publish Native AOT binary
dotnet publish src/RPSPS -c Release

# Run BenchmarkDotNet suite
dotnet run -c Release --project benchmarks/RPSPS.Benchmarks
```

### Requirements
- .NET 10 SDK (for building from source)
- No runtime required for AOT-published binaries

## Tech Stack
- **Spectre.Console** — rich terminal output with progress bars
- **Spectre.Console.Cli** — CLI argument parsing
- **xUnit + FluentAssertions** — test framework
- **BenchmarkDotNet** — performance regression benchmarks
- **Native AOT** — self-contained binaries with no runtime dependency

## License

MIT

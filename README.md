# RPSPS — Rock Paper Scissors Per Second

A novelty benchmark that measures how many Rock-Paper-Scissors tournament games per second your machine can run. Intentionally OOP-heavy to benchmark real-world object allocation, virtual dispatch, GC pressure, and collection usage — not just raw math.

## Quick Start

```bash
# Run with defaults (1 thread, 10 seconds, classic mode)
dotnet run -c Release --project src/RPSPS

# All cores, 30 seconds, fixed seed
dotnet run -c Release --project src/RPSPS -- -t 0 -d 30 -s 42

# Spock mode with async concurrency
dotnet run -c Release --project src/RPSPS -- -t 0 -g spock -c async

# Compare all concurrency modes side-by-side
dotnet run -c Release --project src/RPSPS -- -t 0 --compare

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

  Rock Paper Scissors Per Second  v2.1.0

────────────────────────────────────────────────

⚙️  Configuration
  │ Threads      24
  │ Duration     10s
  │ Seed         42
  │ Game         Spock 🪨📄✂️🦎🖖
  │ Concurrency  channels

─────────────── 📊 Results ────────────────────

  98,247 tournaments/sec

  │ Tournaments          982,471
  │ Total Matches        5,894,826
  │ Total Rounds         30,459,232
  │ Avg Rounds/Match     5.17
  │ Rounds/sec           3,045,923
  │ Duration             10.00s

──────────────── 💾 Memory ────────────────────

  │ Peak Working Set     68 MB
  │ Total Allocated      10.7 GB
  │ Alloc Rate           1.1 GB/s
  │ GC Gen0              643
  │ GC Gen1              8
  │ GC Gen2              0

─────────────── 🏆 Leaderboard ───────────────

  🥇 MarkovPlayer       ██████████████████░░░░  58.1%
  🥈 PatternPlayer      ████████████████░░░░░░  50.1%
  🥉 RandomPlayer       ███████████████░░░░░░░  50.0%
     FrequencyPlayer    ████████████░░░░░░░░░░  41.7%
```

### Comparison Mode

```
./RPSPS -t 24 --compare

📊 Concurrency Comparison (24 threads, classic, 10s each)
╭──────────┬────────────┬────────────┬─────────╮
│   Mode   │    RPSPS   │ Rounds/sec │ vs Best │
├──────────┼────────────┼────────────┼─────────┤
│ threads  │  1,024,310 │  6,288,521 │  100.0% │
│ parallel │    892,100 │  5,476,340 │   87.1% │
│ async    │    978,000 │  6,012,000 │   95.5% │
│ channels │    901,500 │  5,534,000 │   88.0% │
╰──────────┴────────────┴────────────┴─────────╯
```

## CLI Options

```
USAGE:
    rpsps [OPTIONS]

OPTIONS:
    -h, --help                   Prints help information
    -t, --threads <n>            Number of threads/tasks (0 = all cores, default: 1)
    -d, --duration <secs>        Benchmark duration in seconds (default: 10)
    -s, --seed <n>               RNG seed for reproducibility
    -g, --game <mode>            Game variant: classic, spock (default: classic)
    -c, --concurrency <mode>     Concurrency model: threads, parallel, async, channels (default: threads)
        --compare                Run all concurrency modes and show comparison
        --no-color               Disable colored output
        --json                   Output results as JSON
    -v, --verbose                Show per-player stats and match breakdowns
        --nologo                 Suppress the banner header
```

## How It Works

### Game Modes

**Classic** (`--game classic`) — Standard Rock, Paper, Scissors (3 moves). 🪨📄✂️

**Spock** (`--game spock`) — Rock Paper Scissors Lizard Spock (5 moves). 🪨📄✂️🦎🖖

| Move     | Beats              |
|----------|---------------------|
| Rock     | Scissors, Lizard    |
| Paper    | Rock, Spock         |
| Scissors | Paper, Lizard       |
| Lizard   | Spock, Paper        |
| Spock    | Scissors, Rock      |

Each move beats 2 and loses to 2 — perfectly balanced.

### Concurrency Modes

| Mode | Strategy | What it tests |
|------|----------|---------------|
| **threads** | Dedicated `Thread` instances spinning on tournaments | Raw thread throughput baseline |
| **parallel** | `Parallel.For` with TPL work partitioning | TPL scheduling overhead |
| **async** | `Task.Run` + `Task.WhenAll` | Task scheduler overhead for CPU-bound work |
| **channels** | `System.Threading.Channels` producer/consumer | Channel throughput and backpressure |

Use `--compare` to run all four modes sequentially and see a side-by-side comparison table.

### Tournament Structure
- **4 player strategies** compete in a round-robin (6 matches per tournament)
- Each **match** is best-of-5 (first to 3 wins, draws replayed)
- **Fresh player clones** per match — no history leaks between matchups
- Every round constructs a `Round` object; every match builds a `List<Round>` history
- One complete tournament = one "game" for the benchmark score

### Player Strategies

| Player | Strategy | Cost |
|--------|----------|------|
| **RandomPlayer** | Uniform random via `System.Random` | Lightest |
| **PatternPlayer** | Sliding window pattern detection, counters predicted move | Light |
| **FrequencyPlayer** | Tracks opponent move frequency, counters most common | Light |
| **MarkovPlayer** | Transition probability matrix, predicts next move from last | Heaviest |

In Spock mode, all strategies expand to handle 5 moves — MarkovPlayer builds a 5x5 transition matrix, FrequencyPlayer tracks 5 frequencies, etc.

### Allocation Profile

The benchmark is designed to stress-test real OOP allocation patterns. Per tournament (~80+ heap objects):

| Layer | What allocates | Objects |
|-------|---------------|---------|
| Round (inner loop) | `new Round()` — sealed class, every round | ~37 |
| Match | `new Match()` + `new List<Round>()` + backing arrays | ~18 |
| Players | `player.Clone()` — fresh instances with own lists, arrays, RNG | 12 |
| MatchResult | `new MatchResult(...)` holding round history | 6 |
| Tournament | Standings array + `List<MatchResult>` + result | 7 |

This produces real GC pressure: ~1 GB/s allocation rate, hundreds of Gen0 collections per run.

### Metrics Reported
- **RPSPS** — tournaments per second (the headline number)
- Rounds/sec, total matches, average rounds per match
- GC collections (Gen0/1/2), peak working set, total bytes allocated, allocation rate
- Per-player win rates across all tournaments

## Project Structure

```
RPSPS/
├── RPSPS.slnx
├── src/RPSPS/
│   ├── Models/             # Move, Round, Match, Tournament, GameMode, ConcurrencyMode
│   ├── Players/            # Abstract Player + 4 strategies (with Clone())
│   ├── Engine/             # BenchmarkEngineBase + 4 engines + factory
│   └── Display/            # Spectre.Console rich output + comparison tables
├── tests/RPSPS.Tests/      # xUnit + FluentAssertions (90 tests)
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

### Version

Version is injected from git tags at build time. The csproj default is `0.0.0-dev` for local builds. CI and release workflows pass `-p:Version=X.Y.Z` derived from the tag.

To release a new version:
```bash
git tag v2.2.0
git push origin v2.2.0
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

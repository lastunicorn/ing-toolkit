# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ING Toolkit** is a .NET 8 library (`DustInTheWind.Ing.Toolkit` NuGet package) that parses CSV bank statement files exported by ING Homebank (Romania). The CSV format is non-standard — it is an Excel-exported document with multiple printable pages, shifted column headers, multi-record transactions, and interspersed signature blocks.

## Commands

```bash
# Build the solution
dotnet build ./Ing.Toolkit.slnx -c Release

# Restore packages (uses nuget.config)
dotnet restore ./Ing.Toolkit.slnx --configfile ./nuget.config

# Run the demo CLI (loads statement.csv from the output directory)
dotnet run --project sources/Ing.Toolkit.Demo/Ing.Toolkit.Demo.csproj

# Pack the NuGet package
dotnet pack ./sources/Ing.Toolkit/Ing.Toolkit.csproj -c Release -o ./artifacts
```

There are no automated tests in this repository currently.

## Architecture

### Projects

- **`sources/Ing.Toolkit/`** — the library; targets `net8.0`; published as `DustInTheWind.Ing.Toolkit` on NuGet. Assembly name and root namespace are derived from `Directory.Build.props` using `DustInTheWind.$(MSBuildProjectName)`.
- **`sources/Ing.Toolkit.Demo/`** — console app that loads `statement.csv` and prints transactions as a table using `ConsoleTools.Controls.Tables`. The sample `statement.csv` lives in `sources/Ing.Toolkit.Demo/` and is copied to the build output directory by MSBuild automatically — `dotnet run` works without manual setup.

### Parsing pipeline (`Ing.Toolkit/Csv/`)

All CSV internals are `internal`. The public surface is `StatementDocument` (a `Collection<BankTransaction>`) and `DocumentLoadResult`.

**Entry point:** `StatementDocument.LoadFromFileAsync` / `LoadAsync` — these accept a file path, string, `Stream`, `FileInfo`, `StreamReader`, or `TextReader`. All overloads funnel into `LoadInternalAsync(TextReader, CultureInfo)`.

**State machine:** `CsvStatementDocument` wraps `CsvHelper.CsvReader` and advances through `CsvDocumentReadState`:

```
New → PageHeader → TransactionsHeader → Transaction → AccountBalance → PageSignatures → (loop back or Ended)
```

`DetectNextState()` identifies the next section by inspecting the current CSV row's first cell content (Romanian strings like `"Titular cont"`, `"Data"`, `"Sold iniţial"`, or a date in `dd MMMM yyyy` format).

**Key parsing details:**
- Default culture is `ro-RO` (Romanian). Callers may pass a custom `CultureInfo`.
- Dates use format `dd MMMM yyyy` with Romanian month names.
- Decimal values use Romanian comma separator; empty cells map to `null`.
- Transaction details can span multiple CSV rows; continuation rows have an empty first cell and no page-footer marker.
- The transactions header row has cells shifted one column right relative to the data rows; `CsvTransactionsHeader` handles this offset.
- `DocumentLoadResult` wraps the `StatementDocument` plus a `Warnings` list for non-fatal parse issues. It has an implicit cast to `StatementDocument` for convenience.

### Public exception types

All exceptions inherit from `DocumentLoadException`. The full hierarchy exposed by the library:
- `DocumentLoadException` — base; wraps unexpected errors or signals malformed CSV; callers should always catch this.
- `DataHeaderMissingException` — no transactions-header row found.
- `InvalidCsvRecordException` — a record could not be parsed.
- `InvalidCsvRecordLengthException` — a record has the wrong number of fields.
- `InvalidReadStateException` — `CsvStatementDocument` called in the wrong sequence (internal; surfaces if the state machine is misused).

### Publishing

NuGet is published via the `publish-nuget.yml` workflow, triggered by a `v*.*.*` tag. Version is never hardcoded in the csproj — it is passed as `-p:Version=...` at build/pack time. The `Directory.Build.props` sets `Version=0.0.0.0` as a placeholder.

## Code Conventions

From `.github/copilot-instructions.md`:

- Do not use `var`; use the actual type.
- Use `x` as the LINQ lambda parameter name.
- Prefer `new()` for object instantiation.
- Object initializers with more than one property: one property per line.
- Omit curly braces for single-line `if`, `for`, `using` bodies.
- No underscores in C# field names.
- XML doc comments only on public types exposed via the NuGet package; skip them for internal types.
- Test naming: `Having<...>_When<...>_Then<...>`. Each tested method gets its own test file; all test files for a class go in a directory named after the class.
- `Assert.Throws` lambdas must use block bodies.

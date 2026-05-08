# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A Dalamud plugin (FFXIV) that displays high-end-content prog points sourced from Tomestone.gg and FFLogs in-game. Built on `Dalamud.NET.SDK/15.0.0`, targets .NET 8, x64 only. The solution also vendors a fork of `NetStone` (Lodestone scraping library) under `NetStone/`.

In-game commands registered by `Commands.cs`:
- `/ts` — toggle main window; `party`/`p` opens the party view, anything else searches for that character (supports placeholders like `<t>`, `<1>`, `<mo>`).
- `/tsconfig` (or `/ts config`) — toggle config window.

## Build / test

```powershell
dotnet restore -r win TomestoneViewer.sln
dotnet build --configuration Release       # or Debug
dotnet test --no-restore                   # runs NetStone.Test
```

CI (`.github/workflows/build.yml`) downloads Dalamud (latest + staging) into `%AppData%\XIVLauncher\addon\Hooks\dev\` before building — this is required because `Dalamud.NET.SDK` references those binaries at compile time. Locally, your existing XIVLauncher install satisfies the same requirement; without it, build will fail.

Release output (and the artifact pushed to the Dalamud plugin repo) lives at `TomestoneViewer/bin/x64/Release/TomestoneViewer/`.

When bumping the plugin, the version string in `TomestoneViewer/TomestoneViewer.csproj` (`<Version>`) is the source of truth; keep recent commits' style ("Set version to X.Y.Z.W").

## Architecture

### Service locator + Dalamud DI

`Service.cs` is a static service locator. Dalamud injects framework services into `[PluginService]` static properties; the plugin's own singletons (`Configuration`, `CharDataManager`, `MainWindow`, etc.) are assigned in `TomestoneViewerPlugin`'s constructor. This is intentional — most code reaches through `Service.X` rather than passing dependencies.

### Decorated client chains (the most important pattern)

Both external data sources are accessed through decorator chains assembled in `TomestoneViewerPlugin`:

```
TomestoneClient: Web -> Safe -> Sync -> Cached
FFLogsClient:    Web -> Safe -> Sync -> Cached -> Toggleable (gated on Configuration.FFLogsEnabled)
```

Each layer implements the same interface (`ITomestoneClient` / `IFFLogsClient`):
- **Web**: real HTTP, parses dynamic JSON / scrapes Tomestone.gg.
- **Safe**: catches exceptions, converts to typed errors.
- **Sync**: serializes concurrent calls for the same key (avoids duplicate in-flight requests).
- **Cached**: 30-min TTL via `Cache<TKey, TValue, TError>`; `PersistentFFLogsCache` additionally persists FFLogs results to disk under `pluginInterface.ConfigDirectory/FFLogsOldEncountersCache`.
- **Toggleable** (FFLogs only): short-circuits when the feature flag is off.
- **Cancelable** (instantiated per-`CharDataLoader`, not in the chain): wraps a chain so that a load can be cancelled when the displayed/party character changes.

When changing client behavior, decide which layer it belongs in instead of cross-cutting through all of them.

### `ClientResponse<TError, T>` — pervasive Either type

All client methods return `Task<ClientResponse<TError, T>>`. It is an Either with a `Cachable` bit (driven by `IClientError.Cachable`) and methods `Map`/`FlatMap`/`MapAsync`/`FlatMapAsync`/`Fold`/`FoldAsync`/`IfSuccessOrElse`/`Recover`/`Collate`. The codebase uses these monadic combinators heavily — prefer them to imperative null/error checks when extending client/parsing code.

Errors are typed enums-as-classes implementing `IClientError`; `error.CanIgnore` (e.g. `CharacterTomestoneDisabled`) lets callers proceed with empty data instead of surfacing the error.

### Domain: `Location` is the encounter registry

`TomestoneViewer/Character/Encounter/Location.cs` holds a hardcoded `ALL` list of every supported ultimate/savage. Each entry binds:
- a `TerritoryId` (FFXIV territory, used to auto-detect when the player is in the duty),
- a `TomestoneLocation` (slug + expansion + tomestone-internal ids), and
- an `FFLogsLocation` (one or more `FFLogsZone(bossId, isOldEncounter)` — old encounters route through the historical cache path).

**Adding a new encounter = adding a row to `ALL`.** The TODOs in that file (e.g. UMAD with placeholder ids `new(0)`) are the pattern: ship the row early with zeroes, then fill ids when the encounter goes live. `Location.Find(territoryId)` and `Location.AllTerritories()` are how the rest of the plugin maps from the live game state into this registry.

### Character data lifecycle

```
CharDataManager (owns party list + displayedChar)
   └─ CharDataFactory.Create(characterId)
        └─ CharData (public surface)
             └─ CharDataLoader (async orchestration; one CancelableXxxClient per loader)
                  ├─ tomestone.FetchLodestoneId
                  ├─ tomestone.FetchCharacterSummary  (errors with CanIgnore become empty summary)
                  └─ for each Location: FetchTomestoneForLocation -> FetchFFLogsForLocation
```

`EncounterData` per `(character, location)` has both a `Tomestone` and `FFLogs` slot. FFLogs is only fetched if Tomestone reports the encounter as cleared or has no data — this avoids burning the FFLogs API quota on in-progress encounters.

When a new character is selected the previous `CharData.Disable()` cancels any pending requests via the `Cancelable*Client` decorators.

### GUI layout

ImGui through `Dalamud.Interface.Windowing`. Two top-level windows registered in `TomestoneViewerPlugin`: `MainWindow` and `ConfigWindow`. `MainWindow` toggles between a single-character view (`SingleCharacterView`) and `PartyTableView`; both reuse the cell components in `GUI/Widgets/` (`Tabular`, `Tooltiped`, `NameplateWidget`, `EncounterplateWidget`, `TomestoneProgWidget`, `ClearCountWidget`, `PlayerTrackWidget`).

Window/state mutation goes through `WindowsController` and `CharacterSelectorController` — UI code calls into controllers, never directly into `CharDataManager`.

### Game-state integrations

- `TerritoryOfInterestDetector` (`GameSystems/`): hooks into territory changes and into the close events of the LookingForGroup addons to guess the "encounter of interest" so the party view auto-focuses the right column. It only honors territories that exist in `Location.AllTerritories()`.
- `ContextMenu`: adds a "View on Tomestone" entry to the in-game player context menu.
- `External/PlayerTrackInterface`: optional integration with the PlayerTrack plugin (resolved via `IPC`); reads notes for the displayed character.
- `External/FFLogsViewerConfigReader`: one-shot import path for users migrating from the FFLogsViewer plugin.

## Conventions

- StyleCop.Analyzers is enabled (see `stylecop.json` and `.editorconfig`). Notable enforced rules: `using` directives outside the namespace, `System` usings first, blank lines between using groups. The `.editorconfig` also encodes naming rules (private fields camelCase, private static/const PascalCase, events On-prefixed) — let your IDE format-on-save rather than fighting these by hand.
- 4-space indent, LF line endings, UTF-8.
- DEBUG builds inject fake party members in `CharDataManager.UpdatePartyMemebers` (the typo in the method name is real) for layout work without being in a real party. Don't remove the `#if DEBUG` block.

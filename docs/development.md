# Development and validation

[Overview](../README.md) · [Features](features.md)

## Build and repository layout

Use a licensed local installation with MelonLoader-generated IL2CPP references.
Follow [installation](installation.md) first. The project targets .NET 6 and uses
the .NET 10 SDK pinned through `global.json`; packages have a committed lock file.

```bash
./scripts/build.sh -p:RestoreLockedMode=true
```

Output: `src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll`.
Deploy with `./scripts/deploy.sh` while the game is closed. Builds alone do not
replace the installed DLL.

| Location | Purpose |
| --- | --- |
| `src/InterloperOverhaul/` | MelonLoader entry point, Harmony patches, native integration |
| `shared/` | Deterministic policies, recipes, item values, run-state serialization |
| `tests/PolicyChecks/` | Standalone checks without proprietary game references |
| `tests/*Validation.cs` | Native probes compiled only for runtime validation |
| `scripts/` | Setup, build, deployment, launch, isolated runtime validation |
| `docs/features.md` | Current player-facing behavior and exact values |
| `CHANGELOG.md` | Historical changes, including retired behavior |
| `.local/` | Ignored local research, captures, and validation output |
| `<game>/MelonLoader/Il2CppAssemblies/` | Generated native-game references, never committed |

Use `TLD_GAME_DIR` for a non-default installation. `build.sh` also accepts
`-p:GameDir=/path/to/game`. Proprietary game assemblies, assets, save files,
research dumps, and build products are excluded from Git.

## Automated checks

```bash
dotnet run --project tests/PolicyChecks/PolicyChecks.csproj -c Release
bash -n scripts/*.sh
git diff --check
./scripts/validate-runtime.sh
```

The current verified baseline is **3,319 policy checks** and **1,100 native runtime
checks** on game 2.55 / mod 0.24.0. Counts record that run, rather than promising
coverage of every gameplay interaction. Documentation-only updates do not require
opening the game or replacing the installed DLL.

GitHub Actions runs policy checks, shell syntax, and whitespace checks. Full
compilation/runtime validation needs the local licensed game and is not performed
by the public CI runner.

### What runtime validation does

The game must be closed and a production DLL already installed. The script backs
up that DLL, builds with `RuntimeValidation=true`, temporarily installs the probe,
and launches the main menu without loading a survival save. It exercises actual
Harmony patches against real gear prefabs, synthetic objects/components, cloned
bunker configurations, and native serializers. Probe builds enable background
execution so losing window focus does not pause checks.

The installed DLL is restored when the script exits. Results are copied to
`.local/runtime-validation.log`; process output is in `.local/runtime-console.log`.
Validation classes are excluded from production builds. A missing success marker
or loader/Harmony/native interop `[ERROR]` fails the run, even if individual checks
passed. The probe has a timeout and may require forced termination during
Unity/.NET shutdown after recording results.

The validated runtime settings include `DOTNET_ReadyToRun=0`,
`DOTNET_TieredCompilation=0`, and `DOTNET_EnableWriteXorExecute=0`. Intermittent
loader startup and probe shutdown failures have still occurred. A completed probe
is not evidence that those native/runtime issues are fixed.

### Coverage and limits

| Area | Automated coverage | Needs survival playtesting |
| --- | --- | --- |
| World/loot | Authored tables, excluded objects, rarity boundaries, source geography, allocation diversity, old snapshots, second-copy migration/issuance, rejected beach opportunities | Complete world yield, expedition traversal/pickup, full beach respawn cycles |
| Crafting/variants | Native recipes, tools/materials, Cannery start gate, reserved inputs, rollback, progress and identity serialization | Full UI flow, time acceleration, interruptions, Trader collection |
| Items/accessories | Weights, contents/stacks, permanent unlocks, selection, repair setup, clothing/calorie migration, scene unload/asset collection followed by native pickup | Complete clothing/inventory/first-aid interactions during normal play |
| Fire/skills/combat | Tinder display/rolls, feat values, existing/new unlock notification behavior, skill thresholds, controlled repair/morale/swipe hooks, cougar save round trips | Actual fire-start loops, ruined repair with resource consumption, combat |
| Food/afflictions | Bundle crafting/partial saves, salt accounting, separate frostbite gauges, parasite medication/rest, legacy conversion | First-aid UI, exposed body parts, long recovery and interruption sequences |
| Vehicles/mail/caches | Warmth, fuel identity/quantity/empty saves, mailbox pools, 24/25-stone requirement boundary | Search interaction, physical placement, revisits across regions |

Controlled callbacks and menu fixtures do not simulate attacks, consume a player's
repair resources, or replace a complete travel/save/quit/reload playthrough. The
isolated launcher check also covers starting Steam before game preload variables
are exported, and setting the native Steam app IDs.

## Manual playtest checklist

Use a disposable Interloper run and keep an unmodified backup for migration checks.
Existing loot must not be mistaken for the distribution of a newly generated world.

| Scenario | Observe |
| --- | --- |
| New-game setup | Region selection, random start, three unlocked feat slots |
| Coastal Highway and bunkers | Intact house entrances; newly assigned cache distribution; save/reload consistency |
| Expedition equipment | Two copies per firearm/tool, eight identities across five expedition destinations; no generic or beach replacements; no reissue after pickup/loss/revisit |
| Schematics | Native site gives the correct card/text; pickup learns one recipe; save/reload retains knowledge |
| Cannery conversions | Correct light/tools/materials; empty input firearm; condition/identity preserved while the other firearm stays untouched; interrupted work resumes; reversal costs only time |
| Other manufacturing | Reject starts outside the Cannery; correct partial powder; fresh-flare requirements; milling restoration |
| Pickup after travel | Collect research/common books, newsprint, and tools before/after multiple interior transitions; inspection models must disappear and items enter inventory |
| Clothing and accessories | Full/worn values; vest repair; belt affects tools only; one heaviest per type; holster affects one rifle; duplicates do not stack |
| Food and fire | Partial food/bundle weights; mixed salt deposits; tinder selection at Firestarting 3+; UI chance matches behavior |
| Mending/Cooking 5 | Ruined repair starts and consumes normal materials; ruined food keeps native risks; non-ruined food retains immunity |
| Frostbite/parasites | Body-area stacking and warm recovery; one medication dose plus 24 hours rest; save/reload during recovery |
| Wildlife/noisemakers | Cougar swipe/health; prybar flee; timberwolf morale; fixed player explosive damage |
| Roadside supplies | Tank-search interaction; small fuel quantity; persistent empty tanks; paper/book collection in mailboxes |
| Shelter/cache | Warmth while inside and resting; slower shelter decay; 25-stone build/dismantle |
| Difficulty isolation | Compare Stalker/Misery item and system values after returning to the menu in the same process |

## Reporting a problem

Include the following in an issue or playtest report:

- Game, mod, MelonLoader, and operating-system versions, plus other installed mods.
- Difficulty and whether the run predates the current mod/allocation rules.
- Region/location, item or recipe, expected result, actual result, and exact steps.
- A relevant screenshot and the error section of `MelonLoader/Latest.log` or
  `Player.log`; do not publish account information or unrelated private files.
- Whether it reproduces after saving/reloading, revisiting, or in a disposable new run.

## Keeping documentation current

Write all repository documentation, comments, scripts, and test messages in English.
Update the relevant section of [features](features.md) with the final implemented
behavior and values, including save boundaries. Keep the README as an overview
and navigation page. Put chronological changes in the changelog; do not leave
superseded spawn/recipe tables in the current feature reference.

Use relative links for repository content. Put illustration sources and rights in
[media references](media.md), with attribution for every external image. Keep
third-party code notices separate from documentation/media credits.

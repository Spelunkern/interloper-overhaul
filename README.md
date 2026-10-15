# Interloper Overhaul

A community mod for **The Long Dark**, focused on restoring content excluded from
standard **Interloper**, with a few general quality-of-life changes. By Spelunkern.

## Version 0.6.0: full object availability

Interloper's object exclusions are removed for weapons, ammunition, tools, clothing,
unique variants, containers, collectibles, plants, and other world objects that are
available in normal survival gameplay. This applies to scene objects, parent groups,
gear prefabs, native loot-table filtering, and the Trader's item eligibility.

- Original loot tables, weights, locations, random selections, and spawn probabilities
  remain in use. Items are eligible to spawn; every location is not forced to contain
  every possible item.
- Entirely disabled random object groups recover their authored Medium-resource
  (Stalker) count. Existing positive Interloper counts are not increased.
- The NoFirearms, NoRifle, and NoRevolver gear bans are removed from Interloper's
  configuration. Unrelated gameplay tags and exclusions shared with Stalker remain.
- Wildlife and survivor spawn groups retain their original rules. Weather, health,
  decay, general container quantities, and empty-container settings are not rebalanced.
- Mission progress, locks, keys, research-set selection, and DLC ownership still apply.
  Content exclusive to Wintermute or challenges is not imported into Survival.
- Reduced radio-cache and mountain-cargo alternatives remain disabled alongside
  their restored normal versions to prevent overlapping duplicate containers.

This is a content-restoration baseline. Item-specific balance changes are deferred.
All tracked documentation, code comments, test messages, and scripts are in English.

## Other included changes

- **Coastal Highway:** restore intact houses in all five randomized building groups,
  disable the Interloper-only burnt-house group, and restore the two independently
  hidden houses, including their interior access.
- **Prepper caches:** use the original normal distribution of **three furnished and
  six abandoned interiors**. The game still selects their locations.
- **Timberwolf Mountain:** restore normal cargo assemblies, including summit cargo,
  while preserving hacksaw requirements and locks.
- **Hushed River Valley:** restore the Moose-Hide Satchel at the Mysterious Signal
  Fire, keeping the original random choice between its two locations.
- **Tales:** restore unique rewards and normal radio-cache/crash-box variants.
  Alpha, Beta, Gamma, and Omega bunker contents and radio-cache container rewards
  use Stalker generation in a synchronous scope. Private loot-table clones prevent
  their cached filters from affecting ordinary world containers. The original mode
  and table references are restored before generation returns, including on exceptions.
- **Sutherland:** restore story rewards and unlocked exchanges, preserving trust,
  delivery requirements, prices, and progression. General stock now also benefits
  from the removal of object exclusions.
- **New games:** allow normal Stalker region choices and random selection, retaining
  outdoor starts. Allow up to **three already unlocked feats**. Existing saves do not
  receive a different feat selection.
- **Direct main menu:** skip boot logos, initial disclaimers, the keypress gate, and
  the menu introduction movie. Profile loading and error handling still run. This
  startup change is global; in-game cinematics retain normal playback.

Content restoration targets standard Interloper only. Pilgrim, Voyageur, Stalker,
Misery, Custom, challenges, and story mode retain their own settings.

## Existing saves

Use a **new survival game** to obtain complete world generation and the new setup
options. Loot in visited scenes is not regenerated, and searched containers are not
refilled. No save data is deleted or reset.

Saved Coastal house selections are redirected to intact variants when loading the
region. Existing bunker assignments are preserved, including Interloper-specific
interiors; the normal distribution applies when generating a new world. Restored
bunker variants remain recognizable when saving and reloading.

## Setup and use

Verified environment: The Long Dark **2.55**, Unity **6000.0.60f1**, Linux x64,
MelonLoader **0.7.3**, .NET runtime **6.0.36**, .NET SDK **10.0.x**.

Requirements: `git`, .NET 10 SDK, `bash`, `curl`, `unzip`, `sha256sum`, and `pgrep`.
Back up `~/.local/share/Hinterland/TheLongDark` before playing with mods.

```bash
./scripts/setup.sh
./scripts/launch.sh  # First run: wait for assembly generation, then close the game.
./scripts/deploy.sh
./scripts/launch.sh
```

The private runtime is installed under `~/.local/share/tld-modding/dotnet`.
`deploy.sh` requires the game to be closed, builds Release, and installs
`Mods/InterloperOverhaul.dll`. Remove an old `Mods/TheLongDarkMod.dll` scaffold if
present so only the current mod loads. To build without installing, run
`./scripts/build.sh`.

The launcher skips MelonLoader's own startup screen. On Linux, it disables ReadyToRun
for the .NET 6 runtime following intermittent loader startup failures observed during
validation. Set `DOTNET_ReadyToRun=1` to test the original runtime configuration.

Optional variables: `TLD_GAME_DIR` for another game installation and `TLD_DOTNET_ROOT`
for another runtime. `build.sh` also accepts `-p:GameDir=/path/to/game`.
Game assets, save files, generated references, and research dumps are excluded from Git.

### Steam

In Properties > General > Launch Options, enter the absolute launcher path in quotes,
followed by `%command%`. For this checkout:

```text
"/home/franco/Projects/the-long-dark-mod/scripts/launch.sh" %command%
```

Remove this option to launch without MelonLoader. To remove only this mod, delete
`Mods/InterloperOverhaul.dll` while the game is closed.

## Development and validation

- Source: `src/InterloperOverhaul/`.
- World selection policies: `shared/WorldPolicy.cs`.
- Generated references: `<game>/MelonLoader/Il2CppAssemblies/`.
- Loader log: `<game>/MelonLoader/Latest.log`.

```bash
./scripts/build.sh -p:RestoreLockedMode=true
dotnet run --project tests/PolicyChecks/PolicyChecks.csproj -c Release
./scripts/validate-runtime.sh
```

Runtime validation requires a closed game and an installed Release DLL. It temporarily
builds a validation DLL, opens the main menu without input, and exercises the actual
Harmony patches against synthetic world objects, real gear prefabs and loot filters,
and cloned bunker configurations. It never opens a survival save. The installed DLL
is restored afterward; the report is written to `.local/runtime-validation.log`.
Validation code is excluded from the production build. Native interop errors fail
the validation even if individual checks report success. The injected probe may
require forced termination during Unity/.NET shutdown after reporting its results;
this does not validate a normal playthrough or normal shutdown behavior.

GitHub Actions checks policies and scripts without distributing proprietary DLLs.
Full compilation and MelonLoader validation run locally against a licensed game
installation. The mod DLL is built at
`src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll`.

## Compatibility references

- [Interloper restrictions and rewards](https://thelongdark.fandom.com/wiki/Interloper)
- Implementation is checked against components and scenes in the local 2.55 installation.
- [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3)
- [Native Linux setup](https://github.com/LavaGang/MelonWiki/blob/master/docs/gettingstarted.md#linux-native-games)
- [RerollBurntBuildings: Coastal Highway building groups](https://github.com/GruffCassquatch/RerollBurntBuildings)

An independent community project, not affiliated with Hinterland.

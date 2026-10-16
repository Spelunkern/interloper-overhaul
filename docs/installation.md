# Installation and troubleshooting

[Overview](../README.md) · [Features](features.md) · [Compatibility](compatibility.md)

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

The launcher starts Steam in the background if it is absent, waits for its process,
and sets the native game app IDs before loading the mod runtime. This prevents the
-998 startup screen caused by an unavailable Steam client. Steam must be signed in;
launcher diagnostics are saved under `~/.local/state/interloper-overhaul`.
The launcher skips MelonLoader's own startup screen. On Linux, it disables ReadyToRun
for the .NET 6 runtime following intermittent loader startup failures observed during
validation. Tiered compilation and write-xor-execute are also disabled by default,
matching the runtime probe configuration. Each `DOTNET_` variable can be overridden
explicitly; these settings do not claim to eliminate every loader startup failure.

Optional variables: `TLD_GAME_DIR` for another game installation and `TLD_DOTNET_ROOT`
for another runtime. `build.sh` also accepts `-p:GameDir=/path/to/game`.
Game assets, save files, generated references, and research dumps are excluded from Git.

## Steam launch options

In Properties > General > Launch Options, enter the absolute launcher path in quotes,
followed by `%command%`. For this checkout:

```text
"/home/franco/Projects/the-long-dark-mod/scripts/launch.sh" %command%
```

Remove this option to launch without MelonLoader. To remove only this mod, delete
`Mods/InterloperOverhaul.dll` while the game is closed.

## Desktop launcher

The development machine's **Interloper Overhaul** desktop shortcut runs the same
`scripts/launch.sh` entry point. If the checkout moves, update the shortcut's
absolute path and the Steam launch option. The mod DLL remains installed under
the game's `Mods` folder; a repository update only takes effect in-game after
building and deploying it.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Failed to load DLL / error -998 | Ensure Steam is running and signed in; use the included launcher. The observed -998 case was Steam API initialization failing because the client was absent |
| Missing generated references during build | Run setup and launch the game once to let MelonLoader generate its assemblies, then close it and deploy |
| Deploy refuses to run | Close The Long Dark before replacing its loaded DLL |
| Restored content missing in a visited scene | Saved scene contents and searched containers are preserved; test a new run for generation changes |
| F8 opens a disabled debug panel | Load a game, then enable tools for the current session |
| Intermittent crash before the menu | Inspect loader/game logs; the launcher includes tested runtime settings, but intermittent native/loader startup failures have still been observed |
| Old and new mod both load | Remove the obsolete `Mods/TheLongDarkMod.dll` scaffold; keep only the current DLL |

Useful logs:

- `<game>/MelonLoader/Latest.log` — loader messages, Harmony patches, mod version.
- `~/.config/unity3d/Hinterland/TheLongDark/Player.log` — native Unity/game output.
- `${XDG_STATE_HOME:-$HOME/.local/state}/interloper-overhaul/steam-start.log` — Steam
  startup output when the launcher starts the client.

For a reproducible failure, follow the [issue-report checklist](development.md#reporting-a-problem).

## Remove or disable

With the game closed, remove `Mods/InterloperOverhaul.dll` to remove this mod.
Remove the Steam launch option to stop using the supplied launcher. The project
scripts do not automatically restore changes made to a survival save: use a
pre-mod backup if you need to return that run to its previous state. Review
[save compatibility](compatibility.md#removing-the-mod) before removing it from a
run containing custom bundles or modified progression.

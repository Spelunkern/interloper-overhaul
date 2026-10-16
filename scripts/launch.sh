#!/usr/bin/env bash
set -euo pipefail
game_dir="${TLD_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/TheLongDark}"
runtime_dir="${TLD_DOTNET_ROOT:-$HOME/.local/share/tld-modding/dotnet}"
[[ -f "$game_dir/MelonLoader.Bootstrap.so" ]] || { echo "MelonLoader is missing." >&2; exit 1; }
# The native Steam API returns the game's -998 startup screen when the client
# is absent. Start Steam before exporting the game's preload/runtime environment.
if ! pgrep -x steam >/dev/null; then
    command -v steam >/dev/null || { echo "Steam is required to launch The Long Dark." >&2; exit 1; }
    launcher_state_dir="${XDG_STATE_HOME:-$HOME/.local/state}/interloper-overhaul"
    mkdir -p "$launcher_state_dir"
    nohup steam -silent >> "$launcher_state_dir/steam-start.log" 2>&1 < /dev/null &
    for ((attempt=0;attempt<60;attempt++)); do
        if pgrep -x steam >/dev/null; then break; fi
        sleep 1
    done
    pgrep -x steam >/dev/null || { echo "Steam did not start; see $launcher_state_dir/steam-start.log" >&2; exit 1; }
    sleep 3
fi
export SteamAppId="${SteamAppId:-305620}"
export SteamGameId="${SteamGameId:-305620}"
if [[ -x "$runtime_dir/dotnet" ]]; then
    export DOTNET_ROOT="$runtime_dir"
    export DOTNET_ROOT_X64="$runtime_dir"
fi
# Avoid the intermittent CLR 6 startup crash observed with precompiled runtime code.
export DOTNET_ReadyToRun="${DOTNET_ReadyToRun:-0}"
export DOTNET_TieredCompilation="${DOTNET_TieredCompilation:-0}"
export DOTNET_EnableWriteXorExecute="${DOTNET_EnableWriteXorExecute:-0}"
export LD_LIBRARY_PATH="$game_dir${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export LD_PRELOAD="MelonLoader.Bootstrap.so${LD_PRELOAD:+:$LD_PRELOAD}"
if (( $# )); then
    exec "$@" --melonloader.disablestartscreen
fi
cd "$game_dir"
exec ./tld.x86_64 --melonloader.disablestartscreen

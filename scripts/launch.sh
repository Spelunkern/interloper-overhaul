#!/usr/bin/env bash
set -euo pipefail
game_dir="${TLD_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/TheLongDark}"
runtime_dir="${TLD_DOTNET_ROOT:-$HOME/.local/share/tld-modding/dotnet}"
[[ -f "$game_dir/MelonLoader.Bootstrap.so" ]] || { echo "MelonLoader is missing." >&2; exit 1; }
if [[ -x "$runtime_dir/dotnet" ]]; then
    export DOTNET_ROOT="$runtime_dir"
    export DOTNET_ROOT_X64="$runtime_dir"
fi
export LD_LIBRARY_PATH="$game_dir${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
export LD_PRELOAD="MelonLoader.Bootstrap.so${LD_PRELOAD:+:$LD_PRELOAD}"
if (( $# )); then
    exec "$@"
fi
cd "$game_dir"
exec ./tld.x86_64

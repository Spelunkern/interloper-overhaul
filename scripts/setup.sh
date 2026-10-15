#!/usr/bin/env bash
set -euo pipefail
game_dir="${TLD_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/TheLongDark}"
runtime_dir="${TLD_DOTNET_ROOT:-$HOME/.local/share/tld-modding/dotnet}"
cache_dir="${XDG_CACHE_HOME:-$HOME/.cache}/tld-modding"
[[ -f "$game_dir/tld.x86_64" ]] || { echo "Set TLD_GAME_DIR to the native Linux game installation." >&2; exit 1; }
if pgrep -x tld.x86_64 >/dev/null; then
    echo "Close The Long Dark before setup." >&2
    exit 1
fi
mkdir -p "$cache_dir"
if [[ ! -f "$game_dir/MelonLoader.Bootstrap.so" ]]; then
    archive="$cache_dir/MelonLoader.Linux.x64-0.7.3.zip"
    curl --fail --location --retry 3 https://github.com/LavaGang/MelonLoader/releases/download/v0.7.3/MelonLoader.Linux.x64.zip -o "$archive"
    echo "519f25639cde796a749ea0670daedfa251a45d0217608ac6e12c1bc6aa9e114f  $archive" | sha256sum --check
    unzip -q -n "$archive" -d "$game_dir"
fi
if [[ ! -x "$runtime_dir/dotnet" ]] || ! "$runtime_dir/dotnet" --list-runtimes | grep -q 'Microsoft.NETCore.App 6.0.36 '; then
    curl --fail --location --retry 3 https://dot.net/v1/dotnet-install.sh -o "$cache_dir/dotnet-install.sh"
    bash "$cache_dir/dotnet-install.sh" --version 6.0.36 --runtime dotnet --install-dir "$runtime_dir" --no-path
fi
mkdir -p "$game_dir/Mods" "$game_dir/Plugins" "$game_dir/UserLibs"
echo "Environment ready. Run scripts/deploy.sh, then scripts/launch.sh."

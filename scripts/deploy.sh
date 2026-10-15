#!/usr/bin/env bash
set -euo pipefail
repo_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
game_dir="${TLD_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/TheLongDark}"
if pgrep -x tld.x86_64 >/dev/null || pgrep -x tld.exe >/dev/null; then
    echo "Close The Long Dark before deploying." >&2
    exit 1
fi
"$repo_dir/scripts/build.sh" "-p:GameDir=$game_dir"
mkdir -p "$game_dir/Mods"
install -m 644 "$repo_dir/src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll" "$game_dir/Mods/"
echo "Deployed to $game_dir/Mods/InterloperOverhaul.dll"

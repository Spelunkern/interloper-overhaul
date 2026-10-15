#!/usr/bin/env bash
set -euo pipefail
repo_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
game_dir="${TLD_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/TheLongDark}"
if pgrep -x tld.x86_64 >/dev/null; then
    echo "Close The Long Dark before runtime validation." >&2
    exit 1
fi
mod_file="$game_dir/Mods/InterloperOverhaul.dll"
[[ -f "$mod_file" ]] || { echo "Deploy a release build first." >&2; exit 1; }
backup_dir="$(mktemp -d)"
cp "$mod_file" "$backup_dir/InterloperOverhaul.dll"
restore_mod() {
    cp "$backup_dir/InterloperOverhaul.dll" "$mod_file"
    rm -f "$backup_dir/InterloperOverhaul.dll"
    rmdir "$backup_dir"
}
trap restore_mod EXIT
"$repo_dir/scripts/build.sh" -p:RuntimeValidation=true "-p:GameDir=$game_dir"
install -m 644 "$repo_dir/src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll" "$mod_file"
mkdir -p "$repo_dir/.local"
launch_status=0
timeout --signal=TERM --kill-after=10s 120s "$repo_dir/scripts/launch.sh" > "$repo_dir/.local/runtime-console.log" 2>&1 || launch_status=$?
cp "$game_dir/MelonLoader/Latest.log" "$repo_dir/.local/runtime-validation.log"
if ! grep -q '\[Validation\] SUCCESS:' "$repo_dir/.local/runtime-validation.log" ||
    grep -q '\[ERROR\].*\[Il2CppInterop\]' "$repo_dir/.local/runtime-validation.log"; then
    tail -n 35 "$repo_dir/.local/runtime-validation.log"
    echo "Runtime validation failed (process exit $launch_status)." >&2
    exit 1
fi
grep '\[Validation\]' "$repo_dir/.local/runtime-validation.log"

#!/usr/bin/env bash
set -euo pipefail
repo_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
dotnet build "$repo_dir/src/InterloperOverhaul/InterloperOverhaul.csproj" -c Release "$@"

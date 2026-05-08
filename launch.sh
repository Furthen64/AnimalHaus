#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

SYSTEM_NAMES=(
  "Pigpen"
  "Barn"
  "Tractor"
)

SYSTEM_PROJECTS=(
  "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj"
  "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj"
  "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj"
)

PIDS=()

cleanup() {
  trap - EXIT INT TERM

  if [ "${#PIDS[@]}" -eq 0 ]; then
    return
  fi

  echo ""
  echo "=== Stopping AnimalHaus systems ==="

  for pid in "${PIDS[@]}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" 2>/dev/null || true
    fi
  done

  wait "${PIDS[@]}" 2>/dev/null || true
}

trap cleanup EXIT INT TERM

echo "=== Launching AnimalHaus systems ==="

for i in "${!SYSTEM_PROJECTS[@]}"; do
  name="${SYSTEM_NAMES[$i]}"
  project="${SYSTEM_PROJECTS[$i]}"

  if [ ! -f "$project" ]; then
    echo "ERROR: Required system project is missing: $project"
    exit 1
  fi

  echo "Starting ${name} (${project})"
  dotnet run --project "$project" &
  PIDS+=("$!")
done

echo "All systems started. Press Ctrl+C to stop them."
wait

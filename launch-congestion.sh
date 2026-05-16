#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

SYSTEM_NAMES=(
  "Pigpen"
  "Barn"
  "Tractor"
  "MarketPlace"
)

SYSTEM_PROJECTS=(
  "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj"
  "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj"
  "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj"
  "src/systems/AnimalHaus.MarketPlace/AnimalHaus.MarketPlace.csproj"
)

# Defaults tuned to create heavy intra-system traffic and pub/sub pressure,
# especially between Barn and MarketPlace event flows.
TICK_INTERVAL_MS="${ANIMALHAUS_CONGESTION_TICK_INTERVAL_MS:-25}"
MAX_TICKS="${ANIMALHAUS_CONGESTION_MAX_TICKS:-400}"
STARTUP_DELAY_MS="${ANIMALHAUS_CONGESTION_STARTUP_DELAY_MS:-50}"

PIDS=()
START_TIME=$(date +%s)
SHUTDOWN_REASON="normal"

cleanup() {
  trap - EXIT INT TERM

  if [ "${#PIDS[@]}" -eq 0 ]; then
    return
  fi

  echo ""
  echo "=== Stopping AnimalHaus congestion test systems ==="

  for pid in "${PIDS[@]}"; do
    if kill -0 "$pid" 2>/dev/null; then
      kill "$pid" 2>/dev/null || true
    fi
  done

  wait "${PIDS[@]}" 2>/dev/null || true

  local end_time elapsed_seconds
  end_time=$(date +%s)
  elapsed_seconds=$((end_time - START_TIME))

  echo "=== Congestion run summary ==="
  echo "reason=${SHUTDOWN_REASON}"
  echo "systems=${#SYSTEM_NAMES[@]}"
  echo "tickIntervalMs=${TICK_INTERVAL_MS}, maxTicks=${MAX_TICKS}, startupDelayMs=${STARTUP_DELAY_MS}"
  echo "runtimeSeconds=${elapsed_seconds}"
}

on_interrupt() {
  SHUTDOWN_REASON="interrupt"
  cleanup
}

on_term() {
  SHUTDOWN_REASON="terminated"
  cleanup
}

trap cleanup EXIT
trap on_interrupt INT
trap on_term TERM

echo "=== Launching AnimalHaus congestion profile ==="
echo "tickIntervalMs=${TICK_INTERVAL_MS}, maxTicks=${MAX_TICKS}, startupDelayMs=${STARTUP_DELAY_MS}"

env_overrides=(
  "ANIMALHAUS_TICK_INTERVAL_MS=${TICK_INTERVAL_MS}"
  "ANIMALHAUS_MAX_TICKS=${MAX_TICKS}"
  "ANIMALHAUS_STARTUP_DELAY_MS=${STARTUP_DELAY_MS}"
)

for i in "${!SYSTEM_PROJECTS[@]}"; do
  name="${SYSTEM_NAMES[$i]}"
  project="${SYSTEM_PROJECTS[$i]}"

  if [ ! -f "$project" ]; then
    echo "ERROR: Required system project is missing: $project"
    exit 1
  fi

  echo "Starting ${name} (${project})"
  env "${env_overrides[@]}" dotnet run --project "$project" &
  PIDS+=("$!")
done

echo "All systems started with congestion profile. Press Ctrl+C to stop them."
wait

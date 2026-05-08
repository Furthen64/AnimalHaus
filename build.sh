#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

APP_VERSION="0.1.0"
BUILD_DATE="$(date -u +%Y-%m-%d)"
BUILD_CONFIGURATION="${BUILD_CONFIGURATION:-Release}"

SYSTEM_PROJECTS=(
    "src/systems/AnimalHaus.Pigpen/AnimalHaus.Pigpen.csproj"
    "src/systems/AnimalHaus.Barn/AnimalHaus.Barn.csproj"
    "src/systems/AnimalHaus.Tractor/AnimalHaus.Tractor.csproj"
)

echo "=== Building AnimalHaus ==="
echo "    Version:    ${APP_VERSION}"
echo "    Build date: ${BUILD_DATE}"
echo "    Config:     ${BUILD_CONFIGURATION}"

for project in "${SYSTEM_PROJECTS[@]}"; do
    if [ ! -f "$project" ]; then
        echo "ERROR: Required system project is missing: $project"
        exit 1
    fi
done

dotnet build AnimalHaus.sln -c "${BUILD_CONFIGURATION}"

echo "=== Build complete ==="

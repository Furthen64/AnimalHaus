#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

PROJECT="src/tools/AdministrationApp/AdministrationApp.csproj"

if [ ! -f "$PROJECT" ]; then
  echo "ERROR: AdministrationApp project is missing: $PROJECT"
  exit 1
fi

echo "=== Launching AdministrationApp ==="
dotnet run --project "$PROJECT"

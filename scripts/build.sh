#!/usr/bin/env bash
# Restores and builds the .NET solution.
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet restore "$REPO_ROOT/Ballastlane.Tasks.sln"
dotnet build "$REPO_ROOT/Ballastlane.Tasks.sln" --no-restore --configuration Release

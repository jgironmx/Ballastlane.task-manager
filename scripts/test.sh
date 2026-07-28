#!/usr/bin/env bash
# Restores, builds, and runs all .NET tests.
set -euo pipefail
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

dotnet restore "$REPO_ROOT/Ballastlane.Tasks.sln"
dotnet build "$REPO_ROOT/Ballastlane.Tasks.sln" --no-restore --configuration Release
dotnet test "$REPO_ROOT/Ballastlane.Tasks.sln" --no-build --configuration Release

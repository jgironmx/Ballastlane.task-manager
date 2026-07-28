#!/usr/bin/env pwsh
# Restores, builds, and runs all .NET tests.
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

dotnet restore "$RepoRoot/Ballastlane.Tasks.sln"
dotnet build "$RepoRoot/Ballastlane.Tasks.sln" --no-restore --configuration Release
dotnet test "$RepoRoot/Ballastlane.Tasks.sln" --no-build --configuration Release

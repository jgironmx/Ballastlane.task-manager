#!/usr/bin/env pwsh
# Restores and builds the .NET solution.
$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

dotnet restore "$RepoRoot/Ballastlane.Tasks.sln"
dotnet build "$RepoRoot/Ballastlane.Tasks.sln" --no-restore --configuration Release

#!/bin/bash

# This script will run the build.

echo "Restoring Nuget Packages"
nuget restore "RaspiWatcher.sln"

echo "Building"
msbuild "RaspiWatcher\\RaspiWatcher.csproj" /p:Configuration=Release /p:GeneratePackageOnBuild=false
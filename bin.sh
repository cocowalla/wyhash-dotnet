#!/bin/bash
set -ev

dotnet restore ./wyhash-dotnet.sln --runtime netstandard2.0
dotnet build ./src/WyHash/WyHash.csproj --runtime netstandard2.0 --configuration Release

dotnet test ./test/UnitTests/UnitTests.csproj --framework netcoreapp2.2

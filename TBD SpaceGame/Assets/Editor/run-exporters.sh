#!/bin/sh
DIR="$(dirname "$0")"
cd "$DIR" || exit 1
dotnet run --project Exporters.csproj

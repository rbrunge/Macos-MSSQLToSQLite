#!/bin/zsh
# MSSQLToSQLite CLI Tool Runner for macOS
# Usage: ./run-converter.sh

cd "$(dirname "$0")"

dotnet run --project ClassLibrary/ClassLibrary.csproj -- "$@"


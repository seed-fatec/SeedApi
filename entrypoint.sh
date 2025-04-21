#!/bin/bash
set -e

dotnet ef database update --project SeedApi.csproj --startup-project .
dotnet out/SeedApi.dll
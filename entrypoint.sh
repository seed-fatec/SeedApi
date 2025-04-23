#!/bin/bash

dotnet ef database update \
  --project SeedApi/SeedApi.Infrastructure \
  --startup-project SeedApi/SeedApi.API

dotnet /App/publish/SeedApi.API.dll
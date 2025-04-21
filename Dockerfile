# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App
COPY . ./
RUN dotnet publish SeedApi.sln -c Release -o /App/out -p:EnvironmentName=Production

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS runtime
WORKDIR /App

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

COPY --from=build /App/out ./out
COPY --from=build /App ./

# Copia o script de entrypoint
COPY entrypoint.sh .

RUN chmod +x ./entrypoint.sh

EXPOSE 8080
EXPOSE 443

ENTRYPOINT ["./entrypoint.sh"]

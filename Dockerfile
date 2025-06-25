FROM mcr.microsoft.com/dotnet/sdk:9.0 AS final
WORKDIR /App

COPY SeedApi.sln ./
COPY SeedApi/SeedApi.API/SeedApi.API.csproj ./SeedApi/SeedApi.API/
COPY SeedApi/SeedApi.Application/SeedApi.Application.csproj ./SeedApi/SeedApi.Application/
COPY SeedApi/SeedApi.Domain/SeedApi.Domain.csproj ./SeedApi/SeedApi.Domain/
COPY SeedApi/SeedApi.Infrastructure/SeedApi.Infrastructure.csproj ./SeedApi/SeedApi.Infrastructure/

RUN dotnet restore

COPY . ./

# Instala o dotnet-ef globalmente
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"

RUN dotnet publish SeedApi/SeedApi.API -c Release -o /App/publish

COPY entrypoint.sh .
RUN chmod +x ./entrypoint.sh

# Expondo as portas
EXPOSE 8080
EXPOSE 443

ENTRYPOINT ["./entrypoint.sh"]

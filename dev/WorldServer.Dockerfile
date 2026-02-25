FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./src/server/ .
RUN dotnet publish ./WorldServer/WorldServer.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY ./dev/dbc ./dbc
COPY --from=build /app/bin .
COPY ./dev/configuration.json ./configuration.json
ENTRYPOINT ["./Mangos.WorldServer"]
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./src/server/ .
COPY ./src/Directory.Build.props Directory.Build.props
RUN dotnet publish ./GameServer/GameServer.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY ./dev/dbc ./dbc
COPY --from=build /app/bin .
COPY ./dev/configuration.json ./configuration.json
ENTRYPOINT ["./GameServer"]
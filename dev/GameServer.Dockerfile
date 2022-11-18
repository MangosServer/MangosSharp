FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./Source/ .
RUN dotnet publish ./GameServer/GameServer.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY ./.docker/dbc ./dbc
COPY --from=build /app/bin .
COPY ./.docker/configuration.json ./configuration.json
ENTRYPOINT ["./GameServer"]
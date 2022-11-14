FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY ./Source/ .
RUN dotnet publish ./WorldCluster/WorldCluster.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY ./.docker/dbc ./dbc
COPY ./.docker/configs ./configs
COPY --from=build /app/bin .
EXPOSE 50001
ENTRYPOINT ["./WorldCluster"]
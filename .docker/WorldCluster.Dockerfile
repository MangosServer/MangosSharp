FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app
COPY ./Source/ .
RUN dotnet publish ./Services/WorldCluster/WorldCluster.csproj -c Release -o bin

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build /app/bin .
COPY ./.docker/configs ./configs
COPY ./.docker/dbc ./dbc
EXPOSE 50001
ENTRYPOINT ["./WorldCluster"]
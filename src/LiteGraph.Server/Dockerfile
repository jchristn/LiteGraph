FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything from source directory into /App
COPY . ./
EXPOSE 8701
ENTRYPOINT ["dotnet", "LiteGraph.Server.dll"]

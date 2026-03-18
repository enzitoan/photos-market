# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["PhotosMarket.API.csproj", "./"]
RUN dotnet restore "PhotosMarket.API.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "PhotosMarket.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "PhotosMarket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Create directory for credentials
RUN mkdir -p /app/credentials

ENTRYPOINT ["dotnet", "PhotosMarket.API.dll"]

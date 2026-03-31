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

# Instalar dependencias para ImageSharp y fuentes del sistema
# Necesario para aplicar watermarks en imágenes
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    fontconfig \
    fonts-liberation \
    wget \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Descargar e instalar la fuente Montserrat desde Google Fonts
RUN mkdir -p /usr/share/fonts/truetype/montserrat && \
    wget -q https://github.com/JulietaUla/Montserrat/archive/refs/heads/master.zip -O /tmp/montserrat.zip && \
    unzip -q /tmp/montserrat.zip -d /tmp && \
    cp /tmp/Montserrat-master/fonts/ttf/*.ttf /usr/share/fonts/truetype/montserrat/ && \
    fc-cache -f -v && \
    rm -rf /tmp/montserrat.zip /tmp/Montserrat-master

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files
COPY --from=publish /app/publish .

# Create directory for credentials
RUN mkdir -p /app/credentials

ENTRYPOINT ["dotnet", "PhotosMarket.API.dll"]

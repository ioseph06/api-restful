# ETAPA 1: Base (Runtime ligero para ejecutar la app)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# ETAPA 2: Build (SDK pesado para compilar)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar solo el archivo del proyecto primero (optimiza la caché de Docker)
COPY ["MiPrimeraApi.csproj", "./"]
RUN dotnet restore "./MiPrimeraApi.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet build "./MiPrimeraApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# ETAPA 3: Publish (Publicar la app para producción)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MiPrimeraApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ETAPA 4: Final (Combinar la base ligera con la app publicada)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Crear carpeta para los logs de Serilog (evita errores de permisos)
RUN mkdir -p /app/logs 

ENTRYPOINT ["dotnet", "MiPrimeraApi.dll"]
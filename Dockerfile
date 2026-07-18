# syntax=docker/dockerfile:1

# ---------------------------------------------------------------------------
# Etapa "build": compila y publica el proyecto Api usando el SDK completo.
# Esta etapa nunca llega a la imagen final.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos primero SOLO los .csproj, respetando su ruta relativa real,
# para que "dotnet restore" quede cacheado mientras no cambien las
# dependencias (NuGet ni ProjectReference), sin importar cuánto código
# .cs cambie después.
COPY src/AuthService_GR.Domain/AuthService_GR.Domain.csproj src/AuthService_GR.Domain/
COPY src/AuthService_GR.Application/AuthService_GR.Application.csproj src/AuthService_GR.Application/
COPY src/AuthService_GR.Persistence/AuthService_GR.Persistence.csproj src/AuthService_GR.Persistence/
COPY src/AuthService_GR.Api/AuthService_GR.Api.csproj src/AuthService_GR.Api/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/AuthService_GR.Api/AuthService_GR.Api.csproj

# Ahora sí, el resto del código fuente.
COPY src/ src/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish src/AuthService_GR.Api/AuthService_GR.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ---------------------------------------------------------------------------
# Etapa "development": usada por docker-compose en desarrollo. Reutiliza el
# SDK completo (lo necesitamos: "dotnet watch" recompila en cada cambio) y
# solo restaura paquetes -- el código .cs llega por bind mount (Fase 7),
# igual que node_modules no se copiaba en el target "development" de Node.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /src

COPY src/AuthService_GR.Domain/AuthService_GR.Domain.csproj src/AuthService_GR.Domain/
COPY src/AuthService_GR.Application/AuthService_GR.Application.csproj src/AuthService_GR.Application/
COPY src/AuthService_GR.Persistence/AuthService_GR.Persistence.csproj src/AuthService_GR.Persistence/
COPY src/AuthService_GR.Api/AuthService_GR.Api.csproj src/AuthService_GR.Api/

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/AuthService_GR.Api/AuthService_GR.Api.csproj

WORKDIR /src/src/AuthService_GR.Api

EXPOSE 5105
EXPOSE 7030

# "dotnet watch" vigila los .cs (llegados por bind mount) y recompila +
# relanza automáticamente en cada guardado, igual que nodemon en Node.
CMD ["dotnet", "watch", "run", "--no-launch-profile"]

# ---------------------------------------------------------------------------
# Etapa "production": solo el runtime de ASP.NET + los binarios publicados.
# Se llama igual que en Node/Frontend a propósito, para que
# "target: production" sea consistente en los tres Dockerfiles.
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app

# curl: la imagen base "aspnet" no lo trae por defecto. Lo necesitamos
# para que el HEALTHCHECK definido en docker-compose (Fase 7) pueda
# preguntarle al propio contenedor "¿estás vivo?" desde dentro.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Usuario de sistema sin privilegios para ejecutar la app.
RUN useradd --create-home --shell /bin/false authservice

COPY --from=build /app/publish .

# Data Protection ("./keys", ver SecurityExtensions.cs) y Serilog ("./logs")
# escriben relativo al ContentRoot (/app). Se crean aquí con el dueño
# correcto para que, cuando en la Fase 7 se monten como volúmenes,
# el usuario no-root pueda escribir en ellos sin fallar por permisos.
RUN mkdir -p /app/keys /app/logs && chown -R authservice:authservice /app

USER authservice

# Documentan los puertos HTTP y HTTPS de launchSettings.json.
# El binding real (ASPNETCORE_URLS) se define en Compose (Fase 6/7),
# no aquí, para que la misma imagen sirva con o sin certificado montado.
EXPOSE 5105
EXPOSE 7030

ENTRYPOINT ["dotnet", "AuthService_GR.Api.dll"]

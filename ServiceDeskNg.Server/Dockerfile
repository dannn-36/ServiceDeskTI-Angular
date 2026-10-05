# API de ServiceDesk TI (ASP.NET Core 9).
# La interfaz Angular se construye en su propia imagen (servicedeskng.client/Dockerfile).

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restaurar primero solo con el .csproj aprovecha la caché de capas de Docker.
COPY ServiceDeskNg.Server.csproj ./
RUN dotnet restore -p:SinFrontend=true

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore -p:SinFrontend=true -p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Cliente de MySQL (mysqldump / mysql) para el respaldo y la restauración desde el panel.
RUN apt-get update \
 && apt-get install -y --no-install-recommends default-mysql-client \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Carpeta para las claves que cifran la cookie de sesión (se monta como volumen).
# wwwroot queda vacía: en Docker la interfaz la sirve nginx desde su propio contenedor.
RUN mkdir -p /app/claves /app/wwwroot && chown "$APP_UID" /app/claves

# Se ejecuta con el usuario sin privilegios que trae la imagen oficial, no como root.
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceDeskNg.Server.dll"]

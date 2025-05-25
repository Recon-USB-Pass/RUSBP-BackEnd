###############################  BUILD  ################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIG=Release
WORKDIR /src

# Copiamos todos los *.csproj para mejor cacheo
COPY *.csproj ./
RUN dotnet restore

# Copiamos el resto del código y publicamos
COPY . .
RUN dotnet publish Backend_Sistema_Central.csproj \
    -c ${CONFIG} -o /app/publish /p:UseAppHost=false

###############################  RUNTIME  ###############################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080 8443

# psql para health-check en entrypoint
RUN apt-get update && \
    apt-get install -y --no-install-recommends postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Copiamos binarios, certificados y script de arranque
COPY --from=build /app/publish .
COPY certs/ certs/
COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# URLs Kestrel
ENV ASPNETCORE_URLS=https://+:8443;http://+:8080
ENTRYPOINT ["./entrypoint.sh"]

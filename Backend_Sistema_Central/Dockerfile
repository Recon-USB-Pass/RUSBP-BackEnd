###############################  BUILD  ################################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG CONFIG=Release
WORKDIR /src

# Copiamos proyecto y restauramos dependencias
COPY Backend_Sistema_Central.csproj ./
RUN dotnet restore

# Copiamos todo y publicamos
COPY . .
RUN dotnet publish Backend_Sistema_Central.csproj \
    -c ${CONFIG} -o /app/publish /p:UseAppHost=false

###############################  RUNTIME  ###############################
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080 8443

# Cliente psql para entrypoint
RUN apt-get update && \
    apt-get install -y --no-install-recommends postgresql-client && \
    rm -rf /var/lib/apt/lists/*

# Copiamos artefactos y certificados
COPY --from=build /app/publish .          
COPY certs/ certs/                        
COPY entrypoint.sh .                      
RUN chmod +x entrypoint.sh

# URLs = HTTPS 8443 + HTTP 8080
ENV ASPNETCORE_URLS=https://+:8443;http://+:8080
ENTRYPOINT ["./entrypoint.sh"]

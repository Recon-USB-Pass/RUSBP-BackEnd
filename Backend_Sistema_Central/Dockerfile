# -------- base (VS fast mode) --------
    FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
    ARG APP_UID=0
    USER ${APP_UID}
    WORKDIR /app
    EXPOSE 8080 8081 5001
    
    # -------- build ----------------------
    FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
    ARG BUILD_CONFIGURATION=Release
    WORKDIR /src
    COPY ["Backend_Sistema_Central.csproj", "./"]
    RUN dotnet restore
    COPY . .
    RUN dotnet publish Backend_Sistema_Central.csproj -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false
    
    # -------- final ----------------------
    FROM base AS final
    WORKDIR /app
    RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client && rm -rf /var/lib/apt/lists/*
    COPY --from=build /app/publish .
    COPY certs/ ./certs/
    COPY entrypoint.sh .
    RUN chmod +x entrypoint.sh
    ENV ASPNETCORE_URLS=https://+:5001
    ENTRYPOINT ["./entrypoint.sh"]
    
# ── Build ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia os .csproj e restaura dependências (cache de layer)
COPY Login/Login.Domain/Login.Domain.csproj             Login/Login.Domain/
COPY Login/Login.Application/Login.Application.csproj   Login/Login.Application/
COPY Login/Login.Infrastructure/Login.Infrastructure.csproj Login/Login.Infrastructure/
COPY Login/Login/Login.csproj                           Login/Login/

RUN dotnet restore Login/Login/Login.csproj

# Copia o resto e publica
COPY Login/Login.Domain/        Login/Login.Domain/
COPY Login/Login.Application/   Login/Login.Application/
COPY Login/Login.Infrastructure/ Login/Login.Infrastructure/
COPY Login/Login/               Login/Login/

RUN dotnet publish Login/Login/Login.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Login.dll"]

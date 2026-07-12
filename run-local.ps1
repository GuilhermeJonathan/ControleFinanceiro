# Sobe o ambiente local completo do FinDog:
#   Postgres (Docker) + API principal (5241) + API Login (5290) + Expo web (8081)
# Uso: .\run-local.ps1
# Ctrl+C encerra o Expo; as APIs ficam em janelas prÃ³prias.

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# â”€â”€ 1. Postgres local â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
$pg = docker ps -a --filter "name=findog-postgres" --format "{{.Status}}"
if (-not $pg) {
    Write-Host "Criando container findog-postgres..." -ForegroundColor Cyan
    docker run -d --name findog-postgres -e POSTGRES_PASSWORD=findog_local -p 5433:5432 postgres:16 | Out-Null
    Start-Sleep -Seconds 5
    docker exec findog-postgres psql -U postgres -c "CREATE DATABASE findog;" | Out-Null
} elseif ($pg -notlike "Up*") {
    Write-Host "Iniciando container findog-postgres..." -ForegroundColor Cyan
    docker start findog-postgres | Out-Null
    Start-Sleep -Seconds 3
}
Write-Host "Postgres OK (localhost:5433)" -ForegroundColor Green

# â”€â”€ 2. APIs (janelas separadas; migrations aplicam sozinhas no startup) â”€â”€â”€â”€â”€
$mainConn  = "Host=localhost;Port=5433;Database=findog;Username=postgres;Password=findog_local"
$loginConn = "Host=localhost;Port=5433;Database=findog;Username=postgres;Password=findog_local"

Start-Process powershell -ArgumentList "-NoExit", "-Command",
    "`$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ConnectionStrings__DefaultConnection='$mainConn'; cd '$root\ControleFinanceiro.Api'; dotnet run"
Start-Process powershell -ArgumentList "-NoExit", "-Command",
    "`$env:ASPNETCORE_ENVIRONMENT='Development'; `$env:ConnectionStrings__DefaultConnection='$loginConn'; cd '$root\Login\Login'; dotnet run"

Write-Host "API principal -> http://localhost:5241" -ForegroundColor Green
Write-Host "API Login     -> http://localhost:5290" -ForegroundColor Green

# â”€â”€ 3. Frontend web â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Host "Subindo Expo web em http://localhost:8081 ..." -ForegroundColor Cyan
Set-Location "$root\mobile"
npm run web

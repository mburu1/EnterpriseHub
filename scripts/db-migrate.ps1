<#
.SYNOPSIS
    Applies EF Core migrations to the relational stores that have them (MSSQL, PostgreSQL).

.DESCRIPTION
    MySQL is intentionally excluded — see docs/adr/001-database-strategy.md's note on Pomelo's
    EF Core 10 incompatibility for design-time tooling. Run docker-compose up first so the
    target databases are reachable.
#>
param(
    [string]$BackendPath = (Join-Path $PSScriptRoot "..\backend")
)

$ErrorActionPreference = "Stop"

$contexts = @("MssqlDbContext", "PostgresDbContext")

foreach ($context in $contexts) {
    Write-Host "Applying migrations for $context..." -ForegroundColor Cyan
    dotnet ef database update `
        --project (Join-Path $BackendPath "src\EnterpriseHub.Infrastructure") `
        --startup-project (Join-Path $BackendPath "src\EnterpriseHub.API") `
        --context $context

    if ($LASTEXITCODE -ne 0) {
        throw "Migration failed for $context"
    }
}

Write-Host "All migrations applied." -ForegroundColor Green

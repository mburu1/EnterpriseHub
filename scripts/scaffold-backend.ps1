<#
.SYNOPSIS
    Recreates the backend .slnx solution and project layout from scratch.

.DESCRIPTION
    Documents exactly how backend/ was scaffolded (see docs/sdlc.md). Safe to re-read as a
    reference even though the projects already exist in this repo — it exits early if it finds
    an existing solution file rather than overwriting your work.
#>
param(
    [string]$BackendPath = (Join-Path $PSScriptRoot "..\backend")
)

$ErrorActionPreference = "Stop"

if (Test-Path (Join-Path $BackendPath "EnterpriseHub.slnx")) {
    Write-Host "backend/EnterpriseHub.slnx already exists — nothing to scaffold. Delete it first if you really want to start over." -ForegroundColor Yellow
    exit 0
}

New-Item -ItemType Directory -Force -Path $BackendPath | Out-Null
Push-Location $BackendPath
try {
    dotnet new sln -n EnterpriseHub -f slnx

    dotnet new classlib -n EnterpriseHub.Domain -o src/EnterpriseHub.Domain --force
    dotnet new classlib -n EnterpriseHub.Application -o src/EnterpriseHub.Application --force
    dotnet new classlib -n EnterpriseHub.Infrastructure -o src/EnterpriseHub.Infrastructure --force
    dotnet new webapi -n EnterpriseHub.API -o src/EnterpriseHub.API -controllers --force

    dotnet new xunit3 -n EnterpriseHub.Tests.Unit -o src/EnterpriseHub.Tests/Unit --force
    dotnet new xunit3 -n EnterpriseHub.Tests.Integration -o src/EnterpriseHub.Tests/Integration --force
    dotnet new xunit3 -n EnterpriseHub.Tests.E2E -o src/EnterpriseHub.Tests/E2E --force

    dotnet sln add src/EnterpriseHub.Domain/EnterpriseHub.Domain.csproj
    dotnet sln add src/EnterpriseHub.Application/EnterpriseHub.Application.csproj
    dotnet sln add src/EnterpriseHub.Infrastructure/EnterpriseHub.Infrastructure.csproj
    dotnet sln add src/EnterpriseHub.API/EnterpriseHub.API.csproj
    dotnet sln add src/EnterpriseHub.Tests/Unit/EnterpriseHub.Tests.Unit.csproj
    dotnet sln add src/EnterpriseHub.Tests/Integration/EnterpriseHub.Tests.Integration.csproj
    dotnet sln add src/EnterpriseHub.Tests/E2E/EnterpriseHub.Tests.E2E.csproj

    dotnet add src/EnterpriseHub.Application reference src/EnterpriseHub.Domain
    dotnet add src/EnterpriseHub.Infrastructure reference src/EnterpriseHub.Domain src/EnterpriseHub.Application
    dotnet add src/EnterpriseHub.API reference src/EnterpriseHub.Domain src/EnterpriseHub.Application src/EnterpriseHub.Infrastructure
    dotnet add src/EnterpriseHub.Tests/Unit reference src/EnterpriseHub.Domain src/EnterpriseHub.Application
    dotnet add src/EnterpriseHub.Tests/Integration reference src/EnterpriseHub.Domain src/EnterpriseHub.Application src/EnterpriseHub.Infrastructure
    dotnet add src/EnterpriseHub.Tests/E2E reference src/EnterpriseHub.API

    Write-Host "Backend scaffolded. NuGet packages and application code are not reproduced by this script — see git history / docs/sdlc.md." -ForegroundColor Green
}
finally {
    Pop-Location
}

<#
.SYNOPSIS
    Seeds a local API instance with a few sample tenants/users via the public register endpoint.

.DESCRIPTION
    Talks to a running API over HTTP — no direct DB access — so it exercises the same code path
    a real signup would. Run after `docker compose up -d` and `dotnet run` (or the containerized
    api service is up) and reachable at -BaseUrl.
#>
param(
    [string]$BaseUrl = "http://localhost:5001"
)

$ErrorActionPreference = "Stop"

$seedUsers = @(
    @{ organizationName = "Acme Inc";     email = "owner@acme.dev";     password = "Password1"; firstName = "Ada";   lastName = "Lovelace" }
    @{ organizationName = "Globex Corp";  email = "owner@globex.dev";   password = "Password1"; firstName = "Grace"; lastName = "Hopper" }
    @{ organizationName = "Initech";      email = "owner@initech.dev";  password = "Password1"; firstName = "Alan";  lastName = "Turing" }
)

foreach ($user in $seedUsers) {
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/auth/register" -Method Post -Body ($user | ConvertTo-Json) -ContentType "application/json"
        Write-Host "Seeded $($user.organizationName) ($($user.email))" -ForegroundColor Green
    }
    catch {
        Write-Warning "Skipping $($user.email): $($_.Exception.Message)"
    }
}

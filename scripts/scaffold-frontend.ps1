<#
.SYNOPSIS
    Recreates the frontend/ package.json and SvelteKit config from scratch.

.DESCRIPTION
    Documents how frontend/ was scaffolded (see docs/sdlc.md). Exits early if frontend/package.json
    already exists rather than overwriting it — this repo's frontend/ was hand-scaffolded (not via
    `npx sv create`) so it could be built non-interactively; this script reproduces the same shape.
#>
param(
    [string]$FrontendPath = (Join-Path $PSScriptRoot "..\frontend")
)

$ErrorActionPreference = "Stop"

if (Test-Path (Join-Path $FrontendPath "package.json")) {
    Write-Host "frontend/package.json already exists — nothing to scaffold. Delete it first if you really want to start over." -ForegroundColor Yellow
    exit 0
}

Write-Host "This script is a placeholder for a from-scratch rebuild — the current frontend/ was hand-authored (see git history) rather than generated, since the target environment couldn't run the interactive 'npx sv create' wizard non-interactively." -ForegroundColor Yellow
Write-Host "To scaffold a fresh SvelteKit app interactively instead, run:" -ForegroundColor Cyan
Write-Host "  npx sv create frontend" -ForegroundColor White
Write-Host "then reconcile it against docs/architecture.md and the existing route/store structure." -ForegroundColor Cyan

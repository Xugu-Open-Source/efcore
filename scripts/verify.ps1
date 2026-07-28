param(
    [string]$Module = "All",
    [switch]$RunTests
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path (Join-Path $PSScriptRoot "..")

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $dotnetFallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $dotnetFallback) {
        $env:PATH = "C:\Program Files\dotnet;$env:PATH"
    }
}

Write-Host "=== XuguDB EF Core Provider Verify ===" -ForegroundColor Cyan
Write-Host "Root: $Root"

$required = @(
    "docs\contracts\sql-dialect.contract.md",
    "docs\contracts\stub-and-exclusion.contract.md",
    "docs\LIMITATIONS.md",
    "docs\RELEASE-SCOPE.md",
    "docs\references\xugudb-docs-map.md",
    ".trellis\workflow.md"
)
foreach ($f in $required) {
    $path = Join-Path $Root $f
    if (-not (Test-Path $path)) {
        throw "Missing required file: $f"
    }
}
Write-Host "[OK] Contract/reference/Trellis files present" -ForegroundColor Green

& (Join-Path $PSScriptRoot "verify-source-lineage.ps1")
if ($LASTEXITCODE -ne 0) { throw "Source lineage verification failed" }

$pomeloPath = Join-Path $Root "external\Pomelo.EntityFrameworkCore.MySql\src\EFCore.MySql"
if (-not (Test-Path $pomeloPath)) {
    Write-Warning "[WARN] Pomelo reference not cloned"
} else {
    Write-Host "[OK] Pomelo reference present" -ForegroundColor Green
}

$driverPath = Join-Path $Root "external\csharp-driver\XGCSClient\XGCSClient.csproj"
if (-not (Test-Path $driverPath)) {
    Write-Warning "[WARN] csharp-driver not cloned"
} else {
    Write-Host "[OK] csharp-driver present" -ForegroundColor Green
}

$docsPath = "E:\BaiduSyncdisk\docs\content\ecosystem\orm\dotnet\efcore.md"
if (-not (Test-Path $docsPath)) {
    Write-Warning "[WARN] XuguDB docs not found"
} else {
    Write-Host "[OK] XuguDB docs accessible" -ForegroundColor Green
}

$sln = Get-ChildItem -Path $Root -Filter "*.sln" -ErrorAction SilentlyContinue | Select-Object -First 1
if ($null -ne $sln) {
    Write-Host "[OK] Solution found: $($sln.Name)" -ForegroundColor Green
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($null -eq $dotnet) {
        Write-Warning "[WARN] dotnet CLI not found in PATH; skip build"
    } else {
        Write-Host "Building $($sln.Name)..." -ForegroundColor Yellow
        dotnet build $sln.FullName -c Release
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        Write-Host "[OK] Build succeeded" -ForegroundColor Green

        if ($RunTests) {
            Write-Host "Running L1 Unit gate..." -ForegroundColor Yellow
            & (Join-Path $PSScriptRoot "run-unit-gate.ps1") -Configuration Release
            if ($LASTEXITCODE -ne 0) { throw "L1 Unit gate failed" }

            Write-Host "Running L2 native then compat Integration gates..." -ForegroundColor Yellow
            & (Join-Path $PSScriptRoot "run-native-gate.ps1") -Configuration Release -MaxAttempts 3 -NoBuild
            if ($LASTEXITCODE -ne 0) { throw "L2 native gate failed" }
            & (Join-Path $PSScriptRoot "run-compat-gate.ps1") -Configuration Release -MaxAttempts 3 -NoBuild
            if ($LASTEXITCODE -ne 0) { throw "L2 compat gate failed" }
            Write-Host "[OK] L1 + L2 gates passed" -ForegroundColor Green
        }
    }
} else {
    Write-Host "[SKIP] No .sln yet (Phase 0)" -ForegroundColor Yellow
}

Write-Host "=== Verify complete ===" -ForegroundColor Cyan

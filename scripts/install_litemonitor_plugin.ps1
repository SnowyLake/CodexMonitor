param(
    [Parameter(Mandatory = $true)]
    [string]$LiteMonitorDir
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$source = Join-Path $repoRoot "litemonitor\CodexUsage.json"
$targetDir = Join-Path $LiteMonitorDir "resources\plugins"
$target = Join-Path $targetDir "CodexUsage.json"

if (-not (Test-Path $source)) {
    throw "Plugin file not found: $source"
}

if (-not (Test-Path $targetDir)) {
    throw "LiteMonitor plugin directory not found: $targetDir"
}

Copy-Item -LiteralPath $source -Destination $target -Force
Write-Host "Installed LiteMonitor plugin: $target"

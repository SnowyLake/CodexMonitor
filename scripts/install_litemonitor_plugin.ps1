param(
    [Parameter(Mandatory = $true)]
    [string]$LiteMonitorDir,
    [switch]$NoPause
)

$ErrorActionPreference = "Stop"

function Wait-BeforeExit {
    if (-not $NoPause) {
        Read-Host "Press Enter to exit" | Out-Null
    }
}

try {
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
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Wait-BeforeExit
}

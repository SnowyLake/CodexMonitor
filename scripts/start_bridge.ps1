param(
    [string]$Python = "python",
    [int]$Port = 17890,
    [string]$CodexDir = "",
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
    $scriptPath = Join-Path $repoRoot "src\codex_usage_bridge.py"

    $arguments = @($scriptPath, "--port", $Port)
    if ($CodexDir -ne "") {
        $arguments += @("--codex-dir", $CodexDir)
    }

    & $Python @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Bridge exited with code $LASTEXITCODE"
    }
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Wait-BeforeExit
}

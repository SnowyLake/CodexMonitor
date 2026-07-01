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

function Resolve-Python {
    if ($Python -ne "python") {
        return $Python
    }

    $bundledPython = Join-Path $env:USERPROFILE ".cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"
    if (Test-Path $bundledPython) {
        return $bundledPython
    }

    return $Python
}

try {
    $repoRoot = Split-Path -Parent $PSScriptRoot
    $scriptPath = Join-Path $repoRoot "src\codex_usage_bridge.py"
    $pythonPath = Resolve-Python

    $arguments = @($scriptPath, "--port", $Port)
    if ($CodexDir -ne "") {
        $arguments += @("--codex-dir", $CodexDir)
    }

    & $pythonPath @arguments
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

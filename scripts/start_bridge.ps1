param(
    [string]$Python = "python",
    [int]$Port = 17890,
    [string]$CodexDir = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$scriptPath = Join-Path $repoRoot "src\codex_usage_bridge.py"

$arguments = @($scriptPath, "--port", $Port)
if ($CodexDir -ne "") {
    $arguments += @("--codex-dir", $CodexDir)
}

& $Python @arguments

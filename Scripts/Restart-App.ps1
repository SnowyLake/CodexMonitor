param(
    [switch]$NoPause
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$publishDir = Join-Path $repoRoot "Builds\Output\win-x64"
$appProcessNames = @("CodexTray", "CodexTray.App")
$appPath = Join-Path $publishDir "CodexTray.exe"

function Stop-RunningApp {
    $processes = foreach ($appProcessName in $appProcessNames) {
        Get-Process -Name $appProcessName -ErrorAction SilentlyContinue
    }
    if (-not $processes) {
        Write-Host "No running CodexTray process found."
        return
    }

    foreach ($process in $processes) {
        Write-Host "Stopping process $($process.Id): $($process.Path)"
        try {
            Stop-Process -Id $process.Id -Force -ErrorAction Stop
            Wait-Process -Id $process.Id -Timeout 10 -ErrorAction SilentlyContinue
            if (Get-Process -Id $process.Id -ErrorAction SilentlyContinue) {
                Write-Host "Process $($process.Id) is still running after timeout."
            }
            else {
                Write-Host "Process $($process.Id) stopped."
            }
        }
        catch {
            Write-Host "Process $($process.Id) could not be stopped: $($_.Exception.Message)"
        }
    }
}

function Start-PublishedApp {
    $process = Start-Process -FilePath $appPath -WindowStyle Hidden -PassThru
    Write-Host ""
    Write-Host "Started CodexTray."
    Write-Host "Process: $($process.Id)"
    Write-Host "Path:    $appPath"
}

$exitCode = 0
try {
    if (-not (Test-Path -LiteralPath $appPath)) {
        throw "Published executable not found: $appPath"
    }

    Stop-RunningApp
    Start-PublishedApp
}
catch {
    Write-Host ""
    Write-Host "Restart failed."
    Write-Host $_.Exception.Message
    $exitCode = 1
}
finally {
    if (-not $NoPause) {
        Write-Host ""
        Write-Host "Press any key to close this window..."
        [Console]::ReadKey($true) | Out-Null
    }
}

exit $exitCode

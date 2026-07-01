param(
    [string]$TaskName = "CodexUsageLiteMonitorBridge",
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
    $startScript = Join-Path $repoRoot "scripts\start_bridge.ps1"

    if (-not (Test-Path $startScript)) {
        throw "Start script not found: $startScript"
    }

    $argumentParts = @(
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File", "`"$startScript`"",
        "-Python", "`"$Python`"",
        "-Port", "$Port",
        "-NoPause"
    )

    if ($CodexDir -ne "") {
        $argumentParts += @("-CodexDir", "`"$CodexDir`"")
    }

    $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument ($argumentParts -join " ")
    $trigger = New-ScheduledTaskTrigger -AtLogOn
    $principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel LeastPrivilege
    $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -ExecutionTimeLimit (New-TimeSpan -Hours 0)

    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
    Write-Host "Installed startup task: $TaskName"
    Write-Host "Bridge URL: http://127.0.0.1:$Port/codex-usage"
}
catch {
    Write-Error $_
    exit 1
}
finally {
    Wait-BeforeExit
}

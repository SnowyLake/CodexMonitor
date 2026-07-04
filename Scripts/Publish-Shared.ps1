$CodexMonitorRuntime = "win-x64"
$CodexMonitorTargetFramework = "net9.0-windows"

function Invoke-CodexMonitorPublish {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepoRoot,

        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$OutputPath,

        [string]$Title = "CodexMonitor publish started.",

        [switch]$Clean
    )

    Write-Host $Title
    Write-Host "Project: $ProjectPath"
    Write-Host "Output:  $OutputPath"
    if ($Clean -and (Test-Path -LiteralPath $OutputPath)) {
        Remove-PathWithRetry $OutputPath
        Write-Host "Removed previous publish directory."
    }

    Push-Location $RepoRoot
    try {
        dotnet publish $ProjectPath -c Release -f $CodexMonitorTargetFramework -r $CodexMonitorRuntime -p:PublishSingleFile=true -p:SelfContained=false -o $OutputPath
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed with exit code $LASTEXITCODE."
        }

        Remove-PublishDebugSymbols $OutputPath
    }
    finally {
        Pop-Location
    }
}

function Remove-PublishDebugSymbols {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    Get-ChildItem -LiteralPath $Path -Filter "*.pdb" -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}

function Remove-PathWithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    for ($attempt = 1; $attempt -le 10; $attempt++) {
        try {
            Remove-Item -LiteralPath $Path -Recurse -Force -ErrorAction Stop
            return
        }
        catch {
            if ($attempt -eq 10) {
                throw
            }

            Start-Sleep -Milliseconds 500
        }
    }
}

function Wait-BeforeExit {
    param(
        [switch]$NoPause
    )

    if ($NoPause) {
        return
    }

    Write-Host ""
    Write-Host "Press any key to close this window..."
    [Console]::ReadKey($true) | Out-Null
}

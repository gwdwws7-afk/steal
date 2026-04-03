[CmdletBinding()]
param(
    [string]$ProjectPath = $env:UNITY_PROJECT_PATH,
    [string]$UnityExe = $env:UNITY_EXE,
    [ValidateSet("EditMode", "PlayMode", "editmode", "playmode")]
    [string]$TestPlatform = $(if ([string]::IsNullOrWhiteSpace($env:UNITY_TEST_PLATFORM)) { "EditMode" } else { $env:UNITY_TEST_PLATFORM }),
    [string]$ResultsFile,
    [string]$LogFile,
    [switch]$AllowZeroTests
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Resolve-AbsolutePath {
    param(
        [Parameter(Mandatory = $true)][string]$PathValue,
        [Parameter(Mandatory = $true)][string]$Description
    )

    if (-not (Test-Path -LiteralPath $PathValue)) {
        throw "$Description does not exist: $PathValue"
    }

    return (Resolve-Path -LiteralPath $PathValue).Path
}

function Resolve-ProjectRoot {
    param([string]$Candidate)

    if (-not [string]::IsNullOrWhiteSpace($Candidate)) {
        return Resolve-AbsolutePath -PathValue $Candidate -Description "Project path"
    }

    if (-not [string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        return Resolve-AbsolutePath -PathValue $PSScriptRoot -Description "Script root"
    }

    return Resolve-AbsolutePath -PathValue (Get-Location).Path -Description "Current directory"
}

function Normalize-TestPlatform {
    param([string]$Platform)

    switch ($Platform.ToLowerInvariant()) {
        "editmode" { return "EditMode" }
        "playmode" { return "PlayMode" }
        default { throw "Unsupported test platform: $Platform" }
    }
}

function Resolve-UnityExecutable {
    param([string]$CandidatePath)

    $candidates = New-Object System.Collections.Generic.List[string]

    if (-not [string]::IsNullOrWhiteSpace($CandidatePath)) {
        $candidates.Add($CandidatePath.Trim())
    }

    foreach ($envName in @("UNITY_EDITOR_PATH", "UNITY_HUB_EDITOR_PATH", "UNITY_PATH")) {
        $value = [Environment]::GetEnvironmentVariable($envName)
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $candidates.Add($value.Trim())
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:ProgramFiles)) {
        $hubRoot = Join-Path $env:ProgramFiles "Unity\Hub\Editor"
        if (Test-Path -LiteralPath $hubRoot) {
            $editorDirs = Get-ChildItem -LiteralPath $hubRoot -Directory -ErrorAction SilentlyContinue |
                Sort-Object -Property Name -Descending
            foreach ($dir in $editorDirs) {
                $candidates.Add((Join-Path $dir.FullName "Editor\Unity.exe"))
            }
        }
    }

    $candidates.Add("C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe")

    foreach ($path in ($candidates | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)) {
        if (Test-Path -LiteralPath $path) {
            return (Resolve-Path -LiteralPath $path).Path
        }
    }

    $hint = @(
        "Unable to locate Unity.exe.",
        "Provide -UnityExe, or set UNITY_EXE / UNITY_EDITOR_PATH.",
        "Checked candidates under C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe."
    ) -join " "
    throw $hint
}

function Ensure-ParentDirectory {
    param([string]$FilePath)

    $parent = Split-Path -Path $FilePath -Parent
    if (-not [string]::IsNullOrWhiteSpace($parent) -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
}

function Read-IntAttribute {
    param(
        [Parameter(Mandatory = $true)][System.Xml.XmlElement]$Node,
        [Parameter(Mandatory = $true)][string]$Name
    )

    $raw = $Node.GetAttribute($Name)
    if ([string]::IsNullOrWhiteSpace($raw)) {
        return 0
    }
    return [int]$raw
}

Write-Host "======================================"
Write-Host "INTIFALL Regression Tests"
Write-Host "======================================"
Write-Host ""

$projectRoot = Resolve-ProjectRoot -Candidate $ProjectPath
$platform = Normalize-TestPlatform -Platform $TestPlatform
$platformToken = $platform.ToLowerInvariant()

if ([string]::IsNullOrWhiteSpace($ResultsFile)) {
    $ResultsFile = Join-Path $projectRoot "test-results-$platformToken.xml"
}
if ([string]::IsNullOrWhiteSpace($LogFile)) {
    $LogFile = Join-Path $projectRoot "test-$platformToken.log"
}

Ensure-ParentDirectory -FilePath $ResultsFile
Ensure-ParentDirectory -FilePath $LogFile

$unityExeResolved = Resolve-UnityExecutable -CandidatePath $UnityExe

Write-Host "ProjectPath : $projectRoot"
Write-Host "UnityExe    : $unityExeResolved"
Write-Host "Platform    : $platform"
Write-Host "ResultsFile : $ResultsFile"
Write-Host "LogFile     : $LogFile"
Write-Host ""

$startTime = Get-Date
Write-Host "[1/2] Running Unity tests..."

$argList = @(
    "-batchmode",
    "-nographics",
    "-projectPath", $projectRoot,
    "-runTests",
    "-testResults", $ResultsFile,
    "-testPlatform", $platform,
    "-logFile", $LogFile
)

$process = Start-Process -FilePath $unityExeResolved -ArgumentList $argList -NoNewWindow -Wait -PassThru
$exitCode = $process.ExitCode

$unityBatchFailed = $false
if (Test-Path -LiteralPath $LogFile) {
    $logContent = Get-Content -LiteralPath $LogFile -Raw
    if ($logContent -match "Aborting batchmode due to failure") {
        $unityBatchFailed = $true
    }
}

if (-not (Test-Path -LiteralPath $ResultsFile)) {
    Write-Host "[FAIL] Missing Unity test results file: $ResultsFile" -ForegroundColor Red
    if ($unityBatchFailed) {
        Write-Host "[FAIL] Unity aborted in batch mode. See log: $LogFile" -ForegroundColor Red
    }
    exit 1
}

Write-Host "[2/2] Parsing test results..."
$xml = [xml](Get-Content -LiteralPath $ResultsFile -Raw)
$testRun = $xml.SelectSingleNode("/test-run")

if ($null -eq $testRun) {
    Write-Host "[FAIL] Invalid NUnit XML: missing /test-run node." -ForegroundColor Red
    exit 1
}

$total = Read-IntAttribute -Node $testRun -Name "total"
$passed = Read-IntAttribute -Node $testRun -Name "passed"
$failed = Read-IntAttribute -Node $testRun -Name "failed"
$durationRaw = $testRun.GetAttribute("duration")
$duration = if ([string]::IsNullOrWhiteSpace($durationRaw)) { 0.0 } else { [math]::Round([double]$durationRaw, 2) }

Write-Host ""
Write-Host "======================================"
Write-Host "Test Summary"
Write-Host "======================================"
Write-Host "  Total  : $total"
Write-Host "  Passed : $passed" -ForegroundColor Green
Write-Host "  Failed : $failed" -ForegroundColor Red
Write-Host "  Time   : ${duration}s"
Write-Host ""

if (-not $AllowZeroTests.IsPresent -and $total -eq 0) {
    Write-Host "[FAIL] Zero tests executed. Failing to avoid false-positive CI pass." -ForegroundColor Red
    exit 1
}

if ($failed -gt 0) {
    Write-Host "[FAIL] Regression detected: failed tests > 0." -ForegroundColor Red
    exit 1
}

if ($unityBatchFailed -or $exitCode -ne 0) {
    Write-Host "[FAIL] Unity exited abnormally (ExitCode=$exitCode). See log: $LogFile" -ForegroundColor Red
    exit 1
}

$elapsed = (Get-Date) - $startTime
Write-Host "[SUCCESS] All tests passed." -ForegroundColor Green
Write-Host "Elapsed: $($elapsed.TotalSeconds.ToString('F1'))s"
exit 0

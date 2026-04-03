[CmdletBinding()]
param(
    [string]$ProjectPath = "C:\test\Steal",
    [string]$UnityExe = "C:\Program Files\Unity\Hub\Editor\6000.2.14f1\Editor\Unity.exe",
    [string]$OutputPrefix = "codex-rc-final"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$root = (Resolve-Path -LiteralPath $ProjectPath).Path
$runner = Join-Path $root "run-regression-tests.ps1"
if (-not (Test-Path -LiteralPath $runner)) {
    throw "Missing regression runner: $runner"
}

$editResults = Join-Path $root "$OutputPrefix-editmode-results.xml"
$editLog = Join-Path $root "$OutputPrefix-editmode.log"
$playResults = Join-Path $root "$OutputPrefix-playmode-results.xml"
$playLog = Join-Path $root "$OutputPrefix-playmode.log"

Write-Host "======================================"
Write-Host "INTIFALL RC Final Gate"
Write-Host "======================================"
Write-Host "Project   : $root"
Write-Host "UnityExe  : $UnityExe"
Write-Host "Prefix    : $OutputPrefix"
Write-Host ""

& $runner -ProjectPath $root -UnityExe $UnityExe -TestPlatform EditMode -ResultsFile $editResults -LogFile $editLog
& $runner -ProjectPath $root -UnityExe $UnityExe -TestPlatform PlayMode -ResultsFile $playResults -LogFile $playLog

function Read-TestSummary {
    param([string]$Path)
    [xml]$xml = Get-Content -LiteralPath $Path -Raw
    $node = $xml.SelectSingleNode("/test-run")
    return [pscustomobject]@{
        Total = [int]$node.total
        Passed = [int]$node.passed
        Failed = [int]$node.failed
        Duration = [double]$node.duration
    }
}

$edit = Read-TestSummary -Path $editResults
$play = Read-TestSummary -Path $playResults

Write-Host ""
Write-Host "--------------------------------------"
Write-Host "RC Final Gate Summary"
Write-Host "--------------------------------------"
Write-Host ("EditMode: {0}/{1} PASS, failed {2}, duration {3:0.00}s" -f $edit.Passed, $edit.Total, $edit.Failed, $edit.Duration)
Write-Host ("PlayMode: {0}/{1} PASS, failed {2}, duration {3:0.00}s" -f $play.Passed, $play.Total, $play.Failed, $play.Duration)
Write-Host "Results :"
Write-Host "  $editResults"
Write-Host "  $playResults"
Write-Host "Logs    :"
Write-Host "  $editLog"
Write-Host "  $playLog"

$ErrorActionPreference = "Stop"
$PROJECT_PATH = "E:/LevelDesign/Steal"
$UNITY_EXE = "D:/unity/6000.0.2f1/Editor/Unity.exe"
$RESULTS_FILE = "$PROJECT_PATH/test-results-regression.xml"
$LOG_FILE = "$PROJECT_PATH/test-regression.log"

Write-Host "======================================"
Write-Host "INTIFALL 回归测试"
Write-Host "======================================"
Write-Host ""

$startTime = Get-Date

Write-Host "[1/2] 运行测试..."

$argList = @(
    "-batchmode",
    "-projectPath", $PROJECT_PATH,
    "-runTests",
    "-testResults", $RESULTS_FILE,
    "-testPlatform", "playmode",
    "-logFile", $LOG_FILE
)

$pinfo = New-Object System.Diagnostics.ProcessStartInfo
$pinfo.FileName = $UNITY_EXE
$pinfo.Arguments = $argList -join " "
$pinfo.RedirectStandardOutput = $false
$pinfo.RedirectStandardError = $false
$pinfo.UseShellExecute = $false
$pinfo.CreateNoWindow = $true

$p = New-Object System.Diagnostics.Process
$p.StartInfo = $pinfo
$p.Start() | Out-Null
$p.WaitForExit()
$exitCode = $p.ExitCode

if (Test-Path $LOG_FILE) {
    $logContent = Get-Content $LOG_FILE -Raw
    if ($logContent -match "Aborting batchmode due to failure") {
        Write-Host "[FAIL] Unity 运行失败" -ForegroundColor Red
        exit 1
    }
}

if (-not (Test-Path $RESULTS_FILE)) {
    Write-Host "[FAIL] 测试结果文件不存在" -ForegroundColor Red
    exit 1
}

Write-Host "[2/2] 分析结果..."

$xml = [xml](Get-Content $RESULTS_FILE -Raw)
$total = [int]$xml.'test-run'.total
$passed = [int]$xml.'test-run'.passed
$failed = [int]$xml.'test-run'.failed
$duration = [math]::Round([double]$xml.'test-run'.duration, 2)

Write-Host ""
Write-Host "======================================"
Write-Host "测试结果"
Write-Host "======================================"
Write-Host "  总数:   $total"
Write-Host "  通过:   $passed" -ForegroundColor Green
Write-Host "  失败:   $failed" -ForegroundColor Red
Write-Host "  用时:   ${duration}s"
Write-Host ""

if ($failed -gt 0) {
    Write-Host "[REGRESSION DETECTED] 测试失败，禁止合并" -ForegroundColor Red
    exit 1
}

$elapsed = (Get-Date) - $startTime
Write-Host "[SUCCESS] 所有测试通过，代码质量门禁已通过" -ForegroundColor Green
Write-Host "用时: $($elapsed.TotalSeconds.ToString('F1'))s"
exit 0

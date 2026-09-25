param(
    [string]$Editor,
    [ValidateRange(1, 240)][int]$TimeoutMinutes = 30
)
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Unity-Process.ps1')
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $Editor) {
    $versionFile = Join-Path $project 'ProjectSettings/ProjectVersion.txt'
    $version = (Select-String -Path $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value.Trim()
    if (-not $version) { throw "Unity version is missing from $versionFile" }
    $Editor = "D:\unity\$version\Editor\Unity.exe"
}
if (-not (Test-Path -LiteralPath $Editor)) { throw "Unity Editor not found: $Editor" }
$results = Join-Path $project 'TestResults'
$logs = Join-Path $project 'Logs'
New-Item -ItemType Directory -Force -Path $results,$logs | Out-Null
# Invalidate both suites before starting: a failed EditMode run must not leave
# an old PlayMode success that looks like evidence for this invocation.
foreach ($mode in @('EditMode', 'PlayMode')) {
    $oldResult = Join-Path $results "$mode.xml"
    if (Test-Path -LiteralPath $oldResult) { Remove-Item -LiteralPath $oldResult }
}
foreach ($mode in @('EditMode', 'PlayMode')) {
    $result = Join-Path $results "$mode.xml"
    $log = Join-Path $logs "$mode.log"
    # A previous successful report must never hide a failed or incomplete run.
    if (Test-Path -LiteralPath $result) { Remove-Item -LiteralPath $result }
    # Do not pass -quit: Test Runner exits Unity after the test run finishes.
    $arguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $project + '"'), '-runTests', '-testPlatform', $mode, '-testResults', ('"' + $result + '"'), '-logFile', ('"' + $log + '"'))
    Invoke-UnityProcess -Editor $Editor -Arguments $arguments -Log $log -TimeoutMinutes $TimeoutMinutes
    if (-not (Test-Path -LiteralPath $result)) { throw "$mode did not produce test results. See $log" }
    [xml]$report = Get-Content -LiteralPath $result
    $run = $report.'test-run'
    if ($run.result -ne 'Passed' -or [int]$run.total -eq 0 -or [int]$run.failed -ne 0 -or [int]$run.skipped -ne 0) { throw "$mode result: $($run.result), total: $($run.total), failed: $($run.failed), skipped: $($run.skipped)" }
    Write-Output "$mode : $($run.passed) passed, $($run.failed) failed, $($run.skipped) skipped"
}

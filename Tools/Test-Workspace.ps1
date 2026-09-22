[CmdletBinding()]
param(
    [string]$Editor = '',
    [switch]$RunUnity,
    [switch]$BuildAndroid
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$version = (Select-String -Path (Join-Path $project 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value.Trim()
if (-not $Editor) { $Editor = "D:\unity\$version\Editor\Unity.exe" }
$checks = [Collections.Generic.List[object]]::new()
function Check([string]$Name, [scriptblock]$Action) {
    try {
        & $Action | Out-Host
        $checks.Add([pscustomobject]@{name=$Name; status='passed'; detail=''})
        return $true
    } catch {
        $checks.Add([pscustomobject]@{name=$Name; status='failed'; detail=$_.Exception.Message})
        Write-Warning "$Name : $($_.Exception.Message)"
        return $false
    }
}
function Skip([string]$Name, [string]$Reason) {
    $checks.Add([pscustomobject]@{name=$Name; status='not_run'; detail=$Reason})
}
$auditOk = Check 'repository' {
    python (Join-Path $PSScriptRoot 'validate_repo.py')
    if ($LASTEXITCODE -ne 0) { throw 'Static audit failed.' }
}
$lfsOk = Check 'git_lfs' {
    git lfs version
    if ($LASTEXITCODE -ne 0) { throw 'Git LFS unavailable.' }
}
$editorOk = Check 'editor' {
    if (-not (Test-Path -LiteralPath $Editor)) { throw "Missing editor: $Editor" }
    $installed = (Get-Item -LiteralPath $Editor).VersionInfo.ProductVersion
    if ($installed -notlike "$version*") { throw "Expected $version, found $installed" }
}
$androidOk = Check 'android_toolchain' {
    & (Join-Path $PSScriptRoot 'Check-AndroidToolchain.ps1') -AndroidPlayer (Join-Path (Split-Path $Editor) 'Data/PlaybackEngines/AndroidPlayer')
}
$toolsOk = Check 'tooling_tests' {
    python -m unittest discover -s $PSScriptRoot -p 'test_*.py'
    if ($LASTEXITCODE -ne 0) { throw 'Development tooling tests failed.' }
}
$codeOk = $true
if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $codeOk = Check 'domain_checks' {
        dotnet run --project (Join-Path $PSScriptRoot 'DomainCheck/DomainCheck.csproj')
        if ($LASTEXITCODE -ne 0) { throw 'Pure C# domain checks failed.' }
    }
    $syntaxOk = Check 'csharp_syntax' {
        dotnet run --project (Join-Path $PSScriptRoot 'SyntaxCheck/SyntaxCheck.csproj') -- $project
        if ($LASTEXITCODE -ne 0) { throw 'C# syntax checks failed.' }
    }
    $codeOk = $codeOk -and $syntaxOk
} else {
    Skip 'domain_checks' '.NET SDK unavailable; real Unity tests are still required.'
    Skip 'csharp_syntax' '.NET SDK unavailable.'
}
$testsOk = $false
if (($RunUnity -or $BuildAndroid) -and $auditOk -and $editorOk -and $lfsOk -and $toolsOk -and $codeOk) {
    $testsOk = Check 'unity_tests' { & (Join-Path $PSScriptRoot 'Test-Unity.ps1') -Editor $Editor }
} else { Skip 'unity_tests' 'Requires -RunUnity (or -BuildAndroid) and passing prerequisites.' }
if ($BuildAndroid -and $testsOk -and $androidOk) {
    $output = 'build/validation-' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '/MergeStudio.aab'
    $buildOk = Check 'android_bundle' { & (Join-Path $PSScriptRoot 'Build-Android-Aab.ps1') -Editor $Editor -Output $output }
} else { Skip 'android_bundle' 'Requires -BuildAndroid and passing Unity tests/toolchain.' }
Skip 'device_qa' 'Requires a connected device and recorded gameplay/performance results.'
$reportDir = Join-Path $project 'artifacts'
New-Item -ItemType Directory -Force -Path $reportDir | Out-Null
$revision = git -C $project rev-parse HEAD
$dirty = @(git -C $project status --porcelain).Count -gt 0
[ordered]@{utc=[DateTime]::UtcNow.ToString('o'); commit=$revision; dirty=$dirty; editor=$version; checks=@($checks.ToArray())} |
    ConvertTo-Json -Depth 5 | Set-Content -Encoding UTF8 (Join-Path $reportDir 'workspace-status.json')
$checks | Format-Table name,status,detail -Wrap
Write-Host 'Evidence: artifacts/workspace-status.json. not_run is not a passing check.'
if (@($checks | Where-Object status -eq 'failed').Count -gt 0) { exit 1 }

[CmdletBinding()]
param(
    [string]$Editor = "",
    [string]$Output = "build/MergeStudio.aab",
    [switch]$DisableBurst,
    [ValidateRange(1, 240)][int]$TimeoutMinutes = 60
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

$outputPath = [IO.Path]::GetFullPath((Join-Path $project $Output))
if (Test-Path -LiteralPath $outputPath) { throw "Output already exists; choose a fresh -Output path: $outputPath" }
$env:MERGESTUDIO_BUILD_PATH = $outputPath
if ($DisableBurst) { $env:MERGESTUDIO_DISABLE_BURST = '1' } else { Remove-Item Env:MERGESTUDIO_DISABLE_BURST -ErrorAction SilentlyContinue }
$editorDir = Split-Path -Parent $Editor
$androidPlayer = Join-Path (Join-Path $editorDir 'Data') 'PlaybackEngines/AndroidPlayer'
$env:MERGESTUDIO_ANDROID_SDK = Join-Path $androidPlayer 'SDK'
$env:MERGESTUDIO_ANDROID_NDK = Join-Path $androidPlayer 'NDK/android-ndk-r27c'
$env:MERGESTUDIO_JDK = Join-Path $androidPlayer 'OpenJDK'
$log = Join-Path $project 'Logs/Android-AAB.log'
New-Item -ItemType Directory -Force -Path (Split-Path $log) | Out-Null
$arguments = @(
    '-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $project + '"'),
    '-executeMethod', 'MergeStudio.Editor.CiBuild.BuildAndroid',
    '-logFile', ('"' + $log + '"')
)
Invoke-UnityProcess -Editor $Editor -Arguments $arguments -Log $log -TimeoutMinutes $TimeoutMinutes
if (-not (Test-Path -LiteralPath $outputPath)) { throw "Unity exited successfully but did not produce $outputPath" }
python (Join-Path $PSScriptRoot 'release_manifest.py') $outputPath ($outputPath + '.json')
if ($LASTEXITCODE -ne 0) { throw 'Android bundle verification failed.' }
Write-Output "Android AAB: $outputPath ($((Get-Item $outputPath).Length) bytes)"

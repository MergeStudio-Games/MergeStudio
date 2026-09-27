[CmdletBinding()]
param([string]$Adb = '', [string]$Serial = '')
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $Adb) {
    $version = (Select-String -Path (Join-Path $project 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value.Trim()
    $Adb = "D:\unity\$version\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"
}
if (-not (Test-Path -LiteralPath $Adb)) { throw "adb missing: $Adb" }
$rows = @(& $Adb devices)
if ($LASTEXITCODE -ne 0) { throw 'adb devices failed.' }
$ready = @($rows | Where-Object { $_ -match '^\S+\s+device$' } | ForEach-Object { ($_ -split '\s+')[0] })
if (-not $Serial) {
    if ($ready.Count -ne 1) { throw "Expected one authorized Android device; found $($ready.Count). Connect/unlock a device or supply -Serial." }
    $Serial = $ready[0]
}
if ($Serial -notin $ready) { throw 'Requested Android device is not connected and authorized.' }
function Read-Device([string[]]$Arguments) {
    $value = & $Adb -s $Serial shell @Arguments
    if ($LASTEXITCODE -ne 0) { throw 'Device query failed.' }
    return ($value -join "`n").Trim()
}
$result = [ordered]@{
    utc = [DateTime]::UtcNow.ToString('o')
    model = Read-Device @('getprop', 'ro.product.model')
    android = Read-Device @('getprop', 'ro.build.version.release')
    api = Read-Device @('getprop', 'ro.build.version.sdk')
    display = Read-Device @('wm', 'size')
    density = Read-Device @('wm', 'density')
    gameplayVerified = $false
    performanceVerified = $false
}
$directory = Join-Path $project 'artifacts'
New-Item -ItemType Directory -Force -Path $directory | Out-Null
$result | ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $directory 'android-device.json')
Write-Output 'Device inventory recorded in artifacts/android-device.json. This is not a gameplay or performance test.'

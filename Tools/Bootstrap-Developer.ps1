param(
    [string]$Editor = 'D:\unity\6000.6.0f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Write-Host "MergeStudio developer bootstrap: $project"
if (-not (Get-Command git -ErrorAction SilentlyContinue)) { throw 'Git is required.' }
if (-not (Get-Command python -ErrorAction SilentlyContinue)) { throw 'Python is required for the repository audit.' }
git lfs install
if ($LASTEXITCODE -ne 0) { throw 'Git LFS initialization failed.' }
python (Join-Path $project 'Tools/validate_repo.py')
if ($LASTEXITCODE -ne 0) { throw 'Repository audit failed.' }
if (Test-Path -LiteralPath $Editor) {
    Write-Host "Unity editor found: $Editor"
    Write-Host 'Run Tools/Test-Unity.ps1 after Unity finishes importing packages.'
} else {
    Write-Warning "Unity editor not found at $Editor. Install the version in ProjectSettings/ProjectVersion.txt."
}

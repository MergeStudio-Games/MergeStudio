param(
    [Parameter(Mandatory=$true)][ValidatePattern('^[A-Za-z][A-Za-z0-9-]{1,63}$')][string]$Repository,
    [Parameter(Mandatory=$true)][string]$ApplicationId,
    [string]$Product = $Repository,
    [string]$Company = 'MergeStudio Games',
    [string]$Directory = (Join-Path (Get-Location) $Repository),
    [string]$Revision = 'develop',
    [ValidateSet('public','private')][string]$Visibility = 'private',
    [switch]$LocalOnly
)
$ErrorActionPreference = 'Stop'
if (-not $LocalOnly -and -not (Get-Command gh -ErrorAction SilentlyContinue)) { throw 'GitHub CLI is required.' }
python (Join-Path $PSScriptRoot 'create_game.py') --destination $Directory --product $Product --company $Company --identifier $ApplicationId --revision $Revision
if ($LASTEXITCODE -ne 0) { throw 'Local game generation failed; no remote repository was created.' }
if ($LocalOnly) { Write-Host "Local game ready at $Directory; review and commit before publishing."; return }
git -C $Directory add --all
if ($LASTEXITCODE -ne 0) { throw 'Could not stage generated project.' }
git -C $Directory commit -m "Initialize $Product from MergeStudio template"
if ($LASTEXITCODE -ne 0) { throw 'Could not commit; configure your Git identity. Local project preserved.' }
$full = "MergeStudio-Games/$Repository"
# The archive is the template. Start a new history; never overwrite remote refs.
gh repo create $full --$Visibility --source $Directory --remote origin --push --description "MergeStudio Games: $Product"
if ($LASTEXITCODE -ne 0) { throw "Could not publish $full. Local project preserved at $Directory." }
git -C $Directory branch main
if ($LASTEXITCODE -ne 0) { throw 'Could not create main.' }
git -C $Directory push origin main
if ($LASTEXITCODE -ne 0) { throw 'Could not publish main.' }
Write-Host "Created $full. Configure branch protection and per-game CI credentials before team use."

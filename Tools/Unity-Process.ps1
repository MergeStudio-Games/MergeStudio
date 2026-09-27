function Invoke-UnityProcess {
    param(
        [Parameter(Mandatory=$true)][string]$Editor,
        [Parameter(Mandatory=$true)][string[]]$Arguments,
        [Parameter(Mandatory=$true)][string]$Log,
        [ValidateRange(1, 240)][int]$TimeoutMinutes = 30
    )
    if (-not (Test-Path -LiteralPath $Editor)) { throw "Unity Editor not found: $Editor" }
    $process = Start-Process -FilePath $Editor -ArgumentList $Arguments -WindowStyle Hidden -PassThru
    # Wait only for the Editor we launched, not its long-lived licensing children.
    if (-not $process.WaitForExit($TimeoutMinutes * 60000)) {
        $process.Kill()
        throw "Unity timed out after $TimeoutMinutes minutes. See $Log"
    }
    $process.Refresh()
    if ($process.ExitCode -ne 0) {
        $hint = ''
        if ($process.ExitCode -eq 198 -or ((Test-Path -LiteralPath $Log) -and (Select-String -LiteralPath $Log -Pattern 'No valid Unity Editor license' -Quiet))) {
            $hint = ' Sign in to Unity Hub and activate your license under Settings > Licenses.'
        }
        throw "Unity exited with code $($process.ExitCode).$hint See $Log"
    }
}

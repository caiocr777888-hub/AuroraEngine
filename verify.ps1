$ErrorActionPreference = 'Stop'
foreach ($mode in @('--test', '--smoke')) {
    $process = Start-Process -FilePath (Join-Path $PSScriptRoot 'bin\AuroraEngine.exe') -ArgumentList $mode -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Validation failed: $mode" }
}
Get-Content (Join-Path $PSScriptRoot 'bin\test-results.txt')
Get-Content (Join-Path $PSScriptRoot 'bin\smoke-results.txt')


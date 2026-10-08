# Client for the v2rayN AI UI automation pipe (test instances only).
# Requires the target process to run with V2RAYN_AI_AUTOMATION=1.
# Usage:
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\ai-ui.ps1 -ProcessId 1234 -Command "windows"
#   powershell -NoProfile -ExecutionPolicy Bypass -File tools\ai-ui.ps1 -ProcessId 1234 -Command 'set txtRemarks "my note"'
# Prints the one-line JSON response. Exit code 1 on connection or protocol failure.
param(
    [Parameter(Mandatory = $true)][int]$ProcessId,
    [Parameter(Mandatory = $true)][string]$Command
)

$ErrorActionPreference = 'Stop'

if ($Command -match "[\r\n]") {
    Write-Error 'Command must be a single line.'
    exit 1
}

try {
    [Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)
} catch {
    # No console attached (redirected); output encoding stays as-is.
}

$pipeName = "v2rayN-ai-$ProcessId"
$utf8 = New-Object System.Text.UTF8Encoding($false)
$deadline = (Get-Date).AddSeconds(5)
$client = $null

while ($true) {
    $candidate = New-Object System.IO.Pipes.NamedPipeClientStream('.', $pipeName, [System.IO.Pipes.PipeDirection]::InOut, [System.IO.Pipes.PipeOptions]::None)
    $remainingMs = [int][math]::Max(1, ($deadline - (Get-Date)).TotalMilliseconds)
    try {
        $candidate.Connect($remainingMs)
        $client = $candidate
        break
    } catch {
        $candidate.Dispose()
        if ((Get-Date) -ge $deadline) {
            Write-Error "Could not connect to pipe '$pipeName' within 5 seconds. Check the process id and that V2RAYN_AI_AUTOMATION=1 was set at launch."
            exit 1
        }
        Start-Sleep -Milliseconds 100
    }
}

try {
    $writer = New-Object System.IO.StreamWriter($client, $utf8, 4096, $true)
    $writer.AutoFlush = $true
    $writer.NewLine = "`n"
    $writer.WriteLine($Command)

    $reader = New-Object System.IO.StreamReader($client, $utf8, $false, 4096, $true)
    $line = $reader.ReadLine()
    if ($null -eq $line) {
        Write-Error 'No response from the pipe.'
        exit 1
    }

    Write-Output $line
} finally {
    $client.Dispose()
}

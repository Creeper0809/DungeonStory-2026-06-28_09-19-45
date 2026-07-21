[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Command,
    [string]$Target = "",
    [string]$Key = "",
    [string]$Path = "",
    [float]$X = 0,
    [float]$Y = 0,
    [float]$Duration = 0,
    [ValidateRange(0, 2)]
    [int]$Button = 0,
    [string]$ConnectionPath = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($ConnectionPath)) {
    $ConnectionPath = Join-Path $env:USERPROFILE "AppData\LocalLow\DungeonStory\DungeonStoryPlaytest\Automation\bridge.json"
}

if (-not (Test-Path -LiteralPath $ConnectionPath)) {
    throw "Automation connection file was not found: $ConnectionPath"
}

$connection = Get-Content -Raw -LiteralPath $ConnectionPath | ConvertFrom-Json
$request = [ordered]@{
    id = [guid]::NewGuid().ToString("N")
    token = [string]$connection.token
    command = $Command
    target = $Target
    key = $Key
    path = $Path
    x = $X
    y = $Y
    duration = $Duration
    button = $Button
}

$client = [System.Net.Sockets.TcpClient]::new()
try {
    $connect = $client.ConnectAsync([string]$connection.host, [int]$connection.port)
    if (-not $connect.Wait(5000)) {
        throw "Timed out connecting to the player automation bridge."
    }

    $stream = $client.GetStream()
    $writer = [System.IO.StreamWriter]::new($stream, [System.Text.UTF8Encoding]::new($false), 4096, $true)
    $reader = [System.IO.StreamReader]::new($stream, [System.Text.Encoding]::UTF8, $false, 4096, $true)
    try {
        $writer.AutoFlush = $true
        $writer.WriteLine(($request | ConvertTo-Json -Compress))
        $responseLine = $reader.ReadLine()
        if ([string]::IsNullOrWhiteSpace($responseLine)) {
            throw "The player automation bridge returned an empty response."
        }

        $response = $responseLine | ConvertFrom-Json
        if (-not [string]::IsNullOrWhiteSpace([string]$response.data)) {
            try {
                $response | Add-Member -NotePropertyName parsedData -NotePropertyValue ($response.data | ConvertFrom-Json)
            }
            catch {
            }
        }

        if (-not $response.ok) {
            throw "Player automation command failed: $($response.error)"
        }

        $response
    }
    finally {
        $reader.Dispose()
        $writer.Dispose()
        $stream.Dispose()
    }
}
finally {
    $client.Dispose()
}

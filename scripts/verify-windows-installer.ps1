[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InstallerPath
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$canonicalManifest = Get-Content -LiteralPath (Join-Path $repoRoot "BKE AirStack\bke.manifest.json") -Raw | ConvertFrom-Json
$expectedVersion = [string]$canonicalManifest.version
$expectedInstallDir = Join-Path $env:ProgramFiles "BKE AirStack"
$installedExe = Join-Path $expectedInstallDir "BKE AirStack.exe"
$installedManifest = Join-Path $expectedInstallDir "bke.manifest.json"
$installedTemplate = Join-Path $expectedInstallDir "default.vmix"
$userDataDir = Join-Path $env:APPDATA "BKE AirStack"
$databasePath = Join-Path $userDataDir "DataBase\BKEAirStack.db"
$installationIdPath = Join-Path $env:LOCALAPPDATA "BKE Digital Solutions\AIRSTACK\installation.id"
$sharedAgentDir = Join-Path $env:ProgramData "BKE Digital Solutions\Licensing Agent"
$sharedAgentSentinel = Join-Path $sharedAgentDir "air-stack-packaging-test-sentinel.txt"
$hostsPath = Join-Path $env:SystemRoot "System32\drivers\etc\hosts"
$hostsMarker = "# Air Stack packaging verification"
$requestDir = Join-Path $env:RUNNER_TEMP "air-stack-agent-requests"

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

function Wait-ForPath([string]$Path, [int]$TimeoutSeconds) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path -LiteralPath $Path) { return $true }
        Start-Sleep -Milliseconds 200
    }
    return $false
}

function Start-FakeAgent([string]$ResponseJson, [string]$RequestPath) {
    return Start-Job -ArgumentList $ResponseJson, $RequestPath -ScriptBlock {
        param($Json, $OutputPath)
        $listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 43873)
        try {
            $listener.Start()
            $client = $listener.AcceptTcpClient()
            try {
                $stream = $client.GetStream()
                $reader = [IO.StreamReader]::new($stream, [Text.Encoding]::ASCII, $false, 1024, $true)
                $contentLength = 0
                while ($true) {
                    $line = $reader.ReadLine()
                    if ([string]::IsNullOrEmpty($line)) { break }
                    if ($line -match '^Content-Length:\s*(\d+)$') { $contentLength = [int]$Matches[1] }
                }
                $buffer = New-Object char[] $contentLength
                $read = 0
                while ($read -lt $contentLength) {
                    $count = $reader.Read($buffer, $read, $contentLength - $read)
                    if ($count -le 0) { break }
                    $read += $count
                }
                $body = -join $buffer[0..([Math]::Max(0, $read - 1))]
                Set-Content -LiteralPath $OutputPath -Value $body -Encoding UTF8

                $payload = [Text.Encoding]::UTF8.GetBytes($Json)
                $header = "HTTP/1.1 200 OK`r`nContent-Type: application/json`r`nContent-Length: $($payload.Length)`r`nConnection: close`r`n`r`n"
                $headerBytes = [Text.Encoding]::ASCII.GetBytes($header)
                $stream.Write($headerBytes, 0, $headerBytes.Length)
                $stream.Write($payload, 0, $payload.Length)
                $stream.Flush()
            }
            finally {
                $client.Dispose()
            }
        }
        finally {
            $listener.Stop()
        }
    }
}

function Stop-TestProcess([Diagnostics.Process]$Process) {
    if ($null -ne $Process -and -not $Process.HasExited) {
        Stop-Process -Id $Process.Id -Force
        $Process.WaitForExit()
    }
    Start-Sleep -Milliseconds 500
}

function Invoke-BlockedScenario([string]$Name, [string]$ResponseJson) {
    Remove-Item -LiteralPath $userDataDir -Recurse -Force -ErrorAction SilentlyContinue
    $requestPath = Join-Path $requestDir "$Name.json"
    Remove-Item -LiteralPath $requestPath -Force -ErrorAction SilentlyContinue
    $job = Start-FakeAgent $ResponseJson $requestPath
    $process = $null
    try {
        $process = Start-Process -FilePath $installedExe -WorkingDirectory $expectedInstallDir -PassThru
        Assert-True (Wait-ForPath $requestPath 15) "$Name did not reach the Licensing Agent contract."
        Start-Sleep -Seconds 2
        Assert-True (-not $process.HasExited) "$Name did not retain its blocking licensing UX."
        Assert-True (-not (Test-Path -LiteralPath $userDataDir)) "$Name instantiated protected Air Stack functionality."
        $request = Get-Content -LiteralPath $requestPath -Raw | ConvertFrom-Json
        Assert-True ($request.product_id -ceq "bke-air-stack") "$Name sent the wrong product_id."
        Assert-True ($request.version -ceq $expectedVersion) "$Name sent the wrong application version."
        $parsedInstallationId = [Guid]::Empty
        Assert-True ([Guid]::TryParse([string]$request.installation_id, [ref]$parsedInstallationId)) "$Name sent an invalid installation_id."
    }
    finally {
        Stop-TestProcess $process
        Stop-Job $job -ErrorAction SilentlyContinue
        Remove-Job $job -Force -ErrorAction SilentlyContinue
    }
}

if (-not (Test-Path -LiteralPath $InstallerPath -PathType Leaf)) { throw "Installer not found: $InstallerPath" }
New-Item -ItemType Directory -Path $requestDir, $sharedAgentDir -Force | Out-Null
Set-Content -LiteralPath $sharedAgentSentinel -Value "preserve shared agent state" -Encoding ASCII

$originalHosts = Get-Content -LiteralPath $hostsPath -Raw
$installed = $false
try {
    Add-Content -LiteralPath $hostsPath -Value "`r`n127.0.0.1 jl-bke.com $hostsMarker"

    $installer = Start-Process -FilePath $InstallerPath -ArgumentList "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART", "/SP-" -Wait -PassThru
    Assert-True ($installer.ExitCode -eq 0) "Installer exited with code $($installer.ExitCode)."
    $installed = $true

    Assert-True (Test-Path -LiteralPath $installedExe -PathType Leaf) "Installed executable is missing."
    Assert-True (Test-Path -LiteralPath $installedManifest -PathType Leaf) "Installed manifest is missing."
    Assert-True (Test-Path -LiteralPath $installedTemplate -PathType Leaf) "Installed default.vmix is missing."
    Assert-True (@(Get-ChildItem -LiteralPath $expectedInstallDir -Recurse -Filter SQLite.Interop.dll).Count -gt 0) "Installed SQLite native dependency is missing."

    $manifest = Get-Content -LiteralPath $installedManifest -Raw | ConvertFrom-Json
    Assert-True ($manifest.productId -ceq "bke-air-stack") "Installed manifest productId is wrong."
    Assert-True ($manifest.displayName -ceq "Air Stack") "Installed manifest displayName is wrong."
    Assert-True ($manifest.version -ceq $expectedVersion) "Installed manifest version is wrong."
    Assert-True ($manifest.entryPoint -ceq "BKE AirStack.exe") "Installed manifest entryPoint is wrong."

    $forbidden = @(Get-ChildItem -LiteralPath $expectedInstallDir -Recurse -File | Where-Object {
        $_.Name -eq "CPIO TELE-RADYO TOOLS.exe" -or $_.Extension.ToLowerInvariant() -in @(".cs", ".csproj", ".sln", ".vcxproj", ".pfx", ".snk", ".key", ".pdb")
    })
    Assert-True ($forbidden.Count -eq 0) "Installed content contains development or secret-bearing files."
    Assert-True (-not (Select-String -LiteralPath $installedTemplate -Pattern 'C:\\Users\\(?!Public\\)|file:///C:/Users/|jan2x' -Quiet)) "Installed template contains a developer-machine path."

    Invoke-BlockedScenario "denied" '{"authorized":false,"reason":"denied"}'
    Invoke-BlockedScenario "activation-required" '{"authorized":false,"reason":"activation_required"}'

    Remove-Item -LiteralPath $userDataDir -Recurse -Force -ErrorAction SilentlyContinue
    $unavailable = Start-Process -FilePath $installedExe -WorkingDirectory $expectedInstallDir -PassThru
    try {
        Start-Sleep -Seconds 7
        Assert-True (-not $unavailable.HasExited) "Agent-unavailable recovery UX did not remain open."
        Assert-True (-not (Test-Path -LiteralPath $userDataDir)) "Agent-unavailable path instantiated protected Air Stack functionality."
    }
    finally { Stop-TestProcess $unavailable }

    Remove-Item -LiteralPath $userDataDir -Recurse -Force -ErrorAction SilentlyContinue
    $allowRequest = Join-Path $requestDir "allowed.json"
    $allowJob = Start-FakeAgent '{"authorized":true,"reason":"allowed"}' $allowRequest
    $allowed = $null
    try {
        $allowed = Start-Process -FilePath $installedExe -WorkingDirectory $expectedInstallDir -PassThru
        Assert-True (Wait-ForPath $allowRequest 15) "Allowed path did not reach the Licensing Agent contract."
        Assert-True (Wait-ForPath $databasePath 20) "Allowed path did not initialize protected Air Stack functionality."
        Assert-True (-not $allowed.HasExited) "Allowed Air Stack process exited unexpectedly."
    }
    finally {
        Stop-TestProcess $allowed
        Stop-Job $allowJob -ErrorAction SilentlyContinue
        Remove-Job $allowJob -Force -ErrorAction SilentlyContinue
    }

    Assert-True (Test-Path -LiteralPath $installationIdPath -PathType Leaf) "Stable installation identity was not created."
    $installationIdBeforeUninstall = Get-Content -LiteralPath $installationIdPath -Raw
    Set-Content -LiteralPath (Join-Path $userDataDir "packaging-test-user-data.txt") -Value "preserve product data" -Encoding ASCII

    $uninstallerPath = Join-Path $expectedInstallDir "unins000.exe"
    Assert-True (Test-Path -LiteralPath $uninstallerPath -PathType Leaf) "Uninstaller is missing."
    $uninstaller = Start-Process -FilePath $uninstallerPath -ArgumentList "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART" -Wait -PassThru
    Assert-True ($uninstaller.ExitCode -eq 0) "Uninstaller exited with code $($uninstaller.ExitCode)."
    $installed = $false

    Assert-True (-not (Test-Path -LiteralPath $installedExe)) "Application executable remains after uninstall."
    Assert-True (Test-Path -LiteralPath (Join-Path $userDataDir "packaging-test-user-data.txt")) "Uninstall removed Air Stack user data."
    Assert-True (Test-Path -LiteralPath $installationIdPath -PathType Leaf) "Uninstall removed stable installation identity."
    Assert-True ((Get-Content -LiteralPath $installationIdPath -Raw) -ceq $installationIdBeforeUninstall) "Uninstall changed stable installation identity."
    Assert-True (Test-Path -LiteralPath $sharedAgentSentinel -PathType Leaf) "Uninstall damaged shared Licensing Agent state."
}
finally {
    Set-Content -LiteralPath $hostsPath -Value $originalHosts -NoNewline
    if ($installed) {
        $uninstallerPath = Join-Path $expectedInstallDir "unins000.exe"
        if (Test-Path -LiteralPath $uninstallerPath) {
            Start-Process -FilePath $uninstallerPath -ArgumentList "/VERYSILENT", "/SUPPRESSMSGBOXES", "/NORESTART" -Wait
        }
    }
    Remove-Item -LiteralPath $sharedAgentSentinel -Force -ErrorAction SilentlyContinue
}

Write-Host "Installed-runtime verification passed."

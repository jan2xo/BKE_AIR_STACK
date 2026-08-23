[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [string]$InnoCompiler
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $repoRoot "BKE AirStack\BKE AirStack.csproj"
$manifestPath = Join-Path $repoRoot "BKE AirStack\bke.manifest.json"
$assemblyInfoPath = Join-Path $repoRoot "BKE AirStack\Properties\AssemblyInfo.cs"
$templatePath = Join-Path $repoRoot "BKE AirStack\default.vmix"
$publishDir = Join-Path $repoRoot "artifacts\publish\win-x64"
$installerDir = Join-Path $repoRoot "artifacts\installer"
$metadataPath = Join-Path $installerDir "Air-Stack-installer.json"
$innoScript = Join-Path $repoRoot "installer\AirStack.iss"

function Assert-Equal([string]$Actual, [string]$Expected, [string]$Name) {
    if (-not [string]::Equals($Actual, $Expected, [StringComparison]::Ordinal)) {
        throw "$Name mismatch. Expected '$Expected', found '$Actual'."
    }
}

$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([int]$manifest.schemaVersion -ne 1) { throw "Unsupported manifest schemaVersion." }
Assert-Equal $manifest.productId "bke-air-stack" "Manifest productId"
Assert-Equal $manifest.displayName "Air Stack" "Manifest displayName"
Assert-Equal $manifest.entryPoint "BKE AirStack.exe" "Manifest entryPoint"
Assert-Equal $manifest.platform "windows" "Manifest platform"
Assert-Equal $manifest.architecture "x64" "Manifest architecture"
if ([string]::IsNullOrWhiteSpace($manifest.version)) { throw "Manifest version is required." }
$version = [string]$manifest.version

$assemblyInfo = Get-Content -LiteralPath $assemblyInfoPath -Raw
$assemblyMatch = [regex]::Match($assemblyInfo, 'AssemblyVersion\("(?<version>\d+\.\d+\.\d+)\.\d+"\)')
$fileMatch = [regex]::Match($assemblyInfo, 'AssemblyFileVersion\("(?<version>\d+\.\d+\.\d+)\.\d+"\)')
if (-not $assemblyMatch.Success -or -not $fileMatch.Success) {
    throw "Assembly version metadata could not be read."
}
Assert-Equal $assemblyMatch.Groups["version"].Value $version "Assembly version"
Assert-Equal $fileMatch.Groups["version"].Value $version "Assembly file version"

Remove-Item -LiteralPath $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installerDir -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir, $installerDir -Force | Out-Null

& dotnet restore $projectPath -r $RuntimeIdentifier -p:SignManifests=false
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE." }

& dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $RuntimeIdentifier `
    --self-contained true `
    --no-restore `
    -p:PublishSingleFile=false `
    -p:PublishTrimmed=false `
    -p:SignManifests=false `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --output $publishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed with exit code $LASTEXITCODE." }

Copy-Item -LiteralPath $manifestPath -Destination (Join-Path $publishDir "bke.manifest.json") -Force

$template = Get-Content -LiteralPath $templatePath -Raw
$sanitizedTemplate = $template.Replace(
    'C:\Users\jan2x\Documents\vMixStorage\capture.mp4',
    'C:\Users\Public\Videos\Air Stack\capture.mp4')
$sanitizedTemplate = $sanitizedTemplate.Replace(
    'file:///C:/Users/jan2x/AppData/Roaming/CPIO TELE-RADYO/Data_Source/Mdata.xml',
    '')
if ($sanitizedTemplate -eq $template) {
    throw "The expected developer paths were not found in default.vmix; review the staging rule."
}
Set-Content -LiteralPath (Join-Path $publishDir "default.vmix") -Value $sanitizedTemplate -Encoding UTF8 -NoNewline

Get-ChildItem -LiteralPath $publishDir -Recurse -File -Filter *.pdb | Remove-Item -Force

$requiredFiles = @(
    (Join-Path $publishDir "BKE AirStack.exe"),
    (Join-Path $publishDir "BKE AirStack.dll"),
    (Join-Path $publishDir "bke.manifest.json"),
    (Join-Path $publishDir "default.vmix")
)
foreach ($requiredFile in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Required publish output is missing: $requiredFile"
    }
}

$sqliteInterop = @(Get-ChildItem -LiteralPath $publishDir -Recurse -File -Filter "SQLite.Interop.dll")
if ($sqliteInterop.Count -eq 0) { throw "SQLite native runtime dependency is missing." }

$forbiddenNames = @(
    "CPIO TELE-RADYO TOOLS.exe",
    "BKE AirStack_TemporaryKey.pfx",
    ".git"
)
$forbiddenExtensions = @(".cs", ".csproj", ".sln", ".vcxproj", ".pfx", ".snk", ".key", ".pdb")
$packagedFiles = @(Get-ChildItem -LiteralPath $publishDir -Recurse -File)
foreach ($file in $packagedFiles) {
    if ($forbiddenNames -contains $file.Name -or $forbiddenExtensions -contains $file.Extension.ToLowerInvariant()) {
        throw "Forbidden development or secret-bearing file in publish output: $($file.FullName)"
    }
}

$sensitivePathPattern = 'C:\\Users\\(?!Public\\)|file:///C:/Users/|jan2x'
foreach ($file in $packagedFiles | Where-Object { $_.Extension -in @(".json", ".config", ".xml", ".vmix", ".txt") }) {
    if (Select-String -LiteralPath $file.FullName -Pattern $sensitivePathPattern -Quiet) {
        throw "Developer-machine path found in publish output: $($file.FullName)"
    }
}

if ([string]::IsNullOrWhiteSpace($InnoCompiler)) {
    $candidates = @(
        (Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"),
        (Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe")
    )
    $InnoCompiler = $candidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
}
if ([string]::IsNullOrWhiteSpace($InnoCompiler) -or -not (Test-Path -LiteralPath $InnoCompiler -PathType Leaf)) {
    throw "Inno Setup 6 compiler was not found."
}

& $InnoCompiler "/DAppVersion=$version" "/DSourceDir=$publishDir" "/DOutputDir=$installerDir" $innoScript
if ($LASTEXITCODE -ne 0) { throw "Inno Setup failed with exit code $LASTEXITCODE." }

$installerPath = Join-Path $installerDir "Air-Stack-$version-Windows-x64.exe"
if (-not (Test-Path -LiteralPath $installerPath -PathType Leaf)) {
    throw "Expected installer was not produced: $installerPath"
}

$installer = Get-Item -LiteralPath $installerPath
$hash = (Get-FileHash -LiteralPath $installerPath -Algorithm SHA256).Hash.ToLowerInvariant()
$sourceSha = (& git -C $repoRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw "Could not resolve source commit." }

$metadata = [ordered]@{
    product = "Air Stack"
    productId = "bke-air-stack"
    version = $version
    target = "Windows x64"
    filename = $installer.Name
    bytes = $installer.Length
    mib = [Math]::Round($installer.Length / 1MB, 3)
    sha256 = $hash
    sourceCommit = $sourceSha
    selfContained = $true
}
$metadata | ConvertTo-Json | Set-Content -LiteralPath $metadataPath -Encoding UTF8
$metadata | ConvertTo-Json

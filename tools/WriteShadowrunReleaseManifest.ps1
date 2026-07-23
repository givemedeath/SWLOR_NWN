param(
    [string]$ModulePath,
    [string]$HakDirectory,
    [string]$TlkDirectory,
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($ModulePath)) {
    $ModulePath = Join-Path $repoRoot "ModuleSR\Erie Metroplex.mod"
}
if ([string]::IsNullOrWhiteSpace($HakDirectory)) {
    $HakDirectory = Join-Path $repoRoot "debugserver\hak"
}
if ([string]::IsNullOrWhiteSpace($TlkDirectory)) {
    $TlkDirectory = Join-Path $repoRoot "debugserver\tlk"
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $repoRoot "ModuleSR\Erie Metroplex.release.json"
}

$ModulePath = [IO.Path]::GetFullPath($ModulePath)
$HakDirectory = [IO.Path]::GetFullPath($HakDirectory)
$TlkDirectory = [IO.Path]::GetFullPath($TlkDirectory)
$OutputPath = [IO.Path]::GetFullPath($OutputPath)

if (-not (Test-Path -LiteralPath $ModulePath -PathType Leaf)) {
    throw "Packed module not found: $ModulePath"
}
if (-not (Test-Path -LiteralPath $HakDirectory -PathType Container)) {
    throw "HAK directory not found: $HakDirectory"
}
if (-not (Test-Path -LiteralPath $TlkDirectory -PathType Container)) {
    throw "TLK directory not found: $TlkDirectory"
}

$moduleInfo = Get-Content -LiteralPath (Join-Path $repoRoot "ModuleSR\ifo\module.ifo.json") -Raw |
    ConvertFrom-Json
$hakNames = @(
    $moduleInfo.Mod_HakList.value |
        ForEach-Object { $_.Mod_Hak.value }
)
$customTlkName = $moduleInfo.Mod_CustomTlk.value
$customTlkPath = Join-Path $TlkDirectory "$customTlkName.tlk"
if (-not (Test-Path -LiteralPath $customTlkPath -PathType Leaf)) {
    throw "Required Erie custom TLK is missing: $customTlkPath"
}
$customTlkFile = Get-Item -LiteralPath $customTlkPath

$hakArtifacts = foreach ($hakName in $hakNames) {
    $hakPath = Join-Path $HakDirectory "$hakName.hak"
    if (-not (Test-Path -LiteralPath $hakPath -PathType Leaf)) {
        throw "Required Erie HAK is missing: $hakPath"
    }

    $file = Get-Item -LiteralPath $hakPath
    [ordered]@{
        file = $file.Name
        bytes = $file.Length
        sha256 = (Get-FileHash -LiteralPath $hakPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$serverAssemblyPath = Join-Path $repoRoot "debugserver\dotnet\SWLOR.Game.Server.dll"
$serverAssembly = $null
if (Test-Path -LiteralPath $serverAssemblyPath -PathType Leaf) {
    $serverFile = Get-Item -LiteralPath $serverAssemblyPath
    $serverAssembly = [ordered]@{
        file = $serverFile.Name
        bytes = $serverFile.Length
        sha256 = (Get-FileHash -LiteralPath $serverAssemblyPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

$repoCommit = (& git -C $repoRoot rev-parse HEAD).Trim()
$hakCommit = (& git -C (Join-Path $repoRoot "SWLOR_Haks") rev-parse HEAD).Trim()
$sourceDirty = [bool](& git -C $repoRoot status --porcelain)
$haksDirty = [bool](& git -C (Join-Path $repoRoot "SWLOR_Haks") status --porcelain)
$moduleFile = Get-Item -LiteralPath $ModulePath

$manifest = [ordered]@{
    schemaVersion = 1
    world = "Erie Metroplex"
    gameProfile = "shadowrun"
    dataNamespace = "erie"
    generatedUtc = [DateTime]::UtcNow.ToString("o")
    source = [ordered]@{
        repositoryCommit = $repoCommit
        repositoryDirty = $sourceDirty
        haksCommit = $hakCommit
        haksDirty = $haksDirty
    }
    module = [ordered]@{
        file = $moduleFile.Name
        bytes = $moduleFile.Length
        sha256 = (Get-FileHash -LiteralPath $ModulePath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    customTlk = [ordered]@{
        file = $customTlkFile.Name
        bytes = $customTlkFile.Length
        sha256 = (Get-FileHash -LiteralPath $customTlkPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    serverAssembly = $serverAssembly
    haks = @($hakArtifacts)
}

$manifestJson = $manifest | ConvertTo-Json -Depth 10
[IO.File]::WriteAllText(
    $OutputPath,
    $manifestJson + [Environment]::NewLine,
    [Text.UTF8Encoding]::new($false))

$debugModuleDirectory = Join-Path $repoRoot "debugserver\modules"
if (Test-Path -LiteralPath $debugModuleDirectory -PathType Container) {
    Copy-Item -LiteralPath $OutputPath `
        -Destination (Join-Path $debugModuleDirectory ([IO.Path]::GetFileName($OutputPath))) `
        -Force
}

Write-Host "Wrote Erie release manifest: $OutputPath"

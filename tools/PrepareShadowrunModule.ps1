param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$moduleRoot = Join-Path $repoRoot "ModuleSR"
$procgenDll = Join-Path $repoRoot "SWLOR.ProcgenReview\bin\$Configuration\net8.0\SWLOR.ProcgenReview.dll"

function Write-JsonWithoutBom {
    param(
        [Parameter(Mandatory = $true)] $Value,
        [Parameter(Mandatory = $true)] [string] $Path
    )

    $json = $Value | ConvertTo-Json -Depth 100
    # nwn_gff validates the JSON token shape as well as the declared GFF type:
    # {"type":"float","value":0} is rejected, while value 0.0 is valid. PowerShell
    # drops the decimal when a float-valued field is assigned an integral number.
    $json = [regex]::Replace(
        $json,
        '("type"\s*:\s*"float"\s*,\s*"value"\s*:\s*)(-?\d+)(?=\s*[,}])',
        {
            param($match)
            $match.Groups[1].Value + $match.Groups[2].Value + ".0"
        })
    # Match the repository's two-space module JSON convention.
    $json = [regex]::Replace(
        $json,
        '(?m)^(?: {4})+',
        {
            param($match)
            " " * ($match.Value.Length / 2)
        })
    [IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, [Text.UTF8Encoding]::new($false))
}

function Clear-GffJsonList {
    param(
        [Parameter(Mandatory = $true)] [string] $Path,
        [Parameter(Mandatory = $true)] [string] $PropertyName
    )

    $content = [IO.File]::ReadAllText($Path)
    $propertyStart = $content.IndexOf(
        '"' + $PropertyName + '"',
        [StringComparison]::Ordinal)
    if ($propertyStart -lt 0) {
        throw "Could not find '$PropertyName' in '$Path'."
    }

    $valueStart = $content.IndexOf('"value"', $propertyStart, [StringComparison]::Ordinal)
    $openBracket = $content.IndexOf('[', $valueStart)
    if ($valueStart -lt 0 -or $openBracket -lt 0) {
        throw "Could not find the list value for '$PropertyName' in '$Path'."
    }

    $depth = 0
    $inString = $false
    $escaped = $false
    $closeBracket = -1
    for ($index = $openBracket; $index -lt $content.Length; $index++) {
        $character = $content[$index]
        if ($inString) {
            if ($escaped) {
                $escaped = $false
            }
            elseif ($character -eq '\') {
                $escaped = $true
            }
            elseif ($character -eq '"') {
                $inString = $false
            }
            continue
        }

        if ($character -eq '"') {
            $inString = $true
        }
        elseif ($character -eq '[') {
            $depth++
        }
        elseif ($character -eq ']') {
            $depth--
            if ($depth -eq 0) {
                $closeBracket = $index
                break
            }
        }
    }

    if ($closeBracket -lt 0) {
        throw "Could not find the end of '$PropertyName' in '$Path'."
    }
    if ([string]::IsNullOrWhiteSpace(
        $content.Substring($openBracket + 1, $closeBracket - $openBracket - 1))) {
        return
    }

    $closingLineStart = $content.LastIndexOf("`n", $closeBracket) + 1
    $updated = $content.Substring(0, $openBracket + 1) +
        [Environment]::NewLine +
        $content.Substring($closingLineStart)
    [IO.File]::WriteAllText($Path, $updated, [Text.UTF8Encoding]::new($false))
}

function Set-GffJsonScalar {
    param(
        [Parameter(Mandatory = $true)] [string] $Path,
        [Parameter(Mandatory = $true)] [string] $PropertyName,
        [Parameter(Mandatory = $true)] [string] $JsonValue
    )

    $content = [IO.File]::ReadAllText($Path)
    $pattern = '("' + [regex]::Escape($PropertyName) +
        '"\s*:\s*\{\s*"type"\s*:\s*"[^"]+"\s*,\s*"value"\s*:\s*)' +
        '("[^"]*"|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?)(\s*\})'
    $matches = [regex]::Matches($content, $pattern)
    if ($matches.Count -ne 1) {
        throw "Expected exactly one '$PropertyName' scalar in '$Path'; found $($matches.Count)."
    }

    $updated = [regex]::Replace(
        $content,
        $pattern,
        {
            param($match)
            $match.Groups[1].Value + $JsonValue + $match.Groups[3].Value
        })
    [IO.File]::WriteAllText($Path, $updated, [Text.UTF8Encoding]::new($false))
}

function Set-GffJsonList {
    param(
        [Parameter(Mandatory = $true)] [string] $Path,
        [Parameter(Mandatory = $true)] [string] $PropertyName,
        [Parameter(Mandatory = $true)] $Value
    )

    $content = [IO.File]::ReadAllText($Path)
    $propertyStart = $content.IndexOf(
        '"' + $PropertyName + '"',
        [StringComparison]::Ordinal)
    $valueStart = $content.IndexOf('"value"', $propertyStart, [StringComparison]::Ordinal)
    $openBracket = $content.IndexOf('[', $valueStart)
    if ($propertyStart -lt 0 -or $valueStart -lt 0 -or $openBracket -lt 0) {
        throw "Could not find list '$PropertyName' in '$Path'."
    }

    $depth = 0
    $inString = $false
    $escaped = $false
    $closeBracket = -1
    for ($index = $openBracket; $index -lt $content.Length; $index++) {
        $character = $content[$index]
        if ($inString) {
            if ($escaped) {
                $escaped = $false
            }
            elseif ($character -eq '\') {
                $escaped = $true
            }
            elseif ($character -eq '"') {
                $inString = $false
            }
            continue
        }

        if ($character -eq '"') {
            $inString = $true
        }
        elseif ($character -eq '[') {
            $depth++
        }
        elseif ($character -eq ']') {
            $depth--
            if ($depth -eq 0) {
                $closeBracket = $index
                break
            }
        }
    }
    if ($closeBracket -lt 0) {
        throw "Could not find the end of list '$PropertyName' in '$Path'."
    }

    $arrayJson = $Value | ConvertTo-Json -Depth 20
    $arrayJson = [regex]::Replace(
        $arrayJson,
        '(?m)^(?: {4})+',
        {
            param($match)
            " " * ($match.Value.Length / 2)
        })
    $arrayLines = $arrayJson -split '\r?\n'
    for ($lineIndex = 1; $lineIndex -lt $arrayLines.Count; $lineIndex++) {
        $arrayLines[$lineIndex] = "    " + $arrayLines[$lineIndex]
    }
    $indentedArrayJson = $arrayLines -join [Environment]::NewLine

    $updated = $content.Substring(0, $openBracket) +
        $indentedArrayJson +
        $content.Substring($closeBracket + 1)
    [IO.File]::WriteAllText($Path, $updated, [Text.UTF8Encoding]::new($false))
}

if (-not (Test-Path -LiteralPath $procgenDll)) {
    throw "Build SWLOR.ProcgenReview first; expected '$procgenDll'."
}

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("swlor-erie-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot | Out-Null

try {
    & dotnet $procgenDll --areas "scifibase:::4242:8:1:1:plac:nodec" --json-out $tempRoot
    if ($LASTEXITCODE -ne 0) {
        throw "SWLOR.ProcgenReview failed with exit code $LASTEXITCODE."
    }

    $area = Get-Content -LiteralPath (Join-Path $tempRoot "pga1_4242.are.json") -Raw | ConvertFrom-Json
    $area.Name.value.'0' = "Erie Metroplex - Arrival"
    $area.Comments.value = "Deterministic clean-room arrival area. Generated with Sci-Fi Base seed 4242."
    $area.ResRef.value = "erie_arrival"
    $area.Tag.value = "erie_arrival"

    $instances = Get-Content -LiteralPath (Join-Path $tempRoot "pga1_4242.git.json") -Raw | ConvertFrom-Json
    $instances.WaypointList.value[0].Tag.value = "ENTRY_STARTING_WP"
    $instances.WaypointList.value[0].LocalizedName.value.'0' = "Erie Arrival"
    $instances.WaypointList.value[1].Tag.value = "DTH_DEFAULT_RESPAWN_POINT"
    $instances.WaypointList.value[1].LocalizedName.value.'0' = "Erie Default Respawn"

    Write-JsonWithoutBom $area (Join-Path $moduleRoot "are\erie_arrival.are.json")
    Write-JsonWithoutBom $instances (Join-Path $moduleRoot "git\erie_arrival.git.json")

    $noAccessPath = Join-Path $moduleRoot "git\no_access.git.json"
    Clear-GffJsonList $noAccessPath "Creature List"
    $noAccess = Get-Content -LiteralPath $noAccessPath -Raw | ConvertFrom-Json
    $allowedStorageTags = @(
        "TEMP_ITEM_STORAGE",
        "craft_temp_store",
        "QUEST_BARREL",
        "OUTFIT_BARREL",
        "MIGRATION_STORAGE"
    )
    $actualStorageTags = @($noAccess.'Placeable List'.value | ForEach-Object { $_.Tag.value })
    if (@($actualStorageTags | Where-Object { $allowedStorageTags -notcontains $_ }).Count -gt 0 -or
        @($allowedStorageTags | Where-Object { $actualStorageTags -notcontains $_ }).Count -gt 0) {
        throw "The no_access placeable set differs from the five approved runtime storage objects."
    }

    $moduleInfoPath = Join-Path $moduleRoot "ifo\module.ifo.json"
    $moduleAreas = @(
        "erie_arrival",
        "gen_placeholder1",
        "gen_placeholder2",
        "gen_placeholder3",
        "gen_placeholder4",
        "no_access"
    ) | ForEach-Object {
        [ordered]@{
            __struct_id = 6
            Area_Name = [ordered]@{ type = "resref"; value = $_ }
        }
    }
    Set-GffJsonList $moduleInfoPath "Mod_Area_list" $moduleAreas
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_Area" '"erie_arrival"'
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_X" "35.0"
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_Y" "65.0"
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_Z" "0.0"
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_Dir_X" "0.0"
    Set-GffJsonScalar $moduleInfoPath "Mod_Entry_Dir_Y" "1.0"

    # The proof-of-concept supports only the four verified procgen tilesets plus
    # the private service area. Add a HAK here only when a committed ModuleSR
    # resource or an accepted content package creates a real dependency on it.
    $moduleHaks = @(
        "sw_2da",
        "sw_ability",
        "sw_ui",
        "sw_vfx",
        "sw_t_minecave",
        "sw_t_scifibase",
        "sw_t_sewer",
        "sw_t_alienruin",
        "sw_t_template"
    )
    $moduleHakEntries = @(
        $moduleHaks | ForEach-Object {
            [ordered]@{
                __struct_id = 8
                Mod_Hak = [ordered]@{ type = "cexostring"; value = $_ }
            }
        }
    )
    Set-GffJsonList $moduleInfoPath "Mod_HakList" $moduleHakEntries

    foreach ($name in @("survival_knife.uti.json", "fresh_bread.uti.json")) {
        $starterItem = Get-Content -LiteralPath (Join-Path $repoRoot "Module\uti\$name") -Raw |
            ConvertFrom-Json
        foreach ($property in $starterItem.PSObject.Properties) {
            if ($property.Name -like "ModelPart*" -or $property.Name -like "xModelPart*") {
                $property.Value.value = 1
            }
        }
        Write-JsonWithoutBom $starterItem (Join-Path $moduleRoot "uti\$name")
    }

    $clothes = Get-Content -LiteralPath (Join-Path $repoRoot "Module\uti\duskhavenclothes.uti.json") -Raw |
        ConvertFrom-Json
    foreach ($property in $clothes.PSObject.Properties) {
        if ($property.Name -like "ArmorPart_*" -or $property.Name -like "xArmorPart_*") {
            $property.Value.value = if ($property.Name -like "*ArmorPart_Robe") { 0 } else { 1 }
        }
    }
    $clothes.LocalizedName.value.'0' = "Street Clothes"
    $clothes.Tag.value = "travelers_clothes"
    $clothes.TemplateResRef.value = "travelers_clothes"
    $clothes.Description.value.'0' = "Durable street clothes suitable for a runner arriving in Erie."
    $clothes.DescIdentified.value = [ordered]@{
        "0" = "Durable street clothes suitable for a runner arriving in Erie."
    }

    foreach ($destination in @(
        (Join-Path $repoRoot "Module\uti\travelers_clothes.uti.json"),
        (Join-Path $moduleRoot "uti\travelers_clothes.uti.json")
    )) {
        Write-JsonWithoutBom $clothes $destination
    }

    foreach ($relativePath in @(
        "are\ooc_area.are.json",
        "are\czs220_hangar.are.json",
        "git\ooc_area.git.json",
        "git\czs220_hangar.git.json",
        "gic\ooc_area.gic.json",
        "gic\czs220_hangar.gic.json"
    )) {
        $target = Join-Path $moduleRoot $relativePath
        if (Test-Path -LiteralPath $target) {
            Remove-Item -LiteralPath $target
        }
    }
}
finally {
    if (Test-Path -LiteralPath $tempRoot) {
        Remove-Item -LiteralPath $tempRoot -Recurse
    }
}

Write-Host "Prepared ModuleSR: clean Erie arrival, isolated infrastructure, starter resources, and minimal HAK manifest."

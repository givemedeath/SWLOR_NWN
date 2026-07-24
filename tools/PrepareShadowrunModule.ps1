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

    # Bidirectional link to the Barrens proof street (barrens_strip). The outbound
    # transition trigger sits on the full-floor tile at (85,45) -- 10m clear of the
    # respawn point at (85,55) and ~44m from the arrival/return spot at (35,65) -- so
    # neither a fresh spawn, a respawn, nor a returning runner lands inside it.
    # WP_ARRIVAL_FROM_STRIP is where barrens_strip's return trigger delivers the player.
    # These are re-applied here because this script regenerates erie_arrival from procgen.
    $arrivalReturnWaypoint = [ordered]@{
        __struct_id   = 5
        Appearance    = [ordered]@{ type = "byte"; value = 1 }
        Description   = [ordered]@{ type = "cexolocstring"; value = [ordered]@{} }
        HasMapNote    = [ordered]@{ type = "byte"; value = 0 }
        LinkedTo      = [ordered]@{ type = "cexostring"; value = "" }
        LocalizedName = [ordered]@{ type = "cexolocstring"; value = [ordered]@{ "0" = "Arrival from Strip" } }
        MapNote       = [ordered]@{ type = "cexolocstring"; value = [ordered]@{} }
        MapNoteEnabled = [ordered]@{ type = "byte"; value = 0 }
        Tag           = [ordered]@{ type = "cexostring"; value = "WP_ARRIVAL_FROM_STRIP" }
        TemplateResRef = [ordered]@{ type = "resref"; value = "nw_waypoint001" }
        XOrientation  = [ordered]@{ type = "float"; value = 0.0 }
        XPosition     = [ordered]@{ type = "float"; value = 35.0 }
        YOrientation  = [ordered]@{ type = "float"; value = 1.0 }
        YPosition     = [ordered]@{ type = "float"; value = 65.0 }
        ZPosition     = [ordered]@{ type = "float"; value = 0.0 }
    }
    function New-TransitionPoint([double]$x, [double]$y) {
        [ordered]@{
            __struct_id = 3
            PointX = [ordered]@{ type = "float"; value = $x }
            PointY = [ordered]@{ type = "float"; value = $y }
            PointZ = [ordered]@{ type = "float"; value = 0.0 }
        }
    }
    $arrivalOutboundTrigger = [ordered]@{
        __struct_id     = 1
        AutoRemoveKey   = [ordered]@{ type = "byte"; value = 0 }
        Cursor          = [ordered]@{ type = "byte"; value = 1 }
        DisarmDC        = [ordered]@{ type = "byte"; value = 0 }
        Faction         = [ordered]@{ type = "dword"; value = 1 }
        Geometry        = [ordered]@{ type = "list"; value = @(
            (New-TransitionPoint -3.0 -3.0),
            (New-TransitionPoint 3.0 -3.0),
            (New-TransitionPoint 3.0 3.0),
            (New-TransitionPoint -3.0 3.0)) }
        HighlightHeight = [ordered]@{ type = "float"; value = 0.0 }
        KeyName         = [ordered]@{ type = "cexostring"; value = "" }
        LinkedTo        = [ordered]@{ type = "cexostring"; value = "WP_STRIP_FROM_ARRIVAL" }
        LinkedToFlags   = [ordered]@{ type = "byte"; value = 2 }
        LoadScreenID    = [ordered]@{ type = "word"; value = 0 }
        LocalizedName   = [ordered]@{ type = "cexolocstring"; value = [ordered]@{ "0" = "Erie Metroplex - Into the Barrens" } }
        OnClick         = [ordered]@{ type = "resref"; value = "" }
        OnDisarm        = [ordered]@{ type = "resref"; value = "" }
        OnTrapTriggered = [ordered]@{ type = "resref"; value = "" }
        PortraitId      = [ordered]@{ type = "word"; value = 0 }
        ScriptHeartbeat = [ordered]@{ type = "resref"; value = "" }
        ScriptOnEnter   = [ordered]@{ type = "resref"; value = "" }
        ScriptOnExit    = [ordered]@{ type = "resref"; value = "" }
        ScriptUserDefine = [ordered]@{ type = "resref"; value = "" }
        Tag             = [ordered]@{ type = "cexostring"; value = "arrival_to_strip" }
        TemplateResRef  = [ordered]@{ type = "resref"; value = "newtransition" }
        TrapDetectable  = [ordered]@{ type = "byte"; value = 1 }
        TrapDetectDC    = [ordered]@{ type = "byte"; value = 0 }
        TrapDisarmable  = [ordered]@{ type = "byte"; value = 1 }
        TrapFlag        = [ordered]@{ type = "byte"; value = 0 }
        TrapOneShot     = [ordered]@{ type = "byte"; value = 1 }
        TrapType        = [ordered]@{ type = "byte"; value = 0 }
        Type            = [ordered]@{ type = "int"; value = 1 }
        XOrientation    = [ordered]@{ type = "float"; value = 0.0 }
        XPosition       = [ordered]@{ type = "float"; value = 85.0 }
        YOrientation    = [ordered]@{ type = "float"; value = 0.0 }
        YPosition       = [ordered]@{ type = "float"; value = 45.0 }
        ZOrientation    = [ordered]@{ type = "float"; value = 0.0 }
        ZPosition       = [ordered]@{ type = "float"; value = 0.0 }
    }
    $instances.WaypointList.value = @($instances.WaypointList.value) + $arrivalReturnWaypoint
    $instances.TriggerList.value = @($instances.TriggerList.value) + $arrivalOutboundTrigger

    # Visible landmark on the outbound trigger so the exit is findable: an elevator
    # (appearance 1414, shp_elev01) reading as "ride out to street level in the Barrens".
    # Static/Plot set-dressing; the Type-1 trigger under it does the transition.
    $arrivalElevator = [ordered]@{
        __struct_id    = 9
        AnimationState = [ordered]@{ type = "byte"; value = 0 }
        Appearance     = [ordered]@{ type = "dword"; value = 21077 }
        AutoRemoveKey  = [ordered]@{ type = "byte"; value = 0 }
        Bearing        = [ordered]@{ type = "float"; value = 3.14159 }
        BodyBag        = [ordered]@{ type = "byte"; value = 0 }
        CloseLockDC    = [ordered]@{ type = "byte"; value = 0 }
        Conversation   = [ordered]@{ type = "resref"; value = "" }
        CurrentHP      = [ordered]@{ type = "short"; value = 10 }
        Description    = [ordered]@{ type = "cexolocstring"; value = [ordered]@{} }
        DisarmDC       = [ordered]@{ type = "byte"; value = 0 }
        Faction        = [ordered]@{ type = "dword"; value = 3 }
        Fort           = [ordered]@{ type = "byte"; value = 5 }
        Hardness       = [ordered]@{ type = "byte"; value = 5 }
        HasInventory   = [ordered]@{ type = "byte"; value = 0 }
        HP             = [ordered]@{ type = "short"; value = 10 }
        Interruptable  = [ordered]@{ type = "byte"; value = 1 }
        KeyName        = [ordered]@{ type = "cexostring"; value = "" }
        KeyRequired    = [ordered]@{ type = "byte"; value = 0 }
        Lockable       = [ordered]@{ type = "byte"; value = 0 }
        Locked         = [ordered]@{ type = "byte"; value = 0 }
        LocName        = [ordered]@{ type = "cexolocstring"; value = [ordered]@{ "0" = "Elevator to the Barrens" } }
        OnClick        = [ordered]@{ type = "resref"; value = "" }
        OnClosed       = [ordered]@{ type = "resref"; value = "" }
        OnDamaged      = [ordered]@{ type = "resref"; value = "" }
        OnDeath        = [ordered]@{ type = "resref"; value = "" }
        OnDisarm       = [ordered]@{ type = "resref"; value = "" }
        OnHeartbeat    = [ordered]@{ type = "resref"; value = "" }
        OnInvDisturbed = [ordered]@{ type = "resref"; value = "" }
        OnLock         = [ordered]@{ type = "resref"; value = "" }
        OnMeleeAttacked = [ordered]@{ type = "resref"; value = "" }
        OnOpen         = [ordered]@{ type = "resref"; value = "" }
        OnSpellCastAt  = [ordered]@{ type = "resref"; value = "" }
        OnTrapTriggered = [ordered]@{ type = "resref"; value = "" }
        OnUnlock       = [ordered]@{ type = "resref"; value = "" }
        OnUsed         = [ordered]@{ type = "resref"; value = "" }
        OnUserDefined  = [ordered]@{ type = "resref"; value = "" }
        OpenLockDC     = [ordered]@{ type = "byte"; value = 0 }
        Plot           = [ordered]@{ type = "byte"; value = 1 }
        PortraitId     = [ordered]@{ type = "word"; value = 0 }
        Ref            = [ordered]@{ type = "byte"; value = 0 }
        Static         = [ordered]@{ type = "byte"; value = 1 }
        Tag            = [ordered]@{ type = "cexostring"; value = "barrens_elevator" }
        TemplateResRef = [ordered]@{ type = "resref"; value = "_mdrn_pl_elevato" }
        TrapDetectable = [ordered]@{ type = "byte"; value = 0 }
        TrapDetectDC   = [ordered]@{ type = "byte"; value = 0 }
        TrapDisarmable = [ordered]@{ type = "byte"; value = 0 }
        TrapFlag       = [ordered]@{ type = "byte"; value = 0 }
        TrapOneShot    = [ordered]@{ type = "byte"; value = 0 }
        TrapType       = [ordered]@{ type = "byte"; value = 0 }
        Type           = [ordered]@{ type = "byte"; value = 0 }
        Useable        = [ordered]@{ type = "byte"; value = 0 }
        Will           = [ordered]@{ type = "byte"; value = 0 }
        X              = [ordered]@{ type = "float"; value = 85.0 }
        Y              = [ordered]@{ type = "float"; value = 45.0 }
        Z              = [ordered]@{ type = "float"; value = 0.0 }
    }
    $instances.'Placeable List'.value = @($instances.'Placeable List'.value | Where-Object { $_.Tag.value -ne "barrens_elevator" }) + $arrivalElevator

    # Optional large procgen comparison street. It deliberately branches from arrival separately
    # from the accepted Strip so the operator can compare authored dgt04 slum geometry with the
    # generated 40x40 Modern Exterior candidate without changing the accepted route.
    $procgenReturnWaypoint = $arrivalReturnWaypoint | ConvertTo-Json -Depth 100 | ConvertFrom-Json
    $procgenReturnWaypoint.Tag.value = "WP_ARRIVAL_FROM_PGEN"
    $procgenReturnWaypoint.LocalizedName.value.'0' = "Arrival from Generated Barrens Street"
    $procgenReturnWaypoint.XPosition.value = 75.0
    $procgenReturnWaypoint.YPosition.value = 45.0
    $procgenOutboundTrigger = $arrivalOutboundTrigger | ConvertTo-Json -Depth 100 | ConvertFrom-Json
    $procgenOutboundTrigger.Tag.value = "arrival_to_pgen"
    $procgenOutboundTrigger.LinkedTo.value = "WP_PGEN_FROM_ARRIVAL"
    $procgenOutboundTrigger.LocalizedName.value.'0' = "Erie Metroplex - Generated Barrens Test Street"
    $procgenOutboundTrigger.XPosition.value = 75.0
    $procgenOutboundTrigger.YPosition.value = 45.0
    $procgenOutboundTrigger.Geometry.value[0].PointX.value = -5.0
    $procgenOutboundTrigger.Geometry.value[0].PointY.value = -5.0
    $procgenOutboundTrigger.Geometry.value[1].PointX.value = 5.0
    $procgenOutboundTrigger.Geometry.value[1].PointY.value = -5.0
    $procgenOutboundTrigger.Geometry.value[2].PointX.value = 5.0
    $procgenOutboundTrigger.Geometry.value[2].PointY.value = 5.0
    $procgenOutboundTrigger.Geometry.value[3].PointX.value = -5.0
    $procgenOutboundTrigger.Geometry.value[3].PointY.value = 5.0
    # Keep the transition polygon just above the dgt04 walkmesh.  A polygon at
    # Z=0 is rendered/intersected around the character's feet instead of on
    # the floor by the NWN trigger system.
    foreach ($point in $procgenOutboundTrigger.Geometry.value) {
        $point.PointZ.value = 0.02
    }
    $procgenSign = $arrivalElevator | ConvertTo-Json -Depth 100 | ConvertFrom-Json
    $procgenSign.Tag.value = "barrens_pgen_sign"
    $procgenSign.TemplateResRef.value = "_mdrn_pl_exitsi"
    $procgenSign.Appearance.value = 21080
    $procgenSign.LocName.value.'0' = "Generated Barrens Street - Walk East"
    $procgenSign.X.value = 69.0
    $procgenSign.Y.value = 45.0
    $instances.WaypointList.value = @($instances.WaypointList.value) + $procgenReturnWaypoint
    $instances.TriggerList.value = @($instances.TriggerList.value) + $procgenOutboundTrigger
    $instances.'Placeable List'.value = @($instances.'Placeable List'.value) + $procgenSign

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
        "barrens_strip",
        "barrens_pgen40",
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

    # Erie currently includes the full shared SWLOR HAK stack (D25). The minimal
    # allowlist was reverted because committed content (the Barrens street's frontage
    # and street-dressing placeables, and every creature/item P2c/P2d will place)
    # resolves its models from the shared placeable/creature/item HAKs, not just the
    # tileset HAKs. The list is derived from the reference module so the two stay in
    # parity; trimming to a demonstrated subset returns as a pre-release provenance gate.
    $referenceModuleInfo = Get-Content -LiteralPath (Join-Path $repoRoot "Module\ifo\module.ifo.json") -Raw |
        ConvertFrom-Json
    $moduleHaks = @($referenceModuleInfo.Mod_HakList.value | ForEach-Object { $_.Mod_Hak.value })
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

    # Placeable blueprints (.utp): a .git placeable INSTANCE references its blueprint by TemplateResRef,
    # and the packaged module must carry every referenced blueprint or the placeable does not spawn/
    # render (the reference module packs 8000+; a fresh module that ships only starter items renders no
    # dressing at all). Mirror the blueprints committed areas reference from the reference module, the
    # same way ncs/nss are mirrored. Blueprints with no file under Module\utp are base-game and are
    # provided by the engine, so they need no copy.
    $referencedBlueprints = @{}
    Get-ChildItem -LiteralPath (Join-Path $moduleRoot "git") -Filter *.git.json | ForEach-Object {
        $gitInstances = Get-Content -LiteralPath $_.FullName -Raw | ConvertFrom-Json
        foreach ($placeable in @($gitInstances.'Placeable List'.value)) {
            $blueprintResref = $placeable.TemplateResRef.value
            if ($blueprintResref) { $referencedBlueprints[$blueprintResref] = $true }
        }
    }
    foreach ($blueprintResref in $referencedBlueprints.Keys) {
        $blueprintSource = Join-Path $repoRoot "Module\utp\$blueprintResref.utp.json"
        $blueprintDest = Join-Path $moduleRoot "utp\$blueprintResref.utp.json"
        if ((Test-Path -LiteralPath $blueprintSource) -and -not (Test-Path -LiteralPath $blueprintDest)) {
            Copy-Item -LiteralPath $blueprintSource -Destination $blueprintDest
        }
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

Write-Host "Prepared ModuleSR: clean Erie arrival, Barrens proof street, isolated infrastructure, starter resources, and shared HAK manifest."

# ModuleSR — the "Erie Metroplex" module

This is the fresh, minimal module the Shadowrun adaptation is built into. It exists so
new content lands on a clean base with **zero Star Wars residue** and boots in seconds
instead of minutes (the SW `../Module` packs to a ~195 MB `.mod`; this one is a few MB).

See [design/shadowrun/packages/P2c.md](../design/shadowrun/packages/P2c.md) and
[design/shadowrun/DECISIONS.md](../design/shadowrun/DECISIONS.md) (D12 revision → fresh
module) for the rationale.

## Two-module layout

The old SW `../Module` **stays in the repo, intact and dormant.** Dozens of tests assert
specific SW areas, creatures, spawns and placements exist, so deleting or stripping it
in place would turn the suite red. Instead the server chooses which module to load with a
single switch:

- `debugserver/swlor.env` → `NWN_MODULE="Erie Metroplex"` (was `"Star Wars LOR v2"`).

Flip that one line to boot the SW module again.

## What's committed here

| Folder | Contents |
|---|---|
| `ifo/` | `module.ifo.json` — a copy of the SW module's event/hak/TLK wiring, with only the **area list** trimmed to ours and **Mod_Name** set to `Erie Metroplex`. Entry area stays `ooc_area`; custom TLK stays `sw_tlk`; all 113 haks retained. |
| `are/`, `git/`, `gic/` | The three system areas the C# needs at boot, plus the four procgen placeholders — see below. |
| `fac/` | `repute.fac` (faction table). |
| `jrl/` | `module.jrl` (journal). |

### Areas

| Area | Why it's here |
|---|---|
| `ooc_area` | Module entry area (`Mod_Entry_Area`); character-creation staging. |
| `czs220_hangar` | Spawn area — holds `ENTRY_STARTING_WP` (players teleport here on enter) and `DTH_DEFAULT_RESPAWN_POINT`. |
| `no_access` | Service area — holds the `MIGRATION_STORAGE`, `TEMP_ITEM_STORAGE`, and `OUTFIT_BARREL` storage placeables the C# looks up by tag. |
| `gen_placeholder1`–`4` | Required by the procedural area generator (D19). |

These three system areas are copied **verbatim** from the SW module. NWN `.git` instances
are fully self-contained, so they carry their objects without needing the 8k+ blueprint
files — which is what keeps this module tiny. Re-theming the spawn into the Barrens is
**P2b's** job, not P2c's.

## What is NOT committed (materialized at pack time)

`ncs/` and `nss/` are mirrored from `../Module` by `PackModuleSR.cmd`, and the empty
resource folders (`utc/`, `uti/`, `utp/`, …) are created by it. All are `.gitignore`d.

## Building

```bat
ModuleSR\PackModuleSR.cmd
```

Produces `Erie Metroplex.mod` and copies it to `debugserver/modules/`. Then boot with
`NWN_MODULE="Erie Metroplex"`.

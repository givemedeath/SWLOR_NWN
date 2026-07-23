# P2c + P2b — Fresh module reset, then build the Barrens

*Detailed brief for Phase 2 of [design/shadowrun/PLAN.md](design/shadowrun/PLAN.md). Two packages,
in order: **P2c** stands up a fresh Shadowrun module; **P2b** builds the Barrens into it with procgen.
On approval these become `design/shadowrun/packages/P2c.md` and `P2b.md`.*

## Context

Procgen (merged, [D19](design/shadowrun/DECISIONS.md)) makes areas cheap to generate, which removes the
one good reason the original hybrid decision ([D12](design/shadowrun/DECISIONS.md)) kept the 195 MB
Star Wars module. Building the Shadowrun game inside that module means permanent SW residue, players
wandering into Tatooine, and ~4-minute packs. So we switch to a **fresh clean module**, and do it
**before** building the Barrens so content lands on the clean base once, not twice.

**Key constraint that dictates the approach:** the test suite is deeply coupled to the SW module —
dozens of tests assert specific SW areas, creatures, spawns, rares, and placements exist. A
destructive strip-in-place would break all of them. So we use a **two-module layout**: the SW
`Module/` stays in the repo as dormant reference (its tests keep passing), and a new minimal module is
built beside it and made live via the single `NWN_MODULE` switch.

---

## P2c — Fresh Shadowrun module (do first)

Stand up a new module — the **Erie Metroplex** (no "Shadowrun" in the shipped module name, per the IP
caution in D12) — containing only SWLOR's system scaffolding plus a start area. Everything Star Wars is
left behind.

### What the fresh module must contain (all confirmed against the C#)

| Need | Source |
|---|---|
| `module.ifo` — event-script wiring (`Mod_OnModLoad`, `Mod_OnClientEntr`, …) routing to the C# handlers; `Mod_Name`; entry area | copy the event fields from `Module/ifo/module.ifo.json`, repoint `Mod_Entry_Area` to our start area, empty the `Mod_Area_list` down to ours |
| NWScript event stubs (`ncs`/`nss`) the `.ifo` scripts reference | copy from `Module/ncs`, `Module/nss` |
| Runtime payload (`config.json`, the .NET DLLs the module carries) | copy from `Module/` |
| A **start / character-entry area** with `ENTRY_STARTING_WP` and `DTH_DEFAULT_RESPAWN_POINT` waypoints | a small procgen `facility` interior (or a minimal hand area), plus the two waypoints in its `.git` |
| The service objects the C# looks up — the Shadowrun-relevant subset: `MIGRATION_STORAGE`, `TEMP_ITEM_STORAGE`, `OUTFIT_BARREL` (drop `STARSHIP_DOCKPOINT`, `FORCE_QUEST_LANDING`) | placeables in the start area's `.git`; blueprints copied from `Module/utp` |
| Starting-gear blueprints the C# `CreateItemOnObject`s (~5: `survival_knife`, `fresh_bread`, `travelers_clothes`, …) | copy those specific `Module/uti` blueprints, or swap the C# to simple Sixth-World starting gear |
| `gen_placeholder1-4` areas (procgen needs them) | copy from `Module/are`,`Module/git` |

### Wiring the switch
- `debugserver/swlor.env`: `NWN_MODULE="Erie Metroplex"`.
- A `PackModuleSR.cmd` (mirror of `Module/PackModule.cmd`) packing the new module dir, or repoint the
  existing script.
- New module source dir (e.g. `ModuleSR/` with the same `are/git/utc/uti/utp/ifo/ncs/nss/...` layout).

### Test posture
The SW `Module/` stays intact, so every existing SW-content test keeps passing untouched. No test
breakage; the reset adds no red. New Shadowrun content gets its own tests later.

### Gate
Server boots on the Erie Metroplex module; a new character is created, spawns at `ENTRY_STARTING_WP`
with starting gear, and the C# systems (skills, perks, the character sheet in Shadowrun terms) work —
in a module with zero Star Wars content.

---

## P2b — Build the Barrens (onto the fresh module)

Unchanged in intent from the prior draft, retargeted to the new module. Procgen used in **static
mode**: generate → curate → commit fixed areas → hand-wire content (the Barrens is a persistent place,
not a randomized run; runtime on-demand generation stays dormant for later runs).

### Pipeline (confirmed by reading the exporter)
`SWLOR.ProcgenReview` writes `.are.json`/`.git.json` **natively** to a temp stage
([Program.cs:644/687](SWLOR.ProcgenReview/Program.cs)) before packing. Add a **`--json-out <dir>`
flag** (~10 lines) to copy that JSON out before it is converted to GFF and the stage is deleted —
yielding module-ready area files directly. (Fallback: `--erf` then `tools/SWLOR.CLI/nwn_gff.exe`
GFF→JSON.)

### Steps
1. Generate ~4 areas from the already-onboarded profiles: 2–3 `sewer` undercity (tds01, size ~12–16)
   + 1–2 `facility` interiors for the clinic and bar. Record the seeds (deterministic).
2. Curate with `ContentBuilder` (renders minimaps, no server) and/or an in-game look; pick the best.
3. Commit chosen `.are.json`/`.git.json` into the new module; rename to the Barrens roster
   (`brn_undercity1`, `brn_clinic`, `brn_bar`, `brn_turf`); register in the module's `.ifo`; set the
   Barrens mood (dark, wet, sodium-lit) on lighting/fog.
4. Wire content (SWLOR-native data): the cyberdoc NPC (existing `CyberdocDialog`) in the clinic; a
   fixer NPC (Ledger) + new `FixerDialog` and a bartender in the bar; `BarrensSpawnDefinition`
   (`SpawnTableBuilder`) with Rustkings ganger blueprints on the turf; `LinkedTo` transitions anchored
   to the generated `TransitionPoint`s; one entrance from the start area.
5. Pack + walk-test.

### Phase B — streets (after the undercity gate)
Onboard one urban-exterior tileset (`fcx01` futuristic-city, or `srt04` the Shadowrun set) as a
`DungeonTilesetProfile` (`DungeonTilesetProfileBuilder`; validated by `OnboardedTilesetPipelineTests`)
— cataloging its tile inventory into terrain/feature-tiles/set-pieces/exit-groups. Then compose with
the `Streets` layout to generate The Strip and above-ground Barrens, committed as in Phase A. This is
the one real new effort and is gated behind the undercity proof.

---

## Files / artifacts

| File | Change |
|---|---|
| `ModuleSR/` (new module tree: `ifo/git/are/utc/uti/utp/ncs/nss/config.json/...`) | **new** — the fresh Erie Metroplex module |
| `debugserver/swlor.env` | `NWN_MODULE` → Erie Metroplex |
| `PackModuleSR.cmd` | **new** — pack the new module |
| `SWLOR.ProcgenReview/Program.cs` | add `--json-out` flag |
| `ModuleSR/are/brn_*.are.json`, `ModuleSR/git/brn_*.git.json` | generated + themed Barrens areas |
| `SWLOR.Game.Server/Feature/SpawnDefinition/BarrensSpawnDefinition.cs` | Rustkings spawn table |
| `SWLOR.Game.Server/Feature/DialogDefinition/FixerDialog.cs` | the fixer |
| `ModuleSR/utc/*.utc.json` | Rustkings + fixer/bartender blueprints |
| `design/shadowrun/districts/barrens.md` | record chosen seeds + area resrefs |

## Reuse (do not rebuild)

- `SWLOR.ProcgenReview` + `SWLOR.ContentBuilder` (just merged); `sewer`/`facility` profiles + `Warren`/
  `Streets` layouts (onboarded)
- `CyberdocDialog` (exists); `SpawnTableBuilder`; `DialogBase`; `tools/SWLOR.CLI/nwn_gff.exe`
- The SW `Module/` as the copy source for scaffolding (`.ifo` events, `ncs/nss`, service blueprints,
  starting gear, placeholders)
- Barrens roster in [districts/barrens.md](design/shadowrun/districts/barrens.md); setting in
  [D18](design/shadowrun/DECISIONS.md)

## Verification

- **Unit:** full suite stays green (SW `Module/` intact → SW-content tests unaffected); the grafted
  AreaGeneration suite stays green; `--json-out` output asserted to parse as `.are.json`/`.git.json`.
- **P2c gate (in game):** boot the Erie Metroplex module; create a character; spawn with gear; the
  Shadowrun-termed sheet and systems work — no Star Wars anywhere.
- **P2b gate (in game):** walk the Barrens undercity; it reads as the Sixth World; transitions connect;
  the cyberclinic is reachable and a troll street sam chromes up; Gang Turf spawns Rustkings and a
  fight exercises the retuned soak, wounds, and glitches (folding in the P1a/1b/1c/P5/P6 gates); the
  fixer greets the player.

**Gate:** a clean Shadowrun module you spawn into, containing one walkable Barrens undercity with the
cyberclinic and a real fight — the kill-criteria proof, on a fresh base, via generated geometry.

## Out of scope

Deleting the SW `Module/` from the repo (kept as dormant reference); streets/above-ground (Phase B);
runtime on-demand generation; the full 30–50 area district; authored runs (Phase 3); new tileset art.

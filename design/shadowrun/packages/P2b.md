# P2b — Barrens proof-street area build

**Status:** accepted 2026-07-23 — first exterior re-themed and live mood review passed
**Depends on:** P2m (accepted), P2a
**Blocks:** P2c, P2d, Phase 3

## Progress — 2026-07-23

The **first exterior is built and committed**: `barrens_strip`, The Strip (proof-street area 1),
re-themed from the finished `dgt04` hand-built slum `pw_ar_narslum`, then committed as static module JSON
([D26](../DECISIONS.md); full record in [districts/barrens.md](../districts/barrens.md)). It is
registered in `module.ifo`, bidirectionally linked to `erie_arrival` (Type-1 area-transition
triggers, loop-free placement), retains 1,288 static placeables across 167 blueprint resrefs, and
removes legacy labels, sounds, and spawn waypoints. Structural regressions live in
`BarrensProofStreetTests` (area registration, stable resrefs / no procgen residue, closed transition
graph, required waypoints, no-loop clearance, HAK/tileset closure, creatures/shops deferred).
Module packed (6.87 MB) and the release manifest regenerated.

**Per the "finish the first exterior before expanding the route" directive, areas 2–6 are not yet
generated.** They follow once the live mood/walk gate below accepts the Strip's look and performance.
Combat creatures remain P2c; service/story NPCs remain P2d.

**First live feedback (2026-07-23):** the operator could not find the arrival exit. Two fixes: a
visible **elevator** landmark (`barrens_elevator`) now sits on the outbound trigger, and — the deeper
cause — Erie reverted to the **full shared HAK stack** ([D25](../DECISIONS.md)) because the minimal
allowlist rendered no placeable models (the exit marker and the Strip's 631 street-dressing placeables
were all invisible). Module repacked and redeployed with 113 HAKs.

**Rendering confirmed live (2026-07-23):** after packaging the placeable blueprints, the arrival
elevator and the Strip's 631 dressing placeables render in-game.

**Acceptance evidence (2026-07-23):** operator walked the deployed route and judged the dgt04 Strip's
Barrens mood good. The visual kill-criteria question passed. Download / boot / transition-latency /
frame-time measurements remain useful operational notes, but do not block the next proof-street areas.

**Procgen comparison candidate:** `barrens_pgen40` is also installed as a separate 40×40 dgt04
candidate (profile `modernex`, Packed layout, seed `20260723`, 253 decorations). It is reachable from
the second labeled arrival sign and returns to arrival. This is a feasibility review area only; it does
not replace the accepted authored Strip.

## Scope

Build only the 5–8 area proof street defined in P2a. The first exterior is finished to release
quality before the remaining geometry is generated.

## Pipeline

1. Select one supported tileset/profile and record the dependency in the ModuleSR HAK allowlist.
2. Generate deterministic candidates with `SWLOR.ProcgenReview --json-out`; record profile, layout,
   seed, size, feature/decor flags, and tool commit.
3. Render or inspect every candidate; reject broken navigation, repetitive silhouettes, poor
   transitions, and unusable encounter space.
4. Commit selected `.are.json`/`.git.json` under stable Barrens resrefs. Generated geometry becomes
   static world content; the district does not use runtime random generation.
5. Apply the accepted mood standard: lighting, fog, weather, sound, signage, prop density, and
   landmarks. Avoid setting-specific text embedded in geometry where a data-backed name can be used.
6. Wire bidirectional transitions and the critical route before decorative content.
7. Add only generic environmental instances. P2c owns combat creatures; P2d owns service NPCs.
8. Run structural tests, pack Erie, create a release manifest, and walk the whole route.

## Required artifacts

- committed ModuleSR area/instance JSON;
- registered IFO areas and explicit HAK dependencies;
- `design/shadowrun/districts/barrens.md` seed/resref/route table;
- screenshots for the finished-street review;
- transition graph and walk-test record;
- download size, pack time, boot time, transition latency, and observed frame-time notes.

## Gate

A new tester enters from `erie_arrival` and walks the entire critical route without teleportation,
instructions, broken transitions, or obvious legacy content. The finished street reads as Erie’s
Sixth World-inspired setting without a lore explanation. It has clear landmarks, encounter space,
service delivery points, recovery route, and acceptable client/server performance.

Do not generate the rest of the district until this gate is accepted.

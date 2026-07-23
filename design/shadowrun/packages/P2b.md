# P2b — Barrens proof-street area build

**Status:** ready after P2m live acceptance
**Depends on:** P2m, P2a
**Blocks:** P2c, P2d, Phase 3

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

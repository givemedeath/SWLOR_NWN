# Procedural Area Generation — Assessment for the Barrens

**2026-07-22 · verdict: usable, and it changes the Phase 2 approach**

Assessment of `zunath/feature/procedural-areas` against the question: can it generate the Barrens
proof-street areas, removing the toolset constraint that shaped [P2a](packages/P2a.md)?

## Verdict

**Yes — decisively, and it is better than the re-theme plan.** The branch is not the modest cave
generator its design doc describes; it has grown into a **mature, actively-developed urban generator**
that produces area geometry as data, with an **offline exporter that needs no running server**. Its
newest commit (2026-07-21) is literally *"Make city streets read as painted avenues and ground every
building on real platform."* City-street generation is a first-class, polished feature.

This dissolves the constraint P2a was built around — "area geometry is toolset (GUI) work I cannot
do." Procgen generates geometry as `.are`/`.git` files.

**One honest nuance on tilesets** (below): street *exteriors* on a Shadowrun-looking tileset need a
one-time tileset-onboarding step; undercity/sewer/facility areas generate on already-onboarded
tilesets today.

## What the branch actually is

- **~121 implementation files** under `Service/AreaGenerationService/` plus a full test suite. Not a
  spike — a system: a tileset adjacency solver, a macro-layout solver, an area synthesizer, decoration
  planners, building-frontage planners, group stampers, edge-crosser resolution, elevation painters.
- **Urban primitives are implemented and tested**, not deferred: `CityBlockContiguity`,
  `BuildingFrontageComposition`, `PlazaRingStreet`, `PromenadeBandReliability`, `FenceAndAlley`,
  `DistrictDressing`, `EdgeCrosserResolution`, `GroupStamper`. Streets are made of exactly these.
- **A "City Streets" layout profile** (`StandardLayoutProfiles.Streets`, "Exterior city blocks joined
  by wall-embedded Alley crosser tunnels") — a purpose-built street/district macro-layout.
- **Clean theme authoring** via `DungeonDefinitionBuilder`: `.TilesetProfile().LayoutProfile()
  .SizeRange().Tier().AddCreature().Boss().Treasure()`. The shipped `SewerDungeonDefinition` is
  already "undercity drainage tunnels crawling with vermin and scavengers" — Barrens undercity in all
  but name.
- **An offline exporter** — `SWLOR.ProcgenReview` (`dotnet run --project SWLOR.ProcgenReview`) — that
  "builds a standalone review module... without the full SWLOR module," and with `--erf` "packs the
  same generated areas into a standalone ERF containing only `.are/.git`, importable directly via the
  toolset." No server, no NWNX at generation time. There is also a WPF `SWLOR.ContentBuilder` GUI.

## The tileset reality

Layout (macro shape) and tileset (what renders it) are separate. The **Streets layout** exists; what
determines the *look* is which tileset profile it composes with.

| Onboarded profile today | Tileset | Barrens use |
|---|---|---|
| Sewers | `tds01` (`sw_t_sewer`) | Undercity tunnels, drainage — immediate |
| Facility | `zsf01` (`sw_t_scifibase`) | Interiors: clinic, corp rooms — immediate |
| AncientRuin | `vmr01` | Proves exterior "streets" generation works |
| Cavern | `tdt01` | Natural caves |

No **urban-exterior** tileset (the Shadowrun `srt04`, futuristic-city `fcx01`, or base city `tcn01`)
ships as a profile yet. Onboarding one is a defined pipeline (`DungeonTilesetProfileBuilder`,
`OnboardedTilesetPipelineTests`) but real per-tileset work: build the terrain/edge/corner adjacency
profile and validate solve rate. So:

- **Now, no onboarding:** Barrens **undercity / sewer / interior** areas — real, walkable, generated.
- **After onboarding one urban tileset:** proper **street exteriors** with a Sixth-World look.

## Integration paths, cheapest first

| Path | What | Cost | Risk |
|---|---|---|---|
| **C. Offline output, out-of-tree** | Run `ProcgenReview` on the procgen branch in a worktree; copy generated `.are/.git` into our module; layer our content on top | Low | Offline skips path validation → walk-test in game |
| **B. Graft the subsystem** | Cherry-pick `AreaGenerationService/` + `DungeonDefinition/` + `ProcgenReview` + `TilesetPlugin` onto our branch | Medium | Depends on API wrappers/`basegame_sets` also landing; some conflict |
| **A. Full branch merge** | Merge `feature/procedural-areas` into `adaptation/shadowrun` | High | Real conflicts in the combat files we rewrote (soak, glitches, metatype, cyberware); brings runtime NWNX_Tileset dependency |

**Path C needs no merge at all.** We don't integrate the procgen code; we use its offline *output* as
static content. The generated areas become plain module areas with no procgen runtime dependency.
Runtime procgen (on-demand dungeons at level cap) is a separate, later prize that would want Path A/B
and the NWNX_Tileset spike the design doc flags.

## Recommended plan for the Barrens proof street

**Use Path C.** Concretely:

1. **Worktree the procgen branch** (`git worktree add` on `zunath/feature/procedural-areas`) so its
   build is isolated from ours.
2. **Generate a Barrens undercity block** now, no onboarding: run `ProcgenReview --erf` with the
   `sewer`/`facility` tileset profiles and the `Streets`/`Warren` layouts, a couple of seeds, sizes
   ~12–16. Output: `.are/.git` geometry.
3. **Bring the chosen areas into our module** (`Module/are`, `Module/git`), rename resref/tag/name to
   the Barrens roster ([districts/barrens.md](districts/barrens.md)), set the mood (lighting/fog).
4. **Layer Barrens content as data** (my wheelhouse): the cyberdoc + fixer NPCs and dialogs (the
   cyberdoc dialog exists), a `BarrensSpawnDefinition` with Rustkings gangers, `LinkedTo` transitions
   between areas, the exit.
5. **Walk-test** in game (this is where offline's skipped path-validation gets covered) — folding in
   the P1a/1b/1c combat gates as P2a already planned.
6. **Decide on street exteriors:** if the undercity proof convinces, onboard one urban tileset
   (`fcx01` futuristic-city is the strongest Sixth-World look and already appears in the branch's
   decoration references, or `srt04` the literal Shadowrun set) to generate above-ground streets. That
   is the one real new effort, and it is optional until the undercity proves the pipeline.

This **supersedes the re-theme approach in P2a**: instead of re-skinning Star Wars areas, we generate
fresh Barrens geometry on urban/undercity tilesets. The rest of P2a (roster, content, the gate) stands.

## Risks & caveats

- **Offline skips engine path validation** — generated areas are visually complete but not
  traversal-QA'd; the in-game walk is mandatory, not optional.
- **Tileset onboarding is real work** for street exteriors; the undercity path avoids it for the first
  proof.
- **The branch is a moving target** (daily commits) and based on the canonical combat-upgrade lineage,
  which our branch forked and heavily edited — a reason to prefer Path C (no merge) now and treat
  runtime integration (Path A/B) as a deliberate later project.
- **NWNX_Tileset** is only needed for *runtime* generation; the offline path does not touch it.

## Bottom line

Procgen can generate the Barrens areas, and the offline-output path lets us do it **without merging
anything** — a strictly better Phase 2 starting point than re-theming. The immediate, zero-onboarding
win is the Barrens **undercity**; street exteriors follow one tileset-onboarding step later.

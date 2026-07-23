# P2m — Erie module and runtime foundation

**Status:** accepted 2026-07-23; live cold-boot, metatype chargen, cyberclinic, and lifecycle matrix passed
**Depends on:** procgen graft (D19)
**Blocks:** P2b and any external playtest

## Purpose

Provide a clean, reproducible deployment boundary before district content is built. `ModuleSR/` is
the only supported runtime module on the adaptation branch. The historical `Module/` remains a
reference/regression fixture, not a world selected by flipping one environment line.

## Implemented contract

- `debugserver/swlor.env` selects `NWN_MODULE="Erie Metroplex"`,
  `SWLOR_GAME_PROFILE=shadowrun`, and `SWLOR_DATA_NAMESPACE=erie`.
- Shadowrun profile startup fails without a namespace.
- Redis entity keys use `erie:<EntityType>:<id>` and indexes use `erie_<EntityType>`.
- `erie_arrival` is deterministically generated from Sci-Fi Base seed `4242`, committed as static
  JSON, and contains `ENTRY_STARTING_WP` plus `DTH_DEFAULT_RESPAWN_POINT`.
- Character creation exposes exactly Human, Elf, Dwarf, Ork, and Troll with male/female portraits.
- New characters receive packaged `survival_knife`, `fresh_bread`, `travelers_clothes`, and a
  20,000-credit prototype stipend.
- `no_access` contains only runtime storage placeables; its inherited legacy creature is removed.
- The module references 9 HAKs: shared mechanics/UI/VFX resources and only the five tilesets its
  committed areas require.
- `tools/PrepareShadowrunModule.ps1` regenerates the arrival and enforces the manifest.
- `PackModuleSR.cmd` packs/deploys and invokes `tools/WriteShadowrunReleaseManifest.ps1`, which hashes
  the module, custom TLK, server assembly, HAKs, and source revisions.

“Clean” is an observable content boundary: no player-visible Star Wars area, actor, conversation,
race, or undeclared HAK dependency. Shared engine history and generic reusable assets remain; this is
not a legal-clearance claim.

## Automated verification

- exactly five `PlayerRace=1` rows and portrait coverage for both sexes;
- entry area/list/waypoints and all literal starter resources exist;
- HAK manifest is at most 9 entries and covers every committed area tileset;
- private service area has zero creatures;
- profile parsing, namespace validation, Redis key/index construction, and stipend policy;
- HAK/TLK guard, module pack, and release-manifest generation.

## Acceptance gate

Use an empty Erie keyspace and a clean client:

1. cold boot the packed module and archive its release manifest and server log;
2. create male and female characters for all five metatypes;
3. enter at `erie_arrival` with all three items and 20,000 credits;
4. open the character sheet and cyberclinic, install/remove one implant;
5. reconnect, die, respawn, and reconnect again;
6. inspect for missing resources, `Bad Strref`, legacy content, background-service noise, and writes
   outside the `erie:*` namespace;
7. restore from backup and repeat one reconnect.

Record actual results in `LEDGER.md`. Until this passes, P2m is implemented—not accepted.

**Live progress, 2026-07-23:** Erie loads successfully and a previously created Human can log in and
walk the arrival area. A Troll reached chargen but initially had no selectable background/class.
Investigation found all four newly enabled fantasy races missing from `cls_pres_stand.2da` and
`cls_pres_force.2da`; all five metatypes are now explicitly permitted and covered by regression tests.
The new Troll creation path works. The following inspection found that the
debug server still held an older `sw_2da.hak`, whose legacy Star Wars species
were flagged `PlayerRace=1`; the current built artifact contains only Dwarf,
Elf, Ork, Human, and Troll. `PackModuleSR.cmd` now deploys the exact generated
HAK/TLK artifacts before generating the release manifest, preventing a stale
debugserver copy from reintroducing the race list.

**Acceptance, 2026-07-23:** The live matrix was confirmed against the deployed
Erie payload: the five-metatype character-creation path, arrival/login flow,
starter-state verification, character sheet, and cyberclinic install/removal
behaved correctly. P2m no longer blocks P2b; the next implementation package
is the finished Barrens proof street.

## Deferred intentionally

- a production economy replacing the stipend;
- runtime-service allowlisting (required before external playtest);
- public-release asset/license review;
- Barrens areas, creatures, NPCs, and runs.

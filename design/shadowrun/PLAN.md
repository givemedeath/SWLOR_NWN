# Erie Metroplex Adaptation — Plan of Record

**v3 · 2026-07-23 · supersedes v2 and the presentation-layer plan**

This is the authoritative delivery plan for turning SWLOR's engine and reusable systems into a
playable, non-commercial Sixth World-inspired persistent-world vertical slice. Historical work is
recorded in [LEDGER.md](LEDGER.md); architectural decisions and their evidence are in
[DECISIONS.md](DECISIONS.md). Old presentation packages remain useful history, but they do not define
current completion.

## Outcome

The target is one polished Erie district in which a small party can:

1. create one of five metatypes;
2. make a meaningful chrome-versus-magic build decision;
3. meet a fixer, perform a structured run, get paid, and recover;
4. find repeatable work without a GM;
5. participate in richer live events when a GM is present.

The target is a vertical slice, not SWLOR-scale content and not a faithful tabletop rules simulator.
Percentage resolution remains under the hood. Identity systems—metatypes, Essence, cyberware, Drain,
Matrix access, and rigging where admitted by their gates—must create real choices rather than labels.

## Current truth

“Implemented” below means code/data exists and automated checks pass. “Accepted” requires the stated
in-game gate. No package is called complete merely because it builds.

| Capability | State | Evidence still required |
|---|---|---|
| Shadowrun display layer and retuned personal combat | Implemented | consolidated live combat gate |
| Metatypes: Human, Elf, Dwarf, Ork, Troll | Implemented | create both sexes for all five; inspect models and derived stats |
| Cyberware, Essence, five seed implants, clinic NUI | Implemented | install/remove with real character funds; verify Magic loss and combat effects |
| Glitches and wound penalties | Implemented | repeated player/NPC combat with log and VFX inspection |
| Clean Erie module foundation (`P2m`) | Implemented; boot/existing-human login passed | complete new-character metatype matrix, reconnect, respawn, and inspect distribution |
| Barrens district design (`P2a`) | Designed | one finished street mood gate |
| Barrens area build (`P2b`) | Accepted (`barrens_strip`, dgt04 slum source) | build areas 2–6; retain operational perf notes |
| Creature set (`P2c`) | Not started | encounter balance and visual-language gate |
| NPCs, shops, fixers (`P2d`) | Not started | service loop works in-world |
| Authored and repeatable runs | Not started | staffed and unstaffed session gate |
| Magic/Drain, Matrix, rigging | Deferred prototypes | separate feasibility gates before production scope |

The old `Module/` remains in the repository as a read-only-in-practice reference and regression
fixture for inherited tests and assets. On this branch it is **not a supported second runtime**.
`ModuleSR/` is the only deployable game module. The shared assembly retains a `starwars` profile
default for historical tools and existing deployments, but this branch does not promise that Erie
combat/content changes preserve a playable Star Wars world.

## Locked architecture

| Decision | Rule |
|---|---|
| Runtime | `SWLOR_GAME_PROFILE=shadowrun`; boot fails if Erie has no data namespace |
| Persistence | `SWLOR_DATA_NAMESPACE=erie`; entity keys and RediSearch indexes are isolated |
| Module | `ModuleSR/` contains only committed Erie areas/resources plus shared script scaffolding |
| Assets | Erie ships the full shared SWLOR HAK stack for now ([D25](DECISIONS.md)); the minimal allowlist ([D22](DECISIONS.md)) returns as a pre-release provenance/download-size gate |
| Releases | every packed module emits SHA-256 hashes for module, custom TLK, server assembly, HAKs, and source revisions |
| Content | new district and characters; inherited systems are enabled only as their player-facing data is converted |
| Fidelity | Shadowrun-flavored mechanics, not a dice-pool rewrite |
| Setting | original Erie city, original factions/plot; setting vocabulary remains containable and renameable |
| Team | solo direction with agents handling bounded mechanical work; feel, balance, and voice remain human gates |

“Clean” means no player-visible Star Wars areas, actors, conversations, races, or undeclared HAK
dependencies. It does not mean the shared open-source engine contains no historical identifiers.
Likewise, the asset and naming boundary is engineering risk control, not a legal conclusion.

## Delivery sequence

### Foundation — `P2m` · implemented, live gate pending

The foundation exists before more content is admitted:

- deterministic `erie_arrival` entry and default respawn;
- five playable metatypes and complete male/female portrait coverage;
- packaged starter knife, food, and street clothes;
- 20,000-credit prototype stipend so the cyberware gate is reachable;
- isolated Redis entity/index namespace;
- HAK manifest (P2m shipped a 9-entry allowlist; reverted to the full shared stack in [D25](DECISIONS.md) so placeable/creature/item models render — minimal allowlist deferred to a pre-release gate);
- reproducible preparation and release-manifest scripts;
- legacy OOC/hangar content removed; private service area contains storage only.

**Gate:** from a cold server and empty `erie:*` keyspace, create each metatype, enter with all starter
items and 20,000 credits, reconnect, die/respawn, open the character sheet and cyberclinic, and confirm
that no player-visible Star Wars content or missing-resource errors appear. Archive the release
manifest and the relevant server log. The stipend is test scaffolding; economy acceptance must replace
it before public launch.

### Phase 1 — Identity · consolidate and accept

| ID | Package | State | Acceptance |
|---|---|---|---|
| `1a` | Metatypes | implemented | creation/model/stat matrix passes |
| `1b` | Cyberware + Essence | implemented | chrome changes stats and Magic exactly as displayed |
| `1c` | Glitches | implemented | success/failure variants work for players and NPCs |
| `1d` | Attack/Delay vocabulary | open | initiative wording agrees with the actual pass model |
| `1e` | Character-creation and onboarding flow | partial | player understands metatype, Essence, stipend, and next destination without external explanation |

**Consolidated gate:** create a troll street samurai with Wired Reflexes and a dwarf mage, fight the
same calibrated enemy before and after chrome, inspect soak/wounds/glitches, and confirm every displayed
number and term describes the behavior observed. Record time-to-first-decision and every point where a
tester asks what to do.

### Phase 2 — Place · prove one street before a district

| ID | Package | Deliverable |
|---|---|---|
| `2a` | District design | canonical Barrens map, names, routes, service placement, run hooks |
| `2b` | Area build-out | 5–8 area proof street generated statically, curated, then committed |
| `2c` | Creature set | gangers, corp security, drones, critters, and spirits with role/readability matrix |
| `2d` | NPCs, shops, fixers | cyberdoc, fixer/Johnson, merchants, transition and spawn wiring |

Use the offline procgen path: generate with recorded seed → render/inspect → curate → commit fixed
`.are.json`/`.git.json` → hand-place content → pack and walk. Runtime random generation is not part of
the district. The first finished exterior determines lighting, fog, signage, density, and prop
standards before the remaining areas are produced.

**Gate:** a new tester walks arrival → street → bar → clinic → combat area → home route without DM
teleportation or instructions. The street reads as Erie’s Sixth World-inspired setting without a lore
brief, all transitions work both ways, and a four-role encounter is legible and performant.

**Kill criterion:** if one fully finished street still does not communicate the intended world, stop
area expansion and reconsider the presentation or platform. Do not build thirty mediocre areas to
avoid answering this.

### Phase 3 — Loop · one complete run before a catalogue

Three content tiers are complementary:

| Tier | Purpose | Availability |
|---|---|---|
| Authored runs | narrative/content floor | always |
| Contract board | repeatable progression and income | always |
| GM events | consequence, surprise, live story | when staffed |

| ID | Package | Required work |
|---|---|---|
| `3a` | Run vertical slice | one Johnson → legwork → objective → payout → cooldown/repeat path |
| `3b` | Objective vocabulary | declarative reach-location, use-object/hack, defend, and extract/follow |
| `3c` | Contract board | generate offers from a converted item/enemy/location allowlist; not a text-only reskin |
| `3d` | GM event kit | bounded spawn kits, faction levers, staged encounters, cleanup |
| `3e` | Heat / street cred | faction standing with visible consequences and decay policy |
| `3f` | Downtime economy | sources/sinks, cyberware affordability, death/recovery, market/crafting visibility |

The first run must define party ownership, late join, disconnect/reconnect, abandonment, instance
cleanup, reward idempotency, failure, retry, and GM intervention. Those are system requirements, not
polish. Only after this run passes should the authored roster expand toward 8–12.

**Gate:** two or three players complete the run with a GM, then return without a GM and find worthwhile
repeatable work. A second test deliberately disconnects, rejoins, fails, retries, and attempts to claim
the payout twice.

### Phase 4 — Depth · prototype before commitment

| ID | Prototype gate | Production work only if gate passes |
|---|---|---|
| `4a` | one spell demonstrates Magic cost, Drain, recovery, and Essence interaction | spell catalogue and magical roles |
| `4b` | one Matrix room supports entry, one meaningful action, opposition, exit, and failure recovery | Matrix content and progression |
| `4c` | one drone supports deploy, command, loss, reconnect, and ownership recovery | rigging and drone catalogue |
| `4d` | first district retains players and the content pipeline is sustainable | additional districts |

`Space.cs` and droids are architectural references, not estimates. A subsystem’s line count does not
measure its UI, persistence, content, recovery, balance, or operational burden.

## Cross-cutting gates

These are release requirements even though they are not feature phases.

### Economy

Before public testing, replace the 20,000-credit prototype stipend with a documented model covering
starting resources, run payout bands, cyberware/consumable costs, repair/death costs, crafting inputs,
market sources/sinks, inflation telemetry, and exploit limits. Test “time to first meaningful implant”
and “time to recover after a failed run,” not just prices in isolation.

### Runtime service allowlist

Inventory all auto-registered services and handlers under the Shadowrun profile. Classify each as
enabled, converted, dormant-safe, or blocked. A dormant service must have no scheduled work,
player-facing UI, external broadcast, or missing-object noise. Complete this audit before the first
external playtest; do not assume defensive null checks equal isolation.

### Operations and performance

Define cold-start, pack, client-download, area-transition, reconnect, backup/restore, release rollback,
log review, and incident ownership. Record budgets for HAK download size, server boot time, transition
latency, generated-area memory, and party concurrency. The release manifest is the artifact boundary;
a dirty manifest is acceptable for local development but not a release candidate.

### Accessibility and onboarding

Test at supported UI scales and common resolutions. Do not rely on color alone for wounds, glitches,
hostility, or Essence warnings. A new player must reach their first actionable choice and first run
without a wiki or a GM.

### IP and asset provenance

Maintain an inventory for every shipped name, text, logo, portrait, model, texture, sound, and music
asset with source, license/permission, modification status, and replacement owner. No claim of legal
clearance is made by this repository. Before any public release, obtain qualified review or ship under
an original setting/terminology plan with the same mechanics.

## Verification policy

Every package supplies:

- focused automated tests and then a justified broad suite;
- HAK/TLK checks when asset sources change;
- a packed Erie module and release manifest when module resources change;
- a clean-start test against an empty Erie namespace when persistence changes;
- an in-game gate with observer, build, manifest hash, scenario, expected result, actual result, and
  defects recorded in the ledger.

Automated checks prove structure and invariants. They cannot accept visual mood, onboarding clarity,
combat feel, networking/reconnect behavior, or licensing.

## Immediate critical path

1. `P2m` is accepted: preserve its release manifest and live evidence as the clean-module baseline.
2. Run the consolidated Phase 1 live gate; tune only from recorded observations. **Reconciliation
   (2026-07-23):** P2m's operator evidence covers chargen/login/arrival/character-sheet and the
   cyberclinic (install/remove with the Essence display confirmed) — the *identity* half of the gate.
   Its remaining half is live **combat** — fight the same calibrated enemy before and after chrome and
   inspect soak, wounds, glitches, and Magic loss in play — which is **blocked on P2c**: Erie has no
   creatures to fight yet. This is a distinct still-open gate, not a completed one, and it does not
   block P2b preparation.
3. Accept the first Barrens exterior (`2b`, `barrens_strip` — implemented, live walk/mood gate
   pending), then the rest of the proof street, then its minimum creature/NPC set (`2c/2d`). The
   Phase 1 combat gate is best run in Gang Turf once `2c` places the Rustkings, as P2a intended.
4. Ship one complete authored run plus repeatable contract (`3a–3c`) with lifecycle tests.
5. Replace the prototype stipend with the measured economy and complete operations/provenance gates.
6. Expand content only after retention and maintainability justify it.

At solo pace, a credible vertical slice remains a **6–12 month part-time** project. A world at SWLOR’s
scale is years of content production and is explicitly not the objective.

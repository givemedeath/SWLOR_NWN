# P2a — The Barrens: district design + proof street

*Detailed brief for Phase 2a of [design/shadowrun/PLAN.md](design/shadowrun/PLAN.md). On approval this
becomes `design/shadowrun/packages/P2a.md` plus a living design doc at
`design/shadowrun/districts/barrens.md`.*

## Context

Phase 2 is the project's kill criteria: if it does not produce one street that reads as the Sixth
World, the honest answer was Evennia ([PLATFORM-ASSESSMENT.md](design/shadowrun/PLATFORM-ASSESSMENT.md)).
2a is the design that de-risks that gate — done now, while the P1a/1b/1c combat mechanics sit in the
live-test queue, because it is pure design and touches none of the unverified combat code.

**Pipeline reality (the constraint that shapes everything):** area geometry is authored in the NWN
toolset — each `.are.json` is a 64-tile grid whose tile IDs and orientations must interlock, and
that is GUI work I cannot do. But re-theming an existing area (copy its `.are.json`/`.git.json`,
change lighting, fog, name, and populate it) is pure data, and **38 urban-tileset areas already
ship** — Smuggler's Moon slums/docks/hub, an industrial slum, a vertical cityscape, plus the one
`srt04` Shadowrun-tileset area. So the district is assembled mostly from re-themed existing areas,
and only true gaps need toolset authoring.

**Locked decisions (this session):** an invented sprawl with a real-world anchor; the first district
is **The Barrens**; the first *build* is scoped to a **proof street** (~5–8 areas) before committing
to the full 30–50; areas are built by **re-theming existing ones, authoring only the gaps**.

## Setting frame (proposal — rename is cheap by design)

- **City:** the **Erie Metroplex** — a rust-and-neon Great Lakes sprawl loosely anchored to the
  Detroit–Cleveland–Buffalo corridor. Familiar bones (lake, rust belt, dead industry) with fully
  invented names, and deliberately *not* Shadowrun's canonical Seattle/Chicago, for IP separation.
- **District:** **The Barrens** — the metroplex's abandoned edge. "Barrens" is a generic Sixth World
  term, not a trademarked place. Rust, neon, gang turf, cheap chrome, no corp law.
- All place/faction names live behind a naming layer (see IP ground rule) so any of the above can be
  changed in one place.

## The Barrens on paper (full district concept, ~30–50 areas)

Designed in full in `districts/barrens.md`; only the proof street is built now.

| Zone | ~Areas | Re-theme source | Mood |
|---|---|---|---|
| The Strip (main drag) | 4–6 | Hub / vertical cityscape (`dgt04`/`fcx01`) | Neon night, crowds, fog |
| The Squats (residential decay) | 6–10 | Slum areas (`dgt04`) | Dark, decayed, quiet |
| Gang turf | 6–10 | Industrial slum, slums | Hostile, fires, hazards |
| The Docks edge | 4–6 | Shipping district (`dgt04`) | Water, cranes, smuggling |
| Back-alley services (interiors) | 6–10 | Modern interiors (`modint`) | Cramped, lit, functional |
| Chrome & shadows (clinic, bar, fixer dens) | 4–6 | Interiors | Signature Sixth World rooms |

## The proof street (first build — ~6 areas)

Scoped to answer "does this read as the Sixth World?" *and* to exercise the pieces the rest of the
project needs. Each area re-themes an existing one; connections are standard `LinkedTo` transitions.

| # | Area | Re-theme source | Purpose |
|---|---|---|---|
| 1 | **The Strip** (hub) | `pw_ar_narshahub` (Smuggler's Moon – The Hub) | Central neon drag; connects everything |
| 2 | **The Cyberclinic** | a `modint` interior | Home for the cyberdoc NPC — ties P1b's live gate into the world |
| 3 | **The Fixer's Bar** | a bar/tavern interior | Where the Johnson sits; the P3 run-delivery point (D14) |
| 4 | **Gang Turf** (combat) | `pw_ar_narslum` / `ar_pw_indusvel` | A ganger fight — exercises the retuned combat + glitches live |
| 5 | **The Squats** | a slum area | Flavor/exploration, a hidden stash |
| 6 | **Dock Gate** | `pw_ar_nardocks` | The edge/exit; a smuggling hook for later |

This deliberately co-locates the P1a/1b/1c live gates: a player rolls a troll street sam, visits the
cyberclinic (2), chromes up, and walks to Gang Turf (4) to watch soak, glitches, and Magic loss in a
real fight — turning three pending gates into one walkable test.

## Division of labor

| I author (data) | You author (toolset) |
|---|---|
| Re-themed `.are.json`/`.git.json` copies (lighting, fog, name, tag, resref) | New area geometry for any gap the re-themes can't cover |
| Barrens creature blueprints (`.utc`): gangers, go-gangers, a fixer, bar patrons | Verifying/adjusting precise object placement by eye |
| Spawn tables — `Feature/SpawnDefinition/BarrensSpawnDefinition.cs` (`SpawnTableBuilder`) | Placing/moving transitions and waypoints if the JSON edits misalign |
| NPC dialogs: fixer, bartender (the cyberdoc dialog already exists) | Final walk-through and mood judgement |
| Registering areas in the module and repacking | — |

## IP / rename ground rule (folds in the deferred audit)

Per [D12](design/shadowrun/DECISIONS.md), setting-specific vocabulary must stay containable. Establish
now, before the district multiplies names: a single **place/faction naming source** (a small static
map or resource the content reads), so "Erie Metroplex", "The Barrens", gang names, and the like are
defined once. New content references the source, never scatters the literal. This is the cheap-now,
expensive-later insurance the audit flagged.

## Files / artifacts

- `design/shadowrun/districts/barrens.md` — the full district design doc (zones, map, NPC roster, run
  hooks), the durable creative record
- `Module/are/*.are.json` + `Module/git/*.git.json` — ~6 re-themed area copies
- `Module/utc/*.utc.json` — Barrens creature blueprints
- `SWLOR.Game.Server/Feature/SpawnDefinition/BarrensSpawnDefinition.cs` — spawn tables
- `SWLOR.Game.Server/Feature/DialogDefinition/` — fixer and bar dialogs
- module `.ifo` area registration; `Module/PackModule.cmd` to deploy
- naming source for place/faction vocabulary

## Verification

No unit tests for area content; this gate is played, not asserted. After building and
`PackModule.cmd`:

1. In the DM client, jump to **The Strip** and walk the proof street end to end. It reads as the Sixth
   World without explanation — rust, neon, decay.
2. Transitions connect all six areas both ways.
3. The **cyberclinic** is reachable and the cyberdoc opens the clinic (P1b), so a fresh troll street
   sam can chrome up here.
4. **Gang Turf** spawns gangers; a fight exercises the retuned soak, wound modifiers, and glitches
   (P5/P6/P1c) in real play — folding those live gates in.
5. The **fixer** greets the player (the P3 run hook, even if runs aren't built yet).

**Gate:** one walkable Barrens street that reads as the Sixth World, with the cyberclinic and a real
fight in it. If it does not convince, that is the kill-criteria signal — reconsider Evennia — and it
is far cheaper to learn here than after 30 areas.

## Out of scope

The full 30–50 area build (Phase 2b); authored runs and the run board (Phase 3); other districts;
the Matrix/astral; any new tileset art. New geometry beyond what re-theming covers is yours in the
toolset, not part of this data package.

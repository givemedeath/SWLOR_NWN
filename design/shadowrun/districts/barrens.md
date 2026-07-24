# The Barrens — District Design

*The durable creative record for Phase 2 ([P2a](../packages/P2a.md)). The map and roster are designed
in full here; only the proof street is built first. Every place, faction, and NPC name below is the
**single source of truth** — content references these, per the IP ground rule, so the whole setting
can be renamed from this file.*

---

## The city: the Erie Metroplex

A rust-and-neon Great Lakes sprawl, loosely anchored to the dead industrial corridor from Detroit to
Buffalo. Familiar bones — a poisoned lake, drowned factories, a skyline of corp arcologies standing on
the corpse of the twentieth-century auto and steel belt — with wholly invented names. Deliberately not
Shadowrun's canonical Seattle or Chicago: original geography, original corps, minimal trademark
surface ([D12](../DECISIONS.md), [D18](../DECISIONS.md)).

**Feel:** cold, wet, lit by signage no one maintains. The lake wind carries rust and ozone. Above, the
corps; below, everyone else.

## The district: The Barrens

The metroplex's abandoned edge — a wedge of failed industry and collapsed housing between the lake and
the corp cordon. No corp law reaches here; the gangs and a few fixers hold what order there is.
"Barrens" is a generic Sixth World term for a blighted zone, not a trademarked place.

**One-line pitch:** where a new runner starts — danger you can walk into, chrome you can afford, and a
fixer who might have work.

---

## Factions

| Name | What they are | Attitude | Turf |
|---|---|---|---|
| **The Rustkings** | Ork-led go-gang; scavengers and enforcers | Hostile to outsiders on their turf | Gang Turf, the Squats |
| **Lakeline** | Smuggling crew working the water | Neutral, transactional | Dock Gate |
| **The Unwired** | Anti-corp squatter collective | Wary, non-violent | The Squats |
| **Ecerre-Vandt** *(corp, offstage)* | The arcology conglomerate that owns the skyline | Absent here; the enemy everyone shares | — |

Corp names end in invented syllables (`-Vandt`, `-corp`, `Ecerre`) to stay clear of canon.

---

## Key NPCs

| Name | Role | Location | Notes |
|---|---|---|---|
| **Doc Sever** | Street-doc (cyberware) | The Cyberclinic | Uses the existing `CyberdocDialog`; installs P1b chrome |
| **Marta "Ledger" Vane** | Fixer / Johnson | The Fixer's Bar | The P3 run-delivery point ([D14](../DECISIONS.md)) |
| **Cass** | Bartender | The Fixer's Bar | Rumors, flavor, a drink |
| **Grist** | Rustkings lieutenant | Gang Turf | The proof-street fight's named threat |

Names are grounded and un-branded; all live in the roster table so a rename is one edit.

---

## The full district on paper (~30–50 areas)

Built later (Phase 2b); scoped here so the proof street connects into a real place.

| Zone | ~Areas | Re-theme source | Mood | Holds |
|---|---|---|---|---|
| **The Strip** | 4–6 | Hub / vertical cityscape (`dgt04`/`fcx01`) | Neon night, crowds, fog | Services, the social hub |
| **The Squats** | 6–10 | Slum areas (`dgt04`) | Dark, decayed, quiet | The Unwired, stashes, flavor |
| **Gang Turf** | 6–10 | Industrial slum, slums | Hostile, fires, hazards | The Rustkings, fights |
| **The Docks edge** | 4–6 | Shipping district (`dgt04`) | Water, cranes, smuggling | Lakeline, smuggling runs |
| **Back-alley services** | 6–10 | Modern interiors (`modint`) | Cramped, lit, functional | Clinics, gear, safehouses |
| **Chrome & shadows** | 4–6 | Interiors | Signature Sixth World rooms | The clinic, the bar, dens |

---

## The proof street (first build — 6 areas)

Chosen to answer *"does this read as the Sixth World?"* while exercising every downstream system. Each
re-themes an existing area; connections are standard `LinkedTo` transitions.

```
        [5 The Squats]
              |
[2 Cyberclinic]—[1 THE STRIP]—[3 Fixer's Bar]
              |
        [4 Gang Turf]
              |
        [6 Dock Gate]
```

| # | Area | Re-theme source | Mood | Purpose |
|---|---|---|---|---|
| 1 | **The Strip** | `pw_ar_narshahub` | Neon, wet, crowded | Central hub; connects all |
| 2 | **The Cyberclinic** | a `modint` interior | Sterile-ish, humming | Doc Sever; chrome up (P1b gate) |
| 3 | **The Fixer's Bar** | a bar interior | Smoky, low light | Ledger the fixer; Cass; the P3 hook |
| 4 | **Gang Turf** | `pw_ar_narslum` / `ar_pw_indusvel` | Fires, hostile | Rustkings fight — P5/P6/P1c gate |
| 5 | **The Squats** | a slum area | Decayed, quiet | Flavor, a hidden stash |
| 6 | **Dock Gate** | `pw_ar_nardocks` | Water, cranes | The edge/exit; Lakeline hook |

**Why this set:** it folds three pending live gates into one walk — roll a troll street sam, chrome up
at the Cyberclinic (2), then fight the Rustkings in Gang Turf (4) to watch subtractive soak, wound
modifiers, glitches, and Magic loss in real play. The fixer (3) seeds Phase 3 without needing runs yet.

---

## Committed build (P2b) — the first finished exterior

Per [P2b](../packages/P2b.md), the first exterior is finished and committed before the rest of the
route is generated. The re-theme sources in the table above are superseded by the procgen path
([D19](../DECISIONS.md), [D24](../DECISIONS.md)): the Strip is generated on the **D20 Futuristic City**
tileset (`dgt04`), using the finished hand-built slum `pw_ar_narslum`; `fcx01` was rejected in live
review because it reads as high-tech downtown rather than the Barrens.

### Seed / resref / generation record

| Field | Value |
|---|---|
| Area | The Strip (proof-street area 1) |
| Committed resref / tag | `barrens_strip` |
| Area name | `The Barrens - The Strip` |
| Tileset | `dgt04` (D20 Modern Exterior) → HAK `sw_t_modernex` |
| Source area | `pw_ar_narslum` — “Smuggler's Moon - The Slums” |
| Layout | hand-built static slum geometry |
| Seed | not applicable; authored source geometry |
| Size | `16 × 16` |
| Decorations | 1,288 static placeables across 167 blueprint resrefs |
| Transform | renamed, cleaned legacy labels/audio/waypoints, retained generic street dressing |
| Rejected candidate | `fcx01`/`futcity`/`packed` seed 777 read as high-tech downtown |

Generated geometry is committed as **static** `ModuleSR/are/barrens_strip.are.json` /
`git/barrens_strip.git.json`. The district uses no runtime random generation.

### Mood applied

- **Lighting / fog / sky:** retained from the hand-built slum's permanent-night industrial mood.
- **Signage / props / landmarks:** retained 1,288 authored buildings, facades, lights, barriers, refuse,
  and street-dressing placeables; player-facing legacy labels were removed or reworded.
- **Ambient sound / music:** set deliberately (not inherited) — `al_pl_citynite` ambient bed and
  `mus_cityslumnite` music, `mus_bat_city1` in combat.
- **Legacy residue removed:** SW-branded placeable labels, droid/Imperial naming, ship flyby audio,
  old spawn waypoints, and old area-specific routes were removed. Creatures and service NPCs remain
  deferred to P2c/P2d.

### Route from arrival

| From | Transition (Type 1 trigger) | To (waypoint) |
|---|---|---|
| `erie_arrival` | `arrival_to_strip` @ (85,45) | `barrens_strip` : `WP_STRIP_FROM_ARRIVAL` (65,105) |
| `barrens_strip` | `strip_to_arrival` @ (145,119) | `erie_arrival` : `WP_ARRIVAL_FROM_STRIP` (35,65) |

Transitions are the standard NWN engine area-transition trigger (`Type=1`, `LinkedTo` a destination
waypoint tag, `LinkedToFlags=2`). Each trigger is placed clear of every spawnable waypoint in its
own area (the outbound trigger is 10m from the respawn point, 44m from the arrival spot; the return
trigger is ~80m from the strip arrival spot) so a returning or respawning runner never loops.

The outbound trigger is an invisible floor volume, so a **visible elevator** placeable
(`barrens_elevator`, `_mdrn_pl_elevato`) sits on it — the findable "ride out to street level" landmark.
This surfaced in the first live test: the exit could not be found without a marker, which also exposed
that the P2m minimal HAK allowlist rendered no placeable models at all (the elevator, and the Strip's
631 street-dressing placeables, load their models from the shared placeable HAKs). Erie now ships the
full shared SWLOR stack ([D25](../DECISIONS.md)).

### Review result

The operator walked `erie_arrival → barrens_strip → back` on the deployed module and judged the dgt04
Strip's Barrens mood good. P2b's visual acceptance gate passed. Download-size / boot /
transition-latency / frame-time notes remain useful operational measurements for later release review.

### Large procgen comparison

`barrens_pgen40` is a separate deterministic 40×40 dgt04 candidate: profile `modernex`, Packed layout,
seed `20260723`, 253 generated environmental placeables, and 172 packaged blueprint resources. It is
reachable from the second labeled arrival sign, returns to arrival, and exists to answer whether the
procgen pipeline can scale beyond the authored 16×16 Strip. The dgt04 `Streets` layout is not used yet
because its required Alley shape inventory is incomplete.

## Run hooks seeded here (for Phase 3)

Not built now, but the fixtures exist so authored Runs drop in:

- **Ledger** at the bar → the Johnson who hands out Barrens runs.
- **Gang Turf / Grist** → a "clear the Rustkings" or "recover X" combat run.
- **Dock Gate / Lakeline** → a smuggling/escort run to the water.
- **The Squats / The Unwired** → a legwork/talk run with no combat.

---

## Naming / IP ground rule

Everything setting-specific — city, district, factions, NPCs, place names — is defined **in this file
and only here**. Content (dialogs, area names, spawn tables) references these canonical strings rather
than inventing its own. When the first content needs them programmatically, they graduate into a small
naming source that reads from this list. The point ([D12](../DECISIONS.md)): a forced rename is a
contained edit, never a hunt through 900 files.

## Status

Design complete; build (re-themed areas, creature blueprints, spawn tables, fixer/bar dialogs) is the
follow-on, best done with live iteration so the mood judgement in the gate can steer it.

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

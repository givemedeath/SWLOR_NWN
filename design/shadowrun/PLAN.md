# Shadowrun on SWLOR's Engine — Plan of Record

**v2 · 2026-07-22 · supersedes [PLAN-ARCHIVE-presentation-layer.md](PLAN-ARCHIVE-presentation-layer.md)**

v1 was a presentation-layer plan: translate SWLOR's combat vocabulary into Shadowrun terms in 9–12
weeks. That work is **done and validated in live play**. This is a different document — a plan for
building a game, informed by everything the first round measured.

The v1 plan is archived rather than deleted. It remains the accurate record of Waves 0–2.

---

## What the first round actually taught us

Three assumptions in v1 and in [PLATFORM-ASSESSMENT.md](PLATFORM-ASSESSMENT.md) survived only until
someone measured them. That pattern is the most important finding of all, and it shapes how this plan
is written: **every phase below ends in a measurement, not an opinion.**

| Assumption | What measurement showed |
|---|---|
| Displayed dice pools would read as meaningful | Every even fight resolved at 74–76% regardless of pools. The display was decorative until [D9](DECISIONS.md) |
| A 2080s sprawl is a serious tileset project | **D20 Shadowrun Exterior** (315 tiles) and **D20 VirtuNet** (72) already ship. 38 areas already use urban sets |
| The Matrix is a multi-year subsystem | `Space.cs` — the whole parallel-game precedent — is 2,214 LOC and 41 definition files |
| Custom species are a big lift | **269 custom appearance rows** already exist at 10000+. The pipeline is built |
| Metatypes are free | Models exist, but SWLOR **disabled** every fantasy race. Re-enabling is a 2DA edit |
| No cyberware system exists | Confirmed. Genuinely nothing — this one held |

---

## Locked decisions

| Question | Answer | Consequence |
|---|---|---|
| **Content** | Hybrid — new district, existing systems | New areas and creatures; keep item/recipe/quest data and convert as it becomes player-visible |
| **Fidelity** | Shadowrun-flavored systems | Percentage resolution stays under the hood. Cyberware/Essence, metatypes, and Drain get built for real |
| **Setting** | Original city, canonical world | Sixth World rules, metatypes, megacorps; an invented sprawl. No lore-policing, less trademark surface |
| **Team** | Solo + agent assistance | Sequence tightly, minimize context-switching, dispatch mechanical work to subagents |

**"Shadowrun-flavored" is the load-bearing decision.** It means the resolution kernel is settled and
will not be revisited — no dice-pool rewrite — while the *identity* systems are real builds. Cyberware
is not a reskinned perk tree; Essence is a real resource with a real tradeoff.

---

## The unresolved risk: video game rights

`Topps` owns the Shadowrun IP and licenses tabletop to Catalyst Game Labs. **Microsoft holds the video
game rights.** This is a video game.

That is a more direct exposure than a tabletop fan project, and it should be a deliberate choice
rather than something drifted into. The mitigations are conventional and cheap, and all of them are
easier to adopt now than to retrofit:

- Never charge for access, and take no donations tied to access or content
- Use no assets, art, logos, or verbatim text from any published Shadowrun product
- Original city, original characters, original plot — already the chosen setting direction
- Be ready to rename. **Keep setting-specific vocabulary behind `ShadowrunDisplay`** rather than
  hardcoded into 900 content files, so a forced rename is a one-file change

That last point is a concrete architectural requirement, not a disclaimer, and it is why the display
indirection layer built in v1 should be *extended* rather than dissolved into content.

**This plan does not require a decision today.** It requires not foreclosing one.

---

## What we keep, build, and replace

| Keep as-is | Build new | Replace eventually |
|---|---|---|
| Combat kernel (retuned, validated) | Metatypes as playable species | FP/Stamina → Magic + Drain ([P9](#phase-4--depth)) |
| 82 services: crafting, market, property, factions, achievements | Cyberware + Essence | Star Wars areas → sprawl districts |
| Quest engine + dialog snippet system (259 quests, 33 files) | 8–12 authored Runs | Star Wars creatures → Sixth World critters and gangs |
| NUI framework, DB persistence, AI | Run objective vocabulary | Item/recipe data, as it becomes visible |
| NWN DM Client for live GM work | GM event kit | Existing Star Wars quest content |
| 269-row custom appearance pipeline | Sprawl district content | |

---

## Phases

Sized for solo work at roughly 10–15 productive hours per week, with agents taking mechanical bulk.
Each phase ends in a **gate** — something measured or played, not something declared done.

### Phase 1 — Identity · *you can make a Shadowrun character* · ~6–8 weeks

The cheapest path to the game feeling like Shadowrun rather than like SWLOR with new labels.

| # | Package | Notes |
|---|---|---|
| `1a` | **Metatypes** — human, elf, dwarf, ork, troll as playable species | 2DA work plus the existing custom-appearance pipeline. Troll needs a model decision: Ogre/Giant/Minotaur base, or a custom appearance row |
| `1b` | **Cyberware + Essence** | The identity mechanic. New `StatType` entries, an Essence resource, and the magic-versus-chrome tradeoff. Hangs on existing perk + item-property infrastructure |
| `1c` | **Glitches** (was P7) | Carried from v1. Brief accuracy debuff plus VFX on the existing status-effect framework |
| `1d` | **Attack/Delay vocabulary** (was P5b) | Small. Settle `Delay` → Initiative once the pass model is decided |
| `1e` | **Character creation flow** | Metatype choice, starting cyberware, Essence display on the sheet |

**Gate:** roll a troll street samurai with wired reflexes and a dwarf mage, fight something, and have
the character sheet read coherently in Shadowrun terms.

**Cut from v1:** *P8, the 910 perk descriptions.* It is polish on text attached to content being
replaced. Revisit only when the perk trees themselves are Shadowrun-native.

### Phase 2 — Place · *there is somewhere to be* · ~3–6 months · **the long pole**

The single largest time sink in the project, and the one most likely to stall. Everything here is
area-building, which does not parallelize to agents well.

| # | Package | Notes |
|---|---|---|
| `2a` | **District design** — one sprawl district, ~30–50 areas | Downtown core, barrens, corp enclave, docks, a few interiors. Name it, map it, then build |
| `2b` | **Area build-out** | Uses `srt04`, `dgt04`, `fcx01` which already ship. Lighting and fog do more for mood here than geometry |
| `2c` | **Creature set** | Gangers, corp sec, drones, critters, spirits. Reuse SWLOR stat skins; new appearances where needed |
| `2d` | **NPCs, shops, fixers** | The Johnson who hands out work, the fixer who sells gear, the doc who installs chrome. **Place these deliberately — each Johnson is a delivery point for Phase 3's authored Runs**, so the district layout and the run roster should be designed together rather than in sequence |

**Gate:** walk the district end to end and have it read as the Sixth World without explanation.

**Sequencing note:** build **one** area to full finish quality before building thirty. A finished
street answers "does this look right" definitively, and the answer changes everything downstream.

### Phase 3 — The loop · *there is something to do* · ~8–12 weeks

**Three tiers of content, and they are not interchangeable.** This is the core structural insight of
the whole project:

| Tier | Source | Provides | Available |
|---|---|---|---|
| **Authored Runs** | NPC Johnsons | The content **floor** — real missions with structure and stakes | Always |
| **Contract board** | Procedural | Repeatable grind between runs | Always |
| **GM events** | Live staff | The living, breathing world — consequence, surprise, story | When staffed |

GMs make the world feel alive. **Authored Runs make it survive the nights they are offline.** A world
with only GM content is dead six evenings out of seven; a world with only procedural contracts is an
MMO with a Shadowrun skin. Both tiers are required, and the authored Runs are the harder of the two to
retrofit later.

| # | Package | Notes |
|---|---|---|
| `3a` | **Authored Runs** — 8–12 for launch | The floor. Johnson dialog → meet → legwork → objective → payout. Target a mix of one-shots and `IsRepeatable()` runs so the district does not exhaust in a weekend |
| `3b` | **Run objective vocabulary** | Extend the quest builder — see below. Blocks `3a` from feeling varied |
| `3c` | **Contract board** | Reskin `QuestContractBoard` into procedural runs. Existing service, mostly a vocabulary pass |
| `3d` | **GM event kit** | Spawn kits, faction levers, staged encounters a GM triggers live. The DM Client already provides possession, spawning, and invisible observation — this is the content layer on top |
| `3e` | **Heat / street cred** | Existing faction standing, reskinned. `AddFactionStandingReward` already exists on the quest builder |
| `3f` | **Downtime conversion** | Market, property, crafting surfaces as they become visible, per the hybrid decision |

#### What the quest system already supports

Verified rather than assumed. **259 quests already ship across 33 definition files**, and the builder
covers most of a run:

| Run stage | Support today |
|---|---|
| Meet the Johnson | ✅ dialog + `action-accept-quest` snippet |
| Legwork — talk to contacts | ✅ dialog + `action-advance-quest` snippet |
| Eliminate / steal | ✅ `AddKillObjective`, `AddCollectItemObjective` |
| Multi-stage structure | ✅ `AddState` + `SetStateJournalText` |
| Gated follow-ups | ✅ `PrerequisiteQuest`, `PrerequisiteKeyItem`, `PrerequisiteSkill` |
| Payout in nuyen | ✅ `AddGoldReward` |
| Heat / cred | ✅ `AddFactionStandingReward`, `AddFactionPointsReward` |
| Repeatability | ✅ `IsRepeatable()` |
| **Reach a location** | ⚠️ script-only — `ExplorationTrigger` calling `Quest.AdvanceQuest` |
| **Hack a terminal / use an object** | ⚠️ script-only — `PlaceableScripts` calling `Quest.AdvanceQuest` |
| **Extract a person** | ❌ no escort support |

The dialog **snippet system** (`condition-has-quest`, `condition-on-quest-state`,
`action-advance-quest`, `action-request-quest-items`) is the reason most of this already works: any
conversation node can gate on or advance quest state, so legwork is authored in dialog rather than in
code.

**`3b` exists because only two objective types are declarative** — kill and collect. Every run built
on those alone is "kill N" or "fetch N", which is precisely the MMO texture authored Runs are meant to
escape. `Quest.AdvanceQuest(player, source, questId)` is callable from any script, so the escape hatch
exists; the package is about making *reach-location*, *use-object*, and *extract* first-class builder
methods instead of hand-scripted every time.

**Gate:** a GM runs a live session for two or three players — **and those same players log in the
next evening with no GM online and find a Johnson with work worth doing.** Both halves must pass.
This is the whole value proposition, tested directly.

### Phase 4 — Depth · *ongoing, after the slice is real*

| # | Package | Notes |
|---|---|---|
| `4a` | **Magic + Drain** (P9) | Replaces FP/Stamina. Full writeup preserved in the archived plan. Large; do not start before Phase 3's gate |
| `4b` | **Matrix** | Cheaper than assessed. `VirtuNet` tileset ships; `Space.cs` is the architectural precedent at 2,214 LOC |
| `4c` | **More districts** | Only after one district proves the pipeline |
| `4d` | **Rigging / drones** | SWLOR's droid system is the nearest existing analogue |

---

## Working method

**Measure, then decide.** Every significant constant in this project has been wrong on first guess and
right after measurement. The combat harness (`CombatFeelHarnessTests`) is the template: build the
instrument before tuning the thing.

**Agents take mechanical bulk, not judgement.** Per the tiering in
[ORCHESTRATION.md](ORCHESTRATION.md): 2DA edits, bulk text, definition scaffolding, and data
extraction dispatch well. Balance, feel, and setting voice stay with the human.

**Area building does not dispatch.** Phase 2 is hand work. Plan around that rather than hoping
otherwise.

**Keep the durable record current.** [LEDGER.md](LEDGER.md) for what happened,
[DECISIONS.md](DECISIONS.md) for why. Both have already paid for themselves — the empirical constants
in this project are unreconstructible without them.

---

## Kill criteria

v1 set a six-month deadline on the art question. That question is now **answered** — the tilesets
ship, the areas exist, and the visual direction is accepted. Replacing it:

> **If Phase 2 does not produce one finished street that reads as the Sixth World, stop and
> reconsider Evennia.**

Not from-scratch. Evennia — a text MUD has no art pipeline at all, which is the only remaining
structural risk in this plan.

> **If Phase 3's gate fails — a GM runs an event and players have nothing to do the next night —
> the problem is the game loop, not the engine**, and no amount of further building fixes it.

---

## Honest cost

To a **playable vertical slice** — one district, metatypes, cyberware, runs, GM events — at solo pace
with agent assistance: **6–12 months part-time.**

To a world at SWLOR's current scale: **years**, and that is not the goal. The district is the goal.

For comparison, measured earlier: a faithful ruleset conversion was 38–53 engineer-months, and SWLOR
itself represents roughly a decade of work that this plan inherits rather than repeats.

# Platform Assessment — Is SWLOR/NWN:EE the right base for Shadowrun?

**2026-07-22 · status: accepted · supersedes nothing · informs [D1](DECISIONS.md)**

Written in response to the question: *is this adaptation best suited for Shadowrun, or would another
tool — or building from scratch — better provide a free, fan-made, multiplayer persistent world with
GM-orchestrated live events and enough to do when GMs are offline?*

This document is the honest answer, including where it disagrees with [PLAN.md](PLAN.md).

---

## Verdict

**NWN:EE is very likely the correct engine, and SWLOR is the correct starting point — but the plan
understates the project by roughly an order of magnitude, because of a framing error worth fixing
now.**

The four requirements are mutually hostile:

| Requirement | Wants |
|---|---|
| Live GM-orchestrated events | a VTT |
| Persistent world with solo content | an MMO |
| Free and fan-made | no art budget |
| Extensible | a real engine |

Nearly every platform satisfies two and fails two. NWN:EE is one of the only things ever built that
satisfies all four, and not accidentally: the DM Client is a purpose-built live-GM tool welded into a
persistent multiplayer RPG engine, with 25 years of persistent-world community proving the model.

Foundry runs GM events beautifully and offers nothing to do when the GM logs off. A from-scratch
engine gives total control over a game that will not ship.

---

## The framing error

Measured from this repository:

| Layer | Size | Converts to Shadowrun? |
|---|---|---|
| C# systems | 539,315 LOC across 82 services | **Yes — genre-agnostic** |
| Areas | 443 | No |
| Creatures | 938 | No |
| Items | 7,526 | No |
| Dialogs | 609 | No |
| Placeables | 8,345 | Partially |

The systems layer is a decade of exactly the machine the "things to do when GMs aren't available"
requirement needs: skills, perks, crafting, player market, property, faction standing, quests,
achievements, contract boards, DB persistence, NUI. It is genuinely setting-neutral.

The content layer is the game players actually experience, it is 100% Star Wars, and effectively none
of it carries over.

> **This project is not a conversion of SWLOR to Shadowrun. It is a new game built on SWLOR's engine
> layer.**

That distinction drives every scoping decision downstream. [PLAN.md](PLAN.md)'s "9–12 weeks, ~4% of
the faithful path" is accurate about the combat *display layer* and quietly misleading about the
whole. Waves 0–2 produced real work; they did not produce 4% of a Shadowrun game.

---

## Where the fit is unusually good

Strong enough, collectively, to make NWN hard to argue against despite its age.

**Metatypes are free.** `SWLOR.NWN.API/NWScript/Enum/RacialType.cs` already carries `Dwarf`, `Elf`,
`Human`, `Halfelf`, `HumanoidOrc`, and `Giant` natively, and SWLOR already proved custom entries work
(`Robot = 150`, `Zabrak = 154`). Human/elf/dwarf/ork/troll is the one thing a fantasy engine hands you
that no cyberpunk engine can. FiveM cannot make a troll.

**Magic is already built.** The Force system — `Force Attack`, `Force DEF`, FP, and the
Consular/Guardian/Manipulator/Ravager perk trees — is a working spellcasting framework with
progression. Force → Magic is closer to a rename than a build. Spirits map onto NWN summons, which
already exist.

**Guns already won.** SWLOR made ranged weapons the primary combat mode inside a melee engine, with
Pistol/Rifle/Throwing perk trees. That fight is over.

**The starship system is the Matrix's architectural proof.** `SpaceObjectDefinition`,
`ShipDefinition`, `ShipModuleDefinition`, and the `Shuttle` service demonstrate that NWN + NWNX can
host an entire parallel game with its own stats, its own UI, and its own combat resolution. That is
precisely the Matrix's shape, and it is rare to have it already proven in-engine.

**Downtime is the whole point.** Crafting, gathering, fishing, market, property, factions, and the
contract board are what Shadowrun's between-runs life *is*. This is the requirement that eliminates
most alternatives.

---

## Where it genuinely doesn't fit

**Art is the critical path, and it is not close.** Not code. NWN's asset library is medieval; SWLOR
built custom haks for sci-fi, but a 2080s sprawl — neon, arcologies, street level — is a serious
tileset project with weak community coverage. If this project dies, it dies here.

**No cyberware, no Essence.** Searched: nothing. Zero implant or augmentation system exists. Essence
loss and the chrome-versus-magic tradeoff is arguably *the* Shadowrun identity mechanic. The
mitigating factor is that it maps cleanly onto existing perk and item-property infrastructure, so it
is a build rather than a fight.

**The Matrix is a multi-year subsystem** even with the space-combat precedent — and its hard design
problem (the decker plays a different game at a different tempo than the team) is unsolved at the
tabletop too.

**The d20 substrate remains a translation layer permanently.** What Waves 0–2 built works. It is
still paint.

**"Free" carries a ~$20 toll** for the NWN:EE client, and the NWN persistent-world population is
small — low hundreds concurrent across the entire ecosystem.

---

## Alternatives, honestly

| Option | Persistent | GM events | Solo content | Verdict |
|---|---|---|---|---|
| **SWLOR / NWN:EE** | yes | best-in-class | a decade of it | Only option hitting all four |
| **Foundry VTT + SR5e** | no | yes | no | Rules-perfect VTT; not a world |
| **Evennia (MUD/MUSH)** | yes | yes | codeable | **The real alternative** |
| **FiveM (GTA V)** | yes | yes | partial | No metatypes. Fatal. Plus Take-Two |
| **Godot/Unity from scratch** | — | — | — | Where fan projects go to die |

**Evennia deserves more respect than it usually gets.** Shadowrun's most durable persistent
communities have historically been MUSHes running for decades. Text handles the Matrix, astral space,
cyberware, and legwork *natively* — the subsystems that cost years in NWN cost weeks in prose. There
is no art pipeline at all, which removes the single biggest risk identified above. Full ruleset
fidelity is achievable because nothing is fighting you.

Its weakness is real: text filters out most modern players, and MUSHes historically did the
"something to do when the GM is offline" requirement badly — they were GM-dependent, which is exactly
the failure mode this project is trying to avoid.

---

## Recommended changes to the plan

1. **Rename the effort.** "Shadowrun on SWLOR's engine," not "SWLOR adaptation."
2. **Launch a district, not a world.** 40–60 areas of one sprawl district. Not 443.
3. **Cut the Matrix from v1.** Hacking as skill checks and legwork. The space system proves it is
   possible later; it is not a launch feature.
4. **Build cyberware/Essence before more display work.** Highest identity-per-hour available, and the
   infrastructure already exists.
5. **Prove the art pipeline before writing more C#.** Source or commission one sprawl tileset and
   build one convincing street. Everything else is downstream of whether that is achievable.
6. **Do not finish P8.** Rewriting 910 perk descriptions into Star Wars-flavoured Shadowrun vocabulary
   is polish on content that is being replaced anyway.

---

## Kill criteria

Set now, while it is cheap to set:

> **If six months from now there is not one street that looks like the Sixth World, the answer was
> Evennia.**

Not from-scratch — Evennia. Deciding this in advance is what stops sunk cost from deciding it later.

---

## Banked regardless of outcome

- **Subtractive soak** ([D8](DECISIONS.md)) is a real improvement to SWLOR's combat model independent
  of setting. Multiplicative mitigation meant nothing ever bounced.
- **`ShadowrunDisplay`** is a clean vocabulary indirection layer that would serve any retheme.
- **The orchestration layer** ([ORCHESTRATION.md](ORCHESTRATION.md)) is setting-neutral tooling.

None of it is wasted whichever way this goes.

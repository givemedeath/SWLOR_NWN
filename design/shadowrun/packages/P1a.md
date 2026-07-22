# P1a — Metatypes as Playable Species

*Detailed brief for Phase 1a of [design/shadowrun/PLAN.md](design/shadowrun/PLAN.md). On approval this
becomes `design/shadowrun/packages/P1a.md`; the plan of record already carries the one-line 1a entry
and is unchanged.*

## Context

Phase 1 of the Shadowrun build is Identity — the cheapest path to the game feeling like Shadowrun
rather than SWLOR with new labels. 1a is the first and most enabling piece: you cannot write a
Johnson's dialogue about a troll, or design cyberware tradeoffs against metatype attributes, until the
five core metatypes exist as playable species.

Research settled that this is a **paved path**, not new ground. SWLOR already ships 18 custom playable
species (ids 152–169) built on one repeated five-part pattern, and two things that looked hard are
already solved:

- **Troll/dwarf sizing** — `Race.cs:545` applies `SetObjectVisualTransform(Scale)`, and **Ugnaught
  ships at 0.86** — a short race wearing armor in production today. Trolls scale up, dwarves scale
  down, armor keeps working, no new art.
- **Attribute modifiers** — every `StrAdjust` column in `racialtypes.2da` is `0`; SWLOR is
  stat-driven. Racial identity is applied through SWLOR's own systems, not the 2DA.

**Locked choices (this session):** scale base models (no custom troll art for launch); attributes plus
one signature trait per metatype, all modeled as `StatType` adjustments per the AGENTS stat-driven
rule.

## The established five-part pattern

Every custom species is exactly these five parts. Adding a metatype means repeating each once.

| Part | File | Reference to copy |
|---|---|---|
| 1. `racialtypes.2da` row, `PlayerRace=1` | `SWLOR_Haks/sw_2da/racialtypes.2da` | rows 152–169 (Zabrak, Wookiee…) |
| 2. `RacialType` enum value | `SWLOR.NWN.API/NWScript/Enum/RacialType.cs` | `Zabrak = 154` … `Ewok = 169` |
| 3. Appearance entry, male + female | `SWLOR.Game.Server/Service/Race.cs` | `_defaultRaceAppearances*[RacialType.Wookiee]` |
| 4. Language init | `SWLOR.Game.Server/Feature/PlayerInitialization.cs:233` | per-race `switch` |
| 5. Racial identity (attributes + trait) | see "Racial identity" below | new — the only genuinely new code |

Species appear on the **stock NWN race-selection wheel** automatically once `PlayerRace=1`. There is no
custom picker to build.

## Metatype roster

Human/elf/dwarf/ork/troll is Shadowrun canon; the roster is fixed. Human already exists as `id 6`, so
four new rows. SWLOR attribute mapping: **Might** (melee/STR), **Perception** (ranged/DEX), **Vitality**
(Body/CON), **Agility** (evasion/reflexes), **Willpower** (WIS), **Social** (Charisma).

| Metatype | Model approach | Attribute deltas (starting proposal) | Signature trait | Trait stat |
|---|---|---|---|---|
| **Human** | base, scale 1.0 | none — flexible baseline | reserved for a future Edge mechanic ([D6](design/shadowrun/DECISIONS.md)) | — |
| **Elf** | re-enable `AppearanceType.Elf`, 1.0 | +Agility, +Social | low-light vision | `EffectUltravision()` |
| **Dwarf** | `Dwarf`/scaled humanoid, ~0.9 | +Vitality, +Willpower | toxin resistance | `StatType.PoisonDefense` |
| **Ork** | `AppearanceType.HalfOrc`, 1.0 | +Vitality, +Might | low-light vision | `EffectUltravision()` |
| **Troll** | scaled `HalfOrc`, ~1.2, green/grey skin | +Vitality, +Might (large); −Agility, −Social | dermal armor | `StatType.Defense` |

**The deltas are a starting proposal, not locked.** Per the working method, they are tuned in
playtest, not derived — this project's constants have been wrong on first guess and right after
measurement, repeatedly. Record final values in `DECISIONS.md`.

## Racial identity — the one genuinely new piece

Everything above is data entry. Only this is new code, and it must obey the AGENTS stat-driven rule:
**no shared system may special-case a `RacialType`.** Model identity as stat adjustments and let shared
systems read them.

- **Attribute deltas** apply at character init through the existing racial hook in
  `PlayerInitialization.cs` (near the `AutoLevelPlayer` capture/restore at lines 70–86), adjusting base
  ability scores so they show correctly on the sheet and survive rebuild. **First implementation task
  is to verify this integrates cleanly with the AP upgrade economy** — attribute scores carry the
  upgrade buttons, and this is the one real integration risk.
- **Signature traits** apply as `StatType` adjustments (`Defense`, `PoisonDefense`) or a persistent
  effect (`EffectUltravision`) keyed off `GetRacialType`, in a small race-trait module analogous to how
  perks grant stats. Troll dermal armor as a flat `Defense` bonus flows straight into the subtractive
  soak from [D3/D8](design/shadowrun/DECISIONS.md) — a troll literally shrugs off light hits, which is
  the correct Shadowrun texture and free given the combat retune already shipped.

## Housekeeping folded in

- **Fix the dangling strref.** `racialtypes.2da` references `16858047` (TLK id 80831), which renders as
  "Bad Strref" in game. This was explicitly deferred "to a metatype package" — this is it. Point it at
  a valid entry or add the missing `sw_tlk` string, then regenerate `sw_tlk.tlk`.
- **Metatype display names** go in `sw_tlk/sw_tlk.tlk.json` following the AGENTS TLK rules (reuse empty
  slots before appending; `16777216 + tlkId` for 2DA references; regenerate the binary before handoff).

## Files touched

| File | Change |
|---|---|
| `SWLOR_Haks/sw_2da/racialtypes.2da` | 4 new `PlayerRace=1` rows; fix `16858047` |
| `SWLOR_Haks/sw_tlk/sw_tlk.tlk.json` (+ regen `.tlk`) | metatype names; missing strref |
| `SWLOR.NWN.API/NWScript/Enum/RacialType.cs` | 4 enum values |
| `SWLOR.Game.Server/Service/Race.cs` | 8 appearance entries (M+F) with `Scale` |
| `SWLOR.Game.Server/Feature/PlayerInitialization.cs` | languages + racial attribute deltas |
| `SWLOR.Game.Server/Service/Race.cs` **or** a new race-trait module | signature-trait stat adjustments |
| `SWLOR.Game.Server.Tests/…` | metatype roster + trait-application tests |

## Reused utilities (do not rebuild)

- `SetObjectVisualTransform(ObjectVisualTransform.Scale, …)` — troll/dwarf sizing (`Race.cs:545`)
- `Race.GetDefaultAppearance` / `SetDefaultRaceAppearance` — appearance application
- `EffectUltravision()` — `SWLOR.NWN.API/NWScript/EffectFunctions.cs:723`
- `StatType.Defense` (14), `StatType.PoisonDefense` (22) — trait stats, already defined
- `CreaturePlugin.GetRawAbilityScore` / `SetRawAbilityScore` — attribute deltas
- The `racialtypes.2da` custom rows 152–169 — the row template to copy

## Orchestration

Dispatchable to a Sonnet subagent (mechanical, pattern-following): the 2DA rows, enum values,
`Race.cs` appearance entries, and TLK names — parts 1–4. **Lead-tier, kept inline:** the racial
identity code (part 5), the attribute-delta values, and the AP-economy integration check — judgement
and balance. Per the file-ownership rule, the subagent owns the data files and the enum; the controller
owns `PlayerInitialization.cs` and the trait module.

## Verification

Build once, `-p:RunPostBuildEvent=Never`, then filtered tests (AGENTS build rules):

1. **Unit** — a roster test asserts each metatype has a `PlayerRace=1` row, an enum value, M+F
   appearances, and the expected trait stat; a trait test asserts troll gets a `Defense` bonus and
   dwarf a `PoisonDefense` bonus off `GetRacialType`.
2. **No dangling strref** — `node tools/orchestration/checkhaks.js` goes green on the
   `racialtypes.2da` check that currently fails.
3. **Build the haks + pack the module** (`BuildHaks.cmd`, `PackModule.cmd`) so the race wheel and TLK
   names are live.
4. **Live gate** — in the DM client / character creation: **roll a troll street samurai and a dwarf
   mage.** Confirm all five metatypes appear on the race wheel with correct names, the troll is visibly
   larger and the dwarf shorter, both wear starting armor without distortion, low-light races see in
   dark areas, and the character sheet shows the attribute deltas and dermal armor in Shadowrun terms.

**Gate (from PLAN.md):** a troll street samurai and a dwarf mage roll up, fight something, and the sheet
reads coherently in Shadowrun terms.

## Out of scope for 1a

Custom troll appearance art (Phase 4 per the model decision); racial attribute *caps*; multiple traits
per metatype; metavariants; cyberware interaction with Essence (that is 1b).

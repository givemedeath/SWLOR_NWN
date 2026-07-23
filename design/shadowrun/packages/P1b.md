# P1b — Cyberware + Essence (first slice)

*Detailed brief for Phase 1b of [design/shadowrun/PLAN.md](design/shadowrun/PLAN.md). On approval this
becomes `design/shadowrun/packages/P1b.md`. Architecture is locked by
[D16](design/shadowrun/DECISIONS.md); this brief is the concrete build.*

## Context

Cyberware is the Shadowrun identity mechanic — the reason a non-mage can stand next to a mage — and
its cost, Essence, is what creates the chrome-versus-magic tension the setting runs on. P1a gave
metatypes attributes for chrome to trade against; nothing else exists (confirmed: no cyberware, no
Essence, no nuyen currency).

Per the working method, this ships as a **playtest slice**: 3–5 cyberware, the Essence budget, the
Magic-loss hook, and a street-doc clinic — enough to feel whether the tradeoff is fun before building
grades, bioware, or a full catalogue.

**Locked decisions (D16):** dedicated system built on the ship-module template but wired through the
player stat layer; Essence a 0–6 budget with Magic loss for everyone via the single `GetMaxFP`
chokepoint; install/remove at a street-doc cyberclinic NUI; money is NWN gold.

## Architecture: the ship-module template, player-stat wiring

The ship-module system (`Service/SpaceService/ShipModuleBuilder.cs`, definitions in
`Feature/ShipModuleDefinition/`, reflection-discovered via `IShipModuleListDefinition`) is the proven
socketable-modules design to clone. But its `EquippedAction(ShipStatus, int)` mutates a ship-only
struct that personal combat never reads, so cyberware **declares** its grants as data and the shared
player systems read them live — the exact pattern P1a used for metatype traits and that
`Perk.GetStatBonus` uses.

| Ship module system | Cyberware (P1b) |
|---|---|
| `ShipModuleBuilder` / `IShipModuleListDefinition` | `CyberwareBuilder` / `ICyberwareListDefinition`, reflection-discovered |
| `Feature/ShipModuleDefinition/*.cs` | `Feature/CyberwareDefinition/*.cs` |
| `EquippedAction` mutates `ShipStatus` | **Declarative** `IncreasesStat(StatType, amount)` on the detail |
| `HighPowerNodes` slot budget | `Essence` 0–6 budget on `Player` |
| `HighPowerModules` dict on `ShipStatus` | `InstalledCyberware` list on `Player` |
| `ShipManagementViewModel` NUI | `CyberwareViewModel` clinic NUI |
| `RequirePerk` / `ValidationAction` | install validation (Essence + gold + skill gates) |

## The three mechanisms

**1. Passive grants — declarative StatType bonuses, read live.**
Each cyberware declares `Dictionary<StatType,int>` bonuses. A new `Cyberware.GetStatBonus(creature,
stat)` sums the installed pieces' bonuses and is folded into
`Stat.GetStatAdjustmentExcludingTemporaryModifiers` ([Stat.cs:1976](SWLOR.Game.Server/Service/Stat.cs))
next to perk, status, mimicry and metatype — one added term, guarded to short-circuit before any
engine call for stats no cyberware touches (the `Mimicry.GetStatBonus` pattern). No equip/unequip
apply/revoke bookkeeping: the stat layer recomputes live from the installed list. Troll-style: a
Dermal Plating `Defense` bonus flows straight into the subtractive soak for free.

**2. Essence — a budget on `Player`.**
Add `float Essence` (max 6.0) or `EssenceSpent` to
[Player.cs](SWLOR.Game.Server/Entity/Player.cs) plus `List<string> InstalledCyberware`, initialised in
the constructor (Essence 6.0, empty list). Each cyberware has an Essence cost; install validates
`sum(costs) + newCost <= 6`. Persistent character-build data, so **no migration** — covered by the
full-rebuild path per the AGENTS full-rebuild rule.

**3. Magic loss — the `GetMaxFP` chokepoint.**
[Stat.GetMaxFP](SWLOR.Game.Server/Service/Stat.cs) is `baseFP + Willpower*3 + StatType.MaxFP bonus`.
Scale the player-branch result by remaining Essence: `effectiveMaxFP = round(rawMaxFP *
(EssenceAvailable / 6.0))`. Full chrome (6 spent) guts FP; a clean character is untouched.
Self-balancing and needs no caster flag — only Force users spend FP (non-casters' abilities cost
Stamina), so reducing FP "for all" only bites casters. Install/remove must recompute MaxFP and clamp
current FP to the new max.

## Seed cyberware (starting proposal — playtest-tunable)

All passive StatType grants, so the slice uses exactly one grant mechanism. Installing all five spends
6.0 Essence — maxing the budget and zeroing Magic, which demonstrates the tradeoff.

| Cyberware | Grant | Essence | Nuyen (gold) |
|---|---|---|---|
| Dermal Plating | `Defense +4` | 1.0 | 5,000 |
| Wired Reflexes | `Evasion +6`, `Attack +4` | 2.0 | 15,000 |
| Muscle Replacement | `Attack +6` | 1.5 | 10,000 |
| Cybereyes | `Accuracy +5` | 0.5 | 4,000 |
| Reaction Enhancers | `Evasion +5` | 1.0 | 8,000 |

Attribute-boosting cyberware (via the metatype ability-effect path) and active cyberware (granted
feats + recast) are deferred to catalogue expansion.

## Install path: street-doc cyberclinic NUI

- **`CyberwareViewModel` + `CyberwareDefinition`** NUI, registered as a new `GuiWindowType.Cyberware`,
  modelled on `ShipManagementViewModel`/`ShipManagementDefinition`. Lists cyberware with Essence cost,
  gold cost, current Essence remaining, and Install/Remove buttons. Install checks Essence budget +
  gold (`TakeGoldFromCreature`), removes charge a fee and refund Essence.
- **`CyberdocDialog`** — a new `Feature/DialogDefinition/` street-doc conversation that opens the
  window via `Gui.TogglePlayerWindow(player, GuiWindowType.Cyberware)`, following `MarketDialog` /
  `StarportDialog` which already open NUIs from conversation.

## Files

| File | Change |
|---|---|
| `Service/CyberwareService/CyberwareBuilder.cs` | **new** — fluent builder cloned from `ShipModuleBuilder` shape |
| `Service/CyberwareService/CyberwareDetail.cs`, `ICyberwareListDefinition.cs`, `CyberwareType`(if needed) | **new** — detail model + discovery interface |
| `Service/Cyberware.cs` | **new** — reflection load, `GetStatBonus`, install/remove, Essence math |
| `Feature/CyberwareDefinition/*.cs` | **new** — the 5 seed cyberware |
| `Entity/Player.cs` | `Essence` + `InstalledCyberware` fields and constructor defaults |
| `Service/Stat.cs` | add `Cyberware.GetStatBonus` term; scale `GetMaxFP` by Essence |
| `Service/GuiService/GuiWindowType.cs` | add `Cyberware` |
| `Feature/GuiDefinition/CyberwareDefinition.cs` + `ViewModel/CyberwareViewModel.cs` | **new** — clinic NUI |
| `Feature/DialogDefinition/CyberdocDialog.cs` | **new** — street-doc |
| `SWLOR.Game.Server.Tests/...` | Essence budget, stat-bonus aggregation, Magic-loss curve |

## Reuse (do not rebuild)

- `ShipModuleBuilder` / `ShipModuleDetail` / `IShipModuleListDefinition` — builder shape and reflection
  discovery to mirror (`Service/SpaceService/`)
- `Stat.GetStatAdjustmentExcludingTemporaryModifiers` — the single fold-in point for passive grants
- `Stat.GetMaxFP` ([Stat.cs](SWLOR.Game.Server/Service/Stat.cs)) — the Magic-loss chokepoint
- `Mimicry.GetStatBonus` — the short-circuit guard to copy for engine-call safety
- `Gui.TogglePlayerWindow` + `ShipManagementViewModel`/`Definition` — NUI open + clinic template
- `MarketDialog` / `StarportDialog` — dialog-opens-NUI precedent
- `TakeGoldFromCreature` (`CreatureFunctions.cs`) — charge nuyen
- `StatType.Defense`(14)/`Attack`(13)/`Evasion`(18)/`Accuracy`(17) — seed grants, already defined

## Verification

Build once, `-p:RunPostBuildEvent=Never`, then filtered tests.

1. **Unit** — Essence budget rejects a 7th-point install; `Cyberware.GetStatBonus` aggregates installed
   pieces and short-circuits unrelated stats without an engine call; the Magic-loss curve is monotonic
   (`round(maxFP * available/6)`), 0 loss at full Essence, near-total at 0.
2. **Full suite** — the `Stat.cs` fold-in touches the shared stat read (as the metatype term did), so
   run everything.
3. **Live gate** — talk to the street-doc, install Dermal Plating and Wired Reflexes on a troll street
   samurai: Defense/Evasion rise on the sheet and in combat, gold is deducted, Essence drops. Install
   enough chrome on a mage to watch Magic (FP) fall. Remove a piece: stats revert, Essence returns, FP
   recovers. Confirm a non-caster is unaffected by Magic loss.

**Gate:** a street samurai chromes up and gets visibly tougher/faster at a real Magic cost; a mage who
chromes up loses casting. The chrome-versus-magic tradeoff is legible and felt.

## Out of scope for this slice

Cyberware grades (alpha/beta/delta); bioware as a second Essence track; death at 0 Essence;
attribute-boosting and active cyberware; the full catalogue; cyberlimb visual models. All are faithful
additions gated on this slice playtesting well — revisit per D16.

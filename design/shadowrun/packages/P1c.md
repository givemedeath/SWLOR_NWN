# P1c — Glitches

*Detailed brief for Phase 1c of [design/shadowrun/PLAN.md](design/shadowrun/PLAN.md). On approval this
becomes `design/shadowrun/packages/P1c.md`. This is the deferred v1 package P7.*

## Context

Glitches are the third and last of the three behavioural fixes from [D1](design/shadowrun/DECISIONS.md)
that keep the Shadowrun vocabulary honest — the others (subtractive soak, wound modifiers) shipped in
P5/P6. A Shadowrun-literate player expects that sometimes an action goes wrong on its own terms: the
gun jams, the wires cross. Without glitches the dice-pool presentation is missing a signature beat.

The slice is small: a glitch check in the shared attack path that, when it fires, applies a brief
self-debuff plus a VFX cue and a combat-log line. It reuses the status-effect framework and the
critical-roll structure already in the combat hook.

**Locked decisions (this session):** a separate glitch roll (hit → minor glitch, miss → critical
glitch); the glitch rate scales down with competence; the effect is a self-debuff plus VFX; players
and NPCs both glitch.

## Why a separate roll

`ResolveAttackRoll` resolves a hit as `attackRoll = D100; isHit = attackRoll <= hitRate`, and hit rate
is capped at 95 ([Combat.cs](SWLOR.Game.Server/Service/Combat.cs)) — so rolls of 96–100 always miss.
Keying a glitch to a band of that single roll could therefore only ever coincide with a miss, losing
Shadowrun's signature "you succeed but something still goes wrong."

So a glitch gets its own `D100`, exactly mirroring the **critical roll** already rolled a few lines
later in the same hook ([ResolveAttackRoll.cs:251](SWLOR.Game.Server/Native/ResolveAttackRoll.cs)):
resolve the hit, then roll for a glitch. A glitch on a hit is **minor** (complication despite success);
a glitch on a miss is **critical** (it went badly). The rate scales down with the attacker's accuracy,
because in Shadowrun a large dice pool rarely rolls mostly 1s — competence buys reliability.

## Design

**Pure, testable core in `Combat` (mirrors `CalculateHitRate`/`CalculateCriticalRate`):**

- `CalculateGlitchRate(int accuracy)` → `clamp(BaseGlitchRate - accuracy / GlitchAccuracyDivisor,
  MinGlitchRate, BaseGlitchRate)`. Starting proposal: base 5%, divisor 30, floor 1% — a green runner
  (accuracy ~30) glitches ~4%, a veteran (~90) ~2%, an apex attacker ~1%. Playtest-tunable.
- `ResolveGlitch(bool isHit, int glitchRoll, int glitchRate)` → `GlitchOutcome` enum
  (`None` / `Minor` / `Critical`): `None` if `glitchRoll > glitchRate`, else `Minor` when the attack
  hit and `Critical` when it missed.

**Thin application from the hook:** `Combat.TryApplyGlitch(uint attacker, bool isHit, int accuracy)`
rolls `Random.D100(1)`, calls the two pure functions, and on a glitch applies the status effect, the
VFX, and the combat-log line. `ResolveAttackRoll` calls it once after hit/miss is decided — for every
attacker, so players and NPCs both glitch through the one shared path.

**Effects — two declarative status effects** (the `ExposeWeakPointStatusEffect` pattern: a class with
`StatGroup.Stats[...]`):

| Effect | Stats | Duration |
|---|---|---|
| `GlitchStatusEffect` (minor) | `AccuracyPercentAdjustment -15` | ~6s |
| `CriticalGlitchStatusEffect` | `AccuracyPercentAdjustment -25`, `EvasionPercentAdjustment -25` | ~12s |

Applied to the attacker via `StatusEffect.ApplyStatusEffect<T>(attacker, attacker, duration)`. Category
`Debuff`. For the slice these reuse existing `EffectIconType` values (e.g. `Confused` for minor,
`Stunned` for critical) so the icon pipeline is not dragged in; a dedicated glitch icon is deferred.

**VFX** (per the AGENTS VFX rule, chosen by gameplay moment — an instant self-impact on the attacker):
`VisualEffect.Vfx_Com_Sparks_Parry` for a minor glitch (a weapon spark), a stronger cue such as
`Vfx_Imp_Head_Electricity` for a critical glitch (gear shorting out), via
`ApplyEffectToObject(DurationType.Instant, EffectVisualEffect(...), attacker)` — the pattern used
throughout `Combat.cs`.

**Combat log:** a short line in `ColorToken` style — "X's weapon glitches!" / "X suffers a critical
glitch!" — so the moment is legible, using the player-identity display helpers already required by
AGENTS for names.

## Files

| File | Change |
|---|---|
| `Service/Combat.cs` | `CalculateGlitchRate`, `ResolveGlitch`, `GlitchOutcome`, `TryApplyGlitch`; glitch constants |
| `Native/ResolveAttackRoll.cs` | one `Combat.TryApplyGlitch(attacker, isHit, accuracy)` call after hit/miss resolves |
| `Feature/StatusEffectDefinition/GlitchStatusEffect.cs` | **new** — minor debuff |
| `Feature/StatusEffectDefinition/CriticalGlitchStatusEffect.cs` | **new** — critical debuff |
| `SWLOR.Game.Server.Tests/...` | glitch-rate and outcome-selection tests |

## Reuse (do not rebuild)

- `Random.D100(1)` and the critical-roll structure at
  [ResolveAttackRoll.cs:251](SWLOR.Game.Server/Native/ResolveAttackRoll.cs) — the template to mirror
- `StatusEffect.ApplyStatusEffect<T>(source, creature, duration)`
  ([StatusEffect.cs:767](SWLOR.Game.Server/Service/StatusEffect.cs)) — effect application
- `StatusEffectBase` + the `ExposeWeakPointStatusEffect` declarative pattern
- `StatType.AccuracyPercentAdjustment` / `EvasionPercentAdjustment` — the debuff stats
- `EffectVisualEffect` + `ApplyEffectToObject` — the VFX pattern used across `Combat.cs`
- `SWLOR.Game.Server/Readmes/VisualEffectReference.csv` — pick the exact VFX by moment, per AGENTS

## Verification

Build once, `-p:RunPostBuildEvent=Never`, then filtered tests.

1. **Unit** — `CalculateGlitchRate` is non-increasing in accuracy and clamped to `[MinGlitchRate,
   BaseGlitchRate]`; `ResolveGlitch` returns `None` above the rate, `Minor` on a hit within it, and
   `Critical` on a miss within it; the two status effects carry the expected negative stats.
2. **Full suite** — `TryApplyGlitch` runs in the shared attack hook, so run everything to confirm the
   combat path is unaffected when no glitch fires.
3. **Live gate** — with the glitch rate temporarily raised, attack repeatedly on a troll street sam:
   confirm a minor glitch fires on some hits (accuracy briefly drops, spark VFX, log line) and a
   critical glitch on some misses (larger/longer drop, stronger VFX). Confirm an NPC attacker also
   glitches. Restore the rate.

**Gate:** glitches fire at a believable rate, both varieties read clearly in VFX and the combat log,
and a more accurate attacker visibly glitches less. The dice-pool presentation now has its missing
beat.

## Out of scope for this slice

Disruptive mechanical effects (weapon jam skipping an attack, self-stagger); a dedicated glitch icon
and bespoke VFX; glitches on non-attack actions (casting, skill use); Edge spent to reroll a glitch
(the reserved Edge mechanic from [D6](design/shadowrun/DECISIONS.md)). All are follow-ons once the base
beat plays well.

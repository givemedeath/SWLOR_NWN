# Shadowrun Presentation Layer — Decisions

ADR-style record. One entry per decision, written **at the moment it is made**, not retrospectively.

Several constants in this project — the display divisor `K`, the wound-modifier free threshold, the
soak curve — are **empirical values with no derivation from the Shadowrun tabletop rules**. They exist
only to reconcile Shadowrun vocabulary with SWLOR's existing combat math. Without the reasoning
recorded here, nobody will be able to reconstruct why they hold their values, and any later attempt to
retune them will be guesswork.

**Entry format:**

```
## D<n> — <title>
**Date · Package · Status:** accepted | superseded by D<n>
**Decision:** what was chosen
**Why:** the reasoning, including what was rejected
**Revisit when:** the condition that should trigger reconsideration
```

---

## D1 — Convert the presentation layer, not the ruleset

**2026-07-22 · scope · accepted**

**Decision:** Translate SWLOR's player-facing surfaces into Shadowrun vocabulary and make three
targeted behavioural changes. Do not replace the combat resolution kernel with dice pools.

**Why:** A faithful ruleset conversion measures at 38–53 engineer-months — 3–4.5 engineer-years, not
viable below a team of four. Investigation found the player-facing combat surfaces are centralized
behind six choke points, two of which are data rather than code, making a presentation conversion
~9–12 weeks: roughly 4% of the faithful path.

The two are compatible rather than exclusive. Every choke point this work touches is one a faithful
conversion would have to touch anyway, so none of it is throwaway.

Rejected: pure display-only conversion with no behavioural changes (~6–9 weeks). It leaves the
vocabulary writing a cheque the math does not honour in three places obvious to any Shadowrun-literate
player — armor behaving multiplicatively, no wound feedback, no glitches.

**Revisit when:** the audience proves rules-literate enough that the remaining gap grates, or a team
of four or more becomes available.

---

## D2 — Display translation is coherent because ACC/EVA are already pool-shaped

**2026-07-22 · P0 · accepted**

**Decision:** Derive displayed Shadowrun dice pools directly from SWLOR's existing `ACC` and `EVA`
values rather than inventing a parallel stat model.

**Why:** `ACC = 8 + 2·skillRank + attribute + gearBonus` ([Stat.cs:1328]) and `EVA` share the same
shape. That is `skill + attribute + gear` — exactly how a Shadowrun dice pool is composed. SWLOR
already runs an opposed test (`hitRate = 75 + floor((ACC − EVA)/2)`, [Combat.cs:323]); it simply
collapses the difference to a percentage instead of keeping it as net hits.

This means the translation is a reinterpretation of numbers that already mean the right thing, not an
arbitrary mapping laid over unrelated values. It is the finding that makes the whole approach viable.

**Revisit when:** never, unless `Stat.GetAccuracy`/`GetEvasion` change shape.

---

## D3 — Subtractive soak replaces multiplicative mitigation

**2026-07-22 · P5 · accepted**

**Decision:** Replace `ratio = clamp(Attack/Defense, 0.01, 3.625)` in `Combat.CalculateDamageRange`
([Combat.cs:278-285]) with `finalDV = max(0, DV − soak)`.

**Why:** This is the single highest-leverage change in the project. Multiplicative mitigation means
every weapon does *some* damage to every target and nothing ever fully bounces. Subtractive mitigation
is what makes a hold-out pistol useless against an armored troll and an assault cannon indifferent to
that troll's armor — and it is why Armor Penetration is the stat Shadowrun players build around.

No amount of relabeling produces that texture. Accepted knowingly that it invalidates existing damage
balance; curves and `CombatDamageTests` expectations retune together with the change, which is why the
package is Lead-tier and solo rather than dispatched.

**Revisit when:** playtest shows the retuned curves cannot be made to work at level cap.

---

## D4 — Display divisor `K = 8`, calibrated to the behaviourally meaningful band

**2026-07-22 · P0 · accepted**

**Decision:** `AttackPoolDivisor = 8`. Displayed pool is `round((V - 8) / K)` for both `ACC` and `EVA`,
where the `- 8` removes the shared base term in `8 + 2·rank + attribute + gear`.

**Why:** Calibrated against the band where the game actually behaves differently, not against
theoretical extremes. `Combat.CalculateHitRate` is `75 + floor((ACC - EVA)/2)` clamped to `[20, 95]`
([Combat.cs:323](../../SWLOR.Game.Server/Service/Combat.cs:323), constants at
[Combat.cs:35-37](../../SWLOR.Game.Server/Service/Combat.cs:35)), so `ACC - EVA` only matters across
`[-110, +40]`. Outside that range nothing changes in play, and a display that kept resolving detail
there would be showing differences the engine ignores.

At `K = 8`:

- Player-range `ACC` (rank 0 with a low attribute, through rank 50 — the cap for 26 of the skills;
  languages cap at 20) maps to pools of roughly **1 to 18**, which is a readable Shadowrun span.
- The pool *difference* spans about **−13.75 to +5**. Deliberately asymmetric, because SWLOR's clamp
  is asymmetric: a character can fall much further below parity than above it. Displaying that
  honestly is better than flattening it.

**Known softness:** no hard attribute ceiling exists in code — attributes grow through AP with no
explicit cap — so the upper bound assumes a level-cap-scale attribute around 30–40. If a real ceiling
is established later, re-derive `K` against it.

**Revisit when:** a real attribute cap is defined, or playtest shows pools clustering too tightly to
distinguish builds at level cap.

---

## D5 — The display mapping is floored at zero and has no upper bound

**2026-07-22 · P0 · accepted**

**Decision:** Displayed pools, skill ratings and attributes clamp at **0 below** and are **unbounded
above**. No ceiling is applied at any point in the display layer.

**Why:** Strong NPCs and bosses must remain overtunable. `NPCStats`
([NPCStats.cs](../../SWLOR.Game.Server/Service/StatService/NPCStats.cs)) exposes `Level`, `Attack`,
`Evasion` and per-skill ranks as plain uncapped `int`s read from the creature's stat skin — entirely
independent of the player's 0–50 rank band. A boss is *expected* to sit far outside player range.

Capping the display would make a boss with `ACC 300` render identically to a strong player. That
hides threat at exactly the moment the player most needs to see it, and it would silently break the
monotonicity requirement this package is built around — a ceiling means two different `ACC` values
collapsing to the same displayed pool, which is the failure mode P0 exists to prevent.

This is also setting-correct rather than a compromise. Shadowrun's apex threats genuinely roll
enormous pools — great dragons and high-Force spirits sit well past anything a runner reaches — so
"that thing has a 37-dice pool" is the native way to express *out of your league*, not a display bug.

The floor at zero is kept because a negative dice pool is meaningless in Shadowrun, while zero dice is
meaningful: it reads as automatic failure.

Condition monitors need no special handling — `boxes = ceil(HP% · boxCount)` is proportional, so a
50,000 HP boss shows the same box count as a player. That is correct: the monitor communicates *how
hurt* something is, not how much HP it has.

**Revisit when:** never for the ceiling — this is a correctness constraint, not a tuning choice. The
floor could be revisited if a "negative pool" concept is ever introduced.

---

## D6 — FP is not Edge; Force splits into Spellcasting and Spell Defense

**2026-07-22 · P2b · accepted** *(supersedes the `FP → Edge` row of the P0 mapping in D4's package)*

**Decision:**

| SWLOR | Displayed as |
|---|---|
| FP pool | **Magic** — granular, 1:1, never compressed |
| FP Cost | Magic Cost |
| FP Regen | Magic Regen |
| Force Attack | **Spellcasting** |
| Force DEF | **Spell Defense** |
| *(reserved)* | **Edge** — left unused for a real Edge mechanic |

`ShadowrunDisplay.GetEdge` is renamed accordingly.

**Why:** P0 mapped FP to Edge on the surface similarity of "spendable resource". They are not the same
thing. Shadowrun's Edge is a 1–7 luck attribute spent on rerolls — a runner has about three. SWLOR's
FP is a mana pool consumed by ability costs of 2–9 against a base of 10 and a much larger cap.

Displaying FP as Edge would either lie about the scale (`Edge: 200`) or require compressing it, and
compression was measured rather than assumed:

| MaxFP | Casts showing **no** bar change under a 0–7 clamp |
|---|---|
| 30 | 10% |
| 60 | 44% |
| 100 | 67% |
| 150 | 79% |

That is the same defect as the attribute upgrade buttons in D5's wave, and worse in kind: FP is a live
gauge consulted mid-fight, not an occasional glance. **A resource bar must resolve every spend.**

Keeping FP off the Edge name also leaves Edge available for the mechanic it actually describes. Wave 3
adds glitches, and spending Edge to reroll a glitch is the natural pairing — that option stays open
only if nothing else has claimed the word.

Splitting Force into Spellcasting and Spell Defense avoids two collisions at once: Shadowrun already
uses "Force" for a spell's power rating, and calling the attack, the defense, and the pool all "Magic"
reads muddy on a single sheet.

**Revisit when:** a real Edge mechanic lands in Wave 3 and needs a home, or if playtest shows "Magic"
reads poorly for a resource that non-casters also see at zero.

> **Provisional as of 2026-07-22.** This decision makes FP an honest *bar*, which is the best available
> answer while FP remains a bar. It does not make it Shadowrun magic — Magic is an attribute rating and
> the real cost of casting is Drain, not depletion. Together with [D11](#d11--the-wound-penalty-reads-the-physical-track-only-with-three-free-boxes),
> which had to exclude the stun track from wound penalties for the mirror-image reason, this is the
> evidence that the resource *model* rather than its labelling is what disagrees with the setting. See
> **Deferred — `P9`** in [PLAN.md](PLAN.md). Do not invest further in polishing this presentation.

---

## D7 — Hak work lives on a fork, verified by guard rails at each wave gate

**2026-07-22 · haks · accepted**

**Decision:**

- Hak changes go on an **`adaptation/shadowrun` branch in a fork** of `SWLOR_Haks`, not upstream and
  never on a detached HEAD.
- Haks are rebuilt and the module repacked **at wave gates**, not per package.
- Asset integrity is enforced by `tools/orchestration/checkhaks.js`, documented in `HAKS.md`, rather
  than by automation that rebuilds silently.

**Why:** P4 was the first package to touch `SWLOR_Haks`, and it exposed that the submodule was on a
**detached HEAD** pointing at **upstream** — so the TLK edits had nowhere to be committed and any
commit made would have been orphaned. Nothing in the plan had recorded that haks even live in a
separate repository.

A fork rather than an upstream branch because a total conversion diverges permanently — tilesets,
portraits, item art, TLK — and that does not belong in the parent project's repo. A fork keeps upstream
pullable for genuine engine and tooling fixes.

Wave-gate cadence because 113 haks over ~13 GB is too slow to rebuild per package, and leaving it to
deploy would surface asset breakage far from whoever caused it.

Guard rails over full automation because **the .NET toolchain is blind to this entire class of
failure** — nothing in `dotnet build` or `dotnet test` reads a 2DA or a TLK, so a stale binary or a
dangling strref ships through a green build. Silent auto-rebuild on a 13 GB tree is also exactly where
hard-to-see breakage hides; the P4 experience argues for failing loudly instead.

The check earned itself immediately: on its first run it found a **pre-existing dangling strref** in
`racialtypes.2da` (`16858047` → missing id `80831`, with neighbours present), which renders as
`Bad Strref` in game and which nothing else in the toolchain would ever have reported.

**Revisit when:** the hak build gains real incremental support, making per-package rebuilds cheap
enough to move earlier.

---

## D8 — Subtractive soak applies to personal combat only; starships keep the proportional curve

**2026-07-22 · P5 · accepted** *(implements D3)*

**Decision:** `Combat.CalculateSoakDamageRange` is a new function implementing
`finalDV = max(0, DV − soak)`, used by personal combat. `Combat.CalculateDamageRange` keeps the
original `DV × (Attack/Defense)` curve and is used by starship combat.

Soak derives from the same display mapping as everything else:

```
soak = max(0, GetDefensePool(defenderDefense) − GetAttackPool(attackerAttack) + SoakAtParity)
```

`SoakAtParity = 6`, a named constant.

**Why split rather than change it globally:** the two systems shared one function, and 10 starship
module definitions route through it. Starships are explicitly out of scope for this conversion and
their module ratings are balanced against the proportional curve, so a global change would have
silently rebalanced a system nobody asked to touch — with no test coverage to catch it.

The caller split turned out to be perfectly clean, which made the separation nearly free:
`CalculateDamage` is used *only* by the ship modules, while `CalculateDamageWithCriticalMitigation`
is used *only* by personal combat (the native attack roll and two ability paths). Routing the latter
to the new function changes exactly the intended surface.

**Why the attacker's rating reduces soak:** subtraction alone would mitigate nothing at parity, and a
flat soak independent of the attacker would make penetration meaningless. Treating Attack as armor
penetration preserves the opposed character of the original while keeping the subtractive form — a
stronger attacker punches through more armor, which is the stat Shadowrun players build around.

**Parity soak must be proportional, not flat — corrected after live testing.** The first version used
a flat `SoakAtParity = 6`, calibrated against median weapon DV (30) and p95 (111). That is mid-game
gear. Starting weapons and low-level enemies carry DV 2–8, entirely below a flat soak of 6, so **a new
character and the weakest enemy in the module could not damage each other at all** — in either
direction.

Now `soak = max(0, DefensePool − AttackPool) + DefensePool × SoakParityPercent / 100`, with
`SoakParityPercent = 50`. The parity component scales with the defender instead of being fixed:

| Scenario | DV | soak | damage |
|---|---|---|---|
| Mynock → new character (real values) | 7 | 1 | 6 |
| New character → Mynock | 5 | 1 | 4 |
| Mid-tier parity | 30 | 3 | 27 |
| Cap-tier parity | 111 | 8 | 103 |
| Hold-out pistol vs heavy armor | 8 | 24 | **0** |
| Assault cannon vs same armor | 120 | 24 | 96 |

Note that parity mitigation is light at high tiers (7–10%) and heavy at low ones. That is inherent to
subtractive mitigation rather than a defect: armor matters most against weak attacks and least against
strong ones, which is the texture this change exists to create. What playtest still has to answer is
whether armor feels *worth building* at cap.

One behavioural difference worth stating: the proportional model floors any positive-DMG hit at 1
damage, so nothing ever fully bounces. The soak path deliberately does not, because a fully soaked hit
dealing zero **is the point** — flooring it at 1 would mean armor can never actually stop anything.

**Revisit when:** playtest shows armor is decisive or irrelevant at level cap, or when wound modifiers
(P6) change how much incoming damage matters.

---

<!-- New entries appended below. Record boxCount and the wound threshold as they are decided. -->

---

## D9 — An even contest is a coin flip; one displayed pool is worth 8%

**2026-07-22 · combat feel prototype · accepted**

**Decision:** Lower `Combat.BaseHitRate` from 75 to 50 and introduce
`Combat.HitRatePercentPerPool = 8`, which the hit-rate formula converts back to rating points through
`ShadowrunDisplay.PoolDivisor`. Starship combat keeps the original curve via a separate
`CalculateShipHitRate`.

**Why:** The pool display was decorative. Simulating the shipped auto-attack loop across the real
creature curve showed *every* evenly matched fight resolving at 74–76% — a two-point spread across
displayed pools running from 1 to 14. The numbers on screen moved and the outcome did not, which is
exactly the vocabulary-versus-math gap [D1](#d1--convert-the-presentation-layer-not-the-ruleset) set
out to narrow, and it was worse than the plan's risk section estimated.

At 75% base with a shallow slope, one displayed pool was worth 4 percentage points against a floor
that already sat near the maximum. At 50% base with 8 points per pool, an even contest reads as a
coin flip and a three-pool advantage swings the fight by 48 points.

Expressing the slope *per displayed pool* rather than per rating point is deliberate: it locks the
shown number to the felt outcome, so retuning `PoolDivisor` moves the slope with it instead of
letting the two drift apart silently.

`MinimumHitRate` stays at 20 rather than dropping with the base, so an overtuned boss remains
beatable with effort instead of becoming a wall. That floor is now reached at roughly a four-pool
deficit.

Rejected: leaving the base at 75 and accepting that pools are flavour. It makes every other piece of
the conversion dishonest — a player who reads "Pool 12 vs 9" and loses anyway learns the display
means nothing.

**Revisit when:** characters exist at level cap, or if the 8-point step proves too swingy in group
fights where several attackers stack against one defender.

---

## D10 — NPC health is divided by six as a stopgap for the pacing target

**2026-07-22 · combat feel prototype · accepted**

**Decision:** Apply `Combat.NPCHealthCurveDivisor = 6` to NPC maximum hit points only, scaling the
value handed to the engine and never the stored property. Players are untouched.

**Why:** Shadowrun firefights resolve in a handful of attacks. Simulation put evenly matched SWLOR
fights at 14 exchanges low-tier and 42 at prime, against a target of 3–12, because the creature curve
grows hit points far faster than damage — 58 → 12,900 HP while weapon damage goes 8 → 178.

The hit-rate change makes this *worse*, not better: halving the base rate lengthens every fight. A
sweep across base rate, slope, and health curve found no configuration reaching the pacing target on
either knob alone, which is why the two were searched together rather than tuned in sequence. Base
50 with health ÷6 lands every evenly matched tier between 3.4 and 10.4 exchanges.

Scaling only the value passed to `SetMaxHitPoints` — never the `NPCHP` property — matters because
that property is a running total re-read on every equip and unequip. Scaling it in place would
compound and shrink the creature a little more each time.

**This is a stopgap and should be deleted, not retuned.** The correct fix is authoring the health
curve into creature blueprints for a purpose-built module, per
[PLATFORM-ASSESSMENT.md](PLATFORM-ASSESSMENT.md): dividing an inherited Star Wars curve at runtime is
hiding a content problem in code.

**Revisit when:** the first purpose-built creature blueprints exist. Remove the constant then.

---

## D11 — The wound penalty reads the physical track only, with three free boxes

**2026-07-22 · P6 · accepted**

**Decision:** Derive an Accuracy and Evasion penalty from the physical condition monitor at one die
per three filled boxes, with the first three boxes free. Scale it by
`ShadowrunDisplay.PoolDivisor` so one die of penalty is exactly one displayed pool. Add
`StatType.WoundPenaltyFreeBoxes` as the offsetting stat.

**Why — the stun track is excluded deliberately.** [PLAN.md](PLAN.md) specified deriving the penalty
from HP *and* Stamina, matching Shadowrun, which sums the physical and stun tracks. That is wrong for
SWLOR: stamina here is an ability resource that players spend down as a matter of normal rotation, so
a stun-track penalty would charge players for *using abilities* rather than for being hurt. A caster
mid-rotation would read as critically wounded. The stun track earns a penalty only if it ever stops
doubling as a cost pool.

**Why three free boxes.** Shadowrun charges from the first box, which is dramatic across one tabletop
fight and punishing across an evening of respawn-and-retry. This was flagged in the plan as the only
real feel risk in the conversion, so it was measured rather than guessed: duelling every tier to
exhaustion over 400 seeds, with and without penalties, and comparing how much healthier the winner
finishes. A death spiral shows up as the winner walking away far fresher, because the loser stopped
being able to fight back.

At three free boxes the winner finishes 5–7 points healthier and fights run marginally *shorter*.
That is a tilt, not a rout. Penalties from the first box put the same figure far higher.

Scaling by the pool divisor keeps the character sheet honest: a player penalised one die watches
their displayed pool drop by exactly one.

`WoundPenaltyFreeBoxes` exists so pain editors and damage compensators are a stat adjustment rather
than a perk check, per the stat-driven rule in AGENTS.md. It can cancel the penalty outright.

Rejected: applying the penalty to Accuracy only. Shadowrun applies wound modifiers to every test
including defence, and one-sided application would make a hurt character better at dodging than at
shooting.

**Revisit when:** playtest shows whether three free boxes reads as forgiving or as making injury
meaningless. This is the constant most likely to move.

> The stun-track exclusion above is a workaround, not an answer. Under a Drain model the stun monitor
> is *filled* by damage rather than spent on abilities, at which point stun wound penalties become
> correct and this exclusion should be deleted. See **Deferred — `P9`** in [PLAN.md](PLAN.md).

---

## D12 — Shadowrun-flavored systems, hybrid content, original city, solo pace

**2026-07-22 · plan v2 · accepted**

**Decision:** Four project-shaping answers, settled together because each changes what the others cost.

| Question | Answer |
|---|---|
| Content | **Hybrid** — new district and creatures; keep item/recipe/quest data, convert as player-visible |
| Fidelity | **Shadowrun-flavored** — percentage kernel stays; cyberware, Essence, metatypes, Drain get built for real |
| Setting | **Original city, canonical world** — Sixth World rules and metatypes, invented sprawl |
| Team | **Solo + agent assistance** |

**Why fidelity is the load-bearing one:** it closes the resolution-kernel question permanently. No
dice-pool rewrite, no re-opening balance. What "flavored" buys is that the identity systems are real
builds rather than reskins — cyberware is not a renamed perk tree, and Essence is a genuine resource
with a genuine tradeoff. The v1 display layer stays as the vocabulary boundary.

**Why hybrid content:** stripping to an empty module gives the cleanest identity and nothing to play;
reskinning in place keeps Star Wars residue on every surface for years. Hybrid takes the systems —
the decade of work that is genuinely setting-neutral — and replaces only what a player looks at.

**Why an original city:** removes lore-policing entirely, and reduces trademark surface at no cost to
the setting, since metatypes, megacorps, and the Sixth World premise carry the identity rather than
Seattle specifically.

**Why this matters for IP:** Topps owns Shadowrun and licenses tabletop to Catalyst, but **Microsoft
holds the video game rights**, which is a more direct exposure for a video game than for a tabletop
fan project. No decision is forced today, but one must not be foreclosed: setting-specific vocabulary
stays behind `ShadowrunDisplay` rather than hardcoded across content files, so a forced rename is a
one-file change rather than a project-ending one.

**Revisit when:** Phase 3's gate is reached and the game loop is proven, or if a rules-literate
audience proves that flavored systems grate badly enough to justify the kernel rewrite.

---

## D13 — P8 (910 perk descriptions) is cut, not deferred

**2026-07-22 · plan v2 · accepted** *(supersedes the P8 package in the archived v1 plan)*

**Decision:** Do not rewrite the 910 `.Description(...)` literals into Shadowrun vocabulary.

**Why:** It is polish on text attached to perk trees that are Star Wars-shaped and slated for
replacement. Rewriting "reduces the target's Attack by 10%" into Shadowrun phrasing produces
Shadowrun-flavored Star Wars, at the cost of the single largest text package in the plan.

The work becomes worth doing only once the perk trees themselves are Shadowrun-native — at which
point the descriptions get written fresh alongside them rather than translated.

**Revisit when:** perk trees are rebuilt around cyberware, magic traditions, and Sixth World skills.

---

## D14 — Three content tiers, and authored Runs are the floor

**2026-07-22 · plan v2 · accepted**

**Decision:** Ship three distinct content tiers rather than treating "quests" as one thing. Build 8–12
authored Runs handed out by NPC Johnsons for launch, alongside the procedural contract board and the
GM event kit.

| Tier | Source | Provides | Available |
|---|---|---|---|
| Authored Runs | NPC Johnsons | The content floor — real missions with structure and stakes | Always |
| Contract board | Procedural | Repeatable grind between runs | Always |
| GM events | Live staff | The living world — consequence, surprise, story | When staffed |

**Why:** GMs are what make the world feel alive, and they cannot be online every night. A world with
only GM content is dead six evenings out of seven. A world with only procedural contracts is an MMO
wearing a Shadowrun skin. The authored Runs are what a player finds on a Tuesday when nobody is
running anything, and they are much harder to retrofit than either of the other two tiers — the
district layout has to be designed around where the Johnsons stand.

This makes Phase 2 and Phase 3 partially concurrent by necessity: each Johnson is a delivery point for
a specific Run, so the district map and the run roster are one design problem, not two.

**Why a small number:** 8–12 is enough to survive several evenings when mixed with repeatable entries
via `IsRepeatable()`, and few enough to author at solo pace with real quality. Quantity is what the
contract board is for.

**Verified rather than assumed:** the quest engine already carries 259 quests across 33 definition
files, and the dialog **snippet system** (`condition-on-quest-state`, `action-advance-quest`, and
friends) means legwork stages are authored in conversation rather than in code. Most of a run is
buildable today.

The gap is that only **two** objective types are declarative — `AddKillObjective` and
`AddCollectItemObjective`. Runs built on those alone are "kill N" or "fetch N", which is the exact MMO
texture authored Runs exist to escape. `Quest.AdvanceQuest` is callable from any script, so
reach-location and use-object stages are possible but hand-scripted. Making them first-class builder
methods is its own package, and it blocks the Runs from feeling varied.

**Revisit when:** the launch roster is played through and it becomes clear whether 8–12 lasts long
enough, or whether the objective vocabulary needs escort/stealth stages to carry the fiction.

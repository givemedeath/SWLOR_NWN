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

<!-- New entries appended below. Record boxCount and the wound threshold as they are decided. -->

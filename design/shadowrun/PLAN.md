# Shadowrun Presentation Layer for SWLOR

> **This file is the canonical plan of record.** Approved 2026-07-22.
>
> A working copy also exists in the approving session's plan directory
> (`~/.claude/plans/investigate-in-detail-a-merry-moler.md`). That copy is a session
> artifact and is *not* authoritative — it is machine-local and disappears with the session.
> This file is the version that ships with the repo and that anyone picking the work up should read.
>
> **Keeping the two in sync is automated.** `tools/orchestration/track.js` detects drift and warns
> at session end; `node tools/orchestration/syncplan.js` reports or reconciles it on demand. Any
> material change to scope, waves, packages, or verification must land *here*, and be recorded in
> `LEDGER.md` with the reasoning in `DECISIONS.md`. See "Plan of record" in `ORCHESTRATION.md`.

## Context

The `adaptation/shadowrun` branch is currently master with a different name. The question behind it was
whether SWLOR is a viable base for a Shadowrun conversion, and what that costs.

A full faithful-ruleset conversion measures at **38–53 engineer-months** — not viable below a team of
four. But the investigation found that **SWLOR's player-facing combat surfaces are centralized behind
six choke points**, two of which are data rather than code. That makes it possible to present the game
in Shadowrun terms for **~9–12 weeks, roughly 4% of the faithful path**, without touching the
opposed-test kernel, the 928 `StatType` entries, or the balance tuning.

This plan builds that presentation layer plus the three cheapest behavioural changes that keep the new
vocabulary honest — organized as **eight work packages across four waves**, with an orchestration
layer that dispatches cheap packages to parallel Sonnet subagents and tracks every task, decision, and
material change durably in-repo.

Nothing here is throwaway: every choke point touched is one a faithful conversion would touch anyway.

**Out of scope:** world, areas, items, creatures, dialog, quests — the real bulk of any conversion, and
unaffected by this work.

---

## Background from exploration

**SWLOR already owns its combat log.** [Combat.cs:9276](SWLOR.Game.Server/Service/Combat.cs:9276)
builds `"X attacks Y : *hit* : (75% chance to hit)"` itself, and that string already carries the
percentage that needs converting. [FeedbackMessageConfiguration.cs](SWLOR.Game.Server/Feature/FeedbackMessageConfiguration.cs)
already suppresses NWN's native `Initiative` and `ComplexAttack` lines through NWNX.

**The character sheet funnels every displayed stat through one helper.**
[CharacterSheetViewModel.cs:888](SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs:888)
is `AddStat(name, value, tooltip)` — 38 call sites — with `FormatPercent` at line 1081 the sole
percentage formatter.

**The HUD already switches labels contextually.** `PlayerStatusViewModel.ToggleLabels` (line 109) swaps
`HP:/STM:/FP:` for `SH:/HL:/CAP:` in ship mode, so a third label set is an established pattern.

**Item property names resolve through TLK.** [Item.cs:1155](SWLOR.Game.Server/Service/Item.cs:1155)
reads display names from `iprp_*` cost tables via strref — a TLK edit, no C#.

**The math being reinterpreted:**

```
ACC       = 8 + 2·skillRank + attribute + gearBonus          Stat.cs:1328
EVA       = 8 + 2·armorRank + AGI + (AC·5 + evasionBonus)    Stat.cs:2034
hitRate   = 75 + floor((ACC − EVA)/2) + mods,  clamp[20,95]  Combat.cs:323
ratio     = clamp(Attack / Defense, 0.01, 3.625)             Combat.cs:278
maxDamage = (weaponDMG + statDelta) · ratio                  Combat.cs:285
```

`ACC` and `EVA` are already `skill + attribute + gear` — exactly how a Shadowrun dice pool is composed.
The system already runs an opposed test; it just collapses the difference to a percentage instead of
keeping it as net hits. That is why a display translation can be coherent rather than arbitrary.

---

## Orchestration layer

### Roles and tiering

Per established preference, tier is a **ceiling, not a floor**: dispatch self-contained packages to
Sonnet; keep judgement-heavy work with the controller; keep changes smaller than their brief inline.

| Tier | Who | Criteria |
|---|---|---|
| **Lead** | Controller (Opus), inline | Semantic core, balance tuning, anything empirical or cross-cutting. Shared-file packages. |
| **Mid** | Sonnet subagent | Self-contained, single-file-cluster, spec is writable in advance |
| **Low** | Sonnet subagent, fan-out | High-volume mechanical work, verifiable by test or script |
| **Inline** | Controller | Changes where the brief would cost more than the edit |

### The file-ownership rule

**Two files are each touched by two different packages, and this is what makes naive parallel dispatch
fail:** `Combat.cs` (combat log + subtractive soak) and `StatType.cs`/`Stat.cs` (wound modifier +
accuracy reads). Packages sharing a file are **never dispatched concurrently** — they are merged into
one package under one owner, or serialized across waves.

Every package brief declares its owned file set. The controller verifies disjointness before
dispatching a wave. For anything riskier, `Agent(isolation: "worktree")` gives the subagent its own
git worktree — the repo already uses this (`.claude/worktrees/`).

### Wave structure

```
Wave 0  ── P0 ShadowrunDisplay ──────────────────── Lead, solo (everything depends on it)
                    │
Wave 1  ── P1 Feedback  P2 CharSheet  P3 HUD  P4 TLK/2DA ── 4 × Sonnet, parallel (disjoint)
                    │
Wave 2  ── P5 Combat.cs cluster ── P6 Wound modifier ───── Lead (shared files, balance-critical)
                    │
Wave 3  ── P7 Glitches  P8 Perk descriptions ──────────── Sonnet, parallel + fan-out
```

### Two-tier tracking

**Live coordination — the built-in task list.** `TaskCreate` seeds one task per package;
`TaskUpdate` carries `owner` (subagent claims), `addBlockedBy` (wave gates), and `metadata`
(`tier`, `wave`, `ownedFiles`). Subagents call `TaskList`/`TaskGet` to read their brief and
`TaskUpdate` to resolve. This is the coordination substrate and it enforces the wave gates mechanically.

**Durable record — git-committed, in-repo.** The task list is session-scoped and not guaranteed to
survive a restart, so the authoritative record lives in `design/shadowrun/`:

| Artifact | Contents |
|---|---|
| `ORCHESTRATION.md` | This protocol: roles, tiers, file-ownership rule, wave gates, dispatch-brief template |
| `LEDGER.md` | Append-only. Every dispatch, completion, and material change, timestamped |
| `DECISIONS.md` | ADR-style. Every decision with rationale — `K`, the wound threshold, soak curve, label wording |
| `packages/P0…P8.md` | One brief per package — the artifact handed to the subagent |

`DECISIONS.md` matters more than it looks. `K = 8`, the wound-modifier threshold, and the soak curve
are **empirical constants with no derivation from the tabletop rules** — they exist to reconcile
Shadowrun vocabulary with SWLOR's math. Six months on, nobody will remember why they hold those values
unless the reasoning is written down at the moment it's decided.

### Automatic capture

Hooks in a new project-level `.claude/settings.json` (none exists today; only
`settings.local.json` with permissions):

| Hook | Action |
|---|---|
| `PostToolUse` on `Edit`\|`Write` | Append `{timestamp, file, package}` to `design/shadowrun/events.jsonl` |
| `SubagentStop` | Append package completion + owned-file diff summary to `LEDGER.md` |
| `Stop` | Flush session summary; flag any package left `in_progress` |

Hooks can't infer which package an edit belongs to, so the controller writes
`design/shadowrun/.current-package` before each dispatch and the hook reads it. Raw events land in
`events.jsonl`; the controller reconciles them into `LEDGER.md` prose at each wave gate. Use the
`update-config` skill to author the hook configuration rather than hand-editing settings.

---

## Work packages

### Wave 0 — `P0` `ShadowrunDisplay` service · **Lead** · solo

New `SWLOR.Game.Server/Service/ShadowrunDisplay.cs` holding every mapping, so the translation is
auditable in one place and testable without a running server.

| SWLOR value | Displayed as | Mapping |
|---|---|---|
| `ACC` | Attack Pool | `round((ACC − 8) / K)`, `K = 8` → pools ~2–18 |
| `EVA` | Defense Pool | same divisor |
| `chanceToHit` | `(Pool 12 vs 9)` | replaces the `%` in combat log lines |
| `weaponDMG` | DV | direct |
| `HP` / `MaxHP` | Physical condition monitor | `boxes = ceil(HP% · boxCount)` |
| `Stamina` | Stun condition monitor | same formula |
| `FP` | Edge | direct |
| skill rank (1–50) | SR skill rating | `ceil(rank / 8)` → 1–7 |
| attribute score | SR attribute | divisor chosen to land 1–9 |

**The mapping must be monotonic.** Two `ACC` values rounding to the same displayed pool but producing
visibly different hit rates is the failure players find first. Tests assert monotonicity and stability
across the full stat range, not spot values. Keep `K` a named constant — it is the single knob
governing how the whole game reads, and it will be retuned once characters exist at level cap.

Lead-tier because every other package consumes this contract; getting the shape wrong is expensive to
undo. Record `K` and the box-count choice in `DECISIONS.md`.

### Wave 1 — parallel, 4 × Sonnet, disjoint files

| # | Package | Tier | Owned files |
|---|---|---|---|
| `P1` | **Feedback suppression** — extend `SetCombatLogMessageHidden` to cover NWN lines leaking d20 vocabulary | Low | `Feature/FeedbackMessageConfiguration.cs` |
| `P2` | **Character sheet** — relabel 38 `AddStat` sites, add `FormatPool` beside `FormatPercent` (1081), update attribute bindings (842–848). Rewrite tooltips, don't just relabel | Mid | `Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs` |
| `P3` | **HUD / portrait / target** — add SR label set to `ToggleLabels` (`PHYS:`/`STUN:`/`EDGE:`); same for portrait and target VMs | Mid | `PlayerStatusViewModel.cs`, `PlayerStatusPortraitViewModel.cs`, `TargetStatusViewModel.cs` |
| `P4` | **TLK + 2DA relabeling** — item/ability display names | Mid | `sw_tlk.tlk.json`, `sw_2da/iprp_*`, `feat.2da`, `spells.2da` |

`P4`'s brief must carry the AGENTS.md TLK rules verbatim: reuse empty slots before appending, use
`16777216 + tlkId` for 2DA references, regenerate `sw_tlk.tlk` before handoff. That is exactly the kind
of precise, rule-bound, self-contained work a Sonnet subagent does well and a controller wastes tokens on.

### Wave 2 — Lead, serialized on shared files

**`P5` — `Combat.cs` cluster.** The combat log builders and subtractive soak both live in `Combat.cs`,
so they are one package under one owner.

- Log builders ([9276+](SWLOR.Game.Server/Service/Combat.cs:9276)): replace
  `({chanceToHit}% chance to hit)` with pool-vs-pool. Keep `ColorToken.Combat` and
  `PlayerName.GetColoredDisplayName` — the AGENTS.md player-identity rules still apply.
- **Subtractive soak** ([278–285](SWLOR.Game.Server/Service/Combat.cs:278)): replace the multiplicative
  `ratio = Attack/Defense` with `finalDV = max(0, DV − soak)`. This is the highest-leverage change in
  the project. Multiplicative mitigation means everything does *some* damage to everything and nothing
  ever bounces; subtractive mitigation is what makes a hold-out pistol useless against an armored troll
  and an assault cannon indifferent to armor. No relabeling produces that texture.

Expect to retune damage curves; `CombatDamageTests.cs` covers this path and moves with the change.
Lead-tier because it is empirical balance work, not spec-following.

**`P6` — Wound modifier.** One new `StatType` derived from HP% and Stamina%, applied as a penalty to
`ACC` and `EVA`. Declare it with `StatTypeAttribute` (`StatTypeCategory.NonBeneficial`) per the
AGENTS.md stat-driven rule, so shared systems read enum metadata rather than special-casing — which
also makes cyberware like Damage Compensators a plain offsetting stat later.

Ship with a **free threshold before penalties begin.** Shadowrun's `−1 per 3 boxes` is dramatic over
one tabletop fight and punishing over an evening of respawn-and-retry. This is the one item with real
feel risk: make the threshold a constant, tune it in playtest, record the value and reasoning in
`DECISIONS.md`.

Touches `Stat.cs` and `StatType.cs` — shared with `P5`'s reads, so same wave, same owner, serialized.

### Wave 3 — parallel, Sonnet

**`P7` — Glitches** · Mid. Reuse the existing critical/fumble path plus the status-effect framework:
`StatusEffect.ApplyStatusEffect` is already called from `Combat.cs` in five places, with 332 status
effect definitions to pattern-match. A glitch is a brief accuracy debuff plus a VFX cue; a critical
glitch is longer. Brief must carry the AGENTS.md VFX rule — pick from `VisualEffectReference.csv` by
gameplay moment, not constant name.

**`P8` — Perk descriptions** · Low, fan-out. 910 `.Description(...)` literals across
`Feature/PerkDefinition/`, full of SWLOR vocabulary ("reduces the target's Attack by 10%"). The bulk of
the remaining text work and the best parallelism opportunity in the plan: split by definition file
across several Sonnet subagents, each owning a disjoint file set.

Drive through the Design Bible workbook and `tools/SyncCombatBibleDescriptions.py` per the AGENTS.md
workbook rules — **never `openpyxl`**, which silently destroys cached formula values. Every subagent
brief must carry that prohibition; it is the single most likely way this package causes damage.

---

## Critical files

| File | Package | Change |
|---|---|---|
| `Service/ShadowrunDisplay.cs` | P0 | **new** — all mappings |
| [Service/Combat.cs](SWLOR.Game.Server/Service/Combat.cs) | P5 | log builders (9276+), soak (260–308) |
| [Service/Stat.cs](SWLOR.Game.Server/Service/Stat.cs) | P6 | wound-modifier read into `GetAccuracy`/`GetEvasion` |
| [Service/StatService/StatType.cs](SWLOR.Game.Server/Service/StatService/StatType.cs) | P6 | one entry with `StatTypeAttribute` |
| [CharacterSheetViewModel.cs](SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs) | P2 | 38 `AddStat` sites, `FormatPool`, bindings |
| [PlayerStatusViewModel.cs](SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/PlayerStatusViewModel.cs) | P3 | `ToggleLabels` label set |
| [FeedbackMessageConfiguration.cs](SWLOR.Game.Server/Feature/FeedbackMessageConfiguration.cs) | P1 | extend suppression list |
| `sw_tlk.tlk.json`, `sw_2da/*` | P4 | display-name relabeling |
| `Feature/PerkDefinition/*.cs` | P8 | 910 descriptions via Bible pipeline |
| `design/shadowrun/*`, `.claude/settings.json` | — | orchestration artifacts and hooks |

**Reuse rather than rebuild:** `ColorToken.Combat` for log styling,
`PlayerName.GetColoredDisplayName` for all names, `StatusEffect.ApplyStatusEffect` for glitches,
`Stat.GetStatAdjustment` for the wound modifier, `GuiStandardLayout` for any NUI work,
`tools/SyncCombatBibleDescriptions.py` for bulk descriptions.

---

## Verification

Per AGENTS.md, build once and test many, always with `-p:RunPostBuildEvent=Never` to skip the slow
Windows deploy.

```bash
dotnet build SWLOR.Game.Server.Tests/SWLOR.Game.Server.Tests.csproj -p:RunPostBuildEvent=Never
```

**Per package** — every subagent brief ends with its own gate, and no package resolves until it passes:

1. **P0** — new mapping tests assert `ShadowrunDisplay` is monotonic and stable across the full
   `ACC`/`EVA`/HP/skill-rank ranges. The test that catches the failure players find first.
2. **P5** — extend `CombatDamageTests` for subtractive soak: specifically that low-DV attacks bounce
   off high armor and high-DV attacks punch through, which the multiplicative model could not produce.
   ```bash
   dotnet test --no-build --filter "FullyQualifiedName~CombatDamageTests|FullyQualifiedName~ShadowrunDisplay"
   ```
3. **P6** — free threshold holds; penalty reaches both `ACC` and `EVA`; `StatTypeAttribute` correct.
4. **P4/P8** — data packages verify by regeneration, not by eye: `sw_tlk.tlk` regenerates cleanly, and
   the Bible audit CSVs refresh without losing cached formula values.

**Per wave gate** — controller reconciles `events.jsonl` into `LEDGER.md`, confirms every package
resolved in the task list, and runs the full unfiltered suite before opening the next wave.

**Before handoff** — full suite, then in-game via `debugserver/docker-compose up --build`: combat log
reads in pool terms, HUD shows PHYS/STUN/EDGE, character sheet shows pools not percentages, a glitch
fires with its VFX. After TLK/2DA edits, regenerate `sw_tlk.tlk`, run `SWLOR_Haks/BuildHaks.cmd` and
`Module/PackModule.cmd`, and verify item property names on examine. If any icon changed, run
`tools/UpdateGameplayIconStandards.ps1 -AuditOnly` and fix every failure.

---

## Risks

**The wound modifier is the only real feel risk.** Everything else is presentation or a
well-understood math swap. Ship the free threshold, playtest, be willing to set it high.

**Subtractive soak will break balance — that is the point.** But it means damage curves and
`CombatDamageTests` expectations retune together, not separately. Keeping `P5` Lead-tier and solo is
deliberate.

**Parallel dispatch fails on shared files.** `Combat.cs` and `StatType.cs`/`Stat.cs` are each wanted by
two packages. The wave structure exists to serialize exactly those; the disjointness check before each
wave is not optional.

**The vocabulary still outruns the math in places.** Pools shown as `12 vs 9` imply ~41% to someone who
knows Shadowrun; the game hits ~75%. Edge won't do Edge things. This plan narrows the gap at the three
points that matter most; it does not close it. If the audience proves rules-literate enough that this
grates, real dice pools are the escalation — and every choke point touched here is scaffolding for it.

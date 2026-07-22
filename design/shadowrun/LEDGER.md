# Shadowrun Presentation Layer — Ledger

Append-only record of every dispatch, completion, and material change. Newest entries at the bottom.

Raw hook-captured events land in `events.jsonl`; the controller reconciles them into prose here at
each wave gate. See `ORCHESTRATION.md` for the protocol and `DECISIONS.md` for rationale.

**Entry format:** `YYYY-MM-DD · <package> · <event> · <actor>` followed by an indented summary.

---

## Setup

**2026-07-22 · — · plan approved · controller**

Scope settled after investigation: display-layer conversion of all player-facing surfaces to Shadowrun
vocabulary, plus three behavioural fixes (subtractive soak, wound modifiers, glitches). Full faithful
ruleset conversion measured at 38–53 engineer-months and was rejected as not viable below a team of
four; this path is ~9–12 weeks and every choke point it touches is scaffolding a faithful conversion
would need anyway.

Out of scope: world, areas, items, creatures, dialog, quests.

**2026-07-22 · — · task graph seeded · controller**

Eleven tasks created (#1–#11): two setup, nine work packages `P0`–`P8`. Wave gates encoded as
`blockedBy` dependencies:

- `#3` (P0) ← `#1`, `#2` — orchestration must exist before work starts
- `#4`–`#7` (P1–P4, wave 1) ← `#3` — all consume the `ShadowrunDisplay` contract
- `#8` (P5, wave 2) ← `#3`
- `#9` (P6) ← `#8` — shares `Stat.cs`/`StatType.cs` reads with P5, must serialize
- `#10` (P7, wave 3) ← `#8`
- `#11` (P8, wave 3) ← `#3`, `#7` — vocabulary must be settled in TLK before descriptions are rewritten

**2026-07-22 · — · orchestration artifacts created · controller**

`design/shadowrun/` established: `ORCHESTRATION.md` (protocol, tiering, file-ownership rule, wave
gates, dispatch-brief template), `LEDGER.md` (this file), `DECISIONS.md`.

Package briefs are authored at wave-open time rather than up front, so each wave's briefs can
incorporate what the previous wave learned.

---

**2026-07-22 · — · automatic tracking configured and verified live · controller**

Created project-level `.claude/settings.json` with three hooks — `PostToolUse` on `Edit|Write`,
`SubagentStop`, and `Stop` — all routing to `tools/orchestration/track.js`. Existing
`settings.local.json` permissions were left untouched (separate file, merged at load).

Hook logic is a Node script rather than a shell one-liner: **`jq` is not installed in this
environment**, so the canonical `jq`-based hook patterns do not work here. Node is a real install at a
stable path and has JSON natively.

Verified live: a real `Edit` to `ORCHESTRATION.md` produced a correctly attributed event
(`pkg: "setup"`) in `events.jsonl`. Hook fired without needing a session restart.

`tools/orchestration/selftest.js` covers all seven behaviours (Windows and POSIX paths, self-write
exclusion, malformed and empty payloads, package attribution, subagent and stop modes) and sandboxes
itself through `CLAUDE_PROJECT_DIR` so it never writes to the real ledger.

**Finding worth keeping:** verification cost far more than implementation, because a tracking hook
that silently does nothing is indistinguishable from a working one. The script was correct early on;
the apparent failures were malformed *test fixtures* — shell `echo` and heredocs collapse `\\` to `\`
in Windows paths, yielding invalid JSON escapes that `JSON.parse` rightly rejects. Build hook payloads
with `JSON.stringify`, never by hand. `track.js` also carries a `TRACK_DEBUG=1` switch, kept
deliberately: the whole difficulty was that failures were invisible.

**2026-07-22 · — · tracking defects found in review and fixed · controller**

Reviewing `track.js` before opening Wave 0 turned up three defects, one of which had already
produced a junk entry in this file (since removed):

1. `SubagentStop` appended boilerplate to `LEDGER.md` unconditionally, including for subagents that
   changed nothing — polluting the curated record it exists to protect. It now writes only when the
   dispatch actually produced edits, and lists the files it touched.
2. `Stop` claimed to summarise "this session" but counted every event in the file. Because
   `events.jsonl` is gitignored scratch that survives across sessions, those counts inflated without
   bound. Both summaries now measure from the last boundary marker via `eventsSince()`.
3. Paths outside the repo relativised to `../../..` chains. They now stay absolute and carry
   `external: true`.

`selftest.js` grew from 7 cases to 9, covering each fix — notably that a no-op subagent leaves the
ledger byte-identical. All passing.

**2026-07-22 · — · plan of record moved into the repo, with sync guards · controller**

The approved plan previously existed only at `~/.claude/plans/investigate-in-detail-a-merry-moler.md`
— machine-local, outside the repo, and lost when the session ends. It is now committed as
`design/shadowrun/PLAN.md` and marked canonical with a provenance header.

Three guards keep the copies aligned, per "Plan of record" in `ORCHESTRATION.md`:

- `tools/orchestration/syncplan.js` — on-demand drift check at `## `-section granularity, with
  `--to-repo` to reconcile while preserving the repo header. Doubles as a CLI and a module
  (`require.main` guard) so the comparison logic is not duplicated.
- The `Stop` hook emits a `systemMessage` naming the drifted sections when the copies diverge. A
  missing session copy is deliberately *not* treated as drift — outside the approving session there is
  nothing to compare, and a guard that cries wolf gets ignored.
- Edits to either plan copy are recorded with `plan: true` in `events.jsonl`.

Verified end to end: in-sync detection, drift detection, correct drift *direction* (a label-swap bug
was found and fixed during testing — it reported session-only additions as repo-only), addition sync,
deletion sync, and header preservation across a reconcile. `selftest.js` is now 12 cases, all passing.

**2026-07-22 · P0-brief · first live subagent dispatch — succeeded, found two defects · controller**

Dispatched a Sonnet subagent to author `design/shadowrun/packages/P0.md`, chosen as the test case
because it is real work the protocol requires (briefs are authored at wave-open), self-contained, and
owns a file no other package touches.

**Result: the deliverable is good.** All five template sections present, both verification commands
named, and the subagent independently verified the three cited source anchors against the actual code
— catching that `BaseHitRate`/`MinimumHitRate`/`MaximumHitRate` at `Combat.cs:35-37` pin the clamp
exactly, which the dispatch brief had not cited. File-ownership discipline held: it edited exactly one
file and *reported* two out-of-scope observations rather than fixing them.

**Defect 1 — subagents cannot reach the task tools.** The brief told it to claim and resolve task #12
via `TaskGet`/`TaskUpdate`; those tools do not exist in a subagent session. It searched, failed, and
reported the gap instead of inventing a workaround. Protocol corrected: **the controller owns all
task-list state.** Controller sets `in_progress` before dispatch and `completed` only after checking
the gate itself; briefs now end with "report in your final message". This is the better design
regardless — verification belongs with the party that can verify, not with the party self-reporting.

**Defect 2 — subagent completion summaries over-attributed.** The auto-captured entry this replaces
claimed "16 edits across 6 files" for a one-file package. `events.jsonl` was correct throughout; the
summary window was not. It was bounded by the *previous* `SubagentStop`, so it swept in every
controller edit made between dispatches. Now bounded by whichever of `SubagentStart`/`SubagentStop`
came last — a new `SubagentStart` hook marks the true opening, and including `SubagentStop` in the
boundary set prevents two dispatches without an intervening start from both claiming the same edits.

`selftest.js` is now 13 cases, all passing, including one asserting a subagent summary excludes
controller edits made before its dispatch.

**Worth noting:** both defects were invisible to the subagent and to the events log — they only
surfaced by reading the ledger entry against what actually happened. Auto-capture is a starting point
for reconciliation, never a substitute for it.

---

## Wave 0

**2026-07-22 · P0 · ShadowrunDisplay service delivered · controller**

New `SWLOR.Game.Server/Service/ShadowrunDisplay.cs` and
`SWLOR.Game.Server.Tests/Service/ShadowrunDisplayTests.cs`. Gate passed: build clean (0 errors),
**14/14 ShadowrunDisplay tests passing**, and 82 neighbouring combat/stat tests still green
(`CombatDamageTests`, `PerkStatBonusTests`, `CombatAttackDelayTests`). Owned-file discipline held —
`git status` shows exactly the two new files.

Decisions recorded as D4 (`PoolDivisor = 8`, calibrated against the `[20,95]` hit-rate clamp, which
means `ACC − EVA` only matters across `[-110, +40]`) and D5 (floored at zero, **unbounded above**).

D5 came from a scope note during implementation: strong NPCs and bosses must stay overtunable.
`NPCStats` exposes `Level`, `Attack`, `Evasion` and per-skill ranks as uncapped `int`s read from the
creature's stat skin, entirely independent of the player's 0–50 band. Capping the display would render
a boss identically to a strong player — hiding threat precisely when it matters — and would have
broken the monotonicity guarantee the package exists to provide, since a ceiling collapses distinct
ratings onto one displayed value. It is also setting-correct: Shadowrun's apex threats genuinely roll
enormous pools, so "37 dice" is the native way to say *out of your league*. Tests assert this directly
rather than leaving it to a comment, so a later "sensible" clamp fails loudly.

**2026-07-22 · P0 · tracking limitation found and documented · controller**

The hook silently dropped two of four edits during a rapid burst (`ShadowrunDisplay.cs` and a
`DECISIONS.md` edit) while capturing the writes immediately before and after. A follow-up probe
captured normally; the failure did not reproduce and has no confirmed root cause. Separately,
`.current-package` proved able to go stale — a marker set for a cancelled dispatch mis-attributed
subsequent orchestration work as `P0`.

Rather than paper over either, `ORCHESTRATION.md` now states plainly that **`events.jsonl` is a hint
and git is ground truth**, and requires cross-checking `git status` at every wave gate. The durable
record survives a lossy hook precisely because `LEDGER.md` and `DECISIONS.md` are written
deliberately rather than derived from the event log.

---

## Wave 1

**2026-07-22 · W1 · P1–P3 dispatched in parallel and delivered · controller**

Three Sonnet subagents ran concurrently against disjoint owned files. **Gate passed: build clean,
986/986 tests passing.** File-ownership discipline held perfectly — `git status` shows exactly the
owned files and no strays; no subagent reached into a test file or another package's territory.

- **P1** — six d20-vocabulary combat-log types suppressed (`SavingThrow`, `TouchAttack`,
  `SpellResistance`, `Counterspell`, `DispelMagic`, `Polymorph`), each with a justifying comment.
  Damage, death, `Feedback` and `CastSpell` left alone as the brief required.
- **P2** — character sheet converted: Accuracy/Evasion now render as pools, DMG as DV, regen rows
  relabelled Physical/Stun/Edge. A `FormatPool` formatter sits beside `FormatPercent`, and roughly
  twenty stats with no Shadowrun equivalent were deliberately left in SWLOR terms.
- **P3** — HUD labels converted to `PHYS:` / `STUN:` / `EDGE:`; starship branch untouched.

**Two controller repairs during gate verification.** Both were my scoping errors, not subagent
failures, and both were caught only by checking the result rather than trusting the report:

1. **Label/value mismatch.** P2 owned the ViewModel but the static labels live in
   `CharacterSheetDefinition.cs`, so the sheet rendered "Accuracy: 5 — *Chance to hit.*" where 5 was
   now a dice pool. Actively wrong, and worse than not converting. Relabelled to Attack Pool /
   Defense Pool with corrected tooltips.
2. **Dead upgrade buttons.** P2 converted attribute scores to Shadowrun ratings, but those rows carry
   the AP upgrade buttons, and the rating scale compresses about five raw points into one displayed
   point — **82% of upgrades would have shown no change at all**, making the button read as broken.
   Reverted to raw scores with the reasoning in a code comment. An allocation surface has to show the
   units being allocated; flavour belongs on surfaces that are not also controls.

**P3 corrected my brief, correctly.** I had assumed `TargetStatusViewModel` served an on-foot target
frame. It does not: its `Bar1Label`–`Bar3Label` are dead properties, `TargetStatusDefinition.cs`
hardcodes `SH:`/`HL:`/`CAP:`, and the window only ever opens for a ship target. **There is no
ground-combat target frame in this codebase at all.** The subagent refused the conversion and
explained why rather than relabelling starship data as PHYS/STUN/EDGE. Condition-monitor boxes were
likewise declined — the bar regions are too tight without a layout change, which the brief put out of
scope. Both are the judgement the file-ownership rule is meant to produce.

**Follow-up opened as task #13 (P2b).** Roughly seven further label renames are locked by tests that
assert on literal `AddStat` text (`CharacterSheetStatCoverageTests`, `CharacterSheetCombatUpgradeTests`,
`DevicesFieldSupportAndAssaultGadgetsTests`). Those test files sat outside P2's owned set, so labels
and assertions must move together in a package that owns both.

**2026-07-22 · P4 · deferred — not ready to dispatch · controller**

P4 (TLK/2DA display names) was pulled from Wave 1 before dispatch. Recon showed the surface is far
larger and sharper than the plan assumed: 22,284 custom TLK entries with 135 "Accuracy", 149
"Evasion" and 331 "Defense" occurrences, and — the deciding factor — `itempropdef.2da`'s `Name`
column points at **base-game `dialog.tlk` strrefs**, not custom ones. Renaming a property type means
repointing each row at a new custom entry at `16777216 + id`, with every mismatch a silent breakage.

Sending a subagent into that with "rename display names" and no bounded target set would have been
negligent. P4 needs a controller scoping pass to enumerate exactly which property types surface to
players before any brief is written. It also gates P8, so the sequencing cost of getting it right is
real but acceptable.

**2026-07-22 · P2b · test-locked labels finished; FP/Edge mapping corrected · controller**

**Gate passed: build clean, 987/987 tests passing** (one new test added).

The seven labels P2 could not reach are renamed, and the three source-scanning tests that assert them
were updated in the same change. Those tests read `CharacterSheetViewModel.cs` and
`CharacterSheetDefinition.cs` as *text* and assert on exact `AddStat("Label", expression)` substrings —
they guard coverage, not wording, so moving label and assertion together preserves their intent.

| Was | Now |
|---|---|
| Accuracy % | Attack Pool % |
| Evasion % | Defense Pool % |
| Physical DEF % / Physical DEF | Armor % / Armor |
| Force DEF % / Force DEF | Spell Defense % / Spell Defense |
| Force Attack % / Force Attack | Spellcasting % / Spellcasting |
| FP Cost | Magic Cost |
| STM Cost | Stun Cost |
| Ranged Evasion | Ranged Defense Pool |

**The substantive finding was a mis-mapping inherited from P0**, recorded as D6. FP had been mapped to
Edge on the surface similarity of "spendable resource", but Shadowrun's Edge is a 1–7 luck attribute
spent on rerolls while SWLOR's FP is a mana pool drained by ability costs of 2–9 against a base of 10.

The proposed fix — clamp FP onto an Edge-sized range — was measured rather than argued, and it fails:
44% of casts would show no bar change at a 60 pool, 79% at 150. That is the attribute-upgrade defect
from Wave 1 in a worse place, because FP is a gauge players read mid-fight. `ShadowrunDisplay.GetEdge`
became `GetMagicPool`, kept strictly 1:1, with a test asserting it is never compressed. Leaving the
Edge name unclaimed also keeps it available for a real Edge mechanic, which pairs naturally with
Wave 3's glitches.

**One test escaped the first sweep** — `CharacterSheetCombatUpgradeTests` asserts against
`CharacterSheetDefinition.cs`, not the ViewModel, so a grep scoped to the ViewModel missed it. Caught
by the full suite. Worth remembering that these source-scanning tests span *two* files.

The `CharacterSheetCombatUpgradeTests.cs` filename still carries the `CombatUpgrade` milestone label
that AGENTS.md prohibits. Left alone deliberately — out of scope for this package, still open.

**2026-07-22 · P4 · item-property vocabulary converted in the TLK · controller + sonnet**

**Gate passed: build clean, 987/987 tests passing.** 39 TLK entries changed; 22,284 entries and every
`id` preserved; binary regenerated and verified byte-identical to the JSON; no `.2da` touched.

**The scoping pass reversed the earlier risk assessment.** P4 was deferred out of Wave 1 on the belief
that renaming meant repointing base-game `dialog.tlk` strrefs. Measuring instead of assuming showed
**50 of the 89 item-property types SWLOR actually uses already point at custom TLK entries**, so the
work was pure text editing — no repointing, no new ids, therefore no way for a 2DA reference to go
stale. What looked like the most dangerous package in the wave turned out to be the safest.

Two corrections to earlier assumptions, both worth keeping:

- The displayed property name comes from `itempropdef.2da`'s **`GameStrRef`** column, not `Name`. The
  earlier recon read the wrong column and concluded everything was base-game.
- `ConvertTlk.cmd` runs **binary → JSON**, the opposite of what editing requires, and `BuildHaks.cmd`
  does not touch the TLK at all. Regeneration is `nwn_tlk -i sw_tlk.tlk.json -o sw_tlk.tlk`, verified
  byte-identical round-trip *before* any edit so the starting state was known good.

Scope was combat vocabulary only, per direction: `Defense`→`Armor`, `Evasion`→`Defense Pool`,
`DMG`→`DV`, `HP`→`Physical`, `FP`→`Magic`, `Stamina`/`STM`→`Stun`, `Force Attack`→`Spellcasting`,
`Force Defense`→`Spell Defense`. `Attack` and `Delay` were left for P5 so they move together with the
combat log.

18 candidates were deliberately left alone: seven ability proper nouns (`Crippling Defense`,
`Flowing Defense`, `Overwhelming Defense`, `Absolute Defense`, and the three `Purity` entries) because
renaming produces nonsense like "Crippling Armor" and ability naming belongs to P8; and eleven typed
elemental resistances (Electrical, Mind, Light, Dark, Thermal, Explosive, EM) because Shadowrun has no
elemental armor and "Thermal Armor" would misdescribe them.

**Controller repair at the gate.** The three tooltip sentences came back as mechanical substitutions —
"Increases the amount of **Physical** granted when this item is equipped" — using the new terms as bare
nouns. Prose needed rewriting, not find-and-replace. Reworded to "Increases the size of your Physical
condition monitor…" and the binary regenerated again.

**Newly discovered constraint: `SWLOR_Haks` is a git submodule** (`github.com/zunath/SWLOR_Haks`), so
TLK changes live in a separate repository from the C# work. Every prior package touched only the outer
repo. This affects commit, review and deploy sequencing for P4 and any future hak-side package, and it
had not been recorded anywhere in this plan.

---

## Hak management

**2026-07-22 · haks · strategy defined, guard rails built, detached HEAD rescued · controller**

P4 surfaced that hak work had no defined strategy at all. Investigation found three compounding
problems: the submodule sat on a **detached HEAD** (any commit would be orphaned), pointed at
**upstream** `zunath/SWLOR_Haks` (nowhere to push adaptation work), and `.gitmodules` declared
`branch = master` while HEAD was 54 commits along a feature branch.

Strategy recorded as D7 and documented in `HAKS.md`: fork, `adaptation/shadowrun` branch, rebuild at
wave gates, guard rails over silent automation.

**Done now:**

- Submodule switched to a new `adaptation/shadowrun` branch, rescuing the detached HEAD with P4's
  uncommitted TLK changes intact.
- `tools/orchestration/checkhaks.js` added — seven checks covering branch state, push target, commit
  ordering, TLK id uniqueness, binary/JSON sync, dangling 2DA strrefs, and builder presence.
- `HAKS.md` runbook written; wave-gate checklist added to `ORCHESTRATION.md`.

**2026-07-22 · haks · fork wired up and branch rebased onto the specified start point · controller**

Fork supplied: `github.com/givemedeath/SWLOR_Haks`, starting from `feature/combat-upgrade`.

`origin` repointed at the fork, `upstream` added for `zunath/SWLOR_Haks` so engine and tooling fixes
stay pullable, and `.gitmodules` updated (`url` + `branch = adaptation/shadowrun`) so fresh clones
follow the fork rather than upstream.

**The rebase needed checking rather than assuming.** `adaptation/shadowrun` had been branched from the
local detached HEAD (`7477a74a5d`), which had *diverged* from the specified start point — one commit on
the target side, two on ours, with no ancestor relationship either way. Both sides turned out to be the
same "Remove 260 unused fantasy creature appearances" change, ours as a local commit plus a merge, the
fork's as merged PR #366.

Comparing **tree hashes** rather than commit graphs settled it: both tips resolve to
`96420fcd7730202a914b91acba05e251e6e5ec08` — byte-identical content. So `git reset --mixed` onto
`origin/feature/combat-upgrade` was content-neutral, discarding only the redundant local topology in
favour of the canonical merged history. `--mixed` deliberately, not `--hard`, which would have
destroyed P4's uncommitted TLK work; verified afterwards against a backup taken before any remote
operation.

`checkhaks.js` gained a check this exposed: **a branch with no remote tracking ref**. Local-only work is
one disk failure from being lost, and the outer repo cannot reference it — bumping the pointer to a
commit that exists on no remote hands everyone else an unresolvable checkout. Also added a warning when
the `upstream` remote is missing, since a fork without it silently strands itself.

**2026-07-22 · haks · branch pushed; guard rail caught an immediate real regression · controller**

`adaptation/shadowrun` pushed to the fork and now tracks `origin/adaptation/shadowrun`.

**Within a minute of the push, the submodule silently detached back to the old commit** —
`7477a74a5d` instead of `3d60248916`. The branch was fine and the remote was fine; the working tree
still held P4's TLK edits. What happened is the outer repo still *records* the old commit, so a
`git submodule update` (run by other tooling, not deliberately) snapped the submodule back to the
recorded pointer and left HEAD detached.

This is the failure the plan had documented as a hazard and then walked straight into, which is a fair
argument for the guard rail existing at all: `checkhaks.js` reported `DETACHED HEAD` on its next run
rather than letting the next commit be orphaned. Recovery was a single `git switch`, and the tree
comparison confirmed P4's work intact — the two commits have identical trees, so the working-tree
modifications stayed valid across the revert.

A check was added for the underlying cause: **outer pointer versus submodule HEAD.** It warns whenever
they disagree, which is precisely the window in which an incidental `submodule update` can revert
things. Warning rather than failure, because the two legitimately diverge for as long as hak work is in
flight and before the pointer is bumped.

Remaining gate failure is the pre-existing `racialtypes.2da` dangling strref, deliberately deferred.

**The guard rail found a real bug on its first run.** `racialtypes.2da` references strref `16858047`
(id `80831`), which does not exist in the TLK while its immediate neighbours `80830` and `80832` do.
That renders as `Bad Strref` in game. It **predates this conversion** — P4 only edited `text` on
existing entries and removed nothing. Left unfixed: racial-type naming belongs to a metatype package
that does not exist yet. Recorded in `HAKS.md` so it is not rediscovered as a regression.

**Two of the check's own first-run failures were defects in the check, not the repo**, and both are
worth noting because they are the shape of mistake a guard rail makes:

- The dangling-strref scan flagged 8-digit numbers from unrelated 2DA columns as strrefs. Bounded it
  to the range the TLK can actually address (`16777216 .. 16777216 + maxId`).
- The TLK sync check ran `nwn_tlk` with `stdio: 'ignore'`; that binary fails outright when its stdout
  is closed. Switched to piped capture and set `cwd` to the hak directory.

A third finding was reclassified rather than fixed: `BuildHaks.cmd` drives an 80 MB
`NWN.FinalFantasy.CLI.exe` vendored in the submodule while the maintained builder is `SWLOR.CLI.exe -k`.
Both work today, so this is a **divergence warning, not a failure** — flagging it red would train
people to ignore the gate, which is the failure mode that matters most for a check nothing else backs up.

**2026-07-22 · idle · subagent completed · subagent**

12 edits across 6 files:

- `.claude/settings.json`
- `SWLOR.Game.Server.Tests/Service/ShadowrunDisplayTests.cs`
- `design/shadowrun/ORCHESTRATION.md`
- `design/shadowrun/_hookprobe.tmp`
- `tools/orchestration/selftest.js`
- `tools/orchestration/track.js`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · W1 · subagent completed · subagent**

1 edit across 1 file:

- `SWLOR.Game.Server/Feature/FeedbackMessageConfiguration.cs`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · W1 · subagent completed · subagent**

2 edits across 2 files:

- `SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/PlayerStatusPortraitViewModel.cs`
- `SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/PlayerStatusViewModel.cs`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · W1 · subagent completed · subagent**

8 edits across 1 file:

- `SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · idle · subagent completed · subagent**

2 edits across 2 files:

- `SWLOR.Game.Server/Feature/GuiDefinition/CharacterSheetDefinition.cs`
- `SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · idle · subagent completed · subagent**

15 edits across 7 files:

- `SWLOR.Game.Server.Tests/Feature/CharacterSheetCombatUpgradeTests.cs`
- `SWLOR.Game.Server.Tests/Feature/CharacterSheetStatCoverageTests.cs`
- `SWLOR.Game.Server.Tests/Service/ShadowrunDisplayTests.cs`
- `SWLOR.Game.Server/Feature/GuiDefinition/CharacterSheetDefinition.cs`
- `SWLOR.Game.Server/Feature/GuiDefinition/ViewModel/CharacterSheetViewModel.cs`
- `SWLOR.Game.Server/Service/ShadowrunDisplay.cs`
- `design/shadowrun/DECISIONS.md`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · P4 · subagent completed · subagent**

1 edit across 1 file:

- `C:/Users/benco/AppData/Local/Temp/claude/D--source-repos-SWLOR-NWN/0f850b00-1a65-4d9d-b117-cee43358880b/scratchpad/p4_edit.py`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · idle · subagent completed · subagent**

10 edits across 4 files:

- `design/shadowrun/DECISIONS.md`
- `design/shadowrun/HAKS.md`
- `design/shadowrun/ORCHESTRATION.md`
- `tools/orchestration/checkhaks.js`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

**2026-07-22 · idle · subagent completed · subagent**

5 edits across 2 files:

- `design/shadowrun/HAKS.md`
- `tools/orchestration/checkhaks.js`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

---

## Wave 2

**2026-07-22 · P5 · Combat.cs cluster delivered · controller**

**Gate passed: build clean, 992/992 tests passing** (five new).

Two changes, both in `Combat.cs`, kept in one package because they share the file.

**Subtractive soak.** New `CalculateSoakDamageRange` implements `finalDV = max(0, DV − soak)` for
personal combat, with soak derived from the defender's Defense Pool reduced by the attacker's Attack
Pool acting as armor penetration, plus a named `SoakAtParity = 6`. Recorded as D8.

**A scope hazard surfaced before it could do damage.** `CalculateDamageRange` was shared with
**starship combat** — 10 ship module definitions route through it — and starships are explicitly out
of scope, with no test coverage on their damage balance. Changing the function in place would have
silently rebalanced them.

The caller split turned out to be perfectly clean and made the separation nearly free:
`CalculateDamage` is used *only* by ship modules; `CalculateDamageWithCriticalMitigation` *only* by
personal combat. Personal combat now routes to the soak function; ships keep the proportional curve
their ratings are balanced against. A test asserts the two models stay separate — the weak attack that
fully bounces under soak still lands under the proportional model.

**Pool-based combat log.** `BuildCombatLogMessageNative` and `BuildAbilityCombatLogMessage` now render
`(Pool 12 vs 9)` instead of `(75% chance to hit)`. The ratings were not in scope at the log builders,
only the derived percentage, so they are threaded through from the native attack roll where
`attackerAccuracy` and `defenderEvasion` are already computed. Both parameters default to `-1` meaning
"unknown", in which case the builder falls back to the percentage form — **a wrong pool is
indistinguishable from a real one**, so showing a stale percentage is the safer failure. The
ship-facing `BuildCombatLogMessage` is untouched.

New tests assert the properties the proportional model could not express at any tuning: a low-DV
attack fully bounces off heavy armor, a high-DV attack punches through the same armor, mitigation is
flat rather than proportional, and higher Attack penetrates more.

**Still open in P5's scope:** the `Attack` and `Delay` renames that P4 deferred here. The vocabulary
for them was never settled — `Delay` plausibly becomes Initiative once the pass model is decided, and
`Attack` overlaps with the damage-bonus stat P2 deliberately left alone. Both want a decision before
the rename, not during it.

**2026-07-22 · P5 · live test found the soak calibration was unplayable; fixed · controller**

**First real in-game test of any of this work, and it failed immediately.** A new character
(Agility 18, Perception 11) and the weakest enemy in the module could not damage each other at all,
in either direction.

Root cause was mine. `SoakAtParity = 6` was a **flat** constant calibrated against median weapon DV
(30) and p95 (111) — mid-game gear. The real Ashwing Echo carries `Attack 3` and weapon `DV 7` at NPC
level 2; a new character's ratings produce pools of ~2 on both sides. So
`soak = max(0, 2 − 2 + 6) = 6`, and `7 − 6 = 1`, rounding to nothing. Starting gear sits entirely
below a flat soak of 6.

Fixed by making the parity component proportional to the defender's pool rather than fixed:
`soak = max(0, DefensePool − AttackPool) + DefensePool × SoakParityPercent / 100`. Verified across the
real progression curve — low-level fights deal damage again, mid and cap parity mitigate modestly, and
heavy armor still stops a DV-8 attack outright while barely slowing a DV-120 one. 994/994 passing.

**The lesson is about the tests, not the constant.** The suite asserted "low DV bounces off *heavy*
armor" — correct, and it passed throughout. What it never asserted was that **an evenly-matched
low-level fight still deals damage**, which is the very first thing any player experiences. Two
regression tests now cover it, one using the literal Ashwing Echo values and one walking the low, mid
and cap tiers.

This is precisely the gap flagged when Wave 1 closed: nothing in `dotnet build` or `dotnet test` sees
what a player sees. A green suite of 992 tests coexisted with combat being entirely non-functional.

**2026-07-22 · idle · subagent completed · subagent**

9 edits across 5 files:

- `.gitignore`
- `SWLOR.Game.Server.Tests/Service/CombatDamageTests.cs`
- `SWLOR.Game.Server/Service/Combat.cs`
- `design/shadowrun/DECISIONS.md`
- `design/shadowrun/HAKS.md`

*Auto-captured. Replace with a summary of what changed before closing the wave.*

---

**2026-07-22 · platform assessment + combat feel prototype · Lead**

Answered the standing question of whether SWLOR/NWN:EE is the right base at all, recorded in
[PLATFORM-ASSESSMENT.md](PLATFORM-ASSESSMENT.md). Verdict: the engine is very likely correct — it is
one of the only platforms satisfying live GM orchestration *and* persistent solo content — but the
plan understated the project by about an order of magnitude. Measured from the repo: 539,315 LOC of
genre-agnostic systems worth keeping, against 443 areas, 938 creatures, 7,526 items and 609 dialogs
that are entirely Star Wars and do not convert. **This is a new game on SWLOR's engine, not a
conversion of SWLOR.** Kill criteria set: if six months pass without one convincing sprawl street,
the answer was Evennia.

Then built `CombatFeelHarnessTests` — a simulator over real module stat curves that reports hit rate,
damage per exchange, and time-to-kill, and renders the pools a player would see. Built first,
deliberately, because the parity-soak bug shipped precisely from tuning combat without an instrument.

It found two things immediately, one of which nobody had flagged:

1. **The dice pools were decorative.** Every evenly matched fight resolved at 74–76% regardless of
   the ratings — a two-point spread across pools from 1 to 14. The display moved; the outcome did
   not.
2. **Fights ran 2–4x too long.** 14 exchanges low-tier, 42 at prime, against a Shadowrun target of
   3–12. The high tier was worse than the mid tier, because hit points outgrow damage.

A sweep proved no single knob reaches the target — lowering the hit rate to make pools meaningful
*lengthens* fights — so base rate, slope, and health curve were searched together. Applied as
[D9](DECISIONS.md) and [D10](DECISIONS.md): base 50, 8 points per displayed pool, NPC health ÷6.
Every evenly matched tier now lands between 3.4 and 10.4 exchanges. Starship combat keeps the old
curve through a separate `CalculateShipHitRate`, matching the split already made for damage.

1001/1001 tests pass. The harness stays as a permanent guard: a spot assertion on a formula cannot
catch a fight that is individually correct and collectively wrong, which is the failure mode that has
now bitten this project twice.

Caveat carried forward: the harness models the auto-attack loop only, so its exchange counts are an
upper bound. Abilities will make real fights shorter. The pool-flatness finding is unaffected by
that, since abilities resolve through the same hit rate.

---

**2026-07-22 · P6 wound modifier · Lead**

Live test of the pacing retune came back positive on all three checks — combat log reads honestly,
fights resolve in roughly the predicted number of hits, armor behaves as a threshold. First live
validation this project has had, and the harness numbers matched what a player actually experienced.

Shipped the wound modifier as [D11](DECISIONS.md). Two things worth carrying forward:

The plan specified deriving the penalty from HP *and* Stamina, matching the tabletop, and that turned
out to be wrong for this codebase — SWLOR's stamina is an ability cost pool, so a stun-track penalty
would have charged players for using abilities rather than for being injured. Physical track only.

The death-spiral risk was measured rather than argued about. The harness gained a duel simulation
that fights to exhaustion with ratings recomputed as health drops, because wound penalties are
invisible in a single exchange and compound only across a whole fight. At three free boxes the winner
finishes 5–7 points healthier than without penalties and fights run marginally shorter: a tilt, not a
rout. That figure is now a permanent assertion with a 15-point ceiling.

1005/1005 tests pass.

---

**2026-07-22 · P9 deferral recorded · Lead**

Flagged FP and Stamina for rework rather than further translation, as **Deferred — `P9`** in
[PLAN.md](PLAN.md).

The trigger is that two packages have now conceded the same point from opposite directions.
[D6](DECISIONS.md) found FP could not be Edge and settled for an honest bar. [D11](DECISIONS.md) had
to exclude the stun track from wound penalties because stamina is an ability cost pool. Both are
symptoms of one root cause: **SWLOR's resource model is MMO-shaped and Shadowrun's is damage-shaped.**
Shadowrun has no mana bar — Magic is an attribute rating, and casting costs Drain, which lands on the
stun monitor as damage. The wound penalties from that damage are the real limiter.

The three problems share one solution, which is why they should move together rather than piecemeal:
a stun monitor filled by damage rather than spent on abilities gives Drain somewhere to land, makes
stun wound penalties correct, and deletes the FP bar. `Stat.CalculateWoundPenalty` already takes the
right shape — it reads a condition monitor and does not care what filled it.

D6 and D11 annotated as provisional, and both `ShadowrunDisplay.GetMagicPool` and
`GetStunConditionBoxes` carry the same warning in their XML docs so the next person to touch them
sees it before investing.

Also fixed a real defect in `syncplan.js` found while recording this: it documented the repo copy as
canonical but offered only `--to-repo`, which overwrites the repo from the session snapshot. Running
the one available reconcile command would have silently destroyed the plan change it was reporting.
Added `--to-session` for the direction deliberate plan edits actually take.

---

**2026-07-22 · art risk re-assessed · Lead**

Inventoried `SWLOR_Haks` while deciding the next feasibility step, and found the original art
assessment was wrong on the facts. It was written from general knowledge of NWN's asset library rather
than from the haks actually in this repository.

70 tilesets ship here. Among them **D20 Shadowrun Exterior** (315 tiles, with `arcology_torii`
textures) and **D20 VirtuNet** (72 tiles, interior) — a purpose-built Shadowrun set and a ready-made
Matrix visual language, both already packed into the module. Plus D20 Modern Exterior (508 tiles) and
D20 Futuristic City (267).

38 areas already use them, so they are proven in-engine rather than merely downloadable, and several
read as Sixth World in all but name: *Smuggler's Moon - The Slums*, *[Prefab] City, Industrial Slum*,
*[Prefab] Cityscape, Vertical*, *Smuggler's Moon - Shipping District*.

[PLATFORM-ASSESSMENT.md](PLATFORM-ASSESSMENT.md) revised accordingly, with the original claim struck
through rather than deleted so the correction stays visible. The risk moves from "can a sprawl tileset
exist" to "are these assets good enough" — a judgement requiring eyes, not a multi-year build. The
kill-criteria deadline was pulled in from six months to this week on the same grounds: a leash sized
for an unanswerable question should not outlive the question.

This is the second time an assumption in the assessment survived only until it was measured. The first
was combat feel. Worth treating the remaining unmeasured claims in that document as similarly
provisional.

---

**2026-07-22 · claim audit + plan v2 · Lead**

Cross-checked every remaining claim in [PLATFORM-ASSESSMENT.md](PLATFORM-ASSESSMENT.md). Results were
mixed enough to justify the exercise:

- **"Metatypes are free"** — half wrong. The models exist, but SWLOR sets `PlayerRace=0` on every
  fantasy race; only Human plus 18 custom species are selectable. Re-enabling is a 2DA edit. No troll
  model exists; Ogre/Giant/Minotaur are the candidate bases.
- **"No cyberware, no Essence"** — held. A broader search found only NWN's `BiowareXP2` library and an
  unrelated vibroblade ability.
- **"The Matrix is a multi-year subsystem"** — overstated. `Space.cs`, the entire parallel-game
  precedent, is 2,214 LOC plus 41 definition files.
- **"Custom species are a big lift"** — wrong. 269 custom appearance rows already exist at 10000+.
- **"$20 client toll"** — held, $19.99.
- **"Low hundreds concurrent"** — mildly pessimistic. ~464–985 concurrent on Steam, 24h peak 1,080,
  with over half reportedly in persistent worlds, and GOG uncounted.
- **"GM tooling best-in-class"** — true but imprecisely attributed. `DMToolsViewModel` is a
  placeable/layout editor; the live-GM capability is NWN's native DM Client, which is Bioware's work
  rather than SWLOR's.
- **IP** — corrected. Topps owns Shadowrun and licenses tabletop to Catalyst, but **Microsoft holds
  the video game rights**, a more direct exposure than v1 stated.

Four project-shaping questions settled as [D12](DECISIONS.md): hybrid content, Shadowrun-flavored
systems, original city in the canonical world, solo pace with agent assistance. P8 cut outright as
[D13](DECISIONS.md).

v1 archived to `PLAN-ARCHIVE-presentation-layer.md` — it remains the accurate record of Waves 0–2 —
and replaced with a four-phase build plan whose every phase ends in a measurement or a play session
rather than a declaration. That structure is a direct response to the audit: three separate
assumptions in v1 survived only until someone measured them.

---

**2026-07-22 · content tiers added to plan v2 · Lead**

Phase 3 restructured around a distinction the plan had collapsed: **GM events and authored Runs are
different tiers serving different nights.** GMs give the world its living feel; authored Runs are what
survives the evenings no GM is online. Recorded as [D14](DECISIONS.md), with a third procedural tier
(the contract board) for repeatable grind.

Checked the quest infrastructure rather than assuming it. It is in better shape than expected — 259
quests across 33 definition files, and a dialog **snippet system** that lets any conversation node gate
on or advance quest state, which means legwork stages are authored in dialog rather than code. Rewards
already cover nuyen (`AddGoldReward`), heat and street cred (`AddFactionStandingReward`), and
repeatability (`IsRepeatable`).

The real gap is narrower than it first looked: only two objective types are declarative, kill and
collect. Every run built on those alone is "kill N" or "fetch N" — the exact MMO texture authored Runs
are meant to escape. `Quest.AdvanceQuest` is callable from any script so reach-location and use-object
stages work today, but hand-scripted each time. Promoted to its own package (`3b`) because it gates
whether the Runs feel varied.

One structural consequence: Phase 2 and Phase 3 can no longer be fully sequential. Each Johnson is the
delivery point for a specific Run, so the district map and the run roster are one design problem.
Noted in `2d`.

---

**2026-07-22 · P1a metatypes shipped · Lead**

Added the five Shadowrun metatypes as playable species — the first Phase 1 (Identity) package.
Recorded as [D15](DECISIONS.md).

The research paid off twice over. What looked like the main risk — attribute deltas fighting the AP
economy — resolved cleanly once the code was traced: SWLOR already applies ability modifiers as
engine effects (`EffectAbilityIncrease`), which layer on top of the base score, so the rebuild's
`<= 10` validation never sees them and native combat does. Metatype attributes ride that exact
mechanism. Traits (troll dermal armor, dwarf toxin resistance) ride the stat-adjustment layer that
perks already use, so the soak and defense systems read them with no special-casing.

Two failures surfaced and both were informative rather than incidental:
- The `Metatype.GetStatBonus` addition to the shared stat read threw in the unit harness because it
  called NWScript. Fixed by copying `Mimicry`'s guard: short-circuit before any engine call for stats
  no metatype touches. Correct behaviour and test-safe in one move.
- A test had *documented* the dangling biography strref 80831 as an intentional blank. But a missing
  entry renders as "Bad Strref", not blank — so the documented intent was served by adding a real
  empty entry. Fixed the strref and updated the test to assert the resolved state. `checkhaks` now
  passes the dangling-strref check that had failed since the project began.

1013/1013 tests pass. Haks rebuilt and module repacked so the race wheel and TLK names are live.

Still to verify in live play (the gate): roll a troll street samurai and a dwarf mage, confirm all
five appear on the race wheel with correct names, the troll is visibly larger and wears armor, and the
sheet reads coherently. Two specific wrinkles to watch: whether the login-applied attribute effect
feeds HP/FP/STM (derived at init from the base score), and troll skin tone.

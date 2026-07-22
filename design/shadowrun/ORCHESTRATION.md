# Shadowrun Presentation Layer — Orchestration Protocol

This is the operating protocol for the Shadowrun presentation-layer conversion. It governs how work is
partitioned, who executes each package, and how every task, decision, and material change is recorded.

Companion artifacts in this directory:

| File | Purpose |
|---|---|
| `PLAN.md` | **The plan of record.** Canonical, committed. See "Plan of record" below. |
| `HAKS.md` | Hak and TLK runbook — the submodule, TLK regeneration, rebuild cadence, commit ordering. |
| `LEDGER.md` | Append-only. Every dispatch, completion, and material change, timestamped. |
| `DECISIONS.md` | ADR-style. Every decision with rationale. |
| `packages/P*.md` | One brief per work package — the artifact handed to a subagent. |
| `events.jsonl` | Raw hook-captured tool events. Reconciled into `LEDGER.md` at each wave gate. *(gitignored)* |
| `.current-package` | Single-line marker read by hooks to attribute edits to a package. *(gitignored)* |

---

## Plan of record

**`design/shadowrun/PLAN.md` is canonical.** It is committed, ships with the repo, and is what anyone
picking up this work should read first.

Claude Code also maintains a working copy at `~/.claude/plans/investigate-in-detail-a-merry-moler.md`.
That copy is a **session artifact**: machine-local, outside the repo, and gone when the session ends.
It is never authoritative. The risk it creates is specific — a scope change made during a session
lands only in the copy that disappears.

Three mechanisms guard against that:

1. **On-demand check.** `node tools/orchestration/syncplan.js` reports drift at section granularity
   (which `## ` headings were added, removed, or changed) and exits non-zero when the two differ.
   `--to-repo` reconciles session → repo, preserving the repo copy's provenance header.
2. **Automatic warning.** The `Stop` hook calls the same comparison and emits a `systemMessage` when
   the copies have diverged, naming the drifted sections and the command to fix it. A missing session
   copy is *not* drift — outside the approving session there is nothing to compare, and the guard
   stays silent rather than crying wolf.
3. **Edit flagging.** Any edit to either plan copy is recorded with `plan: true` in `events.jsonl`, so
   a wave-gate review can see that scope moved without diffing the plan.

**Material changes go in the repo copy**, then get a `LEDGER.md` entry and, where a judgement call was
involved, a `DECISIONS.md` entry. A plan change with no ledger entry is the failure mode this exists
to prevent: the *what* survives while the *why* is lost.

Material means scope, wave structure, package boundaries, owned-file sets, tiering, or verification
gates. Typo fixes and rewording do not need a ledger entry, but should still be synced so the two
copies never accumulate silent divergence.

---

## Roles and tiering

Tier is a **ceiling, not a floor**. Dispatch self-contained packages to cheaper models; keep
judgement-heavy work with the controller; keep changes smaller than their own brief inline.

| Tier | Executor | Criteria |
|---|---|---|
| **Lead** | Controller (Opus), inline | Semantic core, balance tuning, anything empirical or cross-cutting. All shared-file packages. |
| **Mid** | Sonnet subagent | Self-contained, single file cluster, spec writable in advance |
| **Low** | Sonnet subagent, fan-out | High-volume mechanical work, verifiable by test or script |
| **Inline** | Controller | Changes where writing the brief would cost more than making the edit |

Rationale: the top-tier model is the expensive resource. A package whose specification can be written
completely in advance does not need it. A package that requires reading the room — retuning damage
curves, judging whether a wound penalty feels punishing — does.

---

## The file-ownership rule

**Packages that share a file are never dispatched concurrently.** They are merged into one package
under one owner, or serialized across waves.

Two files in this project are each wanted by two packages, and this is what would make naive parallel
dispatch corrupt the tree:

| File | Wanted by | Resolution |
|---|---|---|
| `Service/Combat.cs` | pool-based log + subtractive soak | Merged into `P5`, one owner |
| `Service/Stat.cs`, `StatService/StatType.cs` | wound modifier + accuracy reads | `P6` serialized after `P5` |

**Before opening any wave the controller verifies that the owned-file sets of all packages in that
wave are pairwise disjoint.** This check is not optional. Every package brief declares its owned file
set explicitly, and subagents are instructed not to edit outside it.

For higher-risk parallel work, `Agent(isolation: "worktree")` gives a subagent its own git worktree.
The repo already uses this (`.claude/worktrees/`). Prefer it whenever a package must touch generated
output or run a build.

---

## Wave structure

```
Wave 0  ── P0 ShadowrunDisplay ──────────────────────────── Lead, solo
                    │                (everything downstream consumes this contract)
Wave 1  ── P1 Feedback · P2 CharSheet · P3 HUD · P4 TLK/2DA ── 4 × Sonnet, parallel
                    │
Wave 2  ── P5 Combat.cs cluster ──▶ P6 Wound modifier ─────── Lead, serialized
                    │
Wave 3  ── P7 Glitches · P8 Perk descriptions ─────────────── Sonnet, parallel + fan-out
```

A wave opens only when every package in the previous wave is `completed` in the task list and the
full test suite is green.

### Wave gate checklist

```bash
dotnet test --no-build                        # full suite, not a filter
node tools/orchestration/checkhaks.js         # hak/TLK guard rails
node tools/orchestration/syncplan.js          # plan of record in sync
git status --short --untracked-files=all      # ground truth for what changed
```

**`dotnet test` does not cover the haks.** Nothing in the test suite reads a 2DA or a TLK, so
asset-side breakage — a stale binary TLK, a dangling strref, a detached submodule — passes a green
build untouched. `checkhaks.js` is the only thing that catches it. See `HAKS.md`.

If the wave touched `SWLOR_Haks`, also rebuild the haks and repack the module before opening the next
wave, and commit the submodule **before** bumping its pointer in the outer repo.

---

## Tracking

### Live coordination — the built-in task list

One task per package. `TaskUpdate` carries:

- `owner` — who is executing it
- `addBlockedBy` — encodes the wave gates, so blocked work cannot be claimed early
- `metadata` — `tier`, `wave`, `model`, `ownedFiles`, `fanOut`

**The controller owns all task-list state. Subagents never touch it.** This was established
empirically: the task tools are not available inside subagent sessions, and the first dispatch that
instructed a subagent to claim and resolve its own task could not do so. The subagent reported the gap
rather than inventing a workaround, which is the behaviour the briefs should keep encouraging.

The resulting protocol is also the better design, because it puts verification with the party that can
actually verify:

1. Controller sets `owner` and `status: in_progress` **before** dispatch.
2. Controller writes `.current-package` so hook attribution is correct.
3. Subagent does the work and **reports outcome in its final message**.
4. Controller checks the verification gate itself, then sets `status: completed`.

**A package is only marked `completed` when its gate passes** — not when the edits land, and never on
a subagent's self-report alone. Partial work, failing tests, or unresolved errors keep it
`in_progress`.

### Durable record — this directory, git-committed

The task list is session-scoped and is not guaranteed to survive a restart. `LEDGER.md` and
`DECISIONS.md` are the authoritative record and must be committed alongside the code they describe.

**`DECISIONS.md` is load-bearing, not ceremony.** The display divisor `K`, the wound-modifier free
threshold, and the soak curve are **empirical constants with no derivation from the tabletop rules** —
they exist purely to reconcile Shadowrun vocabulary with SWLOR's existing math. Nobody will
reconstruct why they hold their values six months from now unless the reasoning is written at the
moment it is decided. Record the decision when you make it, not at the end of the wave.

### Automatic capture — hooks

Configured in project-level `.claude/settings.json`:

| Hook | Action |
|---|---|
| `PostToolUse` on `Edit`\|`Write` | Append `{timestamp, file, package}` to `events.jsonl` |
| `SubagentStop` | Append package completion + owned-file diff summary to `LEDGER.md` |
| `Stop` | Flush session summary; flag any package left `in_progress` |

Hooks cannot infer which package an edit belongs to. **The controller writes `.current-package`
before each dispatch** and the hook reads it. Raw events land in `events.jsonl`; the controller
reconciles them into `LEDGER.md` prose at each wave gate.

### `events.jsonl` is a hint. Git is ground truth.

Two failure modes are known and neither is fully solved, so **never reconcile a wave gate from
`events.jsonl` alone — always cross-check `git status --short --untracked-files=all`.**

**Dropped events.** During P0 the hook silently missed two of four edits in a rapid burst
(`ShadowrunDisplay.cs` and a `DECISIONS.md` edit), while capturing the writes immediately before and
after. A follow-up probe captured normally and the failure did not reproduce, so there is no confirmed
root cause — suspected hook-dispatch coalescing under rapid successive tool calls. The practical
consequence: the log under-reports, so an empty or short event list is not evidence that nothing
changed.

**Stale package markers.** `.current-package` persists until overwritten. A marker set for a dispatch
that is then cancelled will mis-attribute whatever the controller does next — this happened during P0
setup, tagging orchestration work as `P0`. **Reset the marker when a dispatch ends or is abandoned,**
not only when one begins.

Neither undermines the durable record, because `LEDGER.md` and `DECISIONS.md` are written
deliberately rather than derived from the event log. That separation is the reason the design survives
a lossy hook.

The hook logic lives in `tools/orchestration/track.js` (Node, because `jq` is not installed in this
environment); plan-sync comparison lives in `tools/orchestration/syncplan.js`, which doubles as a CLI
and a module so the logic exists in exactly one place. Verify both with:

```bash
node tools/orchestration/selftest.js
```

Run that after changing `track.js`, or whenever tracking looks stale. **A tracking hook that silently
does nothing is indistinguishable from one that is working**, so the self-test asserts on real side
effects rather than exit codes, and sandboxes itself via `CLAUDE_PROJECT_DIR` so the real ledger is
never touched.

One trap it exists to prevent: hand-written test payloads are unreliable here. Shell heredocs and
`echo` collapse `\\` to `\` in Windows paths, producing invalid JSON escapes — a healthy hook then
looks broken because `JSON.parse` correctly rejects the fixture. Build payloads with `JSON.stringify`,
never by hand.

---

## Dispatch brief template

Package briefs live in `packages/` and are authored **at wave-open time**, not up front — each wave's
briefs incorporate what the previous wave learned. Every brief uses this shape:

```markdown
# P<n> — <title>

**Tier:** Low | Mid | Lead      **Wave:** <n>      **Model:** sonnet | inline
**Task ID:** <task list id — for the controller's reference; the subagent cannot touch it>

## Owned files
Explicit list. Editing outside this set is out of scope — report it, do not fix it.

## Context
What the package changes and why, with file:line anchors. Enough to act without the
originating conversation.

## Requirements
Numbered, concrete, individually checkable.

## Project rules that apply
Verbatim excerpts of the AGENTS.md rules this package can violate. Do not paraphrase —
the prohibitions are the point.

## Verification gate
The exact commands to run and what must be true. The subagent runs these and reports
the result; the controller re-checks before marking the task completed.
```

**Do not instruct a subagent to claim or resolve its task** — the task tools are unavailable to them
and the instruction only produces a confusing failure. End every brief with "report what you did, and
anything you noticed but did not fix, in your final message" instead.

### Rules that must be quoted verbatim into briefs

Two packages can damage things outside their own scope, and their briefs must carry the relevant
prohibition word for word:

- **`P4` (TLK/2DA)** — reuse pre-existing empty TLK slots before appending; 2DA references use
  `16777216 + tlkId`, never the raw id; update every reference when moving an entry; regenerate
  `sw_tlk.tlk` before handoff.
- **`P8` (perk descriptions)** — **never** edit a Design Bible workbook with `openpyxl` or any library
  that rewrites the whole workbook. It silently discards cached formula-result values; the perk sync
  tests still pass while formula-backed tabs break tests like `NPCEnemyBalanceAuditTests`. Edit
  surgically at the zip/XML level and repackage copying every other entry byte-for-byte.

---

## Standing build rule

Building or testing `SWLOR.Game.Server` fires a Windows post-build deploy (`SWLOR.CLI.exe -o`) that is
slow and unnecessary for verification. **Always pass `-p:RunPostBuildEvent=Never`.** Build once, then
test many with `--no-build` and a `--filter`:

```bash
dotnet build SWLOR.Game.Server.Tests/SWLOR.Game.Server.Tests.csproj -p:RunPostBuildEvent=Never
dotnet test --no-build --filter "FullyQualifiedName~<RelevantTestClass>"
```

Run the full unfiltered suite at wave gates and before handoff — not after every edit.

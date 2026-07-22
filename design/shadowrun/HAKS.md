# Hak and TLK Management

How asset-side changes are made, verified, and shipped for the Shadowrun conversion.

`SWLOR_Haks` is a **git submodule** — a separate repository from the C# work. That single fact drives
everything here: hak changes are invisible to `dotnet build` and `dotnet test`, they commit and push
separately, and they break in ways the .NET toolchain cannot detect. Nothing in the test suite reads a
2DA or a TLK.

Run the guard rails at every wave gate and after any package that touches `SWLOR_Haks`:

```bash
node tools/orchestration/checkhaks.js
```

---

## Layout

| Path | What it is |
|---|---|
| `SWLOR_Haks/` | Submodule. 113 hak source directories, ~13 GB |
| `SWLOR_Haks/sw_tlk/sw_tlk.tlk.json` | Custom TLK, **source of truth**. 22,284 entries |
| `SWLOR_Haks/sw_tlk/sw_tlk.tlk` | Binary TLK, **generated** from the JSON. Never hand-edit |
| `SWLOR_Haks/sw_2da/` | 747 2DA tables |
| `SWLOR_Haks/hakbuilder.json` | Build manifest — the 113 haks and their source dirs |
| `SWLOR_Haks/output/` | Built `.hak` files. **Gitignored** — artifacts, not sources |
| `Module/ifo/module.ifo.json` | References all 113 haks by name plus `sw_tlk` |

Only *sources* are versioned. Built haks are reproducible artifacts and are never committed.

---

## Repository strategy

Hak work for this adaptation lives on an **`adaptation/shadowrun` branch in a fork** of
`SWLOR_Haks`, not on upstream and not on a detached HEAD.

Forking rather than branching upstream is deliberate: a total conversion diverges permanently —
tilesets, portraits, item art, TLK — and that divergence does not belong in the parent project's
repository. A fork also keeps upstream pullable, so genuine engine or tooling fixes can still be
merged in.

**Current configuration:**

| Setting | Value |
|---|---|
| Fork (`origin`) | `https://github.com/givemedeath/SWLOR_Haks` |
| Working branch | `adaptation/shadowrun` |
| Branched from | `origin/feature/combat-upgrade` (`3d60248916`) |
| `upstream` remote | `https://github.com/zunath/SWLOR_Haks` — kept so engine and tooling fixes stay pullable |
| `.gitmodules` | points at the fork, `branch = adaptation/shadowrun` |

**Remaining step:** the branch exists locally but has not been pushed.

```bash
git -C SWLOR_Haks push -u origin adaptation/shadowrun
```

`checkhaks.js` reports `adaptation branch is pushed` as a failure until then — a local-only branch is
one disk failure from losing the work, and the outer repo cannot reference it, because bumping the
pointer to a commit that exists on no remote gives everyone else an unresolvable checkout.

**Detached HEAD is the trap to watch for.** The submodule arrives detached by default, and a commit
made there is orphaned the moment anything else is checked out. The guard rail checks for this on
every run.

It re-detaches more easily than you would expect. The outer repo records which submodule commit it
expects, and **any `git submodule update` — including one run incidentally by other tooling — snaps
the submodule back to that recorded commit and leaves HEAD detached.** This happened immediately
after the first push of `adaptation/shadowrun`: the branch and its remote were both fine, but the
submodule silently reverted to the old commit because the outer pointer had not been bumped yet.

Recovery is just `git -C SWLOR_Haks switch adaptation/shadowrun`; uncommitted work in the working tree
survives. `checkhaks.js` warns whenever the pointer and HEAD disagree, which is the window in which
this can happen.

**Pulling upstream changes later:**

```bash
git -C SWLOR_Haks fetch upstream
git -C SWLOR_Haks merge upstream/master     # or the relevant upstream branch
```

---

## Commit and push ordering

Two repositories means order matters, and getting it wrong produces a pointer nobody can resolve:

1. Commit **inside** `SWLOR_Haks` first.
2. **Push** the submodule branch.
3. Only then commit the bumped submodule pointer in the outer repo.

Reversing steps 2 and 3 leaves the outer repo referencing a commit that exists only on your machine.
Everyone else gets a broken checkout, and the failure surfaces far from its cause.

The outer repo shows submodule movement as a bare ` m SWLOR_Haks` in `git status` — easy to miss, and
easy to commit accidentally. Check both repos separately.

---

## Editing the TLK

The JSON is the source of truth; the binary is generated.

```bash
# after editing sw_tlk.tlk.json
cd SWLOR_Haks && ./nwn_tlk.exe -i ./sw_tlk/sw_tlk.tlk.json -o ./sw_tlk/sw_tlk.tlk
```

**Do not use `ConvertTlk.cmd` for this.** It runs the opposite direction — binary to JSON — and will
silently overwrite your edits with the stale binary's contents. It exists for extracting, not building.

Rules that matter, from `AGENTS.md`:

- Reuse a pre-existing empty slot or gap before appending new IDs.
- 2DA references use `16777216 + tlkId`. Raw ids are only valid for base-game `dialog.tlk`.
- When moving or adding an entry, update **every** 2DA reference to match.

Editing only the `text` field of existing entries — as P4 did — satisfies all three trivially: no new
ids, no moves, so no reference can go stale. Prefer that shape whenever possible.

### Dangling strrefs

The most dangerous TLK failure is a 2DA pointing at an id that does not exist. NWN renders it as
`Bad Strref` at runtime; nothing in the build or test suite catches it. `checkhaks.js` scans all 747
2DA files against the TLK's actual id set specifically to find these.

**There is a known pre-existing one:** `racialtypes.2da` references strref `16858047` (id `80831`),
which is absent while its neighbours `80830` and `80832` exist. It predates this conversion. Not fixed
here because racial-type naming belongs to a metatype package that does not exist yet.

---

## Rebuild cadence

Rebuild at **wave gates**, not on every hak-touching package. 113 haks over ~13 GB is too slow for
per-package rebuilds, and too risky to leave until deploy.

```bash
cd SWLOR_Haks && BuildHaks.cmd     # all 113 haks
cd Module && PackModule.cmd        # repack the module
```

Only rebuild the haks whose sources actually changed where the builder supports it. A TLK-only change
still needs the module repacked, because the module carries the custom TLK reference.

Two builders exist, which is a divergence risk rather than a present bug: `BuildHaks.cmd` drives an
80 MB `NWN.FinalFantasy.CLI.exe` vendored into the submodule (a leftover from the project SWLOR forked
from), while the maintained builder is `SWLOR.CLI.exe -k` from `HakBuilder.cs`. Both work today. If
`HakBuilder.cs` changes, the vendored copy will not pick it up. `checkhaks.js` reports this as a
warning, not a failure.

---

## What the guard rails check

| Check | Catches |
|---|---|
| Submodule on a branch | Detached HEAD — commits that would be orphaned |
| Submodule points at a fork | Adaptation work with no push target |
| Commit ordering | Reminder when the submodule is dirty, before the pointer is bumped |
| TLK ids unique | Duplicate entries |
| TLK binary matches JSON | Stale binary — edits made but never regenerated |
| No dangling 2DA strrefs | References to missing TLK entries (`Bad Strref` in game) |
| Hak builder present | Missing or diverged builder |

Failures block a wave gate. Warnings are advisory and do not.

---

## Package checklist

For any package that touches `SWLOR_Haks`:

1. Confirm the submodule is on `adaptation/shadowrun` before starting.
2. Edit sources only — never `output/`, never the binary TLK.
3. Regenerate the TLK if the JSON changed.
4. Run `node tools/orchestration/checkhaks.js` and clear every failure.
5. Rebuild haks and repack the module at the wave gate.
6. Commit and push the submodule **before** bumping the outer pointer.
7. Record the change in `LEDGER.md`; note that it is hak-side, since it ships separately.

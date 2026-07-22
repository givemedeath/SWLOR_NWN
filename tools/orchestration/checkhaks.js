#!/usr/bin/env node
/**
 * Hak and TLK guard rails for the Shadowrun conversion.
 *
 *   node tools/orchestration/checkhaks.js
 *
 * Run at every wave gate, and after any package that touches SWLOR_Haks.
 *
 * SWLOR_Haks is a git submodule, which means hak work lives in a different
 * repository from the C# work and fails in ways the .NET build and test suite
 * cannot see. Nothing in `dotnet build` or `dotnet test` reads a 2DA or a TLK, so
 * a dangling strref or a stale binary ships silently. These checks exist to make
 * that class of breakage loud.
 *
 * Exit 0 = all checks passed. Exit 1 = at least one failure. Exit 2 = could not run.
 */

const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');

const REPO_ROOT = process.env.CLAUDE_PROJECT_DIR || path.resolve(__dirname, '..', '..');
const HAKS = path.join(REPO_ROOT, 'SWLOR_Haks');
const TLK_JSON = path.join(HAKS, 'sw_tlk', 'sw_tlk.tlk.json');
const TLK_BIN = path.join(HAKS, 'sw_tlk', 'sw_tlk.tlk');
const TWO_DA = path.join(HAKS, 'sw_2da');
const NWN_TLK = path.join(HAKS, 'nwn_tlk.exe');

/** The branch hak work is expected to live on for this adaptation. */
const EXPECTED_BRANCH = 'adaptation/shadowrun';

/** NWN custom-TLK strrefs start here; anything lower is base-game dialog.tlk. */
const CUSTOM_STRREF_BASE = 16777216;

const results = [];
const record = (ok, name, detail) => results.push({ ok, name, detail });

/** Non-blocking advisory. Reported, but does not fail the gate. */
const warn = (name, detail) => results.push({ ok: true, warn: true, name, detail });

function git(args, cwd = HAKS) {
  try {
    // stderr is piped rather than inherited: several of these probes are expected to
    // fail (an unpushed branch has no upstream), and git's own error text leaking
    // into the report reads like a crash.
    return execFileSync('git', args, { cwd, encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] }).trim();
  } catch {
    return null;
  }
}

// ---------------------------------------------------------------- A. submodule

function checkSubmoduleBranch() {
  const branch = git(['branch', '--show-current']);
  if (branch === null) return record(false, 'submodule reachable', 'could not run git in SWLOR_Haks');

  if (!branch) {
    return record(
      false,
      'submodule is on a branch',
      'DETACHED HEAD — any commit made here will be orphaned. Fix: ' +
        `git -C SWLOR_Haks switch -c ${EXPECTED_BRANCH}`
    );
  }
  if (branch !== EXPECTED_BRANCH) {
    return record(false, 'submodule on the adaptation branch', `on "${branch}", expected "${EXPECTED_BRANCH}"`);
  }
  record(true, 'submodule on the adaptation branch', branch);
}

/**
 * The outer repo records which submodule commit it expects. When the submodule's
 * HEAD has moved past that, any `git submodule update` — including ones run
 * incidentally by other tooling — snaps the submodule back to the recorded commit
 * and leaves it on a DETACHED HEAD, silently undoing a branch switch.
 *
 * This is not hypothetical: it happened immediately after the first push of
 * adaptation/shadowrun, reverting the submodule to the old commit while the branch
 * and its pushed remote were both fine.
 */
function checkPointerMatchesHead() {
  const recorded = git(['ls-tree', 'HEAD', 'SWLOR_Haks'], REPO_ROOT);
  if (!recorded) return;

  const match = recorded.match(/\b([0-9a-f]{40})\b/);
  if (!match) return;

  const pointer = match[1];
  const head = git(['rev-parse', 'HEAD']);
  if (!head) return;

  if (pointer === head) {
    return record(true, 'outer pointer matches submodule HEAD', pointer.slice(0, 10));
  }

  warn(
    'outer pointer matches submodule HEAD',
    `outer repo records ${pointer.slice(0, 10)} but the submodule is at ${head.slice(0, 10)}. ` +
      'Expected while hak work is in flight, but until the outer pointer is bumped, any ' +
      '`git submodule update` will revert the submodule and detach HEAD.'
  );
}

function checkCommitOrdering() {
  const dirty = git(['status', '--porcelain']);
  if (dirty === null) return;
  if (dirty) {
    // Not a failure by itself — but the outer pointer must not be bumped yet.
    return record(
      true,
      'commit ordering',
      'submodule has uncommitted changes — commit INSIDE SWLOR_Haks and push it ' +
        'BEFORE bumping the pointer in the outer repo, or the pointer will reference ' +
        'a commit nobody can fetch'
    );
  }
  record(true, 'commit ordering', 'submodule clean');
}

function checkPushTarget() {
  const url = git(['remote', 'get-url', 'origin']);
  if (!url) return record(false, 'submodule has a push target', 'no origin remote');

  // Pushing adaptation work to the upstream project is almost never intended.
  if (/zunath\/SWLOR_Haks/i.test(url)) {
    return record(
      false,
      'submodule points at a fork, not upstream',
      `origin is upstream (${url}). Adaptation commits have nowhere to go. ` +
        'Fork it and repoint .gitmodules + git remote set-url origin <fork>'
    );
  }
  record(true, 'submodule points at a fork, not upstream', url);

  // Upstream kept as a second remote is what makes engine and tooling fixes
  // mergeable later; without it the fork silently strands itself.
  const upstream = git(['remote', 'get-url', 'upstream']);
  if (!upstream) {
    warn(
      'upstream remote kept for merges',
      'no "upstream" remote — add it so upstream fixes stay pullable: ' +
        'git -C SWLOR_Haks remote add upstream https://github.com/zunath/SWLOR_Haks'
    );
  } else {
    record(true, 'upstream remote kept for merges', upstream);
  }
}

/**
 * A local-only branch is a single disk failure away from losing the work, and the
 * outer repo cannot reference it: bumping the pointer to a commit that exists on no
 * remote gives everyone else an unresolvable checkout.
 */
function checkBranchIsPushed() {
  const branch = git(['branch', '--show-current']);
  if (!branch) return; // already reported by checkSubmoduleBranch

  const tracking = git(['rev-parse', '--abbrev-ref', `${branch}@{upstream}`]);
  if (!tracking) {
    return record(
      false,
      'adaptation branch is pushed',
      `"${branch}" has no remote tracking branch. Push it before bumping the outer ` +
        `pointer: git -C SWLOR_Haks push -u origin ${branch}`
    );
  }

  const ahead = git(['rev-list', '--count', `${tracking}..${branch}`]);
  if (ahead && ahead !== '0') {
    return record(false, 'adaptation branch is pushed', `${ahead} commit(s) not yet pushed to ${tracking}`);
  }
  record(true, 'adaptation branch is pushed', `tracking ${tracking}`);
}

// ---------------------------------------------------------------------- B. TLK

function loadTlk() {
  try {
    return JSON.parse(fs.readFileSync(TLK_JSON, 'utf8'));
  } catch (e) {
    record(false, 'TLK JSON parses', e.message);
    return null;
  }
}

function checkTlkShape(tlk) {
  if (!tlk) return null;
  if (!Array.isArray(tlk.entries)) {
    record(false, 'TLK JSON parses', 'no entries array');
    return null;
  }
  const ids = new Set();
  let dupes = 0;
  for (const e of tlk.entries) {
    if (ids.has(e.id)) dupes++;
    ids.add(e.id);
  }
  record(dupes === 0, 'TLK entry ids are unique', dupes ? `${dupes} duplicate id(s)` : `${tlk.entries.length} entries`);
  return ids;
}

function checkTlkBinaryInSync() {
  if (!fs.existsSync(NWN_TLK)) {
    return record(false, 'TLK binary matches JSON', 'nwn_tlk.exe not found — cannot verify');
  }
  const tmp = path.join(os.tmpdir(), `swlor-tlk-check-${process.pid}.tlk`);
  try {
    // Run from the hak directory, and capture stdio rather than discarding it:
    // nwn_tlk fails outright when its stdout is closed by stdio:'ignore'.
    execFileSync(NWN_TLK, ['-i', TLK_JSON, '-o', tmp], {
      cwd: HAKS,
      stdio: ['ignore', 'pipe', 'pipe'],
    });
    const same = Buffer.compare(fs.readFileSync(TLK_BIN), fs.readFileSync(tmp)) === 0;
    record(
      same,
      'TLK binary matches JSON',
      same
        ? 'in sync'
        : 'STALE — regenerate with: cd SWLOR_Haks && ./nwn_tlk.exe -i ./sw_tlk/sw_tlk.tlk.json -o ./sw_tlk/sw_tlk.tlk'
    );
  } catch (e) {
    record(false, 'TLK binary matches JSON', 'regeneration failed: ' + e.message);
  } finally {
    fs.rmSync(tmp, { force: true });
  }
}

// ------------------------------------------------------------ C. 2DA <-> TLK

/**
 * Every custom strref a 2DA points at must resolve to a real TLK entry.
 *
 * This is the failure AGENTS.md warns about — moving or renumbering a TLK entry
 * without updating its references. Nothing else in the toolchain catches it: the
 * game simply renders "Bad Strref" at runtime.
 */
function checkDanglingStrrefs(ids) {
  if (!ids) return;
  if (!fs.existsSync(TWO_DA)) return record(false, 'no dangling 2DA strrefs', 'sw_2da not found');

  // 2DA columns hold plenty of large numbers that are not strrefs at all. Bound the
  // search to the range the TLK could actually address, or the check drowns in false
  // positives from unrelated numeric columns.
  const maxStrref = CUSTOM_STRREF_BASE + Math.max(...ids);

  const dangling = new Map();
  let scanned = 0;

  for (const file of fs.readdirSync(TWO_DA).filter((f) => f.endsWith('.2da'))) {
    scanned++;
    const text = fs.readFileSync(path.join(TWO_DA, file), 'utf8');
    for (const m of text.matchAll(/\b(\d{8,})\b/g)) {
      const v = Number(m[1]);
      if (v < CUSTOM_STRREF_BASE || v > maxStrref) continue;
      if (!ids.has(v - CUSTOM_STRREF_BASE)) {
        if (!dangling.has(file)) dangling.set(file, new Set());
        dangling.get(file).add(v);
      }
    }
  }

  if (dangling.size === 0) {
    return record(true, 'no dangling 2DA strrefs', `${scanned} 2DA files scanned`);
  }

  const detail = [...dangling.entries()]
    .slice(0, 5)
    .map(([f, refs]) => `${f}: ${[...refs].slice(0, 4).join(', ')}${refs.size > 4 ? '…' : ''}`)
    .join('; ');
  record(false, 'no dangling 2DA strrefs', `${dangling.size} file(s) reference missing entries — ${detail}`);
}

// -------------------------------------------------------------------- D. build

function checkBuildScript() {
  const cmd = path.join(HAKS, 'BuildHaks.cmd');
  if (!fs.existsSync(cmd)) return record(false, 'hak builder present', 'BuildHaks.cmd missing');

  const text = fs.readFileSync(cmd, 'utf8');
  const legacy = path.join(HAKS, 'NWN.FinalFantasy.CLI.exe');

  // BuildHaks.cmd drives a large binary vendored into the submodule, while the
  // maintained builder lives in SWLOR.CLI. Both work today, so this is a divergence
  // advisory rather than a failure: changes to HakBuilder.cs will not reach the
  // vendored copy. Flagging it as a hard failure would train people to ignore the gate.
  if (/FinalFantasy/i.test(text)) {
    if (!fs.existsSync(legacy)) {
      return record(false, 'hak builder present', 'BuildHaks.cmd calls NWN.FinalFantasy.CLI.exe, which is missing');
    }
    warn(
      'hak builder is the maintained one',
      'BuildHaks.cmd drives the vendored NWN.FinalFantasy.CLI.exe; HakBuilder.cs changes in ' +
        'SWLOR.CLI will not reach it. Harmless until the two diverge.'
    );
    return record(true, 'hak builder present', 'vendored builder found');
  }
  record(true, 'hak builder present', 'ok');
}

// ---------------------------------------------------------------------- report

if (!fs.existsSync(HAKS)) {
  console.error('SWLOR_Haks not found — is the submodule initialised?');
  process.exit(2);
}

checkSubmoduleBranch();
checkPushTarget();
checkBranchIsPushed();
checkPointerMatchesHead();
checkCommitOrdering();
const tlk = loadTlk();
const ids = checkTlkShape(tlk);
checkTlkBinaryInSync();
checkDanglingStrrefs(ids);
checkBuildScript();

let failed = 0;
let warned = 0;
for (const r of results) {
  if (!r.ok) failed++;
  else if (r.warn) warned++;
  const tag = r.ok ? (r.warn ? 'WARN' : 'PASS') : 'FAIL';
  console.log(`${tag}  ${r.name}${r.detail ? `\n        ${r.detail}` : ''}`);
}
const summary = [failed ? `${failed} failing` : null, warned ? `${warned} warning(s)` : null]
  .filter(Boolean)
  .join(', ');
console.log('\n' + (summary || 'all passing'));
process.exit(failed ? 1 : 0);

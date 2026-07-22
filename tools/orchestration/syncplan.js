#!/usr/bin/env node
/**
 * Keeps the plan of record in sync between the repo and the approving session.
 *
 *   node tools/orchestration/syncplan.js               # check for drift (default)
 *   node tools/orchestration/syncplan.js --to-session # repo copy wins (the usual direction)
 *   node tools/orchestration/syncplan.js --to-repo    # session copy wins
 *   node tools/orchestration/syncplan.js --diff        # show which sections differ
 *
 * design/shadowrun/PLAN.md is canonical and ships with the repo. Claude Code also
 * keeps a working copy under ~/.claude/plans/, which is machine-local and vanishes
 * with the session — so the repo copy is the one that matters, and drift between
 * them means a material plan change was made somewhere it will not survive.
 *
 * Exit 0 = in sync (or sync applied). Exit 1 = drifted. Exit 2 = a copy is missing.
 */

const fs = require('fs');
const os = require('os');
const path = require('path');

const REPO_ROOT = process.env.CLAUDE_PROJECT_DIR || path.resolve(__dirname, '..', '..');
const PLAN_BASENAME = 'investigate-in-detail-a-merry-moler.md';

const REPO_PLAN = path.join(REPO_ROOT, 'design', 'shadowrun', 'PLAN.md');
const SESSION_PLAN = path.join(os.homedir(), '.claude', 'plans', PLAN_BASENAME);

/**
 * Strip the repo copy's provenance header so the two are comparable.
 *
 * Both files start with the same H1 and both reach a `## ` heading; only the repo
 * copy carries a blockquote between them. Normalising to "H1 + everything from the
 * first `## ` onward" makes the comparison stable without a fragile marker.
 */
function normalize(text) {
  const lines = text.replace(/\r\n/g, '\n').split('\n');
  const h1 = lines.find((l) => l.startsWith('# ')) || '';
  const start = lines.findIndex((l) => l.startsWith('## '));
  const body = start === -1 ? '' : lines.slice(start).join('\n');
  return (h1 + '\n\n' + body).trim();
}

function read(p) {
  try {
    return fs.readFileSync(p, 'utf8');
  } catch {
    return null;
  }
}

/** Section-level diff: which `## ` headings differ, rather than a line dump. */
function sectionsOf(text) {
  const out = new Map();
  let key = '(preamble)';
  let buf = [];
  for (const line of normalize(text).split('\n')) {
    if (line.startsWith('## ')) {
      out.set(key, buf.join('\n').trim());
      key = line.replace(/^##\s+/, '');
      buf = [];
    } else {
      buf.push(line);
    }
  }
  out.set(key, buf.join('\n').trim());
  return out;
}

/** `a` is the session copy, `b` the repo copy. Absence from one means presence in the other. */
function diffSections(a, b) {
  const sa = sectionsOf(a);
  const sb = sectionsOf(b);
  const keys = [...new Set([...sa.keys(), ...sb.keys()])];
  const changed = [];
  for (const k of keys) {
    if (!sa.has(k)) changed.push(`- only in repo:    ${k}`);
    else if (!sb.has(k)) changed.push(`+ only in session: ${k}`);
    else if (sa.get(k) !== sb.get(k)) changed.push(`~ differs:         ${k}`);
  }
  return changed;
}

/**
 * Drift state, for programmatic callers (the Stop hook in track.js uses this so the
 * comparison logic lives in exactly one place).
 *
 * Returns { drifted, reason, sections }. `drifted` is false when there is nothing to
 * compare — a missing session copy is the normal case outside the approving session
 * and must never be reported as drift.
 */
function planDrift() {
  const repo = read(REPO_PLAN);
  const session = read(SESSION_PLAN);
  if (repo === null) return { drifted: false, reason: 'missing-repo-plan', sections: [] };
  if (session === null) return { drifted: false, reason: 'no-session-copy', sections: [] };
  if (normalize(repo) === normalize(session)) return { drifted: false, reason: 'in-sync', sections: [] };
  return { drifted: true, reason: 'drift', sections: diffSections(session, repo) };
}

module.exports = { planDrift, normalize, REPO_PLAN, SESSION_PLAN };

// Everything below is the CLI. Skipped when this file is require()'d.
if (require.main !== module) return;

const mode = process.argv[2] || '--check';
const repo = read(REPO_PLAN);
const session = read(SESSION_PLAN);

if (repo === null) {
  console.error(`missing repo plan: ${REPO_PLAN}`);
  process.exit(2);
}
if (session === null) {
  // Normal outside the approving session — nothing to reconcile against.
  console.log('session plan copy not present; repo plan is the only copy (in sync by default)');
  process.exit(0);
}

const inSync = normalize(repo) === normalize(session);

// The usual direction. Deliberate plan changes are made in the repo copy, because that
// is the one that survives the session, so pushing repo -> session is what reconciling
// normally means. Without this the only available sync overwrites the canonical copy
// from a stale snapshot, which silently destroys the change being recorded.
if (mode === '--to-session') {
  if (inSync) {
    console.log('already in sync; nothing to do');
    process.exit(0);
  }
  fs.writeFileSync(SESSION_PLAN, repo, 'utf8');
  console.log(`synced ${path.relative(REPO_ROOT, REPO_PLAN)} -> session copy`);
  process.exit(0);
}

if (mode === '--to-repo') {
  if (inSync) {
    console.log('already in sync; nothing to do');
    process.exit(0);
  }
  // Preserve the repo copy's provenance header, replace the body.
  const header = repo.slice(0, repo.indexOf('\n## '));
  const bodyStart = session.indexOf('\n## ');
  fs.writeFileSync(REPO_PLAN, header + session.slice(bodyStart), 'utf8');
  console.log(`synced session -> ${path.relative(REPO_ROOT, REPO_PLAN)} (header preserved)`);
  console.log('record the change in LEDGER.md and the reasoning in DECISIONS.md');
  process.exit(0);
}

if (inSync) {
  console.log('plan of record is in sync');
  process.exit(0);
}

console.error('PLAN DRIFT: design/shadowrun/PLAN.md and the session copy differ\n');
for (const line of diffSections(session, repo)) console.error('  ' + line);
console.error('\nreconcile with:  node tools/orchestration/syncplan.js --to-session   (repo wins - usual)');
console.error('             or:  node tools/orchestration/syncplan.js --to-repo      (session wins)');
console.error('then record the change in LEDGER.md and DECISIONS.md');
process.exit(1);

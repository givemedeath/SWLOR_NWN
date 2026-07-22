#!/usr/bin/env node
/**
 * Orchestration tracking hook for the Shadowrun presentation-layer work.
 *
 * Invoked by Claude Code hooks configured in .claude/settings.json. Reads the hook
 * payload as JSON on stdin and appends a durable record under design/shadowrun/.
 *
 *   node tools/orchestration/track.js event      # PostToolUse on Edit|Write
 *   node tools/orchestration/track.js substart   # SubagentStart
 *   node tools/orchestration/track.js subagent   # SubagentStop
 *   node tools/orchestration/track.js stop       # Stop
 *
 * Node is used rather than jq because jq is not installed in this environment.
 *
 * This script must never fail a tool call: every path is wrapped so a tracking
 * problem can't block real work. It exits 0 unconditionally.
 */

const fs = require('fs');
const path = require('path');

const REPO_ROOT = process.env.CLAUDE_PROJECT_DIR || path.resolve(__dirname, '..', '..');
const TRACK_DIR = path.join(REPO_ROOT, 'design', 'shadowrun');
const EVENTS = path.join(TRACK_DIR, 'events.jsonl');
const LEDGER = path.join(TRACK_DIR, 'LEDGER.md');
const CURRENT = path.join(TRACK_DIR, '.current-package');

/** Files whose edits are tracking noise rather than tracked work. */
const SELF_WRITE = /(events\.jsonl|LEDGER\.md|\.current-package)$/;

function currentPackage() {
  try {
    return fs.readFileSync(CURRENT, 'utf8').trim() || 'unassigned';
  } catch {
    return 'unassigned';
  }
}

/**
 * Read the hook payload synchronously from fd 0.
 *
 * Deliberately synchronous. An async reader racing `process.exit(0)` exits before
 * stdin's 'end' fires often enough to drop events silently — which is the worst
 * possible failure mode for a tracking hook, since it looks like nothing happened.
 */
function readStdin() {
  try {
    return fs.readFileSync(0, 'utf8');
  } catch {
    return '';
  }
}

function appendEvent(record) {
  fs.mkdirSync(TRACK_DIR, { recursive: true });
  fs.appendFileSync(EVENTS, JSON.stringify(record) + '\n', 'utf8');
}

function appendLedger(text) {
  fs.mkdirSync(TRACK_DIR, { recursive: true });
  fs.appendFileSync(LEDGER, text, 'utf8');
}

function today() {
  return new Date().toISOString().slice(0, 10);
}

/** All recorded events, oldest first. Malformed lines are skipped, not fatal. */
function readEvents() {
  try {
    return fs
      .readFileSync(EVENTS, 'utf8')
      .trim()
      .split('\n')
      .filter(Boolean)
      .map((l) => {
        try {
          return JSON.parse(l);
        } catch {
          return null;
        }
      })
      .filter(Boolean);
  } catch {
    return [];
  }
}

/**
 * Events recorded after the most recent of the given boundary markers.
 *
 * `events.jsonl` is gitignored scratch that persists across sessions, so counting
 * the whole file would inflate every summary. Boundary markers let each summary
 * describe only the work that belongs to it.
 *
 * Accepts several marker types because a dispatch window closes on *whichever came
 * last*: `SubagentStart` opens it, but a preceding `SubagentStop` must also close
 * the previous one — otherwise two dispatches without an intervening start would
 * both claim the same edits.
 */
function eventsSince(...boundaryTools) {
  const all = readEvents();
  let start = 0;
  for (let i = all.length - 1; i >= 0; i--) {
    if (boundaryTools.includes(all[i].tool)) {
      start = i + 1;
      break;
    }
  }
  return all.slice(start);
}

function handleEvent(payload) {
  const file = payload?.tool_input?.file_path || payload?.tool_response?.filePath || '';
  if (process.env.TRACK_DEBUG) {
    console.error('[track] REPO_ROOT=', REPO_ROOT);
    console.error('[track] EVENTS=', EVENTS);
    console.error('[track] file=', JSON.stringify(file));
    console.error('[track] self?', SELF_WRITE.test(file.replace(/\\/g, '/')));
  }
  if (!file || SELF_WRITE.test(file.replace(/\\/g, '/'))) return;

  // Files outside the repo (scratchpad, home) would relativise to a confusing
  // ../../.. chain. Keep those absolute so the record stays readable.
  const rel = path.relative(REPO_ROOT, file).replace(/\\/g, '/');
  const inRepo = rel && !rel.startsWith('../') && !path.isAbsolute(rel);

  // Plan edits are called out so a wave-gate review can see at a glance that scope
  // moved, without having to diff the plan itself.
  const norm = file.replace(/\\/g, '/').toLowerCase();
  const isPlan = norm.endsWith('/design/shadowrun/plan.md') || norm.includes('/.claude/plans/');

  appendEvent({
    ts: new Date().toISOString(),
    pkg: currentPackage(),
    tool: payload.tool_name || 'unknown',
    file: inRepo ? rel : file.replace(/\\/g, '/'),
    ...(inRepo ? {} : { external: true }),
    ...(isPlan ? { plan: true } : {}),
  });
}

/** Marks the true start of a dispatch so the completion summary can bound itself. */
function handleSubagentStart(payload) {
  appendEvent({
    ts: new Date().toISOString(),
    pkg: currentPackage(),
    tool: 'SubagentStart',
    file: '',
    agent: payload.agent_name || payload.subagent_type || 'subagent',
  });
}

function handleSubagent(payload) {
  const pkg = currentPackage();
  // Bound by whichever of SubagentStart/SubagentStop came last. Bounding only by the
  // previous SubagentStop swept in the controller's own edits between dispatches — that
  // is what made a one-file package report 16 edits across 6 files.
  const work = eventsSince('SubagentStart', 'SubagentStop').filter((e) => e.file);
  const agent = payload.agent_name || payload.subagent_type || 'subagent';

  appendEvent({ ts: new Date().toISOString(), pkg, tool: 'SubagentStop', file: '', agent });

  // Only touch the ledger when the dispatch actually changed something. LEDGER.md
  // is the curated durable record; appending boilerplate for every subagent that
  // did no tracked work degrades exactly the artifact it exists to preserve.
  if (work.length === 0) return;

  const files = [...new Set(work.map((e) => e.file))].sort();
  const shown = files.slice(0, 12);
  const more = files.length - shown.length;

  appendLedger(
    `\n**${today()} · ${pkg} · subagent completed · ${agent}**\n\n` +
      `${work.length} edit${work.length === 1 ? '' : 's'} across ${files.length} file` +
      `${files.length === 1 ? '' : 's'}:\n\n` +
      shown.map((f) => `- \`${f}\``).join('\n') +
      (more > 0 ? `\n- …and ${more} more` : '') +
      `\n\n*Auto-captured. Replace with a summary of what changed before closing the wave.*\n`
  );
}

function handleStop() {
  // Per-package counts for the work done since the last Stop, so the wave-gate
  // reconciliation starts from a summary rather than a raw event dump.
  const since = eventsSince('Stop').filter((e) => e.file);

  if (since.length > 0) {
    const counts = {};
    for (const e of since) counts[e.pkg] = (counts[e.pkg] || 0) + 1;

    const summary = Object.entries(counts)
      .sort((a, b) => b[1] - a[1])
      .map(([pkg, n]) => `${pkg}=${n}`)
      .join(' ');

    appendEvent({ ts: new Date().toISOString(), pkg: 'session', tool: 'Stop', file: '', summary });
  }

  // Back-feed guard: the repo plan is the version that survives this session, so
  // warn if a change landed only in the session's working copy.
  warnOnPlanDrift(since);
}

/**
 * Surface plan drift to the user at session end.
 *
 * Emitting `systemMessage` is the only way a Stop hook can reach the user, and this
 * is the one condition worth interrupting for: a scope change that exists only in
 * the machine-local plan copy is a change that will be lost.
 */
function warnOnPlanDrift(sessionEvents) {
  let drift;
  try {
    drift = require('./syncplan.js').planDrift();
  } catch {
    return; // never let the guard break the hook
  }
  if (!drift.drifted) return;

  const touchedPlan = sessionEvents.some((e) => e.plan);
  const detail = drift.sections.slice(0, 5).join('; ');

  process.stdout.write(
    JSON.stringify({
      systemMessage:
        'PLAN DRIFT: design/shadowrun/PLAN.md differs from the session plan copy' +
        (touchedPlan ? ' (the plan was edited this session)' : '') +
        (detail ? ` — ${detail}` : '') +
        '. Reconcile with: node tools/orchestration/syncplan.js --to-repo',
    })
  );
}

(() => {
  try {
    const mode = process.argv[2];
    const raw = readStdin();
    let payload = {};
    try {
      payload = raw ? JSON.parse(raw) : {};
    } catch {
      payload = {};
    }

    if (mode === 'event') handleEvent(payload);
    else if (mode === 'substart') handleSubagentStart(payload);
    else if (mode === 'subagent') handleSubagent(payload);
    else if (mode === 'stop') handleStop();
  } catch (e) {
    // Tracking must never break a tool call.
    if (process.env.TRACK_DEBUG) console.error('track.js:', e && e.stack);
  }
  process.exit(0);
})();

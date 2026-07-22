#!/usr/bin/env node
/**
 * Self-test for the orchestration tracking hook.
 *
 *   node tools/orchestration/selftest.js
 *
 * Run this after changing track.js, or when tracking appears to have stopped
 * working. A tracking hook that silently does nothing looks identical to one that
 * is working, so this asserts on real side effects rather than exit codes.
 *
 * Fixtures are built with JSON.stringify rather than written by hand: shell
 * heredocs and `echo` mangle backslashes in Windows paths, producing payloads
 * with invalid JSON escapes that make a healthy hook look broken.
 */

const fs = require('fs');
const os = require('os');
const path = require('path');
const { execFileSync } = require('child_process');

const ROOT = path.resolve(__dirname, '..', '..');
const TRACK = path.join(__dirname, 'track.js');
const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'swlor-track-'));

// Redirect the hook's output at a scratch tree so the real ledger is untouched.
const sandbox = fs.mkdtempSync(path.join(os.tmpdir(), 'swlor-sandbox-'));
fs.mkdirSync(path.join(sandbox, 'design', 'shadowrun'), { recursive: true });
fs.writeFileSync(path.join(sandbox, 'design', 'shadowrun', '.current-package'), 'TEST\n');

const EVENTS = path.join(sandbox, 'design', 'shadowrun', 'events.jsonl');
const env = { ...process.env, CLAUDE_PROJECT_DIR: sandbox };

function payload(name, obj) {
  const p = path.join(tmp, name);
  fs.writeFileSync(p, JSON.stringify(obj), 'utf8');
  return p;
}

function run(mode, file) {
  execFileSync(process.execPath, [TRACK, mode], {
    stdio: [file ? fs.openSync(file, 'r') : 'ignore', 'ignore', 'ignore'],
    env,
  });
}

/** Same as run(), but returns the hook's stdout (where systemMessage JSON goes). */
function runCapture(mode, extraEnv = {}) {
  return execFileSync(process.execPath, [TRACK, mode], {
    stdio: ['ignore', 'pipe', 'ignore'],
    env: { ...env, ...extraEnv },
    encoding: 'utf8',
  });
}

function lines() {
  try {
    return fs.readFileSync(EVENTS, 'utf8').trim().split('\n').filter(Boolean);
  } catch {
    return [];
  }
}

const cases = [
  ['windows path is recorded', () => {
    run('event', payload('win.json', {
      tool_name: 'Edit',
      tool_input: { file_path: path.join(ROOT, 'SWLOR.Game.Server', 'Service', 'Combat.cs') },
    }));
    return lines().length === 1;
  }],
  ['forward-slash path is recorded', () => {
    run('event', payload('fwd.json', {
      tool_name: 'Write',
      tool_input: { file_path: 'D:/x/SWLOR.Game.Server/Service/Stat.cs' },
    }));
    return lines().length === 2;
  }],
  ['self-write to LEDGER.md is skipped', () => {
    run('event', payload('self.json', {
      tool_name: 'Write',
      tool_input: { file_path: path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md') },
    }));
    return lines().length === 2;
  }],
  ['malformed payload is survivable', () => {
    run('event', payload('bad.json', 'not-an-object'));
    return lines().length === 2;
  }],
  ['package attribution is captured', () => JSON.parse(lines()[0]).pkg === 'TEST'],
  ['path outside the repo stays absolute and is flagged', () => {
    run('event', payload('ext.json', {
      tool_name: 'Write',
      tool_input: { file_path: path.join(os.tmpdir(), 'somewhere-else.txt') },
    }));
    const last = JSON.parse(lines()[lines().length - 1]);
    return last.external === true && !last.file.startsWith('../');
  }],
  ['subagent that changed files writes a ledger entry listing them', () => {
    run('subagent', payload('sub.json', { agent_name: 'sonnet-P2' }));
    const ledger = fs.readFileSync(path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md'), 'utf8');
    return ledger.includes('sonnet-P2') && ledger.includes('Service/Combat.cs');
  }],
  ['subagent summary excludes controller edits made before dispatch', () => {
    // Controller edit, then dispatch starts, then the subagent's own edit. Only the
    // latter may appear — attributing pre-dispatch work to the subagent is what made
    // a one-file package report 16 edits.
    run('event', payload('ctl.json', {
      tool_name: 'Edit',
      tool_input: { file_path: path.join(sandbox, 'controller-only.md') },
    }));
    run('substart', payload('start.json', { agent_name: 'sonnet-P3' }));
    run('event', payload('own.json', {
      tool_name: 'Write',
      tool_input: { file_path: path.join(sandbox, 'subagent-owned.md') },
    }));
    const before = fs.readFileSync(path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md'), 'utf8');
    run('subagent', payload('sub3.json', { agent_name: 'sonnet-P3' }));
    const added = fs
      .readFileSync(path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md'), 'utf8')
      .slice(before.length);
    return added.includes('subagent-owned.md') && !added.includes('controller-only.md');
  }],
  ['subagent that changed nothing does NOT pollute the ledger', () => {
    const before = fs.readFileSync(path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md'), 'utf8');
    run('subagent', payload('sub2.json', { agent_name: 'idle-agent' }));
    const after = fs.readFileSync(path.join(sandbox, 'design', 'shadowrun', 'LEDGER.md'), 'utf8');
    return before === after;
  }],
  ['stop counts only work since the previous stop', () => {
    run('stop', null);
    const first = JSON.parse(lines()[lines().length - 1]);
    // A second stop with no intervening edits must not emit a duplicate summary.
    const n = lines().length;
    run('stop', null);
    return first.tool === 'Stop' && first.summary.includes('TEST=') && lines().length === n;
  }],
  ['plan edits are flagged for wave-gate review', () => {
    run('event', payload('plan.json', {
      tool_name: 'Edit',
      tool_input: { file_path: path.join(sandbox, 'design', 'shadowrun', 'PLAN.md') },
    }));
    return JSON.parse(lines()[lines().length - 1]).plan === true;
  }],
  ['no plan drift warning when there is nothing to compare', () => {
    // No repo PLAN.md in the sandbox: must stay silent rather than cry wolf.
    return runCapture('stop').trim() === '';
  }],
  ['plan drift emits a systemMessage the user will see', () => {
    // Give the sandbox both copies, differing by one section.
    const home = fs.mkdtempSync(path.join(os.tmpdir(), 'swlor-home-'));
    fs.mkdirSync(path.join(home, '.claude', 'plans'), { recursive: true });
    const base = '# Plan\n\n## Context\n\nshared body\n';
    fs.writeFileSync(path.join(sandbox, 'design', 'shadowrun', 'PLAN.md'), base);
    fs.writeFileSync(
      path.join(home, '.claude', 'plans', 'investigate-in-detail-a-merry-moler.md'),
      base + '\n## Added Later\n\nonly in session\n'
    );
    run('event', payload('more.json', {
      tool_name: 'Edit',
      tool_input: { file_path: path.join(sandbox, 'SWLOR.Game.Server', 'Service', 'X.cs') },
    }));
    const out = runCapture('stop', { USERPROFILE: home, HOME: home });
    fs.rmSync(home, { recursive: true, force: true });
    if (!out.trim()) return false;
    const msg = JSON.parse(out).systemMessage || '';
    return msg.includes('PLAN DRIFT') && msg.includes('Added Later') && msg.includes('syncplan.js');
  }],
];

let failed = 0;
for (const [name, fn] of cases) {
  let ok = false;
  try {
    ok = fn();
  } catch (e) {
    ok = false;
    console.error('   ' + e.message);
  }
  console.log(`${ok ? 'PASS' : 'FAIL'}  ${name}`);
  if (!ok) failed++;
}

fs.rmSync(tmp, { recursive: true, force: true });
fs.rmSync(sandbox, { recursive: true, force: true });

console.log(failed ? `\n${failed} failing` : '\nall passing');
process.exit(failed ? 1 : 0);

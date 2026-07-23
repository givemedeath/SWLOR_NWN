# P2c — Barrens creature and encounter set

**Status:** not started
**Depends on:** accepted P2b combat space
**Blocks:** Phase 2 gate and run combat

## Scope

Create the minimum readable encounter vocabulary for the proof street:

| Role | First-slice example | Player-readable cue |
|---|---|---|
| pressure | melee ganger | closes distance; light armor |
| ranged damage | ganger shooter | visible firearm and firing lane |
| control/support | lieutenant or mage | distinctive silhouette/VFX |
| durable threat | corp heavy or augmented ganger | heavy armor; weak attacks can soak to zero |
| drone/critters | one non-metahuman threat | different movement and damage expectations |

Each blueprint needs a provenance-safe appearance, role-level stat target, equipment source,
loot/economy classification, spawn budget, combat log name, and removal/cleanup behavior. Reuse the
existing stat-skin and spawn-table infrastructure, but do not ship Star Wars names, faction text, or
unconverted loot through it.

## Work

1. Define a level/role matrix against the current subtractive-soak curve.
2. Author the smallest blueprint set that covers the matrix.
3. Add a Barrens spawn table with bounded density, cooldown, cleanup, and party scaling policy.
4. Build an explicit loot allowlist; every drop must belong to the Erie economy or be marked
   unavailable.
5. Validate glitch, wound, enmity, death, respawn, and fully-soaked-hit feedback for NPCs and players.
6. Measure server/frame impact at expected and stress concurrency.

## Gate

A small party can identify roles before reading logs, weak attacks visibly bounce from the durable
target, focus/control decisions matter, rewards are Erie-appropriate, and repeated spawn/cleanup
cycles leak neither objects nor encounter state. Record combat duration and resource use for at least
three builds, including a fresh character.

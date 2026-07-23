# P2d — Barrens NPCs, shops, fixers, and service wiring

**Status:** not started
**Depends on:** P2b; coordinate with P2c
**Blocks:** Phase 2 acceptance and Phase 3 run delivery

## Scope

Populate the proof street with only the actors needed to explain and operate its loop:

- cyberdoc who opens the existing clinic NUI;
- fixer/Johnson who becomes the authored-run delivery point;
- merchant or bartender who anchors recovery and basic supplies;
- ambient actors sufficient to make the space inhabited;
- transitions, service waypoints, and spawn anchors.

## Rules

All names, factions, dialog, inventory, prices, and appearances must be Erie-native and traceable to
the district source of truth. Shops use an explicit item allowlist; inherited stores and generic item
searches must not leak legacy inventory. Player-facing identity uses the `PlayerName` service. Service
actors need failure behavior for unavailable systems rather than dead conversation branches.

Coordinate Johnson placement with the first authored run’s route, legwork, hostile space, return path,
and party lifecycle. The fixer is not accepted merely because a conversation opens.

## Gate

A new player can enter the district, understand where to get work, buy/recover, reach the clinic,
install/remove cyberware, and return to the fixer without external instructions. Dialog contains no
legacy vocabulary, inventories contain no restricted or unavailable items, service actions are
idempotent, and all NPCs survive reset/reconnect behavior as designed.

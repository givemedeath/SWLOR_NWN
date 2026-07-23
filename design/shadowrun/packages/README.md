# Package Briefs

Each brief is a self-contained, bounded work package that can be handed to an agent without the
originating conversation.

The original `P0`–`P4` files record the presentation/combat wave; `P1a`–`P1c` record identity
mechanics. Current world packages use the unique IDs in [the plan of record](../PLAN.md):

- `P2m` — module/runtime foundation;
- `P2a` — district design;
- `P2b` — area build-out;
- `P2c` — creatures and encounters;
- `P2d` — NPCs, shops, fixers, and service wiring.

A brief must never duplicate the master plan, combine two package IDs, or call an automated build an
in-game acceptance. New briefs are authored when their dependencies and measurements are current, and
must include scope, exclusions, artifacts, automated verification, live gate, and rollback/cleanup
where relevant.

See [ORCHESTRATION.md](../ORCHESTRATION.md) for delegation rules. Asset-side packages must also include
the applicable HAK/TLK rules from [HAKS.md](../HAKS.md).

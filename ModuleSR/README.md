# ModuleSR — Erie Metroplex

`ModuleSR/` is the only supported runtime module on the Shadowrun adaptation branch. It provides a
small, explicit content boundary while the historical `../Module` remains in the repository for
regression tests and reference. The old module is not maintained as a second playable world here.

See [P2m](../design/shadowrun/packages/P2m.md), the current
[plan of record](../design/shadowrun/PLAN.md), and D20–D23 in
[DECISIONS.md](../design/shadowrun/DECISIONS.md).

## Runtime contract

The debug server selects:

```text
NWN_MODULE="Erie Metroplex"
SWLOR_GAME_PROFILE=shadowrun
SWLOR_DATA_NAMESPACE=erie
```

Shadowrun-profile startup fails without a data namespace. Persistent entity keys and RediSearch
indexes therefore cannot silently collide with the historical world.

## Committed resources

| Path | Purpose |
|---|---|
| `ifo/module.ifo.json` | Erie name, event/TLK wiring, six-area list, entry coordinates, 9-HAK allowlist |
| `are/erie_arrival.are.json` | deterministic clean-room arrival generated from Sci-Fi Base seed 4242 |
| `git/erie_arrival.git.json` | entry and default-respawn waypoints |
| `are/git gen_placeholder1-4` | four verified procgen templates |
| `are/git/gic no_access` | private runtime storage area; no creature instances |
| `uti/` | the three items `PlayerInitialization` creates for new characters |
| `fac/`, `jrl/` | faction and journal scaffolding |

Erie ships the **full shared SWLOR HAK stack** (derived from the reference `Module/` IFO so the two
stay in parity). The P2m 9-HAK allowlist was reverted in
[D25](../design/shadowrun/DECISIONS.md) because placeables, creatures, and items render their models
from the shared placeable/creature/item HAKs, not the tileset HAKs — a minimal tileset-only allowlist
left committed street dressing silently invisible. This is not a claim that inherited generic assets
have completed a legal/provenance review; the minimal allowlist returns as a pre-release gate.

`ncs/` and `nss/` are mirrored from `../Module` at pack time because the C# event bridge remains
shared. Other empty resource directories are created for the packer and ignored.

## Prepare and build

After changing the arrival, starter resources, area list, or HAK policy:

```powershell
powershell -ExecutionPolicy Bypass -File tools/PrepareShadowrunModule.ps1
```

Pack and deploy:

```bat
ModuleSR\PackModuleSR.cmd
```

The pack command deploys the module and the exact nine HAKs plus custom TLK
from `SWLOR_Haks/output/` to `debugserver/`. Build the changed HAK first; it
fails rather than silently retaining an older deployed copy.

Packing also writes `Erie Metroplex.release.json` beside the module and in
`debugserver/modules/`. It records SHA-256 hashes of the module, custom TLK, server assembly, and every
referenced HAK, along with both source revisions and dirty-state flags. Dirty manifests are development
artifacts, not release candidates.

## Acceptance

Automated tests enforce the five playable metatypes, portrait coverage, entry/respawn waypoints,
starter resources, private-area contents, and HAK/tileset closure. Final acceptance still requires
the cold-boot/create/reconnect/respawn/cyberclinic matrix in
[P2m](../design/shadowrun/packages/P2m.md).

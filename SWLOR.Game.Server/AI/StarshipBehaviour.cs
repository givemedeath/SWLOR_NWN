﻿using SWLOR.Game.Server.GameObject;

using NWN;
using SWLOR.Game.Server.NWNX;
using SWLOR.Game.Server.Service;

using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.AI
{
    /// <summary>
    /// Standard behaviour which executes for all derived behaviours
    /// </summary>
    public class StarshipBehaviour : BehaviourBase
    {
        public override bool IgnoreNWNEvents => true;
        
        public override void OnPhysicalAttacked(NWCreature self)
        {
            base.OnPhysicalAttacked(self);
            NWCreature attacker = NWScript.GetLastAttacker();
            SpaceService.OnPhysicalAttacked(self, attacker);
        }

        public override void OnPerception(NWCreature self)
        {
            base.OnPerception(self);
            SpaceService.OnPerception(NWScript.OBJECT_SELF, NWScript.GetLastPerceived());
        }

        public override void OnHeartbeat(NWCreature self)
        {
            base.OnHeartbeat(self);
            SpaceService.OnHeartbeat(NWScript.OBJECT_SELF);
        }
    }
}

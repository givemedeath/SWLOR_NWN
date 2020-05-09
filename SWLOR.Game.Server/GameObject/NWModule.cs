﻿
using System.Collections.Generic;
using NWN;
using NWScript = SWLOR.Game.Server.NWN.NWScript;


namespace SWLOR.Game.Server.GameObject
{
    public class NWModule : NWObject
    {
        public NWModule(uint nwnObject) 
            : base(nwnObject)
        {
        }

        public static NWModule Get()
        {
            return new NWModule(NWScript.GetModule());
        }

        public IEnumerable<NWPlayer> Players
        {
            get
            {
                for (NWPlayer pc = NWScript.GetFirstPC(); pc.IsValid; pc = NWScript.GetNextPC())
                {
                    yield return pc;
                }
            }
        }

        public IEnumerable<NWArea> Areas
        {
            get
            {
                for (NWArea area = NWScript.GetFirstArea(); NWScript.GetIsObjectValid(area) == NWScript.TRUE; area = NWScript.GetNextArea())
                {
                    yield return area;
                }
            }
        }

        //
        // -- BELOW THIS POINT IS JUNK TO MAKE THE API FRIENDLIER!
        //

        public static bool operator ==(NWModule lhs, NWModule rhs)
        {
            bool lhsNull = lhs is null;
            bool rhsNull = rhs is null;
            return (lhsNull && rhsNull) || (!lhsNull && !rhsNull && lhs.Object == rhs.Object);
        }

        public static bool operator !=(NWModule lhs, NWModule rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            NWModule other = o as NWModule;
            return other != null && other == this;
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }

        public static implicit operator uint(NWModule o)
        {
            return o.Object;
        }
        public static implicit operator NWModule(uint o)
        {
            return new NWModule(o);
        }

    }
}

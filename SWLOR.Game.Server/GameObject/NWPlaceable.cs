﻿using NWN;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.GameObject
{
    public class NWPlaceable : NWObject
    {
        public NWPlaceable(uint nwnObject) 
            : base(nwnObject)
        {
        }

        public virtual bool IsUseable
        {
            get => NWScript.GetUseableFlag(Object) == 1;
            set => NWScript.SetUseableFlag(Object, value ? 1 : 0);
        }

        public virtual bool IsLocked
        {
            get => NWScript.GetLocked(Object) == 1;
            set => NWScript.SetLocked(Object, value ? 1 : 0);
        }

        //
        // -- BELOW THIS POINT IS JUNK TO MAKE THE API FRIENDLIER!
        //

        public static bool operator ==(NWPlaceable lhs, NWPlaceable rhs)
        {
            bool lhsNull = lhs is null;
            bool rhsNull = rhs is null;
            return (lhsNull && rhsNull) || (!lhsNull && !rhsNull && lhs.Object == rhs.Object);
        }

        public static bool operator !=(NWPlaceable lhs, NWPlaceable rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            NWPlaceable other = o as NWPlaceable;
            return other != null && other == this;
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }

        public static implicit operator uint(NWPlaceable o)
        {
            return o.Object;
        }

        public static implicit operator NWPlaceable(uint o)
        {
            return new NWPlaceable(o);
        }
    }
}

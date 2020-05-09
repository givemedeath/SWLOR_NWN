using System.Collections.Generic;
using System.Linq;

using NWN;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.NWN.Enum;
using static SWLOR.Game.Server.NWN.NWScript;
using NWScript = SWLOR.Game.Server.NWN.NWScript;

namespace SWLOR.Game.Server.GameObject
{
    public class NWCreature : NWObject
    {
        public NWCreature(uint o)
            : base(o)
        {

        }

        public virtual int Age => NWScript.GetAge(Object);

        public virtual float ChallengeRating => NWScript.GetChallengeRating(Object);

        public virtual ClassType Class1 => NWScript.GetClassByPosition(1, Object);

        public virtual ClassType Class2 => NWScript.GetClassByPosition(2, Object);

        public virtual ClassType Class3 => NWScript.GetClassByPosition(3, Object);

        public virtual bool IsCommandable
        {
            get => NWScript.GetCommandable(Object) == 1;
            set => NWScript.SetCommandable(value ? 1 : 0, Object);
        }

        public virtual int Size => NWScript.GetCreatureSize(Object);

        public virtual int Phenotype
        {
            get => NWScript.GetPhenoType(Object);
            set => NWScript.SetPhenoType(value, Object);
        }

        public virtual string Deity
        {
            get => NWScript.GetDeity(Object);
            set => NWScript.SetDeity(Object, value);
        }

        public virtual int RacialType => NWScript.GetRacialType(Object);

        public virtual int Gender => NWScript.GetGender(Object);

        public virtual bool IsResting => NWScript.GetIsResting(Object) == 1;

        public virtual float Weight => NWScript.GetWeight(Object) * 0.1f;

        public virtual int Strength => NWScript.GetAbilityScore(Object, ABILITY_STRENGTH);

        public virtual int Dexterity => NWScript.GetAbilityScore(Object, ABILITY_DEXTERITY);

        public virtual int Constitution => NWScript.GetAbilityScore(Object, ABILITY_CONSTITUTION);

        public virtual int Wisdom => NWScript.GetAbilityScore(Object, ABILITY_WISDOM);
        public virtual int Intelligence => NWScript.GetAbilityScore(Object, ABILITY_INTELLIGENCE);

        public virtual int Charisma => NWScript.GetAbilityScore(Object, ABILITY_CHARISMA);
        
        public virtual int StrengthModifier => NWScript.GetAbilityModifier(ABILITY_STRENGTH, Object);
        public virtual int DexterityModifier => NWScript.GetAbilityModifier(ABILITY_DEXTERITY, Object);
        public virtual int ConstitutionModifier => NWScript.GetAbilityModifier(ABILITY_CONSTITUTION, Object);
        public virtual int WisdomModifier => NWScript.GetAbilityModifier(ABILITY_WISDOM, Object);
        public virtual int IntelligenceModifier => NWScript.GetAbilityModifier(ABILITY_INTELLIGENCE, Object);
        public virtual int CharismaModifier => NWScript.GetAbilityModifier(ABILITY_CHARISMA, Object);

        public virtual int XP
        {
            get => NWScript.GetXP(Object);
            set => NWScript.SetXP(Object, value);
        }

        public bool IsInCombat => NWScript.GetIsInCombat(Object) == 1;

        public virtual void ClearAllActions(bool clearCombatState = false)
        {
            AssignCommand(() =>
            {
                NWScript.ClearAllActions(clearCombatState ? 1 : 0);
            });
        }

        public virtual NWItem Head => NWScript.GetItemInSlot(INVENTORY_SLOT_HEAD, Object);
        public virtual NWItem Chest => NWScript.GetItemInSlot(INVENTORY_SLOT_CHEST, Object);
        public virtual NWItem Boots => NWScript.GetItemInSlot(INVENTORY_SLOT_BOOTS, Object);
        public virtual NWItem Arms => NWScript.GetItemInSlot(INVENTORY_SLOT_ARMS, Object);
        public virtual NWItem RightHand => NWScript.GetItemInSlot(INVENTORY_SLOT_RIGHTHAND, Object);
        public virtual NWItem LeftHand => NWScript.GetItemInSlot(INVENTORY_SLOT_LEFTHAND, Object);
        public virtual NWItem Cloak => NWScript.GetItemInSlot(INVENTORY_SLOT_CLOAK, Object);
        public virtual NWItem LeftRing => NWScript.GetItemInSlot(INVENTORY_SLOT_LEFTRING, Object);
        public virtual NWItem RightRing => NWScript.GetItemInSlot(INVENTORY_SLOT_RIGHTRING, Object);
        public virtual NWItem Neck => NWScript.GetItemInSlot(INVENTORY_SLOT_NECK, Object);
        public virtual NWItem Belt => NWScript.GetItemInSlot(INVENTORY_SLOT_BELT, Object);
        public virtual NWItem Arrows => NWScript.GetItemInSlot(INVENTORY_SLOT_ARROWS, Object);
        public virtual NWItem Bullets => NWScript.GetItemInSlot(INVENTORY_SLOT_BULLETS, Object);
        public virtual NWItem Bolts => NWScript.GetItemInSlot(INVENTORY_SLOT_BOLTS, Object);
        public virtual NWItem CreatureWeaponLeft => NWScript.GetItemInSlot(INVENTORY_SLOT_CWEAPON_L, Object);
        public virtual NWItem CreatureWeaponRight => NWScript.GetItemInSlot(INVENTORY_SLOT_CWEAPON_R, Object);
        public virtual NWItem CreatureWeaponBite => NWScript.GetItemInSlot(INVENTORY_SLOT_CWEAPON_B, Object);
        public virtual NWItem CreatureHide => NWScript.GetItemInSlot(INVENTORY_SLOT_CARMOUR, Object);

        public virtual void FloatingText(string text, bool displayToFaction = false)
        {
            NWScript.FloatingTextStringOnCreature(text, Object, displayToFaction ? 1 : 0);
        }

        public virtual void SendMessage(string text)
        {
            NWScript.SendMessageToPC(Object, text);
        }

        public virtual bool IsDead => NWScript.GetIsDead(Object) == 1;

        public virtual bool IsPossessedFamiliar => NWScript.GetIsPossessedFamiliar(Object) == TRUE;

        public virtual bool IsDMPossessed => NWScript.GetIsDMPossessed(Object) == TRUE;

        public bool HasAnyEffect(params int[] effectIDs)
        {
            Effect eff = NWScript.GetFirstEffect(Object);
            while (NWScript.GetIsEffectValid(eff) == TRUE)
            {
                if (effectIDs.Contains(NWScript.GetEffectType(eff)))
                {
                    return true;
                }

                eff = NWScript.GetNextEffect(Object);
            }

            return false;
        }


        public virtual IEnumerable<NWItem> EquippedItems
        {
            get
            {
                for (int slot = 0; slot < NUM_INVENTORY_SLOTS; slot++)
                {
                    yield return NWScript.GetItemInSlot(slot, Object);
                }
            }
        }

        public virtual IEnumerable<NWCreature> PartyMembers
        {
            get
            {
                for (NWPlayer member = NWScript.GetFirstFactionMember(Object, FALSE); member.IsValid; member = NWScript.GetNextFactionMember(Object, FALSE))
                {
                    yield return member;
                }
            }
        }

        public virtual bool IsBusy
        {
            get => GetLocalInt("IS_BUSY") == 1;
            set => SetLocalInt("IS_BUSY", value ? 1 : 0);
        }

        //
        // -- BELOW THIS POINT IS JUNK TO MAKE THE API FRIENDLIER!
        //

        public static bool operator ==(NWCreature lhs, NWCreature rhs)
        {
            bool lhsNull = lhs is null;
            bool rhsNull = rhs is null;
            return (lhsNull && rhsNull) || (!lhsNull && !rhsNull && lhs.Object == rhs.Object);
        }

        public static bool operator !=(NWCreature lhs, NWCreature rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            NWCreature other = o as NWCreature;
            return other != null && other == this;
        }

        public override int GetHashCode()
        {
            return Object.GetHashCode();
        }

        public static implicit operator uint(NWCreature o)
        {
            return o.Object;
        }
        public static implicit operator NWCreature(uint o)
        {
            return new NWCreature(o);
        }
    }
}

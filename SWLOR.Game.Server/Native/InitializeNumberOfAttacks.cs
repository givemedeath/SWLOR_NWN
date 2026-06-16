using System.Runtime.InteropServices;
using NWN.Native.API;
using NWNX.NET;
using SWLOR.Game.Server.Core;

namespace SWLOR.Game.Server.Native
{
    public static unsafe class InitializeNumberOfAttacks
    {
        internal delegate void InitializeNumberOfAttacksHook(void* pCombatRound);

        // ReSharper disable once NotAccessedField.Local
        private static InitializeNumberOfAttacksHook _callOriginal;

        [NWNEventHandler(ScriptName.OnModuleLoad)]
        public static void RegisterHook()
        {
            delegate* unmanaged<void*, void> pHook = &OnInitializeNumberOfAttacks;
            var functionPtr = NativeLibrary.GetExport(
                NativeLibrary.GetMainProgramHandle(), "_ZN15CNWSCombatRound25InitializeNumberOfAttacksEv");
            var hookPtr = NWNXAPI.RequestFunctionHook(
                functionPtr,
                (IntPtr)pHook,
                -1000000);
            _callOriginal = Marshal.GetDelegateForFunctionPointer<InitializeNumberOfAttacksHook>((IntPtr)hookPtr);
        }

        [UnmanagedCallersOnly]
        private static void OnInitializeNumberOfAttacks(void* pCombatRound)
        {
            _callOriginal(pCombatRound);

            var pCombatRoundObject = CNWSCombatRound.FromPointer(pCombatRound);
            var pCreature = pCombatRoundObject?.m_pBaseCreature;

            if (pCreature == null ||
                !OnAIActionAttackObject.TryConsumeScheduledAttackBatch(
                    pCreature.m_idSelf,
                    out var additionalAttacks,
                    out var gateDelayMilliseconds))
            {
                return;
            }

            if (additionalAttacks <= 0)
            {
                return;
            }

            pCombatRoundObject.m_nAdditionalAttacks += additionalAttacks;
            pCombatRoundObject.m_nRoundLength = Math.Max(
                pCombatRoundObject.m_nRoundLength,
                gateDelayMilliseconds);
        }
    }
}

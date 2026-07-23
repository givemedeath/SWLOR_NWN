using SWLOR.Game.Server.Service.StatService;
using SWLOR.Game.Server.Service.StatusEffectService;
using SWLOR.NWN.API.NWScript.Enum;

namespace SWLOR.Game.Server.Feature.StatusEffectDefinition
{
    /// <summary>
    /// A critical glitch: the action went badly on a miss - a serious malfunction. Reduces both the
    /// attacker's Accuracy and Evasion for longer than a minor glitch while they sort it out.
    ///
    /// Reuses the <c>Stunned</c> effect icon for the slice; a dedicated glitch icon is deferred
    /// (P1c brief).
    /// </summary>
    public sealed class CriticalGlitchStatusEffect : StatusEffectBase
    {
        public override string Name => "Critical Glitch";
        public override EffectIconType Icon => EffectIconType.Stunned;
        public override StatusEffectCategory Categories => StatusEffectCategory.Debuff;

        public CriticalGlitchStatusEffect()
        {
            StatGroup.Stats[StatType.AccuracyPercentAdjustment] = -25;
            StatGroup.Stats[StatType.EvasionPercentAdjustment] = -25;
        }
    }
}

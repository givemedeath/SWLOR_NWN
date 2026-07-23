using SWLOR.Game.Server.Service.StatService;
using SWLOR.Game.Server.Service.StatusEffectService;
using SWLOR.NWN.API.NWScript.Enum;

namespace SWLOR.Game.Server.Feature.StatusEffectDefinition
{
    /// <summary>
    /// A minor glitch: a complication that struck even though the attack landed. Briefly reduces the
    /// attacker's Accuracy while they recover from the fumble.
    ///
    /// Reuses the <c>Confused</c> effect icon for the slice rather than dragging in the icon pipeline;
    /// a dedicated glitch icon is deferred (P1c brief).
    /// </summary>
    public sealed class GlitchStatusEffect : StatusEffectBase
    {
        public override string Name => "Glitch";
        public override EffectIconType Icon => EffectIconType.Confused;
        public override StatusEffectCategory Categories => StatusEffectCategory.Debuff;

        public GlitchStatusEffect()
        {
            StatGroup.Stats[StatType.AccuracyPercentAdjustment] = -15;
        }
    }
}

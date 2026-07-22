using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Core.NWNX.Enum;
using SWLOR.NWN.API.NWNX;

namespace SWLOR.Game.Server.Feature
{
    public static class FeedbackMessageConfiguration
    {
        /// <summary>
        /// When the module loads, configure the feedback messages.
        /// </summary>
        [NWNEventHandler(ScriptName.OnModuleLoad)]
        public static void ConfigureFeedbackMessages()
        {
            FeedbackPlugin.SetFeedbackMessageHidden(FeedbackMessageTypes.UseItemCantUse, true);
            FeedbackPlugin.SetFeedbackMessageHidden(FeedbackMessageTypes.CombatRunningOutOfAmmo, true);
            FeedbackPlugin.SetFeedbackMessageHidden(FeedbackMessageTypes.RestBeginningRest, true);
            FeedbackPlugin.SetFeedbackMessageHidden(FeedbackMessageTypes.RestFinishedRest, true);
            FeedbackPlugin.SetFeedbackMessageHidden(FeedbackMessageTypes.RestCancelRest, true);

            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.Initiative, true);
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.ComplexAttack, true);

            // Saving throw rolls ("<save type> : success/failure") are a d20 defense mechanic with
            // no Shadowrun equivalent; the SWLOR combat log reports defense outcomes separately.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.SavingThrow, true);

            // Touch attacks are a d20-only targeting concept (bypassing armor for touch-range
            // effects); Shadowrun has no "touch attack" and SWLOR does not use the term.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.TouchAttack, true);

            // Spell Resistance reports a d20 SR value and success/failure check that has no
            // Shadowrun-vocabulary equivalent; hiding it stops that D&D term leaking into ability use.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.SpellResistance, true);

            // Counterspell announces "<caster> casts <spell> : countered by <caster> casting <spell>",
            // a D&D immediate-action magic mechanic SWLOR does not implement.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.Counterspell, true);

            // Dispel Magic names the D&D spell effect directly ("Dispel Magic : <caster> : <spells>"),
            // which has no Shadowrun-vocabulary equivalent.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.DispelMagic, true);

            // Polymorph is a D&D transformation-spell term with no Shadowrun equivalent; suppressed
            // defensively even though the engine's own comment notes this type is likely unused.
            FeedbackPlugin.SetCombatLogMessageHidden(CombatLogMessageType.Polymorph, true);
        }
    }
}

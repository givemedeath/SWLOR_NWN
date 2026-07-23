using System.Collections.Generic;
using SWLOR.Game.Server.Service.CyberwareService;
using SWLOR.Game.Server.Service.StatService;

namespace SWLOR.Game.Server.Feature.CyberwareDefinition
{
    /// <summary>
    /// The seed cyberware for the first slice - five passive pieces that together spend the full 6.0
    /// Essence budget, so installing everything zeroes a character's Magic and makes the
    /// chrome-versus-magic tradeoff impossible to miss.
    ///
    /// Numbers are a starting proposal to tune in playtest, per the project's working method. All
    /// grants are passive stat bonuses read live through <see cref="Service.Cyberware.GetStatBonus"/>;
    /// attribute-boosting and active cyberware are deferred (decision D16).
    /// </summary>
    public class CoreCyberwareDefinition : ICyberwareListDefinition
    {
        private readonly CyberwareBuilder _builder = new();

        public Dictionary<string, CyberwareDetail> BuildCyberware()
        {
            DermalPlating();
            WiredReflexes();
            MuscleReplacement();
            Cybereyes();
            ReactionEnhancers();

            return _builder.Build();
        }

        private void DermalPlating()
        {
            _builder.Create("dermal_plating")
                .Name("Dermal Plating")
                .ShortName("Dermal Plating")
                .Description("Subdermal armor plating. Increases Defense, shrugging off light hits outright.")
                .EssenceCost(1.0f)
                .Price(5000)
                .IncreasesStat(StatType.Defense, 4);
        }

        private void WiredReflexes()
        {
            _builder.Create("wired_reflexes")
                .Name("Wired Reflexes")
                .ShortName("Wired Reflexes")
                .Description("Reflex-boosting nerve wiring. Sharply improves Evasion and Attack - the street samurai's signature chrome.")
                .EssenceCost(2.0f)
                .Price(15000)
                .IncreasesStat(StatType.Evasion, 6)
                .IncreasesStat(StatType.Attack, 4);
        }

        private void MuscleReplacement()
        {
            _builder.Create("muscle_replacement")
                .Name("Muscle Replacement")
                .ShortName("Muscle Repl.")
                .Description("Synthetic muscle grafts. Increases Attack through raw physical power.")
                .EssenceCost(1.5f)
                .Price(10000)
                .IncreasesStat(StatType.Attack, 6);
        }

        private void Cybereyes()
        {
            _builder.Create("cybereyes")
                .Name("Cybereyes")
                .ShortName("Cybereyes")
                .Description("Replacement optical systems with targeting overlays. Increases Accuracy. Cheap on Essence.")
                .EssenceCost(0.5f)
                .Price(4000)
                .IncreasesStat(StatType.Accuracy, 5);
        }

        private void ReactionEnhancers()
        {
            _builder.Create("reaction_enhancers")
                .Name("Reaction Enhancers")
                .ShortName("React. Enh.")
                .Description("Spinal reaction accelerators. Increases Evasion.")
                .EssenceCost(1.0f)
                .Price(8000)
                .IncreasesStat(StatType.Evasion, 5);
        }
    }
}

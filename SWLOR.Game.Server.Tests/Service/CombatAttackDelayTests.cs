using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.AbilityService;
using SWLOR.NWN.API.NWScript.Enum.Item;

namespace SWLOR.Game.Server.Tests.Service;

public class CombatAttackDelayTests
{
    [Test]
    public void CalculateAttackDelayMilliseconds_UsesSingleWeaponDelay()
    {
        var delay = Combat.CalculateAttackDelayMilliseconds(210, 0, 0, 0);

        delay.Should().Be(3500);
        Combat.CalculateEffectiveAttackDelay(delay).Should().Be(1750);
    }

    [Test]
    public void CalculateAttackDelayMilliseconds_DualWieldCountsDefaultDelayOnce()
    {
        var delay = Combat.CalculateAttackDelayMilliseconds(210, 210, 0, 0);

        delay.Should().Be(5250);
        Combat.CalculateEffectiveAttackDelay(delay).Should().Be(3500);
    }

    [Test]
    public void CalculateAttackDelayMilliseconds_AppliesOffhandReductionBeforeCombiningDualWieldDelay()
    {
        var delay = Combat.CalculateAttackDelayMilliseconds(210, 210, 0, 30);

        delay.Should().Be(4200);
        Combat.CalculateEffectiveAttackDelay(delay).Should().Be(2450);
    }

    [Test]
    public void CalculateEffectiveAttackDelay_SubtractsDefaultDelayFromHigherAttackerDelay()
    {
        var attackerDelay = Combat.BaseAttackDelayMilliseconds + 2500;

        var effectiveDelay = Combat.CalculateEffectiveAttackDelay(attackerDelay);

        effectiveDelay.Should().Be(2500);
    }

    [Test]
    public void CalculateEffectiveAttackDelay_ClampsPostBaselineDelayToDefaultMinimum()
    {
        var attackerDelay = Combat.BaseAttackDelayMilliseconds + 1250;

        var effectiveDelay = Combat.CalculateEffectiveAttackDelay(attackerDelay);

        effectiveDelay.Should().Be(Combat.BaseAttackDelayMilliseconds);
    }

    [Test]
    public void CalculateAttackDelayMilliseconds_FastestWeaponDelayCanBenefitFromHaste()
    {
        var unmodifiedDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 0, 0);
        var hastenOneDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 15, 0);
        var hastenTwoDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 25, 0);

        Combat.CalculateEffectiveAttackDelay(unmodifiedDelay).Should().BeGreaterThan(Combat.BaseAttackDelayMilliseconds);
        Combat.CalculateEffectiveAttackDelay(hastenOneDelay).Should().BeGreaterThan(Combat.BaseAttackDelayMilliseconds);
        Combat.CalculateEffectiveAttackDelay(hastenTwoDelay).Should().BeGreaterThan(Combat.BaseAttackDelayMilliseconds);
        Combat.CalculateEffectiveAttackDelay(hastenTwoDelay).Should().BeLessThan(Combat.CalculateEffectiveAttackDelay(hastenOneDelay));
    }

    [Test]
    public void NaturalWeaponDelay_UsesFastestWeaponDelayAndBenefitsFromHaste()
    {
        var naturalWeaponTypes = new[]
        {
            BaseItem.CreatureSlashWeapon,
            BaseItem.CreaturePierceWeapon,
            BaseItem.CreatureBludgeonWeapon,
            BaseItem.CreatureSlashPierceWeapon
        };

        foreach (var naturalWeaponType in naturalWeaponTypes)
        {
            WeaponDelay.GetWeaponDelay(naturalWeaponType).Should().Be(29);
        }

        var unmodifiedDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 0, 0);
        var hastenOneDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 15, 0);
        var hastenTwoDelay = Combat.CalculateAttackDelayMilliseconds(290, 0, 25, 0);

        Combat.CalculateEffectiveAttackDelay(unmodifiedDelay).Should().Be(3083);
        Combat.CalculateEffectiveAttackDelay(hastenOneDelay).Should().Be(2359);
        Combat.CalculateEffectiveAttackDelay(hastenTwoDelay).Should().Be(1875);
    }

    [Test]
    public void LegacySlingPistolDelay_UsesPistolDelay()
    {
        WeaponDelay.GetWeaponDelay(BaseItem.Sling).Should().Be(37);
    }

    [Test]
    public void CalculateEffectiveAttackDelay_ClampsReducedDualWieldDelayToDefaultMinimum()
    {
        var delay = Combat.CalculateAttackDelayMilliseconds(210, 210, 45, 30);

        Combat.CalculateEffectiveAttackDelay(delay).Should().Be(Combat.BaseAttackDelayMilliseconds);
    }

    [Test]
    public void CalculateEffectiveAttackDelay_UsesDefaultDelayWhenAttackerDelayIsSameOrLower()
    {
        var attackerDelays = new[]
        {
            0,
            Combat.BaseAttackDelayMilliseconds - 1,
            Combat.BaseAttackDelayMilliseconds
        };

        foreach (var attackerDelay in attackerDelays)
        {
            var effectiveDelay = Combat.CalculateEffectiveAttackDelay(attackerDelay);

            effectiveDelay.Should().Be(Combat.BaseAttackDelayMilliseconds);
        }
    }

    [Test]
    public void CalculateEffectiveAttackDelay_UsesDefaultMinimumWhenNoDelayAttackIsQueued()
    {
        var attackerDelay = Combat.BaseAttackDelayMilliseconds + 2000;

        var effectiveDelay = Combat.CalculateEffectiveAttackDelay(attackerDelay, true);

        effectiveDelay.Should().Be(Combat.BaseAttackDelayMilliseconds);
    }

    [Test]
    public void CalculateAutoAttackDelayWindow_UsesDesiredDelayWhenAboveEngineMinimum()
    {
        var window = Combat.CalculateAutoAttackDelayWindow(
            Combat.BaseAttackDelayMilliseconds + 2500,
            false,
            0);

        window.DesiredDelayMilliseconds.Should().Be(2500);
        window.GateDelayMilliseconds.Should().Be(2500);
        window.AdditionalAttacks.Should().Be(0);
        window.OverflowCarry.Should().Be(0);
    }

    [Test]
    public void CalculateAutoAttackDelayWindow_AccumulatesSubMinimumAttackCredit()
    {
        var carry = 0d;
        AutoAttackDelayWindow window = default;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            window = Combat.CalculateAutoAttackDelayWindow(
                Combat.BaseAttackDelayMilliseconds + 1500,
                false,
                carry);

            window.DesiredDelayMilliseconds.Should().Be(1500);
            window.GateDelayMilliseconds.Should().Be(Combat.BaseAttackDelayMilliseconds);
            window.AdditionalAttacks.Should().Be(0);

            carry = window.OverflowCarry;
        }

        window = Combat.CalculateAutoAttackDelayWindow(
            Combat.BaseAttackDelayMilliseconds + 1500,
            false,
            carry);

        window.AdditionalAttacks.Should().Be(1);
        window.OverflowCarry.Should().BeApproximately(0, 0.001);
    }

    [Test]
    public void CalculateAutoAttackDelayWindow_CapsAdditionalAttacksPerVisibleBatch()
    {
        var window = Combat.CalculateAutoAttackDelayWindow(
            Combat.BaseAttackDelayMilliseconds + 500,
            false,
            0);

        window.DesiredDelayMilliseconds.Should().Be(500);
        window.GateDelayMilliseconds.Should().Be(Combat.BaseAttackDelayMilliseconds);
        window.AdditionalAttacks.Should().Be(1);
        window.OverflowCarry.Should().BeGreaterThan(0);
    }

    [Test]
    public void CalculateAutoAttackDelayWindow_NoDelayAttackUsesEngineMinimumWithoutOverflow()
    {
        var window = Combat.CalculateAutoAttackDelayWindow(
            Combat.BaseAttackDelayMilliseconds + 500,
            true,
            0.75f);

        window.DesiredDelayMilliseconds.Should().Be(Combat.BaseAttackDelayMilliseconds);
        window.GateDelayMilliseconds.Should().Be(Combat.BaseAttackDelayMilliseconds);
        window.AdditionalAttacks.Should().Be(0);
        window.OverflowCarry.Should().Be(0);
    }

    [Test]
    public void NativeAttackDelayHook_QueuesVisibleExternalAutoAttacksWithoutGlobalAttackCountHook()
    {
        var root = FindRepositoryRoot();
        var attackObjectHookSource = File.ReadAllText(Path.Combine(
            root.FullName,
            "SWLOR.Game.Server",
            "Native",
            "OnAIActionAttackObject.cs"));

        attackObjectHookSource.Should().NotContain("_ZN15CNWSCombatRound25InitializeNumberOfAttacksEv");
        attackObjectHookSource.Should().NotContain("m_nAdditionalAttacks +=");
        attackObjectHookSource.Should().NotContain("TryConsumeScheduledAttackBatch");
        attackObjectHookSource.Should().NotContain("AutoAttackDesiredDelayOverrideMilliseconds");
        attackObjectHookSource.Should().NotContain("AddAttackActions");
        attackObjectHookSource.Should().Contain("CalculateAutoAttackDelayWindow");
        attackObjectHookSource.Should().Contain("ExternalResolveAttack");
        attackObjectHookSource.Should().Contain("SetPauseTimer(animationTime)");
        attackObjectHookSource.Should().Contain("var canQueueVisibleAutoAttacks = pCombatTargetCreature != null;");
        attackObjectHookSource.Should().Contain("pendingAutoAttack.Target != oidAttackTarget");
        attackObjectHookSource.Should().Contain("TryGetActiveCreatureTarget");
        attackObjectHookSource.Should().Contain("GetAttackActionPending");
        attackObjectHookSource.Should().Contain("var pTargetNwsObject = pGameObject?.AsNWSObject();");
        attackObjectHookSource.Should().Contain("var pTargetCreature = pGameObject?.AsNWSCreature();");
        attackObjectHookSource.Should().Contain("var nAttacks = 1;");
        attackObjectHookSource.Should().NotContain("pPendingAction.m_nNumAttacks");
    }

    [Test]
    public void CanConsumeNextAbilityNoDelay_RequiresHostileAbility()
    {
        Combat.CanConsumeNextAbilityNoDelay(new AbilityDetail
        {
            IsHostileAbility = true
        })
            .Should()
            .BeTrue();

        Combat.CanConsumeNextAbilityNoDelay(new AbilityDetail
        {
            IsHostileAbility = false
        })
            .Should()
            .BeFalse();
    }

    [Test]
    public void WeaponDelayMigration_CoversLivePlayerInventoryAndSerializedItems()
    {
        var root = FindRepositoryRoot();
        var playerMigrationSource = File.ReadAllText(Path.Combine(
            root.FullName,
            "SWLOR.Game.Server",
            "Feature",
            "MigrationDefinition",
            "PlayerMigration",
            "_14_MigrateResistanceItemProperties.cs"));
        var serverMigrationSource = File.ReadAllText(Path.Combine(
            root.FullName,
            "SWLOR.Game.Server",
            "Feature",
            "MigrationDefinition",
            "ServerMigration",
            "_31_MigrateResistanceItemProperties.cs"));
        var weaponDelayMigrationSource = File.ReadAllText(Path.Combine(
            root.FullName,
            "SWLOR.Game.Server",
            "Feature",
            "MigrationDefinition",
            "SerializedItemWeaponDamageTypeMigration.cs"));

        playerMigrationSource.Should().Contain("SerializedItemWeaponDamageTypeMigration.MigrateObject(player);");
        serverMigrationSource.Should().Contain("SerializedItemWeaponDamageTypeMigration.MigrateSerializedObject");
        weaponDelayMigrationSource.Should().Contain("ItemPropertyType.Delay");
        weaponDelayMigrationSource.Should().Contain("WeaponDelay.GetWeaponDelay(baseItem)");
        weaponDelayMigrationSource.Should().Contain("[\"t_knife\"] = 32");
        weaponDelayMigrationSource.Should().Contain("[\"t_shuriken\"] = 32");
        weaponDelayMigrationSource.Should().Contain("GetHasInventory(obj)");
        weaponDelayMigrationSource.Should().Contain("GetItemInSlot((InventorySlot)index, creature)");
    }

    [Test]
    public void ModuleNaturalWeaponDelayProperties_AreNormalized()
    {
        var root = FindRepositoryRoot();
        var moduleRoot = Path.Combine(root.FullName, "Module");
        var files = Directory.EnumerateFiles(Path.Combine(moduleRoot, "uti"), "*.json")
            .Concat(Directory.EnumerateFiles(Path.Combine(moduleRoot, "git"), "*.json"));
        var findings = new List<string>();

        foreach (var file in files)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(file));
            InspectNaturalWeaponDelays(document.RootElement, Path.GetRelativePath(root.FullName, file), string.Empty, findings);
        }

        findings.Should().BeEmpty(string.Join("\n", findings.Take(25)));
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "SWLOR.Game.Server.sln")))
            directory = directory.Parent;

        directory.Should().NotBeNull("the repository root should be discoverable from the test directory");
        return directory!;
    }

    private static readonly HashSet<int> NaturalWeaponBaseItems = new()
    {
        (int)BaseItem.CreatureSlashWeapon,
        (int)BaseItem.CreaturePierceWeapon,
        (int)BaseItem.CreatureBludgeonWeapon,
        (int)BaseItem.CreatureSlashPierceWeapon
    };

    private static void InspectNaturalWeaponDelays(
        JsonElement element,
        string file,
        string path,
        ICollection<string> findings)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                if (TryGetWrappedInt(element, "BaseItem", out var baseItem) &&
                    NaturalWeaponBaseItems.Contains(baseItem) &&
                    TryGetWrappedValue(element, "PropertiesList", out var propertiesList))
                {
                    var delayCosts = GetDelayCostValues(propertiesList).ToList();
                    if (delayCosts.Count == 0)
                    {
                        findings.Add($"{file}:{path} missing natural weapon Delay");
                    }
                    else if (delayCosts.Any(x => x != 29))
                    {
                        findings.Add($"{file}:{path} natural weapon Delay [{string.Join(", ", delayCosts)}] should be 29");
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name == "__struct_id")
                        continue;

                    InspectNaturalWeaponDelays(
                        property.Value,
                        file,
                        string.IsNullOrWhiteSpace(path) ? property.Name : $"{path}.{property.Name}",
                        findings);
                }
                break;
            case JsonValueKind.Array:
                var index = 0;
                foreach (var item in element.EnumerateArray())
                {
                    InspectNaturalWeaponDelays(item, file, $"{path}[{index}]", findings);
                    index++;
                }
                break;
        }
    }

    private static IEnumerable<int> GetDelayCostValues(JsonElement propertiesList)
    {
        if (propertiesList.ValueKind != JsonValueKind.Array)
            yield break;

        foreach (var property in propertiesList.EnumerateArray())
        {
            if (TryGetWrappedInt(property, "PropertyName", out var propertyName) &&
                propertyName == 98 &&
                TryGetWrappedInt(property, "CostTable", out var costTable) &&
                costTable == 52 &&
                TryGetWrappedInt(property, "CostValue", out var costValue))
            {
                yield return costValue;
            }
        }
    }

    private static bool TryGetWrappedValue(JsonElement element, string propertyName, out JsonElement value)
    {
        value = default;
        if (element.ValueKind != JsonValueKind.Object ||
            !element.TryGetProperty(propertyName, out var wrapper) ||
            wrapper.ValueKind != JsonValueKind.Object ||
            !wrapper.TryGetProperty("value", out value))
        {
            return false;
        }

        return true;
    }

    private static bool TryGetWrappedInt(JsonElement element, string propertyName, out int value)
    {
        value = 0;
        return TryGetWrappedValue(element, propertyName, out var wrapperValue) &&
               wrapperValue.ValueKind == JsonValueKind.Number &&
               wrapperValue.TryGetInt32(out value);
    }
}

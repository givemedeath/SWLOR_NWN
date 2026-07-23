using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Service;

namespace SWLOR.Game.Server.Tests.Service;

public class ShadowrunModuleScaffoldingTests
{
    [Test]
    public void EveryMetatypeHasMaleAndFemaleCharacterCreationPortraits()
    {
        var root = FindRepositoryRoot();
        var portraitsPath = Path.Combine(root.FullName, "SWLOR_Haks", "sw_2da", "portraits.2da");
        var portraitsByRaceAndSex = ReadPortraitRaceAndSexPairs(portraitsPath);

        foreach (var metatype in Metatype.Metatypes)
        {
            var race = ((int)metatype).ToString();
            portraitsByRaceAndSex.Should().Contain(
                (race, "0"),
                $"{metatype} needs at least one male portrait or character creation cannot advance");
            portraitsByRaceAndSex.Should().Contain(
                (race, "1"),
                $"{metatype} needs at least one female portrait or character creation cannot advance");
        }
    }

    [Test]
    public void CharacterCreationOffersExactlyTheFiveShadowrunMetatypes()
    {
        var root = FindRepositoryRoot();
        var racialTypesPath = Path.Combine(root.FullName, "SWLOR_Haks", "sw_2da", "racialtypes.2da");
        var playableRaceIds = ReadTwoDaRows(racialTypesPath)
            .Where(columns => columns.Length >= 21 && columns[20] == "1")
            .Select(columns => int.Parse(columns[0]))
            .ToHashSet();

        playableRaceIds.Should().BeEquivalentTo(
            Metatype.Metatypes.Select(metatype => (int)metatype),
            "Erie character creation must not expose legacy Star Wars races");
    }

    [Test]
    public void EveryShadowrunMetatypeCanSelectEveryPlayableCharacterClass()
    {
        var root = FindRepositoryRoot();
        var twoDaRoot = Path.Combine(root.FullName, "SWLOR_Haks", "sw_2da");
        var playableClasses = ReadTwoDaRows(Path.Combine(twoDaRoot, "classes.2da"))
            // classes.2da columns: row, Label, ..., PlayerClass at column 16,
            // PreReqTable at column 49.
            .Where(columns => columns.Length > 49 && columns[16] == "1")
            .Select(columns => (Label: columns[1], PrerequisiteTable: columns[49]))
            .ToList();

        playableClasses.Should().NotBeEmpty();
        foreach (var (label, prerequisiteTable) in playableClasses)
        {
            prerequisiteTable.Should().NotBe("****", $"{label} needs a race prerequisite table");
            var permittedRaceIds = ReadTwoDaRows(
                    Path.Combine(twoDaRoot, $"{prerequisiteTable}.2da"))
                // prerequisite columns: row, LABEL, ReqType, ReqParam1, ReqParam2.
                .Where(columns => columns.Length >= 4 && columns[2] == "RACE")
                .Select(columns => int.Parse(columns[3]))
                .ToHashSet();

            permittedRaceIds.Should().Contain(
                Metatype.Metatypes.Select(metatype => (int)metatype),
                $"{label} must remain selectable for every player-visible metatype");
        }
    }

    [Test]
    public void FreshModuleUsesCleanErieArrivalAndShipsStartingResources()
    {
        var root = FindRepositoryRoot();
        var moduleRoot = Path.Combine(root.FullName, "ModuleSR");
        using var moduleInfo = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "ifo", "module.ifo.json")));

        var entryArea = moduleInfo.RootElement
            .GetProperty("Mod_Entry_Area")
            .GetProperty("value")
            .GetString();
        entryArea.Should().Be("erie_arrival");

        var areaNames = moduleInfo.RootElement
            .GetProperty("Mod_Area_list")
            .GetProperty("value")
            .EnumerateArray()
            .Select(area => area.GetProperty("Area_Name").GetProperty("value").GetString())
            .ToList();
        areaNames.Should().Contain("erie_arrival");
        areaNames.Should().NotContain(new[] { "ooc_area", "czs220_hangar" });

        using var arrivalInstances = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", "erie_arrival.git.json")));
        var waypointTags = arrivalInstances.RootElement
            .GetProperty("WaypointList")
            .GetProperty("value")
            .EnumerateArray()
            .Select(waypoint => waypoint.GetProperty("Tag").GetProperty("value").GetString())
            .ToHashSet();
        waypointTags.Should().Contain("ENTRY_STARTING_WP");
        waypointTags.Should().Contain("DTH_DEFAULT_RESPAWN_POINT");

        foreach (var resref in new[] { "survival_knife", "fresh_bread", "travelers_clothes" })
        {
            File.Exists(Path.Combine(moduleRoot, "uti", $"{resref}.uti.json")).Should().BeTrue(
                $"PlayerInitialization creates {resref} for every new Erie character");
        }

        using var clothes = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "uti", "travelers_clothes.uti.json")));
        var customArmorParts = clothes.RootElement
            .EnumerateObject()
            .Where(property => property.Name.StartsWith("ArmorPart_", StringComparison.Ordinal) ||
                               property.Name.StartsWith("xArmorPart_", StringComparison.Ordinal))
            .Where(property => property.Value.GetProperty("value").GetInt32() > 1)
            .Select(property => property.Name)
            .ToList();
        customArmorParts.Should().BeEmpty(
            "starter clothing must use base-game body parts so Erie does not need the 23 body-part HAKs");
    }

    [Test]
    public void FreshModuleHakManifestIsDeliberateAndCoversEveryCommittedAreaTileset()
    {
        var root = FindRepositoryRoot();
        var moduleRoot = Path.Combine(root.FullName, "ModuleSR");
        using var moduleInfo = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "ifo", "module.ifo.json")));

        var haks = moduleInfo.RootElement
            .GetProperty("Mod_HakList")
            .GetProperty("value")
            .EnumerateArray()
            .Select(hak => hak.GetProperty("Mod_Hak").GetProperty("value").GetString())
            .Where(hak => !string.IsNullOrWhiteSpace(hak))
            .Select(hak => hak!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tilesetHaks = Directory
            .EnumerateFiles(Path.Combine(root.FullName, "SWLOR_Haks"), "*.set", SearchOption.AllDirectories)
            .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new DirectoryInfo(Path.GetDirectoryName(group.First())!).Name,
                StringComparer.OrdinalIgnoreCase);

        haks.Should().HaveCountLessThanOrEqualTo(9, "Erie must not inherit the full legacy HAK stack");
        foreach (var areaPath in Directory.EnumerateFiles(Path.Combine(moduleRoot, "are"), "*.are.json"))
        {
            using var area = JsonDocument.Parse(File.ReadAllText(areaPath));
            var tileset = area.RootElement.GetProperty("Tileset").GetProperty("value").GetString();
            var containingHak = tilesetHaks.GetValueOrDefault(tileset!);

            containingHak.Should().NotBeNull($"the tileset for {Path.GetFileName(areaPath)} must exist");
            haks.Should().Contain(containingHak!, $"{Path.GetFileName(areaPath)} uses tileset {tileset}");
        }
    }

    [Test]
    public void PrivateServiceAreaContainsNoLegacyCreatureInstances()
    {
        var root = FindRepositoryRoot();
        using var instances = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(root.FullName, "ModuleSR", "git", "no_access.git.json")));

        instances.RootElement
            .GetProperty("Creature List")
            .GetProperty("value")
            .GetArrayLength()
            .Should()
            .Be(0);
    }

    private static HashSet<(string Race, string Sex)> ReadPortraitRaceAndSexPairs(string path)
    {
        var rows = ReadTwoDaRows(path).Where(columns => columns.Length >= 4);

        // portraits.2da columns are: row, BaseResRef, Sex, Race, ...
        return rows.Select(columns => (Race: columns[3], Sex: columns[2])).ToHashSet();
    }

    private static IEnumerable<string[]> ReadTwoDaRows(string path)
    {
        return File.ReadLines(path)
            .SkipWhile(line => !line.TrimStart().StartsWith("Label", StringComparison.OrdinalIgnoreCase) &&
                               !line.TrimStart().StartsWith("BaseResRef", StringComparison.OrdinalIgnoreCase))
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "SWLOR.Game.Server.sln")))
            directory = directory.Parent;

        return directory ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}

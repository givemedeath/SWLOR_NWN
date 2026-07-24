using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace SWLOR.Game.Server.Tests.Service;

/// <summary>
/// Structural regressions for the P2b Barrens proof street (barrens_strip), the first committed
/// Erie district exterior. These guard the invariants a live walk-test cannot re-derive cheaply:
/// area registration, stable Barrens resrefs with no procgen/legacy residue, a closed bidirectional
/// transition graph from erie_arrival, the required transition waypoints, HAK/tileset closure for the
/// D20 Modern Exterior tileset, and the deferral of creatures (P2c) and service NPCs (P2d).
/// Visual mood, path traversal, and performance remain the operator's live gate.
/// </summary>
public class BarrensProofStreetTests
{
    private const string StripResref = "barrens_strip";
    private const string StripTileset = "dgt04";          // D20 Modern Exterior (gritty slum), re-themed from a hand-built area
    private const string StripTilesetHak = "sw_t_modernex";

    // Transition anchor tags forming the arrival <-> strip loop.
    private const string ArrivalOutboundTrigger = "arrival_to_strip";
    private const string StripReturnTrigger = "strip_to_arrival";
    private const string StripArrivalWaypoint = "WP_STRIP_FROM_ARRIVAL";
    private const string ArrivalReturnWaypoint = "WP_ARRIVAL_FROM_STRIP";

    [Test]
    public void BarrensStripIsRegisteredAsACommittedModuleArea()
    {
        var moduleRoot = ModuleRoot();
        File.Exists(Path.Combine(moduleRoot, "are", $"{StripResref}.are.json"))
            .Should().BeTrue("the Barrens proof street must be committed as static area geometry");
        File.Exists(Path.Combine(moduleRoot, "git", $"{StripResref}.git.json"))
            .Should().BeTrue("the Barrens proof street must be committed as static instance data");

        AreaList(moduleRoot).Should().Contain(StripResref,
            "barrens_strip must be registered in module.ifo or NWN will not load it");
    }

    [Test]
    public void BarrensStripUsesStableResrefsWithNoProcgenOrLegacyResidue()
    {
        var moduleRoot = ModuleRoot();
        using var area = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "are", $"{StripResref}.are.json")));

        area.RootElement.GetProperty("Tag").GetProperty("value").GetString().Should().Be(StripResref);
        area.RootElement.GetProperty("ResRef").GetProperty("value").GetString().Should().Be(StripResref);

        var name = area.RootElement.GetProperty("Name").GetProperty("value").GetProperty("0").GetString();
        name.Should().Be("The Barrens - The Strip");
        name!.ToLowerInvariant().Should().NotContainAny("procgen", "placeholder", "pga", "seed",
            "star wars", "smuggler", "nar shaddaa");
    }

    [Test]
    public void BarrensStripUsesTheModernExteriorTilesetAndDeclaresItsHak()
    {
        var moduleRoot = ModuleRoot();
        using var area = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "are", $"{StripResref}.are.json")));
        area.RootElement.GetProperty("Tileset").GetProperty("value").GetString().Should().Be(StripTileset);

        HakList(moduleRoot).Should().Contain(StripTilesetHak,
            "barrens_strip renders on the D20 Modern Exterior tileset (dgt04)");
    }

    [Test]
    public void LargeModernExteriorProcgenCandidateIsRegisteredAndDressed()
    {
        var moduleRoot = ModuleRoot();
        const string candidate = "barrens_pgen40";
        AreaList(moduleRoot).Should().Contain(candidate);

        using var area = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "are", $"{candidate}.are.json")));
        area.RootElement.GetProperty("Tileset").GetProperty("value").GetString().Should().Be("dgt04");
        area.RootElement.GetProperty("Width").GetProperty("value").GetInt32().Should().Be(40);
        area.RootElement.GetProperty("Height").GetProperty("value").GetInt32().Should().Be(40);
        area.RootElement.GetProperty("Name").GetProperty("value").GetProperty("0").GetString()
            .Should().Contain("Generated Proof Street");

        using var instances = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{candidate}.git.json")));
        instances.RootElement.GetProperty("Placeable List").GetProperty("value").GetArrayLength()
            .Should().BeGreaterThan(0, "the large procgen candidate must prove environmental dressing emits");
        instances.RootElement.GetProperty("TriggerList").GetProperty("value").GetArrayLength()
            .Should().Be(1, "the comparison area must have a return route");
    }

    [Test]
    public void EveryTransitionTriggerLinksToAWaypointThatExistsInACommittedArea()
    {
        var moduleRoot = ModuleRoot();
        var waypointTags = AllWaypointTags(moduleRoot);

        foreach (var (areaResref, trigger) in AllTransitionTriggers(moduleRoot))
        {
            var target = trigger.LinkedTo;
            target.Should().NotBeNullOrEmpty(
                $"transition trigger '{trigger.Tag}' in {areaResref} must point at a destination waypoint");
            waypointTags.Should().Contain(target,
                $"transition '{trigger.Tag}' in {areaResref} links to '{target}', which no committed area provides — a Bad Strref-class dead transition");
        }
    }

    [Test]
    public void ArrivalAndStripAreConnectedBidirectionally()
    {
        var moduleRoot = ModuleRoot();

        var arrivalWaypoints = WaypointTags(moduleRoot, "erie_arrival");
        var arrivalTriggers = TransitionTriggers(moduleRoot, "erie_arrival");
        var stripWaypoints = WaypointTags(moduleRoot, StripResref);
        var stripTriggers = TransitionTriggers(moduleRoot, StripResref);

        // Arrival keeps its own required waypoints and gains the return anchor.
        arrivalWaypoints.Should().Contain(new[]
        {
            "ENTRY_STARTING_WP", "DTH_DEFAULT_RESPAWN_POINT", ArrivalReturnWaypoint
        });
        stripWaypoints.Should().Contain(StripArrivalWaypoint);

        // Outbound: arrival -> strip.
        var outbound = arrivalTriggers.Should().ContainSingle(t => t.Tag == ArrivalOutboundTrigger).Subject;
        outbound.Type.Should().Be(1, "an NWN area-transition trigger must be Type 1");
        outbound.LinkedTo.Should().Be(StripArrivalWaypoint);

        // Return: strip -> arrival.
        var ret = stripTriggers.Should().ContainSingle(t => t.Tag == StripReturnTrigger).Subject;
        ret.Type.Should().Be(1);
        ret.LinkedTo.Should().Be(ArrivalReturnWaypoint);
    }

    [Test]
    public void TransitionTriggersDoNotOverlapTheirOwnDestinationOrRespawnPoints()
    {
        // A returning or respawning player must never spawn inside a transition trigger, or they
        // re-transition instantly (the loop bug). Every transition trigger keeps clear of the
        // waypoints a player can materialise on in the SAME area.
        var moduleRoot = ModuleRoot();
        const float triggerHalfExtent = 3f; // barrens transitions use a 6m box
        const float clearance = triggerHalfExtent + 2f;

        foreach (var areaResref in new[] { "erie_arrival", StripResref })
        {
            var waypoints = Waypoints(moduleRoot, areaResref);
            var spawnableTags = new[] { "ENTRY_STARTING_WP", "DTH_DEFAULT_RESPAWN_POINT", ArrivalReturnWaypoint, StripArrivalWaypoint };
            var spawnPoints = waypoints.Where(w => spawnableTags.Contains(w.Tag)).ToList();

            foreach (var trigger in TransitionTriggers(moduleRoot, areaResref))
            foreach (var spawn in spawnPoints)
            {
                var distance = MathF.Sqrt(
                    MathF.Pow(trigger.X - spawn.X, 2) + MathF.Pow(trigger.Y - spawn.Y, 2));
                distance.Should().BeGreaterThan(clearance,
                    $"trigger '{trigger.Tag}' at ({trigger.X},{trigger.Y}) sits on spawnable waypoint '{spawn.Tag}' in {areaResref} — a returning/respawning player would loop");
            }
        }
    }

    [Test]
    public void BarrensStripCarriesNoFantasyDoorsAndNoProcgenWaypointResidue()
    {
        var moduleRoot = ModuleRoot();
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{StripResref}.git.json")));

        var doorResrefs = git.RootElement.GetProperty("Door List").GetProperty("value").EnumerateArray()
            .Select(d => d.GetProperty("TemplateResRef").GetProperty("value").GetString())
            .ToList();
        doorResrefs.Should().NotContain("nw_door_fancy",
            "the generated medieval door reads as fantasy residue in a neon-city street");

        WaypointTags(moduleRoot, StripResref).Should().NotContain(tag => tag.StartsWith("PG_", StringComparison.Ordinal),
            "procgen scaffolding waypoints (PG_*) must be renamed to Barrens anchors before committing");
    }

    [Test]
    public void BarrensStripHasNoCreatureOrStoreInstancesYet()
    {
        // Combat creatures are P2c; service/story NPCs and shops are P2d. The committed proof street
        // ships only generic environmental dressing.
        var moduleRoot = ModuleRoot();
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{StripResref}.git.json")));

        git.RootElement.GetProperty("Creature List").GetProperty("value").GetArrayLength()
            .Should().Be(0, "creatures belong to P2c, not the area build");
        git.RootElement.GetProperty("StoreList").GetProperty("value").GetArrayLength()
            .Should().Be(0, "shops belong to P2d, not the area build");
    }

    [Test]
    public void BarrensStripAppliesTheDeliberateUrbanNightAudioMood()
    {
        var moduleRoot = ModuleRoot();
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{StripResref}.git.json")));
        var props = git.RootElement.GetProperty("AreaProperties").GetProperty("value");

        int Prop(string name) => props.GetProperty(name).GetProperty("value").GetInt32();

        // al_pl_citynite ambient + mus_cityslumnite music: the blighted-sprawl night mood, chosen
        // deliberately rather than inherited from the procgen placeholder.
        Prop("AmbientSndNight").Should().Be(14);
        Prop("MusicNight").Should().Be(16);
        Prop("AmbientSndNitVol").Should().BeGreaterThan(0, "an ambient bed with zero volume is silent");
    }

    [Test]
    public void ArrivalHasAVisibleExitMarkerOnTheOutboundTrigger()
    {
        // The outbound transition is an invisible floor trigger; without a landmark the exit is
        // unfindable. A visible elevator placeable sits on it so the player can see the way out.
        var moduleRoot = ModuleRoot();
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", "erie_arrival.git.json")));

        var marker = git.RootElement.GetProperty("Placeable List").GetProperty("value").EnumerateArray()
            .Where(p => p.GetProperty("Tag").GetProperty("value").GetString() == "barrens_elevator")
            .Select(p => (X: p.GetProperty("X").GetProperty("value").GetSingle(),
                          Y: p.GetProperty("Y").GetProperty("value").GetSingle()))
            .ToList();
        marker.Should().ContainSingle("the arrival exit needs exactly one visible landmark");

        var trigger = TransitionTriggers(moduleRoot, "erie_arrival")
            .Single(t => t.Tag == ArrivalOutboundTrigger);
        var distance = MathF.Sqrt(
            MathF.Pow(marker[0].X - trigger.X, 2) + MathF.Pow(marker[0].Y - trigger.Y, 2));
        distance.Should().BeLessThan(5f, "the elevator marker must sit on the outbound trigger it advertises");
    }

    [Test]
    public void EveryCommittedPlaceableBlueprintIsPackagedOrBaseGame()
    {
        // A .git placeable instance references its blueprint by TemplateResRef; a custom blueprint
        // (one that exists in the reference module) must be packaged in ModuleSR/utp or the placeable
        // will not render in-game. Base-game blueprints (absent from the reference module) are provided
        // by the engine. This guards the whole-district dressing, not just the Barrens street.
        var root = FindRepositoryRoot().FullName;
        var moduleRoot = Path.Combine(root, "ModuleSR");
        var referenceUtp = Path.Combine(root, "Module", "utp");
        var packagedUtp = Directory.Exists(Path.Combine(moduleRoot, "utp"))
            ? Directory.EnumerateFiles(Path.Combine(moduleRoot, "utp"), "*.utp.json")
                .Select(p => Path.GetFileName(p)!.Replace(".utp.json", "", StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var missing = new List<string>();
        foreach (var gitPath in Directory.EnumerateFiles(Path.Combine(moduleRoot, "git"), "*.git.json"))
        {
            using var git = JsonDocument.Parse(File.ReadAllText(gitPath));
            foreach (var placeable in git.RootElement.GetProperty("Placeable List").GetProperty("value").EnumerateArray())
            {
                var resref = placeable.GetProperty("TemplateResRef").GetProperty("value").GetString();
                if (string.IsNullOrEmpty(resref)) continue;
                var isCustom = File.Exists(Path.Combine(referenceUtp, $"{resref}.utp.json"));
                if (isCustom && !packagedUtp.Contains(resref))
                    missing.Add($"{Path.GetFileName(gitPath)} -> {resref}");
            }
        }

        missing.Should().BeEmpty(
            "custom placeable blueprints must be packaged in ModuleSR/utp or the placeables render as nothing");
    }

    private sealed record TransitionTrigger(string Tag, int Type, string LinkedTo, float X, float Y);
    private sealed record Waypoint(string Tag, float X, float Y);

    private static string ModuleRoot() => Path.Combine(FindRepositoryRoot().FullName, "ModuleSR");

    private static List<string> AreaList(string moduleRoot)
    {
        using var ifo = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "ifo", "module.ifo.json")));
        return ifo.RootElement.GetProperty("Mod_Area_list").GetProperty("value").EnumerateArray()
            .Select(a => a.GetProperty("Area_Name").GetProperty("value").GetString()!)
            .ToList();
    }

    private static List<string> HakList(string moduleRoot)
    {
        using var ifo = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "ifo", "module.ifo.json")));
        return ifo.RootElement.GetProperty("Mod_HakList").GetProperty("value").EnumerateArray()
            .Select(h => h.GetProperty("Mod_Hak").GetProperty("value").GetString()!)
            .ToList();
    }

    private static List<Waypoint> Waypoints(string moduleRoot, string areaResref)
    {
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{areaResref}.git.json")));
        return git.RootElement.GetProperty("WaypointList").GetProperty("value").EnumerateArray()
            .Select(w => new Waypoint(
                w.GetProperty("Tag").GetProperty("value").GetString()!,
                w.GetProperty("XPosition").GetProperty("value").GetSingle(),
                w.GetProperty("YPosition").GetProperty("value").GetSingle()))
            .ToList();
    }

    private static List<string> WaypointTags(string moduleRoot, string areaResref)
        => Waypoints(moduleRoot, areaResref).Select(w => w.Tag).ToList();

    private static List<TransitionTrigger> TransitionTriggers(string moduleRoot, string areaResref)
    {
        using var git = JsonDocument.Parse(
            File.ReadAllText(Path.Combine(moduleRoot, "git", $"{areaResref}.git.json")));
        return git.RootElement.GetProperty("TriggerList").GetProperty("value").EnumerateArray()
            .Select(t => new TransitionTrigger(
                t.GetProperty("Tag").GetProperty("value").GetString()!,
                t.GetProperty("Type").GetProperty("value").GetInt32(),
                t.TryGetProperty("LinkedTo", out var l) ? l.GetProperty("value").GetString() ?? "" : "",
                t.GetProperty("XPosition").GetProperty("value").GetSingle(),
                t.GetProperty("YPosition").GetProperty("value").GetSingle()))
            .Where(t => t.Type == 1)
            .ToList();
    }

    private static IEnumerable<(string AreaResref, TransitionTrigger Trigger)> AllTransitionTriggers(string moduleRoot)
    {
        foreach (var areaResref in AreaList(moduleRoot))
        {
            var gitPath = Path.Combine(moduleRoot, "git", $"{areaResref}.git.json");
            if (!File.Exists(gitPath)) continue;
            foreach (var trigger in TransitionTriggers(moduleRoot, areaResref))
                yield return (areaResref, trigger);
        }
    }

    private static HashSet<string> AllWaypointTags(string moduleRoot)
    {
        var tags = new HashSet<string>(StringComparer.Ordinal);
        foreach (var areaResref in AreaList(moduleRoot))
        {
            var gitPath = Path.Combine(moduleRoot, "git", $"{areaResref}.git.json");
            if (!File.Exists(gitPath)) continue;
            foreach (var tag in WaypointTags(moduleRoot, areaResref))
                tags.Add(tag);
        }
        return tags;
    }

    private static DirectoryInfo FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory != null && !File.Exists(Path.Combine(directory.FullName, "SWLOR.Game.Server.sln")))
            directory = directory.Parent;

        return directory ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}

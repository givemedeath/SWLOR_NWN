using System;
using FluentAssertions;
using NUnit.Framework;
using SWLOR.Game.Server.Enumeration;

namespace SWLOR.Game.Server.Tests;

public class ApplicationSettingsTests
{
    [TestCase(null, GameProfileType.StarWars)]
    [TestCase("", GameProfileType.StarWars)]
    [TestCase("starwars", GameProfileType.StarWars)]
    [TestCase("shadowrun", GameProfileType.Shadowrun)]
    public void ParseGameProfile_RecognizesSupportedProfiles(string value, GameProfileType expected)
    {
        ApplicationSettings.ParseGameProfile(value).Should().Be(expected);
    }

    [Test]
    public void ParseGameProfile_RejectsUnknownProfile()
    {
        var action = () => ApplicationSettings.ParseGameProfile("mixed");
        action.Should().Throw<InvalidOperationException>();
    }

    [TestCase("Erie", "erie")]
    [TestCase("erie_test-2", "erie_test-2")]
    public void NormalizeDataNamespace_ProducesSafeRedisNamespace(string value, string expected)
    {
        ApplicationSettings.NormalizeDataNamespace(value).Should().Be(expected);
    }

    [TestCase("erie:shared")]
    [TestCase("erie shared")]
    public void NormalizeDataNamespace_RejectsRedisDelimiters(string value)
    {
        var action = () => ApplicationSettings.NormalizeDataNamespace(value);
        action.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void ShadowrunEntityKeysAndIndexesAreNamespaced()
    {
        ApplicationSettings.BuildEntityKeyPrefix("erie", "Player").Should().Be("erie:Player");
        ApplicationSettings.BuildEntityIndexName("erie", "Player").Should().Be("erie_Player");
    }

    [Test]
    public void StartingCreditsMatchTheSelectedWorldProfile()
    {
        ApplicationSettings.GetStartingCredits(GameProfileType.StarWars).Should().Be(200);
        ApplicationSettings.GetStartingCredits(GameProfileType.Shadowrun).Should().Be(20000);
    }
}

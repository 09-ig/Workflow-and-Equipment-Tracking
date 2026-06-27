using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class TroubleshootingServiceTests
{
    [Fact]
    public void FindRule_MatchesSymptomsWithoutCaseSensitivity()
    {
        var rules = new[]
        {
            new TroubleshootingRule
            {
                Symptom = "Vibration",
                PossibleCauses = ["Bearing wear"],
                RecommendedChecks = ["Inspect bearing"]
            }
        };

        var result = new TroubleshootingService().FindRule("vibration", rules);

        Assert.NotNull(result);
        Assert.Equal("Vibration", result.Symptom);
    }

    [Fact]
    public void FindRule_ReturnsNull_WhenNoMatchingSymptom()
    {
        var rules = new[]
        {
            new TroubleshootingRule
            {
                Symptom = "Overheating",
                PossibleCauses = ["Blocked intake"],
                RecommendedChecks = ["Check fan"]
            }
        };

        var result = new TroubleshootingService().FindRule("Unknown Problem", rules);

        Assert.Null(result);
    }

    [Fact]
    public void FindRule_FindsCorrectRule_WhenMultipleRulesExist()
    {
        var rules = new[]
        {
            new TroubleshootingRule
            {
                Symptom = "Overheating",
                PossibleCauses = ["Blocked intake"],
                RecommendedChecks = ["Check fan"]
            },
            new TroubleshootingRule
            {
                Symptom = "Vibration",
                PossibleCauses = ["Bearing wear"],
                RecommendedChecks = ["Inspect bearing"]
            }
        };

        var result = new TroubleshootingService().FindRule("Overheating", rules);

        Assert.NotNull(result);
        Assert.Equal("Overheating", result.Symptom);
        Assert.Single(result.PossibleCauses);
        Assert.Equal("Blocked intake", result.PossibleCauses[0]);
    }

    [Fact]
    public void FindRule_ReturnsNull_WhenRuleListIsEmpty()
    {
        var result = new TroubleshootingService().FindRule("Vibration", []);

        Assert.Null(result);
    }

    [Fact]
    public void ParseMultilineList_TrimsBulletsAndRemovesBlankAndDuplicateRows()
    {
        var result = new TroubleshootingService().ParseMultilineList(
            """
            - Bearing wear

            * Bearing wear
            Loose mount
            """);

        Assert.Equal(2, result.Count);
        Assert.Equal("Bearing wear", result[0]);
        Assert.Equal("Loose mount", result[1]);
    }
}

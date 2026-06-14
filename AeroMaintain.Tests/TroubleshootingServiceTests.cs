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
}

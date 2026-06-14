using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain.Tests;

public class HealthScoreServiceTests
{
    private readonly HealthScoreService _service = new();

    [Fact]
    public void Calculate_ReturnsPerfectScore_ForHealthyEquipmentWithNoIssues()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-10),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy,
            RecentIssueCount = 0
        };

        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(100, result.Score);
        Assert.Equal("Stable", result.Label);
    }

    [Fact]
    public void Calculate_AppliesOverdueStatusAndIssuePenalties()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-40),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Critical,
            RecentIssueCount = 3
        };

        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(30, result.Score);
        Assert.Equal("Immediate Check Required", result.Label);
    }

    [Fact]
    public void CalculateAverageScore_ReturnsZero_ForEmptyFleet()
    {
        var result = _service.CalculateAverageScore([], new DateTime(2026, 6, 14));

        Assert.Equal(0, result);
    }
}

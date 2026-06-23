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

    [Fact]
    public void Calculate_ReturnsNeedsAttentionLabel_ForWatchStatusWithIssues()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-10),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Watch,
            RecentIssueCount = 2
        };

        // score = 100 - 0 (not overdue) - 15 (Watch) - 10 (2 issues) = 75
        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(75, result.Score);
        Assert.Equal("Needs Attention", result.Label);
    }

    [Fact]
    public void Calculate_CapsOverduePenalty_AtFortyFive()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-125),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy,
            RecentIssueCount = 0
        };

        // nextDue = refDate-95, overdueDays=95, penalty = min(45, 190) = 45
        // score = 100 - 45 - 0 - 0 = 55
        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(55, result.Score);
        Assert.Equal("Needs Attention", result.Label);
    }

    [Fact]
    public void Calculate_CapsIssuePenalty_AtTwentyFive()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-5),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Healthy,
            RecentIssueCount = 10
        };

        // issuePenalty = min(25, 50) = 25; score = 100 - 0 - 0 - 25 = 75
        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(75, result.Score);
        Assert.Equal("Needs Attention", result.Label);
    }

    [Fact]
    public void CalculateAverageScore_ReturnsCorrectAverage_ForMixedFleet()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var fleet = new[]
        {
            new Equipment
            {
                LastMaintenanceDate = referenceDate.AddDays(-5),
                MaintenanceIntervalDays = 30,
                Status = EquipmentStatus.Healthy,
                RecentIssueCount = 0
            },
            new Equipment
            {
                LastMaintenanceDate = referenceDate.AddDays(-5),
                MaintenanceIntervalDays = 30,
                Status = EquipmentStatus.Critical,
                RecentIssueCount = 0
            }
        };

        // Equipment A: score=100, Equipment B: score=65 → average=82.5
        var result = _service.CalculateAverageScore(fleet, referenceDate);

        Assert.Equal(82.5, result, 1);
    }

    [Fact]
    public void Calculate_AssignsImmediateCheckLabel_WhenScoreDropsBelowFiftyFive()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-40),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Critical,
            RecentIssueCount = 2
        };

        // overdueDays=10, overduePenalty=20; statusPenalty=35; issuePenalty=10
        // score = 100 - 20 - 35 - 10 = 35
        var result = _service.Calculate(equipment, referenceDate);

        Assert.Equal(35, result.Score);
        Assert.Equal("Immediate Check Required", result.Label);
    }

    [Fact]
    public void Calculate_SummaryDescribesImmediateEscalation_ForLowScore()
    {
        var referenceDate = new DateTime(2026, 6, 14);
        var equipment = new Equipment
        {
            LastMaintenanceDate = referenceDate.AddDays(-60),
            MaintenanceIntervalDays = 30,
            Status = EquipmentStatus.Critical,
            RecentIssueCount = 5
        };

        var result = _service.Calculate(equipment, referenceDate);

        Assert.Contains("Escalate", result.Summary, StringComparison.OrdinalIgnoreCase);
    }
}

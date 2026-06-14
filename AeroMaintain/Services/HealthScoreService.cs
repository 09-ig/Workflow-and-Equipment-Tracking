using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class HealthScoreService
{
    public HealthAssessment Calculate(Equipment equipment, DateTime referenceDate)
    {
        var overdueDays = Math.Max(0, (referenceDate.Date - equipment.NextDueDate.Date).Days);
        var overduePenalty = Math.Min(45, overdueDays * 2);

        var statusPenalty = equipment.Status switch
        {
            EquipmentStatus.Healthy => 0,
            EquipmentStatus.Watch => 15,
            EquipmentStatus.Critical => 35,
            _ => 0
        };

        var issuePenalty = Math.Min(25, equipment.RecentIssueCount * 5);
        var score = Math.Clamp(100 - overduePenalty - statusPenalty - issuePenalty, 0, 100);

        return new HealthAssessment
        {
            Score = score,
            Label = GetLabel(score),
            Summary = BuildSummary(score, overdueDays, equipment.RecentIssueCount)
        };
    }

    public double CalculateAverageScore(IEnumerable<Equipment> equipment, DateTime referenceDate)
    {
        var list = equipment.ToList();
        if (list.Count == 0)
        {
            return 0;
        }

        return list.Average(e => Calculate(e, referenceDate).Score);
    }

    private static string GetLabel(int score)
    {
        if (score >= 80)
        {
            return "Stable";
        }

        if (score >= 55)
        {
            return "Needs Attention";
        }

        return "Immediate Check Required";
    }

    private static string BuildSummary(int score, int overdueDays, int recentIssues)
    {
        if (score >= 80)
        {
            return "Maintenance trend is healthy. Keep routine checks on schedule.";
        }

        if (score >= 55)
        {
            return $"Watchlist item: {overdueDays} overdue days and {recentIssues} recent issues logged.";
        }

        return $"Escalate for inspection: {overdueDays} overdue days and repeated issues detected.";
    }
}

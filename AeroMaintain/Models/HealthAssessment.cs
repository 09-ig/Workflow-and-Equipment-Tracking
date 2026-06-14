namespace AeroMaintain.Models;

public class HealthAssessment
{
    public int Score { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}

namespace AeroMaintain.Models;

public class TroubleshootingRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Symptom { get; set; } = string.Empty;
    public List<string> PossibleCauses { get; set; } = new();
    public List<string> RecommendedChecks { get; set; } = new();
}

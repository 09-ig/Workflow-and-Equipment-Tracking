using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class TroubleshootingService
{
    public TroubleshootingRule? FindRule(string symptom, IEnumerable<TroubleshootingRule> rules)
    {
        return rules.FirstOrDefault(r =>
            string.Equals(r.Symptom, symptom, StringComparison.OrdinalIgnoreCase));
    }
}

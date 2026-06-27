using AeroMaintain.Models;

namespace AeroMaintain.Services;

public class TroubleshootingService
{
    public TroubleshootingRule? FindRule(string symptom, IEnumerable<TroubleshootingRule> rules)
    {
        return rules.FirstOrDefault(r =>
            string.Equals(r.Symptom, symptom, StringComparison.OrdinalIgnoreCase));
    }

    public List<string> ParseMultilineList(string value)
    {
        return value
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
            .Select(line => line.Trim().TrimStart('-', '*', '\u2022').Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

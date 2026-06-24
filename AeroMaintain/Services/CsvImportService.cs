using System.IO;
using System.Text;
using AeroMaintain.Models;

namespace AeroMaintain.Services;

public record CsvImportResult(List<Equipment> Imported, int SkippedRows, List<string> Errors);

public class CsvImportService
{
    // Column name candidates, checked case-insensitively in order of preference
    private static readonly string[] NameCols     = ["Name", "Equipment Name", "Asset Name", "Description", "Asset Description", "Equipment Description"];
    private static readonly string[] SerialCols   = ["Serial Number", "Serial", "Asset ID", "Tag Number", "Tag", "Asset Number", "Equipment ID"];
    private static readonly string[] CategoryCols = ["Category", "Equipment Type", "Asset Type", "Type", "Class"];
    private static readonly string[] LastMaintCols = ["Last Maintenance Date", "Last Maintenance", "Last Service Date", "Last Service", "Last PM Date", "Last PM", "Service Date"];
    private static readonly string[] IntervalCols = ["Maintenance Interval", "Interval (Days)", "PM Interval", "Interval", "Frequency (Days)", "Frequency"];
    private static readonly string[] StatusCols   = ["Status", "Condition", "Asset Condition", "Equipment Condition"];
    private static readonly string[] IssueCols    = ["Recent Issues", "Issue Count", "Issues", "Fault Count", "Defect Count"];
    private static readonly string[] NotesCols    = ["Notes", "Comments", "Remarks", "Details"];

    public CsvImportResult Import(string filePath)
    {
        var errors = new List<string>();
        var imported = new List<Equipment>();
        int skipped = 0;

        string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);
        if (lines.Length < 2)
            return new CsvImportResult(imported, skipped, ["File has no data rows."]);

        var headers = ParseLine(lines[0])
            .Select((h, i) => (Name: h.Trim(), Index: i))
            .ToList();

        int? nameIdx     = FindCol(headers, NameCols);
        int? serialIdx   = FindCol(headers, SerialCols);
        int? categoryIdx = FindCol(headers, CategoryCols);
        int? lastMaintIdx = FindCol(headers, LastMaintCols);
        int? intervalIdx = FindCol(headers, IntervalCols);
        int? statusIdx   = FindCol(headers, StatusCols);
        int? issueIdx    = FindCol(headers, IssueCols);
        int? notesIdx    = FindCol(headers, NotesCols);

        if (nameIdx is null)
            return new CsvImportResult(imported, skipped,
                [$"No recognisable name column found. Expected one of: {string.Join(", ", NameCols)}"]);

        for (int row = 1; row < lines.Length; row++)
        {
            if (string.IsNullOrWhiteSpace(lines[row])) continue;

            var cols = ParseLine(lines[row]);

            try
            {
                var name = Col(cols, nameIdx).Trim();
                if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                var serial = Col(cols, serialIdx).Trim();
                if (string.IsNullOrWhiteSpace(serial))
                    serial = $"IMP-{Guid.NewGuid().ToString()[..8].ToUpper()}";

                var category = Col(cols, categoryIdx).Trim();
                var notes    = Col(cols, notesIdx).Trim();

                DateTime lastMaint = DateTime.Today;
                if (lastMaintIdx is not null)
                    DateTime.TryParse(Col(cols, lastMaintIdx), out lastMaint);

                int interval = 30;
                if (intervalIdx is not null && int.TryParse(Col(cols, intervalIdx), out int parsed) && parsed > 0)
                    interval = Math.Min(parsed, 3650);

                var status = ParseStatus(Col(cols, statusIdx));

                int issueCount = 0;
                if (issueIdx is not null)
                    int.TryParse(Col(cols, issueIdx), out issueCount);

                imported.Add(new Equipment
                {
                    Name = name,
                    SerialNumber = serial,
                    Category = category,
                    LastMaintenanceDate = lastMaint,
                    MaintenanceIntervalDays = interval,
                    Status = status,
                    RecentIssueCount = Math.Clamp(issueCount, 0, 10),
                    Notes = notes
                });
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row + 1}: {ex.Message}");
            }
        }

        return new CsvImportResult(imported, skipped, errors);
    }

    private static EquipmentStatus ParseStatus(string raw) =>
        raw.ToLowerInvariant() switch
        {
            "watch" or "monitor" or "warning" or "fair" or "degraded" or "caution" => EquipmentStatus.Watch,
            "critical" or "down" or "failed" or "poor" or "out of service" or "fault" => EquipmentStatus.Critical,
            _ => EquipmentStatus.Healthy
        };

    private static int? FindCol(IEnumerable<(string Name, int Index)> headers, string[] candidates)
    {
        foreach (var candidate in candidates)
            foreach (var (name, index) in headers)
                if (string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase))
                    return index;
        return null;
    }

    private static string Col(List<string> cols, int? index) =>
        index is null || index >= cols.Count ? string.Empty : cols[index.Value];

    // RFC 4180-compliant CSV line parser (handles quoted fields with embedded commas/newlines)
    private static List<string> ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        fields.Add(current.ToString());
        return fields;
    }
}

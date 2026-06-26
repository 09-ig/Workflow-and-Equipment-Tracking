using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AeroMaintain.Models;
using AeroMaintain.Services;

namespace AeroMaintain;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly DataService _dataService = new();
    private readonly MaintenanceService _maintenanceService = new();
    private readonly HealthScoreService _healthScoreService = new();
    private readonly TroubleshootingService _troubleshootingService = new();
    private readonly ExportService _exportService = new();
    private readonly CsvImportService _csvImportService = new();
    private readonly NotificationService _notificationService = new();
    private readonly DesktopNotificationService _desktopNotificationService = new();

    private readonly List<Equipment> _equipmentStore = new();
    private readonly List<TroubleshootingRule> _rules = new();
    private readonly List<MaintenanceLog> _maintenanceLogStore = new();
    private readonly List<AuditLog> _auditLogStore = new();
    private List<MaintenanceTask> _allTasks = new();
    private Guid? _editingEquipmentId;

    private int _healthyCount;
    private int _watchCount;
    private int _criticalCount;
    private int _overdueCount;
    private int _upcomingCount;
    private int _progressMax = 1;
    private string _averageHealthScoreText = "0.0";
    private string _atRiskNote = "No immediate priority items.";
    private string _registryNote = "Add your first equipment entry to start the workflow.";
    private string _fleetEmptyState = "No equipment registered yet.";
    private string _schedulerNote = "No maintenance tasks available.";
    private string _selectedSymptomTitle = "Guided Checks";
    private string _lastExportMessage = "Exports will be saved under the local Exports folder.";
    private string _reportPreview = "No report generated yet.";
    private string _historyNote = "Select equipment to focus its maintenance history.";
    private string _auditNote = "Audit events will appear after equipment changes.";
    private string _alertSummary = "No active operational notifications.";
    private string _alertNote = "Critical and overdue equipment will appear here.";
    private string _selectedStatusFilter = "All";
    private string _selectedDueFilter = "All";
    private string _searchText = string.Empty;
    private bool _startupNotificationShown;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<MaintenanceTask> FilteredTasks { get; } = new();
    public ObservableCollection<MaintenanceTask> AtRiskTasks { get; } = new();
    public ObservableCollection<string> SymptomOptions { get; } = new();
    public ObservableCollection<string> PossibleCauses { get; } = new();
    public ObservableCollection<string> RecommendedChecks { get; } = new();
    public ObservableCollection<MaintenanceLog> MaintenanceHistory { get; } = new();
    public ObservableCollection<AuditLog> AuditTrail { get; } = new();
    public ObservableCollection<MaintenanceAlert> ActiveAlerts { get; } = new();

    public List<string> StatusFilterOptions { get; } = new() { "All", "Healthy", "Watch", "Critical" };
    public List<string> DueFilterOptions { get; } = new() { "All", "Overdue", "Due within 7 days", "Due within 30 days" };

    public Equipment? SelectedEquipment { get; set; }

    public string TodayLabel => DateTime.Today.ToString("dd MMM yyyy");

    public int HealthyCount
    {
        get => _healthyCount;
        set => SetProperty(ref _healthyCount, value);
    }

    public int WatchCount
    {
        get => _watchCount;
        set => SetProperty(ref _watchCount, value);
    }

    public int CriticalCount
    {
        get => _criticalCount;
        set => SetProperty(ref _criticalCount, value);
    }

    public int OverdueCount
    {
        get => _overdueCount;
        set => SetProperty(ref _overdueCount, value);
    }

    public int UpcomingCount
    {
        get => _upcomingCount;
        set => SetProperty(ref _upcomingCount, value);
    }

    public int ProgressMax
    {
        get => _progressMax;
        set => SetProperty(ref _progressMax, value);
    }

    public string AverageHealthScoreText
    {
        get => _averageHealthScoreText;
        set => SetProperty(ref _averageHealthScoreText, value);
    }

    public string AtRiskNote
    {
        get => _atRiskNote;
        set => SetProperty(ref _atRiskNote, value);
    }

    public string RegistryNote
    {
        get => _registryNote;
        set => SetProperty(ref _registryNote, value);
    }

    public string FleetEmptyState
    {
        get => _fleetEmptyState;
        set => SetProperty(ref _fleetEmptyState, value);
    }

    public string SchedulerNote
    {
        get => _schedulerNote;
        set => SetProperty(ref _schedulerNote, value);
    }

    public string SelectedSymptomTitle
    {
        get => _selectedSymptomTitle;
        set => SetProperty(ref _selectedSymptomTitle, value);
    }

    public string LastExportMessage
    {
        get => _lastExportMessage;
        set => SetProperty(ref _lastExportMessage, value);
    }

    public string ReportPreview
    {
        get => _reportPreview;
        set => SetProperty(ref _reportPreview, value);
    }

    public string HistoryNote
    {
        get => _historyNote;
        set => SetProperty(ref _historyNote, value);
    }

    public string AuditNote
    {
        get => _auditNote;
        set => SetProperty(ref _auditNote, value);
    }

    public string AlertSummary
    {
        get => _alertSummary;
        set => SetProperty(ref _alertSummary, value);
    }

    public string AlertNote
    {
        get => _alertNote;
        set => SetProperty(ref _alertNote, value);
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
    }

    public string SelectedDueFilter
    {
        get => _selectedDueFilter;
        set => SetProperty(ref _selectedDueFilter, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (string.Equals(_searchText, value, StringComparison.Ordinal)) return;
            _searchText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchText)));
            RefreshEquipmentGrid();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        LastMaintenanceDatePicker.SelectedDate = DateTime.Today;
        IntervalTextBox.Text = "30";
        IssueCountTextBox.Text = "0";
        MaintenancePerformedByTextBox.Text = Environment.UserName;
        MaintenanceSummaryTextBox.Text = "Routine maintenance completed.";
        MaintenancePartsTextBox.Text = "None";
        MaintenanceLaborHoursTextBox.Text = "0";
        MaintenanceCostTextBox.Text = "0";
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Could not load saved data.\n\n{ex.Message}\n\nThe app will start with an empty registry.",
                "Load Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        RefreshAllViews();
        LoadSymptomOptions();
        if (SymptomOptions.Count > 0)
        {
            SymptomComboBox.SelectedIndex = 0;
        }

        ShowStartupDesktopNotificationIfNeeded();
    }

    private async Task LoadDataAsync()
    {
        var loadedEquipment = await _dataService.LoadEquipmentAsync();
        _equipmentStore.Clear();
        _equipmentStore.AddRange(loadedEquipment);

        var loadedRules = await _dataService.LoadTroubleshootingRulesAsync();
        _rules.Clear();
        _rules.AddRange(loadedRules);

        var maintenanceLogs = await _dataService.LoadMaintenanceLogsAsync(take: 500);
        _maintenanceLogStore.Clear();
        _maintenanceLogStore.AddRange(maintenanceLogs);

        var auditLogs = await _dataService.LoadAuditLogsAsync(500);
        _auditLogStore.Clear();
        _auditLogStore.AddRange(auditLogs);
    }

    private void RefreshAllViews()
    {
        RecalculateFleetHealth();
        RefreshEquipmentGrid();
        RefreshScheduler();
        RefreshHistoryViews();
    }

    private void RecalculateFleetHealth()
    {
        foreach (var equipment in _equipmentStore)
        {
            var assessment = _healthScoreService.Calculate(equipment, DateTime.Today);
            equipment.HealthScore = assessment.Score;
            equipment.HealthLabel = assessment.Label;
        }

        HealthyCount = _equipmentStore.Count(e => e.Status == EquipmentStatus.Healthy);
        WatchCount = _equipmentStore.Count(e => e.Status == EquipmentStatus.Watch);
        CriticalCount = _equipmentStore.Count(e => e.Status == EquipmentStatus.Critical);
        AverageHealthScoreText = _healthScoreService.CalculateAverageScore(_equipmentStore, DateTime.Today).ToString("F1");
        ProgressMax = Math.Max(1, _equipmentStore.Count);
    }

    private void RefreshEquipmentGrid()
    {
        EquipmentItems.Clear();

        var filtered = string.IsNullOrWhiteSpace(_searchText)
            ? _equipmentStore
            : _equipmentStore.Where(e =>
                e.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                e.SerialNumber.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                e.Category.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

        foreach (var equipment in filtered.OrderBy(e => e.NextDueDate).ThenBy(e => e.Name))
        {
            EquipmentItems.Add(equipment);
        }

        FleetEmptyState = _equipmentStore.Count == 0
            ? "Empty state: no equipment in registry. Add your first unit to activate scheduler and analytics."
            : $"{EquipmentItems.Count} of {_equipmentStore.Count} items shown. Select a row to edit.";
    }

    private void RefreshScheduler()
    {
        _allTasks = _maintenanceService.BuildTasks(_equipmentStore, DateTime.Today);
        OverdueCount = _allTasks.Count(t => t.IsOverdue);
        UpcomingCount = _allTasks.Count(t => !t.IsOverdue && t.DaysRemaining <= 30);

        ApplySchedulerFilters();
        RefreshAtRiskTasks();
        RefreshOperationalAlerts();
    }

    private void RefreshAtRiskTasks()
    {
        AtRiskTasks.Clear();
        var items = _allTasks
            .Where(t => t.IsOverdue || t.EquipmentStatus == EquipmentStatus.Critical)
            .OrderBy(t => t.DaysRemaining)
            .ThenBy(t => t.EquipmentName)
            .Take(8);

        foreach (var task in items)
        {
            AtRiskTasks.Add(task);
        }

        AtRiskNote = AtRiskTasks.Count == 0
            ? "No urgent items. Current fleet condition is stable."
            : $"{AtRiskTasks.Count} priority items listed. Review due dates and assign checks.";
    }

    private void RefreshOperationalAlerts()
    {
        ActiveAlerts.Clear();

        var alerts = _notificationService.BuildAlerts(_allTasks);
        foreach (var alert in alerts.Take(12))
        {
            ActiveAlerts.Add(alert);
        }

        AlertSummary = _notificationService.BuildSummary(alerts);
        AlertNote = ActiveAlerts.Count == 0
            ? "No active notifications. Fleet condition is clear for this horizon."
            : $"{ActiveAlerts.Count} active notifications shown. Critical items are ordered first.";
    }

    private void ShowStartupDesktopNotificationIfNeeded()
    {
        if (_startupNotificationShown || ActiveAlerts.Count == 0)
        {
            return;
        }

        _startupNotificationShown = true;

        if (ActiveAlerts.Any(alert => alert.IsCritical))
        {
            _desktopNotificationService.ShowOperationalAlertSummary(ActiveAlerts, AlertSummary);
        }
    }

    private async Task RefreshProductionRecordsAsync()
    {
        var maintenanceLogs = await _dataService.LoadMaintenanceLogsAsync(take: 500);
        _maintenanceLogStore.Clear();
        _maintenanceLogStore.AddRange(maintenanceLogs);

        var auditLogs = await _dataService.LoadAuditLogsAsync(500);
        _auditLogStore.Clear();
        _auditLogStore.AddRange(auditLogs);

        RefreshHistoryViews();
    }

    private void RefreshHistoryViews()
    {
        MaintenanceHistory.Clear();

        var selectedId = SelectedEquipment?.Id;
        var logs = selectedId.HasValue
            ? _maintenanceLogStore.Where(l => l.EquipmentId == selectedId.Value)
            : _maintenanceLogStore;

        foreach (var log in logs
                     .OrderByDescending(l => l.CompletedOn)
                     .ThenByDescending(l => l.LoggedAtUtc))
        {
            MaintenanceHistory.Add(log);
        }

        HistoryNote = selectedId.HasValue && SelectedEquipment is not null
            ? $"{MaintenanceHistory.Count} maintenance log entries for {SelectedEquipment.Name}."
            : $"{MaintenanceHistory.Count} recent maintenance log entries shown across the fleet.";

        AuditTrail.Clear();
        foreach (var audit in _auditLogStore
                     .OrderByDescending(a => a.ChangedAtUtc)
                     .Take(200))
        {
            AuditTrail.Add(audit);
        }

        AuditNote = AuditTrail.Count == 0
            ? "No audit events recorded yet."
            : $"{AuditTrail.Count} recent audit events shown.";
    }

    private void ApplySchedulerFilters()
    {
        var filtered = _maintenanceService.ApplyFilters(
            _allTasks,
            SelectedStatusFilter,
            SelectedDueFilter,
            DateTime.Today);

        FilteredTasks.Clear();
        foreach (var task in filtered)
        {
            FilteredTasks.Add(task);
        }

        SchedulerNote = FilteredTasks.Count == 0
            ? "No tasks match this filter."
            : $"{FilteredTasks.Count} tasks shown. Overdue rows are highlighted for quick action.";
    }

    private void LoadSymptomOptions()
    {
        SymptomOptions.Clear();
        foreach (var rule in _rules.OrderBy(r => r.Symptom))
        {
            SymptomOptions.Add(rule.Symptom);
        }
    }

    private async void SaveEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildEquipmentFromForm(out var builtEquipment))
        {
            return;
        }

        if (_editingEquipmentId.HasValue)
        {
            var existing = _equipmentStore.FirstOrDefault(e => e.Id == _editingEquipmentId.Value);
            if (existing is null)
            {
                RegistryNote = "Selected item no longer exists. Please try again.";
                return;
            }

            existing.Name = builtEquipment.Name;
            existing.SerialNumber = builtEquipment.SerialNumber;
            existing.Category = builtEquipment.Category;
            existing.LastMaintenanceDate = builtEquipment.LastMaintenanceDate;
            existing.MaintenanceIntervalDays = builtEquipment.MaintenanceIntervalDays;
            existing.Status = builtEquipment.Status;
            existing.RecentIssueCount = builtEquipment.RecentIssueCount;
            existing.Notes = builtEquipment.Notes;
            RegistryNote = $"Updated {existing.Name}.";
        }
        else
        {
            _equipmentStore.Add(builtEquipment);
            RegistryNote = $"Added {builtEquipment.Name}.";
        }

        await PersistEquipmentAsync();
        RefreshAllViews();
        ClearForm();
    }

    private async void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEquipment is null)
        {
            RegistryNote = "Select a row first, then delete.";
            return;
        }

        var removed = _equipmentStore.RemoveAll(e => e.Id == SelectedEquipment.Id);
        if (removed > 0)
        {
            RegistryNote = $"Removed {SelectedEquipment.Name}.";
            await PersistEquipmentAsync();
            RefreshAllViews();
            ClearForm();
        }
        else
        {
            RegistryNote = "Could not remove the selected item.";
        }
    }

    private async void PersistButton_Click(object sender, RoutedEventArgs e)
    {
        await PersistEquipmentAsync();
        RegistryNote = "Equipment registry saved to the local database.";
    }

    private void ClearFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearForm();
        RegistryNote = "Form cleared. Ready for a new entry.";
    }

    private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
    {
        SearchText = string.Empty;
    }

    private async void MarkMaintainedButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEquipment is null)
        {
            RegistryNote = "Select a row first, then log maintenance.";
            return;
        }

        var existing = _equipmentStore.FirstOrDefault(eq => eq.Id == SelectedEquipment.Id);
        if (existing is null)
        {
            RegistryNote = "Selected item no longer exists.";
            return;
        }

        if (!TryBuildMaintenanceLogDetails(
                out var performedBy,
                out var summary,
                out var parts,
                out var cost,
                out var laborHours,
                out var notes))
        {
            return;
        }

        try
        {
            var log = await _dataService.RecordMaintenanceAsync(
                existing.Id,
                performedBy,
                summary,
                parts,
                cost,
                laborHours,
                DateTime.Today,
                notes,
                Environment.UserName);

            existing.LastMaintenanceDate = log.CompletedOn;
            RegistryNote = $"Logged maintenance for {existing.Name} on {log.CompletedOn:yyyy-MM-dd}.";
            await RefreshProductionRecordsAsync();
            RefreshAllViews();
            ClearForm();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to log maintenance.\n\n{ex.Message}",
                "Maintenance Log Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ClearForm()
    {
        _editingEquipmentId = null;
        NameTextBox.Text = string.Empty;
        SerialTextBox.Text = string.Empty;
        if (CategoryComboBox.Items.Count > 0)
        {
            CategoryComboBox.SelectedIndex = 0;
        }
        LastMaintenanceDatePicker.SelectedDate = DateTime.Today;
        IntervalTextBox.Text = "30";
        if (StatusComboBox.Items.Count > 0)
        {
            StatusComboBox.SelectedIndex = 0;
        }
        IssueCountTextBox.Text = "0";
        NotesTextBox.Text = string.Empty;
        MaintenancePerformedByTextBox.Text = Environment.UserName;
        MaintenanceSummaryTextBox.Text = "Routine maintenance completed.";
        MaintenancePartsTextBox.Text = "None";
        MaintenanceLaborHoursTextBox.Text = "0";
        MaintenanceCostTextBox.Text = "0";
        MaintenanceNotesTextBox.Text = string.Empty;
        EquipmentDataGrid.SelectedItem = null;
        SelectedEquipment = null;
        RefreshHistoryViews();
    }

    private void EquipmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EquipmentDataGrid.SelectedItem is not Equipment selected)
        {
            return;
        }

        SelectedEquipment = selected;
        _editingEquipmentId = selected.Id;
        NameTextBox.Text = selected.Name;
        SerialTextBox.Text = selected.SerialNumber;
        SetComboBoxSelection(CategoryComboBox, selected.Category);
        LastMaintenanceDatePicker.SelectedDate = selected.LastMaintenanceDate;
        IntervalTextBox.Text = selected.MaintenanceIntervalDays.ToString();
        SetComboBoxSelection(StatusComboBox, selected.Status.ToString());
        IssueCountTextBox.Text = selected.RecentIssueCount.ToString();
        NotesTextBox.Text = selected.Notes;
        RegistryNote = $"Editing {selected.Name}. Click Save Equipment to apply updates.";
        RefreshHistoryViews();
    }

    private void SchedulerFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ApplySchedulerFilters();
    }

    private void SymptomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var symptom = SymptomComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(symptom))
        {
            return;
        }

        var rule = _troubleshootingService.FindRule(symptom, _rules);
        PossibleCauses.Clear();
        RecommendedChecks.Clear();

        SelectedSymptomTitle = $"Guided Checks: {symptom}";

        if (rule is null)
        {
            PossibleCauses.Add("No local rule found for this symptom.");
            RecommendedChecks.Add("Check sensor logs and verify maintenance history manually.");
            return;
        }

        foreach (var cause in rule.PossibleCauses)
        {
            PossibleCauses.Add("• " + cause);
        }

        foreach (var check in rule.RecommendedChecks)
        {
            RecommendedChecks.Add("• " + check);
        }
    }

    private void ExportTasksCsvButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = _exportService.ExportTasksCsv(FilteredTasks, GetExportDirectory());
            LastExportMessage = $"Tasks exported: {path}";
        }
        catch (Exception ex)
        {
            LastExportMessage = $"Export failed: {ex.Message}";
        }
    }

    private void ShowDesktopNotificationButton_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveAlerts.Count == 0)
        {
            AlertNote = "No active notifications to show.";
            return;
        }

        _desktopNotificationService.ShowOperationalAlertSummary(ActiveAlerts, AlertSummary);
        AlertNote = "Desktop notification sent for the current alert set.";
    }

    private void ExportEquipmentCsvButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = _exportService.ExportEquipmentCsv(_equipmentStore, GetExportDirectory());
            LastExportMessage = $"Equipment exported: {path}";
        }
        catch (Exception ex)
        {
            LastExportMessage = $"Export failed: {ex.Message}";
        }
    }

    private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var path = _exportService.BuildTextReport(
                _equipmentStore,
                _allTasks,
                _healthScoreService.CalculateAverageScore(_equipmentStore, DateTime.Today),
                GetExportDirectory());

            ReportPreview = File.ReadAllText(path, Encoding.UTF8);
            LastExportMessage = $"Report generated: {path}";
        }
        catch (Exception ex)
        {
            LastExportMessage = $"Report generation failed: {ex.Message}";
        }
    }

    private async void ImportCsvButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Import Equipment from CSV",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var result = _csvImportService.Import(dialog.FileName);

            if (result.Errors.Count > 0 && result.Imported.Count == 0)
            {
                RegistryNote = $"Import failed: {result.Errors[0]}";
                return;
            }

            int added = 0, duplicates = 0;
            foreach (var item in result.Imported)
            {
                if (_equipmentStore.Any(eq => eq.SerialNumber.Equals(item.SerialNumber, StringComparison.OrdinalIgnoreCase)))
                {
                    duplicates++;
                    continue;
                }
                _equipmentStore.Add(item);
                added++;
            }

            await PersistEquipmentAsync();
            RefreshAllViews();

            var note = $"Imported {added} records from CSV.";
            if (duplicates > 0) note += $" {duplicates} skipped (duplicate serial).";
            if (result.SkippedRows > 0) note += $" {result.SkippedRows} empty rows ignored.";
            if (result.Errors.Count > 0) note += $" {result.Errors.Count} row parse errors.";
            RegistryNote = note;
        }
        catch (Exception ex)
        {
            RegistryNote = $"Import error: {ex.Message}";
        }
    }

    private async Task PersistEquipmentAsync()
    {
        try
        {
            await _dataService.SaveEquipmentAsync(_equipmentStore, Environment.UserName);
            await RefreshProductionRecordsAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to save equipment data.\n\n{ex.Message}",
                "Save Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void SetComboBoxSelection(System.Windows.Controls.ComboBox comboBox, string target)
    {
        for (var i = 0; i < comboBox.Items.Count; i++)
        {
            if (comboBox.Items[i] is ComboBoxItem item
                && string.Equals(item.Content?.ToString(), target, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = i;
                return;
            }
        }
    }

    private bool TryBuildEquipmentFromForm(out Equipment equipment)
    {
        equipment = new Equipment();

        var name = NameTextBox.Text.Trim();
        var serial = SerialTextBox.Text.Trim();
        var intervalText = IntervalTextBox.Text.Trim();
        var issueCountText = IssueCountTextBox.Text.Trim();
        var notes = NotesTextBox.Text.Trim();
        var category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;
        var statusText = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Healthy";

        if (string.IsNullOrWhiteSpace(name))
        {
            RegistryNote = "Equipment name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(serial))
        {
            RegistryNote = "Serial number is required.";
            return false;
        }

        if (!int.TryParse(intervalText, out var intervalDays) || intervalDays <= 0 || intervalDays > 3650)
        {
            RegistryNote = "Maintenance interval must be a valid number between 1 and 3650.";
            return false;
        }

        if (!int.TryParse(issueCountText, out var issueCount) || issueCount < 0 || issueCount > 10)
        {
            RegistryNote = "Recent issue count must be between 0 and 10.";
            return false;
        }

        if (LastMaintenanceDatePicker.SelectedDate is null)
        {
            RegistryNote = "Please select a last maintenance date.";
            return false;
        }

        if (!Enum.TryParse<EquipmentStatus>(statusText, out var status))
        {
            status = EquipmentStatus.Healthy;
        }

        if (_equipmentStore.Any(e =>
                e.SerialNumber.Equals(serial, StringComparison.OrdinalIgnoreCase)
                && (!_editingEquipmentId.HasValue || e.Id != _editingEquipmentId.Value)))
        {
            RegistryNote = "Serial number already exists. Use unique serial IDs.";
            return false;
        }

        equipment = new Equipment
        {
            Id = _editingEquipmentId ?? Guid.NewGuid(),
            Name = name,
            SerialNumber = serial,
            Category = category,
            LastMaintenanceDate = LastMaintenanceDatePicker.SelectedDate.Value,
            MaintenanceIntervalDays = intervalDays,
            Status = status,
            RecentIssueCount = issueCount,
            Notes = notes
        };

        return true;
    }

    private bool TryBuildMaintenanceLogDetails(
        out string performedBy,
        out string summary,
        out string parts,
        out decimal cost,
        out double laborHours,
        out string notes)
    {
        performedBy = MaintenancePerformedByTextBox.Text.Trim();
        summary = MaintenanceSummaryTextBox.Text.Trim();
        parts = MaintenancePartsTextBox.Text.Trim();
        notes = MaintenanceNotesTextBox.Text.Trim();
        cost = 0;
        laborHours = 0;

        if (string.IsNullOrWhiteSpace(performedBy))
        {
            performedBy = Environment.UserName;
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            RegistryNote = "Maintenance work summary is required.";
            return false;
        }

        var laborText = MaintenanceLaborHoursTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(laborText)
            && (!double.TryParse(laborText, NumberStyles.Number, CultureInfo.CurrentCulture, out laborHours) || laborHours < 0))
        {
            RegistryNote = "Labor hours must be a non-negative number.";
            return false;
        }

        var costText = MaintenanceCostTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(costText)
            && (!decimal.TryParse(costText, NumberStyles.Currency, CultureInfo.CurrentCulture, out cost) || cost < 0))
        {
            RegistryNote = "Maintenance cost must be a non-negative number.";
            return false;
        }

        return true;
    }

    private static string GetExportDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "AeroMaintain",
            "Exports");
    }

    private void SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
        {
            return;
        }

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnClosed(EventArgs e)
    {
        _desktopNotificationService.Dispose();
        base.OnClosed(e);
    }
}

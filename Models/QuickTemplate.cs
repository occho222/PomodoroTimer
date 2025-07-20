using System.Windows.Media;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// クイックテンプレート
    /// </summary>
    public class QuickTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int EstimatedMinutes { get; set; } = 25;
        public string BackgroundColor { get; set; } = "#3B82F6";
        public SolidColorBrush BackgroundBrush => new((System.Windows.Media.Color)(System.Windows.Media.ColorConverter.ConvertFromString(BackgroundColor) ?? System.Windows.Media.Colors.Blue));
        public List<ChecklistItem> DefaultChecklist { get; set; } = new();
    }
}
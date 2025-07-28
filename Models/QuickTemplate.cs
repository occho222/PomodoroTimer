using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Serialization;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// クイックテンプレート
    /// </summary>
    public partial class QuickTemplate : ObservableObject
    {
        [ObservableProperty]
        private string id = string.Empty;

        [ObservableProperty]
        private string displayName = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private string taskDescription = string.Empty;

        [ObservableProperty]
        private string category = string.Empty;

        [ObservableProperty]
        private List<string> tags = new();

        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;

        [ObservableProperty]
        private int estimatedMinutes = 25;

        [ObservableProperty]
        private string backgroundColor = "#3B82F6";

        [JsonIgnore]
        public SolidColorBrush BackgroundBrush => new((System.Windows.Media.Color)(System.Windows.Media.ColorConverter.ConvertFromString(BackgroundColor) ?? System.Windows.Media.Colors.Blue));

        [ObservableProperty]
        private List<ChecklistItem> defaultChecklist = new();

        [ObservableProperty]
        private List<LinkItem> defaultLinks = new();

        public QuickTemplate()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}
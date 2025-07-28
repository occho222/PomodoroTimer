using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    public partial class TaskTemplate : ObservableObject
    {
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private string taskDescription = string.Empty;

        [ObservableProperty]
        private string category = string.Empty;

        [ObservableProperty]
        private int estimatedPomodoros = 1;

        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;

        [ObservableProperty]
        private List<string> tags = new();

        [ObservableProperty]
        private List<ChecklistItem> defaultChecklist = new();

        [ObservableProperty]
        private List<LinkItem> defaultLinks = new();

        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        [ObservableProperty]
        private DateTime updatedAt = DateTime.Now;

        [ObservableProperty]
        private int usageCount = 0;

        [ObservableProperty]
        private DateTime? lastUsedAt;

        public string TagsText
        {
            get => string.Join(", ", Tags);
            set
            {
                var newTags = value?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToList() ?? new List<string>();
                
                if (!Tags.SequenceEqual(newTags))
                {
                    Tags = newTags;
                    OnPropertyChanged(nameof(TagsText));
                }
            }
        }

        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "低",
            TaskPriority.Medium => "中",
            TaskPriority.High => "高",
            TaskPriority.Urgent => "緊急",
            _ => "中"
        };

        public string PriorityColor => Priority switch
        {
            TaskPriority.Low => "Green",
            TaskPriority.Medium => "Orange",
            TaskPriority.High => "Red",
            TaskPriority.Urgent => "DarkRed",
            _ => "Orange"
        };

        public TaskTemplate()
        {
        }

        public TaskTemplate(string name, string taskTitle)
        {
            Name = name ?? string.Empty;
            TaskTitle = taskTitle ?? string.Empty;
        }

        public PomodoroTask CreateTask()
        {
            var task = new PomodoroTask(TaskTitle, EstimatedPomodoros * 25) // ポモドーロ数から分に変換
            {
                Description = TaskDescription,
                Category = Category,
                Priority = Priority,
                TagsText = TagsText
            };

            // チェックリストをコピー
            foreach (var checklistItem in DefaultChecklist)
            {
                task.Checklist.Add(new ChecklistItem(checklistItem.Text)
                {
                    IsChecked = checklistItem.IsChecked
                });
            }

            // リンクをコピー
            foreach (var linkItem in DefaultLinks)
            {
                task.Links.Add(new LinkItem(linkItem.Title, linkItem.Url)
                {
                    CreatedAt = linkItem.CreatedAt
                });
            }

            UsageCount++;
            LastUsedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;

            return task;
        }

        public TaskTemplate Clone()
        {
            var clone = new TaskTemplate
            {
                Id = Guid.NewGuid(),
                Name = $"{Name} - コピー",
                Description = Description,
                TaskTitle = TaskTitle,
                TaskDescription = TaskDescription,
                Category = Category,
                EstimatedPomodoros = EstimatedPomodoros,
                Priority = Priority,
                Tags = new List<string>(Tags),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UsageCount = 0,
                LastUsedAt = null
            };

            // チェックリストもコピー
            foreach (var checklistItem in DefaultChecklist)
            {
                clone.DefaultChecklist.Add(new ChecklistItem(checklistItem.Text)
                {
                    IsChecked = checklistItem.IsChecked
                });
            }

            // リンクもコピー
            foreach (var linkItem in DefaultLinks)
            {
                clone.DefaultLinks.Add(new LinkItem(linkItem.Title, linkItem.Url)
                {
                    CreatedAt = linkItem.CreatedAt
                });
            }

            return clone;
        }

        public void UpdateFromTask(PomodoroTask task)
        {
            TaskTitle = task.Title;
            TaskDescription = task.Description;
            Category = task.Category;
            EstimatedPomodoros = (int)Math.Ceiling((double)task.EstimatedMinutes / 25); // 分からポモドーロ数に変換
            Priority = task.Priority;
            TagsText = task.TagsText;

            // チェックリストも更新
            DefaultChecklist.Clear();
            foreach (var checklistItem in task.Checklist)
            {
                DefaultChecklist.Add(new ChecklistItem(checklistItem.Text)
                {
                    IsChecked = checklistItem.IsChecked
                });
            }

            // リンクも更新
            DefaultLinks.Clear();
            foreach (var linkItem in task.Links)
            {
                DefaultLinks.Add(new LinkItem(linkItem.Title, linkItem.Url)
                {
                    CreatedAt = linkItem.CreatedAt
                });
            }

            UpdatedAt = DateTime.Now;
        }
    }
}
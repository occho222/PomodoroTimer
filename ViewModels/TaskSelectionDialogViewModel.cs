using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TaskStatus = PomodoroTimer.Models.TaskStatus;

namespace PomodoroTimer.ViewModels
{
    public partial class TaskSelectionDialogViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> availableTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> filteredTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> todoTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> waitingTasks = new();

        [ObservableProperty]
        private ObservableCollection<string> availableCategories = new();

        [ObservableProperty]
        private ObservableCollection<string> availablePriorities = new();

        [ObservableProperty]
        private ObservableCollection<string> availableDueDateFilters = new();

        [ObservableProperty]
        private PomodoroTask? selectedTask;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedCategory = string.Empty;

        [ObservableProperty]
        private string selectedPriority = "すべて";

        [ObservableProperty]
        private string selectedDueDateFilter = "すべて";

        [ObservableProperty]
        private string remainingTime = "00:00";

        public event Action<bool?>? DialogResultChanged;

        public TaskSelectionResult Result { get; private set; } = TaskSelectionResult.StopSession;
        public PomodoroTask? SelectedTaskResult => SelectedTask;

        public TaskSelectionDialogViewModel(IPomodoroService pomodoroService, TimeSpan remainingTime)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            RemainingTime = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";

            PropertyChanged += OnPropertyChanged;
            LoadAvailableTasks();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText) || 
                e.PropertyName == nameof(SelectedCategory) ||
                e.PropertyName == nameof(SelectedPriority) ||
                e.PropertyName == nameof(SelectedDueDateFilter))
            {
                ApplyFilters();
            }
        }

        private void LoadAvailableTasks()
        {
            var tasks = _pomodoroService.GetTasks();
            
            // 待機中または未開始のタスクのみを表示
            var availableTasksList = tasks
                .Where(t => t.Status == TaskStatus.Waiting || t.Status == TaskStatus.Todo)
                .OrderByDescending(t => t.Status == TaskStatus.Waiting) // 待機中を優先
                .ThenBy(t => t.DisplayOrder)
                .ToList();

            AvailableTasks.Clear();
            foreach (var task in availableTasksList)
            {
                AvailableTasks.Add(task);
            }

            UpdateCategories();
            UpdatePriorities();
            UpdateDueDateFilters();
            ApplyFilters();
            
            // 最初の進行中タスクを自動選択
            var firstWaitingTask = AvailableTasks.FirstOrDefault(t => t.Status == TaskStatus.Waiting);
            if (firstWaitingTask != null)
            {
                SelectedTask = firstWaitingTask;
            }
        }

        private void UpdateCategories()
        {
            AvailableCategories.Clear();
            AvailableCategories.Add("すべて");
            
            var categories = AvailableTasks
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c);
                
            foreach (var category in categories)
            {
                AvailableCategories.Add(category);
            }
        }

        private void UpdatePriorities()
        {
            AvailablePriorities.Clear();
            AvailablePriorities.Add("すべて");
            AvailablePriorities.Add("緊急");
            AvailablePriorities.Add("高");
            AvailablePriorities.Add("中");
            AvailablePriorities.Add("低");
        }

        private void UpdateDueDateFilters()
        {
            AvailableDueDateFilters.Clear();
            AvailableDueDateFilters.Add("すべて");
            AvailableDueDateFilters.Add("今日まで");
            AvailableDueDateFilters.Add("明日まで");
            AvailableDueDateFilters.Add("今週まで");
            AvailableDueDateFilters.Add("来週まで");
            AvailableDueDateFilters.Add("期日なし");
            AvailableDueDateFilters.Add("期日あり");
        }

        private void ApplyFilters()
        {
            var filtered = AvailableTasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t => 
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategory) && SelectedCategory != "すべて")
            {
                filtered = filtered.Where(t => 
                    t.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SelectedPriority) && SelectedPriority != "すべて")
            {
                filtered = filtered.Where(t => GetPriorityText(t.Priority) == SelectedPriority);
            }

            if (!string.IsNullOrWhiteSpace(SelectedDueDateFilter) && SelectedDueDateFilter != "すべて")
            {
                filtered = ApplyDueDateFilter(filtered, SelectedDueDateFilter);
            }

            // 旧フィルター結果を維持（レガシー対応）
            FilteredTasks.Clear();
            foreach (var task in filtered.OrderByDescending(t => t.Status == TaskStatus.Waiting).ThenBy(t => t.DisplayOrder))
            {
                FilteredTasks.Add(task);
            }

            // カンバン表示用に分離
            var filteredList = filtered.ToList();
            
            TodoTasks.Clear();
            WaitingTasks.Clear();
            
            foreach (var task in filteredList.Where(t => t.Status == TaskStatus.Todo).OrderBy(t => t.DisplayOrder))
            {
                TodoTasks.Add(task);
            }
            
            foreach (var task in filteredList.Where(t => t.Status == TaskStatus.Waiting).OrderBy(t => t.DisplayOrder))
            {
                WaitingTasks.Add(task);
            }
        }

        private string GetPriorityText(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Urgent => "緊急",
                TaskPriority.High => "高",
                TaskPriority.Medium => "中",
                TaskPriority.Low => "低",
                _ => "中"
            };
        }

        private IEnumerable<PomodoroTask> ApplyDueDateFilter(IEnumerable<PomodoroTask> tasks, string filter)
        {
            var today = DateTime.Today;
            
            return filter switch
            {
                "今日まで" => tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= today),
                "明日まで" => tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= today.AddDays(1)),
                "今週まで" => tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= today.AddDays(7 - (int)today.DayOfWeek)),
                "来週まで" => tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= today.AddDays(14 - (int)today.DayOfWeek)),
                "期日なし" => tasks.Where(t => !t.DueDate.HasValue),
                "期日あり" => tasks.Where(t => t.DueDate.HasValue),
                _ => tasks
            };
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = "すべて";
            SelectedPriority = "すべて";
            SelectedDueDateFilter = "すべて";
        }

        [RelayCommand]
        private void ContinueWithSelectedTask()
        {
            if (SelectedTask != null)
            {
                Result = TaskSelectionResult.ContinueWithTask;
                DialogResultChanged?.Invoke(true);
            }
        }

        [RelayCommand]
        private void StartBreak()
        {
            Result = TaskSelectionResult.StartBreak;
            DialogResultChanged?.Invoke(true);
        }

        [RelayCommand]
        private void StopSession()
        {
            Result = TaskSelectionResult.StopSession;
            DialogResultChanged?.Invoke(false);
        }
    }

    public enum TaskSelectionResult
    {
        StopSession,
        StartBreak,
        ContinueWithTask
    }
}
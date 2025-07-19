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
        private ObservableCollection<string> availableCategories = new();

        [ObservableProperty]
        private PomodoroTask? selectedTask;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedCategory = string.Empty;

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
            if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedCategory))
            {
                ApplyFilters();
            }
        }

        private void LoadAvailableTasks()
        {
            var tasks = _pomodoroService.GetTasks();
            
            // 進行中または未開始のタスクのみを表示
            var availableTasksList = tasks
                .Where(t => t.Status == TaskStatus.InProgress || t.Status == TaskStatus.Todo)
                .OrderByDescending(t => t.Status == TaskStatus.InProgress) // 進行中を優先
                .ThenBy(t => t.DisplayOrder)
                .ToList();

            AvailableTasks.Clear();
            foreach (var task in availableTasksList)
            {
                AvailableTasks.Add(task);
            }

            UpdateCategories();
            ApplyFilters();
            
            // 最初の進行中タスクを自動選択
            var firstInProgressTask = AvailableTasks.FirstOrDefault(t => t.Status == TaskStatus.InProgress);
            if (firstInProgressTask != null)
            {
                SelectedTask = firstInProgressTask;
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

            FilteredTasks.Clear();
            foreach (var task in filtered.OrderByDescending(t => t.Status == TaskStatus.InProgress).ThenBy(t => t.DisplayOrder))
            {
                FilteredTasks.Add(task);
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = "すべて";
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
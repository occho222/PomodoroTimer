using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using TaskStatus = PomodoroTimer.Models.TaskStatus;
using MessageBox = System.Windows.MessageBox;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// タスクの一括操作ViewModel
    /// </summary>
    public partial class BulkOperationViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> allTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> selectedTasks = new();

        [ObservableProperty]
        private bool selectAll = false;

        [ObservableProperty]
        private string searchFilter = string.Empty;

        [ObservableProperty]
        private string selectedCategoryFilter = "すべて";

        [ObservableProperty]
        private TaskStatus? selectedStatusFilter;

        [ObservableProperty]
        private TaskPriority? selectedPriorityFilter;

        [ObservableProperty]
        private ObservableCollection<string> availableCategories = new();

        // 一括操作設定
        [ObservableProperty]
        private TaskStatus? bulkNewStatus;

        [ObservableProperty]
        private TaskPriority? bulkNewPriority;

        [ObservableProperty]
        private string bulkNewCategory = string.Empty;

        [ObservableProperty]
        private string bulkNewTags = string.Empty;

        [ObservableProperty]
        private DateTime? bulkNewDueDate;

        [ObservableProperty]
        private bool deleteTasks = false;

        public event Action? DialogClosing;
        public event Action? TasksUpdated;

        public BulkOperationViewModel(IPomodoroService pomodoroService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            
            try
            {
                LoadTasks();
                UpdateAvailableCategories();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BulkOperationViewModel初期化エラー: {ex.Message}");
                // エラーが発生しても最低限の初期化は行う
                AllTasks = new ObservableCollection<PomodoroTask>();
                SelectedTasks = new ObservableCollection<PomodoroTask>();
                AvailableCategories = new ObservableCollection<string> { "すべて" };
            }
        }

        /// <summary>
        /// タスクを読み込む
        /// </summary>
        private void LoadTasks()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AllTasks.Clear();
                    var tasks = _pomodoroService.GetTasks();
                    foreach (var task in tasks)
                    {
                        AllTasks.Add(task);
                    }
                });

                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクの読み込みに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 利用可能なカテゴリを更新
        /// </summary>
        private void UpdateAvailableCategories()
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableCategories.Clear();
                    AvailableCategories.Add("すべて");
                    
                    var categories = _pomodoroService.GetAllCategories();
                    foreach (var category in categories)
                    {
                        AvailableCategories.Add(category);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"カテゴリの更新に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// フィルターを適用
        /// </summary>
        private void ApplyFilters()
        {
            // フィルタリング処理は必要に応じて実装
            // 現在は全タスクを表示
        }

        /// <summary>
        /// 全選択切り替えコマンド
        /// </summary>
        [RelayCommand]
        private void ToggleSelectAll()
        {
            SelectedTasks.Clear();
            
            if (SelectAll)
            {
                foreach (var task in AllTasks)
                {
                    SelectedTasks.Add(task);
                }
            }
        }

        /// <summary>
        /// タスク選択切り替えコマンド
        /// </summary>
        [RelayCommand]
        private void ToggleTaskSelection(PomodoroTask task)
        {
            if (task == null) return;

            if (SelectedTasks.Contains(task))
            {
                SelectedTasks.Remove(task);
            }
            else
            {
                SelectedTasks.Add(task);
            }

            // 全選択状態の更新
            SelectAll = SelectedTasks.Count == AllTasks.Count;
        }

        /// <summary>
        /// フィルターによる選択コマンド
        /// </summary>
        [RelayCommand]
        private void SelectByFilter()
        {
            SelectedTasks.Clear();

            foreach (var task in AllTasks)
            {
                bool matches = true;

                // カテゴリフィルター
                if (!string.IsNullOrEmpty(SelectedCategoryFilter) && SelectedCategoryFilter != "すべて")
                {
                    matches &= task.Category.Equals(SelectedCategoryFilter, StringComparison.OrdinalIgnoreCase);
                }

                // ステータスフィルター
                if (SelectedStatusFilter.HasValue)
                {
                    matches &= task.Status == SelectedStatusFilter.Value;
                }

                // 優先度フィルター
                if (SelectedPriorityFilter.HasValue)
                {
                    matches &= task.Priority == SelectedPriorityFilter.Value;
                }

                // 検索フィルター
                if (!string.IsNullOrEmpty(SearchFilter))
                {
                    matches &= task.Title.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                              task.Description.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase);
                }

                if (matches)
                {
                    SelectedTasks.Add(task);
                }
            }

            SelectAll = SelectedTasks.Count == AllTasks.Count;
        }

        /// <summary>
        /// 一括操作実行コマンド
        /// </summary>
        [RelayCommand]
        private async Task ExecuteBulkOperation()
        {
            if (SelectedTasks.Count == 0)
            {
                MessageBox.Show("操作するタスクを選択してください。", "確認",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var operationsText = new List<string>();
            
            if (DeleteTasks)
            {
                operationsText.Add($"削除: {SelectedTasks.Count}件");
            }
            else
            {
                if (BulkNewStatus.HasValue)
                    operationsText.Add($"ステータス変更: {BulkNewStatus.Value}");
                if (BulkNewPriority.HasValue)
                    operationsText.Add($"優先度変更: {BulkNewPriority.Value}");
                if (!string.IsNullOrEmpty(BulkNewCategory))
                    operationsText.Add($"カテゴリ変更: {BulkNewCategory}");
                if (!string.IsNullOrEmpty(BulkNewTags))
                    operationsText.Add($"タグ設定: {BulkNewTags}");
                if (BulkNewDueDate.HasValue)
                    operationsText.Add($"期限設定: {BulkNewDueDate.Value:yyyy/MM/dd}");
            }

            if (operationsText.Count == 0)
            {
                MessageBox.Show("実行する操作を設定してください。", "確認",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var message = $"選択されたタスク({SelectedTasks.Count}件)に対して以下の操作を実行しますか？\n\n" +
                         string.Join("\n", operationsText);

            var result = MessageBox.Show(message, "一括操作確認",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await PerformBulkOperation();
                    MessageBox.Show($"{SelectedTasks.Count}件のタスクの操作が完了しました。", "完了",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    TasksUpdated?.Invoke();
                    DialogClosing?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"一括操作に失敗しました: {ex.Message}", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 一括操作を実行
        /// </summary>
        private async Task PerformBulkOperation()
        {
            var tasksToProcess = SelectedTasks.ToList();

            foreach (var task in tasksToProcess)
            {
                if (DeleteTasks)
                {
                    _pomodoroService.RemoveTask(task);
                }
                else
                {
                    // ステータス変更
                    if (BulkNewStatus.HasValue)
                    {
                        task.Status = BulkNewStatus.Value;
                        
                        // ステータスに応じて他のプロパティも更新
                        switch (BulkNewStatus.Value)
                        {
                            case TaskStatus.Todo:
                                task.StartedAt = null;
                                task.CompletedAt = null;
                                task.IsCompleted = false;
                                break;
                            case TaskStatus.Waiting:
                                if (task.StartedAt == null)
                                    task.StartedAt = DateTime.Now;
                                task.CompletedAt = null;
                                task.IsCompleted = false;
                                break;
                            case TaskStatus.Completed:
                                if (task.StartedAt == null)
                                    task.StartedAt = DateTime.Now;
                                task.CompletedAt = DateTime.Now;
                                task.IsCompleted = true;
                                break;
                        }
                    }

                    // 優先度変更
                    if (BulkNewPriority.HasValue)
                    {
                        task.Priority = BulkNewPriority.Value;
                    }

                    // カテゴリ変更
                    if (!string.IsNullOrEmpty(BulkNewCategory))
                    {
                        task.Category = BulkNewCategory;
                    }

                    // タグ設定
                    if (!string.IsNullOrEmpty(BulkNewTags))
                    {
                        task.TagsText = BulkNewTags;
                    }

                    // 期限設定
                    if (BulkNewDueDate.HasValue)
                    {
                        task.DueDate = BulkNewDueDate.Value;
                    }

                    _pomodoroService.UpdateTask(task);
                }
            }

            // データを保存
            await _pomodoroService.SaveTasksAsync();
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        [RelayCommand]
        private void Cancel()
        {
            DialogClosing?.Invoke();
        }

        /// <summary>
        /// フィルターリセットコマンド
        /// </summary>
        [RelayCommand]
        private void ResetFilters()
        {
            SearchFilter = string.Empty;
            SelectedCategoryFilter = "すべて";
            SelectedStatusFilter = null;
            SelectedPriorityFilter = null;
            ApplyFilters();
        }

        /// <summary>
        /// 操作設定リセットコマンド
        /// </summary>
        [RelayCommand]
        private void ResetBulkSettings()
        {
            BulkNewStatus = null;
            BulkNewPriority = null;
            BulkNewCategory = string.Empty;
            BulkNewTags = string.Empty;
            BulkNewDueDate = null;
            DeleteTasks = false;
        }
    }
}
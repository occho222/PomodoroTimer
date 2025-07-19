using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using TaskStatus = PomodoroTimer.Models.TaskStatus; // 名前空間の競合を解決

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// ポモドーロタイマーのメインビューモデル
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ITimerService _timerService;
        private readonly IStatisticsService _statisticsService;
        private readonly IDataPersistenceService _dataPersistenceService;
        private readonly ISystemTrayService _systemTrayService;
        private readonly IGraphService _graphService;
        private readonly ITaskTemplateService _taskTemplateService;
        private AppSettings _settings;

        // タスク関連プロパティ
        [ObservableProperty]
        private ObservableCollection<PomodoroTask> tasks = new();

        [ObservableProperty]
        private PomodoroTask? currentTask;

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> filteredTasks = new();

        // カンバンボード用のタスクコレクション
        [ObservableProperty]
        private ObservableCollection<PomodoroTask> todoTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> inProgressTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> executingTasks = new();

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> doneTasksCollection = new();

        // フィルタリング関連プロパティ
        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedCategory = string.Empty;

        [ObservableProperty]
        private string selectedTag = string.Empty;

        [ObservableProperty]
        private TaskPriority? selectedPriority;

        // クイックタスク関連プロパティ
        [ObservableProperty]
        private string quickTaskText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> taskHistory = new();

        [ObservableProperty]
        private ObservableCollection<string> filteredTaskHistory = new();

        [ObservableProperty]
        private ObservableCollection<string> availableCategories = new();

        [ObservableProperty]
        private ObservableCollection<string> availableTags = new();

        // タイマー関連プロパティ
        [ObservableProperty]
        private string timeRemaining = "25:00";

        [ObservableProperty]
        private string sessionTypeText = "作業セッション";

        [ObservableProperty]
        private string startPauseButtonText = "開始";

        [ObservableProperty]
        private bool isRunning = false;

        // 統計関連プロパティ
        [ObservableProperty]
        private int completedPomodoros = 0;

        [ObservableProperty]
        private int completedTasks = 0;

        [ObservableProperty]
        private string totalFocusTime = "0時間 0分";

        [ObservableProperty]
        private DailyStatistics todayStatistics = new();

        // UI関連プロパティ
        [ObservableProperty]
        private System.Windows.Point progressPoint = new(150, 30);

        [ObservableProperty]
        private bool isLargeArc = false;

        [ObservableProperty]
        private double progressValue = 0;

        // ホットキー関連
        private RoutedCommand? _startPauseHotkey;
        private RoutedCommand? _stopHotkey;
        private RoutedCommand? _skipHotkey;
        private RoutedCommand? _addTaskHotkey;
        private RoutedCommand? _settingsHotkey;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainViewModel(IPomodoroService pomodoroService, ITimerService timerService, 
            IStatisticsService statisticsService, IDataPersistenceService dataPersistenceService,
            ISystemTrayService systemTrayService, IGraphService graphService, ITaskTemplateService taskTemplateService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _systemTrayService = systemTrayService ?? throw new ArgumentNullException(nameof(systemTrayService));
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _taskTemplateService = taskTemplateService ?? throw new ArgumentNullException(nameof(taskTemplateService));
            _settings = new AppSettings();

            try
            {
                Console.WriteLine("MainViewModel の初期化を開始します...");

                // 初期化処理を非同期で実行
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await InitializeDataAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"非同期データ初期化でエラー: {ex.Message}");
                    }
                });

                // サービスからタスクを取得
                Tasks = _pomodoroService.GetTasks() ?? new ObservableCollection<PomodoroTask>();
                FilteredTasks = new ObservableCollection<PomodoroTask>(Tasks);

                // タイマーイベントを購読
                SubscribeToTimerEvents();

                // ホットキーを初期化
                InitializeHotkeys();

                // システムトレイを初期化
                _systemTrayService.Initialize();

                // タイマーに設定を適用
                _timerService.UpdateSettings(_settings);

                // フィルタリング用リストを初期化
                UpdateFilteringLists();

                // カンバンボードを初期化
                UpdateKanbanColumns();

                // 統計情報を読み込み
                LoadTodayStatistics();

                // 初期表示更新
                UpdateProgress();
                UpdateSessionTypeText();

                // プロパティ変更監視
                PropertyChanged += OnPropertyChanged;

                Console.WriteLine("MainViewModel が正常に初期化されました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainViewModel の初期化中にエラーが発生しました: {ex.Message}");
                Console.WriteLine($"スタックトレース: {ex.StackTrace}");
                
                // 最小限の初期化を試行
                try
                {
                    InitializeMinimal();
                    Console.WriteLine("最小限の初期化が完了しました");
                }
                catch (Exception minimalEx)
                {
                    Console.WriteLine($"最小限の初期化も失敗しました: {minimalEx.Message}");
                    System.Windows.MessageBox.Show($"アプリケーションの初期化に失敗しました: {ex.Message}", "初期化エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 最小限の初期化（エラー時のフォールバック）
        /// </summary>
        private void InitializeMinimal()
        {
            Tasks = new ObservableCollection<PomodoroTask>();
            FilteredTasks = new ObservableCollection<PomodoroTask>();
            TodoTasks = new ObservableCollection<PomodoroTask>();
            InProgressTasks = new ObservableCollection<PomodoroTask>();
            DoneTasksCollection = new ObservableCollection<PomodoroTask>();
            AvailableCategories = new ObservableCollection<string> { "すべて" };
            AvailableTags = new ObservableCollection<string> { "すべて" };
            TodayStatistics = new DailyStatistics();
            
            // 基本的なタイマー表示を設定
            UpdateSessionTypeText();
            UpdateProgress();
        }

        /// <summary>
        /// データを初期化する
        /// </summary>
        private async Task InitializeDataAsync()
        {
            try
            {
                // 設定を読み込み
                var loadedSettings = await _dataPersistenceService.LoadDataAsync<AppSettings>("settings.json");
                if (loadedSettings != null)
                {
                    _settings = loadedSettings;
                    _timerService.UpdateSettings(_settings);
                }

                // タスクを読み込み
                await _pomodoroService.LoadTasksAsync();

                // 統計データを読み込み
                await _statisticsService.LoadStatisticsAsync();

                // テンプレートデータを読み込み
                await _taskTemplateService.LoadTemplatesAsync();

                // UIスレッドで更新
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Tasks = _pomodoroService.GetTasks();
                    FilteredTasks = new ObservableCollection<PomodoroTask>(Tasks);
                    UpdateFilteringLists();
                    UpdateKanbanColumns(); // カンバンボードも初期化
                    LoadTodayStatistics();
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"データの初期化に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// プロパティ変更時の処理
        /// </summary>
        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText) || 
                e.PropertyName == nameof(SelectedCategory) || 
                e.PropertyName == nameof(SelectedTag) || 
                e.PropertyName == nameof(SelectedPriority))
            {
                ApplyFilters();
                UpdateKanbanColumns(); // カンバンボードも更新
            }
        }

        /// <summary>
        /// ホットキーを初期化する
        /// </summary>
        private void InitializeHotkeys()
        {
            _startPauseHotkey = new RoutedCommand();
            _startPauseHotkey.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));

            _stopHotkey = new RoutedCommand();
            _stopHotkey.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            _skipHotkey = new RoutedCommand();
            _skipHotkey.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));

            _addTaskHotkey = new RoutedCommand();
            _addTaskHotkey.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));

            _settingsHotkey = new RoutedCommand();
            _settingsHotkey.InputGestures.Add(new KeyGesture(Key.F1));
        }

        /// <summary>
        /// ホットキーのコマンドバインディングを取得する
        /// </summary>
        public CommandBinding[] GetHotkeyBindings()
        {
            return new[]
            {
                new CommandBinding(_startPauseHotkey, (s, e) => StartPause()),
                new CommandBinding(_stopHotkey, (s, e) => Stop()),
                new CommandBinding(_skipHotkey, (s, e) => Skip()),
                new CommandBinding(_addTaskHotkey, (s, e) => AddTask()),
                new CommandBinding(_settingsHotkey, (s, e) => OpenSettings())
            };
        }

        /// <summary>
        /// ホットキーのインプットバインディングを取得する
        /// </summary>
        public InputBinding[] GetHotkeyInputBindings()
        {
            return new[]
            {
                new InputBinding(_startPauseHotkey, new KeyGesture(Key.Space, ModifierKeys.Control)),
                new InputBinding(_stopHotkey, new KeyGesture(Key.S, ModifierKeys.Control)),
                new InputBinding(_skipHotkey, new KeyGesture(Key.N, ModifierKeys.Control)),
                new InputBinding(_addTaskHotkey, new KeyGesture(Key.T, ModifierKeys.Control)),
                new InputBinding(_settingsHotkey, new KeyGesture(Key.F1))
            };
        }

        /// <summary>
        /// タイマーイベントを購読する
        /// </summary>
        private void SubscribeToTimerEvents()
        {
            _timerService.TimerStarted += OnTimerStarted;
            _timerService.TimerStopped += OnTimerStopped;
            _timerService.TimerPaused += OnTimerPaused;
            _timerService.TimerResumed += OnTimerResumed;
            _timerService.TimeUpdated += OnTimeUpdated;
            _timerService.SessionCompleted += OnSessionCompleted;
            _timerService.SessionTypeChanged += OnSessionTypeChanged;
        }

        /// <summary>
        /// 開始/一時停止コマンド
        /// </summary>
        [RelayCommand]
        private void StartPause()
        {
            if (IsRunning)
            {
                _timerService.Pause();
            }
            else
            {
                if (_timerService.RemainingTime <= TimeSpan.Zero)
                {
                    // 新しいポモドーロサイクルを開始
                    _timerService.StartNewPomodoroCycle();
                }
                else
                {
                    // 一時停止から再開
                    _timerService.Resume();
                }
            }
        }

        /// <summary>
        /// 停止コマンド
        /// </summary>
        [RelayCommand]
        private void Stop()
        {
            // 実行中タスクを進行中に戻す
            if (CurrentTask != null && CurrentTask.Status == TaskStatus.Executing)
            {
                CurrentTask.StopExecution();
                UpdateKanbanColumns();
                
                // タスクデータを保存
                _ = Task.Run(_pomodoroService.SaveTasksAsync);
            }
            
            _timerService.Stop();
        }

        /// <summary>
        /// スキップコマンド
        /// </summary>
        [RelayCommand]
        private void Skip()
        {
            _timerService.Skip();
        }

        /// <summary>
        /// タスク開始コマンド
        /// </summary>
        /// <param name="task">開始するタスク</param>
        [RelayCommand]
        private void StartTask(PomodoroTask task)
        {
            CurrentTask = task;
            task.StartTask(); // タスクを進行中状態にする
            UpdateKanbanColumns(); // カンバンボードを更新
            
            // タスクデータを保存
            _ = Task.Run(_pomodoroService.SaveTasksAsync);
            
            Stop(); // 現在のタイマーを停止してリセット
        }

        /// <summary>
        /// クイックタスク追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddQuickTask()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(QuickTaskText))
                    return;

                var taskTitle = QuickTaskText.Trim();
                
                // タスク履歴に追加（重複除去）
                if (!TaskHistory.Contains(taskTitle))
                {
                    TaskHistory.Insert(0, taskTitle);
                    // 履歴は最大20件に制限
                    if (TaskHistory.Count > 20)
                    {
                        TaskHistory.RemoveAt(TaskHistory.Count - 1);
                    }
                }

                // スマートデフォルト設定でタスクを作成
                var newTask = new PomodoroTask(taskTitle, 1)
                {
                    Description = string.Empty,
                    Category = "クイック登録",
                    Priority = TaskPriority.Medium,
                    DisplayOrder = Tasks.Count
                };
                
                // サービスにタスクを追加
                _pomodoroService?.AddTask(newTask);
                
                // UI更新
                UpdateFilteringLists();
                ApplyFilters();
                UpdateKanbanColumns();
                
                // 入力フィールドをクリア
                QuickTaskText = string.Empty;
                
                // 視覚的フィードバック（アニメーション的効果）
                ShowQuickTaskFeedback($"タスク「{taskTitle}」を追加しました");
                
                // タスクデータを保存
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _pomodoroService.SaveTasksAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show($"タスクの保存に失敗しました: {ex.Message}", "保存エラー", 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"クイックタスクの追加中にエラーが発生しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// タスク追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddTask()
        {
            try
            {
                var dialog = new Views.TaskDialog();
                if (dialog.ShowDialog() == true)
                {
                    // ダイアログからデータを取得して検証
                    var title = dialog.TaskTitle;
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        System.Windows.MessageBox.Show("タスク名が正しく設定されていません。", "エラー", 
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var newTask = new PomodoroTask(title, dialog.EstimatedPomodoros * 25) // ポモドーロ数を分に変換
                    {
                        Description = dialog.TaskDescription ?? string.Empty,
                        Category = dialog.Category ?? string.Empty,
                        TagsText = dialog.TagsText ?? string.Empty,
                        Priority = dialog.Priority
                    };
                    
                    // サービスにタスクを追加
                    _pomodoroService?.AddTask(newTask);
                    
                    // UI更新
                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns(); // カンバンボードも更新
                    
                    // タスクデータを保存
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _pomodoroService.SaveTasksAsync();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                System.Windows.MessageBox.Show($"タスクの保存に失敗しました: {ex.Message}", "保存エラー", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                            });
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"タスクの追加中にエラーが発生しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// タスク編集コマンド
        /// </summary>
        /// <param name="task">編集するタスク</param>
        [RelayCommand]
        private void EditTask(PomodoroTask task)
        {
            var dialog = new Views.TaskDialog(task);
            if (dialog.ShowDialog() == true)
            {
                task.Title = dialog.TaskTitle;
                task.Description = dialog.TaskDescription;
                task.Category = dialog.Category;
                task.TagsText = dialog.TagsText;
                task.Priority = dialog.Priority;
                task.EstimatedMinutes = dialog.EstimatedPomodoros * 25;
                
                _pomodoroService.UpdateTask(task);
                UpdateFilteringLists();
                ApplyFilters();
                UpdateKanbanColumns(); // カンバンボードも更新
            }
        }

        /// <summary>
        /// タスク詳細ダイアログを開くコマンド
        /// </summary>
        /// <param name="task">詳細を表示するタスク</param>
        [RelayCommand]
        private void OpenTaskDetail(PomodoroTask task)
        {
            try
            {
                var viewModel = new TaskDetailDialogViewModel(_pomodoroService, task);
                var dialog = new Views.TaskDetailDialog(viewModel)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                
                if (dialog.ShowDialog() == true)
                {
                    // タスクが更新された場合、UIを更新
                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"タスク詳細の表示に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// タスク削除コマンド
        /// </summary>
        /// <param name="task">削除するタスク</param>
        [RelayCommand]
        private void DeleteTask(PomodoroTask task)
        {
            var result = System.Windows.MessageBox.Show($"タスク「{task.Title}」を削除しますか？", "確認", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _pomodoroService.RemoveTask(task);
                UpdateFilteringLists();
                ApplyFilters();
                UpdateKanbanColumns(); // カンバンボードも更新
            }
        }

        /// <summary>
        /// タスク完了コマンド
        /// </summary>
        /// <param name="task">完了するタスク</param>
        [RelayCommand]
        private async Task CompleteTask(PomodoroTask task)
        {
            task.CompleteTask(); // タスクを完了状態にする
            _pomodoroService.CompleteTask(task);
            
            // タスク完了を統計に記録
            _statisticsService.RecordTaskComplete(task);
            
            // 統計情報を再読み込み
            LoadTodayStatistics();
            
            // カンバンボードを更新
            UpdateKanbanColumns();
            
            // タスクデータを保存
            _ = Task.Run(_pomodoroService.SaveTasksAsync);

            // セッション継続機能
            if (task == CurrentTask && IsRunning && _settings.ContinueSessionOnTaskComplete)
            {
                await HandleTaskCompletionDuringSession();
            }
        }

        /// <summary>
        /// タスクリセットコマンド（完了から未開始に戻す）
        /// </summary>
        /// <param name="task">リセットするタスク</param>
        [RelayCommand]
        private void ResetTask(PomodoroTask task)
        {
            task.ResetTask();
            UpdateKanbanColumns();
            
            // タスクデータを保存
            _ = Task.Run(_pomodoroService.SaveTasksAsync);
        }

        /// <summary>
        /// タスクの状態変更コマンド
        /// </summary>
        /// <param name="task">状態を変更するタスク</param>
        /// <param name="newStatus">新しい状態</param>
        [RelayCommand]
        private void ChangeTaskStatus(object[] parameters)
        {
            if (parameters?.Length >= 2 && 
                parameters[0] is PomodoroTask task && 
                parameters[1] is TaskStatus newStatus)
            {
                task.Status = newStatus;
                
                // 状態に応じて他のプロパティも更新
                switch (newStatus)
                {
                    case TaskStatus.Todo:
                        task.StartedAt = null;
                        task.CompletedAt = null;
                        task.IsCompleted = false;
                        break;
                    case TaskStatus.InProgress:
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
                        
                        // 統計に記録
                        _statisticsService.RecordTaskComplete(task);
                        LoadTodayStatistics();
                        break;
                }
                
                UpdateKanbanColumns();
                
                // タスクデータを保存
                _ = Task.Run(_pomodoroService.SaveTasksAsync);
            }
        }

        /// <summary>
        /// 設定画面を開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsDialog = new SettingsDialog(_settings, _dataPersistenceService);
            if (settingsDialog.ShowDialog() == true)
            {
                // 新しい設定を適用
                UpdateSettings(settingsDialog.Settings);
                
                // UIの表示を更新
                UpdateSessionTypeText();
                UpdateProgress();
                UpdateTotalFocusTime();

                System.Windows.MessageBox.Show("設定が更新されました。", "設定", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 統計画面を開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenStatistics()
        {
            var statisticsDialog = new StatisticsDialog(_statisticsService, _pomodoroService);
            statisticsDialog.ShowDialog();
        }

        /// <summary>
        /// システムトレイに最小化するコマンド
        /// </summary>
        [RelayCommand]
        private void MinimizeToTray()
        {
            if (_settings.MinimizeToTray)
            {
                _systemTrayService.MinimizeToTray();
            }
        }

        /// <summary>
        /// システムトレイから復元するコマンド
        /// </summary>
        [RelayCommand]
        private void RestoreFromTray()
        {
            _systemTrayService.RestoreFromTray();
        }

        /// <summary>
        /// フィルターをクリアするコマンド
        /// </summary>
        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedCategory = string.Empty;
            SelectedTag = string.Empty;
            SelectedPriority = null;
        }

        /// <summary>
        /// フィルタリング用リストを更新する
        /// </summary>
        private void UpdateFilteringLists()
        {
            AvailableCategories.Clear();
            AvailableCategories.Add("すべて");
            foreach (var category in _pomodoroService.GetAllCategories())
            {
                AvailableCategories.Add(category);
            }

            AvailableTags.Clear();
            AvailableTags.Add("すべて");
            foreach (var tag in _pomodoroService.GetAllTags())
            {
                AvailableTags.Add(tag);
            }
        }

        /// <summary>
        /// フィルターを適用する
        /// </summary>
        private void ApplyFilters()
        {
            var filteredTasks = Tasks.AsEnumerable();

            // テキスト検索
            if (!string.IsNullOrEmpty(SearchText))
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // カテゴリフィルター
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "すべて")
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
            }

            // タグフィルター
            if (!string.IsNullOrEmpty(SelectedTag) && SelectedTag != "すべて")
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.Tags.Any(tag => tag.Equals(SelectedTag, StringComparison.OrdinalIgnoreCase)));
            }

            // 優先度フィルター
            if (SelectedPriority.HasValue)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == SelectedPriority.Value);
            }

            FilteredTasks.Clear();
            foreach (var task in filteredTasks.OrderBy(t => t.DisplayOrder))
            {
                FilteredTasks.Add(task);
            }
        }

        /// <summary>
        /// 本日の統計を読み込む
        /// </summary>
        private void LoadTodayStatistics()
        {
            TodayStatistics = _statisticsService.GetDailyStatistics(DateTime.Today);
            CompletedPomodoros = TodayStatistics.CompletedPomodoros;
            CompletedTasks = TodayStatistics.CompletedTasks;
            UpdateTotalFocusTime();
        }

        /// <summary>
        /// タスクの順序を変更する
        /// </summary>
        /// <param name="draggedTask">ドラッグされたタスク</param>
        /// <param name="targetTask">ドロップ先のタスク</param>
        public void ReorderTasks(PomodoroTask draggedTask, PomodoroTask targetTask)
        {
            var draggedIndex = Tasks.IndexOf(draggedTask);
            var targetIndex = Tasks.IndexOf(targetTask);

            if (draggedIndex != -1 && targetIndex != -1)
            {
                _pomodoroService.ReorderTasks(draggedIndex, targetIndex);
                ApplyFilters();
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public async Task SaveSettingsAsync()
        {
            try
            {
                await _dataPersistenceService.SaveDataAsync("settings.json", _settings);
                await _pomodoroService.SaveTasksAsync();
                await _statisticsService.SaveStatisticsAsync();
                await _taskTemplateService.SaveTemplatesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定の保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// アプリケーション終了時のクリーンアップ
        /// </summary>
        public async Task CleanupAsync()
        {
            try
            {
                await SaveSettingsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"クリーンアップに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 設定を更新する
        /// </summary>
        /// <param name="settings">新しい設定</param>
        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings;
            _timerService.UpdateSettings(settings);
        }

        /// <summary>
        /// 現在の設定を取得する
        /// </summary>
        /// <returns>現在の設定</returns>
        public AppSettings GetCurrentSettings()
        {
            return _settings;
        }

        #region タイマーイベントハンドラ

        private void OnTimerStarted()
        {
            IsRunning = true;
            StartPauseButtonText = "一時停止";
            _systemTrayService.UpdateTimerStatus(true, _timerService.RemainingTime);
        }

        private void OnTimerStopped()
        {
            IsRunning = false;
            StartPauseButtonText = "開始";
            _systemTrayService.UpdateTimerStatus(false, _timerService.RemainingTime);
        }

        private void OnTimerPaused()
        {
            IsRunning = false;
            StartPauseButtonText = "再開";
            _systemTrayService.UpdateTimerStatus(false, _timerService.RemainingTime);
        }

        private void OnTimerResumed()
        {
            IsRunning = true;
            StartPauseButtonText = "一時停止";
            _systemTrayService.UpdateTimerStatus(true, _timerService.RemainingTime);
        }

        private void OnTimeUpdated(TimeSpan remainingTime)
        {
            TimeRemaining = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            UpdateProgress();
            _systemTrayService.UpdateTimerStatus(IsRunning, remainingTime);
        }

        private void OnSessionCompleted(SessionType sessionType)
        {
            // ワークセッションが完了した場合の処理
            if (sessionType == SessionType.Work)
            {
                CompletedPomodoros = _timerService.CompletedPomodoros;
                
                if (CurrentTask != null)
                {
                    _pomodoroService.IncrementTaskPomodoro(CurrentTask);
                    
                    // ポモドーロ完了を統計に記録
                    _statisticsService.RecordPomodoroComplete(CurrentTask, _settings.WorkSessionMinutes);
                    
                    // 実行中タスクを進行中に戻す
                    if (CurrentTask.Status == TaskStatus.Executing)
                    {
                        CurrentTask.StopExecution();
                    }
                    
                    if (CurrentTask.IsCompleted)
                    {
                        CompletedTasks++;
                        // タスク完了を統計に記録
                        _statisticsService.RecordTaskComplete(CurrentTask);
                    }
                }
                else
                {
                    // 現在のタスクが選択されていない場合でも、一般的なポモドーロとして記録
                    var defaultTask = new PomodoroTask("一般作業", 1) { Category = "その他" };
                    _statisticsService.RecordPomodoroComplete(defaultTask, _settings.WorkSessionMinutes);
                }

                // カンバンボードを更新
                UpdateKanbanColumns();

                // 統計情報を再読み込み
                LoadTodayStatistics();

                // 集中時間を更新
                UpdateTotalFocusTime();
                
                // システムトレイ通知
                var sessionName = sessionType switch
                {
                    SessionType.Work => "作業セッション",
                    SessionType.ShortBreak => "短い休憩",
                    SessionType.LongBreak => "長い休憩",
                    _ => "セッション"
                };
                _systemTrayService.ShowBalloonTip("ポモドーロタイマー", 
                    $"{sessionName}が完了しました！", System.Windows.Forms.ToolTipIcon.Info);
            }

            // セッション完了後の状態更新
            IsRunning = false;
            StartPauseButtonText = "開始";
            _systemTrayService.UpdateTimerStatus(false, _timerService.RemainingTime);
        }

        private void OnSessionTypeChanged(SessionType sessionType)
        {
            UpdateSessionTypeText();
        }

        #endregion

        /// <summary>
        /// セッションタイプのテキストを更新する
        /// </summary>
        private void UpdateSessionTypeText()
        {
            SessionTypeText = _timerService.CurrentSessionType switch
            {
                SessionType.Work => "作業セッション",
                SessionType.ShortBreak => "短い休憩",
                SessionType.LongBreak => "長い休憩",
                _ => "作業セッション"
            };
        }

        /// <summary>
        /// 進捗表示を更新する
        /// </summary>
        private void UpdateProgress()
        {
            var totalSeconds = _timerService.SessionDuration.TotalSeconds;
            var remainingSeconds = _timerService.RemainingTime.TotalSeconds;
            
            if (totalSeconds <= 0) return;

            var progress = (totalSeconds - remainingSeconds) / totalSeconds;
            
            // 円形プログレス用
            var angle = progress * 360;
            var radians = (angle - 90) * Math.PI / 180;

            var x = 150 + 120 * Math.Cos(radians);
            var y = 150 + 120 * Math.Sin(radians);

            ProgressPoint = new System.Windows.Point(x, y);
            IsLargeArc = angle > 180;

            // 線形プログレス用（パーセンテージ）
            ProgressValue = progress * 100;
        }

        /// <summary>
        /// 総集中時間を更新する
        /// </summary>
        private void UpdateTotalFocusTime()
        {
            var totalMinutes = CompletedPomodoros * _settings.WorkSessionMinutes;
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            TotalFocusTime = $"{hours}h {minutes}m";
        }

        /// <summary>
        /// カンバンボードの列を更新する
        /// </summary>
        private void UpdateKanbanColumns()
        {
            try
            {
                var filteredTasks = GetFilteredTasks();

                // 各状態別にタスクを分類
                TodoTasks?.Clear();
                InProgressTasks?.Clear();
                ExecutingTasks?.Clear();
                DoneTasksCollection?.Clear();

                if (filteredTasks != null)
                {
                    foreach (var task in filteredTasks.OrderBy(t => t.DisplayOrder))
                    {
                        if (task == null) continue;

                        switch (task.Status)
                        {
                            case TaskStatus.Todo:
                                TodoTasks?.Add(task);
                                break;
                            case TaskStatus.InProgress:
                                InProgressTasks?.Add(task);
                                break;
                            case TaskStatus.Executing:
                                ExecutingTasks?.Add(task);
                                break;
                            case TaskStatus.Completed:
                                DoneTasksCollection?.Add(task);
                                break;
                        }
                    }
                }

                Console.WriteLine($"カンバンボード更新: 未開始={TodoTasks?.Count ?? 0}, 進行中={InProgressTasks?.Count ?? 0}, 実行中={ExecutingTasks?.Count ?? 0}, 完了={DoneTasksCollection?.Count ?? 0}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"カンバンボードの更新中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// フィルタ済みのタスクを取得する
        /// </summary>
        /// <returns>フィルタ適用後のタスクリスト</returns>
        private IEnumerable<PomodoroTask> GetFilteredTasks()
        {
            try
            {
                if (Tasks == null) return Enumerable.Empty<PomodoroTask>();

                var filteredTasks = Tasks.AsEnumerable();

                // テキスト検索
                if (!string.IsNullOrEmpty(SearchText))
                {
                    filteredTasks = filteredTasks.Where(t => 
                        t != null && 
                        (t.Title?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true ||
                        t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));
                }

                // カテゴリフィルター
                if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "すべて")
                {
                    filteredTasks = filteredTasks.Where(t => 
                        t?.Category?.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase) == true);
                }

                // タグフィルター
                if (!string.IsNullOrEmpty(SelectedTag) && SelectedTag != "すべて")
                {
                    filteredTasks = filteredTasks.Where(t => 
                        t?.Tags?.Any(tag => tag?.Equals(SelectedTag, StringComparison.OrdinalIgnoreCase) == true) == true);
                }

                // 優先度フィルター
                if (SelectedPriority.HasValue)
                {
                    filteredTasks = filteredTasks.Where(t => t?.Priority == SelectedPriority.Value);
                }

                return filteredTasks.Where(t => t != null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"フィルタ処理中にエラーが発生しました: {ex.Message}");
                return Enumerable.Empty<PomodoroTask>();
            }
        }

        /// <summary>
        /// クイックタスク追加の視覚的フィードバックを表示
        /// </summary>
        /// <param name="message">表示するメッセージ</param>
        private void ShowQuickTaskFeedback(string message)
        {
            try
            {
                // コンソールログ
                Console.WriteLine($"[フィードバック] {message}");
                
                // システムトレイ通知（設定により）
                if (_settings.ShowNotifications)
                {
                    _systemTrayService.ShowBalloonTip("タスク追加", message, System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"フィードバック表示でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// タスク履歴のフィルタリングを更新するコマンド
        /// </summary>
        /// <param name="searchText">検索テキスト</param>
        [RelayCommand]
        private void UpdateFilteredTaskHistory(string searchText)
        {
            try
            {
                FilteredTaskHistory.Clear();
                
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    return;
                }

                var filtered = TaskHistory
                    .Where(item => item.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .Take(5) // 最大5件まで表示
                    .ToList();

                foreach (var item in filtered)
                {
                    FilteredTaskHistory.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスク履歴フィルタリングでエラー: {ex.Message}");
            }
        }


        /// <summary>
        /// タスクをCSVからインポートするコマンド
        /// </summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task ImportTasks()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSVファイル (*.csv)|*.csv",
                Title = "タスクのインポート"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // CSVからタスクをインポート
                    await _pomodoroService.ImportTasksFromCsvAsync(openFileDialog.FileName);
                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns(); // カンバンボードも更新
                    System.Windows.MessageBox.Show("タスクのインポートが完了しました。", "成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"タスクのインポートに失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// タスクを未開始状態に戻すコマンド（進行中から未開始に戻す）
        /// </summary>
        /// <param name="task">未開始に戻すタスク</param>
        [RelayCommand]
        private void MoveTaskToTodo(PomodoroTask task)
        {
            task.Status = TaskStatus.Todo;
            task.StartedAt = null;
            task.IsCompleted = false;
            UpdateKanbanColumns();
            
            // タスクデータを保存
            _ = Task.Run(_pomodoroService.SaveTasksAsync);
        }

        /// <summary>
        /// 進行中タスクを実行中（25分セッション）に移行するコマンド
        /// </summary>
        /// <param name="task">実行するタスク</param>
        [RelayCommand]
        private void ExecuteTask(PomodoroTask task)
        {
            if (task.Status == TaskStatus.InProgress)
            {
                // 既に実行中のタスクがある場合は停止
                var currentExecutingTask = ExecutingTasks.FirstOrDefault();
                if (currentExecutingTask != null)
                {
                    currentExecutingTask.StopExecution();
                }

                // 現在のタスクをストップしてから新しいタスクを開始
                if (IsRunning)
                {
                    Stop();
                }

                // 新しいタスクを実行中に移行
                task.StartExecution();
                CurrentTask = task;
                
                // タイマーを開始
                StartPause();
                
                UpdateKanbanColumns();
                
                // タスクデータを保存
                _ = Task.Run(_pomodoroService.SaveTasksAsync);
            }
        }

        /// <summary>
        /// 実行中タスクを進行中に戻すコマンド（25分セッション終了）
        /// </summary>
        /// <param name="task">停止するタスク</param>
        [RelayCommand]
        private void StopTaskExecution(PomodoroTask task)
        {
            if (task.Status == TaskStatus.Executing)
            {
                task.StopExecution();
                
                // タイマーも停止
                if (IsRunning)
                {
                    Stop();
                }

                UpdateKanbanColumns();
                
                // タスクデータを保存
                _ = Task.Run(_pomodoroService.SaveTasksAsync);
            }
        }

        /// <summary>
        /// Microsoft Graphからタスクをインポートするコマンド
        /// </summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task ImportFromMicrosoftGraph()
        {
            try
            {
                // 認証を行う
                bool isAuthenticated = await _graphService.AuthenticateAsync();
                if (!isAuthenticated)
                {
                    System.Windows.MessageBox.Show("Microsoft Graphの認証に失敗しました。", "認証エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // インポート先選択ダイアログを表示
                var importDialog = new Views.GraphImportDialog();
                if (importDialog.ShowDialog() != true)
                {
                    return;
                }

                var importedTasks = new List<PomodoroTask>();

                // 選択されたソースからタスクをインポート
                if (importDialog.ImportFromMicrosoftToDo)
                {
                    var todoTasks = await _graphService.ImportTasksFromMicrosoftToDoAsync();
                    importedTasks.AddRange(todoTasks);
                }

                if (importDialog.ImportFromPlanner)
                {
                    var plannerTasks = await _graphService.ImportTasksFromPlannerAsync();
                    importedTasks.AddRange(plannerTasks);
                }

                if (importDialog.ImportFromOutlook)
                {
                    var outlookTasks = await _graphService.ImportTasksFromOutlookAsync();
                    importedTasks.AddRange(outlookTasks);
                }

                if (importedTasks.Count > 0)
                {
                    // PomodoroServiceを通じてタスクをインポート
                    await _pomodoroService.ImportTasksFromGraphAsync(importedTasks);
                    
                    // UI更新
                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns();

                    System.Windows.MessageBox.Show($"{importedTasks.Count} 件のタスクをインポートしました。", "インポート完了", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("インポートするタスクが見つかりませんでした。", "情報", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Microsoft Graphからのタスクインポートに失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Microsoft Graphインポートエラー: {ex}");
            }
        }

        /// <summary>
        /// Microsoft Graphからログアウトするコマンド
        /// </summary>
        [RelayCommand]
        private async System.Threading.Tasks.Task LogoutFromMicrosoftGraph()
        {
            try
            {
                await _graphService.LogoutAsync();
                System.Windows.MessageBox.Show("Microsoft Graphからログアウトしました。", "ログアウト", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"ログアウトに失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// テンプレート管理画面を開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenTemplateManager()
        {
            try
            {
                var viewModel = new TaskTemplateDialogViewModel(_taskTemplateService, _pomodoroService);
                var templateDialog = new Views.TaskTemplateDialog(viewModel);

                viewModel.TaskCreated += (task) =>
                {
                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns();
                };

                templateDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テンプレート管理画面の起動に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// テンプレートからタスクを作成するコマンド
        /// </summary>
        [RelayCommand]
        private void CreateTaskFromTemplate()
        {
            try
            {
                var viewModel = new QuickTemplateDialogViewModel(_taskTemplateService);
                var templateDialog = new Views.QuickTemplateDialog(viewModel);

                if (templateDialog.ShowDialog() == true && templateDialog.SelectedTemplate != null)
                {
                    var task = _taskTemplateService.CreateTaskFromTemplate(templateDialog.SelectedTemplate);
                    _pomodoroService.AddTask(task);

                    UpdateFilteringLists();
                    ApplyFilters();
                    UpdateKanbanColumns();

                    System.Windows.MessageBox.Show($"テンプレート「{templateDialog.SelectedTemplate.Name}」からタスクを作成しました。", 
                        "タスク作成", MessageBoxButton.OK, MessageBoxImage.Information);

                    _ = Task.Run(_pomodoroService.SaveTasksAsync);
                    _ = Task.Run(_taskTemplateService.SaveTemplatesAsync);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テンプレートからのタスク作成に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 既存タスクからテンプレートを作成するコマンド
        /// </summary>
        [RelayCommand]
        private async Task CreateTemplateFromTask(PomodoroTask task)
        {
            if (task == null) return;

            try
            {
                var templateName = System.Windows.MessageBox.Show(
                    $"タスク「{task.Title}」からテンプレートを作成しますか？",
                    "テンプレート作成確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (templateName == MessageBoxResult.Yes)
                {
                    var defaultName = $"{task.Title}のテンプレート";
                    var template = await _taskTemplateService.CreateTemplateFromTaskAsync(
                        task, 
                        defaultName, 
                        $"「{task.Title}」から作成されたテンプレート");
                    
                    System.Windows.MessageBox.Show($"テンプレート「{template.Name}」を作成しました。", "テンプレート作成", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テンプレートの作成に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// セッション中にタスクが完了した時の処理
        /// </summary>
        private async Task HandleTaskCompletionDuringSession()
        {
            try
            {
                // 残り時間を確認
                var remainingTime = _timerService.RemainingTime;
                
                // 残り時間が5分未満の場合は通常通りセッションを継続
                if (remainingTime.TotalMinutes < 5)
                {
                    // 現在のタスクをnullにして、セッション完了まで継続
                    CurrentTask = null;
                    return;
                }

                if (_settings.ShowTaskSelectionDialog)
                {
                    // タスク選択ダイアログを表示
                    await ShowTaskSelectionDialog(remainingTime);
                }
                else
                {
                    // 設定で無効化されている場合は、次の進行中タスクを自動選択
                    await AutoSelectNextTask();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セッション継続処理でエラーが発生しました: {ex.Message}");
                // エラーが発生した場合は現在のタスクをnullにしてセッションを継続
                CurrentTask = null;
            }
        }

        /// <summary>
        /// タスク選択ダイアログを表示
        /// </summary>
        private async Task ShowTaskSelectionDialog(TimeSpan remainingTime)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var viewModel = new TaskSelectionDialogViewModel(_pomodoroService, remainingTime);
                    var dialog = new Views.TaskSelectionDialog(viewModel);
                    dialog.Owner = System.Windows.Application.Current.MainWindow;

                    var result = dialog.ShowDialog();
                    
                    if (result == true)
                    {
                        switch (viewModel.Result)
                        {
                            case TaskSelectionResult.ContinueWithTask:
                                if (viewModel.SelectedTaskResult != null)
                                {
                                    // 選択されたタスクを実行中に移行
                                    var selectedTask = viewModel.SelectedTaskResult;
                                    if (selectedTask.Status == TaskStatus.InProgress)
                                    {
                                        selectedTask.StartExecution();
                                    }
                                    else if (selectedTask.Status == TaskStatus.Todo)
                                    {
                                        selectedTask.StartTask();
                                        selectedTask.StartExecution();
                                    }
                                    
                                    CurrentTask = selectedTask;
                                    UpdateKanbanColumns();
                                    _ = Task.Run(_pomodoroService.SaveTasksAsync);
                                }
                                break;
                                
                            case TaskSelectionResult.StartBreak:
                                // 休憩モードに移行
                                CurrentTask = null;
                                _timerService.Skip(); // 現在のワークセッションをスキップして休憩に移行
                                break;
                                
                            case TaskSelectionResult.StopSession:
                                // セッションを停止
                                Stop();
                                break;
                        }
                    }
                    else
                    {
                        // ダイアログがキャンセルされた場合はセッションを停止
                        Stop();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"タスク選択ダイアログの表示でエラーが発生しました: {ex.Message}");
                    // エラーが発生した場合は自動選択にフォールバック
                    _ = AutoSelectNextTask();
                }
            });
        }

        /// <summary>
        /// 次のタスクを自動選択
        /// </summary>
        private async Task AutoSelectNextTask()
        {
            try
            {
                var tasks = _pomodoroService.GetTasks();
                
                // 進行中のタスクを優先して選択
                var nextTask = tasks.FirstOrDefault(t => t.Status == TaskStatus.InProgress);
                
                // 進行中のタスクがない場合は、未開始のタスクを選択
                if (nextTask == null)
                {
                    nextTask = tasks.FirstOrDefault(t => t.Status == TaskStatus.Todo);
                    if (nextTask != null)
                    {
                        nextTask.StartTask();
                    }
                }

                if (nextTask != null)
                {
                    nextTask.StartExecution();
                    
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentTask = nextTask;
                        UpdateKanbanColumns();
                    });
                    
                    await _pomodoroService.SaveTasksAsync();
                }
                else
                {
                    // 利用可能なタスクがない場合は現在のタスクをnullにしてセッションを継続
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        CurrentTask = null;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"自動タスク選択でエラーが発生しました: {ex.Message}");
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentTask = null;
                });
            }
        }
    }
}
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
        private readonly INotificationService _notificationService;
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
        private ObservableCollection<PomodoroTask> waitingTasks = new();

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

        [ObservableProperty]
        private System.Windows.Controls.ComboBoxItem? selectedDueDateFilter;

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

        [ObservableProperty]
        private ObservableCollection<TaskPriority?> availablePriorities = new();

        // クイックテンプレート関連プロパティ
        [ObservableProperty]
        private ObservableCollection<QuickTemplate> quickTemplates = new();

        // 一括選択関連プロパティ
        [ObservableProperty]
        private bool isBulkSelectionMode = false;

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> selectedTasks = new();

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

        /// <summary>
        /// 未開始タスクの合計見積もり時間（分）
        /// </summary>
        public int TodoTasksEstimatedMinutes => TodoTasks?.Sum(t => t.EstimatedMinutes) ?? 0;

        /// <summary>
        /// 待機中タスクの合計見積もり時間（分）
        /// </summary>
        public int WaitingTasksEstimatedMinutes => WaitingTasks?.Sum(t => t.EstimatedMinutes) ?? 0;

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
            ISystemTrayService systemTrayService, IGraphService graphService, ITaskTemplateService taskTemplateService,
            INotificationService notificationService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _systemTrayService = systemTrayService ?? throw new ArgumentNullException(nameof(systemTrayService));
            _graphService = graphService ?? throw new ArgumentNullException(nameof(graphService));
            _taskTemplateService = taskTemplateService ?? throw new ArgumentNullException(nameof(taskTemplateService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _settings = new AppSettings();

            try
            {
                DebugLog("MainViewModel の初期化を開始します...");

                // 初期化処理を非同期で実行
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await InitializeDataAsync();
                    }
                    catch (Exception ex)
                    {
                        DebugLog($"非同期データ初期化でエラー: {ex.Message}");
                    }
                });

                // 初期化時は空のコレクションを設定（データ読み込み後に更新される）
                Tasks = new ObservableCollection<PomodoroTask>();
                FilteredTasks = new ObservableCollection<PomodoroTask>();

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

                // クイックテンプレートを初期化（同期的に実行）
                Console.WriteLine("InitializeQuickTemplatesSync を呼び出します...");
                InitializeQuickTemplatesSync();
                Console.WriteLine($"InitializeQuickTemplatesSync 完了後のQuickTemplatesの数: {QuickTemplates?.Count ?? 0}");
                
                // デバッグ用：QuickTemplatesの状態を確認
                Console.WriteLine($"初期化時のQuickTemplatesの数: {QuickTemplates?.Count ?? 0}");
                
                // 初期化完了後、少し遅延してからもう一度確認
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000); // 2秒待機
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine($"初期化2秒後のQuickTemplatesの数: {QuickTemplates?.Count ?? 0}");
                        if (QuickTemplates != null && QuickTemplates.Count > 0)
                        {
                            Console.WriteLine("=== 現在のクイックテンプレート一覧 ===");
                            for (int i = 0; i < QuickTemplates.Count; i++)
                            {
                                Console.WriteLine($"  {i+1}. DisplayName: '{QuickTemplates[i].DisplayName}'");
                                Console.WriteLine($"     TaskTitle: '{QuickTemplates[i].TaskTitle}'");
                                Console.WriteLine($"     Description: '{QuickTemplates[i].Description}'");
                                Console.WriteLine($"     Category: '{QuickTemplates[i].Category}'");
                                Console.WriteLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("警告: QuickTemplatesが空またはnullです");
                        }
                    });
                });

                // 初期表示更新
                UpdateProgress();
                UpdateSessionTypeText();

                // プロパティ変更監視
                PropertyChanged += OnPropertyChanged;

                Console.WriteLine("MainViewModel が正常に初期化されました");
            }
            catch (Exception ex)
            {
                DebugLog($"MainViewModel の初期化中にエラーが発生しました: {ex.Message}");
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
            WaitingTasks = new ObservableCollection<PomodoroTask>();
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
                EnsureUIThread(() =>
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
                DebugLog($"データの初期化に失敗しました: {ex.Message}");
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
                e.PropertyName == nameof(SelectedPriority) ||
                e.PropertyName == nameof(SelectedDueDateFilter))
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
                CurrentTask = null;
                OnPropertyChanged(nameof(CurrentTask));
                UpdateKanbanColumns();
                
                // タスクデータを保存
                SaveDataAsync();
                
                Console.WriteLine("実行中タスクを停止し、CurrentTaskをnullに設定しました");
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
        /// タイマーに1分追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddOneMinute()
        {
            _timerService.AddTime(TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// タイマーに10秒追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddTenSeconds()
        {
            _timerService.AddTime(TimeSpan.FromSeconds(10));
        }

        /// <summary>
        /// タイマーから1分減らすコマンド
        /// </summary>
        [RelayCommand]
        private void SubtractOneMinute()
        {
            // タイマー調整時にも経過時間を記録（セッション開始時刻はリセットしない）
            RecordCurrentTaskElapsedTimeForTimerAdjustment();
            _timerService.AddTime(TimeSpan.FromMinutes(-1));
        }

        /// <summary>
        /// タイマーから10秒減らすコマンド
        /// </summary>
        [RelayCommand]
        private void SubtractTenSeconds()
        {
            // タイマー調整時にも経過時間を記録（セッション開始時刻はリセットしない）
            RecordCurrentTaskElapsedTimeForTimerAdjustment();
            _timerService.AddTime(TimeSpan.FromSeconds(-10));
        }

        /// <summary>
        /// タスク開始コマンド
        /// </summary>
        /// <param name="task">開始するタスク</param>
        [RelayCommand]
        private void StartTask(PomodoroTask task)
        {
            task.StartTask(); // タスクを待機中状態にする
            UpdateKanbanColumns(); // カンバンボードを更新
            
            // タスクデータを保存
            SaveDataAsync();
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
                    DisplayOrder = Tasks.Count,
                    DueDate = DateTime.Today
                };
                
                // サービスにタスクを追加
                _pomodoroService?.AddTask(newTask);
                
                // UI更新
                RefreshUI();
                
                // 入力フィールドをクリア
                QuickTaskText = string.Empty;
                
                // 視覚的フィードバック（アニメーション的効果）
                ShowQuickTaskFeedback($"タスク「{taskTitle}」を追加しました");
                
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
                var viewModel = new TaskDetailDialogViewModel(_pomodoroService);
                var dialog = new Views.TaskDetailDialog(viewModel)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                
                if (dialog.ShowDialog() == true)
                {
                    // タスクはTaskDetailDialogViewModelのSaveメソッドで既に追加されているため、
                    // ここではUI更新のみを行う
                    RefreshUI(); // カンバンボードも更新
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
                RefreshUI(); // カンバンボードも更新
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
                    RefreshUI();
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
                RefreshUI(); // カンバンボードも更新
            }
        }

        /// <summary>
        /// タスク完了コマンド
        /// </summary>
        /// <param name="task">完了するタスク</param>
        [RelayCommand]
        private async Task CompleteTask(PomodoroTask task)
        {
            // 既に完了済みの場合は処理しない
            if (task.Status == TaskStatus.Completed)
            {
                return;
            }

            // セッション継続が必要かチェック（CurrentTaskをクリアする前に）
            bool shouldContinueSession = task == CurrentTask && IsRunning && _settings.ContinueSessionOnTaskComplete;

            // 実行中タスクの場合は、経過時間を記録してから実行中画面をクリア
            if (task.Status == TaskStatus.Executing && CurrentTask == task)
            {
                // タスク完了時の経過時間を記録
                RecordCurrentTaskElapsedTime();
                CurrentTask = null;
            }

            task.CompleteTask(); // タスクを完了状態にする
            _pomodoroService.CompleteTask(task);
            
            // タスク完了を統計に記録
            _statisticsService.RecordTaskComplete(task);
            
            // 統計情報を再読み込み
            LoadTodayStatistics();
            
            // カンバンボードを更新
            UpdateKanbanColumns();
            
            // タスクデータを保存
            SaveDataAsync();

            // セッション継続機能
            if (shouldContinueSession)
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
            SaveDataAsync();
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
                        
                        // 統計に記録
                        _statisticsService.RecordTaskComplete(task);
                        LoadTodayStatistics();
                        break;
                }
                
                UpdateKanbanColumns();
                
                // タスクデータを保存
                SaveDataAsync();
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
            SelectedDueDateFilter = null;
        }

        /// <summary>
        /// 全てのUI要素を更新する共通メソッド
        /// </summary>
        private void RefreshUI()
        {
            UpdateFilteringLists();
            ApplyFilters();
            UpdateKanbanColumns();
        }

        /// <summary>
        /// データを非同期で保存する共通メソッド
        /// </summary>
        private void SaveDataAsync()
        {
            _ = Task.Run(_pomodoroService.SaveTasksAsync);
        }

        /// <summary>
        /// デバッグログ出力メソッド（リリースビルドでは無効化）
        /// </summary>
        /// <param name="message">ログメッセージ</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private static void DebugLog(string message)
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// UIスレッドで処理を実行する（必要な場合のみディスパッチ）
        /// </summary>
        /// <param name="action">実行するアクション</param>
        private void EnsureUIThread(Action action)
        {
            if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
                action();
            else
                System.Windows.Application.Current?.Dispatcher?.Invoke(action);
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

            AvailablePriorities.Clear();
            AvailablePriorities.Add(null); // すべて
            AvailablePriorities.Add(TaskPriority.Low);
            AvailablePriorities.Add(TaskPriority.Medium);
            AvailablePriorities.Add(TaskPriority.High);
        }

        /// <summary>
        /// フィルターを適用する
        /// </summary>
        private void ApplyFilters()
        {
            var filteredTasks = GetFilteredTasks();
            
            FilteredTasks.Clear();
            foreach (var task in filteredTasks)
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
        /// クイックテンプレートを同期的に初期化
        /// </summary>
        private void InitializeQuickTemplatesSync()
        {
            try
            {
                Console.WriteLine("同期的クイックテンプレート初期化を開始");
                
                // QuickTemplatesが確実に初期化されていることを確認
                if (QuickTemplates == null)
                {
                    Console.WriteLine("QuickTemplatesがnullのため、新しいObservableCollectionを作成");
                    QuickTemplates = new ObservableCollection<QuickTemplate>();
                }
                
                Console.WriteLine($"初期化前のQuickTemplatesの数: {QuickTemplates.Count}");
                
                // 初期化時はデフォルトテンプレートを即座に作成
                CreateDefaultQuickTemplates();
                
                // 非同期でファイルからの読み込みを試行
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var templates = await _dataPersistenceService.LoadDataAsync<List<QuickTemplate>>("quick_templates.json");
                        if (templates != null && templates.Count > 0)
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                Console.WriteLine($"ファイルから {templates.Count} 個のテンプレートを読み込み、既存のデフォルトを置き換え");
                                QuickTemplates.Clear();
                                foreach (var template in templates)
                                {
                                    QuickTemplates.Add(template);
                                    Console.WriteLine($"読み込み済みテンプレート: DisplayName='{template.DisplayName}', TaskTitle='{template.TaskTitle}'");
                                }
                                Console.WriteLine($"ファイルからの読み込み完了。QuickTemplatesの数: {QuickTemplates.Count}");
                                OnPropertyChanged(nameof(QuickTemplates));
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ファイルからの読み込みに失敗（デフォルトを維持）: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"同期的初期化に失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// クイックテンプレートを読み込む
        /// </summary>
        private async void LoadQuickTemplates()
        {
            try
            {
                Console.WriteLine("クイックテンプレートの読み込みを開始");
                var templates = await _dataPersistenceService.LoadDataAsync<List<QuickTemplate>>("quick_templates.json");
                
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    QuickTemplates.Clear();
                    if (templates != null && templates.Count > 0)
                    {
                        Console.WriteLine($"ファイルから {templates.Count} 個のテンプレートを読み込み");
                        foreach (var template in templates)
                        {
                            QuickTemplates.Add(template);
                            Console.WriteLine($"LoadQuickTemplates - 読み込み済みテンプレート: DisplayName='{template.DisplayName}', TaskTitle='{template.TaskTitle}'");
                        }
                    }
                    else
                    {
                        Console.WriteLine("テンプレートファイルが見つからないか空のため、デフォルトテンプレートを作成");
                        // デフォルトのクイックテンプレートを作成
                        CreateDefaultQuickTemplates();
                    }
                    Console.WriteLine($"QuickTemplatesに追加後の数: {QuickTemplates.Count}");
                    OnPropertyChanged(nameof(QuickTemplates));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"クイックテンプレートの読み込みに失敗しました: {ex.Message}");
                // エラー時もデフォルトテンプレートを作成
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    CreateDefaultQuickTemplates();
                    Console.WriteLine($"エラー時のQuickTemplatesの数: {QuickTemplates.Count}");
                    OnPropertyChanged(nameof(QuickTemplates));
                });
            }
        }

        /// <summary>
        /// デフォルトのクイックテンプレートを作成
        /// </summary>
        private void CreateDefaultQuickTemplates()
        {
            try
            {
                Console.WriteLine("デフォルトクイックテンプレートの作成を開始");
                
                // QuickTemplatesが確実に存在することを確認
                if (QuickTemplates == null)
                {
                    Console.WriteLine("CreateDefaultQuickTemplates: QuickTemplatesがnullです。新しく作成します。");
                    QuickTemplates = new ObservableCollection<QuickTemplate>();
                }
                
                QuickTemplates.Clear();
                Console.WriteLine("QuickTemplatesをクリアしました");
                
                var defaultTemplates = new List<QuickTemplate>
                {
                    new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = "📧 メール確認",
                        Description = "メールをチェックして返信",
                        TaskTitle = "メール確認・返信",
                        TaskDescription = "重要なメールをチェックして必要に応じて返信する",
                        Category = "コミュニケーション",
                        Tags = new List<string> { "メール", "連絡" },
                        Priority = TaskPriority.Medium,
                        EstimatedMinutes = 25,
                        BackgroundColor = "#3B82F6"
                    },
                    new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = "📝 文書作成",
                        Description = "文書やレポートの作成",
                        TaskTitle = "文書作成",
                        TaskDescription = "必要な文書やレポートを作成する",
                        Category = "文書作業",
                        Tags = new List<string> { "文書", "作成" },
                        Priority = TaskPriority.Medium,
                        EstimatedMinutes = 50,
                        BackgroundColor = "#10B981"
                    },
                    new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = "💡 アイデア整理",
                        Description = "アイデアや思考の整理",
                        TaskTitle = "アイデア・思考整理",
                        TaskDescription = "散らかった考えやアイデアを整理してまとめる",
                        Category = "企画・思考",
                        Tags = new List<string> { "アイデア", "整理" },
                        Priority = TaskPriority.Low,
                        EstimatedMinutes = 25,
                        BackgroundColor = "#8B5CF6"
                    },
                    new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = "🔍 調査・リサーチ",
                        Description = "情報収集と調査",
                        TaskTitle = "調査・リサーチ",
                        TaskDescription = "必要な情報を調査・収集する",
                        Category = "調査",
                        Tags = new List<string> { "調査", "リサーチ" },
                        Priority = TaskPriority.Medium,
                        EstimatedMinutes = 25,
                        BackgroundColor = "#F59E0B"
                    },
                    new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = "⚡ 緊急対応",
                        Description = "緊急性の高いタスク",
                        TaskTitle = "緊急対応",
                        TaskDescription = "緊急に対応が必要なタスク",
                        Category = "緊急",
                        Tags = new List<string> { "緊急", "対応" },
                        Priority = TaskPriority.High,
                        EstimatedMinutes = 25,
                        BackgroundColor = "#EF4444"
                    }
                };

                foreach (var template in defaultTemplates)
                {
                    QuickTemplates.Add(template);
                }
                
                Console.WriteLine($"デフォルトテンプレート作成完了。QuickTemplatesの数: {QuickTemplates.Count}");
                
                // 作成されたテンプレートの詳細を出力
                for (int i = 0; i < QuickTemplates.Count; i++)
                {
                    Console.WriteLine($"テンプレート{i}: DisplayName='{QuickTemplates[i].DisplayName}', TaskTitle='{QuickTemplates[i].TaskTitle}'");
                }
                
                // プロパティ変更通知を明示的に発行
                OnPropertyChanged(nameof(QuickTemplates));
                
                // さらに強制的に更新を試行
                System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    OnPropertyChanged(nameof(QuickTemplates));
                    Console.WriteLine("Dispatcher経由でプロパティ変更通知を再送信しました");
                });

                // デフォルトテンプレートを保存
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _dataPersistenceService.SaveDataAsync("quick_templates.json", defaultTemplates);
                        Console.WriteLine("デフォルトのクイックテンプレートを作成・保存しました");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"デフォルトテンプレートの保存に失敗: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"デフォルトテンプレートの作成に失敗: {ex.Message}");
            }
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
        /// タスクを保存する（公開メソッド）
        /// </summary>
        public async Task SaveTasksAsync()
        {
            try
            {
                await _pomodoroService.SaveTasksAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクの保存に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// カンバンボードを更新する（公開メソッド）
        /// </summary>
        public void UpdateKanbanColumns()
        {
            try
            {
                var filteredTasks = GetFilteredTasks();

                // 各状態別にタスクを分類
                TodoTasks?.Clear();
                WaitingTasks?.Clear();
                ExecutingTasks?.Clear();
                DoneTasksCollection?.Clear();

                PomodoroTask? currentExecutingTask = null;

                if (filteredTasks != null)
                {
                    // 期限の早い順、期限なしのものは最後、同じ期限内では優先度順、最後にDisplayOrder順でソート
                    var sortedTasks = filteredTasks.OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                                                  .ThenByDescending(t => t.Priority)
                                                  .ThenBy(t => t.DisplayOrder);

                    foreach (var task in sortedTasks)
                    {
                        if (task == null) continue;

                        switch (task.Status)
                        {
                            case TaskStatus.Todo:
                                TodoTasks?.Add(task);
                                break;
                            case TaskStatus.Waiting:
                                WaitingTasks?.Add(task);
                                break;
                            case TaskStatus.Executing:
                                ExecutingTasks?.Add(task);
                                currentExecutingTask = task;
                                break;
                            case TaskStatus.Completed:
                                DoneTasksCollection?.Add(task);
                                break;
                        }
                    }
                }

                // 実行中タスクがある場合はCurrentTaskに設定、ない場合はnullに設定
                if (currentExecutingTask != null && CurrentTask != currentExecutingTask)
                {
                    CurrentTask = currentExecutingTask;
                    OnPropertyChanged(nameof(CurrentTask));
                    Console.WriteLine($"CurrentTaskを実行中タスク「{currentExecutingTask.Title}」に設定しました");
                }
                else if (currentExecutingTask == null && CurrentTask != null)
                {
                    CurrentTask = null;
                    OnPropertyChanged(nameof(CurrentTask));
                    Console.WriteLine("実行中タスクがないため、CurrentTaskをnullに設定しました");
                }

                Console.WriteLine($"カンバンボード更新: 未開始={TodoTasks?.Count ?? 0}, 待機中={WaitingTasks?.Count ?? 0}, 実行中={ExecutingTasks?.Count ?? 0}, 完了={DoneTasksCollection?.Count ?? 0}");
                Console.WriteLine($"CurrentTask: {CurrentTask?.Title ?? "null"}, CurrentTask.Status: {CurrentTask?.Status.ToString() ?? "null"}");
                
                // 見積もり時間合計の変更を通知
                OnPropertyChanged(nameof(TodoTasksEstimatedMinutes));
                OnPropertyChanged(nameof(WaitingTasksEstimatedMinutes));
            }
            catch (Exception ex)
            {
                DebugLog($"カンバンボードの更新中にエラーが発生しました: {ex.Message}");
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
            _notificationService.UpdateSettings(settings.EnableSoundNotification, settings.EnableDesktopNotification);
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
                    
                    // セッション終了後もタスクは実行中状態を維持（StopExecutionは削除）
                    
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
            }

            // 全てのセッションタイプで通知を表示
            var sessionName = sessionType switch
            {
                SessionType.Work => "作業セッション",
                SessionType.ShortBreak => "短い休憩",
                SessionType.LongBreak => "長い休憩",
                _ => "セッション"
            };
            
            var notificationMessage = $"{sessionName}が完了しました！";
            
            // 設定に応じて通知を表示
            if (_settings.ShowNotifications)
            {
                // ポップアップ通知を表示（ブロッキング）
                _notificationService.ShowDesktopNotification("ポモドーロタイマー", notificationMessage);
            }
            else
            {
                // 設定がOFFでもシステムトレイ通知は表示
                _systemTrayService.ShowBalloonTip("ポモドーロタイマー", notificationMessage, System.Windows.Forms.ToolTipIcon.Info);
            }

            // セッション完了後の状態更新
            IsRunning = false;
            StartPauseButtonText = "開始";
            _systemTrayService.UpdateTimerStatus(false, _timerService.RemainingTime);
            
            // 自動開始設定に基づいて次のセッションを開始
            if (_settings.AutoStartNextSession)
            {
                // 自動開始が有効な場合は次のセッションを開始
                // セッション種別の切り替えを待つため、少し遅延させる
                _ = Task.Run(async () =>
                {
                    await Task.Delay(100); // TimerServiceのセッション切り替えを待つ
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StartNextSession();
                    });
                });
            }
            // AutoStartNextSessionがfalseの場合は何もしない（ユーザーが手動で開始ボタンを押すまで待機）
        }

        /// <summary>
        /// 自動開始時に次のセッションを開始する
        /// </summary>
        private void StartNextSession()
        {
            // 現在のセッションタイプに応じてタイマーを開始
            var currentSessionType = _timerService.CurrentSessionType;
            var duration = currentSessionType switch
            {
                SessionType.Work => TimeSpan.FromMinutes(_settings.WorkSessionMinutes),
                SessionType.ShortBreak => TimeSpan.FromMinutes(_settings.ShortBreakMinutes),
                SessionType.LongBreak => TimeSpan.FromMinutes(_settings.LongBreakMinutes),
                _ => TimeSpan.FromMinutes(_settings.WorkSessionMinutes)
            };
            
            // 適切な時間でタイマーを開始
            _timerService.Start(duration);
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
        /// 現在実行中のタスクに経過時間を記録する
        /// </summary>
        private void RecordCurrentTaskElapsedTime()
        {
            if (CurrentTask != null && CurrentTask.CurrentSessionStartTime.HasValue)
            {
                // 現在のセッション開始時刻からの経過時間を計算（分単位）
                var now = DateTime.Now;
                var elapsedTime = now - CurrentTask.CurrentSessionStartTime.Value;
                var elapsedMinutes = (int)Math.Ceiling(elapsedTime.TotalMinutes);
                
                // 1分以上の場合のみ記録
                if (elapsedMinutes > 0)
                {
                    CurrentTask.ActualMinutes += elapsedMinutes;
                    Console.WriteLine($"タスク '{CurrentTask.Title}' に {elapsedMinutes}分を記録。合計: {CurrentTask.ActualMinutes}分");
                    
                    // セッション開始時刻をリセット
                    CurrentTask.CurrentSessionStartTime = null;
                    
                    // データを保存
                    SaveDataAsync();
                }
            }
        }

        /// <summary>
        /// タイマー調整時に現在実行中のタスクに経過時間を記録する（セッション開始時刻はリセットしない）
        /// </summary>
        private void RecordCurrentTaskElapsedTimeForTimerAdjustment()
        {
            if (CurrentTask != null && CurrentTask.CurrentSessionStartTime.HasValue)
            {
                // 現在のセッション開始時刻からの経過時間を計算（分単位）
                var now = DateTime.Now;
                var elapsedTime = now - CurrentTask.CurrentSessionStartTime.Value;
                var elapsedMinutes = (int)Math.Ceiling(elapsedTime.TotalMinutes);
                
                // 1分以上の場合のみ記録
                if (elapsedMinutes > 0)
                {
                    CurrentTask.ActualMinutes += elapsedMinutes;
                    Console.WriteLine($"タイマー調整時: タスク '{CurrentTask.Title}' に {elapsedMinutes}分を記録。合計: {CurrentTask.ActualMinutes}分");
                    
                    // セッション開始時刻を現在時刻に更新（計測継続のため）
                    CurrentTask.CurrentSessionStartTime = now;
                    
                    // データを保存
                    SaveDataAsync();
                }
            }
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

                // 期限フィルター
                if (SelectedDueDateFilter != null)
                {
                    var filterTag = SelectedDueDateFilter.Tag?.ToString();
                    var today = DateTime.Today;
                    
                    filteredTasks = filterTag switch
                    {
                        "None" => filteredTasks.Where(t => t?.DueDate == null),
                        "Today" => filteredTasks.Where(t => t?.DueDate.HasValue == true && t.DueDate.Value.Date <= today),
                        "Tomorrow" => filteredTasks.Where(t => t?.DueDate.HasValue == true && t.DueDate.Value.Date <= today.AddDays(1)),
                        "ThisWeek" => filteredTasks.Where(t => t?.DueDate.HasValue == true && t.DueDate.Value.Date <= today.AddDays(7)),
                        "Overdue" => filteredTasks.Where(t => t?.DueDate.HasValue == true && t.DueDate.Value.Date < today),
                        "All" or _ => filteredTasks
                    };
                }

                // 期限の早い順、期限なしのものは最後、同じ期限内では優先度順、最後にDisplayOrder順でソート
                return filteredTasks.Where(t => t != null)
                                   .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                                   .ThenByDescending(t => t.Priority)
                                   .ThenBy(t => t.DisplayOrder);
            }
            catch (Exception ex)
            {
                DebugLog($"フィルタ処理中にエラーが発生しました: {ex.Message}");
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
                    RefreshUI(); // カンバンボードも更新
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
            SaveDataAsync();
        }

        /// <summary>
        /// 進行中タスクを実行中（25分セッション）に移行するコマンド
        /// </summary>
        /// <param name="task">実行するタスク</param>
        [RelayCommand]
        private void ExecuteTask(PomodoroTask task)
        {
            if (task.Status == TaskStatus.Waiting)
            {
                // 既に実行中のタスクがある場合は停止
                var currentExecutingTask = ExecutingTasks.FirstOrDefault();
                if (currentExecutingTask != null)
                {
                    currentExecutingTask.StopExecution();
                }

                // 現在実行中のタスクの経過時間を記録してから新しいタスクを開始
                RecordCurrentTaskElapsedTime();
                
                if (IsRunning)
                {
                    Stop();
                }

                // 新しいタスクを実行中に移行
                task.StartExecution();
                CurrentTask = task;
                
                // セッション開始時刻を記録
                CurrentTask.CurrentSessionStartTime = DateTime.Now;
                
                // タイマーを開始
                StartPause();
                
                UpdateKanbanColumns();
                
                // CurrentTaskの変更を明示的に通知
                OnPropertyChanged(nameof(CurrentTask));
                
                // タスクデータを保存
                SaveDataAsync();
                
                Console.WriteLine($"タスク「{task.Title}」を実行中に設定しました。CurrentTask: {CurrentTask?.Title ?? "null"}");
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
                // 経過時間を記録してからタスクを停止
                RecordCurrentTaskElapsedTime();
                
                task.StopExecution();
                
                // タイマーも停止
                if (IsRunning)
                {
                    _timerService.Stop();
                }

                // 実行中画面をクリア
                CurrentTask = null;
                OnPropertyChanged(nameof(CurrentTask));
                
                UpdateKanbanColumns();
                
                // タスクデータを保存
                SaveDataAsync();
                
                Console.WriteLine($"タスク「{task.Title}」の実行を停止し、CurrentTaskをnullに設定しました");
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
                    RefreshUI();

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
                DebugLog($"Microsoft Graphインポートエラー: {ex}");
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
                    RefreshUI();
                };

                templateDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テンプレート管理画面の起動に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void OpenProjectTagManager()
        {
            try
            {
                var viewModel = new ProjectTagManagerViewModel(_pomodoroService);
                var dialog = new Views.ProjectTagManagerDialog
                {
                    DataContext = viewModel
                };
                
                var mainWindow = System.Windows.Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    dialog.Owner = mainWindow;
                }

                viewModel.DialogClosing += () => dialog.Close();
                dialog.ShowDialog();

                // ダイアログ閉じた後にフィルタリング用のリストを更新
                UpdateFilteringLists();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"プロジェクト・タグ管理画面の表示でエラー: {ex.Message}");
                System.Windows.MessageBox.Show($"プロジェクト・タグ管理画面の表示に失敗しました: {ex.Message}",
                    "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }



        [RelayCommand]
        private void RefreshAll()
        {
            try
            {
                RefreshUI();
                LoadTodayStatistics();
                
                // QuickTemplatesの状態も出力
                Console.WriteLine($"RefreshAll実行時のQuickTemplatesの数: {QuickTemplates?.Count ?? 0}");
                if (QuickTemplates != null)
                {
                    for (int i = 0; i < QuickTemplates.Count; i++)
                    {
                        Console.WriteLine($"RefreshAll - テンプレート{i}: DisplayName='{QuickTemplates[i].DisplayName}'");
                    }
                }
                
                Console.WriteLine("データを更新しました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"データの更新に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// クイックテンプレート管理ダイアログを開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenQuickTemplateManager()
        {
            try
            {
                var viewModel = new QuickTemplateManagerViewModel(_dataPersistenceService);
                var dialog = new Views.QuickTemplateManagerDialog(viewModel)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };
                
                dialog.ShowDialog();
                
                // ダイアログ閉じた後にクイックテンプレートを再読み込み
                LoadQuickTemplates();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"クイックテンプレート管理画面の表示に失敗しました: {ex.Message}",
                    "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// クイックテンプレートからタスクを作成するコマンド
        /// </summary>
        /// <param name="template">使用するテンプレート</param>
        [RelayCommand]
        private void CreateTaskFromTemplate(QuickTemplate template)
        {
            Console.WriteLine($"CreateTaskFromTemplate が呼び出されました。テンプレート: {template?.DisplayName ?? "null"}");
            
            if (template == null) 
            {
                Console.WriteLine("テンプレートがnullのため処理を中断");
                return;
            }

            try
            {
                var newTask = new PomodoroTask
                {
                    Id = Guid.NewGuid(),
                    Title = template.TaskTitle,
                    Description = template.TaskDescription,
                    Category = template.Category,
                    TagsText = string.Join(", ", template.Tags),
                    Priority = template.Priority,
                    EstimatedMinutes = template.EstimatedMinutes,
                    Status = TaskStatus.Todo,
                    CreatedAt = DateTime.Now,
                    IsCompleted = false
                };

                // チェックリストをコピー
                foreach (var checklistItem in template.DefaultChecklist)
                {
                    newTask.Checklist.Add(new ChecklistItem(checklistItem.Text)
                    {
                        IsChecked = checklistItem.IsChecked
                    });
                }

                _pomodoroService.AddTask(newTask);
                
                // UI更新
                RefreshUI();
                

                Console.WriteLine($"テンプレート「{template.DisplayName}」からタスク「{newTask.Title}」を作成しました");
                
                // 作成成功を通知
                ShowQuickTaskFeedback($"テンプレート「{template.DisplayName}」からタスクを作成しました");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"テンプレートからタスクの作成に失敗しました: {ex.Message}", "エラー",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ToggleBulkSelection()
        {
            IsBulkSelectionMode = !IsBulkSelectionMode;
            if (!IsBulkSelectionMode)
            {
                SelectedTasks.Clear();
            }
            Console.WriteLine($"一括選択モード: {(IsBulkSelectionMode ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 一括操作ダイアログを開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenBulkOperation()
        {
            try
            {
                var viewModel = new BulkOperationViewModel(_pomodoroService);
                var dialog = new Views.BulkOperationDialog(viewModel)
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                // タスク更新イベントの設定
                viewModel.TasksUpdated += () =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        RefreshUI();
                        LoadTodayStatistics();
                    });
                };

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"一括操作画面の表示に失敗しました: {ex.Message}",
                    "エラー", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task OpenSortOptions()
        {
            try
            {
                // TODO: SortOptionsDialogを実装する
                throw new NotImplementedException("SortOptionsDialogは未実装です");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ソートオプションダイアログの表示に失敗しました: {ex.Message}");
                // ソートダイアログが存在しない場合は簡易ソートを実行
                await ApplySimpleSort();
            }
        }

        private async Task ApplySort(string criteria, string direction)
        {
            try
            {
                var allTasks = _pomodoroService.GetTasks().ToList();
                
                switch (criteria?.ToLower())
                {
                    case "priority":
                        allTasks = direction == "desc" 
                            ? allTasks.OrderByDescending(t => t.Priority).ToList()
                            : allTasks.OrderBy(t => t.Priority).ToList();
                        break;
                    case "duedate":
                        allTasks = direction == "desc"
                            ? allTasks.OrderByDescending(t => t.DueDate ?? DateTime.MaxValue).ToList()
                            : allTasks.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ToList();
                        break;
                    case "created":
                        allTasks = direction == "desc"
                            ? allTasks.OrderByDescending(t => t.CreatedAt).ToList()
                            : allTasks.OrderBy(t => t.CreatedAt).ToList();
                        break;
                    default:
                        allTasks = allTasks.OrderBy(t => t.Title).ToList();
                        break;
                }

                // タスクの順序を更新（表示順序を更新）
                for (int i = 0; i < allTasks.Count; i++)
                {
                    allTasks[i].DisplayOrder = i;
                }
                await _pomodoroService.SaveTasksAsync();
                UpdateKanbanColumns();
                Console.WriteLine($"タスクを '{criteria}' でソートしました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ソート処理に失敗しました: {ex.Message}");
            }
        }

        private async Task ApplySimpleSort()
        {
            try
            {
                var allTasks = _pomodoroService.GetTasks()
                    .OrderByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
                    .ThenBy(t => t.CreatedAt)
                    .ToList();

                // タスクの表示順序を更新
                for (int i = 0; i < allTasks.Count; i++)
                {
                    allTasks[i].DisplayOrder = i;
                }
                await _pomodoroService.SaveTasksAsync();
                UpdateKanbanColumns();
                Console.WriteLine("タスクを優先度・期限順でソートしました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"簡易ソート処理に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// テンプレートからタスクを作成するコマンド
        /// </summary>
        [RelayCommand]
        private void CreateTaskFromTaskTemplate()
        {
            try
            {
                var viewModel = new QuickTemplateDialogViewModel(_taskTemplateService);
                var templateDialog = new Views.QuickTemplateDialog(viewModel);

                if (templateDialog.ShowDialog() == true && templateDialog.SelectedTemplate != null)
                {
                    var task = _taskTemplateService.CreateTaskFromTemplate(templateDialog.SelectedTemplate);
                    _pomodoroService.AddTask(task);

                    RefreshUI();

                    System.Windows.MessageBox.Show($"テンプレート「{templateDialog.SelectedTemplate.Name}」からタスクを作成しました。", 
                        "タスク作成", MessageBoxButton.OK, MessageBoxImage.Information);

                    SaveDataAsync();
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
                    // 設定で無効化されている場合は、次の待機中タスクを自動選択
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
                                    if (selectedTask.Status == TaskStatus.Waiting)
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
                                    SaveDataAsync();
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
                
                // 待機中のタスクを優先して選択
                var nextTask = tasks.FirstOrDefault(t => t.Status == TaskStatus.Waiting);
                
                // 待機中のタスクがない場合は、未開始のタスクを選択
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
                    // 未開始タスクの場合は待機中を経由してから実行中に移行
                    if (nextTask.Status == TaskStatus.Todo)
                    {
                        // 既に StartTask() は上で呼ばれているので、ここでは StartExecution() のみ
                    }
                    
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

        /// <summary>
        /// チェックリストアイテムのチェック状態をトグルするコマンド
        /// </summary>
        /// <param name="checklistItem">チェック状態を変更するアイテム</param>
        [RelayCommand]
        private async Task ToggleChecklistItem(object checklistItem)
        {
            try
            {
                if (checklistItem is ChecklistItem item && CurrentTask != null)
                {
                    // チェック状態をトグル
                    item.IsChecked = !item.IsChecked;
                    
                    // 完了日時を更新
                    item.CompletedAt = item.IsChecked ? DateTime.Now : null;
                    
                    // タスクデータを保存
                    await _pomodoroService.SaveTasksAsync();
                    
                    Console.WriteLine($"チェックリストアイテム「{item.Text}」を{(item.IsChecked ? "完了" : "未完了")}に変更しました");
                    
                    // チェックリスト完了率をログ出力
                    if (CurrentTask.Checklist?.Count > 0)
                    {
                        var completedCount = CurrentTask.Checklist.Count(c => c.IsChecked);
                        var completionRate = (double)completedCount / CurrentTask.Checklist.Count * 100;
                        Console.WriteLine($"チェックリスト進捗: {completedCount}/{CurrentTask.Checklist.Count} ({completionRate:F1}%)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"チェックリストアイテムの更新に失敗しました: {ex.Message}");
                System.Windows.MessageBox.Show($"チェックリストの更新に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
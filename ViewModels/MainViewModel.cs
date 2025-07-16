using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

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
        private AppSettings _settings;

        // タスク関連プロパティ
        [ObservableProperty]
        private ObservableCollection<PomodoroTask> tasks = new();

        [ObservableProperty]
        private PomodoroTask? currentTask;

        [ObservableProperty]
        private ObservableCollection<PomodoroTask> filteredTasks = new();

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
        private Point progressPoint = new(150, 30);

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
            IStatisticsService statisticsService, IDataPersistenceService dataPersistenceService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _settings = new AppSettings();

            // サービスからタスクを取得
            Tasks = _pomodoroService.GetTasks();
            FilteredTasks = new ObservableCollection<PomodoroTask>(Tasks);

            // タイマーイベントを購読
            SubscribeToTimerEvents();

            // ホットキーを初期化
            InitializeHotkeys();

            // タイマーに設定を適用
            _timerService.UpdateSettings(_settings);

            // フィルタリング用リストを初期化
            UpdateFilteringLists();

            // 統計情報を読み込み
            LoadTodayStatistics();

            // 初期表示更新
            UpdateProgress();
            UpdateSessionTypeText();

            // プロパティ変更監視
            PropertyChanged += OnPropertyChanged;
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
            Stop(); // 現在のタイマーを停止してリセット
        }

        /// <summary>
        /// タスク追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddTask()
        {
            var dialog = new TaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var newTask = new PomodoroTask(dialog.TaskTitle, dialog.EstimatedPomodoros)
                {
                    Description = dialog.TaskDescription,
                    Category = dialog.Category,
                    TagsText = dialog.TagsText,
                    Priority = dialog.Priority
                };
                
                _pomodoroService.AddTask(newTask);
                UpdateFilteringLists();
                ApplyFilters();
            }
        }

        /// <summary>
        /// タスク編集コマンド
        /// </summary>
        /// <param name="task">編集するタスク</param>
        [RelayCommand]
        private void EditTask(PomodoroTask task)
        {
            var dialog = new TaskDialog(task);
            if (dialog.ShowDialog() == true)
            {
                task.Title = dialog.TaskTitle;
                task.Description = dialog.TaskDescription;
                task.Category = dialog.Category;
                task.TagsText = dialog.TagsText;
                task.Priority = dialog.Priority;
                task.EstimatedPomodoros = dialog.EstimatedPomodoros;
                
                _pomodoroService.UpdateTask(task);
                UpdateFilteringLists();
                ApplyFilters();
            }
        }

        /// <summary>
        /// タスク削除コマンド
        /// </summary>
        /// <param name="task">削除するタスク</param>
        [RelayCommand]
        private void DeleteTask(PomodoroTask task)
        {
            var result = MessageBox.Show($"タスク「{task.Title}」を削除しますか？", "確認", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                _pomodoroService.RemoveTask(task);
                UpdateFilteringLists();
                ApplyFilters();
            }
        }

        /// <summary>
        /// タスク完了コマンド
        /// </summary>
        /// <param name="task">完了するタスク</param>
        [RelayCommand]
        private void CompleteTask(PomodoroTask task)
        {
            _pomodoroService.CompleteTask(task);
            _statisticsService.RecordTaskComplete(task);
            LoadTodayStatistics();
            ApplyFilters();
        }

        /// <summary>
        /// タスクインポートコマンド
        /// </summary>
        [RelayCommand]
        private async Task ImportTasks()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "タスクファイルを選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _pomodoroService.ImportTasksFromCsvAsync(openFileDialog.FileName);
                    UpdateFilteringLists();
                    ApplyFilters();
                    MessageBox.Show("タスクのインポートが完了しました。", "成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"インポートに失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// タスクエクスポートコマンド
        /// </summary>
        [RelayCommand]
        private async Task ExportTasks()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "エクスポート先を選択",
                FileName = $"tasks_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _pomodoroService.ExportTasksToCsvAsync(saveFileDialog.FileName);
                    MessageBox.Show("タスクのエクスポートが完了しました。", "成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// 設定画面を開くコマンド
        /// </summary>
        [RelayCommand]
        private void OpenSettings()
        {
            var settingsDialog = new SettingsDialog(_settings);
            if (settingsDialog.ShowDialog() == true)
            {
                // 新しい設定を適用
                UpdateSettings(settingsDialog.Settings);
                
                // UIの表示を更新
                UpdateSessionTypeText();
                UpdateProgress();
                UpdateTotalFocusTime();

                MessageBox.Show("設定が更新されました。", "設定", 
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($"設定の保存に失敗しました: {ex.Message}");
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

        #region タイマーイベントハンドラ

        private void OnTimerStarted()
        {
            IsRunning = true;
            StartPauseButtonText = "Pause";
        }

        private void OnTimerStopped()
        {
            IsRunning = false;
            StartPauseButtonText = "Start";
        }

        private void OnTimerPaused()
        {
            IsRunning = false;
            StartPauseButtonText = "Resume";
        }

        private void OnTimerResumed()
        {
            IsRunning = true;
            StartPauseButtonText = "Pause";
        }

        private void OnTimeUpdated(TimeSpan remainingTime)
        {
            TimeRemaining = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            UpdateProgress();
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
                    
                    if (CurrentTask.IsCompleted)
                    {
                        CompletedTasks++;
                    }
                }

                // 集中時間を更新
                UpdateTotalFocusTime();
            }

            // セッション完了後の状態更新
            IsRunning = false;
            StartPauseButtonText = "Start";
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
                SessionType.Work => "Work Session",
                SessionType.ShortBreak => "Short Break",
                SessionType.LongBreak => "Long Break",
                _ => "Work Session"
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

            ProgressPoint = new Point(x, y);
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
    }
}
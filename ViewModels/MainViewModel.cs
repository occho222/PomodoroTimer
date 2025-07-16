using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// ポモドーロタイマーのメインビューモデル
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ITimerService _timerService;
        private AppSettings _settings;

        // タスク関連プロパティ
        [ObservableProperty]
        private ObservableCollection<PomodoroTask> tasks = new();

        [ObservableProperty]
        private PomodoroTask? currentTask;

        // タイマー関連プロパティ
        [ObservableProperty]
        private string timeRemaining = "25:00";

        [ObservableProperty]
        private string sessionTypeText = "Work Session";

        [ObservableProperty]
        private string startPauseButtonText = "Start";

        [ObservableProperty]
        private bool isRunning = false;

        // 統計関連プロパティ
        [ObservableProperty]
        private int completedPomodoros = 0;

        [ObservableProperty]
        private int completedTasks = 0;

        [ObservableProperty]
        private string totalFocusTime = "0h 0m";

        // UI関連プロパティ
        [ObservableProperty]
        private Point progressPoint = new(150, 30);

        [ObservableProperty]
        private bool isLargeArc = false;

        [ObservableProperty]
        private double progressValue = 0;

        // ホットキー関連
        private RoutedCommand _startPauseHotkey;
        private RoutedCommand _stopHotkey;
        private RoutedCommand _skipHotkey;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pomodoroService">ポモドーロサービス</param>
        /// <param name="timerService">タイマーサービス</param>
        public MainViewModel(IPomodoroService pomodoroService, ITimerService timerService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _settings = new AppSettings();

            // サービスからタスクを取得
            Tasks = _pomodoroService.GetTasks();

            // タイマーイベントを購読
            SubscribeToTimerEvents();

            // ホットキーを初期化
            InitializeHotkeys();

            // タイマーに設定を適用
            _timerService.UpdateSettings(_settings);

            // 初期表示更新
            UpdateProgress();
            UpdateSessionTypeText();
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
                new CommandBinding(_skipHotkey, (s, e) => Skip())
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
                new InputBinding(_skipHotkey, new KeyGesture(Key.N, ModifierKeys.Control))
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
                var newTask = new PomodoroTask(dialog.TaskTitle, dialog.EstimatedPomodoros);
                _pomodoroService.AddTask(newTask);
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

                MessageBox.Show("Settings updated successfully!", "Settings", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
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
            }
        }

        /// <summary>
        /// 設定を保存する
        /// </summary>
        public void SaveSettings()
        {
            // 設定保存の実装（将来実装予定）
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
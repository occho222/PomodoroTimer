using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// ポモドーロタイマーのメインビューモデル
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ITimerService _timerService;

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

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pomodoroService">ポモドーロサービス</param>
        /// <param name="timerService">タイマーサービス</param>
        public MainViewModel(IPomodoroService pomodoroService, ITimerService timerService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));

            // サービスからタスクを取得
            Tasks = _pomodoroService.GetTasks();

            // タイマーイベントを購読
            SubscribeToTimerEvents();

            // 初期表示更新
            UpdateProgress();
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
                    // 新しいセッションを開始
                    _timerService.Start(TimeSpan.FromMinutes(25));
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
            MessageBox.Show("Settings will be implemented in the future.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void OnSessionCompleted()
        {
            // セッション完了処理
            CompletedPomodoros++;
            
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

            // 完了通知
            MessageBox.Show("Session completed! Great work.", "Pomodoro Timer", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            // 次のセッションの準備
            IsRunning = false;
            StartPauseButtonText = "Start";
        }

        #endregion

        /// <summary>
        /// 進捗表示を更新する
        /// </summary>
        private void UpdateProgress()
        {
            var totalSeconds = _timerService.SessionDuration.TotalSeconds;
            var remainingSeconds = _timerService.RemainingTime.TotalSeconds;
            
            if (totalSeconds <= 0) return;

            var progress = (totalSeconds - remainingSeconds) / totalSeconds;
            var angle = progress * 360;
            var radians = (angle - 90) * Math.PI / 180;

            var x = 150 + 120 * Math.Cos(radians);
            var y = 150 + 120 * Math.Sin(radians);

            ProgressPoint = new Point(x, y);
            IsLargeArc = angle > 180;
        }

        /// <summary>
        /// 総集中時間を更新する
        /// </summary>
        private void UpdateTotalFocusTime()
        {
            var totalMinutes = CompletedPomodoros * 25; // 1ポモドーロ = 25分として計算
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            TotalFocusTime = $"{hours}h {minutes}m";
        }
    }
}
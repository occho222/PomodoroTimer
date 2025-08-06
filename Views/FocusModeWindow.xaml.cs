using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 集中モード用の小さなウィンドウ
    /// </summary>
    public partial class FocusModeWindow : Window
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ITimerService _timerService;
        private readonly MainViewModel _mainViewModel;
        private readonly DispatcherTimer _uiUpdateTimer;
        private MainWindow _mainWindow;
        private bool _isForceClosing = false;
        private bool _isMinimized = false;

        public FocusModeWindow(IPomodoroService pomodoroService, ITimerService timerService, MainViewModel mainViewModel, MainWindow mainWindow)
        {
            InitializeComponent();
            
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
            _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

            // DataContextをMainViewModelに設定
            DataContext = _mainViewModel;

            // UIの初期化
            InitializeUI();

            // UIアップデートタイマーの設定
            _uiUpdateTimer = new DispatcherTimer();
            _uiUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            _uiUpdateTimer.Tick += UpdateUI;
            _uiUpdateTimer.Start();

            // 設定に応じて常に前面表示を設定
            UpdateAlwaysOnTop();

            // イベント購読
            SubscribeToEvents();
            
            // ウィンドウの位置設定はLoaded後に実行
            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // ウィンドウの位置を右下に設定
            SetWindowPosition();
        }

        private void InitializeUI()
        {
            UpdateCurrentTask();
            UpdateTimerDisplay();
            UpdatePlayPauseButton();
        }

        private void SetWindowPosition()
        {
            // 画面の右下に配置
            var workArea = SystemParameters.WorkArea;
            
            // ウィンドウがまだレンダリングされていない場合は設計時のサイズを使用
            double windowWidth = ActualWidth > 0 ? ActualWidth : Width;
            double windowHeight = ActualHeight > 0 ? ActualHeight : Height;
            
            Left = workArea.Right - windowWidth - 20;
            Top = workArea.Bottom - windowHeight - 20;
            
            // 画面内に収まるよう調整
            if (Left < 0) Left = 20;
            if (Top < 0) Top = 20;
        }

        private void UpdateAlwaysOnTop()
        {
            var settings = _mainViewModel.GetCurrentSettings();
            if (settings != null)
            {
                Topmost = settings.FocusModeAlwaysOnTop;
                AlwaysOnTopToggle.IsChecked = settings.FocusModeAlwaysOnTop;
            }
        }

        private void SubscribeToEvents()
        {
            _timerService.TimeUpdated += OnTimeUpdated;
            _timerService.SessionCompleted += OnSessionCompleted;
            
            // MainViewModelのPropertyChangedイベントを監視してCurrentTaskの変更を検知
            if (_mainViewModel is INotifyPropertyChanged viewModel)
            {
                viewModel.PropertyChanged += OnMainViewModelPropertyChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            _timerService.TimeUpdated -= OnTimeUpdated;
            _timerService.SessionCompleted -= OnSessionCompleted;
            
            // MainViewModelのPropertyChangedイベントの購読を解除
            if (_mainViewModel is INotifyPropertyChanged viewModel)
            {
                viewModel.PropertyChanged -= OnMainViewModelPropertyChanged;
            }
        }

        private void OnTimeUpdated(TimeSpan remainingTime)
        {
            Dispatcher.Invoke(UpdateTimerDisplay);
        }

        private void OnSessionCompleted(SessionType sessionType)
        {
            Dispatcher.Invoke(() =>
            {
                UpdatePlayPauseButton();
                // セッション完了時の処理は既存のロジックに任せる
            });
        }

        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentTask))
            {
                Dispatcher.Invoke(UpdateCurrentTask);
            }
        }

        private void UpdateUI(object? sender, EventArgs e)
        {
            UpdateTimerDisplay();
            UpdatePlayPauseButton();
            UpdateCurrentTask();
        }

        private void UpdateCurrentTask()
        {
            var currentTask = _mainViewModel.CurrentTask;
            if (currentTask != null)
            {
                // CurrentTaskTextはXAMLでBindingされているため、明示的な更新は不要
                // DataContextが正しく設定されていれば自動更新される
            }
            else
            {
                // タスクがない場合の処理は必要に応じて追加
            }
        }

        private void UpdateTimerDisplay()
        {
            // TimerTextはXAMLでTimeRemainingにバインドされているため、
            // MainViewModelのTimeRemainingプロパティが更新されれば自動で更新される
        }

        private void UpdatePlayPauseButton()
        {
            if (_timerService.IsRunning)
            {
                PlayPauseButton.Content = "⏸";
            }
            else
            {
                PlayPauseButton.Content = "▶";
            }
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timerService.IsRunning)
            {
                _mainViewModel.StartPauseCommand.Execute(null);
            }
            else
            {
                _mainViewModel.StartPauseCommand.Execute(null);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _mainViewModel.StopCommand.Execute(null);
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentTask = _mainViewModel.CurrentTask;
                if (currentTask != null)
                {
                    // 集中モード専用のタスク完了処理（MainViewModelの自動選択機能をバイパス）
                    CompleteTaskInFocusMode(currentTask);
                    
                    // 次のタスク選択ダイアログを表示
                    ShowNextTaskSelectionAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスク完了処理でエラー: {ex.Message}");
                System.Windows.MessageBox.Show($"タスク完了処理でエラーが発生しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 集中モード専用のタスク完了処理（MainViewModelの自動選択ダイアログを無効化）
        /// </summary>
        private void CompleteTaskInFocusMode(PomodoroTask task)
        {
            try
            {
                // 既に完了済みの場合は処理しない
                if (task.Status == Models.TaskStatus.Completed)
                {
                    return;
                }

                // ContinueSessionOnTaskCompleteフラグを一時的に無効化してMainViewModelのCompleteTaskを呼び出し
                var settings = _mainViewModel.GetCurrentSettings();
                var originalContinueSession = settings.ContinueSessionOnTaskComplete;
                
                try
                {
                    // 一時的にセッション継続を無効化
                    settings.ContinueSessionOnTaskComplete = false;
                    
                    // MainViewModelのCompleteTaskCommandを実行（これにより統計やデータ保存も適切に処理される）
                    _mainViewModel.CompleteTaskCommand?.Execute(task);
                }
                finally
                {
                    // 設定を元に戻す
                    settings.ContinueSessionOnTaskComplete = originalContinueSession;
                }
                
                Console.WriteLine($"[FOCUS MODE] タスク '{task.Title}' を完了しました（自動選択ダイアログは集中モードで処理）");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モードでのタスク完了処理でエラー: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 次のタスク選択ダイアログを表示
        /// </summary>
        private void ShowNextTaskSelectionAsync()
        {
            try
            {
                // 設定に応じて次のタスク選択ダイアログを表示
                var settings = _mainViewModel.GetCurrentSettings();
                if (settings?.ShowTaskSelectionDialog == true)
                {
                    // 待機中のタスクを取得
                    var waitingTasks = _mainViewModel.Tasks?.Where(t => t.Status == Models.TaskStatus.Waiting).ToList();
                    
                    if (waitingTasks?.Count > 0)
                    {
                        // TaskSelectionDialogViewModelを作成
                        var viewModel = new TaskSelectionDialogViewModel(_pomodoroService, TimeSpan.Zero);
                        var dialog = new TaskSelectionDialog(viewModel)
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        
                        var result = dialog.ShowDialog();
                        if (result == true && viewModel.SelectedTaskResult != null)
                        {
                            // 集中モード専用のタスク実行（タイマーを継続）
                            ExecuteTaskInFocusMode(viewModel.SelectedTaskResult);
                        }
                        else
                        {
                            // タスクが選択されなかった場合、集中モードを終了
                            BackToMainWindow();
                        }
                    }
                    else
                    {
                        // 待機中のタスクがない場合のメッセージ
                        var result = System.Windows.MessageBox.Show(
                            "待機中のタスクがありません。\n\n集中モードを終了してメイン画面に戻りますか？",
                            "次のタスクなし",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                            
                        if (result == MessageBoxResult.Yes)
                        {
                            BackToMainWindow();
                        }
                    }
                }
                else
                {
                    // タスク選択ダイアログを表示しない設定の場合、集中モードを継続
                    System.Windows.MessageBox.Show(
                        "タスクが完了しました。\n\n新しいタスクを開始するには、待機中タスクから選択してください。",
                        "タスク完了",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"次のタスク選択でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 集中モード専用のタスク実行処理（タイマーを継続）
        /// </summary>
        private void ExecuteTaskInFocusMode(PomodoroTask task)
        {
            try
            {
                if (task.Status == Models.TaskStatus.Waiting)
                {
                    // 既に実行中のタスクがある場合は停止
                    var currentExecutingTask = _mainViewModel.ExecutingTasks.FirstOrDefault();
                    if (currentExecutingTask != null)
                    {
                        currentExecutingTask.StopExecution();
                    }

                    // 現在実行中のタスクの経過時間を記録
                    _mainViewModel.RecordCurrentTaskElapsedTime();
                    
                    // 新しいタスクを実行中に移行（タイマーは継続）
                    task.StartExecution();
                    _mainViewModel.CurrentTask = task;
                    
                    // セッション開始時刻を記録
                    _mainViewModel.CurrentTask.CurrentSessionStartTime = DateTime.Now;
                    
                    // カンバンボードを更新
                    _mainViewModel.UpdateKanbanColumns();
                    
                    // タスクデータを保存
                    _mainViewModel.SaveDataAsync();
                    
                    Console.WriteLine($"[FOCUS MODE] タスク「{task.Title}」を実行中に設定しました（タイマー継続）。");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モードでのタスク実行処理でエラー: {ex.Message}");
                System.Windows.MessageBox.Show($"タスク実行処理でエラーが発生しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MinimizeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isMinimized = MinimizeToggle.IsChecked == true;
            ToggleMinimizedView();
        }

        private void ToggleMinimizedView()
        {
            if (_isMinimized)
            {
                // ミニマイズモード：タイマーのみ表示
                MainContent.Visibility = Visibility.Collapsed;
                FooterContent.Visibility = Visibility.Collapsed;
                MinimizedContent.Visibility = Visibility.Visible;
                
                // ウィンドウサイズを調整
                Height = 200;
                Width = 320;
                MinHeight = 180;
                MinWidth = 280;
                
                // ボタンのアイコンを変更
                MinimizeToggle.Content = "🔼";
                MinimizeToggle.ToolTip = "カード表示に戻す";
            }
            else
            {
                // 通常モード：カード全体表示
                MainContent.Visibility = Visibility.Visible;
                FooterContent.Visibility = Visibility.Visible;
                MinimizedContent.Visibility = Visibility.Collapsed;
                
                // ウィンドウサイズを元に戻す
                Height = 700;
                Width = 480;
                MinHeight = 400;
                MinWidth = 320;
                
                // ボタンのアイコンを変更
                MinimizeToggle.Content = "🔽";
                MinimizeToggle.ToolTip = "タイマーのみ表示";
            }
            
            // ウィンドウを画面内に再配置（レイアウト更新後）
            Dispatcher.BeginInvoke(new Action(() => SetWindowPosition()), DispatcherPriority.Loaded);
        }

        private void AlwaysOnTopToggle_Click(object sender, RoutedEventArgs e)
        {
            var settings = _mainViewModel.GetCurrentSettings();
            if (settings != null)
            {
                settings.FocusModeAlwaysOnTop = AlwaysOnTopToggle.IsChecked == true;
                Topmost = settings.FocusModeAlwaysOnTop;
                
                // 設定を保存
                _ = _mainViewModel.SaveSettingsAsync();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            BackToMainWindow();
        }

        private void BackToMainButton_Click(object sender, RoutedEventArgs e)
        {
            BackToMainWindow();
        }

        private void BackToMainWindow()
        {
            // 集中モードを無効化
            var settings = _mainViewModel.GetCurrentSettings();
            if (settings != null)
            {
                settings.EnableFocusMode = false;
                _ = _mainViewModel.SaveSettingsAsync();
            }

            // 集中モード表示フラグをリセット
            _mainViewModel.ResetFocusModeShowingFlag();

            // メインウィンドウを表示
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }

            // このウィンドウを閉じる
            CleanupAndForceClose();
        }

        /// <summary>
        /// クリーンアップ処理を行ってウィンドウを強制的に閉じる
        /// </summary>
        private void CleanupAndForceClose()
        {
            // 強制クローズフラグを設定
            _isForceClosing = true;
            
            // イベントの購読を解除
            UnsubscribeFromEvents();
            
            // タイマーを停止
            _uiUpdateTimer?.Stop();

            // ウィンドウを閉じる
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // 強制クローズの場合は通常の閉じる処理を実行
            if (_isForceClosing)
            {
                base.OnClosing(e);
                return;
            }
            
            // WindowStyle="None"のため、通常はここには来ない
            // 念のため閉じる処理をキャンセル
            e.Cancel = true;
        }

        // ヘッダー部分のマウスダウンイベントハンドラー
        private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // ヘッダー部分のみドラッグ移動を可能にする
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
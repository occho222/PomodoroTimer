using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using System.Reflection;
using WpfMessageBox = System.Windows.MessageBox;
using WpfApplication = System.Windows.Application;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// メインウィンドウのコードビハインド
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel? _viewModel;
        private ISystemTrayService? _systemTrayService;
        private FocusModeWindow? _focusModeWindow;

        /// <summary>
        /// デフォルトコンストラクタ（デザイナー用）
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                // バージョン情報をタイトルに設定
                SetWindowTitle();
                
                // サービスの依存関係を手動で構築
                var dataPersistenceService = new JsonDataPersistenceService();
                var pomodoroService = new PomodoroService(dataPersistenceService);
                var timerService = new TimerService();
                var statisticsService = new StatisticsService(dataPersistenceService);
                _systemTrayService = new SystemTrayService();
                
                // 設定を読み込んでからGraphServiceを初期化
                var settings = new AppSettings();
                try
                {
                    // 同期的な初期化のため、デフォルト設定を使用し、後で非同期で読み込む
                    Console.WriteLine("デフォルト設定を使用して初期化します");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"設定の初期化に失敗しました: {ex.Message}");
                    // デフォルト設定を使用
                }
                
                var graphService = new GraphService(settings);
                var taskTemplateService = new TaskTemplateService(dataPersistenceService);
                var notificationService = new NotificationService();
                
                _viewModel = new MainViewModel(pomodoroService, timerService, statisticsService, 
                    dataPersistenceService, _systemTrayService, graphService, taskTemplateService, notificationService);
                
                DataContext = _viewModel;

                // ホットキーの登録
                RegisterHotKeys();
                
                // ウィンドウイベントの購読
                StateChanged += OnWindowStateChanged;
                Closing += OnWindowClosing;
                
                Console.WriteLine("MainWindow が正常に初期化されました");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MainWindow の初期化に失敗しました: {ex.Message}");
                WpfMessageBox.Show($"アプリケーションの初期化に失敗しました:\n\n{ex.Message}\n\n詳細:\n{ex.StackTrace}", 
                    "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // 最小限の状態で起動を試行
                try
                {
                    InitializeMinimal();
                }
                catch (Exception minimalEx)
                {
                    WpfMessageBox.Show($"最小限の初期化も失敗しました: {minimalEx.Message}", 
                        "重大なエラー", MessageBoxButton.OK, MessageBoxImage.Error);
                    WpfApplication.Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// 最小限の初期化（エラー時のフォールバック）
        /// </summary>
        private void InitializeMinimal()
        {
            SetWindowTitle("（最小モード）");
            Console.WriteLine("最小限のモードで起動しました");
        }

        /// <summary>
        /// ウィンドウタイトルにバージョン情報を設定
        /// </summary>
        /// <param name="suffix">タイトルに追加する接尾辞（オプション）</param>
        private void SetWindowTitle(string suffix = "")
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version?.ToString(3) ?? "1.0.0";
                Title = $"ポモドーロタイマー - カンバンボード v{version}{suffix}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タイトル設定でエラー: {ex.Message}");
                Title = $"ポモドーロタイマー - カンバンボード{suffix}";
            }
        }

        /// <summary>
        /// ウィンドウ状態変更時の処理
        /// </summary>
        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            try
            {
                // 最小化時にシステムトレイに移動する設定の場合
                if (_viewModel != null && WindowState == WindowState.Minimized)
                {
                    var settings = _viewModel.GetCurrentSettings();
                    if (settings.MinimizeToTray)
                    {
                        _viewModel.MinimizeToTrayCommand?.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ウィンドウ状態変更の処理でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 集中モードウィンドウを表示
        /// </summary>
        public void ShowFocusMode()
        {
            try
            {
                Console.WriteLine("[DEBUG] ShowFocusMode() が呼び出されました。");
                
                if (_viewModel == null) 
                {
                    Console.WriteLine("[DEBUG] _viewModel が null です。");
                    return;
                }

                Console.WriteLine("[DEBUG] MainViewModelから既存のサービスインスタンスを取得中...");
                
                // MainViewModelから既存のサービスインスタンスを取得
                var pomodoroService = _viewModel.GetPomodoroService();
                var timerService = _viewModel.GetTimerService();

                Console.WriteLine("[DEBUG] サービスインスタンスを取得しました。");

                // 既存の集中モードウィンドウがあれば閉じる
                _focusModeWindow?.Close();

                Console.WriteLine("[DEBUG] 新しい集中モードウィンドウを作成中...");

                // 新しい集中モードウィンドウを作成
                _focusModeWindow = new FocusModeWindow(
                    pomodoroService,
                    timerService,
                    _viewModel,
                    this
                );

                Console.WriteLine("[DEBUG] メインウィンドウを非表示にします。");

                // メインウィンドウを非表示
                Hide();

                Console.WriteLine("[DEBUG] 集中モードウィンドウを表示します。");

                // 集中モードウィンドウを表示
                _focusModeWindow.Show();
                
                Console.WriteLine("[DEBUG] 集中モードウィンドウが正常に表示されました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モード表示でエラー: {ex.Message}");
                Console.WriteLine($"[DEBUG] スタックトレース: {ex.StackTrace}");
                WpfMessageBox.Show($"集中モードの表示に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 集中モードを終了してメイン画面に戻る
        /// </summary>
        public void ExitFocusMode()
        {
            try
            {
                // 集中モードウィンドウを閉じる
                _focusModeWindow?.Close();
                _focusModeWindow = null;

                // メインウィンドウを表示
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モード終了でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウが閉じられる時の処理
        /// </summary>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            try
            {
                // 設定によってはシステムトレイに最小化して終了をキャンセル
                if (_viewModel != null)
                {
                    var settings = _viewModel.GetCurrentSettings();
                    if (settings.MinimizeToTray)
                    {
                        e.Cancel = true;
                        _viewModel.MinimizeToTrayCommand?.Execute(null);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ウィンドウクロージングの処理でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ホットキーを登録する
        /// </summary>
        private void RegisterHotKeys()
        {
            try
            {
                if (_viewModel == null) return;

                // Ctrl+Space: 開始/一時停止
                var startPauseCommand = new RoutedCommand();
                startPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(startPauseCommand, (s, e) => _viewModel.StartPauseCommand?.Execute(null)));

                // Ctrl+S: 停止
                var stopCommand = new RoutedCommand();
                stopCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(stopCommand, (s, e) => _viewModel.StopCommand?.Execute(null)));

                // Ctrl+N: 次のセッション
                var skipCommand = new RoutedCommand();
                skipCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(skipCommand, (s, e) => _viewModel.SkipCommand?.Execute(null)));

                // Ctrl+T: タスク追加
                var addTaskCommand = new RoutedCommand();
                addTaskCommand.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(addTaskCommand, (s, e) => _viewModel.AddTaskCommand?.Execute(null)));

                // F1: 設定画面
                var settingsCommand = new RoutedCommand();
                settingsCommand.InputGestures.Add(new KeyGesture(Key.F1));
                CommandBindings.Add(new CommandBinding(settingsCommand, (s, e) => _viewModel.OpenSettingsCommand?.Execute(null)));

                // F2: 統計画面
                var statisticsCommand = new RoutedCommand();
                statisticsCommand.InputGestures.Add(new KeyGesture(Key.F2));
                CommandBindings.Add(new CommandBinding(statisticsCommand, (s, e) => _viewModel.OpenStatisticsCommand?.Execute(null)));

                // F3: プロジェクト・タグ管理
                var projectTagCommand = new RoutedCommand();
                projectTagCommand.InputGestures.Add(new KeyGesture(Key.F3));
                CommandBindings.Add(new CommandBinding(projectTagCommand, (s, e) => _viewModel.OpenProjectTagManagerCommand?.Execute(null)));

                // Ctrl+R: 全て更新
                var refreshCommand = new RoutedCommand();
                refreshCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(refreshCommand, (s, e) => _viewModel.RefreshAllCommand?.Execute(null)));

                // Ctrl+B: 一括選択モード切り替え
                var bulkSelectCommand = new RoutedCommand();
                bulkSelectCommand.InputGestures.Add(new KeyGesture(Key.B, ModifierKeys.Control));
                CommandBindings.Add(new CommandBinding(bulkSelectCommand, (s, e) => _viewModel.ToggleBulkSelectionCommand?.Execute(null)));


                // Escape: フィルタークリア
                var clearFiltersCommand = new RoutedCommand();
                clearFiltersCommand.InputGestures.Add(new KeyGesture(Key.Escape));
                CommandBindings.Add(new CommandBinding(clearFiltersCommand, (s, e) => _viewModel.ClearFiltersCommand?.Execute(null)));

                // Ctrl+1-4: カンバン列フォーカス移動
                for (int i = 1; i <= 4; i++)
                {
                    var columnFocusCommand = new RoutedCommand();
                    var key = (Key)Enum.Parse(typeof(Key), $"D{i}");
                    columnFocusCommand.InputGestures.Add(new KeyGesture(key, ModifierKeys.Control));
                    var columnIndex = i - 1;
                    CommandBindings.Add(new CommandBinding(columnFocusCommand, (s, e) => FocusKanbanColumn(columnIndex)));
                }
                
                Console.WriteLine("ホットキーが正常に登録されました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ホットキーの登録でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// クイックタスク入力フィールドでのキー入力処理
        /// </summary>
        private void QuickTaskInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter && _viewModel != null)
                {
                    AutoCompletePopup.IsOpen = false;
                    _viewModel.AddQuickTaskCommand?.Execute(null);
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    AutoCompletePopup.IsOpen = false;
                    e.Handled = true;
                }
                else if (e.Key == Key.Down && AutoCompletePopup.IsOpen)
                {
                    if (AutoCompleteList.Items.Count > 0)
                    {
                        AutoCompleteList.Focus();
                        AutoCompleteList.SelectedIndex = 0;
                    }
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"クイックタスク入力でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// クイックタスク入力フィールドのテキスト変更処理
        /// </summary>
        private void QuickTaskInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_viewModel != null && sender is System.Windows.Controls.TextBox textBox)
                {
                    _viewModel.UpdateFilteredTaskHistoryCommand?.Execute(textBox.Text);
                    
                    // オートコンプリート候補がある場合にポップアップを表示
                    if (!string.IsNullOrWhiteSpace(textBox.Text) && 
                        _viewModel.FilteredTaskHistory?.Count > 0)
                    {
                        AutoCompletePopup.IsOpen = true;
                    }
                    else
                    {
                        AutoCompletePopup.IsOpen = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"オートコンプリートでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// クイックタスク入力フィールドのフォーカス喪失処理
        /// </summary>
        private void QuickTaskInput_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                // 少し遅延させてポップアップを閉じる（アイテム選択の時間を確保）
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => AutoCompletePopup.IsOpen = false);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"フォーカス喪失処理でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// オートコンプリートリストの選択変更処理
        /// </summary>
        private void AutoCompleteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.ListBox listBox && listBox.SelectedItem is string selectedText && _viewModel != null)
                {
                    _viewModel.QuickTaskText = selectedText;
                    AutoCompletePopup.IsOpen = false;
                    QuickTaskInput.Focus();
                    QuickTaskInput.CaretIndex = selectedText.Length;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"オートコンプリート選択でエラー: {ex.Message}");
            }
        }


        /// <summary>
        /// カンバン列にフォーカスを移動
        /// </summary>
        private void FocusKanbanColumn(int columnIndex)
        {
            try
            {
                var kanbanGrid = FindName("KanbanGrid") as Grid;
                if (kanbanGrid?.Children.Count > columnIndex)
                {
                    var column = kanbanGrid.Children[columnIndex] as FrameworkElement;
                    column?.Focus();
                    Console.WriteLine($"カンバン列 {columnIndex + 1} にフォーカスを移動しました");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"カンバン列フォーカス移動でエラー: {ex.Message}");
            }
        }


        /// <summary>
        /// 集中モードトグルボタンのクリックイベント
        /// </summary>
        private void FocusModeToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ViewModelのEnableFocusModeプロパティは既にToggleButtonのIsCheckedとバインドされているため、
                // 設定は自動的に更新される
                Console.WriteLine($"[INFO] 集中モードが{(_viewModel?.EnableFocusMode == true ? "有効" : "無効")}に設定されました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モード設定変更でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// タスクカードのダブルクリックイベントハンドラー
        /// </summary>
        private void TaskCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount == 2 && sender is Border border && border.Tag is PomodoroTask task && _viewModel != null)
                {
                    Console.WriteLine($"[TaskCard_MouseLeftButtonDown] ダブルクリック: タスク「{task.Title}」の詳細を開きます");
                    ExecuteTaskAction("OpenTaskDetail", task);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクカードダブルクリックでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 3点メニューボタンのクリックイベントハンドラー
        /// </summary>
        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.Button button && button.ContextMenu != null)
                {
                    button.ContextMenu.PlacementTarget = button;
                    button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    button.ContextMenu.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"メニューボタンクリックでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// メニュー項目のクリックイベントハンドラー
        /// </summary>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem menuItem && _viewModel != null)
                {
                    var action = menuItem.Tag?.ToString();
                    if (string.IsNullOrEmpty(action)) return;

                    // ContextMenuから親のButtonを取得してTaskを特定
                    var contextMenu = menuItem.Parent as ContextMenu;
                    var button = contextMenu?.PlacementTarget as System.Windows.Controls.Button;
                    var task = button?.Tag as PomodoroTask;

                    if (task == null)
                    {
                        Console.WriteLine($"[MenuItem_Click] タスクが見つかりません。Action: {action}");
                        return;
                    }

                    Console.WriteLine($"[MenuItem_Click] アクション: {action}, タスク: {task.Title}");

                    // 共通化されたタスクアクション実行
                    ExecuteTaskAction(action, task);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"メニュー項目クリックでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// タスクアクションを実行する共通メソッド
        /// </summary>
        private void ExecuteTaskAction(string action, PomodoroTask task)
        {
            if (_viewModel == null) return;

            try
            {
                switch (action)
                {
                    case "StartTask":
                        _viewModel.StartTaskCommand?.Execute(task);
                        break;
                    case "ExecuteTask":
                        _viewModel.ExecuteTaskCommand?.Execute(task);
                        break;
                    case "CompleteTask":
                        _viewModel.CompleteTaskCommand?.Execute(task);
                        break;
                    case "StopTaskExecution":
                        _viewModel.StopTaskExecutionCommand?.Execute(task);
                        break;
                    case "MoveTaskToTodo":
                        _viewModel.MoveTaskToTodoCommand?.Execute(task);
                        break;
                    case "MoveCompletedTaskToExecuting":
                        _viewModel.MoveCompletedTaskToExecutingCommand?.Execute(task);
                        break;
                    case "MoveCompletedTaskToWaiting":
                        _viewModel.MoveCompletedTaskToWaitingCommand?.Execute(task);
                        break;
                    case "ResetTask":
                        _viewModel.ResetTaskCommand?.Execute(task);
                        break;
                    case "OpenTaskDetail":
                        _viewModel.OpenTaskDetailCommand?.Execute(task);
                        break;
                    case "CreateTemplateFromTask":
                        _viewModel.CreateTemplateFromTaskCommand?.Execute(task);
                        break;
                    case "DeleteTask":
                        _viewModel.DeleteTaskCommand?.Execute(task);
                        break;
                    default:
                        Console.WriteLine($"未知のアクション: {action}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクアクション実行でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウが閉じられる時の処理
        /// </summary>
        protected override async void OnClosed(EventArgs e)
        {
            try
            {
                // ViewModelのクリーンアップを実行
                if (_viewModel != null)
                {
                    await _viewModel.CleanupAsync();
                }
                
                _systemTrayService?.Dispose();
                
                Console.WriteLine("MainWindow のクリーンアップが完了しました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"クリーンアップでエラー: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }
}
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
        private HotkeyService? _hotkeyService;
        private AppSettings? _appSettings;

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
                
                var taskTemplateService = new TaskTemplateService(dataPersistenceService);
                var notificationService = new NotificationService();
                var activityExportService = new ActivityExportService(pomodoroService, statisticsService);
                var hotkeyService = new HotkeyService();
                
                _viewModel = new MainViewModel(pomodoroService, timerService, statisticsService, 
                    dataPersistenceService, _systemTrayService, taskTemplateService, notificationService, activityExportService);
                
                // ホットキーサービスをクラスフィールドとして保存
                _hotkeyService = hotkeyService;
                _appSettings = new AppSettings();
                
                DataContext = _viewModel;
                
                // ViewModelイベントの購読
                if (_viewModel != null)
                {
                    _viewModel.HotkeyReregistrationRequested += OnHotkeyReregistrationRequested;
                }
                

                // CollectionViewSourceのフィルタを設定
                var cvs = (System.Windows.Data.CollectionViewSource)FindResource("TodoTasksGroupedSource");
                if (cvs != null)
                {
                    cvs.Filter += TodoTasksGrouped_Filter;
                }

                // GroupCollapseVisibilityConverterに関数を設定（Todo用）
                PomodoroTimer.Converters.GroupCollapseVisibilityConverter.IsGroupCollapsed = 
                    (category) => collapsedTodoGroups.Contains(category);
                
                // WaitingGroupCollapseVisibilityConverterに関数を設定（Waiting用）
                PomodoroTimer.Converters.WaitingGroupCollapseVisibilityConverter.IsWaitingGroupCollapsed = 
                    (category) => collapsedWaitingGroups.Contains(category);
                
                // CompletedGroupCollapseVisibilityConverterに関数を設定（Completed用）
                PomodoroTimer.Converters.CompletedGroupCollapseVisibilityConverter.IsCompletedGroupCollapsed = 
                    (category) => collapsedCompletedGroups.Contains(category);

                // ホットキーの登録は後でOnWindowLoadedで行う
                
                // ウィンドウイベントの購読
                StateChanged += OnWindowStateChanged;
                Closing += OnWindowClosing;
                Loaded += OnWindowLoaded;
                
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
                Title = $"Pomoban v{version}{suffix}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タイトル設定でエラー: {ex.Message}");
                Title = $"Pomoban{suffix}";
            }
        }

        /// <summary>
        /// ホットキー再登録要求の処理
        /// </summary>
        private void OnHotkeyReregistrationRequested()
        {
            try
            {
                if (_viewModel != null)
                {
                    // 現在の設定を取得
                    var currentSettings = _viewModel.GetCurrentSettings();
                    
                    // 既存のホットキーをクリア
                    ClearAllHotkeys();
                    Console.WriteLine("[DEBUG] 既存のホットキーをすべて削除しました");
                    
                    // 新しい設定でホットキーを再登録
                    InitializeHotkeySystem(currentSettings);
                    Console.WriteLine("[DEBUG] 新しい設定でホットキーを再登録しました");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ホットキー再登録でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウを最前面に表示
        /// </summary>
        private void BringToFront()
        {
            try
            {
                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
                
                Activate();
                Topmost = true;
                Focus();
                
                // Topmostを一時的に解除（常に最前面にならないように）
                System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                    Topmost = false;
                }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                
                Console.WriteLine("ウィンドウを最前面に表示しました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ウィンドウ最前面表示でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ウィンドウロード完了時の処理
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 設定に応じてホットキーシステムを初期化
                if (_appSettings != null)
                {
                    InitializeHotkeySystem(_appSettings);
                    Console.WriteLine("ホットキーが正常に登録されました");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ホットキー初期化でエラー: {ex.Message}");
                // エラーが発生してもアプリケーションは継続する
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

                // 既存の集中モードウィンドウがあるかチェック
                if (_focusModeWindow == null || !_focusModeWindow.IsVisible)
                {
                    Console.WriteLine("[DEBUG] 新しい集中モードウィンドウを作成中...");

                    // 既存のウィンドウがあれば閉じる
                    _focusModeWindow?.Close();

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
                }
                else
                {
                    Console.WriteLine("[DEBUG] 既存の集中モードウィンドウを再利用します。");
                    // 既存のウィンドウを前面に表示
                    _focusModeWindow.Activate();
                    _focusModeWindow.Topmost = true;
                    _focusModeWindow.Topmost = _focusModeWindow.AlwaysOnTopToggle.IsChecked == true;
                }
                
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
        /// 集中モードウィンドウを閉じる（設定変更時）
        /// </summary>
        public void CloseFocusMode()
        {
            try
            {
                Console.WriteLine("[DEBUG] CloseFocusMode called");
                
                // 集中モードウィンドウを閉じる
                if (_focusModeWindow != null && _focusModeWindow.IsVisible)
                {
                    _focusModeWindow.Close();
                    _focusModeWindow = null;
                }

                // 集中モード表示フラグをリセット
                _viewModel?.ResetFocusModeShowingFlag();

                // メインウィンドウを表示
                if (!IsVisible)
                {
                    Show();
                    WindowState = WindowState.Normal;
                    Activate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"集中モード終了でエラー: {ex.Message}");
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

                // 集中モード表示フラグをリセット
                _viewModel?.ResetFocusModeShowingFlag();

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
        /// タスクカードのクリックイベントハンドラー
        /// </summary>
        private void TaskCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Border border && border.Tag is PomodoroTask task && _viewModel != null)
                {
                    if (e.ClickCount == 1)
                    {
                        // シングルクリック: タスク詳細をサイドパネルに表示
                        Console.WriteLine($"[TaskCard_MouseLeftButtonDown] シングルクリック: タスク「{task.Title}」の詳細を表示します");
                        _viewModel.SelectedTaskDetail = task;
                        e.Handled = true;
                    }
                    else if (e.ClickCount == 2)
                    {
                        // ダブルクリック: 詳細ダイアログを開く
                        Console.WriteLine($"[TaskCard_MouseLeftButtonDown] ダブルクリック: タスク「{task.Title}」の詳細ダイアログを開きます");
                        ExecuteTaskAction("OpenTaskDetail", task);
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクカードクリックでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// タスク詳細を閉じるボタンのクリックイベントハンドラー
        /// </summary>
        private void CloseTaskDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedTaskDetail = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスク詳細を閉じる際にエラー: {ex.Message}");
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
                    case "CopyTask":
                        _viewModel.CopyTaskCommand?.Execute(task);
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
        /// サイドバーの折りたたみ/展開を切り替える
        /// </summary>
        private void SidebarToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sidebarColumn = FindName("SidebarColumn") as ColumnDefinition;
                var sidebarPanel = FindName("SidebarPanel") as Border;
                var toggleIcon = FindName("SidebarToggleIcon") as TextBlock;

                if (sidebarColumn != null && sidebarPanel != null && toggleIcon != null)
                {
                    if (sidebarColumn.Width.Value > 50)
                    {
                        // サイドバーを折りたたむ
                        sidebarColumn.Width = new GridLength(0);
                        sidebarColumn.MinWidth = 0;
                        sidebarPanel.Visibility = Visibility.Collapsed;
                        sidebarPanel.Margin = new Thickness(0);
                        toggleIcon.Text = "▶";
                    }
                    else
                    {
                        // サイドバーを展開
                        sidebarColumn.Width = new GridLength(250);
                        sidebarColumn.MinWidth = 200;
                        sidebarPanel.Visibility = Visibility.Visible;
                        sidebarPanel.Margin = new Thickness(10, 10, 0, 10);
                        toggleIcon.Text = "◀";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"サイドバー切り替えでエラー: {ex.Message}");
            }
        }

        // 折りたたまれたグループを管理（未開始用と待機用と完了用で分離）
        private readonly HashSet<string> collapsedTodoGroups = new();
        private readonly HashSet<string> collapsedWaitingGroups = new();
        private readonly HashSet<string> collapsedCompletedGroups = new();

        /// <summary>
        /// プロジェクトグループの展開・折りたたみを切り替える
        /// </summary>
        private void GroupExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.Button button && button.Tag is string categoryName)
                {
                    var normalizedCategory = string.IsNullOrWhiteSpace(categoryName) ? "その他" : categoryName;
                    
                    // 折りたたみ状態を切り替え
                    if (collapsedTodoGroups.Contains(normalizedCategory))
                    {
                        // 展開
                        collapsedTodoGroups.Remove(normalizedCategory);
                        button.Content = "▼";
                    }
                    else
                    {
                        // 折りたたみ
                        collapsedTodoGroups.Add(normalizedCategory);
                        button.Content = "▶";
                    }
                    
                    // CollectionViewSourceを更新してVisibilityConverterを再評価
                    var cvs = (System.Windows.Data.CollectionViewSource)FindResource("TodoTasksGroupedSource");
                    if (cvs?.View != null)
                    {
                        cvs.View.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"プロジェクトグループ切り替えでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// プロジェクトグループの展開・折りたたみを切り替える（待機ボード用）
        /// </summary>
        private void WaitingGroupExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.Button button && button.Tag is string categoryName)
                {
                    var normalizedCategory = string.IsNullOrWhiteSpace(categoryName) ? "その他" : categoryName;
                    
                    // 折りたたみ状態を切り替え
                    if (collapsedWaitingGroups.Contains(normalizedCategory))
                    {
                        // 展開
                        collapsedWaitingGroups.Remove(normalizedCategory);
                        button.Content = "▼";
                    }
                    else
                    {
                        // 折りたたみ
                        collapsedWaitingGroups.Add(normalizedCategory);
                        button.Content = "▶";
                    }
                    
                    // CollectionViewSourceを更新してVisibilityConverterを再評価
                    var cvs = (System.Windows.Data.CollectionViewSource)FindResource("WaitingTasksGroupedSource");
                    if (cvs?.View != null)
                    {
                        cvs.View.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"待機グループ展開・折りたたみでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// プロジェクトグループの展開・折りたたみを切り替える（完了ボード用）
        /// </summary>
        private void CompletedGroupExpandCollapse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Controls.Button button && button.Tag is string categoryName)
                {
                    var normalizedCategory = string.IsNullOrWhiteSpace(categoryName) ? "その他" : categoryName;
                    
                    // 折りたたみ状態を切り替え
                    if (collapsedCompletedGroups.Contains(normalizedCategory))
                    {
                        // 展開
                        collapsedCompletedGroups.Remove(normalizedCategory);
                        button.Content = "▼";
                    }
                    else
                    {
                        // 折りたたみ
                        collapsedCompletedGroups.Add(normalizedCategory);
                        button.Content = "▶";
                    }
                    
                    // CollectionViewSourceを更新してVisibilityConverterを再評価
                    var cvs = (System.Windows.Data.CollectionViewSource)FindResource("CompletedTasksGroupedSource");
                    if (cvs?.View != null)
                    {
                        cvs.View.Refresh();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"完了グループ展開・折りたたみでエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// TodoTasksのグループフィルタ処理
        /// </summary>
        private void TodoTasksGrouped_Filter(object sender, System.Windows.Data.FilterEventArgs e)
        {
            // フィルタを使わずに、すべてのアイテムを表示
            // 折りたたみはItemTemplateのVisibilityで制御
            e.Accepted = true;
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
                    _viewModel.HotkeyReregistrationRequested -= OnHotkeyReregistrationRequested;
                    await _viewModel.CleanupAsync();
                }
                
                // ホットキーサービスのクリーンアップ
                _hotkeyService?.Dispose();
                
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

        /// <summary>
        /// 設定に応じてホットキーシステムを初期化
        /// </summary>
        private void InitializeHotkeySystem(AppSettings settings)
        {
            try
            {
                // グローバルホットキーを初期化（個別設定でGlobal=trueのもののみ）
                InitializeGlobalHotkeys(settings);
                
                // ローカルホットキーを動的に登録（個別設定でGlobal=falseのもののみ）
                RegisterLocalHotkeys(settings);
                
                Console.WriteLine("ホットキーシステムを初期化しました（個別設定対応）");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ホットキーシステム初期化でエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// グローバルホットキーを初期化
        /// </summary>
        private void InitializeGlobalHotkeys(AppSettings settings)
        {
            if (_hotkeyService != null && _viewModel != null)
            {
                _hotkeyService.Initialize(this);
                RegisterHotkeys(_hotkeyService, settings);
            }
        }

        /// <summary>
        /// ローカルホットキーを動的に登録
        /// </summary>
        private void RegisterLocalHotkeys(AppSettings settings)
        {
            try
            {
                // 既存のコマンドバインディングをクリア
                CommandBindings.Clear();
                
                var localHotkeyMappings = new List<(string name, string hotkey, bool enabled, bool global, ExecutedRoutedEventHandler action)>
                {
                    ("StartPause", settings.HotkeySettings.StartPauseHotkey, settings.HotkeySettings.StartPauseHotkeyEnabled, settings.HotkeySettings.StartPauseHotkeyGlobal, (s, e) => _viewModel?.StartPauseCommand?.Execute(null)),
                    ("Stop", settings.HotkeySettings.StopHotkey, settings.HotkeySettings.StopHotkeyEnabled, settings.HotkeySettings.StopHotkeyGlobal, (s, e) => _viewModel?.StopCommand?.Execute(null)),
                    ("Skip", settings.HotkeySettings.SkipHotkey, settings.HotkeySettings.SkipHotkeyEnabled, settings.HotkeySettings.SkipHotkeyGlobal, (s, e) => _viewModel?.SkipCommand?.Execute(null)),
                    ("AddTask", settings.HotkeySettings.AddTaskHotkey, settings.HotkeySettings.AddTaskHotkeyEnabled, settings.HotkeySettings.AddTaskHotkeyGlobal, (s, e) => _viewModel?.AddTaskCommand?.Execute(null)),
                    ("OpenSettings", settings.HotkeySettings.OpenSettingsHotkey, settings.HotkeySettings.OpenSettingsHotkeyEnabled, settings.HotkeySettings.OpenSettingsHotkeyGlobal, (s, e) => _viewModel?.OpenSettingsCommand?.Execute(null)),
                    ("OpenStatistics", settings.HotkeySettings.OpenStatisticsHotkey, settings.HotkeySettings.OpenStatisticsHotkeyEnabled, settings.HotkeySettings.OpenStatisticsHotkeyGlobal, (s, e) => _viewModel?.OpenStatisticsCommand?.Execute(null)),
                    ("FocusMode", settings.HotkeySettings.FocusModeHotkey, settings.HotkeySettings.FocusModeHotkeyEnabled, settings.HotkeySettings.FocusModeHotkeyGlobal, (s, e) => {
                        if (_viewModel?.EnableFocusMode == true) ShowFocusMode();
                        else CloseFocusMode();
                    }),
                    ("QuickAddTask", settings.HotkeySettings.QuickAddTaskHotkey, settings.HotkeySettings.QuickAddTaskHotkeyEnabled, settings.HotkeySettings.QuickAddTaskHotkeyGlobal, (s, e) => {
                        BringToFront();
                        _viewModel?.AddTaskCommand?.Execute(null);
                    })
                };
                
                foreach (var mapping in localHotkeyMappings)
                {
                    // 無効またはグローバル設定のものはスキップ
                    if (!mapping.enabled || mapping.global || string.IsNullOrWhiteSpace(mapping.hotkey))
                    {
                        Console.WriteLine($"ローカルホットキースキップ: {mapping.name} -> 有効={mapping.enabled}, グローバル={mapping.global}");
                        continue;
                    }
                    
                    try
                    {
                        var keyGesture = ParseKeyGesture(mapping.hotkey);
                        if (keyGesture != null)
                        {
                            var command = new RoutedCommand();
                            command.InputGestures.Add(keyGesture);
                            CommandBindings.Add(new CommandBinding(command, mapping.action));
                            Console.WriteLine($"ローカルホットキー登録: {mapping.name} -> {mapping.hotkey}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ローカルホットキー登録エラー: {mapping.name} -> {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ローカルホットキー初期化エラー: {ex.Message}");
            }
        }
        
        /// <summary>
        /// ホットキー文字列をKeyGestureに変換
        /// </summary>
        private KeyGesture? ParseKeyGesture(string hotkey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hotkey)) return null;
                
                var parts = hotkey.Split('+');
                if (parts.Length == 0) return null;
                
                var keyString = parts[^1].Trim();
                var modifiers = ModifierKeys.None;
                
                foreach (var part in parts[..^1])
                {
                    modifiers |= part.Trim().ToLower() switch
                    {
                        "ctrl" or "control" => ModifierKeys.Control,
                        "alt" => ModifierKeys.Alt,
                        "shift" => ModifierKeys.Shift,
                        "win" or "windows" => ModifierKeys.Windows,
                        _ => ModifierKeys.None
                    };
                }
                
                if (Enum.TryParse<Key>(keyString, true, out var key))
                {
                    return new KeyGesture(key, modifiers);
                }
                
                // 特殊キーの処理
                key = keyString.ToUpper() switch
                {
                    "SPACE" => Key.Space,
                    "ENTER" => Key.Enter,
                    "ESCAPE" or "ESC" => Key.Escape,
                    "TAB" => Key.Tab,
                    "BACKSPACE" => Key.Back,
                    "DELETE" or "DEL" => Key.Delete,
                    _ => Key.None
                };
                
                return key != Key.None ? new KeyGesture(key, modifiers) : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// すべてのホットキーをクリア
        /// </summary>
        private void ClearAllHotkeys()
        {
            // グローバルホットキーをクリア
            _hotkeyService?.UnregisterAllHotkeys();
            
            // ローカルホットキーをクリア
            CommandBindings.Clear();
        }

        /// <summary>
        /// ホットキーを登録
        /// </summary>
        private void RegisterHotkeys(HotkeyService hotkeyService, AppSettings settings)
        {
            if (_viewModel == null) return;

            // 実際のViewModelコマンドを実行するホットキーアクション
            var hotkeyActions = new Dictionary<string, Action>
            {
                { "StartPause", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.StartPauseCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"StartPause hotkey error: {ex.Message}"); }
                    });
                }},
                { "Stop", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.StopCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"Stop hotkey error: {ex.Message}"); }
                    });
                }},
                { "Skip", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.SkipCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"Skip hotkey error: {ex.Message}"); }
                    });
                }},
                { "AddTask", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.AddTaskCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"AddTask hotkey error: {ex.Message}"); }
                    });
                }},
                { "OpenSettings", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.OpenSettingsCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"OpenSettings hotkey error: {ex.Message}"); }
                    });
                }},
                { "OpenStatistics", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { _viewModel.OpenStatisticsCommand?.Execute(null); }
                        catch (Exception ex) { Console.WriteLine($"OpenStatistics hotkey error: {ex.Message}"); }
                    });
                }},
                { "FocusMode", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { 
                            if (_viewModel.EnableFocusMode)
                                ShowFocusMode();
                            else
                                CloseFocusMode();
                        }
                        catch (Exception ex) { Console.WriteLine($"FocusMode hotkey error: {ex.Message}"); }
                    });
                }},
                { "QuickAddTask", () => { 
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(() => {
                        try { 
                            // ウィンドウを最前面に表示
                            BringToFront();
                            
                            // タスク詳細ダイアログを開く
                            _viewModel.AddTaskCommand?.Execute(null); 
                        }
                        catch (Exception ex) { Console.WriteLine($"QuickAddTask hotkey error: {ex.Message}"); }
                    });
                }}
            };

            hotkeyService.RegisterHotkeysFromSettings(settings.HotkeySettings, hotkeyActions);
        }
    }
}
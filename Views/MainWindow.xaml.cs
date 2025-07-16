using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using WpfDragEventArgs = System.Windows.DragEventArgs;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// メインウィンドウのコードビハインド
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ISystemTrayService _systemTrayService;
        private PomodoroTask? _draggedTask;

        /// <summary>
        /// デフォルトコンストラクタ（デザイナー用）
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // サービスの依存関係を手動で構築
            var dataPersistenceService = new JsonDataPersistenceService();
            var pomodoroService = new PomodoroService(dataPersistenceService);
            var timerService = new TimerService();
            var statisticsService = new StatisticsService(dataPersistenceService);
            _systemTrayService = new SystemTrayService();
            
            _viewModel = new MainViewModel(pomodoroService, timerService, statisticsService, 
                dataPersistenceService, _systemTrayService);
            
            DataContext = _viewModel;

            // ホットキーの登録
            RegisterHotKeys();
            
            // ウィンドウイベントの購読
            StateChanged += OnWindowStateChanged;
            Closing += OnWindowClosing;
        }

        /// <summary>
        /// ウィンドウ状態変更時の処理
        /// </summary>
        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            // 最小化時にシステムトレイに移動する設定の場合
            var settings = _viewModel.GetCurrentSettings();
            if (WindowState == WindowState.Minimized && settings.MinimizeToTray)
            {
                _viewModel.MinimizeToTrayCommand.Execute(null);
            }
        }

        /// <summary>
        /// ウィンドウが閉じられる時の処理
        /// </summary>
        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            // 設定によってはシステムトレイに最小化して終了をキャンセル
            var settings = _viewModel.GetCurrentSettings();
            if (settings.MinimizeToTray)
            {
                e.Cancel = true;
                _viewModel.MinimizeToTrayCommand.Execute(null);
                return;
            }
        }

        /// <summary>
        /// ホットキーを登録する
        /// </summary>
        private void RegisterHotKeys()
        {
            // Ctrl+Space: 開始/一時停止
            var startPauseCommand = new RoutedCommand();
            startPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(startPauseCommand, (s, e) => _viewModel.StartPauseCommand.Execute(null)));

            // Ctrl+S: 停止
            var stopCommand = new RoutedCommand();
            stopCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(stopCommand, (s, e) => _viewModel.StopCommand.Execute(null)));

            // Ctrl+N: 次のセッション
            var skipCommand = new RoutedCommand();
            skipCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(skipCommand, (s, e) => _viewModel.SkipCommand.Execute(null)));

            // Ctrl+T: タスク追加
            var addTaskCommand = new RoutedCommand();
            addTaskCommand.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(addTaskCommand, (s, e) => _viewModel.AddTaskCommand.Execute(null)));

            // F1: 設定画面
            var settingsCommand = new RoutedCommand();
            settingsCommand.InputGestures.Add(new KeyGesture(Key.F1));
            CommandBindings.Add(new CommandBinding(settingsCommand, (s, e) => _viewModel.OpenSettingsCommand.Execute(null)));
        }

        #region ドラッグ&ドロップ処理

        /// <summary>
        /// タスクアイテムがマウスでクリックされた時の処理
        /// </summary>
        private void TaskItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is PomodoroTask task)
            {
                _draggedTask = task;
                DragDrop.DoDragDrop(border, task, System.Windows.DragDropEffects.Move);
            }
        }

        /// <summary>
        /// タスクアイテムがドロップされた時の処理
        /// </summary>
        private void TaskItem_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (sender is Border border && border.DataContext is PomodoroTask targetTask && _draggedTask != null)
            {
                _viewModel.ReorderTasks(_draggedTask, targetTask);
                _draggedTask = null;
            }
        }

        /// <summary>
        /// ドラッグオーバー時の処理
        /// </summary>
        private void TaskItem_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PomodoroTask)))
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        #endregion

        /// <summary>
        /// ウィンドウが閉じられる時の処理
        /// </summary>
        protected override async void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            try
            {
                await _viewModel.SaveSettingsAsync();
                _systemTrayService.Dispose();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"設定の保存に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
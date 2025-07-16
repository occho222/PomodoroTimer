using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// メインウィンドウのコードビハインド
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
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
            
            _viewModel = new MainViewModel(pomodoroService, timerService, statisticsService, dataPersistenceService);
            
            DataContext = _viewModel;

            // ホットキーの登録
            RegisterHotKeys();
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
                DragDrop.DoDragDrop(border, task, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// タスクアイテムがドロップされた時の処理
        /// </summary>
        private void TaskItem_Drop(object sender, DragEventArgs e)
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
        private void TaskItem_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PomodoroTask)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定の保存に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
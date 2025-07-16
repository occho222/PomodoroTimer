using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

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
        /// DIコンテナから注入されるコンストラクタ
        /// </summary>
        /// <param name="viewModel">メインビューモデル</param>
        public MainWindow(MainViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = _viewModel;

            // ホットキーの登録
            RegisterHotKeys();
        }

        /// <summary>
        /// デフォルトコンストラクタ（デザイナー用）
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // DIコンテナから取得を試行
                var app = (App)Application.Current;
                _viewModel = app.Services.GetRequiredService<MainViewModel>();
            }
            catch
            {
                // DIコンテナが設定されていない場合のフォールバック
                var pomodoroService = new PomodoroService();
                var timerService = new TimerService();
                _viewModel = new MainViewModel(pomodoroService, timerService);
            }
            
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
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.SaveSettings();
        }
    }
}
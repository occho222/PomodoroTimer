﻿using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
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

        /// <summary>
        /// デフォルトコンストラクタ（デザイナー用）
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
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
                    var loadedSettings = Task.Run(async () => 
                        await dataPersistenceService.LoadDataAsync<AppSettings>("settings.json")).Result;
                    if (loadedSettings != null)
                    {
                        settings = loadedSettings;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"設定の読み込みに失敗しました: {ex.Message}");
                    // デフォルト設定を使用
                }
                
                var graphService = new GraphService(settings);
                var taskTemplateService = new TaskTemplateService(dataPersistenceService);
                
                _viewModel = new MainViewModel(pomodoroService, timerService, statisticsService, 
                    dataPersistenceService, _systemTrayService, graphService, taskTemplateService);
                
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
            Title = "ポモドーロタイマー（最小モード）";
            Console.WriteLine("最小限のモードで起動しました");
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
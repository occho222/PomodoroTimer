using System.Windows;
using System.IO;
using PomodoroTimer.Helpers;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Diagnostics;
using PomodoroTimer.ViewModels;
using System.Windows.Input;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 設定ダイアログ
    /// </summary>
    public partial class SettingsDialog : Window
    {
        /// <summary>
        /// 設定データ
        /// </summary>
        public AppSettings Settings { get; private set; }

        private readonly IDataPersistenceService _dataPersistenceService;
        private GraphService? _graphService;

        public SettingsDialog(AppSettings currentSettings, IDataPersistenceService? dataPersistenceService = null)
        {
            InitializeComponent();
            
            // サービスの初期化（DIが利用できない場合はデフォルト実装を使用）
            _dataPersistenceService = dataPersistenceService ?? new JsonDataPersistenceService();
            
            // 現在の設定をコピーして編集用にする
            Settings = new AppSettings
            {
                WorkSessionMinutes = currentSettings.WorkSessionMinutes,
                ShortBreakMinutes = currentSettings.ShortBreakMinutes,
                LongBreakMinutes = currentSettings.LongBreakMinutes,
                LongBreakInterval = currentSettings.LongBreakInterval,
                ShowNotifications = currentSettings.ShowNotifications,
                MinimizeToTray = currentSettings.MinimizeToTray,
                AutoStartNextSession = currentSettings.AutoStartNextSession,
                EnableFocusMode = currentSettings.EnableFocusMode,
                FocusModeAlwaysOnTop = currentSettings.FocusModeAlwaysOnTop,
                GraphSettings = new GraphSettings
                {
                    ClientId = currentSettings.GraphSettings.ClientId,
                    TenantId = currentSettings.GraphSettings.TenantId,
                    EnableAutoLogin = currentSettings.GraphSettings.EnableAutoLogin,
                    EnableMicrosoftToDoImport = currentSettings.GraphSettings.EnableMicrosoftToDoImport,
                    EnablePlannerImport = currentSettings.GraphSettings.EnablePlannerImport,
                    EnableOutlookImport = currentSettings.GraphSettings.EnableOutlookImport,
                    LastAuthenticationTime = currentSettings.GraphSettings.LastAuthenticationTime
                },
                HotkeySettings = new HotkeySettings
                {
                    StartPauseHotkey = currentSettings.HotkeySettings.StartPauseHotkey,
                    StopHotkey = currentSettings.HotkeySettings.StopHotkey,
                    SkipHotkey = currentSettings.HotkeySettings.SkipHotkey,
                    AddTaskHotkey = currentSettings.HotkeySettings.AddTaskHotkey,
                    OpenSettingsHotkey = currentSettings.HotkeySettings.OpenSettingsHotkey,
                    OpenStatisticsHotkey = currentSettings.HotkeySettings.OpenStatisticsHotkey,
                    FocusModeHotkey = currentSettings.HotkeySettings.FocusModeHotkey,
                    QuickAddTaskHotkey = currentSettings.HotkeySettings.QuickAddTaskHotkey,
                    ExportAIAnalysisHotkey = currentSettings.HotkeySettings.ExportAIAnalysisHotkey,
                    // 有効フラグもコピー
                    StartPauseHotkeyEnabled = currentSettings.HotkeySettings.StartPauseHotkeyEnabled,
                    StopHotkeyEnabled = currentSettings.HotkeySettings.StopHotkeyEnabled,
                    SkipHotkeyEnabled = currentSettings.HotkeySettings.SkipHotkeyEnabled,
                    AddTaskHotkeyEnabled = currentSettings.HotkeySettings.AddTaskHotkeyEnabled,
                    OpenSettingsHotkeyEnabled = currentSettings.HotkeySettings.OpenSettingsHotkeyEnabled,
                    OpenStatisticsHotkeyEnabled = currentSettings.HotkeySettings.OpenStatisticsHotkeyEnabled,
                    FocusModeHotkeyEnabled = currentSettings.HotkeySettings.FocusModeHotkeyEnabled,
                    QuickAddTaskHotkeyEnabled = currentSettings.HotkeySettings.QuickAddTaskHotkeyEnabled,
                    ExportAIAnalysisHotkeyEnabled = currentSettings.HotkeySettings.ExportAIAnalysisHotkeyEnabled
                }
            };
            
            // SettingsViewModelを作成してDataContextに設定
            var settingsViewModel = new SettingsViewModel(Settings);
            DataContext = settingsViewModel;
            
            // GraphServiceを初期化
            _graphService = new GraphService(Settings);
            
            // データフォルダパスを表示
            DisplayDataFolderPath();
            
            // Microsoft Graph認証状態を更新
            UpdateAuthenticationStatus();
        }

        /// <summary>
        /// データフォルダパスを表示する
        /// </summary>
        private void DisplayDataFolderPath()
        {
            try
            {
                var dataPath = AppPaths.AppDataDirectory;
                var dataFolderText = FindName("DataFolderPath") as System.Windows.Controls.TextBlock;
                if (dataFolderText != null)
                {
                    dataFolderText.Text = dataPath;
                }
            }
            catch (Exception ex)
            {
                var dataFolderText = FindName("DataFolderPath") as System.Windows.Controls.TextBlock;
                if (dataFolderText != null)
                {
                    dataFolderText.Text = $"パスの取得に失敗しました: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// OKボタンクリック時の処理
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセルボタンクリック時の処理
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// データ初期化ボタンクリック時の処理
        /// </summary>
        private async void ResetData_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "すべてのデータ（タスク、設定、統計）を削除して初期状態に戻します。\n\nこの操作は取り消すことができません。\n\n本当に実行しますか？",
                "データ初期化の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                // 再確認
                var confirmResult = System.Windows.MessageBox.Show(
                    "最終確認: すべてのデータが削除されます。\n\n実行してもよろしいですか？",
                    "最終確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Stop,
                    MessageBoxResult.No);

                if (confirmResult == MessageBoxResult.Yes)
                {
                    try
                    {
                        // データ初期化ボタンを無効化
                        var resetButton = FindName("ResetDataButton") as System.Windows.Controls.Button;
                        if (resetButton != null)
                        {
                            resetButton.IsEnabled = false;
                            resetButton.Content = "初期化中...";
                        }

                        // データを初期化
                        await _dataPersistenceService.ResetDataAsync();

                        System.Windows.MessageBox.Show(
                            "データの初期化が完了しました。\nアプリケーションを再起動してください。",
                            "初期化完了",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // アプリケーションを終了
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(
                            $"データの初期化に失敗しました: {ex.Message}",
                            "エラー",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        // ボタンを有効化
                        var resetButton = FindName("ResetDataButton") as System.Windows.Controls.Button;
                        if (resetButton != null)
                        {
                            resetButton.IsEnabled = true;
                            resetButton.Content = "すべてのデータを初期化";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// データバックアップボタンクリック時の処理
        /// </summary>
        private async void BackupData_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "ZIPファイル (*.zip)|*.zip",
                Title = "データバックアップの保存",
                FileName = $"PomodoroTimer_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var backupButton = FindName("BackupDataButton") as System.Windows.Controls.Button;
                    if (backupButton != null)
                    {
                        backupButton.IsEnabled = false;
                        backupButton.Content = "バックアップ中...";
                    }

                    await CreateBackupAsync(saveFileDialog.FileName);

                    System.Windows.MessageBox.Show(
                        "データのバックアップが完了しました。",
                        "バックアップ完了",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"データのバックアップに失敗しました: {ex.Message}",
                        "エラー",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    var backupButton = FindName("BackupDataButton") as System.Windows.Controls.Button;
                    if (backupButton != null)
                    {
                        backupButton.IsEnabled = true;
                        backupButton.Content = "データをバックアップ";
                    }
                }
            }
        }

        /// <summary>
        /// データ復元ボタンクリック時の処理
        /// </summary>
        private async void RestoreData_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "ZIPファイル (*.zip)|*.zip",
                Title = "データバックアップの選択"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var result = System.Windows.MessageBox.Show(
                    "現在のデータは上書きされます。\n\n続行しますか？",
                    "データ復元の確認",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var restoreButton = FindName("RestoreDataButton") as System.Windows.Controls.Button;
                        if (restoreButton != null)
                        {
                            restoreButton.IsEnabled = false;
                            restoreButton.Content = "復元中...";
                        }

                        await RestoreBackupAsync(openFileDialog.FileName);

                        System.Windows.MessageBox.Show(
                            "データの復元が完了しました。\nアプリケーションを再起動してください。",
                            "復元完了",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        // アプリケーションを再起動
                        System.Diagnostics.Process.Start(Environment.ProcessPath ?? "PomodoroTimer.exe");
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(
                            $"データの復元に失敗しました: {ex.Message}",
                            "エラー",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                    finally
                    {
                        var restoreButton = FindName("RestoreDataButton") as System.Windows.Controls.Button;
                        if (restoreButton != null)
                        {
                            restoreButton.IsEnabled = true;
                            restoreButton.Content = "データを復元";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// バックアップファイルを作成する
        /// </summary>
        /// <param name="backupPath">バックアップファイルのパス</param>
        private async Task CreateBackupAsync(string backupPath)
        {
            var dataPath = AppPaths.AppDataDirectory;

            if (!Directory.Exists(dataPath))
            {
                throw new DirectoryNotFoundException("データフォルダが見つかりません。");
            }

            // ZIPファイルを作成
            await Task.Run(() =>
            {
                System.IO.Compression.ZipFile.CreateFromDirectory(dataPath, backupPath);
            });
        }

        /// <summary>
        /// バックアップファイルから復元する
        /// </summary>
        /// <param name="backupPath">バックアップファイルのパス</param>
        private async Task RestoreBackupAsync(string backupPath)
        {
            var dataPath = AppPaths.AppDataDirectory;

            await Task.Run(() =>
            {
                // 既存データを削除
                if (Directory.Exists(dataPath))
                {
                    Directory.Delete(dataPath, true);
                }

                // バックアップから復元
                System.IO.Compression.ZipFile.ExtractToDirectory(backupPath, dataPath);
            });
        }

        /// <summary>
        /// Microsoft Graph認証状態を更新する
        /// </summary>
        private void UpdateAuthenticationStatus()
        {
            try
            {
                var statusText = FindName("AuthenticationStatusText") as System.Windows.Controls.TextBlock;
                var lastAuthText = FindName("LastAuthTimeText") as System.Windows.Controls.TextBlock;

                if (statusText != null && lastAuthText != null)
                {
                    if (_graphService?.IsAuthenticated == true)
                    {
                        statusText.Text = "認証済み";
                        statusText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        statusText.Text = "未認証";
                        statusText.Foreground = System.Windows.Media.Brushes.Red;
                    }

                    if (Settings.GraphSettings.LastAuthenticationTime.HasValue)
                    {
                        lastAuthText.Text = $"最終認証: {Settings.GraphSettings.LastAuthenticationTime.Value:yyyy/MM/dd HH:mm}";
                    }
                    else
                    {
                        lastAuthText.Text = "認証履歴なし";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"認証状態の更新に失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// Microsoft Graph接続テストボタンクリック時の処理
        /// </summary>
        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            var testButton = FindName("TestConnectionButton") as System.Windows.Controls.Button;
            if (testButton == null) return;

            try
            {
                testButton.IsEnabled = false;
                testButton.Content = "接続中...";

                // 現在の設定でGraphServiceを再初期化
                _graphService = new GraphService(Settings);

                // 認証テスト
                bool isAuthenticated = await _graphService.AuthenticateAsync();

                if (isAuthenticated)
                {
                    Settings.GraphSettings.LastAuthenticationTime = DateTime.Now;
                    System.Windows.MessageBox.Show("Microsoft Graphへの接続に成功しました。", "接続テスト", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Microsoft Graphへの接続に失敗しました。クライアントIDとテナントIDを確認してください。", "接続テスト", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                UpdateAuthenticationStatus();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"接続テストに失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                testButton.IsEnabled = true;
                testButton.Content = "接続テスト";
            }
        }

        /// <summary>
        /// Azure ポータルを開くボタンクリック時の処理
        /// </summary>
        private void OpenAzurePortal_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var azurePortalUrl = "https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationsListBlade";
                Process.Start(new ProcessStartInfo
                {
                    FileName = azurePortalUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Azure ポータルを開けませんでした: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// ホットキーテキストボックスのキー入力処理
        /// </summary>
        private void HotkeyTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
            
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            var modifiers = new List<string>();
            var key = "";

            // モディファイアキーをチェック
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                modifiers.Add("Ctrl");
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                modifiers.Add("Alt");
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                modifiers.Add("Shift");
            if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
                modifiers.Add("Win");

            // キーを取得
            if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl &&
                e.Key != Key.LeftAlt && e.Key != Key.RightAlt &&
                e.Key != Key.LeftShift && e.Key != Key.RightShift &&
                e.Key != Key.LWin && e.Key != Key.RWin)
            {
                key = GetKeyName(e.Key);
            }

            // ショートカットキー文字列を作成
            if (!string.IsNullOrEmpty(key))
            {
                var hotkeyString = string.Join("+", modifiers.Concat(new[] { key }));
                textBox.Text = hotkeyString;
                
                // データバインディングを手動で更新
                var binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                binding?.UpdateSource();
            }
        }

        /// <summary>
        /// キーから表示名を取得
        /// </summary>
        private string GetKeyName(Key key)
        {
            return key switch
            {
                Key.Space => "Space",
                Key.Enter => "Enter",
                Key.Escape => "Escape",
                Key.Tab => "Tab",
                Key.Back => "Backspace",
                Key.Delete => "Delete",
                Key.Insert => "Insert",
                Key.Home => "Home",
                Key.End => "End",
                Key.PageUp => "PageUp",
                Key.PageDown => "PageDown",
                Key.Up => "Up",
                Key.Down => "Down",
                Key.Left => "Left",
                Key.Right => "Right",
                Key.F1 => "F1",
                Key.F2 => "F2",
                Key.F3 => "F3",
                Key.F4 => "F4",
                Key.F5 => "F5",
                Key.F6 => "F6",
                Key.F7 => "F7",
                Key.F8 => "F8",
                Key.F9 => "F9",
                Key.F10 => "F10",
                Key.F11 => "F11",
                Key.F12 => "F12",
                _ => key.ToString()
            };
        }

        /// <summary>
        /// ホットキーをデフォルトに戻すボタンクリック時の処理
        /// </summary>
        private void ResetHotkeys_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "すべてのショートカットキーをデフォルト設定に戻しますか？",
                "確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (DataContext is SettingsViewModel viewModel)
                {
                    viewModel.ResetHotkeysToDefault();
                    System.Windows.MessageBox.Show("ショートカットキーをデフォルト設定に戻しました。", "完了", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// ホットキーテストボタンクリック時の処理
        /// </summary>
        private void TestHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                // 重複チェック
                if (!viewModel.ValidateHotkeys())
                {
                    System.Windows.MessageBox.Show("重複しているショートカットキーがあります。", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 無効なホットキーチェック
                var invalidHotkeys = viewModel.GetInvalidHotkeys();
                if (invalidHotkeys.Any())
                {
                    var invalidList = string.Join("\n", invalidHotkeys.Select(h => $"• {h.DisplayName}: {h.Hotkey}"));
                    System.Windows.MessageBox.Show($"無効なショートカットキーがあります:\n\n{invalidList}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                System.Windows.MessageBox.Show("すべてのショートカットキーが有効です。", "テスト結果", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
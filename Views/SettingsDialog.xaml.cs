using System.Windows;
using System.IO;
using PomodoroTimer.Models;
using PomodoroTimer.Services;

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
                AutoStartNextSession = currentSettings.AutoStartNextSession
            };
            
            DataContext = Settings;
            
            // データフォルダパスを表示
            DisplayDataFolderPath();
        }

        /// <summary>
        /// データフォルダパスを表示する
        /// </summary>
        private void DisplayDataFolderPath()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dataPath = Path.Combine(appDataPath, "PomodoroTimer");
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
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dataPath = Path.Combine(appDataPath, "PomodoroTimer");

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
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dataPath = Path.Combine(appDataPath, "PomodoroTimer");

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
    }
}
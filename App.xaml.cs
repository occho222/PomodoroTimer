using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Views;
using System.Windows;

namespace PomodoroTimer
{
    /// <summary>
    /// アプリケーションエントリポイント
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// アプリケーション開始時の処理
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // グローバル例外ハンドラーを設定
            DispatcherUnhandledException += (sender, ex) =>
            {
                var message = $"予期しないエラーが発生しました: {ex.Exception.Message}";
                if (ex.Exception.InnerException != null)
                {
                    message += $"\n詳細: {ex.Exception.InnerException.Message}";
                }
                
                MessageBox.Show(message, "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };

            // アプリケーションドメインレベルの例外ハンドラー
            AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
            {
                MessageBox.Show($"重大なエラーが発生しました: {ex.ExceptionObject}", "重大なエラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };

            try
            {
                // メインウィンドウを作成・設定（DIコンテナを使わずにシンプルに）
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                
                // ウィンドウを表示
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                // 起動時エラーのデバッグ情報を表示
                var errorMessage = $"アプリケーションの起動に失敗しました: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n詳細: {ex.InnerException.Message}";
                }
                
                MessageBox.Show(errorMessage, "起動エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // アプリケーションを終了
                Shutdown();
            }
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
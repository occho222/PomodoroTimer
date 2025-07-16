using Microsoft.Extensions.DependencyInjection;
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
        private ServiceProvider? _serviceProvider;

        /// <summary>
        /// サービスプロバイダー
        /// </summary>
        public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Services not configured");

        /// <summary>
        /// アプリケーション開始時の処理
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            // グローバル例外ハンドラーを設定
            DispatcherUnhandledException += (sender, ex) =>
            {
                var message = $"Unhandled exception: {ex.Exception.Message}";
                if (ex.Exception.InnerException != null)
                {
                    message += $"\nInner Exception: {ex.Exception.InnerException.Message}";
                }
                
                MessageBox.Show(message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };

            // アプリケーションドメインレベルの例外ハンドラー
            AppDomain.CurrentDomain.UnhandledException += (sender, ex) =>
            {
                MessageBox.Show($"Unhandled domain exception: {ex.ExceptionObject}", "Critical Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            };

            try
            {
                // DIコンテナの設定
                ConfigureServices();

                // メインウィンドウを作成・設定
                var mainWindow = Services.GetRequiredService<MainWindow>();
                MainWindow = mainWindow;
                
                // ウィンドウを表示
                mainWindow.Show();

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                // 起動時エラーのデバッグ情報を表示
                var errorMessage = $"Application startup failed: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }
                errorMessage += $"\n\nStack Trace:\n{ex.StackTrace}";
                
                MessageBox.Show(errorMessage, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // アプリケーションを終了
                Shutdown();
            }
        }

        /// <summary>
        /// サービスの設定
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // サービスの登録
            services.AddSingleton<IPomodoroService, PomodoroService>();
            services.AddSingleton<ITimerService, TimerService>();

            // ViewModelの登録
            services.AddSingleton<MainViewModel>();

            // Viewの登録（ViewModelを注入）
            services.AddTransient<MainWindow>(serviceProvider => 
            {
                var viewModel = serviceProvider.GetRequiredService<MainViewModel>();
                return new MainWindow(viewModel);
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}
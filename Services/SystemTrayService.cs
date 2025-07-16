using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// システムトレイサービスの実装
    /// </summary>
    public class SystemTrayService : ISystemTrayService, IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private Window? _mainWindow;
        private bool _isDisposed = false;

        public SystemTrayService()
        {
            _mainWindow = System.Windows.Application.Current?.MainWindow;
        }

        public void Initialize()
        {
            CreateNotifyIcon();
        }

        public void MinimizeToTray()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Hide();
                ShowTrayIcon();
            }
        }

        public void RestoreFromTray()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Show();
                _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
                HideTrayIcon();
            }
        }

        public void ShowTrayIcon()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = true;
            }
        }

        public void HideTrayIcon()
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
            }
        }

        public void ShowBalloonTip(string title, string message, ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (_notifyIcon != null && _notifyIcon.Visible)
            {
                _notifyIcon.ShowBalloonTip(3000, title, message, icon);
            }
        }

        public void UpdateTimerStatus(bool isRunning, TimeSpan remainingTime)
        {
            if (_notifyIcon == null) return;

            // ツールチップテキストを更新
            var statusText = isRunning ? "動作中" : "停止中";
            var timeText = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            _notifyIcon.Text = $"ポモドーロタイマー - {statusText} ({timeText})";

            // アイコンを状態に応じて変更
            UpdateTrayIcon(isRunning);
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDefaultIcon(),
                Text = "ポモドーロタイマー",
                Visible = false
            };

            // アイコンがダブルクリックされた時のイベント
            _notifyIcon.DoubleClick += (sender, args) => RestoreFromTray();

            // 右クリックメニューイベント
            _notifyIcon.MouseClick += OnNotifyIconClick;
        }

        private void OnNotifyIconClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowContextMenu();
            }
        }

        private void ShowContextMenu()
        {
            // 簡単なコンテキストメニューの代替実装
            var result = System.Windows.MessageBox.Show(
                "ポモドーロタイマーを表示しますか？\n\n「はい」: 表示\n「いいえ」: そのまま\n「キャンセル」: 終了", 
                "ポモドーロタイマー", 
                MessageBoxButton.YesNoCancel, 
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    RestoreFromTray();
                    break;
                case MessageBoxResult.Cancel:
                    ExitApplication();
                    break;
                // MessageBoxResult.No の場合は何もしない
            }
        }

        private Icon CreateDefaultIcon()
        {
            // デフォルトアイコンを作成（簡単な円形）
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(Brushes.Red, 2, 2, 12, 12);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private Icon CreateRunningIcon()
        {
            // 動作中アイコンを作成（緑色の円形）
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.FillEllipse(Brushes.Green, 2, 2, 12, 12);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private void UpdateTrayIcon(bool isRunning)
        {
            if (_notifyIcon == null) return;

            try
            {
                var oldIcon = _notifyIcon.Icon;
                _notifyIcon.Icon = isRunning ? CreateRunningIcon() : CreateDefaultIcon();
                oldIcon?.Dispose();
            }
            catch (Exception ex)
            {
                // アイコン更新エラーはログのみ記録
                System.Diagnostics.Debug.WriteLine($"トレイアイコン更新エラー: {ex.Message}");
            }
        }

        private void ExitApplication()
        {
            System.Windows.Application.Current?.Shutdown();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _notifyIcon?.Dispose();
                _isDisposed = true;
            }
        }

        ~SystemTrayService()
        {
            Dispose(false);
        }
    }
}
using System.Drawing;
using System.Windows.Forms;
using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// システムトレイサービスの実装
    /// </summary>
    public class SystemTrayService : ISystemTrayService, IDisposable
    {
        private NotifyIcon? _notifyIcon;
        private bool _disposed = false;

        public void Initialize()
        {
            try
            {
                _notifyIcon = new NotifyIcon
                {
                    Icon = SystemIcons.Application, // デフォルトアイコンを使用
                    Text = "ポモドーロタイマー",
                    Visible = false
                };

                // コンテキストメニューを作成
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("表示", null, (s, e) => RestoreFromTray());
                contextMenu.Items.Add("-"); // セパレーター
                contextMenu.Items.Add("終了", null, (s, e) => System.Windows.Application.Current.Shutdown());

                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"システムトレイの初期化に失敗しました: {ex.Message}");
            }
        }

        public void MinimizeToTray()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                    System.Windows.Application.Current.MainWindow.Hide();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"システムトレイへの最小化に失敗しました: {ex.Message}");
            }
        }

        public void RestoreFromTray()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    System.Windows.Application.Current.MainWindow.Show();
                    System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                    System.Windows.Application.Current.MainWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"システムトレイからの復元に失敗しました: {ex.Message}");
            }
        }

        public void ShowTrayIcon()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"システムトレイアイコンの表示に失敗しました: {ex.Message}");
            }
        }

        public void HideTrayIcon()
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"システムトレイアイコンの非表示に失敗しました: {ex.Message}");
            }
        }

        public void UpdateTimerStatus(bool isRunning, TimeSpan remainingTime)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    var status = isRunning ? "実行中" : "停止中";
                    _notifyIcon.Text = $"ポモドーロタイマー - {status} ({remainingTime.Minutes:D2}:{remainingTime.Seconds:D2})";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タイマー状態の更新に失敗しました: {ex.Message}");
            }
        }

        public void ShowBalloonTip(string title, string text, ToolTipIcon icon)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(3000, title, text, icon);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"バルーン通知の表示に失敗しました: {ex.Message}");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _notifyIcon?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
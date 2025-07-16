using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// �V�X�e���g���C�T�[�r�X�̎���
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

            // �c�[���`�b�v�e�L�X�g���X�V
            var statusText = isRunning ? "���쒆" : "��~��";
            var timeText = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            _notifyIcon.Text = $"�|���h�[���^�C�}�[ - {statusText} ({timeText})";

            // �A�C�R������Ԃɉ����ĕύX
            UpdateTrayIcon(isRunning);
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = CreateDefaultIcon(),
                Text = "�|���h�[���^�C�}�[",
                Visible = false
            };

            // �A�C�R�����_�u���N���b�N���ꂽ���̃C�x���g
            _notifyIcon.DoubleClick += (sender, args) => RestoreFromTray();

            // �E�N���b�N���j���[�C�x���g
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
            // �ȒP�ȃR���e�L�X�g���j���[�̑�֎���
            var result = System.Windows.MessageBox.Show(
                "�|���h�[���^�C�}�[��\�����܂����H\n\n�u�͂��v: �\��\n�u�������v: ���̂܂�\n�u�L�����Z���v: �I��", 
                "�|���h�[���^�C�}�[", 
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
                // MessageBoxResult.No �̏ꍇ�͉������Ȃ�
            }
        }

        private Icon CreateDefaultIcon()
        {
            // �f�t�H���g�A�C�R�����쐬�i�ȒP�ȉ~�`�j
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
            // ���쒆�A�C�R�����쐬�i�ΐF�̉~�`�j
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
                // �A�C�R���X�V�G���[�̓��O�̂݋L�^
                System.Diagnostics.Debug.WriteLine($"�g���C�A�C�R���X�V�G���[: {ex.Message}");
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
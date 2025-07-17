using System.Drawing;
using System.Windows.Forms;
using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// �V�X�e���g���C�T�[�r�X�̎���
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
                    Icon = SystemIcons.Application, // �f�t�H���g�A�C�R�����g�p
                    Text = "�|���h�[���^�C�}�[",
                    Visible = false
                };

                // �R���e�L�X�g���j���[���쐬
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("�\��", null, (s, e) => RestoreFromTray());
                contextMenu.Items.Add("-"); // �Z�p���[�^�[
                contextMenu.Items.Add("�I��", null, (s, e) => System.Windows.Application.Current.Shutdown());

                _notifyIcon.ContextMenuStrip = contextMenu;
                _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"�V�X�e���g���C�̏������Ɏ��s���܂���: {ex.Message}");
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
                Console.WriteLine($"�V�X�e���g���C�ւ̍ŏ����Ɏ��s���܂���: {ex.Message}");
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
                Console.WriteLine($"�V�X�e���g���C����̕����Ɏ��s���܂���: {ex.Message}");
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
                Console.WriteLine($"�V�X�e���g���C�A�C�R���̕\���Ɏ��s���܂���: {ex.Message}");
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
                Console.WriteLine($"�V�X�e���g���C�A�C�R���̔�\���Ɏ��s���܂���: {ex.Message}");
            }
        }

        public void UpdateTimerStatus(bool isRunning, TimeSpan remainingTime)
        {
            try
            {
                if (_notifyIcon != null)
                {
                    var status = isRunning ? "���s��" : "��~��";
                    _notifyIcon.Text = $"�|���h�[���^�C�}�[ - {status} ({remainingTime.Minutes:D2}:{remainingTime.Seconds:D2})";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"�^�C�}�[��Ԃ̍X�V�Ɏ��s���܂���: {ex.Message}");
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
                Console.WriteLine($"�o���[���ʒm�̕\���Ɏ��s���܂���: {ex.Message}");
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
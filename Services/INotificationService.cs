using System.Media;
using System.Windows;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// �ʒm�T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// �f�X�N�g�b�v�ʒm��\������
        /// </summary>
        /// <param name="title">�ʒm�̃^�C�g��</param>
        /// <param name="message">�ʒm�̃��b�Z�[�W</param>
        void ShowDesktopNotification(string title, string message);

        /// <summary>
        /// �����ʒm���Đ�����
        /// </summary>
        void PlayNotificationSound();

        /// <summary>
        /// �ݒ���X�V����
        /// </summary>
        /// <param name="enableSound">�����ʒm��L���ɂ��邩</param>
        /// <param name="enableDesktop">�f�X�N�g�b�v�ʒm��L���ɂ��邩</param>
        void UpdateSettings(bool enableSound, bool enableDesktop);
    }

    /// <summary>
    /// �ʒm�T�[�r�X�̎���
    /// </summary>
    public class NotificationService : INotificationService
    {
        private bool _enableSoundNotification = true;
        private bool _enableDesktopNotification = true;
        private SoundPlayer? _soundPlayer;

        public NotificationService()
        {
            InitializeSound();
        }

        private void InitializeSound()
        {
            try
            {
                // �V�X�e���̒ʒm�����g�p
                _soundPlayer = new SoundPlayer();
            }
            catch
            {
                // �T�E���h�̏������Ɏ��s�����ꍇ�� null �̂܂�
                _soundPlayer = null;
            }
        }

        public void UpdateSettings(bool enableSound, bool enableDesktop)
        {
            _enableSoundNotification = enableSound;
            _enableDesktopNotification = enableDesktop;
        }

        public void ShowDesktopNotification(string title, string message)
        {
            if (!_enableDesktopNotification) return;

            try
            {
                // WPF MessageBox ���g�p�i�����I�ɂ�Toast�ʒm�ɕύX�\�j
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(message, title, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch
            {
                // �ʒm�\���Ɏ��s�����ꍇ�͖���
            }
        }

        public void PlayNotificationSound()
        {
            if (!_enableSoundNotification) return;

            try
            {
                // �V�X�e���T�E���h���Đ�
                SystemSounds.Beep.Play();
            }
            catch
            {
                // �T�E���h�Đ��Ɏ��s�����ꍇ�͖���
            }
        }
    }
}
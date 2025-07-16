using System.Media;
using System.Windows;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// 通知サービスのインターフェース
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// デスクトップ通知を表示する
        /// </summary>
        /// <param name="title">通知のタイトル</param>
        /// <param name="message">通知のメッセージ</param>
        void ShowDesktopNotification(string title, string message);

        /// <summary>
        /// 音声通知を再生する
        /// </summary>
        void PlayNotificationSound();

        /// <summary>
        /// 設定を更新する
        /// </summary>
        /// <param name="enableSound">音声通知を有効にするか</param>
        /// <param name="enableDesktop">デスクトップ通知を有効にするか</param>
        void UpdateSettings(bool enableSound, bool enableDesktop);
    }

    /// <summary>
    /// 通知サービスの実装
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
                // システムの通知音を使用
                _soundPlayer = new SoundPlayer();
            }
            catch
            {
                // サウンドの初期化に失敗した場合は null のまま
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
                // WPF MessageBox を使用（将来的にはToast通知に変更可能）
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(message, title, 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
            catch
            {
                // 通知表示に失敗した場合は無視
            }
        }

        public void PlayNotificationSound()
        {
            if (!_enableSoundNotification) return;

            try
            {
                // システムサウンドを再生
                SystemSounds.Beep.Play();
            }
            catch
            {
                // サウンド再生に失敗した場合は無視
            }
        }
    }
}
using System.Windows;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// �V�X�e���g���C�T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface ISystemTrayService
    {
        /// <summary>
        /// �V�X�e���g���C������������
        /// </summary>
        void Initialize();

        /// <summary>
        /// �E�B���h�E���V�X�e���g���C�ɍŏ�������
        /// </summary>
        void MinimizeToTray();

        /// <summary>
        /// �V�X�e���g���C����E�B���h�E�𕜌�����
        /// </summary>
        void RestoreFromTray();

        /// <summary>
        /// �V�X�e���g���C�A�C�R����\������
        /// </summary>
        void ShowTrayIcon();

        /// <summary>
        /// �V�X�e���g���C�A�C�R�����\���ɂ���
        /// </summary>
        void HideTrayIcon();

        /// <summary>
        /// �o���[���ʒm��\������
        /// </summary>
        /// <param name="title">�ʒm�^�C�g��</param>
        /// <param name="message">�ʒm���b�Z�[�W</param>
        /// <param name="icon">�ʒm�A�C�R��</param>
        void ShowBalloonTip(string title, string message, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info);

        /// <summary>
        /// �V�X�e���g���C�̃��\�[�X���������
        /// </summary>
        void Dispose();

        /// <summary>
        /// �^�C�}�[��Ԃ��X�V����i�A�C�R���ύX�p�j
        /// </summary>
        /// <param name="isRunning">�^�C�}�[�����쒆���ǂ���</param>
        /// <param name="remainingTime">�c�莞��</param>
        void UpdateTimerStatus(bool isRunning, TimeSpan remainingTime);
    }
}
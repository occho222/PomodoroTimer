namespace PomodoroTimer.Models
{
    /// <summary>
    /// �A�v���P�[�V�����ݒ胂�f��
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// ��ƃZ�b�V�������ԁi���j
        /// </summary>
        public int WorkSessionMinutes { get; set; } = 25;

        /// <summary>
        /// �Z���x�e���ԁi���j
        /// </summary>
        public int ShortBreakMinutes { get; set; } = 5;

        /// <summary>
        /// �����x�e���ԁi���j
        /// </summary>
        public int LongBreakMinutes { get; set; } = 15;

        /// <summary>
        /// �����x�e�O�̃|���h�[����
        /// </summary>
        public int PomodorosBeforeLongBreak { get; set; } = 4;

        /// <summary>
        /// �����ʒm��L���ɂ��邩�ǂ���
        /// </summary>
        public bool EnableSoundNotification { get; set; } = true;

        /// <summary>
        /// �f�X�N�g�b�v�ʒm��L���ɂ��邩�ǂ���
        /// </summary>
        public bool EnableDesktopNotification { get; set; } = true;

        /// <summary>
        /// �����Ŏ��̃Z�b�V�������J�n���邩�ǂ���
        /// </summary>
        public bool AutoStartNextSession { get; set; } = false;

        /// <summary>
        /// �_�[�N�e�[�}���g�p���邩�ǂ���
        /// </summary>
        public bool UseDarkTheme { get; set; } = false;

        /// <summary>
        /// ���v�f�[�^��ۑ����邩�ǂ���
        /// </summary>
        public bool SaveStatistics { get; set; } = true;

        /// <summary>
        /// �ŏ������ɃV�X�e���g���C�Ɋi�[���邩�ǂ���
        /// </summary>
        public bool MinimizeToTray { get; set; } = false;
    }
}
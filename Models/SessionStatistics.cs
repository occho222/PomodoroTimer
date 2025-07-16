namespace PomodoroTimer.Models
{
    /// <summary>
    /// �Z�b�V�������v���
    /// </summary>
    public class SessionStatistics
    {
        /// <summary>
        /// ���t
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>
        /// �����|���h�[����
        /// </summary>
        public int CompletedPomodoros { get; set; }

        /// <summary>
        /// �����^�X�N��
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// ���W�����ԁi���j
        /// </summary>
        public int TotalFocusTimeMinutes { get; set; }

        /// <summary>
        /// �J�n����
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// �I������
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// ���f��
        /// </summary>
        public int InterruptionCount { get; set; }

        /// <summary>
        /// ���W�����Ԃ����Ԃƕ��̕�����Ŏ擾����
        /// </summary>
        public string TotalFocusTimeFormatted
        {
            get
            {
                var hours = TotalFocusTimeMinutes / 60;
                var minutes = TotalFocusTimeMinutes % 60;
                return $"{hours}����{minutes}��";
            }
        }

        /// <summary>
        /// ���σ|���h�[�����Ԃ��擾����
        /// </summary>
        public double AveragePomodoroTimeMinutes => CompletedPomodoros > 0 ? 
            (double)TotalFocusTimeMinutes / CompletedPomodoros : 0;
    }
}
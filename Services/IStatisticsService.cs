using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ���v���T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// �������v���擾����
        /// </summary>
        /// <param name="date">�Ώۓ�</param>
        /// <returns>�������v���</returns>
        DailyStatistics GetDailyStatistics(DateTime date);

        /// <summary>
        /// �T�����v���擾����
        /// </summary>
        /// <param name="weekStart">�T�̊J�n��</param>
        /// <returns>�T�����v���</returns>
        WeeklyStatistics GetWeeklyStatistics(DateTime weekStart);

        /// <summary>
        /// �|���h�[���������L�^����
        /// </summary>
        /// <param name="task">���������^�X�N</param>
        /// <param name="sessionDurationMinutes">�Z�b�V�������ԁi���j</param>
        void RecordPomodoroComplete(PomodoroTask task, int sessionDurationMinutes);

        /// <summary>
        /// �^�X�N�������L�^����
        /// </summary>
        /// <param name="task">���������^�X�N</param>
        void RecordTaskComplete(PomodoroTask task);

        /// <summary>
        /// �v���W�F�N�g�ʓ��v���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>�v���W�F�N�g�ʓ��v�̃f�B�N�V���i��</returns>
        Dictionary<string, ProjectStatistics> GetProjectStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// ���v�f�[�^���N���A����
        /// </summary>
        void ClearStatistics();

        /// <summary>
        /// ���v�f�[�^��ۑ�����
        /// </summary>
        Task SaveStatisticsAsync();

        /// <summary>
        /// ���v�f�[�^��ǂݍ���
        /// </summary>
        Task LoadStatisticsAsync();
    }
}
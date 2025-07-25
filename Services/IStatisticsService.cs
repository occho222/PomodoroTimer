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
        List<DailyStatistics> GetWeeklyStatistics(DateTime weekStart);

        /// <summary>
        /// �S���Ԃ̓��v���擾����
        /// </summary>
        /// <returns>�S���ԓ��v���</returns>
        AllTimeStatistics GetAllTimeStatistics();

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
        /// �^�X�N�������L�^���������i���������֖߂����ꍇ�j
        /// </summary>
        /// <param name="task">���������֖߂����^�X�N</param>
        void UndoTaskComplete(PomodoroTask task);

        /// <summary>
        /// �v���W�F�N�g�ʓ��v���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>�v���W�F�N�g�ʓ��v�̃f�B�N�V���i��</returns>
        Dictionary<string, ProjectStatistics> GetProjectStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// �^�O�ʓ��v���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>�^�O�ʓ��v�̃f�B�N�V���i��</returns>
        Dictionary<string, TagStatistics> GetTagStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// �J�e�S���ʂ̍�Ǝ��ԃ����L���O���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <param name="topCount">��ʉ��ʂ܂Ŏ擾���邩</param>
        /// <returns>��Ǝ��ԏ��̃J�e�S�������L���O</returns>
        List<(string Category, int FocusMinutes, int CompletedPomodoros)> GetCategoryRanking(
            DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// �^�O�ʂ̍�Ǝ��ԃ����L���O���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <param name="topCount">��ʉ��ʂ܂Ŏ擾���邩</param>
        /// <returns>��Ǝ��ԏ��̃^�O�����L���O</returns>
        List<(string Tag, int FocusMinutes, int CompletedPomodoros)> GetTagRanking(
            DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// �T�����|�[�g���擾����
        /// </summary>
        /// <param name="weekStart">�T�̊J�n��</param>
        /// <returns>�T�����|�[�g</returns>
        WeeklyReport GetWeeklyReport(DateTime weekStart);

        /// <summary>
        /// ���ԓ��v���擾����
        /// </summary>
        /// <param name="year">�N</param>
        /// <param name="month">��</param>
        /// <returns>���ԓ��v</returns>
        MonthlyStatistics GetMonthlyStatistics(int year, int month);

        /// <summary>
        /// ���Y���g�����h���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>���Y���g�����h�f�[�^</returns>
        List<ProductivityTrend> GetProductivityTrend(DateTime startDate, DateTime endDate);

        /// <summary>
        /// ���ԑѕʍ�ƕ��͂��擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>���ԑѕʍ�ƕ���</returns>
        Dictionary<int, HourlyProductivity> GetHourlyProductivity(DateTime startDate, DateTime endDate);

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
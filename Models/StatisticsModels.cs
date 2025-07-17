using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// �������v���
    /// </summary>
    public partial class DailyStatistics : ObservableObject
    {
        /// <summary>
        /// ���t
        /// </summary>
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        /// <summary>
        /// �����|���h�[����
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// �����^�X�N��
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// ���W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int totalFocusMinutes = 0;

        /// <summary>
        /// �v���W�F�N�g�ʓ��v
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, ProjectStatistics> projectStatistics = new();

        /// <summary>
        /// �^�O�ʓ��v
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, TagStatistics> tagStatistics = new();

        /// <summary>
        /// �W���x�i0-100�̒l�j
        /// </summary>
        [ObservableProperty]
        private double focusScore = 0;

        /// <summary>
        /// ���W�����Ԃ̕�����\��
        /// </summary>
        public string TotalFocusTimeText
        {
            get
            {
                var hours = TotalFocusMinutes / 60;
                var minutes = TotalFocusMinutes % 60;
                return $"{hours}����{minutes}��";
            }
        }
    }

    /// <summary>
    /// �S���ԓ��v���
    /// </summary>
    public partial class AllTimeStatistics : ObservableObject
    {
        /// <summary>
        /// ���|���h�[����
        /// </summary>
        [ObservableProperty]
        private int totalPomodoros = 0;

        /// <summary>
        /// �������^�X�N��
        /// </summary>
        [ObservableProperty]
        private int totalCompletedTasks = 0;

        /// <summary>
        /// ���W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int totalFocusMinutes = 0;

        /// <summary>
        /// ���p�J�n��
        /// </summary>
        [ObservableProperty]
        private DateTime startDate = DateTime.Today;

        /// <summary>
        /// �����p����
        /// </summary>
        public int TotalDays => (DateTime.Today - StartDate).Days + 1;

        /// <summary>
        /// 1�����σ|���h�[����
        /// </summary>
        public double AveragePomodorosPerDay => TotalDays > 0 ? (double)TotalPomodoros / TotalDays : 0;

        /// <summary>
        /// 1�����σ^�X�N��
        /// </summary>
        public double AverageTasksPerDay => TotalDays > 0 ? (double)TotalCompletedTasks / TotalDays : 0;

        /// <summary>
        /// 1�����ϏW�����ԁi���j
        /// </summary>
        public double AverageFocusMinutesPerDay => TotalDays > 0 ? (double)TotalFocusMinutes / TotalDays : 0;

        /// <summary>
        /// ���W�����Ԃ̕�����\��
        /// </summary>
        public string TotalFocusTimeText
        {
            get
            {
                var hours = TotalFocusMinutes / 60;
                var minutes = TotalFocusMinutes % 60;
                return $"{hours}����{minutes}��";
            }
        }
    }

    /// <summary>
    /// �v���W�F�N�g�ʓ��v���
    /// </summary>
    public partial class ProjectStatistics : ObservableObject
    {
        /// <summary>
        /// �v���W�F�N�g��
        /// </summary>
        [ObservableProperty]
        private string projectName = string.Empty;

        /// <summary>
        /// �����|���h�[����
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// �����^�X�N��
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// �W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// �W�����Ԃ̕�����\��
        /// </summary>
        public string FocusTimeText
        {
            get
            {
                var hours = FocusMinutes / 60;
                var minutes = FocusMinutes % 60;
                return $"{hours}h {minutes}m";
            }
        }
    }

    /// <summary>
    /// �^�O�ʓ��v���
    /// </summary>
    public partial class TagStatistics : ObservableObject
    {
        /// <summary>
        /// �^�O��
        /// </summary>
        [ObservableProperty]
        private string tagName = string.Empty;

        /// <summary>
        /// �����|���h�[����
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// �����^�X�N��
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// �W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// �W�����Ԃ̕�����\��
        /// </summary>
        public string FocusTimeText
        {
            get
            {
                var hours = FocusMinutes / 60;
                var minutes = FocusMinutes % 60;
                return $"{hours}h {minutes}m";
            }
        }
    }

    /// <summary>
    /// �T�����v���
    /// </summary>
    public partial class WeeklyStatistics : ObservableObject
    {
        /// <summary>
        /// �T�̊J�n��
        /// </summary>
        [ObservableProperty]
        private DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        /// <summary>
        /// ���ʓ��v�̃��X�g
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// �T�̑��|���h�[����
        /// </summary>
        public int TotalPomodoros => DailyStatistics.Sum(d => d.CompletedPomodoros);

        /// <summary>
        /// �T�̑��^�X�N��
        /// </summary>
        public int TotalTasks => DailyStatistics.Sum(d => d.CompletedTasks);

        /// <summary>
        /// �T�̑��W�����ԁi���j
        /// </summary>
        public int TotalFocusMinutes => DailyStatistics.Sum(d => d.TotalFocusMinutes);

        /// <summary>
        /// ���ϏW���x
        /// </summary>
        public double AverageFocusScore
        {
            get
            {
                var validDays = DailyStatistics.Where(d => d.FocusScore > 0).ToList();
                return validDays.Count > 0 ? validDays.Average(d => d.FocusScore) : 0;
            }
        }
    }
}
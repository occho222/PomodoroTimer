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

    /// <summary>
    /// �T�����|�[�g
    /// </summary>
    public partial class WeeklyReport : ObservableObject
    {
        /// <summary>
        /// �T�̊J�n��
        /// </summary>
        [ObservableProperty]
        private DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        /// <summary>
        /// �T�̏I����
        /// </summary>
        public DateTime WeekEnd => WeekStart.AddDays(6);

        /// <summary>
        /// ���ʓ��v�̃��X�g
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// �ł����Y�I��������
        /// </summary>
        public DailyStatistics? MostProductiveDay =>
            DailyStatistics.OrderByDescending(d => d.TotalFocusMinutes).FirstOrDefault();

        /// <summary>
        /// �ł��g�p���ꂽ�v���W�F�N�g
        /// </summary>
        [ObservableProperty]
        private string topProject = string.Empty;

        /// <summary>
        /// �ł��g�p���ꂽ�^�O
        /// </summary>
        [ObservableProperty]
        private string topTag = string.Empty;

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
        /// �����σ|���h�[����
        /// </summary>
        public double AveragePomodorosPerDay => DailyStatistics.Count > 0 ? (double)TotalPomodoros / 7 : 0;

        /// <summary>
        /// �O�T��r�i�|���h�[�����j
        /// </summary>
        [ObservableProperty]
        private int pomodoroChangeFromLastWeek = 0;

        /// <summary>
        /// �O�T��r�̕\���e�L�X�g
        /// </summary>
        public string PomodoroChangeText
        {
            get
            {
                if (PomodoroChangeFromLastWeek > 0)
                    return $"? +{PomodoroChangeFromLastWeek}";
                else if (PomodoroChangeFromLastWeek < 0)
                    return $"? {PomodoroChangeFromLastWeek}";
                else
                    return "�� �}0";
            }
        }
    }

    /// <summary>
    /// ���ԓ��v
    /// </summary>
    public partial class MonthlyStatistics : ObservableObject
    {
        /// <summary>
        /// �N
        /// </summary>
        [ObservableProperty]
        private int year = DateTime.Today.Year;

        /// <summary>
        /// ��
        /// </summary>
        [ObservableProperty]
        private int month = DateTime.Today.Month;

        /// <summary>
        /// ���ʓ��v�̃��X�g
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// ���̑��|���h�[����
        /// </summary>
        public int TotalPomodoros => DailyStatistics.Sum(d => d.CompletedPomodoros);

        /// <summary>
        /// ���̑��^�X�N��
        /// </summary>
        public int TotalTasks => DailyStatistics.Sum(d => d.CompletedTasks);

        /// <summary>
        /// ���̑��W�����ԁi���j
        /// </summary>
        public int TotalFocusMinutes => DailyStatistics.Sum(d => d.TotalFocusMinutes);

        /// <summary>
        /// ��Ƃ�������
        /// </summary>
        public int WorkedDays => DailyStatistics.Count(d => d.CompletedPomodoros > 0);

        /// <summary>
        /// ���̖��O
        /// </summary>
        public string MonthName => $"{Year}�N{Month}��";
    }

    /// <summary>
    /// ���Y���g�����h
    /// </summary>
    public partial class ProductivityTrend : ObservableObject
    {
        /// <summary>
        /// ���t
        /// </summary>
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        /// <summary>
        /// �|���h�[����
        /// </summary>
        [ObservableProperty]
        private int pomodoros = 0;

        /// <summary>
        /// �W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// �W���x�X�R�A
        /// </summary>
        [ObservableProperty]
        private double focusScore = 0;

        /// <summary>
        /// ���t�\��
        /// </summary>
        public string DateText => Date.ToString("MM/dd");
    }

    /// <summary>
    /// ���ԑѕʐ��Y��
    /// </summary>
    public partial class HourlyProductivity : ObservableObject
    {
        /// <summary>
        /// ���ԁi0-23�j
        /// </summary>
        [ObservableProperty]
        private int hour = 0;

        /// <summary>
        /// �|���h�[����
        /// </summary>
        [ObservableProperty]
        private int pomodoros = 0;

        /// <summary>
        /// �W�����ԁi���j
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// ���ԕ\��
        /// </summary>
        public string HourText => $"{Hour:00}:00";

        /// <summary>
        /// ���ԑт̖��O
        /// </summary>
        public string TimePeriodName
        {
            get
            {
                return Hour switch
                {
                    >= 5 and < 12 => "��",
                    >= 12 and < 17 => "��",
                    >= 17 and < 21 => "�[��",
                    _ => "��"
                };
            }
        }
    }
}
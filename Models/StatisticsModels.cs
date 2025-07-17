using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// 日次統計情報
    /// </summary>
    public partial class DailyStatistics : ObservableObject
    {
        /// <summary>
        /// 日付
        /// </summary>
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        /// <summary>
        /// 完了ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// 完了タスク数
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// 総集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int totalFocusMinutes = 0;

        /// <summary>
        /// プロジェクト別統計
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, ProjectStatistics> projectStatistics = new();

        /// <summary>
        /// タグ別統計
        /// </summary>
        [ObservableProperty]
        private Dictionary<string, TagStatistics> tagStatistics = new();

        /// <summary>
        /// 集中度（0-100の値）
        /// </summary>
        [ObservableProperty]
        private double focusScore = 0;

        /// <summary>
        /// 総集中時間の文字列表現
        /// </summary>
        public string TotalFocusTimeText
        {
            get
            {
                var hours = TotalFocusMinutes / 60;
                var minutes = TotalFocusMinutes % 60;
                return $"{hours}時間{minutes}分";
            }
        }
    }

    /// <summary>
    /// 全期間統計情報
    /// </summary>
    public partial class AllTimeStatistics : ObservableObject
    {
        /// <summary>
        /// 総ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int totalPomodoros = 0;

        /// <summary>
        /// 総完了タスク数
        /// </summary>
        [ObservableProperty]
        private int totalCompletedTasks = 0;

        /// <summary>
        /// 総集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int totalFocusMinutes = 0;

        /// <summary>
        /// 利用開始日
        /// </summary>
        [ObservableProperty]
        private DateTime startDate = DateTime.Today;

        /// <summary>
        /// 総利用日数
        /// </summary>
        public int TotalDays => (DateTime.Today - StartDate).Days + 1;

        /// <summary>
        /// 1日平均ポモドーロ数
        /// </summary>
        public double AveragePomodorosPerDay => TotalDays > 0 ? (double)TotalPomodoros / TotalDays : 0;

        /// <summary>
        /// 1日平均タスク数
        /// </summary>
        public double AverageTasksPerDay => TotalDays > 0 ? (double)TotalCompletedTasks / TotalDays : 0;

        /// <summary>
        /// 1日平均集中時間（分）
        /// </summary>
        public double AverageFocusMinutesPerDay => TotalDays > 0 ? (double)TotalFocusMinutes / TotalDays : 0;

        /// <summary>
        /// 総集中時間の文字列表現
        /// </summary>
        public string TotalFocusTimeText
        {
            get
            {
                var hours = TotalFocusMinutes / 60;
                var minutes = TotalFocusMinutes % 60;
                return $"{hours}時間{minutes}分";
            }
        }
    }

    /// <summary>
    /// プロジェクト別統計情報
    /// </summary>
    public partial class ProjectStatistics : ObservableObject
    {
        /// <summary>
        /// プロジェクト名
        /// </summary>
        [ObservableProperty]
        private string projectName = string.Empty;

        /// <summary>
        /// 完了ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// 完了タスク数
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// 集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// 集中時間の文字列表現
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
    /// タグ別統計情報
    /// </summary>
    public partial class TagStatistics : ObservableObject
    {
        /// <summary>
        /// タグ名
        /// </summary>
        [ObservableProperty]
        private string tagName = string.Empty;

        /// <summary>
        /// 完了ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// 完了タスク数
        /// </summary>
        [ObservableProperty]
        private int completedTasks = 0;

        /// <summary>
        /// 集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// 集中時間の文字列表現
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
    /// 週次統計情報
    /// </summary>
    public partial class WeeklyStatistics : ObservableObject
    {
        /// <summary>
        /// 週の開始日
        /// </summary>
        [ObservableProperty]
        private DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        /// <summary>
        /// 日別統計のリスト
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// 週の総ポモドーロ数
        /// </summary>
        public int TotalPomodoros => DailyStatistics.Sum(d => d.CompletedPomodoros);

        /// <summary>
        /// 週の総タスク数
        /// </summary>
        public int TotalTasks => DailyStatistics.Sum(d => d.CompletedTasks);

        /// <summary>
        /// 週の総集中時間（分）
        /// </summary>
        public int TotalFocusMinutes => DailyStatistics.Sum(d => d.TotalFocusMinutes);

        /// <summary>
        /// 平均集中度
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
    /// 週次レポート
    /// </summary>
    public partial class WeeklyReport : ObservableObject
    {
        /// <summary>
        /// 週の開始日
        /// </summary>
        [ObservableProperty]
        private DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

        /// <summary>
        /// 週の終了日
        /// </summary>
        public DateTime WeekEnd => WeekStart.AddDays(6);

        /// <summary>
        /// 日別統計のリスト
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// 最も生産的だった日
        /// </summary>
        public DailyStatistics? MostProductiveDay =>
            DailyStatistics.OrderByDescending(d => d.TotalFocusMinutes).FirstOrDefault();

        /// <summary>
        /// 最も使用されたプロジェクト
        /// </summary>
        [ObservableProperty]
        private string topProject = string.Empty;

        /// <summary>
        /// 最も使用されたタグ
        /// </summary>
        [ObservableProperty]
        private string topTag = string.Empty;

        /// <summary>
        /// 週の総ポモドーロ数
        /// </summary>
        public int TotalPomodoros => DailyStatistics.Sum(d => d.CompletedPomodoros);

        /// <summary>
        /// 週の総タスク数
        /// </summary>
        public int TotalTasks => DailyStatistics.Sum(d => d.CompletedTasks);

        /// <summary>
        /// 週の総集中時間（分）
        /// </summary>
        public int TotalFocusMinutes => DailyStatistics.Sum(d => d.TotalFocusMinutes);

        /// <summary>
        /// 日平均ポモドーロ数
        /// </summary>
        public double AveragePomodorosPerDay => DailyStatistics.Count > 0 ? (double)TotalPomodoros / 7 : 0;

        /// <summary>
        /// 前週比較（ポモドーロ数）
        /// </summary>
        [ObservableProperty]
        private int pomodoroChangeFromLastWeek = 0;

        /// <summary>
        /// 前週比較の表示テキスト
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
                    return "→ ±0";
            }
        }
    }

    /// <summary>
    /// 月間統計
    /// </summary>
    public partial class MonthlyStatistics : ObservableObject
    {
        /// <summary>
        /// 年
        /// </summary>
        [ObservableProperty]
        private int year = DateTime.Today.Year;

        /// <summary>
        /// 月
        /// </summary>
        [ObservableProperty]
        private int month = DateTime.Today.Month;

        /// <summary>
        /// 日別統計のリスト
        /// </summary>
        [ObservableProperty]
        private List<DailyStatistics> dailyStatistics = new();

        /// <summary>
        /// 月の総ポモドーロ数
        /// </summary>
        public int TotalPomodoros => DailyStatistics.Sum(d => d.CompletedPomodoros);

        /// <summary>
        /// 月の総タスク数
        /// </summary>
        public int TotalTasks => DailyStatistics.Sum(d => d.CompletedTasks);

        /// <summary>
        /// 月の総集中時間（分）
        /// </summary>
        public int TotalFocusMinutes => DailyStatistics.Sum(d => d.TotalFocusMinutes);

        /// <summary>
        /// 作業した日数
        /// </summary>
        public int WorkedDays => DailyStatistics.Count(d => d.CompletedPomodoros > 0);

        /// <summary>
        /// 月の名前
        /// </summary>
        public string MonthName => $"{Year}年{Month}月";
    }

    /// <summary>
    /// 生産性トレンド
    /// </summary>
    public partial class ProductivityTrend : ObservableObject
    {
        /// <summary>
        /// 日付
        /// </summary>
        [ObservableProperty]
        private DateTime date = DateTime.Today;

        /// <summary>
        /// ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int pomodoros = 0;

        /// <summary>
        /// 集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// 集中度スコア
        /// </summary>
        [ObservableProperty]
        private double focusScore = 0;

        /// <summary>
        /// 日付表示
        /// </summary>
        public string DateText => Date.ToString("MM/dd");
    }

    /// <summary>
    /// 時間帯別生産性
    /// </summary>
    public partial class HourlyProductivity : ObservableObject
    {
        /// <summary>
        /// 時間（0-23）
        /// </summary>
        [ObservableProperty]
        private int hour = 0;

        /// <summary>
        /// ポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int pomodoros = 0;

        /// <summary>
        /// 集中時間（分）
        /// </summary>
        [ObservableProperty]
        private int focusMinutes = 0;

        /// <summary>
        /// 時間表示
        /// </summary>
        public string HourText => $"{Hour:00}:00";

        /// <summary>
        /// 時間帯の名前
        /// </summary>
        public string TimePeriodName
        {
            get
            {
                return Hour switch
                {
                    >= 5 and < 12 => "朝",
                    >= 12 and < 17 => "昼",
                    >= 17 and < 21 => "夕方",
                    _ => "夜"
                };
            }
        }
    }
}
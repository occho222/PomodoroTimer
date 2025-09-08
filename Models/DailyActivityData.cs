using System.Text.Json.Serialization;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// 指定日のアクティビティデータ
    /// </summary>
    public class DailyActivityData
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("summary")]
        public ActivitySummary Summary { get; set; } = new();

        [JsonPropertyName("completed_tasks")]
        public List<CompletedTaskData> CompletedTasks { get; set; } = new();

        [JsonPropertyName("pomodoro_sessions")]
        public List<PomodoroSessionData> PomodoroSessions { get; set; } = new();

        [JsonPropertyName("time_distribution")]
        public Dictionary<string, int> TimeDistribution { get; set; } = new();

        [JsonPropertyName("productivity_metrics")]
        public ProductivityMetrics ProductivityMetrics { get; set; } = new();
    }

    public class ActivitySummary
    {
        [JsonPropertyName("total_focus_time_minutes")]
        public int TotalFocusTimeMinutes { get; set; }

        [JsonPropertyName("total_pomodoros_completed")]
        public int TotalPomodorosCompleted { get; set; }

        [JsonPropertyName("total_tasks_completed")]
        public int TotalTasksCompleted { get; set; }

        [JsonPropertyName("total_break_time_minutes")]
        public int TotalBreakTimeMinutes { get; set; }

        [JsonPropertyName("focus_efficiency_percentage")]
        public double FocusEfficiencyPercentage { get; set; }

        [JsonPropertyName("average_pomodoro_length_minutes")]
        public double AveragePomodoroLengthMinutes { get; set; }
    }

    public class CompletedTaskData
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = "";

        [JsonPropertyName("completed_at")]
        public DateTime CompletedAt { get; set; }

        [JsonPropertyName("total_pomodoros_used")]
        public int TotalPomodorosUsed { get; set; }

        [JsonPropertyName("total_focus_time_minutes")]
        public int TotalFocusTimeMinutes { get; set; }

        [JsonPropertyName("estimated_vs_actual")]
        public EstimatedVsActualData EstimatedVsActual { get; set; } = new();
    }

    public class EstimatedVsActualData
    {
        [JsonPropertyName("estimated_minutes")]
        public int EstimatedMinutes { get; set; }

        [JsonPropertyName("actual_minutes")]
        public int ActualMinutes { get; set; }

        [JsonPropertyName("variance_percentage")]
        public double VariancePercentage { get; set; }
    }

    public class PomodoroSessionData
    {
        [JsonPropertyName("task_title")]
        public string TaskTitle { get; set; } = "";

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = "";

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonPropertyName("interruptions")]
        public int Interruptions { get; set; }

        [JsonPropertyName("completed_successfully")]
        public bool CompletedSuccessfully { get; set; }
    }

    public class ProductivityMetrics
    {
        [JsonPropertyName("deep_work_ratio")]
        public double DeepWorkRatio { get; set; }

        [JsonPropertyName("task_completion_rate")]
        public double TaskCompletionRate { get; set; }

        [JsonPropertyName("average_task_size_minutes")]
        public double AverageTaskSizeMinutes { get; set; }

        [JsonPropertyName("most_productive_hour")]
        public int MostProductiveHour { get; set; }

        [JsonPropertyName("peak_performance_periods")]
        public List<TimeRange> PeakPerformancePeriods { get; set; } = new();
    }

    public class TimeRange
    {
        [JsonPropertyName("start_hour")]
        public int StartHour { get; set; }

        [JsonPropertyName("end_hour")]
        public int EndHour { get; set; }

        [JsonPropertyName("focus_minutes")]
        public int FocusMinutes { get; set; }
    }

    /// <summary>
    /// 期間アクティビティデータ
    /// </summary>
    public class PeriodActivityData
    {
        [JsonPropertyName("start_date")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("total_days")]
        public int TotalDays { get; set; }

        [JsonPropertyName("summary")]
        public ActivitySummary Summary { get; set; } = new();

        [JsonPropertyName("daily_breakdown")]
        public List<DailyActivityData> DailyBreakdown { get; set; } = new();

        [JsonPropertyName("period_completed_tasks")]
        public List<CompletedTaskData> PeriodCompletedTasks { get; set; } = new();

        [JsonPropertyName("period_time_distribution")]
        public Dictionary<string, int> PeriodTimeDistribution { get; set; } = new();

        [JsonPropertyName("period_productivity_metrics")]
        public PeriodProductivityMetrics PeriodProductivityMetrics { get; set; } = new();

        [JsonPropertyName("daily_trends")]
        public List<DailyTrend> DailyTrends { get; set; } = new();
    }

    /// <summary>
    /// 期間生産性メトリクス
    /// </summary>
    public class PeriodProductivityMetrics
    {
        [JsonPropertyName("average_focus_time_per_day")]
        public double AverageFocusTimePerDay { get; set; }

        [JsonPropertyName("average_tasks_per_day")]
        public double AverageTasksPerDay { get; set; }

        [JsonPropertyName("average_pomodoros_per_day")]
        public double AveragePomodorosPerDay { get; set; }

        [JsonPropertyName("most_productive_day")]
        public DateTime MostProductiveDay { get; set; }

        [JsonPropertyName("least_productive_day")]
        public DateTime LeastProductiveDay { get; set; }

        [JsonPropertyName("consistency_score")]
        public double ConsistencyScore { get; set; }

        [JsonPropertyName("weekly_pattern")]
        public Dictionary<string, double> WeeklyPattern { get; set; } = new();

        [JsonPropertyName("category_focus_distribution")]
        public Dictionary<string, double> CategoryFocusDistribution { get; set; } = new();
    }

    /// <summary>
    /// 日次トレンド
    /// </summary>
    public class DailyTrend
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("focus_minutes")]
        public int FocusMinutes { get; set; }

        [JsonPropertyName("completed_pomodoros")]
        public int CompletedPomodoros { get; set; }

        [JsonPropertyName("completed_tasks")]
        public int CompletedTasks { get; set; }

        [JsonPropertyName("productivity_score")]
        public double ProductivityScore { get; set; }
    }
}
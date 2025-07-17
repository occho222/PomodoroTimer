using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Services;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// 統計画面のViewModel
    /// </summary>
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IPomodoroService _pomodoroService;

        [ObservableProperty]
        private int todayPomodoros;

        [ObservableProperty]
        private int todayTasks;

        [ObservableProperty]
        private string todayFocusTime = "0h 0m";

        [ObservableProperty]
        private int weekPomodoros;

        [ObservableProperty]
        private int weekTasks;

        [ObservableProperty]
        private string weekFocusTime = "0h 0m";

        [ObservableProperty]
        private int totalPomodoros;

        [ObservableProperty]
        private int totalTasks;

        [ObservableProperty]
        private string totalFocusTime = "0h 0m";

        public StatisticsViewModel(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));

            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                // 本日の統計
                var today = _statisticsService.GetDailyStatistics(DateTime.Today);
                TodayPomodoros = today.CompletedPomodoros;
                TodayTasks = today.CompletedTasks;
                TodayFocusTime = FormatTime(today.TotalFocusMinutes);

                // 今週の統計
                var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                var weekStats = _statisticsService.GetWeeklyStatistics(weekStart);
                WeekPomodoros = weekStats.Sum(d => d.CompletedPomodoros);
                WeekTasks = weekStats.Sum(d => d.CompletedTasks);
                WeekFocusTime = FormatTime(weekStats.Sum(d => d.TotalFocusMinutes));

                // 総計の統計
                var allStats = _statisticsService.GetAllTimeStatistics();
                TotalPomodoros = allStats.TotalPomodoros;
                TotalTasks = allStats.TotalCompletedTasks;
                TotalFocusTime = FormatTime(allStats.TotalFocusMinutes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"統計データの読み込みに失敗しました: {ex.Message}");
            }
        }

        private string FormatTime(int totalMinutes)
        {
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return $"{hours}h {minutes}m";
        }
    }
}
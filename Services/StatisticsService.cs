using PomodoroTimer.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// 統計情報サービスの実装
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        private readonly ConcurrentDictionary<DateTime, DailyStatistics> _dailyStatistics;
        private readonly IDataPersistenceService _dataPersistenceService;
        private const string StatisticsFileName = "statistics.json";

        public StatisticsService(IDataPersistenceService dataPersistenceService)
        {
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _dailyStatistics = new ConcurrentDictionary<DateTime, DailyStatistics>();
        }

        public DailyStatistics GetDailyStatistics(DateTime date)
        {
            var dateKey = date.Date;
            return _dailyStatistics.GetOrAdd(dateKey, _ => new DailyStatistics { Date = dateKey });
        }

        public WeeklyStatistics GetWeeklyStatistics(DateTime weekStart)
        {
            var weekStartDate = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek);
            var weeklyStats = new WeeklyStatistics
            {
                WeekStart = weekStartDate,
                DailyStatistics = new List<DailyStatistics>()
            };

            // 一週間分の統計を収集
            for (int i = 0; i < 7; i++)
            {
                var date = weekStartDate.AddDays(i);
                weeklyStats.DailyStatistics.Add(GetDailyStatistics(date));
            }

            return weeklyStats;
        }

        public void RecordPomodoroComplete(PomodoroTask task, int sessionDurationMinutes)
        {
            var today = DateTime.Today;
            var dailyStats = GetDailyStatistics(today);

            dailyStats.CompletedPomodoros++;
            dailyStats.TotalFocusMinutes += sessionDurationMinutes;

            // プロジェクト別統計の更新
            if (!string.IsNullOrEmpty(task.Category))
            {
                var projectStats = dailyStats.ProjectStatistics.GetValueOrDefault(task.Category, 
                    new ProjectStatistics { ProjectName = task.Category });
                
                projectStats.CompletedPomodoros++;
                projectStats.FocusMinutes += sessionDurationMinutes;
                
                dailyStats.ProjectStatistics[task.Category] = projectStats;
            }

            // 集中度の計算（簡易版：完了したポモドーロ数に基づく）
            UpdateFocusScore(dailyStats);
        }

        public void RecordTaskComplete(PomodoroTask task)
        {
            var today = DateTime.Today;
            var dailyStats = GetDailyStatistics(today);

            dailyStats.CompletedTasks++;

            // プロジェクト別統計の更新
            if (!string.IsNullOrEmpty(task.Category))
            {
                var projectStats = dailyStats.ProjectStatistics.GetValueOrDefault(task.Category, 
                    new ProjectStatistics { ProjectName = task.Category });
                
                projectStats.CompletedTasks++;
                
                dailyStats.ProjectStatistics[task.Category] = projectStats;
            }

            UpdateFocusScore(dailyStats);
        }

        public Dictionary<string, ProjectStatistics> GetProjectStatistics(DateTime startDate, DateTime endDate)
        {
            var projectStats = new Dictionary<string, ProjectStatistics>();

            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                var dailyStats = GetDailyStatistics(currentDate);
                
                foreach (var kvp in dailyStats.ProjectStatistics)
                {
                    if (!projectStats.ContainsKey(kvp.Key))
                    {
                        projectStats[kvp.Key] = new ProjectStatistics { ProjectName = kvp.Key };
                    }

                    var totalStats = projectStats[kvp.Key];
                    totalStats.CompletedPomodoros += kvp.Value.CompletedPomodoros;
                    totalStats.CompletedTasks += kvp.Value.CompletedTasks;
                    totalStats.FocusMinutes += kvp.Value.FocusMinutes;
                }

                currentDate = currentDate.AddDays(1);
            }

            return projectStats;
        }

        public void ClearStatistics()
        {
            _dailyStatistics.Clear();
        }

        public async Task SaveStatisticsAsync()
        {
            try
            {
                var statisticsData = _dailyStatistics.ToDictionary(
                    kvp => kvp.Key.ToString("yyyy-MM-dd"), 
                    kvp => kvp.Value);

                await _dataPersistenceService.SaveDataAsync("statistics.json", statisticsData);
            }
            catch (Exception ex)
            {
                // ログ記録（実装時に適切なロガーを使用）
                Console.WriteLine($"統計データの保存に失敗しました: {ex.Message}");
            }
        }

        public async Task LoadStatisticsAsync()
        {
            try
            {
                var statisticsData = await _dataPersistenceService.LoadDataAsync<Dictionary<string, DailyStatistics>>("statistics.json");
                
                if (statisticsData != null)
                {
                    _dailyStatistics.Clear();
                    
                    foreach (var kvp in statisticsData)
                    {
                        if (DateTime.TryParse(kvp.Key, out var date))
                        {
                            _dailyStatistics[date.Date] = kvp.Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ログ記録（実装時に適切なロガーを使用）
                Console.WriteLine($"統計データの読み込みに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 集中度スコアを更新する
        /// </summary>
        /// <param name="dailyStats">日次統計</param>
        private void UpdateFocusScore(DailyStatistics dailyStats)
        {
            // 簡易的な集中度計算：完了ポモドーロ数と完了タスク数に基づく
            var pomodoroScore = Math.Min(dailyStats.CompletedPomodoros * 10, 70);
            var taskScore = Math.Min(dailyStats.CompletedTasks * 15, 30);
            
            dailyStats.FocusScore = Math.Min(pomodoroScore + taskScore, 100);
        }
    }
}
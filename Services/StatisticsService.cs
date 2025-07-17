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
            
            // 起動時に統計データを読み込み
            _ = Task.Run(LoadStatisticsAsync);
        }

        public DailyStatistics GetDailyStatistics(DateTime date)
        {
            var dateKey = date.Date;
            return _dailyStatistics.GetOrAdd(dateKey, _ => new DailyStatistics { Date = dateKey });
        }

        public List<DailyStatistics> GetWeeklyStatistics(DateTime weekStart)
        {
            var weekStartDate = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek);
            var weeklyStatsList = new List<DailyStatistics>();

            // 一週間分の統計を収集
            for (int i = 0; i < 7; i++)
            {
                var date = weekStartDate.AddDays(i);
                weeklyStatsList.Add(GetDailyStatistics(date));
            }

            return weeklyStatsList;
        }

        public AllTimeStatistics GetAllTimeStatistics()
        {
            var allTimeStats = new AllTimeStatistics();

            if (_dailyStatistics.Any())
            {
                allTimeStats.StartDate = _dailyStatistics.Keys.Min();
                allTimeStats.TotalPomodoros = _dailyStatistics.Values.Sum(d => d.CompletedPomodoros);
                allTimeStats.TotalCompletedTasks = _dailyStatistics.Values.Sum(d => d.CompletedTasks);
                allTimeStats.TotalFocusMinutes = _dailyStatistics.Values.Sum(d => d.TotalFocusMinutes);
            }

            return allTimeStats;
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

            // タグ別統計の更新
            foreach (var tag in task.Tags.Where(t => !string.IsNullOrEmpty(t)))
            {
                var tagStats = dailyStats.TagStatistics.GetValueOrDefault(tag,
                    new TagStatistics { TagName = tag });
                
                tagStats.CompletedPomodoros++;
                tagStats.FocusMinutes += sessionDurationMinutes;
                
                dailyStats.TagStatistics[tag] = tagStats;
            }

            // 集中度の計算（簡易版：完了したポモドーロ数に基づく）
            UpdateFocusScore(dailyStats);
            
            // 統計データを保存
            _ = Task.Run(SaveStatisticsAsync);
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

            // タグ別統計の更新
            foreach (var tag in task.Tags.Where(t => !string.IsNullOrEmpty(t)))
            {
                var tagStats = dailyStats.TagStatistics.GetValueOrDefault(tag,
                    new TagStatistics { TagName = tag });
                
                tagStats.CompletedTasks++;
                
                dailyStats.TagStatistics[tag] = tagStats;
            }

            UpdateFocusScore(dailyStats);
            
            // 統計データを保存
            _ = Task.Run(SaveStatisticsAsync);
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

        /// <summary>
        /// タグ別統計を取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>タグ別統計のディクショナリ</returns>
        public Dictionary<string, TagStatistics> GetTagStatistics(DateTime startDate, DateTime endDate)
        {
            var tagStats = new Dictionary<string, TagStatistics>();

            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                var dailyStats = GetDailyStatistics(currentDate);
                
                foreach (var kvp in dailyStats.TagStatistics)
                {
                    if (!tagStats.ContainsKey(kvp.Key))
                    {
                        tagStats[kvp.Key] = new TagStatistics { TagName = kvp.Key };
                    }

                    var totalStats = tagStats[kvp.Key];
                    totalStats.CompletedPomodoros += kvp.Value.CompletedPomodoros;
                    totalStats.CompletedTasks += kvp.Value.CompletedTasks;
                    totalStats.FocusMinutes += kvp.Value.FocusMinutes;
                }

                currentDate = currentDate.AddDays(1);
            }

            return tagStats;
        }

        /// <summary>
        /// カテゴリ別の作業時間ランキングを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="topCount">上位何位まで取得するか</param>
        /// <returns>作業時間順のカテゴリランキング</returns>
        public List<(string Category, int FocusMinutes, int CompletedPomodoros)> GetCategoryRanking(
            DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var projectStats = GetProjectStatistics(startDate, endDate);
            
            return projectStats
                .OrderByDescending(x => x.Value.FocusMinutes)
                .Take(topCount)
                .Select(x => (x.Key, x.Value.FocusMinutes, x.Value.CompletedPomodoros))
                .ToList();
        }

        /// <summary>
        /// タグ別の作業時間ランキングを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="topCount">上位何位まで取得するか</param>
        /// <returns>作業時間順のタグランキング</returns>
        public List<(string Tag, int FocusMinutes, int CompletedPomodoros)> GetTagRanking(
            DateTime startDate, DateTime endDate, int topCount = 10)
        {
            var tagStats = GetTagStatistics(startDate, endDate);
            
            return tagStats
                .OrderByDescending(x => x.Value.FocusMinutes)
                .Take(topCount)
                .Select(x => (x.Key, x.Value.FocusMinutes, x.Value.CompletedPomodoros))
                .ToList();
        }

        /// <summary>
        /// 週次レポートを取得する
        /// </summary>
        /// <param name="weekStart">週の開始日</param>
        /// <returns>週次レポート</returns>
        public WeeklyReport GetWeeklyReport(DateTime weekStart)
        {
            var weekStartDate = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek);
            var weeklyStats = GetWeeklyStatistics(weekStartDate);
            
            var report = new WeeklyReport
            {
                WeekStart = weekStartDate,
                DailyStatistics = weeklyStats
            };

            // 最も使用されたプロジェクトを取得
            var projectStats = GetProjectStatistics(weekStartDate, weekStartDate.AddDays(6));
            var topProject = projectStats.OrderByDescending(p => p.Value.FocusMinutes).FirstOrDefault();
            report.TopProject = topProject.Key ?? "なし";

            // 最も使用されたタグを取得
            var tagStats = GetTagStatistics(weekStartDate, weekStartDate.AddDays(6));
            var topTag = tagStats.OrderByDescending(t => t.Value.FocusMinutes).FirstOrDefault();
            report.TopTag = topTag.Key ?? "なし";

            // 前週との比較
            var lastWeekStart = weekStartDate.AddDays(-7);
            var lastWeekStats = GetWeeklyStatistics(lastWeekStart);
            var lastWeekPomodoros = lastWeekStats.Sum(d => d.CompletedPomodoros);
            report.PomodoroChangeFromLastWeek = report.TotalPomodoros - lastWeekPomodoros;

            return report;
        }

        /// <summary>
        /// 月間統計を取得する
        /// </summary>
        /// <param name="year">年</param>
        /// <param name="month">月</param>
        /// <returns>月間統計</returns>
        public MonthlyStatistics GetMonthlyStatistics(int year, int month)
        {
            var monthlyStats = new MonthlyStatistics
            {
                Year = year,
                Month = month
            };

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var dailyStats = new List<DailyStatistics>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var dailyStat = GetDailyStatistics(currentDate);
                if (dailyStat.CompletedPomodoros > 0 || dailyStat.CompletedTasks > 0)
                {
                    dailyStats.Add(dailyStat);
                }
                currentDate = currentDate.AddDays(1);
            }

            monthlyStats.DailyStatistics = dailyStats;
            return monthlyStats;
        }

        /// <summary>
        /// 生産性トレンドを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>生産性トレンドデータ</returns>
        public List<ProductivityTrend> GetProductivityTrend(DateTime startDate, DateTime endDate)
        {
            var trends = new List<ProductivityTrend>();
            var currentDate = startDate.Date;

            while (currentDate <= endDate.Date)
            {
                var dailyStats = GetDailyStatistics(currentDate);
                trends.Add(new ProductivityTrend
                {
                    Date = currentDate,
                    Pomodoros = dailyStats.CompletedPomodoros,
                    FocusMinutes = dailyStats.TotalFocusMinutes,
                    FocusScore = dailyStats.FocusScore
                });

                currentDate = currentDate.AddDays(1);
            }

            return trends;
        }

        /// <summary>
        /// 時間帯別作業分析を取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>時間帯別作業分析</returns>
        public Dictionary<int, HourlyProductivity> GetHourlyProductivity(DateTime startDate, DateTime endDate)
        {
            // 簡易実装：実際の時間帯データが必要な場合は、セッション開始時間の記録機能を追加する必要があります
            var hourlyStats = new Dictionary<int, HourlyProductivity>();

            // 0-23時の初期化
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyStats[hour] = new HourlyProductivity { Hour = hour };
            }

            // 現在はサンプルデータとして、一般的な作業時間帯にデータを配分
            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                var dailyStats = GetDailyStatistics(currentDate);
                if (dailyStats.CompletedPomodoros > 0)
                {
                    // 作業時間を朝(9-12)、昼(13-17)、夜(19-22)に分散
                    var morningPomodoros = (int)(dailyStats.CompletedPomodoros * 0.4);
                    var afternoonPomodoros = (int)(dailyStats.CompletedPomodoros * 0.5);
                    var eveningPomodoros = dailyStats.CompletedPomodoros - morningPomodoros - afternoonPomodoros;

                    // 朝の時間帯
                    for (int h = 9; h <= 11; h++)
                    {
                        hourlyStats[h].Pomodoros += morningPomodoros / 3;
                        hourlyStats[h].FocusMinutes += (dailyStats.TotalFocusMinutes * 40 / 100) / 3;
                    }

                    // 昼の時間帯
                    for (int h = 13; h <= 17; h++)
                    {
                        hourlyStats[h].Pomodoros += afternoonPomodoros / 5;
                        hourlyStats[h].FocusMinutes += (dailyStats.TotalFocusMinutes * 50 / 100) / 5;
                    }

                    // 夜の時間帯
                    for (int h = 19; h <= 21; h++)
                    {
                        hourlyStats[h].Pomodoros += eveningPomodoros / 3;
                        hourlyStats[h].FocusMinutes += (dailyStats.TotalFocusMinutes * 10 / 100) / 3;
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return hourlyStats;
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
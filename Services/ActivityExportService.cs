using PomodoroTimer.Models;
using System.Text.Json;
using System.Text;
using TaskStatus = PomodoroTimer.Models.TaskStatus;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// アクティビティデータエクスポートサービスの実装
    /// </summary>
    public class ActivityExportService : IActivityExportService
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly IStatisticsService _statisticsService;

        public ActivityExportService(IPomodoroService pomodoroService, IStatisticsService statisticsService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
        }

        public DailyActivityData GetDailyActivityData(DateTime date)
        {
            var dateOnly = date.Date;
            var tasks = _pomodoroService.GetTasks();
            var dailyStats = _statisticsService.GetDailyStatistics(dateOnly);

            var activityData = new DailyActivityData
            {
                Date = dateOnly
            };

            // 完了したタスクを取得
            var completedTasks = tasks
                .Where(t => t.Status == TaskStatus.Completed && 
                           t.CompletedAt.HasValue && 
                           t.CompletedAt.Value.Date == dateOnly)
                .ToList();

            // サマリー情報を設定
            activityData.Summary = new ActivitySummary
            {
                TotalFocusTimeMinutes = dailyStats.TotalFocusMinutes,
                TotalPomodorosCompleted = dailyStats.CompletedPomodoros,
                TotalTasksCompleted = completedTasks.Count,
                TotalBreakTimeMinutes = 0,
                FocusEfficiencyPercentage = CalculateFocusEfficiency(dailyStats),
                AveragePomodoroLengthMinutes = dailyStats.CompletedPomodoros > 0 
                    ? (double)dailyStats.TotalFocusMinutes / dailyStats.CompletedPomodoros 
                    : 0
            };

            // 完了タスクデータを設定
            activityData.CompletedTasks = completedTasks.Select(task => new CompletedTaskData
            {
                Title = task.Title,
                Description = task.Description,
                Category = task.Category,
                Tags = task.Tags.ToList(),
                Priority = GetPriorityText(task.Priority),
                CompletedAt = task.CompletedAt ?? dateOnly,
                TotalPomodorosUsed = task.CompletedPomodoros,
                TotalFocusTimeMinutes = task.ActualMinutes,
                EstimatedVsActual = new EstimatedVsActualData
                {
                    EstimatedMinutes = task.EstimatedMinutes,
                    ActualMinutes = task.ActualMinutes,
                    VariancePercentage = task.EstimatedMinutes > 0 
                        ? ((double)(task.ActualMinutes - task.EstimatedMinutes) / task.EstimatedMinutes) * 100 
                        : 0
                }
            }).ToList();

            // ポモドーロセッションデータ（タスクの作業履歴から推定）
            activityData.PomodoroSessions = GeneratePomodoroSessions(completedTasks, dateOnly);

            // 時間配分データ
            activityData.TimeDistribution = CalculateTimeDistribution(completedTasks);

            // 生産性メトリクス
            activityData.ProductivityMetrics = CalculateProductivityMetrics(completedTasks, dailyStats);

            return activityData;
        }

        public async Task ExportActivityDataToJsonAsync(DailyActivityData activityData, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var jsonString = JsonSerializer.Serialize(activityData, options);
            await System.IO.File.WriteAllTextAsync(filePath, jsonString, Encoding.UTF8);
        }

        public async Task ExportReflectionPromptAsync(DailyActivityData activityData, string jsonFilePath, string promptFilePath)
        {
            var prompt = GenerateReflectionPrompt(activityData, jsonFilePath);
            await System.IO.File.WriteAllTextAsync(promptFilePath, prompt, Encoding.UTF8);
        }

        public async Task<(string jsonFilePath, string promptFilePath)> ExportDailyActivityAsync(DateTime date, string outputDirectory)
        {
            if (!System.IO.Directory.Exists(outputDirectory))
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }

            var dateString = date.ToString("yyyy-MM-dd");
            var jsonFileName = $"activity_{dateString}.json";
            var promptFileName = $"reflection_prompt_{dateString}.txt";
            
            var jsonFilePath = System.IO.Path.Combine(outputDirectory, jsonFileName);
            var promptFilePath = System.IO.Path.Combine(outputDirectory, promptFileName);

            var activityData = GetDailyActivityData(date);
            
            await ExportActivityDataToJsonAsync(activityData, jsonFilePath);
            await ExportReflectionPromptAsync(activityData, jsonFileName, promptFilePath);

            return (jsonFilePath, promptFilePath);
        }

        private double CalculateFocusEfficiency(DailyStatistics stats)
        {
            var totalTime = stats.TotalFocusMinutes + 0;
            return totalTime > 0 ? (double)stats.TotalFocusMinutes / totalTime * 100 : 0;
        }

        private string GetPriorityText(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Urgent => "緊急",
                TaskPriority.High => "高",
                TaskPriority.Medium => "中",
                TaskPriority.Low => "低",
                _ => "中"
            };
        }

        private List<PomodoroSessionData> GeneratePomodoroSessions(List<PomodoroTask> completedTasks, DateTime date)
        {
            var sessions = new List<PomodoroSessionData>();
            var currentTime = date.AddHours(9); // 9時開始と仮定

            foreach (var task in completedTasks.OrderBy(t => t.CompletedAt))
            {
                for (int i = 0; i < task.CompletedPomodoros; i++)
                {
                    var sessionStart = currentTime.AddMinutes(i * 30); // 25分作業 + 5分休憩
                    var sessionEnd = sessionStart.AddMinutes(25);

                    sessions.Add(new PomodoroSessionData
                    {
                        TaskTitle = task.Title,
                        StartTime = sessionStart,
                        EndTime = sessionEnd,
                        DurationMinutes = 25,
                        Category = task.Category,
                        Tags = task.Tags.ToList(),
                        Interruptions = 0, // 仮の値
                        CompletedSuccessfully = true
                    });
                }
                currentTime = currentTime.AddMinutes(task.CompletedPomodoros * 30);
            }

            return sessions;
        }

        private Dictionary<string, int> CalculateTimeDistribution(List<PomodoroTask> completedTasks)
        {
            var distribution = new Dictionary<string, int>();

            foreach (var task in completedTasks)
            {
                var category = string.IsNullOrEmpty(task.Category) ? "未分類" : task.Category;
                if (distribution.ContainsKey(category))
                {
                    distribution[category] += task.ActualMinutes;
                }
                else
                {
                    distribution[category] = task.ActualMinutes;
                }
            }

            return distribution;
        }

        private ProductivityMetrics CalculateProductivityMetrics(List<PomodoroTask> completedTasks, DailyStatistics stats)
        {
            var totalMinutes = completedTasks.Sum(t => t.ActualMinutes);
            var deepWorkTime = completedTasks.Where(t => t.ActualMinutes >= 50).Sum(t => t.ActualMinutes);
            
            return new ProductivityMetrics
            {
                DeepWorkRatio = stats.TotalFocusMinutes > 0 ? (double)deepWorkTime / stats.TotalFocusMinutes : 0,
                TaskCompletionRate = completedTasks.Count > 0 ? 100.0 : 0,
                AverageTaskSizeMinutes = completedTasks.Any() ? (double)totalMinutes / completedTasks.Count : 0,
                MostProductiveHour = 10, // 仮の値（10時）
                PeakPerformancePeriods = new List<TimeRange>
                {
                    new TimeRange { StartHour = 9, EndHour = 12, FocusMinutes = stats.TotalFocusMinutes / 2 },
                    new TimeRange { StartHour = 14, EndHour = 17, FocusMinutes = stats.TotalFocusMinutes / 2 }
                }
            };
        }

        private string GenerateReflectionPrompt(DailyActivityData activityData, string jsonFileName)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("# 日次アクティビティ振り返り用プロンプト");
            sb.AppendLine();
            
            // まず最初にサマリーを表示
            sb.AppendLine("## 📊 今日のアクティビティサマリー");
            sb.AppendLine($"**対象日**: {activityData.Date:yyyy年MM月dd日}");
            sb.AppendLine($"**総集中時間**: {activityData.Summary.TotalFocusTimeMinutes}分 ({activityData.Summary.TotalFocusTimeMinutes / 60.0:F1}時間)");
            sb.AppendLine($"**完了ポモドーロ数**: {activityData.Summary.TotalPomodorosCompleted}個");
            sb.AppendLine($"**完了タスク数**: {activityData.Summary.TotalTasksCompleted}個");
            sb.AppendLine($"**集中効率**: {activityData.Summary.FocusEfficiencyPercentage:F1}%");
            
            // カテゴリ別時間配分を追加
            if (activityData.TimeDistribution.Any())
            {
                sb.AppendLine();
                sb.AppendLine("**カテゴリ別時間配分**:");
                foreach (var category in activityData.TimeDistribution.OrderByDescending(c => c.Value))
                {
                    var percentage = activityData.Summary.TotalFocusTimeMinutes > 0 
                        ? (category.Value * 100.0 / activityData.Summary.TotalFocusTimeMinutes) 
                        : 0;
                    sb.AppendLine($"- {category.Key}: {category.Value}分 ({percentage:F1}%)");
                }
            }
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            
            // 分析依頼内容
            sb.AppendLine("## 🎯 分析依頼");
            sb.AppendLine("以下のJSONデータは、私の1日の作業アクティビティを詳細に記録したものです。");
            sb.AppendLine("上記のサマリーと詳細なJSONデータを分析して、生産性向上のための具体的なアドバイスと振り返りを提供してください。");
            sb.AppendLine();
            
            // 期待する出力形式を最初に明示
            sb.AppendLine("## 📝 期待する出力形式");
            sb.AppendLine("以下の形式で**必ずサマリーから開始**してください:");
            sb.AppendLine();
            sb.AppendLine("### 🏆 今日の成果ハイライト");
            sb.AppendLine("- 最も重要な成果3つを簡潔に");
            sb.AppendLine("- 定量的な数値を含めて");
            sb.AppendLine();
            sb.AppendLine("### ✅ 良かった点");
            sb.AppendLine("- 継続すべき行動や判断");
            sb.AppendLine("- 効果的だった時間の使い方");
            sb.AppendLine();
            sb.AppendLine("### 🔄 改善点");
            sb.AppendLine("- 具体的な改善アクション");
            sb.AppendLine("- 時間配分の最適化案");
            sb.AppendLine();
            sb.AppendLine("### 🚀 明日への提案");
            sb.AppendLine("- 明日の計画立案に役立つ実践的アドバイス");
            sb.AppendLine("- 優先順位付けの改善案");
            sb.AppendLine();
            
            // 分析観点
            sb.AppendLine("## 🔍 分析してほしい観点");
            sb.AppendLine("1. **時間配分の効率性**");
            sb.AppendLine("   - カテゴリごとの時間配分は適切か");
            sb.AppendLine("   - 優先度と実際の時間投入のバランス");
            sb.AppendLine();
            sb.AppendLine("2. **タスク管理の精度**");
            sb.AppendLine("   - 見積もりと実際の作業時間の差異分析");
            sb.AppendLine("   - タスクのサイズ設定の適切性");
            sb.AppendLine();
            sb.AppendLine("3. **集中力と生産性**");
            sb.AppendLine("   - ポモドーロテクニックの活用効果");
            sb.AppendLine("   - 深い作業（Deep Work）の割合と質");
            sb.AppendLine("   - 集中できた時間帯とパターン分析");
            sb.AppendLine();
            
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## 📄 詳細データ");
            sb.AppendLine("詳細なJSONデータは以下のファイルを参照してください:");
            sb.AppendLine($"**ファイル名**: {jsonFileName}");
            sb.AppendLine();
            sb.AppendLine("💡 **使用方法**: このプロンプトと一緒にJSONファイルの内容を生成AIに渡して、");
            sb.AppendLine("パーソナライズされた振り返りと改善提案を受けてください。");

            return sb.ToString();
        }

        public PeriodActivityData GetPeriodActivityData(DateTime startDate, DateTime endDate)
        {
            var startDateOnly = startDate.Date;
            var endDateOnly = endDate.Date;
            var totalDays = (endDateOnly - startDateOnly).Days + 1;

            var periodData = new PeriodActivityData
            {
                StartDate = startDateOnly,
                EndDate = endDateOnly,
                TotalDays = totalDays
            };

            // 期間内の全ての日のデータを取得
            var dailyDataList = new List<DailyActivityData>();
            var allTasks = _pomodoroService.GetTasks();

            for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
            {
                var dailyData = GetDailyActivityData(date);
                dailyDataList.Add(dailyData);
            }

            periodData.DailyBreakdown = dailyDataList;

            // 期間全体のサマリーを計算
            periodData.Summary = new ActivitySummary
            {
                TotalFocusTimeMinutes = dailyDataList.Sum(d => d.Summary.TotalFocusTimeMinutes),
                TotalPomodorosCompleted = dailyDataList.Sum(d => d.Summary.TotalPomodorosCompleted),
                TotalTasksCompleted = dailyDataList.Sum(d => d.Summary.TotalTasksCompleted),
                TotalBreakTimeMinutes = 0,
                FocusEfficiencyPercentage = dailyDataList.Where(d => d.Summary.TotalFocusTimeMinutes > 0).Any() 
                    ? dailyDataList.Where(d => d.Summary.TotalFocusTimeMinutes > 0).Average(d => d.Summary.FocusEfficiencyPercentage) 
                    : 0,
                AveragePomodoroLengthMinutes = dailyDataList.Sum(d => d.Summary.TotalPomodorosCompleted) > 0
                    ? (double)dailyDataList.Sum(d => d.Summary.TotalFocusTimeMinutes) / dailyDataList.Sum(d => d.Summary.TotalPomodorosCompleted)
                    : 0
            };

            // 期間内の完了タスク
            var periodCompletedTasks = allTasks
                .Where(t => t.Status == TaskStatus.Completed && 
                           t.CompletedAt.HasValue && 
                           t.CompletedAt.Value.Date >= startDateOnly && 
                           t.CompletedAt.Value.Date <= endDateOnly)
                .Select(task => new CompletedTaskData
                {
                    Title = task.Title,
                    Description = task.Description,
                    Category = task.Category,
                    Tags = task.Tags.ToList(),
                    Priority = GetPriorityText(task.Priority),
                    CompletedAt = task.CompletedAt ?? startDateOnly,
                    TotalPomodorosUsed = task.CompletedPomodoros,
                    TotalFocusTimeMinutes = task.ActualMinutes,
                    EstimatedVsActual = new EstimatedVsActualData
                    {
                        EstimatedMinutes = task.EstimatedMinutes,
                        ActualMinutes = task.ActualMinutes,
                        VariancePercentage = task.EstimatedMinutes > 0 
                            ? ((double)(task.ActualMinutes - task.EstimatedMinutes) / task.EstimatedMinutes) * 100 
                            : 0
                    }
                }).ToList();

            periodData.PeriodCompletedTasks = periodCompletedTasks;

            // 期間全体の時間配分
            var periodTimeDistribution = new Dictionary<string, int>();
            foreach (var task in periodCompletedTasks)
            {
                var category = string.IsNullOrEmpty(task.Category) ? "未分類" : task.Category;
                if (periodTimeDistribution.ContainsKey(category))
                {
                    periodTimeDistribution[category] += task.TotalFocusTimeMinutes;
                }
                else
                {
                    periodTimeDistribution[category] = task.TotalFocusTimeMinutes;
                }
            }
            periodData.PeriodTimeDistribution = periodTimeDistribution;

            // 期間生産性メトリクス
            periodData.PeriodProductivityMetrics = CalculatePeriodProductivityMetrics(dailyDataList, periodCompletedTasks);

            // 日次トレンド
            periodData.DailyTrends = dailyDataList.Select(d => new DailyTrend
            {
                Date = d.Date,
                FocusMinutes = d.Summary.TotalFocusTimeMinutes,
                CompletedPomodoros = d.Summary.TotalPomodorosCompleted,
                CompletedTasks = d.Summary.TotalTasksCompleted,
                ProductivityScore = d.Summary.TotalFocusTimeMinutes > 0 && d.Summary.TotalTasksCompleted > 0
                    ? (d.Summary.TotalFocusTimeMinutes * 0.7) + (d.Summary.TotalTasksCompleted * 30 * 0.3)
                    : 0
            }).ToList();

            return periodData;
        }

        public async Task<(string jsonFilePath, string promptFilePath)> ExportPeriodActivityAsync(DateTime startDate, DateTime endDate, string outputDirectory)
        {
            if (!System.IO.Directory.Exists(outputDirectory))
            {
                System.IO.Directory.CreateDirectory(outputDirectory);
            }

            var startDateString = startDate.ToString("yyyy-MM-dd");
            var endDateString = endDate.ToString("yyyy-MM-dd");
            var jsonFileName = $"activity_period_{startDateString}_to_{endDateString}.json";
            var promptFileName = $"ai_analysis_prompt_{startDateString}_to_{endDateString}.txt";
            
            var jsonFilePath = System.IO.Path.Combine(outputDirectory, jsonFileName);
            var promptFilePath = System.IO.Path.Combine(outputDirectory, promptFileName);

            var activityData = GetPeriodActivityData(startDate, endDate);
            
            await ExportPeriodActivityDataToJsonAsync(activityData, jsonFilePath);
            await ExportPeriodReflectionPromptAsync(activityData, jsonFileName, promptFilePath);

            return (jsonFilePath, promptFilePath);
        }

        private async Task ExportPeriodActivityDataToJsonAsync(PeriodActivityData activityData, string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var jsonString = JsonSerializer.Serialize(activityData, options);
            await System.IO.File.WriteAllTextAsync(filePath, jsonString, Encoding.UTF8);
        }

        private async Task ExportPeriodReflectionPromptAsync(PeriodActivityData activityData, string jsonFileName, string promptFilePath)
        {
            var prompt = GeneratePeriodReflectionPrompt(activityData, jsonFileName);
            await System.IO.File.WriteAllTextAsync(promptFilePath, prompt, Encoding.UTF8);
        }

        private PeriodProductivityMetrics CalculatePeriodProductivityMetrics(List<DailyActivityData> dailyDataList, List<CompletedTaskData> periodCompletedTasks)
        {
            var workingDays = dailyDataList.Where(d => d.Summary.TotalFocusTimeMinutes > 0 || d.Summary.TotalTasksCompleted > 0).ToList();
            
            var mostProductiveDay = dailyDataList.OrderByDescending(d => d.Summary.TotalFocusTimeMinutes).FirstOrDefault()?.Date ?? DateTime.Today;
            var leastProductiveDay = workingDays.OrderBy(d => d.Summary.TotalFocusTimeMinutes).FirstOrDefault()?.Date ?? DateTime.Today;

            // 一貫性スコア（標準偏差から計算）
            var focusTimes = workingDays.Select(d => (double)d.Summary.TotalFocusTimeMinutes).ToList();
            var consistencyScore = focusTimes.Any() 
                ? Math.Max(0, 100 - (CalculateStandardDeviation(focusTimes) / (focusTimes.Average() + 1) * 100))
                : 0;

            // 週パターン分析
            var weeklyPattern = new Dictionary<string, double>();
            var dayGroups = dailyDataList.GroupBy(d => d.Date.DayOfWeek.ToString());
            foreach (var group in dayGroups)
            {
                weeklyPattern[group.Key] = group.Where(d => d.Summary.TotalFocusTimeMinutes > 0).Any() 
                    ? group.Average(d => d.Summary.TotalFocusTimeMinutes) 
                    : 0;
            }

            // カテゴリ集中度分布
            var totalFocusTime = periodCompletedTasks.Sum(t => t.TotalFocusTimeMinutes);
            var categoryFocusDistribution = new Dictionary<string, double>();
            var categoryGroups = periodCompletedTasks.GroupBy(t => string.IsNullOrEmpty(t.Category) ? "未分類" : t.Category);
            foreach (var group in categoryGroups)
            {
                var categoryFocusTime = group.Sum(t => t.TotalFocusTimeMinutes);
                categoryFocusDistribution[group.Key] = totalFocusTime > 0 ? (double)categoryFocusTime / totalFocusTime * 100 : 0;
            }

            return new PeriodProductivityMetrics
            {
                AverageFocusTimePerDay = workingDays.Any() ? workingDays.Average(d => d.Summary.TotalFocusTimeMinutes) : 0,
                AverageTasksPerDay = workingDays.Any() ? workingDays.Average(d => d.Summary.TotalTasksCompleted) : 0,
                AveragePomodorosPerDay = workingDays.Any() ? workingDays.Average(d => d.Summary.TotalPomodorosCompleted) : 0,
                MostProductiveDay = mostProductiveDay,
                LeastProductiveDay = leastProductiveDay,
                ConsistencyScore = consistencyScore,
                WeeklyPattern = weeklyPattern,
                CategoryFocusDistribution = categoryFocusDistribution
            };
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            if (values.Count <= 1) return 0;
            
            var average = values.Average();
            var sumOfSquaresOfDifferences = values.Select(val => (val - average) * (val - average)).Sum();
            return Math.Sqrt(sumOfSquaresOfDifferences / values.Count);
        }

        private string GeneratePeriodReflectionPrompt(PeriodActivityData activityData, string jsonFileName)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("# 期間アクティビティ分析用プロンプト");
            sb.AppendLine();
            sb.AppendLine("以下のJSONデータは、私の指定期間における作業アクティビティを詳細に記録したものです。");
            sb.AppendLine("このデータを分析して、生産性向上のための包括的な洞察と実践的なアドバイスを提供してください。");
            sb.AppendLine();
            sb.AppendLine("## 📊 分析してほしい観点:");
            sb.AppendLine();
            sb.AppendLine("### 1. **期間全体のパフォーマンス評価**");
            sb.AppendLine("   - 全体的な生産性レベルの評価");
            sb.AppendLine("   - 目標に対する達成度（仮想的なベンチマークとの比較）");
            sb.AppendLine("   - 期間中の成長・改善傾向");
            sb.AppendLine();
            sb.AppendLine("### 2. **時間配分と優先順位の分析**");
            sb.AppendLine("   - カテゴリ別時間配分の適切性");
            sb.AppendLine("   - 重要なタスクへの時間投入バランス");
            sb.AppendLine("   - 時間の無駄遣いや非効率な部分の特定");
            sb.AppendLine();
            sb.AppendLine("### 3. **日次パフォーマンスの一貫性**");
            sb.AppendLine("   - 日々の生産性のばらつき分析");
            sb.AppendLine("   - 最も生産的だった日と最も低調だった日の要因分析");
            sb.AppendLine("   - 週単位のパターンや曜日別の傾向");
            sb.AppendLine();
            sb.AppendLine("### 4. **タスク管理の精度と効率性**");
            sb.AppendLine("   - 見積もり精度の向上ポイント");
            sb.AppendLine("   - タスク完了率と品質のバランス");
            sb.AppendLine("   - 中断や延期が多いタスクの特徴");
            sb.AppendLine();
            sb.AppendLine("### 5. **集中力と深い作業の質**");
            sb.AppendLine("   - ポモドーロテクニックの効果的な活用状況");
            sb.AppendLine("   - 長時間集中が必要なタスクへの取り組み方");
            sb.AppendLine("   - 集中を阻害する要因の特定");
            sb.AppendLine();
            sb.AppendLine("## 🎯 求める出力形式:");
            sb.AppendLine();
            sb.AppendLine("### **エグゼクティブサマリー**");
            sb.AppendLine("- 期間全体の成果を3-5つの重要ポイントで要約");
            sb.AppendLine("- 数値データに基づく客観的評価");
            sb.AppendLine();
            sb.AppendLine("### **強み（継続すべきポイント）**");
            sb.AppendLine("- データから明らかになった優秀な習慣や判断");
            sb.AppendLine("- さらに強化すべき長所");
            sb.AppendLine();
            sb.AppendLine("### **改善機会（重要度順）**");
            sb.AppendLine("- 最もインパクトの大きい改善ポイント3つ");
            sb.AppendLine("- 具体的な実践方法と期待される効果");
            sb.AppendLine();
            sb.AppendLine("### **次期間への戦略的提案**");
            sb.AppendLine("- 短期目標（1-2週間）と中期目標（1ヶ月）の設定案");
            sb.AppendLine("- 生産性向上のための具体的アクション");
            sb.AppendLine("- 測定可能な成功指標の提案");
            sb.AppendLine();
            sb.AppendLine("## 📈 期間データサマリー:");
            sb.AppendLine($"**分析対象期間**: {activityData.StartDate:yyyy年MM月dd日} ～ {activityData.EndDate:yyyy年MM月dd日} ({activityData.TotalDays}日間)");
            sb.AppendLine($"**総集中時間**: {activityData.Summary.TotalFocusTimeMinutes}分 ({activityData.Summary.TotalFocusTimeMinutes/60.0:F1}時間)");
            sb.AppendLine($"**完了ポモドーロ数**: {activityData.Summary.TotalPomodorosCompleted}個");
            sb.AppendLine($"**完了タスク数**: {activityData.Summary.TotalTasksCompleted}個");
            sb.AppendLine($"**1日平均集中時間**: {activityData.PeriodProductivityMetrics.AverageFocusTimePerDay:F1}分");
            sb.AppendLine($"**1日平均タスク完了数**: {activityData.PeriodProductivityMetrics.AverageTasksPerDay:F1}個");
            sb.AppendLine($"**生産性一貫性スコア**: {activityData.PeriodProductivityMetrics.ConsistencyScore:F1}%");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
            sb.AppendLine("## 📎 使用方法:");
            sb.AppendLine("1. このプロンプトと一緒に添付のJSONファイルをChatGPT、Claude、Geminiなどのに渡してください");
            sb.AppendLine("2. AIが詳細なデータ分析と改善提案を行います");
            sb.AppendLine("3. 提案された改善策を実際の作業に取り入れて生産性を向上させましょう");
            sb.AppendLine();
            sb.AppendLine($"**📄 添付データファイル**: {jsonFileName}");
            sb.AppendLine();
            sb.AppendLine("💡 **ヒント**: 分析結果を参考に、次の期間の作業計画を立てることで、");
            sb.AppendLine("継続的な生産性向上のサイクルを作ることができます。");

            return sb.ToString();
        }
    }
}
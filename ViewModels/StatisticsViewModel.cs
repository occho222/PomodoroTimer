using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// 統計画面のViewModel
    /// </summary>
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IPomodoroService _pomodoroService;

        // 日次統計プロパティ
        [ObservableProperty]
        private DailyStatistics todayStatistics = new();

        [ObservableProperty]
        private WeeklyStatistics weeklyStatistics = new();

        [ObservableProperty]
        private ObservableCollection<DailyStatistics> last7DaysStatistics = new();

        // プロジェクト別統計プロパティ
        [ObservableProperty]
        private ObservableCollection<ProjectStatistics> projectStatistics = new();

        // チャート用データ
        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> dailyPomodoroChart = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> categoryDistributionChart = new();

        // 表示期間設定
        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private StatisticsPeriod selectedPeriod = StatisticsPeriod.Today;

        // サマリー情報
        [ObservableProperty]
        private string totalPomodorosText = "0";

        [ObservableProperty]
        private string totalTasksText = "0";

        [ObservableProperty]
        private string averageFocusTimeText = "0分";

        [ObservableProperty]
        private string focusStreakText = "0日";

        [ObservableProperty]
        private double focusScoreProgress = 0;

        public StatisticsViewModel(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));

            LoadStatisticsData();
        }

        /// <summary>
        /// 統計データを読み込む
        /// </summary>
        [RelayCommand]
        private void LoadStatisticsData()
        {
            switch (SelectedPeriod)
            {
                case StatisticsPeriod.Today:
                    LoadTodayStatistics();
                    break;
                case StatisticsPeriod.Week:
                    LoadWeeklyStatistics();
                    break;
                case StatisticsPeriod.Month:
                    LoadMonthlyStatistics();
                    break;
            }

            LoadProjectStatistics();
            UpdateChartData();
            UpdateSummaryInfo();
        }

        /// <summary>
        /// 本日の統計を読み込む
        /// </summary>
        private void LoadTodayStatistics()
        {
            TodayStatistics = _statisticsService.GetDailyStatistics(SelectedDate);
        }

        /// <summary>
        /// 週次統計を読み込む
        /// </summary>
        private void LoadWeeklyStatistics()
        {
            WeeklyStatistics = _statisticsService.GetWeeklyStatistics(SelectedDate);
            
            Last7DaysStatistics.Clear();
            foreach (var dailyStat in WeeklyStatistics.DailyStatistics)
            {
                Last7DaysStatistics.Add(dailyStat);
            }
        }

        /// <summary>
        /// 月次統計を読み込む
        /// </summary>
        private void LoadMonthlyStatistics()
        {
            var monthStart = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            Last7DaysStatistics.Clear();
            var currentDate = monthStart;
            while (currentDate <= monthEnd)
            {
                var dailyStat = _statisticsService.GetDailyStatistics(currentDate);
                Last7DaysStatistics.Add(dailyStat);
                currentDate = currentDate.AddDays(1);
            }
        }

        /// <summary>
        /// プロジェクト別統計を読み込む
        /// </summary>
        private void LoadProjectStatistics()
        {
            var startDate = SelectedPeriod switch
            {
                StatisticsPeriod.Today => SelectedDate,
                StatisticsPeriod.Week => SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek),
                StatisticsPeriod.Month => new DateTime(SelectedDate.Year, SelectedDate.Month, 1),
                _ => SelectedDate
            };

            var endDate = SelectedPeriod switch
            {
                StatisticsPeriod.Today => SelectedDate,
                StatisticsPeriod.Week => startDate.AddDays(6),
                StatisticsPeriod.Month => startDate.AddMonths(1).AddDays(-1),
                _ => SelectedDate
            };

            var projectStats = _statisticsService.GetProjectStatistics(startDate, endDate);
            
            ProjectStatistics.Clear();
            foreach (var stat in projectStats.Values.OrderByDescending(p => p.CompletedPomodoros))
            {
                ProjectStatistics.Add(stat);
            }
        }

        /// <summary>
        /// チャートデータを更新する
        /// </summary>
        private void UpdateChartData()
        {
            // 日別ポモドーロ数チャート
            DailyPomodoroChart.Clear();
            foreach (var stat in Last7DaysStatistics.Take(7))
            {
                DailyPomodoroChart.Add(new ChartDataPoint
                {
                    Label = stat.Date.ToString("M/d"),
                    Value = stat.CompletedPomodoros,
                    Color = "#6366F1"
                });
            }

            // カテゴリ別分散チャート
            CategoryDistributionChart.Clear();
            var totalPomodoros = ProjectStatistics.Sum(p => p.CompletedPomodoros);
            
            if (totalPomodoros > 0)
            {
                var colors = new[] { "#6366F1", "#EC4899", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6" };
                int colorIndex = 0;

                foreach (var project in ProjectStatistics.Take(6))
                {
                    var percentage = (double)project.CompletedPomodoros / totalPomodoros * 100;
                    CategoryDistributionChart.Add(new ChartDataPoint
                    {
                        Label = project.ProjectName,
                        Value = percentage,
                        Color = colors[colorIndex % colors.Length]
                    });
                    colorIndex++;
                }
            }
        }

        /// <summary>
        /// サマリー情報を更新する
        /// </summary>
        private void UpdateSummaryInfo()
        {
            var totalPomodoros = SelectedPeriod switch
            {
                StatisticsPeriod.Today => TodayStatistics.CompletedPomodoros,
                StatisticsPeriod.Week => WeeklyStatistics.TotalPomodoros,
                StatisticsPeriod.Month => Last7DaysStatistics.Sum(d => d.CompletedPomodoros),
                _ => 0
            };

            var totalTasks = SelectedPeriod switch
            {
                StatisticsPeriod.Today => TodayStatistics.CompletedTasks,
                StatisticsPeriod.Week => WeeklyStatistics.TotalTasks,
                StatisticsPeriod.Month => Last7DaysStatistics.Sum(d => d.CompletedTasks),
                _ => 0
            };

            TotalPomodorosText = totalPomodoros.ToString();
            TotalTasksText = totalTasks.ToString();

            // 平均集中時間の計算
            var totalMinutes = SelectedPeriod switch
            {
                StatisticsPeriod.Today => TodayStatistics.TotalFocusMinutes,
                StatisticsPeriod.Week => WeeklyStatistics.TotalFocusMinutes,
                StatisticsPeriod.Month => Last7DaysStatistics.Sum(d => d.TotalFocusMinutes),
                _ => 0
            };

            var days = SelectedPeriod switch
            {
                StatisticsPeriod.Today => 1,
                StatisticsPeriod.Week => 7,
                StatisticsPeriod.Month => Last7DaysStatistics.Count,
                _ => 1
            };

            var averageMinutes = days > 0 ? totalMinutes / days : 0;
            AverageFocusTimeText = $"{averageMinutes}分";

            // 集中度スコア
            var focusScore = SelectedPeriod switch
            {
                StatisticsPeriod.Today => TodayStatistics.FocusScore,
                StatisticsPeriod.Week => WeeklyStatistics.AverageFocusScore,
                StatisticsPeriod.Month => Last7DaysStatistics.Where(d => d.FocusScore > 0).DefaultIfEmpty().Average(d => d?.FocusScore ?? 0),
                _ => 0
            };

            FocusScoreProgress = focusScore;

            // 集中ストリーク（連続集中日数）の計算
            CalculateFocusStreak();
        }

        /// <summary>
        /// 集中ストリークを計算する
        /// </summary>
        private void CalculateFocusStreak()
        {
            int streak = 0;
            var currentDate = DateTime.Today;

            while (true)
            {
                var dailyStat = _statisticsService.GetDailyStatistics(currentDate);
                if (dailyStat.CompletedPomodoros > 0)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            FocusStreakText = $"{streak}日";
        }

        /// <summary>
        /// 統計データをエクスポートする
        /// </summary>
        [RelayCommand]
        private async Task ExportStatistics()
        {
            try
            {
                // CSVエクスポートの実装
                var fileName = $"statistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                
                // 統計データのCSV出力
                await ExportStatisticsToCsv(filePath);
                
                System.Windows.MessageBox.Show($"統計データを {fileName} にエクスポートしました。", "エクスポート完了", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 統計データをCSVにエクスポートする
        /// </summary>
        /// <param name="filePath">出力ファイルパス</param>
        private async Task ExportStatisticsToCsv(string filePath)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("日付,完了ポモドーロ数,完了タスク数,集中時間(分),集中度スコア");

            foreach (var stat in Last7DaysStatistics)
            {
                csv.AppendLine($"{stat.Date:yyyy-MM-dd},{stat.CompletedPomodoros},{stat.CompletedTasks},{stat.TotalFocusMinutes},{stat.FocusScore:F1}");
            }

            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// 前の期間に移動する
        /// </summary>
        [RelayCommand]
        private void MoveToPreviousPeriod()
        {
            SelectedDate = SelectedPeriod switch
            {
                StatisticsPeriod.Today => SelectedDate.AddDays(-1),
                StatisticsPeriod.Week => SelectedDate.AddDays(-7),
                StatisticsPeriod.Month => SelectedDate.AddMonths(-1),
                _ => SelectedDate
            };

            LoadStatisticsData();
        }

        /// <summary>
        /// 次の期間に移動する
        /// </summary>
        [RelayCommand]
        private void MoveToNextPeriod()
        {
            SelectedDate = SelectedPeriod switch
            {
                StatisticsPeriod.Today => SelectedDate.AddDays(1),
                StatisticsPeriod.Week => SelectedDate.AddDays(7),
                StatisticsPeriod.Month => SelectedDate.AddMonths(1),
                _ => SelectedDate
            };

            LoadStatisticsData();
        }
    }

    /// <summary>
    /// 統計期間の種類
    /// </summary>
    public enum StatisticsPeriod
    {
        Today,
        Week,
        Month
    }

    /// <summary>
    /// チャート用のデータポイント
    /// </summary>
    public partial class ChartDataPoint : ObservableObject
    {
        [ObservableProperty]
        private string label = string.Empty;

        [ObservableProperty]
        private double value = 0;

        [ObservableProperty]
        private string color = "#6366F1";
    }
}
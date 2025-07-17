using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Text;

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

        // 新機能：プロジェクト別統計
        [ObservableProperty]
        private ObservableCollection<ProjectStatistics> projectStatistics = new();

        // 新機能：タグ別統計
        [ObservableProperty]
        private ObservableCollection<TagStatistics> tagStatistics = new();

        // 新機能：週次レポート
        [ObservableProperty]
        private WeeklyReport? currentWeekReport;

        // 新機能：月間統計
        [ObservableProperty]
        private MonthlyStatistics? currentMonthStats;

        // 新機能：生産性トレンド（過去2週間）
        [ObservableProperty]
        private ObservableCollection<ProductivityTrend> productivityTrend = new();

        // 新機能：時間帯別生産性
        [ObservableProperty]
        private ObservableCollection<HourlyProductivity> hourlyProductivity = new();

        // 新機能：トップカテゴリ
        [ObservableProperty]
        private List<(string Category, int FocusMinutes, int CompletedPomodoros)> topCategories = new();

        // 新機能：トップタグ
        [ObservableProperty]
        private List<(string Tag, int FocusMinutes, int CompletedPomodoros)> topTags = new();

        // 統計期間の選択
        [ObservableProperty]
        private DateTime selectedStartDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime selectedEndDate = DateTime.Today;

        // タブ選択
        [ObservableProperty]
        private int selectedTabIndex = 0;

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
                LoadBasicStatistics();
                LoadProjectAndTagStatistics();
                LoadWeeklyReport();
                LoadMonthlyStatistics();
                LoadProductivityTrend();
                LoadHourlyProductivity();
                LoadRankings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"統計データの読み込みに失敗しました: {ex.Message}");
            }
        }

        private void LoadBasicStatistics()
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

        private void LoadProjectAndTagStatistics()
        {
            // プロジェクト別統計
            var projects = _statisticsService.GetProjectStatistics(SelectedStartDate, SelectedEndDate);
            ProjectStatistics.Clear();
            foreach (var project in projects.Values.OrderByDescending(p => p.FocusMinutes))
            {
                ProjectStatistics.Add(project);
            }

            // タグ別統計
            var tags = _statisticsService.GetTagStatistics(SelectedStartDate, SelectedEndDate);
            TagStatistics.Clear();
            foreach (var tag in tags.Values.OrderByDescending(t => t.FocusMinutes))
            {
                TagStatistics.Add(tag);
            }
        }

        private void LoadWeeklyReport()
        {
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            CurrentWeekReport = _statisticsService.GetWeeklyReport(weekStart);
        }

        private void LoadMonthlyStatistics()
        {
            CurrentMonthStats = _statisticsService.GetMonthlyStatistics(DateTime.Today.Year, DateTime.Today.Month);
        }

        private void LoadProductivityTrend()
        {
            var trendData = _statisticsService.GetProductivityTrend(DateTime.Today.AddDays(-14), DateTime.Today);
            ProductivityTrend.Clear();
            foreach (var trend in trendData)
            {
                ProductivityTrend.Add(trend);
            }
        }

        private void LoadHourlyProductivity()
        {
            var hourlyData = _statisticsService.GetHourlyProductivity(SelectedStartDate, SelectedEndDate);
            HourlyProductivity.Clear();
            foreach (var hour in hourlyData.Values.Where(h => h.Pomodoros > 0).OrderBy(h => h.Hour))
            {
                HourlyProductivity.Add(hour);
            }
        }

        private void LoadRankings()
        {
            TopCategories = _statisticsService.GetCategoryRanking(SelectedStartDate, SelectedEndDate, 5);
            TopTags = _statisticsService.GetTagRanking(SelectedStartDate, SelectedEndDate, 5);
        }

        [RelayCommand]
        private void RefreshStatistics()
        {
            LoadStatistics();
        }

        [RelayCommand]
        private void UpdateDateRange()
        {
            LoadProjectAndTagStatistics();
            LoadHourlyProductivity();
            LoadRankings();
        }

        [RelayCommand]
        private void ExportStatistics()
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSVファイル (*.csv)|*.csv",
                    Title = "統計データのエクスポート",
                    FileName = $"統計データ_{DateTime.Today:yyyy-MM-dd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportStatisticsToCSV(saveFileDialog.FileName);
                    System.Windows.MessageBox.Show("統計データのエクスポートが完了しました。", "完了", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExportStatisticsToCSV(string filePath)
        {
            var csv = new StringBuilder();
            
            // ヘッダー行
            csv.AppendLine("データ種別,名前,完了ポモドーロ数,完了タスク数,集中時間(分)");
            
            // プロジェクト別統計
            foreach (var project in ProjectStatistics)
            {
                csv.AppendLine($"プロジェクト,{project.ProjectName},{project.CompletedPomodoros},{project.CompletedTasks},{project.FocusMinutes}");
            }
            
            // タグ別統計
            foreach (var tag in TagStatistics)
            {
                csv.AppendLine($"タグ,{tag.TagName},{tag.CompletedPomodoros},{tag.CompletedTasks},{tag.FocusMinutes}");
            }
            
            // 時間帯別統計
            foreach (var hourly in HourlyProductivity)
            {
                csv.AppendLine($"時間帯,{hourly.HourText},{hourly.Pomodoros},0,{hourly.FocusMinutes}");
            }
            
            System.IO.File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
        }

        private string FormatTime(int totalMinutes)
        {
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            return $"{hours}h {minutes}m";
        }
    }
}
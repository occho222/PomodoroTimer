using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Text;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// ���v��ʂ�ViewModel
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

        // �V�@�\�F�v���W�F�N�g�ʓ��v
        [ObservableProperty]
        private ObservableCollection<ProjectStatistics> projectStatistics = new();

        // �V�@�\�F�^�O�ʓ��v
        [ObservableProperty]
        private ObservableCollection<TagStatistics> tagStatistics = new();

        // �V�@�\�F�T�����|�[�g
        [ObservableProperty]
        private WeeklyReport? currentWeekReport;

        // �V�@�\�F���ԓ��v
        [ObservableProperty]
        private MonthlyStatistics? currentMonthStats;

        // �V�@�\�F���Y���g�����h�i�ߋ�2�T�ԁj
        [ObservableProperty]
        private ObservableCollection<ProductivityTrend> productivityTrend = new();

        // �V�@�\�F���ԑѕʐ��Y��
        [ObservableProperty]
        private ObservableCollection<HourlyProductivity> hourlyProductivity = new();

        // �V�@�\�F�g�b�v�J�e�S��
        [ObservableProperty]
        private List<(string Category, int FocusMinutes, int CompletedPomodoros)> topCategories = new();

        // �V�@�\�F�g�b�v�^�O
        [ObservableProperty]
        private List<(string Tag, int FocusMinutes, int CompletedPomodoros)> topTags = new();

        // ���v���Ԃ̑I��
        [ObservableProperty]
        private DateTime selectedStartDate = DateTime.Today.AddDays(-7);

        [ObservableProperty]
        private DateTime selectedEndDate = DateTime.Today;

        // �^�u�I��
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
                Console.WriteLine($"���v�f�[�^�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
            }
        }

        private void LoadBasicStatistics()
        {
            // �{���̓��v
            var today = _statisticsService.GetDailyStatistics(DateTime.Today);
            TodayPomodoros = today.CompletedPomodoros;
            TodayTasks = today.CompletedTasks;
            TodayFocusTime = FormatTime(today.TotalFocusMinutes);

            // ���T�̓��v
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var weekStats = _statisticsService.GetWeeklyStatistics(weekStart);
            WeekPomodoros = weekStats.Sum(d => d.CompletedPomodoros);
            WeekTasks = weekStats.Sum(d => d.CompletedTasks);
            WeekFocusTime = FormatTime(weekStats.Sum(d => d.TotalFocusMinutes));

            // ���v�̓��v
            var allStats = _statisticsService.GetAllTimeStatistics();
            TotalPomodoros = allStats.TotalPomodoros;
            TotalTasks = allStats.TotalCompletedTasks;
            TotalFocusTime = FormatTime(allStats.TotalFocusMinutes);
        }

        private void LoadProjectAndTagStatistics()
        {
            // �v���W�F�N�g�ʓ��v
            var projects = _statisticsService.GetProjectStatistics(SelectedStartDate, SelectedEndDate);
            ProjectStatistics.Clear();
            foreach (var project in projects.Values.OrderByDescending(p => p.FocusMinutes))
            {
                ProjectStatistics.Add(project);
            }

            // �^�O�ʓ��v
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
                    Filter = "CSV�t�@�C�� (*.csv)|*.csv",
                    Title = "���v�f�[�^�̃G�N�X�|�[�g",
                    FileName = $"���v�f�[�^_{DateTime.Today:yyyy-MM-dd}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportStatisticsToCSV(saveFileDialog.FileName);
                    System.Windows.MessageBox.Show("���v�f�[�^�̃G�N�X�|�[�g���������܂����B", "����", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"�G�N�X�|�[�g�Ɏ��s���܂���: {ex.Message}", "�G���[", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void ExportStatisticsToCSV(string filePath)
        {
            var csv = new StringBuilder();
            
            // �w�b�_�[�s
            csv.AppendLine("�f�[�^���,���O,�����|���h�[����,�����^�X�N��,�W������(��)");
            
            // �v���W�F�N�g�ʓ��v
            foreach (var project in ProjectStatistics)
            {
                csv.AppendLine($"�v���W�F�N�g,{project.ProjectName},{project.CompletedPomodoros},{project.CompletedTasks},{project.FocusMinutes}");
            }
            
            // �^�O�ʓ��v
            foreach (var tag in TagStatistics)
            {
                csv.AppendLine($"�^�O,{tag.TagName},{tag.CompletedPomodoros},{tag.CompletedTasks},{tag.FocusMinutes}");
            }
            
            // ���ԑѕʓ��v
            foreach (var hourly in HourlyProductivity)
            {
                csv.AppendLine($"���ԑ�,{hourly.HourText},{hourly.Pomodoros},0,{hourly.FocusMinutes}");
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
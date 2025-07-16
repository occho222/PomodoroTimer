using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// ���v��ʂ�ViewModel
    /// </summary>
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IPomodoroService _pomodoroService;

        // �������v�v���p�e�B
        [ObservableProperty]
        private DailyStatistics todayStatistics = new();

        [ObservableProperty]
        private WeeklyStatistics weeklyStatistics = new();

        [ObservableProperty]
        private ObservableCollection<DailyStatistics> last7DaysStatistics = new();

        // �v���W�F�N�g�ʓ��v�v���p�e�B
        [ObservableProperty]
        private ObservableCollection<ProjectStatistics> projectStatistics = new();

        // �`���[�g�p�f�[�^
        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> dailyPomodoroChart = new();

        [ObservableProperty]
        private ObservableCollection<ChartDataPoint> categoryDistributionChart = new();

        // �\�����Ԑݒ�
        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private StatisticsPeriod selectedPeriod = StatisticsPeriod.Today;

        // �T�}���[���
        [ObservableProperty]
        private string totalPomodorosText = "0";

        [ObservableProperty]
        private string totalTasksText = "0";

        [ObservableProperty]
        private string averageFocusTimeText = "0��";

        [ObservableProperty]
        private string focusStreakText = "0��";

        [ObservableProperty]
        private double focusScoreProgress = 0;

        public StatisticsViewModel(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            _statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));

            LoadStatisticsData();
        }

        /// <summary>
        /// ���v�f�[�^��ǂݍ���
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
        /// �{���̓��v��ǂݍ���
        /// </summary>
        private void LoadTodayStatistics()
        {
            TodayStatistics = _statisticsService.GetDailyStatistics(SelectedDate);
        }

        /// <summary>
        /// �T�����v��ǂݍ���
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
        /// �������v��ǂݍ���
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
        /// �v���W�F�N�g�ʓ��v��ǂݍ���
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
        /// �`���[�g�f�[�^���X�V����
        /// </summary>
        private void UpdateChartData()
        {
            // ���ʃ|���h�[�����`���[�g
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

            // �J�e�S���ʕ��U�`���[�g
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
        /// �T�}���[�����X�V����
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

            // ���ϏW�����Ԃ̌v�Z
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
            AverageFocusTimeText = $"{averageMinutes}��";

            // �W���x�X�R�A
            var focusScore = SelectedPeriod switch
            {
                StatisticsPeriod.Today => TodayStatistics.FocusScore,
                StatisticsPeriod.Week => WeeklyStatistics.AverageFocusScore,
                StatisticsPeriod.Month => Last7DaysStatistics.Where(d => d.FocusScore > 0).DefaultIfEmpty().Average(d => d?.FocusScore ?? 0),
                _ => 0
            };

            FocusScoreProgress = focusScore;

            // �W���X�g���[�N�i�A���W�������j�̌v�Z
            CalculateFocusStreak();
        }

        /// <summary>
        /// �W���X�g���[�N���v�Z����
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

            FocusStreakText = $"{streak}��";
        }

        /// <summary>
        /// ���v�f�[�^���G�N�X�|�[�g����
        /// </summary>
        [RelayCommand]
        private async Task ExportStatistics()
        {
            try
            {
                // CSV�G�N�X�|�[�g�̎���
                var fileName = $"statistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
                
                // ���v�f�[�^��CSV�o��
                await ExportStatisticsToCsv(filePath);
                
                System.Windows.MessageBox.Show($"���v�f�[�^�� {fileName} �ɃG�N�X�|�[�g���܂����B", "�G�N�X�|�[�g����", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"�G�N�X�|�[�g�Ɏ��s���܂���: {ex.Message}", "�G���[", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// ���v�f�[�^��CSV�ɃG�N�X�|�[�g����
        /// </summary>
        /// <param name="filePath">�o�̓t�@�C���p�X</param>
        private async Task ExportStatisticsToCsv(string filePath)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("���t,�����|���h�[����,�����^�X�N��,�W������(��),�W���x�X�R�A");

            foreach (var stat in Last7DaysStatistics)
            {
                csv.AppendLine($"{stat.Date:yyyy-MM-dd},{stat.CompletedPomodoros},{stat.CompletedTasks},{stat.TotalFocusMinutes},{stat.FocusScore:F1}");
            }

            await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// �O�̊��ԂɈړ�����
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
        /// ���̊��ԂɈړ�����
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
    /// ���v���Ԃ̎��
    /// </summary>
    public enum StatisticsPeriod
    {
        Today,
        Week,
        Month
    }

    /// <summary>
    /// �`���[�g�p�̃f�[�^�|�C���g
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
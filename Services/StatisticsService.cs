using PomodoroTimer.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ���v���T�[�r�X�̎���
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
            
            // �N�����ɓ��v�f�[�^��ǂݍ���
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

            // ��T�ԕ��̓��v�����W
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

            // �v���W�F�N�g�ʓ��v�̍X�V
            if (!string.IsNullOrEmpty(task.Category))
            {
                var projectStats = dailyStats.ProjectStatistics.GetValueOrDefault(task.Category, 
                    new ProjectStatistics { ProjectName = task.Category });
                
                projectStats.CompletedPomodoros++;
                projectStats.FocusMinutes += sessionDurationMinutes;
                
                dailyStats.ProjectStatistics[task.Category] = projectStats;
            }

            // �^�O�ʓ��v�̍X�V
            foreach (var tag in task.Tags.Where(t => !string.IsNullOrEmpty(t)))
            {
                var tagStats = dailyStats.TagStatistics.GetValueOrDefault(tag,
                    new TagStatistics { TagName = tag });
                
                tagStats.CompletedPomodoros++;
                tagStats.FocusMinutes += sessionDurationMinutes;
                
                dailyStats.TagStatistics[tag] = tagStats;
            }

            // �W���x�̌v�Z�i�ȈՔŁF���������|���h�[�����Ɋ�Â��j
            UpdateFocusScore(dailyStats);
            
            // ���v�f�[�^��ۑ�
            _ = Task.Run(SaveStatisticsAsync);
        }

        public void RecordTaskComplete(PomodoroTask task)
        {
            var today = DateTime.Today;
            var dailyStats = GetDailyStatistics(today);

            dailyStats.CompletedTasks++;

            // �v���W�F�N�g�ʓ��v�̍X�V
            if (!string.IsNullOrEmpty(task.Category))
            {
                var projectStats = dailyStats.ProjectStatistics.GetValueOrDefault(task.Category, 
                    new ProjectStatistics { ProjectName = task.Category });
                
                projectStats.CompletedTasks++;
                
                dailyStats.ProjectStatistics[task.Category] = projectStats;
            }

            // �^�O�ʓ��v�̍X�V
            foreach (var tag in task.Tags.Where(t => !string.IsNullOrEmpty(t)))
            {
                var tagStats = dailyStats.TagStatistics.GetValueOrDefault(tag,
                    new TagStatistics { TagName = tag });
                
                tagStats.CompletedTasks++;
                
                dailyStats.TagStatistics[tag] = tagStats;
            }

            UpdateFocusScore(dailyStats);
            
            // ���v�f�[�^��ۑ�
            _ = Task.Run(SaveStatisticsAsync);
        }

        public void UndoTaskComplete(PomodoroTask task)
        {
            var today = DateTime.Today;
            var dailyStats = GetDailyStatistics(today);

            // �S���̊������J�E���g�����Z
            if (dailyStats.CompletedTasks > 0)
            {
                dailyStats.CompletedTasks--;
            }

            // �v���W�F�N�g�ʓ��v�̍X�V
            if (!string.IsNullOrEmpty(task.Category) && 
                dailyStats.ProjectStatistics.ContainsKey(task.Category))
            {
                var projectStats = dailyStats.ProjectStatistics[task.Category];
                if (projectStats.CompletedTasks > 0)
                {
                    projectStats.CompletedTasks--;
                }
                
                // ���ی�������A0�̏ꍇ�͍폜
                if (projectStats.CompletedTasks == 0 && projectStats.FocusMinutes == 0 && projectStats.CompletedPomodoros == 0)
                {
                    dailyStats.ProjectStatistics.Remove(task.Category);
                }
                else
                {
                    dailyStats.ProjectStatistics[task.Category] = projectStats;
                }
            }

            // �^�O�ʓ��v�̍X�V
            foreach (var tag in task.Tags.Where(t => !string.IsNullOrEmpty(t)))
            {
                if (dailyStats.TagStatistics.ContainsKey(tag))
                {
                    var tagStats = dailyStats.TagStatistics[tag];
                    if (tagStats.CompletedTasks > 0)
                    {
                        tagStats.CompletedTasks--;
                    }
                    
                    // ���ی�������A0�̏ꍇ�͍폜
                    if (tagStats.CompletedTasks == 0 && tagStats.FocusMinutes == 0 && tagStats.CompletedPomodoros == 0)
                    {
                        dailyStats.TagStatistics.Remove(tag);
                    }
                    else
                    {
                        dailyStats.TagStatistics[tag] = tagStats;
                    }
                }
            }

            UpdateFocusScore(dailyStats);
            
            // ���v�f�[�^��ۑ�
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
        /// �^�O�ʓ��v���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>�^�O�ʓ��v�̃f�B�N�V���i��</returns>
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
        /// �J�e�S���ʂ̍�Ǝ��ԃ����L���O���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <param name="topCount">��ʉ��ʂ܂Ŏ擾���邩</param>
        /// <returns>��Ǝ��ԏ��̃J�e�S�������L���O</returns>
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
        /// �^�O�ʂ̍�Ǝ��ԃ����L���O���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <param name="topCount">��ʉ��ʂ܂Ŏ擾���邩</param>
        /// <returns>��Ǝ��ԏ��̃^�O�����L���O</returns>
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
        /// �T�����|�[�g���擾����
        /// </summary>
        /// <param name="weekStart">�T�̊J�n��</param>
        /// <returns>�T�����|�[�g</returns>
        public WeeklyReport GetWeeklyReport(DateTime weekStart)
        {
            var weekStartDate = weekStart.Date.AddDays(-(int)weekStart.DayOfWeek);
            var weeklyStats = GetWeeklyStatistics(weekStartDate);
            
            var report = new WeeklyReport
            {
                WeekStart = weekStartDate,
                DailyStatistics = weeklyStats
            };

            // �ł��g�p���ꂽ�v���W�F�N�g���擾
            var projectStats = GetProjectStatistics(weekStartDate, weekStartDate.AddDays(6));
            var topProject = projectStats.OrderByDescending(p => p.Value.FocusMinutes).FirstOrDefault();
            report.TopProject = topProject.Key ?? "�Ȃ�";

            // �ł��g�p���ꂽ�^�O���擾
            var tagStats = GetTagStatistics(weekStartDate, weekStartDate.AddDays(6));
            var topTag = tagStats.OrderByDescending(t => t.Value.FocusMinutes).FirstOrDefault();
            report.TopTag = topTag.Key ?? "�Ȃ�";

            // �O�T�Ƃ̔�r
            var lastWeekStart = weekStartDate.AddDays(-7);
            var lastWeekStats = GetWeeklyStatistics(lastWeekStart);
            var lastWeekPomodoros = lastWeekStats.Sum(d => d.CompletedPomodoros);
            report.PomodoroChangeFromLastWeek = report.TotalPomodoros - lastWeekPomodoros;

            return report;
        }

        /// <summary>
        /// ���ԓ��v���擾����
        /// </summary>
        /// <param name="year">�N</param>
        /// <param name="month">��</param>
        /// <returns>���ԓ��v</returns>
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
        /// ���Y���g�����h���擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>���Y���g�����h�f�[�^</returns>
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
        /// ���ԑѕʍ�ƕ��͂��擾����
        /// </summary>
        /// <param name="startDate">�J�n��</param>
        /// <param name="endDate">�I����</param>
        /// <returns>���ԑѕʍ�ƕ���</returns>
        public Dictionary<int, HourlyProductivity> GetHourlyProductivity(DateTime startDate, DateTime endDate)
        {
            // �ȈՎ����F���ۂ̎��ԑуf�[�^���K�v�ȏꍇ�́A�Z�b�V�����J�n���Ԃ̋L�^�@�\��ǉ�����K�v������܂�
            var hourlyStats = new Dictionary<int, HourlyProductivity>();

            // 0-23���̏�����
            for (int hour = 0; hour < 24; hour++)
            {
                hourlyStats[hour] = new HourlyProductivity { Hour = hour };
            }

            // ���݂̓T���v���f�[�^�Ƃ��āA��ʓI�ȍ�Ǝ��ԑтɃf�[�^��z��
            var currentDate = startDate.Date;
            while (currentDate <= endDate.Date)
            {
                var dailyStats = GetDailyStatistics(currentDate);
                if (dailyStats.CompletedPomodoros > 0)
                {
                    // ��Ǝ��Ԃ�(9-12)�A��(13-17)�A��(19-22)�ɕ��U
                    var morningPomodoros = (int)(dailyStats.CompletedPomodoros * 0.4);
                    var afternoonPomodoros = (int)(dailyStats.CompletedPomodoros * 0.5);
                    var eveningPomodoros = dailyStats.CompletedPomodoros - morningPomodoros - afternoonPomodoros;

                    // ���̎��ԑ�
                    for (int h = 9; h <= 11; h++)
                    {
                        hourlyStats[h].Pomodoros += morningPomodoros / 3;
                        hourlyStats[h].FocusMinutes += (dailyStats.TotalFocusMinutes * 40 / 100) / 3;
                    }

                    // ���̎��ԑ�
                    for (int h = 13; h <= 17; h++)
                    {
                        hourlyStats[h].Pomodoros += afternoonPomodoros / 5;
                        hourlyStats[h].FocusMinutes += (dailyStats.TotalFocusMinutes * 50 / 100) / 5;
                    }

                    // ��̎��ԑ�
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
                // ���O�L�^�i�������ɓK�؂ȃ��K�[���g�p�j
                Console.WriteLine($"���v�f�[�^�̕ۑ��Ɏ��s���܂���: {ex.Message}");
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
                // ���O�L�^�i�������ɓK�؂ȃ��K�[���g�p�j
                Console.WriteLine($"���v�f�[�^�̓ǂݍ��݂Ɏ��s���܂���: {ex.Message}");
            }
        }

        /// <summary>
        /// �W���x�X�R�A���X�V����
        /// </summary>
        /// <param name="dailyStats">�������v</param>
        private void UpdateFocusScore(DailyStatistics dailyStats)
        {
            // �ȈՓI�ȏW���x�v�Z�F�����|���h�[�����Ɗ����^�X�N���Ɋ�Â�
            var pomodoroScore = Math.Min(dailyStats.CompletedPomodoros * 10, 70);
            var taskScore = Math.Min(dailyStats.CompletedTasks * 15, 30);
            
            dailyStats.FocusScore = Math.Min(pomodoroScore + taskScore, 100);
        }
    }
}
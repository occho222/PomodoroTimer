using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using PomodoroTimer.Views;
using System.Collections.ObjectModel;
using System.Windows;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// �|���h�[���^�C�}�[�̃��C���r���[���f��
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly ITimerService _timerService;

        // �^�X�N�֘A�v���p�e�B
        [ObservableProperty]
        private ObservableCollection<PomodoroTask> tasks = new();

        [ObservableProperty]
        private PomodoroTask? currentTask;

        // �^�C�}�[�֘A�v���p�e�B
        [ObservableProperty]
        private string timeRemaining = "25:00";

        [ObservableProperty]
        private string sessionTypeText = "Work Session";

        [ObservableProperty]
        private string startPauseButtonText = "Start";

        [ObservableProperty]
        private bool isRunning = false;

        // ���v�֘A�v���p�e�B
        [ObservableProperty]
        private int completedPomodoros = 0;

        [ObservableProperty]
        private int completedTasks = 0;

        [ObservableProperty]
        private string totalFocusTime = "0h 0m";

        // UI�֘A�v���p�e�B
        [ObservableProperty]
        private Point progressPoint = new(150, 30);

        [ObservableProperty]
        private bool isLargeArc = false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="pomodoroService">�|���h�[���T�[�r�X</param>
        /// <param name="timerService">�^�C�}�[�T�[�r�X</param>
        public MainViewModel(IPomodoroService pomodoroService, ITimerService timerService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));

            // �T�[�r�X����^�X�N���擾
            Tasks = _pomodoroService.GetTasks();

            // �^�C�}�[�C�x���g���w��
            SubscribeToTimerEvents();

            // �����\���X�V
            UpdateProgress();
        }

        /// <summary>
        /// �^�C�}�[�C�x���g���w�ǂ���
        /// </summary>
        private void SubscribeToTimerEvents()
        {
            _timerService.TimerStarted += OnTimerStarted;
            _timerService.TimerStopped += OnTimerStopped;
            _timerService.TimerPaused += OnTimerPaused;
            _timerService.TimerResumed += OnTimerResumed;
            _timerService.TimeUpdated += OnTimeUpdated;
            _timerService.SessionCompleted += OnSessionCompleted;
        }

        /// <summary>
        /// �J�n/�ꎞ��~�R�}���h
        /// </summary>
        [RelayCommand]
        private void StartPause()
        {
            if (IsRunning)
            {
                _timerService.Pause();
            }
            else
            {
                if (_timerService.RemainingTime <= TimeSpan.Zero)
                {
                    // �V�����Z�b�V�������J�n
                    _timerService.Start(TimeSpan.FromMinutes(25));
                }
                else
                {
                    // �ꎞ��~����ĊJ
                    _timerService.Resume();
                }
            }
        }

        /// <summary>
        /// ��~�R�}���h
        /// </summary>
        [RelayCommand]
        private void Stop()
        {
            _timerService.Stop();
        }

        /// <summary>
        /// �X�L�b�v�R�}���h
        /// </summary>
        [RelayCommand]
        private void Skip()
        {
            _timerService.Skip();
        }

        /// <summary>
        /// �^�X�N�J�n�R�}���h
        /// </summary>
        /// <param name="task">�J�n����^�X�N</param>
        [RelayCommand]
        private void StartTask(PomodoroTask task)
        {
            CurrentTask = task;
            Stop(); // ���݂̃^�C�}�[���~���ă��Z�b�g
        }

        /// <summary>
        /// �^�X�N�ǉ��R�}���h
        /// </summary>
        [RelayCommand]
        private void AddTask()
        {
            var dialog = new TaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var newTask = new PomodoroTask(dialog.TaskTitle, dialog.EstimatedPomodoros);
                _pomodoroService.AddTask(newTask);
            }
        }

        /// <summary>
        /// �ݒ��ʂ��J���R�}���h
        /// </summary>
        [RelayCommand]
        private void OpenSettings()
        {
            MessageBox.Show("Settings will be implemented in the future.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// �^�X�N�̏�����ύX����
        /// </summary>
        /// <param name="draggedTask">�h���b�O���ꂽ�^�X�N</param>
        /// <param name="targetTask">�h���b�v��̃^�X�N</param>
        public void ReorderTasks(PomodoroTask draggedTask, PomodoroTask targetTask)
        {
            var draggedIndex = Tasks.IndexOf(draggedTask);
            var targetIndex = Tasks.IndexOf(targetTask);

            if (draggedIndex != -1 && targetIndex != -1)
            {
                _pomodoroService.ReorderTasks(draggedIndex, targetIndex);
            }
        }

        /// <summary>
        /// �ݒ��ۑ�����
        /// </summary>
        public void SaveSettings()
        {
            // �ݒ�ۑ��̎����i���������\��j
        }

        #region �^�C�}�[�C�x���g�n���h��

        private void OnTimerStarted()
        {
            IsRunning = true;
            StartPauseButtonText = "Pause";
        }

        private void OnTimerStopped()
        {
            IsRunning = false;
            StartPauseButtonText = "Start";
        }

        private void OnTimerPaused()
        {
            IsRunning = false;
            StartPauseButtonText = "Resume";
        }

        private void OnTimerResumed()
        {
            IsRunning = true;
            StartPauseButtonText = "Pause";
        }

        private void OnTimeUpdated(TimeSpan remainingTime)
        {
            TimeRemaining = $"{remainingTime.Minutes:D2}:{remainingTime.Seconds:D2}";
            UpdateProgress();
        }

        private void OnSessionCompleted()
        {
            // �Z�b�V������������
            CompletedPomodoros++;
            
            if (CurrentTask != null)
            {
                _pomodoroService.IncrementTaskPomodoro(CurrentTask);
                
                if (CurrentTask.IsCompleted)
                {
                    CompletedTasks++;
                }
            }

            // �W�����Ԃ��X�V
            UpdateTotalFocusTime();

            // �����ʒm
            MessageBox.Show("Session completed! Great work.", "Pomodoro Timer", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            // ���̃Z�b�V�����̏���
            IsRunning = false;
            StartPauseButtonText = "Start";
        }

        #endregion

        /// <summary>
        /// �i���\�����X�V����
        /// </summary>
        private void UpdateProgress()
        {
            var totalSeconds = _timerService.SessionDuration.TotalSeconds;
            var remainingSeconds = _timerService.RemainingTime.TotalSeconds;
            
            if (totalSeconds <= 0) return;

            var progress = (totalSeconds - remainingSeconds) / totalSeconds;
            var angle = progress * 360;
            var radians = (angle - 90) * Math.PI / 180;

            var x = 150 + 120 * Math.Cos(radians);
            var y = 150 + 120 * Math.Sin(radians);

            ProgressPoint = new Point(x, y);
            IsLargeArc = angle > 180;
        }

        /// <summary>
        /// ���W�����Ԃ��X�V����
        /// </summary>
        private void UpdateTotalFocusTime()
        {
            var totalMinutes = CompletedPomodoros * 25; // 1�|���h�[�� = 25���Ƃ��Čv�Z
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            TotalFocusTime = $"{hours}h {minutes}m";
        }
    }
}
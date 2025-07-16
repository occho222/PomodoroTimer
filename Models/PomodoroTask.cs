using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// �|���h�[���^�X�N�̃��f��
    /// </summary>
    public partial class PomodoroTask : ObservableObject
    {
        /// <summary>
        /// �^�X�N�̃^�C�g��
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// �\��|���h�[����
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
        [NotifyPropertyChangedFor(nameof(RemainingPomodoros))]
        private int estimatedPomodoros = 1;

        /// <summary>
        /// �����|���h�[����
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
        [NotifyPropertyChangedFor(nameof(RemainingPomodoros))]
        private int completedPomodoros = 0;

        /// <summary>
        /// �^�X�N���������Ă��邩�ǂ���
        /// </summary>
        [ObservableProperty]
        private bool isCompleted = false;

        /// <summary>
        /// �^�X�N�̍쐬����
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        /// <summary>
        /// �^�X�N�̊�������
        /// </summary>
        [ObservableProperty]
        private DateTime? completedAt;

        /// <summary>
        /// �^�X�N�̗D��x
        /// </summary>
        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;

        /// <summary>
        /// �^�X�N�̐���
        /// </summary>
        [ObservableProperty]
        private string description = string.Empty;

        /// <summary>
        /// �^�X�N�̃J�e�S��
        /// </summary>
        [ObservableProperty]
        private string category = string.Empty;

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public PomodoroTask()
        {
        }

        /// <summary>
        /// �^�C�g���Ɨ\��|���h�[�������w�肷��R���X�g���N�^
        /// </summary>
        /// <param name="title">�^�X�N�̃^�C�g��</param>
        /// <param name="estimatedPomodoros">�\��|���h�[����</param>
        public PomodoroTask(string title, int estimatedPomodoros = 1)
        {
            Title = title ?? string.Empty;
            EstimatedPomodoros = Math.Max(1, estimatedPomodoros);
        }

        /// <summary>
        /// �^�X�N�̐i�������擾����
        /// </summary>
        public double ProgressPercentage
        {
            get
            {
                try
                {
                    if (EstimatedPomodoros <= 0) return 0;
                    var progress = (double)CompletedPomodoros / EstimatedPomodoros * 100;
                    return Math.Min(100, Math.Max(0, progress));
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// �c��\��|���h�[�������擾����
        /// </summary>
        public int RemainingPomodoros
        {
            get
            {
                try
                {
                    return Math.Max(0, EstimatedPomodoros - CompletedPomodoros);
                }
                catch
                {
                    return 0;
                }
            }
        }
    }

    /// <summary>
    /// �^�X�N�̗D��x
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// ��D��x
        /// </summary>
        Low,
        
        /// <summary>
        /// ���D��x
        /// </summary>
        Medium,
        
        /// <summary>
        /// ���D��x
        /// </summary>
        High,
        
        /// <summary>
        /// �ً}
        /// </summary>
        Urgent
    }
}
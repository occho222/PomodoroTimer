using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// ポモドーロタスクのモデル
    /// </summary>
    public partial class PomodoroTask : ObservableObject
    {
        /// <summary>
        /// タスクの一意のID
        /// </summary>
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        /// <summary>
        /// タスクのタイトル
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// 予定ポモドーロ数
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
        [NotifyPropertyChangedFor(nameof(RemainingPomodoros))]
        private int estimatedPomodoros = 1;

        /// <summary>
        /// 完了ポモドーロ数
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ProgressPercentage))]
        [NotifyPropertyChangedFor(nameof(RemainingPomodoros))]
        private int completedPomodoros = 0;

        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        [ObservableProperty]
        private bool isCompleted = false;

        /// <summary>
        /// タスクの作成日時
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        /// <summary>
        /// タスクの完了日時
        /// </summary>
        [ObservableProperty]
        private DateTime? completedAt;

        /// <summary>
        /// タスクの優先度
        /// </summary>
        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;

        /// <summary>
        /// タスクの説明
        /// </summary>
        [ObservableProperty]
        private string description = string.Empty;

        /// <summary>
        /// タスクのカテゴリ
        /// </summary>
        [ObservableProperty]
        private string category = string.Empty;

        /// <summary>
        /// タスクのタグリスト
        /// </summary>
        [ObservableProperty]
        private List<string> tags = new();

        /// <summary>
        /// 表示順序
        /// </summary>
        [ObservableProperty]
        private int displayOrder = 0;

        /// <summary>
        /// タグの文字列表現（カンマ区切り）
        /// </summary>
        public string TagsText
        {
            get => string.Join(", ", Tags);
            set
            {
                var newTags = value?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(tag => tag.Trim())
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .ToList() ?? new List<string>();
                
                if (!Tags.SequenceEqual(newTags))
                {
                    Tags = newTags;
                    OnPropertyChanged(nameof(TagsText));
                }
            }
        }

        /// <summary>
        /// 優先度の表示テキスト
        /// </summary>
        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "低",
            TaskPriority.Medium => "中",
            TaskPriority.High => "高",
            TaskPriority.Urgent => "緊急",
            _ => "中"
        };

        /// <summary>
        /// 優先度の色
        /// </summary>
        public string PriorityColor => Priority switch
        {
            TaskPriority.Low => "Green",
            TaskPriority.Medium => "Orange",
            TaskPriority.High => "Red",
            TaskPriority.Urgent => "DarkRed",
            _ => "Orange"
        };

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PomodoroTask()
        {
        }

        /// <summary>
        /// タイトルと予定ポモドーロ数を指定するコンストラクタ
        /// </summary>
        /// <param name="title">タスクのタイトル</param>
        /// <param name="estimatedPomodoros">予定ポモドーロ数</param>
        public PomodoroTask(string title, int estimatedPomodoros = 1)
        {
            Title = title ?? string.Empty;
            EstimatedPomodoros = Math.Max(1, estimatedPomodoros);
        }

        /// <summary>
        /// タスクの進捗率を取得する
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
        /// 残り予定ポモドーロ数を取得する
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
    /// タスクの優先度
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// 低優先度
        /// </summary>
        Low,
        
        /// <summary>
        /// 中優先度
        /// </summary>
        Medium,
        
        /// <summary>
        /// 高優先度
        /// </summary>
        High,
        
        /// <summary>
        /// 緊急
        /// </summary>
        Urgent
    }
}
using CommunityToolkit.Mvvm.ComponentModel;

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
        /// 詳細な説明（リッチテキスト対応）
        /// </summary>
        [ObservableProperty]
        private string detailedDescription = string.Empty;

        /// <summary>
        /// 添付ファイルのパスリスト
        /// </summary>
        [ObservableProperty]
        private List<string> attachments = new();

        /// <summary>
        /// チェックリスト項目
        /// </summary>
        [ObservableProperty]
        private List<ChecklistItem> checklist = new();

        /// <summary>
        /// Checklistプロパティが変更された時の処理
        /// </summary>
        partial void OnChecklistChanged(List<ChecklistItem> value)
        {
            // 既存のイベント購読を解除
            if (checklist != null)
            {
                foreach (var item in checklist)
                {
                    item.PropertyChanged -= ChecklistItem_PropertyChanged;
                }
            }

            // 新しいアイテムのイベントを購読
            if (value != null)
            {
                foreach (var item in value)
                {
                    item.PropertyChanged += ChecklistItem_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// チェックリストアイテムのプロパティが変更された時の処理
        /// </summary>
        private void ChecklistItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChecklistItem.IsChecked))
            {
                // チェックリスト関連のプロパティ変更通知を発行
                OnPropertyChanged(nameof(CheckedItemsCount));
                OnPropertyChanged(nameof(ChecklistProgress));
                OnPropertyChanged(nameof(ChecklistCompletionPercentage));
            }
        }


        /// <summary>
        /// チェックリストアイテムのイベント購読
        /// </summary>
        private void SubscribeToChecklistItems()
        {
            if (Checklist != null)
            {
                foreach (var item in Checklist)
                {
                    item.PropertyChanged += ChecklistItem_PropertyChanged;
                }
            }
        }

        /// <summary>
        /// 期限日時
        /// </summary>
        [ObservableProperty]
        private DateTime? dueDate;

        /// <summary>
        /// 見積もり時間（分）
        /// </summary>
        [ObservableProperty]
        private int estimatedMinutes = 25;

        /// <summary>
        /// 実際の作業時間（分）
        /// </summary>
        [ObservableProperty]
        private int actualMinutes = 0;

        /// <summary>
        /// 見積もりポモドーロ数
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EstimatedMinutes))]
        private int estimatedPomodoros = 1;

        /// <summary>
        /// 完了したポモドーロ数
        /// </summary>
        [ObservableProperty]
        private int completedPomodoros = 0;

        /// <summary>
        /// EstimatedPomodorosが変更された時の処理
        /// </summary>
        partial void OnEstimatedPomodorosChanged(int value)
        {
            // 見積もりポモドーロ数が変更されたら、見積もり分数も更新
            if (value > 0)
            {
                EstimatedMinutes = value * 25; // 1ポモドーロ = 25分
            }
        }

        /// <summary>
        /// EstimatedMinutesが変更された時の処理
        /// </summary>
        partial void OnEstimatedMinutesChanged(int value)
        {
            // 見積もり分数が変更されたら、ポモドーロ数も更新（ただし、ポモドーロ数から変更された場合は除く）
            if (value > 0)
            {
                var calculatedPomodoros = Math.Max(1, (int)Math.Ceiling(value / 25.0));
                if (calculatedPomodoros != EstimatedPomodoros)
                {
                    // 無限ループを避けるため、直接フィールドを更新
                    estimatedPomodoros = calculatedPomodoros;
                    OnPropertyChanged(nameof(EstimatedPomodoros));
                }
            }
        }

        /// <summary>
        /// タスクが完了しているかどうか
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Status))]
        private bool isCompleted = false;

        /// <summary>
        /// タスクの状態
        /// </summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        [NotifyPropertyChangedFor(nameof(StatusColor))]
        private TaskStatus status = TaskStatus.Todo;

        /// <summary>
        /// タスクの作成日時
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        /// <summary>
        /// タスクの開始日時
        /// </summary>
        [ObservableProperty]
        private DateTime? startedAt;

        /// <summary>
        /// タスクの完了日時
        /// </summary>
        [ObservableProperty]
        private DateTime? completedAt;

        /// <summary>
        /// 現在のセッションの開始日時（実行中タスクのみ）
        /// </summary>
        [ObservableProperty]
        private DateTime? currentSessionStartTime;

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
        /// 状態の表示テキスト
        /// </summary>
        public string StatusText => Status switch
        {
            TaskStatus.Todo => "未開始",
            TaskStatus.Waiting => "待機中",
            TaskStatus.Executing => "実行中",
            TaskStatus.Completed => "完了",
            _ => "未開始"
        };

        /// <summary>
        /// 状態の色
        /// </summary>
        public string StatusColor => Status switch
        {
            TaskStatus.Todo => "#94A3B8",      // グレー
            TaskStatus.Waiting => "#3B82F6", // ブルー
            TaskStatus.Executing => "#F59E0B",  // オレンジ（実行中）
            TaskStatus.Completed => "#10B981",  // グリーン
            _ => "#94A3B8"
        };

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PomodoroTask()
        {
            // 既存のチェックリストアイテムのイベントを購読
            SubscribeToChecklistItems();
        }

        /// <summary>
        /// タイトルと見積もり時間を指定するコンストラクタ
        /// </summary>
        /// <param name="title">タスクのタイトル</param>
        /// <param name="estimatedMinutes">見積もり時間（分）</param>
        public PomodoroTask(string title, int estimatedMinutes = 25)
        {
            Title = title ?? string.Empty;
            EstimatedMinutes = Math.Max(1, estimatedMinutes);
            // ポモドーロ数を分数から計算
            EstimatedPomodoros = Math.Max(1, (int)Math.Ceiling(estimatedMinutes / 25.0));
            // 既存のチェックリストアイテムのイベントを購読
            SubscribeToChecklistItems();
        }

        /// <summary>
        /// タスクの進捗率を取得する（時間ベース）
        /// </summary>
        public double ProgressPercentage
        {
            get
            {
                try
                {
                    if (EstimatedMinutes <= 0) return 0;
                    var progress = (double)ActualMinutes / EstimatedMinutes * 100;
                    return Math.Min(100, Math.Max(0, progress));
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 残り見積もり時間を取得する
        /// </summary>
        public int RemainingMinutes
        {
            get
            {
                try
                {
                    return Math.Max(0, EstimatedMinutes - ActualMinutes);
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// 残りポモドーロ数を取得する
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

        /// <summary>
        /// ポモドーロベースの進捗率を取得する
        /// </summary>
        public double PomodoroProgressPercentage
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
        /// チェックリストの完了率を取得する
        /// </summary>
        public double ChecklistCompletionPercentage
        {
            get
            {
                try
                {
                    if (Checklist.Count == 0) return 0;
                    var completedCount = Checklist.Count(item => item.IsChecked);
                    return (double)completedCount / Checklist.Count * 100;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// チェック済みアイテム数
        /// </summary>
        public int CheckedItemsCount
        {
            get
            {
                try
                {
                    return Checklist?.Count(item => item.IsChecked) ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// チェックリストの進捗表示テキスト
        /// </summary>
        public string ChecklistProgress
        {
            get
            {
                try
                {
                    if (Checklist == null || Checklist.Count == 0) return "";
                    var checkedCount = CheckedItemsCount;
                    return $"{checkedCount}/{Checklist.Count}";
                }
                catch
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// 添付ファイル数を取得する
        /// </summary>
        public int AttachmentCount => Attachments?.Count ?? 0;

        /// <summary>
        /// 期限までの残り日数を取得する
        /// </summary>
        public int? DaysUntilDue
        {
            get
            {
                if (DueDate == null) return null;
                var days = (DueDate.Value - DateTime.Now).Days;
                return days;
            }
        }

        /// <summary>
        /// 期限が過ぎているかどうか
        /// </summary>
        public bool IsOverdue
        {
            get
            {
                if (DueDate == null || IsCompleted) return false;
                return DateTime.Now > DueDate.Value;
            }
        }

        /// <summary>
        /// 期限の表示テキスト
        /// </summary>
        public string DueDateText
        {
            get
            {
                if (DueDate == null) return string.Empty;
                
                var today = DateTime.Today;
                var dueDate = DueDate.Value.Date;
                
                if (dueDate == today) return "今日";
                if (dueDate == today.AddDays(1)) return "明日";
                if (dueDate == today.AddDays(-1)) return "昨日";
                
                var daysDiff = (dueDate - today).Days;
                if (daysDiff > 0 && daysDiff <= 7) return $"{daysDiff}日後";
                if (daysDiff < 0 && daysDiff >= -7) return $"{Math.Abs(daysDiff)}日前";
                
                return dueDate.ToString("M/d");
            }
        }

        /// <summary>
        /// 期限の色
        /// </summary>
        public string DueDateColor
        {
            get
            {
                if (DueDate == null) return "Gray";
                
                var today = DateTime.Today;
                var dueDate = DueDate.Value.Date;
                
                if (dueDate < today) return "#EF4444"; // 赤色（過ぎた）
                if (dueDate == today) return "#F59E0B"; // オレンジ色（今日）
                if (dueDate == today.AddDays(1)) return "#10B981"; // 緑色（明日）
                
                return "#6B7280"; // グレー（それ以外）
            }
        }

        /// <summary>
        /// タスクを開始状態にする
        /// </summary>
        public void StartTask()
        {
            if (Status == TaskStatus.Todo)
            {
                Status = TaskStatus.Waiting;
                StartedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// タスクを実行中状態にする（25分セッション開始）
        /// </summary>
        public void StartExecution()
        {
            if (Status == TaskStatus.Waiting)
            {
                Status = TaskStatus.Executing;
            }
        }

        /// <summary>
        /// タスクを進行中状態に戻す（25分セッション終了）
        /// </summary>
        public void StopExecution()
        {
            if (Status == TaskStatus.Executing)
            {
                Status = TaskStatus.Waiting;
            }
        }

        /// <summary>
        /// タスクを完了状態にする
        /// </summary>
        public void CompleteTask()
        {
            IsCompleted = true;
            Status = TaskStatus.Completed;
            CompletedAt = DateTime.Now;
        }

        /// <summary>
        /// タスクを未開始状態に戻す
        /// </summary>
        public void ResetTask()
        {
            IsCompleted = false;
            Status = TaskStatus.Todo;
            StartedAt = null;
            CompletedAt = null;
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

    /// <summary>
    /// タスクの状態
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// 未開始
        /// </summary>
        Todo,
        
        /// <summary>
        /// 待機中
        /// </summary>
        Waiting,
        
        /// <summary>
        /// 実行中（25分セッション中）
        /// </summary>
        Executing,
        
        /// <summary>
        /// 完了
        /// </summary>
        Completed
    }
}
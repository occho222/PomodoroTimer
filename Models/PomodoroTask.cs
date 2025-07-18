﻿using CommunityToolkit.Mvvm.ComponentModel;
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
            TaskStatus.InProgress => "進行中",
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
            TaskStatus.InProgress => "#3B82F6", // ブルー
            TaskStatus.Executing => "#F59E0B",  // オレンジ（実行中）
            TaskStatus.Completed => "#10B981",  // グリーン
            _ => "#94A3B8"
        };

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public PomodoroTask()
        {
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
        /// タスクを開始状態にする
        /// </summary>
        public void StartTask()
        {
            if (Status == TaskStatus.Todo)
            {
                Status = TaskStatus.InProgress;
                StartedAt = DateTime.Now;
            }
        }

        /// <summary>
        /// タスクを実行中状態にする（25分セッション開始）
        /// </summary>
        public void StartExecution()
        {
            if (Status == TaskStatus.InProgress)
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
                Status = TaskStatus.InProgress;
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
        /// 進行中
        /// </summary>
        InProgress,
        
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
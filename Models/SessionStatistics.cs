namespace PomodoroTimer.Models
{
    /// <summary>
    /// セッション統計情報
    /// </summary>
    public class SessionStatistics
    {
        /// <summary>
        /// 日付
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Today;

        /// <summary>
        /// 完了ポモドーロ数
        /// </summary>
        public int CompletedPomodoros { get; set; }

        /// <summary>
        /// 完了タスク数
        /// </summary>
        public int CompletedTasks { get; set; }

        /// <summary>
        /// 総集中時間（分）
        /// </summary>
        public int TotalFocusTimeMinutes { get; set; }

        /// <summary>
        /// 開始時刻
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 終了時刻
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 中断回数
        /// </summary>
        public int InterruptionCount { get; set; }

        /// <summary>
        /// 総集中時間を時間と分の文字列で取得する
        /// </summary>
        public string TotalFocusTimeFormatted
        {
            get
            {
                var hours = TotalFocusTimeMinutes / 60;
                var minutes = TotalFocusTimeMinutes % 60;
                return $"{hours}時間{minutes}分";
            }
        }

        /// <summary>
        /// 平均ポモドーロ時間を取得する
        /// </summary>
        public double AveragePomodoroTimeMinutes => CompletedPomodoros > 0 ? 
            (double)TotalFocusTimeMinutes / CompletedPomodoros : 0;
    }
}
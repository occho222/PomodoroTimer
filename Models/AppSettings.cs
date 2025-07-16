namespace PomodoroTimer.Models
{
    /// <summary>
    /// アプリケーション設定モデル
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 作業セッション時間（分）
        /// </summary>
        public int WorkSessionMinutes { get; set; } = 25;

        /// <summary>
        /// 短い休憩時間（分）
        /// </summary>
        public int ShortBreakMinutes { get; set; } = 5;

        /// <summary>
        /// 長い休憩時間（分）
        /// </summary>
        public int LongBreakMinutes { get; set; } = 15;

        /// <summary>
        /// 長い休憩前のポモドーロ数
        /// </summary>
        public int PomodorosBeforeLongBreak { get; set; } = 4;

        /// <summary>
        /// 音声通知を有効にするかどうか
        /// </summary>
        public bool EnableSoundNotification { get; set; } = true;

        /// <summary>
        /// デスクトップ通知を有効にするかどうか
        /// </summary>
        public bool EnableDesktopNotification { get; set; } = true;

        /// <summary>
        /// 自動で次のセッションを開始するかどうか
        /// </summary>
        public bool AutoStartNextSession { get; set; } = false;

        /// <summary>
        /// ダークテーマを使用するかどうか
        /// </summary>
        public bool UseDarkTheme { get; set; } = false;

        /// <summary>
        /// 統計データを保存するかどうか
        /// </summary>
        public bool SaveStatistics { get; set; } = true;

        /// <summary>
        /// 最小化時にシステムトレイに格納するかどうか
        /// </summary>
        public bool MinimizeToTray { get; set; } = false;
    }
}
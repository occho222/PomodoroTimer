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
        /// 長い休憩の間隔（ポモドーロ数）
        /// </summary>
        public int LongBreakInterval { get; set; } = 4;

        /// <summary>
        /// 通知を表示するかどうか
        /// </summary>
        public bool ShowNotifications { get; set; } = true;

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
        /// システムテーマに従うかどうか
        /// </summary>
        public bool FollowSystemTheme { get; set; } = true;

        /// <summary>
        /// 統計データを保存するかどうか
        /// </summary>
        public bool SaveStatistics { get; set; } = true;

        /// <summary>
        /// 最小化時にシステムトレイに格納するかどうか
        /// </summary>
        public bool MinimizeToTray { get; set; } = false;

        /// <summary>
        /// Windows起動時に自動起動するかどうか
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// 起動時に最小化するかどうか
        /// </summary>
        public bool StartMinimized { get; set; } = false;

        /// <summary>
        /// グローバルホットキーを有効にするかどうか
        /// </summary>
        public bool EnableGlobalHotkeys { get; set; } = true;

        /// <summary>
        /// 開始/一時停止のホットキー
        /// </summary>
        public string StartPauseHotkey { get; set; } = "Ctrl+Space";

        /// <summary>
        /// 停止のホットキー
        /// </summary>
        public string StopHotkey { get; set; } = "Ctrl+S";

        /// <summary>
        /// スキップのホットキー
        /// </summary>
        public string SkipHotkey { get; set; } = "Ctrl+N";

        /// <summary>
        /// タスク追加のホットキー
        /// </summary>
        public string AddTaskHotkey { get; set; } = "Ctrl+T";

        /// <summary>
        /// 設定画面のホットキー
        /// </summary>
        public string SettingsHotkey { get; set; } = "F1";

        /// <summary>
        /// 自動保存間隔（分）
        /// </summary>
        public int AutoSaveIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// データバックアップの保持日数
        /// </summary>
        public int BackupRetentionDays { get; set; } = 30;

        /// <summary>
        /// アプリケーションの言語設定
        /// </summary>
        public string Language { get; set; } = "ja-JP";

        /// <summary>
        /// カスタムテーマの色設定
        /// </summary>
        public ThemeSettings ThemeSettings { get; set; } = new ThemeSettings();
    }

    /// <summary>
    /// テーマ設定
    /// </summary>
    public class ThemeSettings
    {
        /// <summary>
        /// プライマリカラー
        /// </summary>
        public string PrimaryColor { get; set; } = "#6366F1";

        /// <summary>
        /// セカンダリカラー
        /// </summary>
        public string SecondaryColor { get; set; } = "#EC4899";

        /// <summary>
        /// 背景色
        /// </summary>
        public string BackgroundColor { get; set; } = "#F8FAFC";

        /// <summary>
        /// テキスト色
        /// </summary>
        public string TextColor { get; set; } = "#1E293B";

        /// <summary>
        /// アクセント色
        /// </summary>
        public string AccentColor { get; set; } = "#10B981";

        /// <summary>
        /// 警告色
        /// </summary>
        public string WarningColor { get; set; } = "#F59E0B";

        /// <summary>
        /// エラー色
        /// </summary>
        public string ErrorColor { get; set; } = "#EF4444";
    }
}
namespace PomodoroTimer.Models
{
    /// <summary>
    /// アプリケーション設定モデル
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// データフォーマットのバージョン
        /// </summary>
        public string DataVersion { get; set; } = "1.5.31";

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
        /// タスク完了時にセッションを継続するかどうか
        /// </summary>
        public bool ContinueSessionOnTaskComplete { get; set; } = true;

        /// <summary>
        /// セッション継続時に自動でタスク選択ダイアログを表示するかどうか
        /// </summary>
        public bool ShowTaskSelectionDialog { get; set; } = true;

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
        /// 集中モードを有効にするかどうか
        /// </summary>
        public bool EnableFocusMode { get; set; } = false;

        /// <summary>
        /// 集中モード時に前面固定するかどうか
        /// </summary>
        public bool FocusModeAlwaysOnTop { get; set; } = false;

        /// <summary>
        /// クイックタスクのデフォルト時間（分）
        /// </summary>
        public int QuickTaskDefaultMinutes { get; set; } = 5;

        /// <summary>
        /// カスタムテーマの色設定
        /// </summary>
        public ThemeSettings ThemeSettings { get; set; } = new ThemeSettings();

        /// <summary>
        /// タスク詳細ダイアログの横幅
        /// </summary>
        public double TaskDetailDialogWidth { get; set; } = 900;

        /// <summary>
        /// タスク詳細ダイアログの縦幅
        /// </summary>
        public double TaskDetailDialogHeight { get; set; } = 700;

        /// <summary>
        /// 通知ダイアログを最前面に表示するかどうか
        /// </summary>
        public bool TopmostNotification { get; set; } = true;

        /// <summary>
        /// Microsoft Graph設定
        /// </summary>
        public GraphSettings GraphSettings { get; set; } = new GraphSettings();

        /// <summary>
        /// コンストラクタでGraphSettingsの初期化を確実にする
        /// </summary>
        public AppSettings()
        {
            // GraphSettingsが確実に初期化されるようにする
            GraphSettings ??= new GraphSettings();
            ThemeSettings ??= new ThemeSettings();
        }
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

    /// <summary>
    /// Microsoft Graph設定
    /// </summary>
    public class GraphSettings
    {
        /// <summary>
        /// Azure ADアプリケーションのクライアントID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// テナントID（マルチテナントの場合は"common"）
        /// </summary>
        public string TenantId { get; set; } = "common";

        /// <summary>
        /// 自動ログインを有効にするかどうか
        /// </summary>
        public bool EnableAutoLogin { get; set; } = false;

        /// <summary>
        /// Microsoft To-Doからのインポートを有効にするかどうか
        /// </summary>
        public bool EnableMicrosoftToDoImport { get; set; } = true;

        /// <summary>
        /// Microsoft Plannerからのインポートを有効にするかどうか
        /// </summary>
        public bool EnablePlannerImport { get; set; } = true;

        /// <summary>
        /// Outlookタスクからのインポートを有効にするかどうか
        /// </summary>
        public bool EnableOutlookImport { get; set; } = true;

        /// <summary>
        /// 最後の認証日時
        /// </summary>
        public DateTime? LastAuthenticationTime { get; set; }
    }
}
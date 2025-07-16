using System.Windows;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// システムトレイサービスのインターフェース
    /// </summary>
    public interface ISystemTrayService
    {
        /// <summary>
        /// システムトレイを初期化する
        /// </summary>
        void Initialize();

        /// <summary>
        /// ウィンドウをシステムトレイに最小化する
        /// </summary>
        void MinimizeToTray();

        /// <summary>
        /// システムトレイからウィンドウを復元する
        /// </summary>
        void RestoreFromTray();

        /// <summary>
        /// システムトレイアイコンを表示する
        /// </summary>
        void ShowTrayIcon();

        /// <summary>
        /// システムトレイアイコンを非表示にする
        /// </summary>
        void HideTrayIcon();

        /// <summary>
        /// バルーン通知を表示する
        /// </summary>
        /// <param name="title">通知タイトル</param>
        /// <param name="message">通知メッセージ</param>
        /// <param name="icon">通知アイコン</param>
        void ShowBalloonTip(string title, string message, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Info);

        /// <summary>
        /// システムトレイのリソースを解放する
        /// </summary>
        void Dispose();

        /// <summary>
        /// タイマー状態を更新する（アイコン変更用）
        /// </summary>
        /// <param name="isRunning">タイマーが動作中かどうか</param>
        /// <param name="remainingTime">残り時間</param>
        void UpdateTimerStatus(bool isRunning, TimeSpan remainingTime);
    }
}
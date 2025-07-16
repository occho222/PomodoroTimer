namespace PomodoroTimer.Services
{
    /// <summary>
    /// セッションタイプの列挙型
    /// </summary>
    public enum SessionType
    {
        Work,
        ShortBreak,
        LongBreak
    }

    /// <summary>
    /// タイマーサービスのインターフェース
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// タイマーが開始された時に発生するイベント
        /// </summary>
        event Action? TimerStarted;

        /// <summary>
        /// タイマーが停止された時に発生するイベント
        /// </summary>
        event Action? TimerStopped;

        /// <summary>
        /// タイマーが一時停止された時に発生するイベント
        /// </summary>
        event Action? TimerPaused;

        /// <summary>
        /// タイマーが再開された時に発生するイベント
        /// </summary>
        event Action? TimerResumed;

        /// <summary>
        /// タイマーの時間が更新された時に発生するイベント
        /// </summary>
        event Action<TimeSpan>? TimeUpdated;

        /// <summary>
        /// セッションが完了した時に発生するイベント
        /// </summary>
        event Action<SessionType>? SessionCompleted;

        /// <summary>
        /// セッションタイプが変更された時に発生するイベント
        /// </summary>
        event Action<SessionType>? SessionTypeChanged;

        /// <summary>
        /// タイマーが実行中かどうか
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 現在の残り時間
        /// </summary>
        TimeSpan RemainingTime { get; }

        /// <summary>
        /// セッションの合計時間
        /// </summary>
        TimeSpan SessionDuration { get; }

        /// <summary>
        /// 現在のセッションタイプ
        /// </summary>
        SessionType CurrentSessionType { get; }

        /// <summary>
        /// 完了したポモドーロ数
        /// </summary>
        int CompletedPomodoros { get; }

        /// <summary>
        /// タイマーを開始する
        /// </summary>
        /// <param name="duration">セッション時間</param>
        void Start(TimeSpan duration);

        /// <summary>
        /// 新しいポモドーロサイクルを開始する
        /// </summary>
        void StartNewPomodoroCycle();

        /// <summary>
        /// タイマーを停止する
        /// </summary>
        void Stop();

        /// <summary>
        /// タイマーを一時停止する
        /// </summary>
        void Pause();

        /// <summary>
        /// タイマーを再開する
        /// </summary>
        void Resume();

        /// <summary>
        /// 現在のセッションをスキップする
        /// </summary>
        void Skip();

        /// <summary>
        /// 設定を更新する
        /// </summary>
        /// <param name="settings">アプリケーション設定</param>
        void UpdateSettings(Models.AppSettings settings);
    }
}
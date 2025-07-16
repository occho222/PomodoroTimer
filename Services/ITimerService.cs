namespace PomodoroTimer.Services
{
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
        event Action? SessionCompleted;

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
        /// タイマーを開始する
        /// </summary>
        /// <param name="duration">セッション時間</param>
        void Start(TimeSpan duration);

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
    }
}
using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// 統計情報サービスのインターフェース
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// 日次統計を取得する
        /// </summary>
        /// <param name="date">対象日</param>
        /// <returns>日次統計情報</returns>
        DailyStatistics GetDailyStatistics(DateTime date);

        /// <summary>
        /// 週次統計を取得する
        /// </summary>
        /// <param name="weekStart">週の開始日</param>
        /// <returns>週次統計情報</returns>
        List<DailyStatistics> GetWeeklyStatistics(DateTime weekStart);

        /// <summary>
        /// 全期間の統計を取得する
        /// </summary>
        /// <returns>全期間統計情報</returns>
        AllTimeStatistics GetAllTimeStatistics();

        /// <summary>
        /// ポモドーロ完了を記録する
        /// </summary>
        /// <param name="task">完了したタスク</param>
        /// <param name="sessionDurationMinutes">セッション時間（分）</param>
        void RecordPomodoroComplete(PomodoroTask task, int sessionDurationMinutes);

        /// <summary>
        /// タスク完了を記録する
        /// </summary>
        /// <param name="task">完了したタスク</param>
        void RecordTaskComplete(PomodoroTask task);

        /// <summary>
        /// プロジェクト別統計を取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>プロジェクト別統計のディクショナリ</returns>
        Dictionary<string, ProjectStatistics> GetProjectStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// タグ別統計を取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>タグ別統計のディクショナリ</returns>
        Dictionary<string, TagStatistics> GetTagStatistics(DateTime startDate, DateTime endDate);

        /// <summary>
        /// カテゴリ別の作業時間ランキングを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="topCount">上位何位まで取得するか</param>
        /// <returns>作業時間順のカテゴリランキング</returns>
        List<(string Category, int FocusMinutes, int CompletedPomodoros)> GetCategoryRanking(
            DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// タグ別の作業時間ランキングを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="topCount">上位何位まで取得するか</param>
        /// <returns>作業時間順のタグランキング</returns>
        List<(string Tag, int FocusMinutes, int CompletedPomodoros)> GetTagRanking(
            DateTime startDate, DateTime endDate, int topCount = 10);

        /// <summary>
        /// 統計データをクリアする
        /// </summary>
        void ClearStatistics();

        /// <summary>
        /// 統計データを保存する
        /// </summary>
        Task SaveStatisticsAsync();

        /// <summary>
        /// 統計データを読み込む
        /// </summary>
        Task LoadStatisticsAsync();
    }
}
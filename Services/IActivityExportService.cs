using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// アクティビティデータエクスポートサービスのインターフェース
    /// </summary>
    public interface IActivityExportService
    {
        /// <summary>
        /// 指定した日のアクティビティデータを取得する
        /// </summary>
        /// <param name="date">対象日</param>
        /// <returns>アクティビティデータ</returns>
        DailyActivityData GetDailyActivityData(DateTime date);

        /// <summary>
        /// アクティビティデータをJSONファイルとしてエクスポートする
        /// </summary>
        /// <param name="activityData">アクティビティデータ</param>
        /// <param name="filePath">出力ファイルパス</param>
        /// <returns>エクスポートタスク</returns>
        Task ExportActivityDataToJsonAsync(DailyActivityData activityData, string filePath);

        /// <summary>
        /// AI振り返り用プロンプトをテキストファイルとしてエクスポートする
        /// </summary>
        /// <param name="activityData">アクティビティデータ</param>
        /// <param name="jsonFilePath">JSONファイルパス</param>
        /// <param name="promptFilePath">プロンプトファイルパス</param>
        /// <returns>エクスポートタスク</returns>
        Task ExportReflectionPromptAsync(DailyActivityData activityData, string jsonFilePath, string promptFilePath);

        /// <summary>
        /// アクティビティデータとプロンプトを一括エクスポートする（単一日）
        /// </summary>
        /// <param name="date">対象日</param>
        /// <param name="outputDirectory">出力ディレクトリ</param>
        /// <returns>エクスポートファイルパスのタプル(JSONファイル, プロンプトファイル)</returns>
        Task<(string jsonFilePath, string promptFilePath)> ExportDailyActivityAsync(DateTime date, string outputDirectory);

        /// <summary>
        /// 指定期間のアクティビティデータとプロンプトを一括エクスポートする
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <param name="outputDirectory">出力ディレクトリ</param>
        /// <returns>エクスポートファイルパスのタプル(JSONファイル, プロンプトファイル)</returns>
        Task<(string jsonFilePath, string promptFilePath)> ExportPeriodActivityAsync(DateTime startDate, DateTime endDate, string outputDirectory);

        /// <summary>
        /// 指定期間のアクティビティデータを取得する
        /// </summary>
        /// <param name="startDate">開始日</param>
        /// <param name="endDate">終了日</param>
        /// <returns>期間アクティビティデータ</returns>
        PeriodActivityData GetPeriodActivityData(DateTime startDate, DateTime endDate);
    }
}
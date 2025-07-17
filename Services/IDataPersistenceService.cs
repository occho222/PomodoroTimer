using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// データ永続化サービスのインターフェース
    /// </summary>
    public interface IDataPersistenceService
    {
        /// <summary>
        /// データを保存する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="data">保存するデータ</param>
        /// <returns>保存完了タスク</returns>
        Task SaveDataAsync<T>(string fileName, T data);

        /// <summary>
        /// データを読み込む
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>読み込まれたデータ</returns>
        Task<T?> LoadDataAsync<T>(string fileName);

        /// <summary>
        /// ファイルが存在するかチェックする
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>存在する場合true</returns>
        bool FileExists(string fileName);

        /// <summary>
        /// ファイルを削除する
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>削除完了タスク</returns>
        Task DeleteFileAsync(string fileName);

        /// <summary>
        /// すべてのデータを初期化する（削除する）
        /// </summary>
        /// <returns>初期化完了タスク</returns>
        Task ResetDataAsync();
    }
}
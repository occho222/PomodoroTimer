using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ポモドーロサービスのインターフェース
    /// </summary>
    public interface IPomodoroService
    {
        /// <summary>
        /// すべてのタスクを取得する
        /// </summary>
        /// <returns>タスクのコレクション</returns>
        ObservableCollection<PomodoroTask> GetTasks();

        /// <summary>
        /// タスクを追加する
        /// </summary>
        /// <param name="task">追加するタスク</param>
        void AddTask(PomodoroTask task);

        /// <summary>
        /// タスクを削除する
        /// </summary>
        /// <param name="task">削除するタスク</param>
        void RemoveTask(PomodoroTask task);

        /// <summary>
        /// タスクを更新する
        /// </summary>
        /// <param name="task">更新するタスク</param>
        void UpdateTask(PomodoroTask task);

        /// <summary>
        /// IDでタスクを取得する
        /// </summary>
        /// <param name="taskId">タスクID</param>
        /// <returns>タスク（見つからない場合はnull）</returns>
        PomodoroTask? GetTaskById(Guid taskId);

        /// <summary>
        /// タスクの順序を変更する
        /// </summary>
        /// <param name="sourceIndex">移動元のインデックス</param>
        /// <param name="targetIndex">移動先のインデックス</param>
        void ReorderTasks(int sourceIndex, int targetIndex);

        /// <summary>
        /// タスクを完了状態にする
        /// </summary>
        /// <param name="task">完了するタスク</param>
        void CompleteTask(PomodoroTask task);

        /// <summary>
        /// タスクのポモドーロ数を増加させる
        /// </summary>
        /// <param name="task">対象タスク</param>
        void IncrementTaskPomodoro(PomodoroTask task);

        /// <summary>
        /// カテゴリでタスクをフィルタリングする
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <returns>フィルタリングされたタスクのリスト</returns>
        List<PomodoroTask> GetTasksByCategory(string category);

        /// <summary>
        /// タグでタスクを検索する
        /// </summary>
        /// <param name="tag">タグ名</param>
        /// <returns>該当タグを持つタスクのリスト</returns>
        List<PomodoroTask> GetTasksByTag(string tag);

        /// <summary>
        /// 優先度でタスクをフィルタリングする
        /// </summary>
        /// <param name="priority">優先度</param>
        /// <returns>指定優先度のタスクのリスト</returns>
        List<PomodoroTask> GetTasksByPriority(TaskPriority priority);

        /// <summary>
        /// タスクをCSV形式でエクスポートする
        /// </summary>
        /// <param name="filePath">保存先ファイルパス</param>
        /// <returns>エクスポート完了タスク</returns>
        Task ExportTasksToCsvAsync(string filePath);

        /// <summary>
        /// CSV形式のタスクをインポートする
        /// </summary>
        /// <param name="filePath">インポート元ファイルパス</param>
        /// <returns>インポート完了タスク</returns>
        Task ImportTasksFromCsvAsync(string filePath);

        /// <summary>
        /// 全てのカテゴリを取得する
        /// </summary>
        /// <returns>カテゴリのリスト</returns>
        List<string> GetAllCategories();

        /// <summary>
        /// 全てのタグを取得する
        /// </summary>
        /// <returns>タグのリスト</returns>
        List<string> GetAllTags();

        /// <summary>
        /// タスクデータを保存する
        /// </summary>
        /// <returns>保存完了タスク</returns>
        Task SaveTasksAsync();

        /// <summary>
        /// タスクデータを読み込む
        /// </summary>
        /// <returns>読み込み完了タスク</returns>
        Task LoadTasksAsync();
    }
}
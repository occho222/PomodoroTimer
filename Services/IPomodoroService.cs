using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ポモドーロタイマーのビジネスロジックを定義するインターフェース
    /// </summary>
    public interface IPomodoroService
    {
        /// <summary>
        /// タスクリストを取得する
        /// </summary>
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
        /// タスクの順序を変更する
        /// </summary>
        /// <param name="sourceIndex">移動元のインデックス</param>
        /// <param name="targetIndex">移動先のインデックス</param>
        void ReorderTasks(int sourceIndex, int targetIndex);

        /// <summary>
        /// タスクを完了にする
        /// </summary>
        /// <param name="task">完了するタスク</param>
        void CompleteTask(PomodoroTask task);

        /// <summary>
        /// タスクのポモドーロ数を増加する
        /// </summary>
        /// <param name="task">対象のタスク</param>
        void IncrementTaskPomodoro(PomodoroTask task);
    }
}
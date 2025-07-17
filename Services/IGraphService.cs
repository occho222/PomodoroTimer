using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// Microsoft Graph APIサービスのインターフェース
    /// </summary>
    public interface IGraphService
    {
        /// <summary>
        /// Microsoft Graphから認証を行う
        /// </summary>
        /// <returns>認証成功の可否</returns>
        Task<bool> AuthenticateAsync();

        /// <summary>
        /// Microsoft To Do からタスクをインポートする
        /// </summary>
        /// <returns>インポートされたタスクのリスト</returns>
        Task<List<PomodoroTask>> ImportTasksFromMicrosoftToDoAsync();

        /// <summary>
        /// Microsoft Plannerからタスクをインポートする
        /// </summary>
        /// <returns>インポートされたタスクのリスト</returns>
        Task<List<PomodoroTask>> ImportTasksFromPlannerAsync();

        /// <summary>
        /// Outlookタスクからタスクをインポートする
        /// </summary>
        /// <returns>インポートされたタスクのリスト</returns>
        Task<List<PomodoroTask>> ImportTasksFromOutlookAsync();

        /// <summary>
        /// 現在の認証状態を確認する
        /// </summary>
        /// <returns>認証されているかどうか</returns>
        bool IsAuthenticated { get; }

        /// <summary>
        /// ログアウトする
        /// </summary>
        /// <returns>ログアウト完了タスク</returns>
        Task LogoutAsync();
    }
}
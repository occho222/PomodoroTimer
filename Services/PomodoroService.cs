using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ポモドーロタイマーのビジネスロジック実装
    /// </summary>
    public class PomodoroService : IPomodoroService
    {
        private readonly ObservableCollection<PomodoroTask> _tasks;

        public PomodoroService()
        {
            _tasks = new ObservableCollection<PomodoroTask>();
            
            // 初期サンプルタスクを追加
            InitializeSampleTasks();
        }

        /// <summary>
        /// 初期サンプルタスクを設定する
        /// </summary>
        private void InitializeSampleTasks()
        {
            _tasks.Add(new PomodoroTask("Check and reply to emails", 1));
            _tasks.Add(new PomodoroTask("Create project documentation", 3));
            _tasks.Add(new PomodoroTask("Prepare for meeting", 2));
        }

        public ObservableCollection<PomodoroTask> GetTasks()
        {
            return _tasks;
        }

        public void AddTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _tasks.Add(task);
        }

        public void RemoveTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _tasks.Remove(task);
        }

        public void ReorderTasks(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));

            if (targetIndex < 0 || targetIndex >= _tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            _tasks.Move(sourceIndex, targetIndex);
        }

        public void CompleteTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;
        }

        public void IncrementTaskPomodoro(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.CompletedPomodoros++;
            
            // 予定ポモドーロ数に達した場合は自動完了
            if (task.CompletedPomodoros >= task.EstimatedPomodoros)
            {
                CompleteTask(task);
            }
        }
    }
}
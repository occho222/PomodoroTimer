using PomodoroTimer.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ポモドーロタイマーのビジネスロジック実装
    /// </summary>
    public class PomodoroService : IPomodoroService
    {
        private readonly ObservableCollection<PomodoroTask> _tasks;
        private readonly IDataPersistenceService _dataPersistenceService;
        private const string TasksFileName = "tasks.json";

        public PomodoroService(IDataPersistenceService dataPersistenceService)
        {
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _tasks = new ObservableCollection<PomodoroTask>();
            
            // 起動時にタスクデータを読み込み
            _ = Task.Run(async () =>
            {
                await LoadTasksAsync();
                
                // データが存在しない場合のみサンプルタスクを追加
                if (_tasks.Count == 0)
                {
                    InitializeSampleTasks();
                }
            });
        }

        /// <summary>
        /// 初期サンプルタスクを設定する
        /// </summary>
        private void InitializeSampleTasks()
        {
            var sampleTasks = new[]
            {
                new PomodoroTask("メールの確認と返信", 1) 
                { 
                    Priority = TaskPriority.Medium, 
                    Category = "仕事",
                    TagsText = "コミュニケーション, 日課"
                },
                new PomodoroTask("プロジェクト資料の作成", 3) 
                { 
                    Priority = TaskPriority.High, 
                    Category = "仕事",
                    TagsText = "ドキュメント, 重要"
                },
                new PomodoroTask("会議の準備", 2) 
                { 
                    Priority = TaskPriority.High, 
                    Category = "仕事",
                    TagsText = "会議, 準備"
                }
            };

            foreach (var task in sampleTasks)
            {
                _tasks.Add(task);
            }
        }

        public ObservableCollection<PomodoroTask> GetTasks()
        {
            return _tasks;
        }

        public void AddTask(PomodoroTask task)
        {
            if (task == null)
            {
                Console.WriteLine("警告: nullのタスクを追加しようとしました");
                throw new ArgumentNullException(nameof(task));
            }

            try
            {
                task.DisplayOrder = _tasks.Count;
                _tasks.Add(task);
                
                Console.WriteLine($"タスクが追加されました: {task.Title}");
                
                // タスク追加時に自動保存
                _ = Task.Run(SaveTasksAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクの追加に失敗しました: {ex.Message}");
                throw;
            }
        }

        public void RemoveTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            _tasks.Remove(task);
            UpdateDisplayOrders();
            
            // タスク削除時に自動保存
            _ = Task.Run(SaveTasksAsync);
        }

        public void UpdateTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var existingTask = GetTaskById(task.Id);
            if (existingTask != null)
            {
                var index = _tasks.IndexOf(existingTask);
                if (index >= 0)
                {
                    _tasks[index] = task;
                    
                    // タスク更新時に自動保存
                    _ = Task.Run(SaveTasksAsync);
                }
            }
        }

        public PomodoroTask? GetTaskById(Guid taskId)
        {
            return _tasks.FirstOrDefault(t => t.Id == taskId);
        }

        public void ReorderTasks(int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= _tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));

            if (targetIndex < 0 || targetIndex >= _tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(targetIndex));

            _tasks.Move(sourceIndex, targetIndex);
            UpdateDisplayOrders();
            
            // 順序変更時に自動保存
            _ = Task.Run(SaveTasksAsync);
        }

        public void CompleteTask(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.IsCompleted = true;
            task.CompletedAt = DateTime.Now;
            
            // タスク完了時に自動保存
            _ = Task.Run(SaveTasksAsync);
        }

        public void IncrementTaskPomodoro(PomodoroTask task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            task.ActualMinutes += 25; // 25分セッション完了時に実際の作業時間を追加
            
            // 見積もり時間に達した場合は自動完了
            if (task.ActualMinutes >= task.EstimatedMinutes)
            {
                CompleteTask(task);
            }
            else
            {
                // 完了しない場合も保存
                _ = Task.Run(SaveTasksAsync);
            }
        }

        public List<PomodoroTask> GetTasksByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return new List<PomodoroTask>();

            return _tasks.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public List<PomodoroTask> GetTasksByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return new List<PomodoroTask>();

            return _tasks.Where(t => t.Tags.Any(tagItem => 
                tagItem.Equals(tag, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        public List<PomodoroTask> GetTasksByPriority(TaskPriority priority)
        {
            return _tasks.Where(t => t.Priority == priority).ToList();
        }

        public async Task ExportTasksToCsvAsync(string filePath)
        {
            try
            {
                var csv = new StringBuilder();
                
                // ヘッダー行
                csv.AppendLine("ID,タイトル,説明,カテゴリ,タグ,優先度,予定ポモドーロ数,完了ポモドーロ数,完了状態,作成日,完了日");

                // データ行
                foreach (var task in _tasks)
                {
                    csv.AppendLine($"{task.Id}," +
                                 $"\"{EscapeCsvField(task.Title)}\"," +
                                 $"\"{EscapeCsvField(task.Description)}\"," +
                                 $"\"{EscapeCsvField(task.Category)}\"," +
                                 $"\"{EscapeCsvField(task.TagsText)}\"," +
                                 $"{task.Priority}," +
                                 $"{task.EstimatedMinutes}," +
                                 $"{task.ActualMinutes}," +
                                 $"{(task.IsCompleted ? "完了" : "未完了")}," +
                                 $"{task.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                                 $"{(task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "")}");
                }

                await System.IO.File.WriteAllTextAsync(filePath, csv.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"CSVエクスポートに失敗しました: {ex.Message}", ex);
            }
        }

        public async Task ImportTasksFromCsvAsync(string filePath)
        {
            try
            {
                var lines = await System.IO.File.ReadAllLinesAsync(filePath, Encoding.UTF8);
                
                if (lines.Length < 2) // ヘッダー + 最低1行のデータ
                    return;

                // ヘッダー行をスキップして処理
                for (int i = 1; i < lines.Length; i++)
                {
                    var fields = ParseCsvLine(lines[i]);
                    if (fields.Length < 11) continue;

                    try
                    {
                        var task = new PomodoroTask
                        {
                            Id = Guid.TryParse(fields[0], out var id) ? id : Guid.NewGuid(),
                            Title = fields[1],
                            Description = fields[2],
                            Category = fields[3],
                            TagsText = fields[4],
                            Priority = Enum.TryParse<TaskPriority>(fields[5], out var priority) ? priority : TaskPriority.Medium,
                            EstimatedMinutes = int.TryParse(fields[6], out var estimated) ? estimated : 25,
                            ActualMinutes = int.TryParse(fields[7], out var completed) ? completed : 0,
                            IsCompleted = fields[8] == "完了",
                            CreatedAt = DateTime.TryParse(fields[9], out var created) ? created : DateTime.Now,
                            CompletedAt = DateTime.TryParse(fields[10], out var completedAt) ? completedAt : null
                        };

                        AddTask(task);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"行 {i + 1} のインポートに失敗しました: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"CSVインポートに失敗しました: {ex.Message}", ex);
            }
        }

        public async Task ImportTasksFromGraphAsync(List<PomodoroTask> tasks)
        {
            try
            {
                if (tasks == null || tasks.Count == 0)
                {
                    Console.WriteLine("インポートするタスクがありません。");
                    return;
                }

                int importedCount = 0;
                int duplicateCount = 0;

                foreach (var task in tasks)
                {
                    // 既存のタスクと重複しないかチェック（タイトルとカテゴリで判断）
                    var existingTask = _tasks.FirstOrDefault(t => 
                        t.Title.Equals(task.Title, StringComparison.OrdinalIgnoreCase) && 
                        t.Category.Equals(task.Category, StringComparison.OrdinalIgnoreCase));

                    if (existingTask == null)
                    {
                        // 新しいIDを生成して追加
                        task.Id = Guid.NewGuid();
                        task.DisplayOrder = _tasks.Count;
                        _tasks.Add(task);
                        importedCount++;
                    }
                    else
                    {
                        duplicateCount++;
                        Console.WriteLine($"重複タスクをスキップしました: {task.Title}");
                    }
                }

                Console.WriteLine($"Microsoft Graphから {importedCount} 件のタスクをインポートしました。重複スキップ: {duplicateCount} 件");

                // インポート後に自動保存
                await SaveTasksAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Microsoft Graphからのタスクインポートに失敗しました: {ex.Message}", ex);
            }
        }

        public List<string> GetAllCategories()
        {
            try
            {
                var tasksCopy = _tasks.ToList();
                return tasksCopy
                    .Where(t => t != null && !string.IsNullOrEmpty(t.Category))
                    .Select(t => t.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllCategoriesでエラーが発生しました: {ex.Message}");
                return new List<string>();
            }
        }

        public List<string> GetAllTags()
        {
            try
            {
                var tasksCopy = _tasks.ToList();
                return tasksCopy
                    .Where(t => t != null && t.Tags != null)
                    .SelectMany(t => t.Tags)
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .Distinct()
                    .OrderBy(tag => tag)
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllTagsでエラーが発生しました: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task SaveTasksAsync()
        {
            try
            {
                await _dataPersistenceService.SaveDataAsync("tasks.json", _tasks.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクデータの保存に失敗しました: {ex.Message}");
            }
        }

        public async Task LoadTasksAsync()
        {
            try
            {
                var tasks = await _dataPersistenceService.LoadDataAsync<List<PomodoroTask>>("tasks.json");
                
                if (tasks != null && tasks.Count > 0)
                {
                    _tasks.Clear();
                    // null値をフィルタリングして有効なタスクのみを追加
                    foreach (var task in tasks.Where(t => t != null).OrderBy(t => t.DisplayOrder))
                    {
                        _tasks.Add(task);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"タスクデータの読み込みに失敗しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 表示順序を更新する
        /// </summary>
        private void UpdateDisplayOrders()
        {
            for (int i = 0; i < _tasks.Count; i++)
            {
                _tasks[i].DisplayOrder = i;
            }
        }

        /// <summary>
        /// CSVフィールドをエスケープする
        /// </summary>
        /// <param name="field">フィールド値</param>
        /// <returns>エスケープされた値</returns>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            return field.Replace("\"", "\"\"");
        }

        /// <summary>
        /// CSV行を解析する
        /// </summary>
        /// <param name="line">CSV行</param>
        /// <returns>フィールドの配列</returns>
        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            currentField.Append('"');
                            i++; // Skip next quote
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        fields.Add(currentField.ToString());
                        currentField.Clear();
                    }
                    else
                    {
                        currentField.Append(c);
                    }
                }
            }

            fields.Add(currentField.ToString());
            return fields.ToArray();
        }
    }
}
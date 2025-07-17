using Microsoft.Graph;
using Microsoft.Graph.Models;
using PomodoroTimer.Models;
using Azure.Identity;
using System.Diagnostics;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// Microsoft Graph API実装サービス
    /// </summary>
    public class GraphService : IGraphService
    {
        private GraphServiceClient? _graphServiceClient;
        private readonly string[] _scopes = { "https://graph.microsoft.com/Tasks.ReadWrite", 
                                             "https://graph.microsoft.com/Group.Read.All" };
        private readonly AppSettings _settings;

        public bool IsAuthenticated => _graphServiceClient != null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">アプリケーション設定</param>
        public GraphService(AppSettings? settings = null)
        {
            _settings = settings ?? new AppSettings();
            // GraphSettingsが確実に初期化されるように確認
            _settings.GraphSettings ??= new GraphSettings();
        }

        /// <summary>
        /// Microsoft Graphから認証を行う
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                // クライアントIDが設定されていない場合
                if (string.IsNullOrWhiteSpace(_settings.GraphSettings.ClientId))
                {
                    Console.WriteLine("Microsoft Graph API のクライアントIDが設定されていません。");
                    return false;
                }

                var options = new InteractiveBrowserCredentialOptions
                {
                    TenantId = _settings.GraphSettings.TenantId,
                    ClientId = _settings.GraphSettings.ClientId,
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                    RedirectUri = new Uri("http://localhost")
                };

                var interactiveCredential = new InteractiveBrowserCredential(options);
                
                _graphServiceClient = new GraphServiceClient(interactiveCredential, _scopes);

                // 認証をテストするため、ユーザー情報を取得
                var user = await _graphServiceClient.Me.GetAsync();
                
                if (user != null)
                {
                    Console.WriteLine($"Microsoft Graphに正常に認証されました。ユーザー: {user.DisplayName}");
                    _settings.GraphSettings.LastAuthenticationTime = DateTime.Now;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft Graph認証エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Microsoft To Do からタスクをインポートする
        /// </summary>
        public async Task<List<PomodoroTask>> ImportTasksFromMicrosoftToDoAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("認証されていません。最初にAuthenticateAsync()を呼び出してください。");
            }

            try
            {
                // Microsoft To-Doのタスクリストを取得
                var taskLists = await _graphServiceClient!.Me.Todo.Lists.GetAsync();

                if (taskLists?.Value != null)
                {
                    foreach (var taskList in taskLists.Value)
                    {
                        if (taskList?.Id != null)
                        {
                            // 各リストのタスクを取得
                            var tasks = await _graphServiceClient.Me.Todo.Lists[taskList.Id].Tasks.GetAsync();

                            if (tasks?.Value != null)
                            {
                                foreach (var task in tasks.Value)
                                {
                                    if (task != null)
                                    {
                                        var pomodoroTask = ConvertToDoTaskToPomodoroTask(task, taskList.DisplayName ?? "未分類");
                                        importedTasks.Add(pomodoroTask);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Microsoft To-Doから {importedTasks.Count} 件のタスクをインポートしました。");
                return importedTasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft To-Doタスクのインポートエラー: {ex.Message}");
                throw new InvalidOperationException($"Microsoft To-Doからのタスクインポートに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Microsoft Plannerからタスクをインポートする
        /// </summary>
        public async Task<List<PomodoroTask>> ImportTasksFromPlannerAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("認証されていません。最初にAuthenticateAsync()を呼び出してください。");
            }

            try
            {
                // ユーザーのプランを取得
                var plans = await _graphServiceClient!.Me.Planner.Plans.GetAsync();

                if (plans?.Value != null)
                {
                    foreach (var plan in plans.Value)
                    {
                        if (plan?.Id != null)
                        {
                            // 各プランのタスクを取得
                            var tasks = await _graphServiceClient.Planner.Plans[plan.Id].Tasks.GetAsync();

                            if (tasks?.Value != null)
                            {
                                foreach (var task in tasks.Value)
                                {
                                    if (task != null)
                                    {
                                        var pomodoroTask = ConvertPlannerTaskToPomodoroTask(task, plan.Title ?? "未分類");
                                        importedTasks.Add(pomodoroTask);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Microsoft Plannerから {importedTasks.Count} 件のタスクをインポートしました。");
                return importedTasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft Plannerタスクのインポートエラー: {ex.Message}");
                throw new InvalidOperationException($"Microsoft Plannerからのタスクインポートに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Outlookタスクからタスクをインポートする
        /// </summary>
        public async Task<List<PomodoroTask>> ImportTasksFromOutlookAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("認証されていません。最初にAuthenticateAsync()を呼び出してください。");
            }

            try
            {
                // 注意: Outlookタスクは現在のMicrosoft Graph APIではサポートされていない可能性があります
                // 将来的にサポートされる場合に備えてメソッドは残しておきます
                
                Console.WriteLine("Outlookタスクのインポートは現在サポートされていません。");
                return importedTasks;

                /*
                // Outlookタスクを取得
                var tasks = await _graphServiceClient!.Me.Outlook.Tasks.GetAsync();

                if (tasks?.Value != null)
                {
                    foreach (var task in tasks.Value)
                    {
                        if (task != null)
                        {
                            var pomodoroTask = ConvertOutlookTaskToPomodoroTask(task);
                            importedTasks.Add(pomodoroTask);
                        }
                    }
                }

                Console.WriteLine($"Outlookから {importedTasks.Count} 件のタスクをインポートしました。");
                return importedTasks;
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Outlookタスクのインポートエラー: {ex.Message}");
                throw new InvalidOperationException($"Outlookからのタスクインポートに失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ログアウトする
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                _graphServiceClient = null;
                Console.WriteLine("Microsoft Graphからログアウトしました。");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ログアウトエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// Microsoft To-DoタスクをPomodoroTaskに変換する
        /// </summary>
        private PomodoroTask ConvertToDoTaskToPomodoroTask(TodoTask todoTask, string category)
        {
            var priority = todoTask.Importance switch
            {
                Importance.High => TaskPriority.High,
                Importance.Normal => TaskPriority.Medium,
                Importance.Low => TaskPriority.Low,
                _ => TaskPriority.Medium
            };

            var status = todoTask.Status switch
            {
                Microsoft.Graph.Models.TaskStatus.NotStarted => Models.TaskStatus.Todo,
                Microsoft.Graph.Models.TaskStatus.InProgress => Models.TaskStatus.InProgress,
                Microsoft.Graph.Models.TaskStatus.Completed => Models.TaskStatus.Completed,
                _ => Models.TaskStatus.Todo
            };

            var task = new PomodoroTask(todoTask.Title ?? "無題のタスク", 1)
            {
                Description = todoTask.Body?.Content ?? string.Empty,
                Category = category,
                Priority = priority,
                Status = status,
                TagsText = "Microsoft To-Do"
            };

            if (todoTask.CreatedDateTime.HasValue)
            {
                task.CreatedAt = todoTask.CreatedDateTime.Value.DateTime;
            }

            if (todoTask.CompletedDateTime != null && !string.IsNullOrEmpty(todoTask.CompletedDateTime.DateTime))
            {
                if (DateTime.TryParse(todoTask.CompletedDateTime.DateTime, out var completedDate))
                {
                    task.CompletedAt = completedDate;
                    task.IsCompleted = true;
                }
            }

            return task;
        }

        /// <summary>
        /// Microsoft PlannerタスクをPomodoroTaskに変換する
        /// </summary>
        private PomodoroTask ConvertPlannerTaskToPomodoroTask(PlannerTask plannerTask, string category)
        {
            var priority = plannerTask.Priority switch
            {
                1 or 2 or 3 => TaskPriority.High,
                4 or 5 or 6 => TaskPriority.Medium,
                _ => TaskPriority.Low
            };

            var status = plannerTask.PercentComplete switch
            {
                0 => Models.TaskStatus.Todo,
                100 => Models.TaskStatus.Completed,
                _ => Models.TaskStatus.InProgress
            };

            var task = new PomodoroTask(plannerTask.Title ?? "無題のタスク", 1)
            {
                Description = string.Empty,
                Category = category,
                Priority = priority,
                Status = status,
                TagsText = "Microsoft Planner"
            };

            if (plannerTask.CreatedDateTime.HasValue)
            {
                task.CreatedAt = plannerTask.CreatedDateTime.Value.DateTime;
            }

            if (plannerTask.CompletedDateTime.HasValue)
            {
                task.CompletedAt = plannerTask.CompletedDateTime.Value.DateTime;
                task.IsCompleted = true;
            }

            return task;
        }

        /// <summary>
        /// OutlookタスクをPomodoroTaskに変換する
        /// </summary>
        private PomodoroTask ConvertOutlookTaskToPomodoroTask(object outlookTask)
        {
            // 現在OutlookTaskはサポートされていないため、ダミー実装
            var task = new PomodoroTask("Outlookタスク（未サポート）", 1)
            {
                Description = "Outlookタスクのインポートは現在サポートされていません。",
                Category = "Outlook",
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Todo,
                TagsText = "Outlook, 未サポート"
            };

            return task;

            /*
            // 将来の実装用
            var priority = outlookTask.Importance switch
            {
                Importance.High => TaskPriority.High,
                Importance.Normal => TaskPriority.Medium,
                Importance.Low => TaskPriority.Low,
                _ => TaskPriority.Medium
            };

            var status = outlookTask.Status switch
            {
                Microsoft.Graph.Models.TaskStatus.NotStarted => Models.TaskStatus.Todo,
                Microsoft.Graph.Models.TaskStatus.InProgress => Models.TaskStatus.InProgress,
                Microsoft.Graph.Models.TaskStatus.Completed => Models.TaskStatus.Completed,
                _ => Models.TaskStatus.Todo
            };

            var task = new PomodoroTask(outlookTask.Subject ?? "無題のタスク", 1)
            {
                Description = outlookTask.Body?.Content ?? string.Empty,
                Category = "Outlook",
                Priority = priority,
                Status = status,
                TagsText = "Outlook"
            };

            if (outlookTask.CreatedDateTime.HasValue)
            {
                task.CreatedAt = outlookTask.CreatedDateTime.Value.DateTime;
            }

            if (outlookTask.CompletedDateTime?.DateTime != null)
            {
                task.CompletedAt = outlookTask.CompletedDateTime.DateTime.DateTime;
                task.IsCompleted = true;
            }

            return task;
            */
        }
    }
}
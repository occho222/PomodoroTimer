using Microsoft.Graph;
using Microsoft.Graph.Models;
using PomodoroTimer.Models;
using Azure.Identity;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// Microsoft Graph API�����T�[�r�X
    /// </summary>
    public class GraphService : IGraphService
    {
        private GraphServiceClient? _graphServiceClient;
        private readonly string[] _scopes = { "https://graph.microsoft.com/Tasks.ReadWrite", 
                                             "https://graph.microsoft.com/Group.Read.All" };
        private readonly AppSettings _settings;

        public bool IsAuthenticated => _graphServiceClient != null;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="settings">�A�v���P�[�V�����ݒ�</param>
        public GraphService(AppSettings? settings = null)
        {
            _settings = settings ?? new AppSettings();
            // GraphSettings���m���ɏ����������悤�Ɋm�F
            _settings.GraphSettings ??= new GraphSettings();
        }

        /// <summary>
        /// Microsoft Graph����F�؂��s��
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                // �N���C�A���gID���ݒ肳��Ă��Ȃ��ꍇ
                if (string.IsNullOrWhiteSpace(_settings.GraphSettings.ClientId))
                {
                    Console.WriteLine("Microsoft Graph API �̃N���C�A���gID���ݒ肳��Ă��܂���B");
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

                // �F�؂��e�X�g���邽�߁A���[�U�[�����擾
                var user = await _graphServiceClient.Me.GetAsync();
                
                if (user != null)
                {
                    Console.WriteLine($"Microsoft Graph�ɐ���ɔF�؂���܂����B���[�U�[: {user.DisplayName}");
                    _settings.GraphSettings.LastAuthenticationTime = DateTime.Now;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft Graph�F�؃G���[: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Microsoft To Do ����^�X�N���C���|�[�g����
        /// </summary>
        public async Task<List<PomodoroTask>> ImportTasksFromMicrosoftToDoAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("�F�؂���Ă��܂���B�ŏ���AuthenticateAsync()���Ăяo���Ă��������B");
            }

            try
            {
                // Microsoft To-Do�̃^�X�N���X�g���擾
                var taskLists = await _graphServiceClient!.Me.Todo.Lists.GetAsync();

                if (taskLists?.Value != null)
                {
                    foreach (var taskList in taskLists.Value)
                    {
                        if (taskList?.Id != null)
                        {
                            // �e���X�g�̃^�X�N���擾
                            var tasks = await _graphServiceClient.Me.Todo.Lists[taskList.Id].Tasks.GetAsync();

                            if (tasks?.Value != null)
                            {
                                foreach (var task in tasks.Value)
                                {
                                    if (task != null)
                                    {
                                        var pomodoroTask = ConvertToDoTaskToPomodoroTask(task, taskList.DisplayName ?? "������");
                                        importedTasks.Add(pomodoroTask);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Microsoft To-Do���� {importedTasks.Count} ���̃^�X�N���C���|�[�g���܂����B");
                return importedTasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft To-Do�^�X�N�̃C���|�[�g�G���[: {ex.Message}");
                throw new InvalidOperationException($"Microsoft To-Do����̃^�X�N�C���|�[�g�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Microsoft Planner����^�X�N���C���|�[�g����
        /// </summary>
        public async Task<List<PomodoroTask>> ImportTasksFromPlannerAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("�F�؂���Ă��܂���B�ŏ���AuthenticateAsync()���Ăяo���Ă��������B");
            }

            try
            {
                // ���[�U�[�̃v�������擾
                var plans = await _graphServiceClient!.Me.Planner.Plans.GetAsync();

                if (plans?.Value != null)
                {
                    foreach (var plan in plans.Value)
                    {
                        if (plan?.Id != null)
                        {
                            // �e�v�����̃^�X�N���擾
                            var tasks = await _graphServiceClient.Planner.Plans[plan.Id].Tasks.GetAsync();

                            if (tasks?.Value != null)
                            {
                                foreach (var task in tasks.Value)
                                {
                                    if (task != null)
                                    {
                                        var pomodoroTask = ConvertPlannerTaskToPomodoroTask(task, plan.Title ?? "������");
                                        importedTasks.Add(pomodoroTask);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"Microsoft Planner���� {importedTasks.Count} ���̃^�X�N���C���|�[�g���܂����B");
                return importedTasks;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Microsoft Planner�^�X�N�̃C���|�[�g�G���[: {ex.Message}");
                throw new InvalidOperationException($"Microsoft Planner����̃^�X�N�C���|�[�g�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Outlook�^�X�N����^�X�N���C���|�[�g����
        /// </summary>
        public Task<List<PomodoroTask>> ImportTasksFromOutlookAsync()
        {
            var importedTasks = new List<PomodoroTask>();

            if (!IsAuthenticated)
            {
                throw new InvalidOperationException("�F�؂���Ă��܂���B�ŏ���AuthenticateAsync()���Ăяo���Ă��������B");
            }

            try
            {
                // ����: Outlook�^�X�N�͌��݂�Microsoft Graph API�ł̓T�|�[�g����Ă��Ȃ��\��������܂�
                // �����I�ɃT�|�[�g�����ꍇ�ɔ����ă��\�b�h�͎c���Ă����܂�
                
                Console.WriteLine("Outlook�^�X�N�̃C���|�[�g�͌��݃T�|�[�g����Ă��܂���B");
                return Task.FromResult(importedTasks);

                /*
                // Outlook�^�X�N���擾
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

                Console.WriteLine($"Outlook���� {importedTasks.Count} ���̃^�X�N���C���|�[�g���܂����B");
                return importedTasks;
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Outlook�^�X�N�̃C���|�[�g�G���[: {ex.Message}");
                throw new InvalidOperationException($"Outlook����̃^�X�N�C���|�[�g�Ɏ��s���܂���: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ���O�A�E�g����
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                _graphServiceClient = null;
                Console.WriteLine("Microsoft Graph���烍�O�A�E�g���܂����B");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"���O�A�E�g�G���[: {ex.Message}");
            }
        }

        /// <summary>
        /// Microsoft To-Do�^�X�N��PomodoroTask�ɕϊ�����
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
                Microsoft.Graph.Models.TaskStatus.InProgress => Models.TaskStatus.Waiting,
                Microsoft.Graph.Models.TaskStatus.Completed => Models.TaskStatus.Completed,
                _ => Models.TaskStatus.Todo
            };

            var task = new PomodoroTask(todoTask.Title ?? "����̃^�X�N", 1)
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
        /// Microsoft Planner�^�X�N��PomodoroTask�ɕϊ�����
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
                _ => Models.TaskStatus.Waiting
            };

            var task = new PomodoroTask(plannerTask.Title ?? "����̃^�X�N", 1)
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
        /// Outlook�^�X�N��PomodoroTask�ɕϊ�����
        /// </summary>
        private PomodoroTask ConvertOutlookTaskToPomodoroTask(object outlookTask)
        {
            // ����OutlookTask�̓T�|�[�g����Ă��Ȃ����߁A�_�~�[����
            var task = new PomodoroTask("Outlook�^�X�N�i���T�|�[�g�j", 1)
            {
                Description = "Outlook�^�X�N�̃C���|�[�g�͌��݃T�|�[�g����Ă��܂���B",
                Category = "Outlook",
                Priority = TaskPriority.Medium,
                Status = Models.TaskStatus.Todo,
                TagsText = "Outlook, ���T�|�[�g"
            };

            return task;

            /*
            // �����̎����p
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
                Microsoft.Graph.Models.TaskStatus.InProgress => Models.TaskStatus.Waiting,
                Microsoft.Graph.Models.TaskStatus.Completed => Models.TaskStatus.Completed,
                _ => Models.TaskStatus.Todo
            };

            var task = new PomodoroTask(outlookTask.Subject ?? "����̃^�X�N", 1)
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
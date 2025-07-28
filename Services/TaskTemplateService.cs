using PomodoroTimer.Models;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;

namespace PomodoroTimer.Services
{
    public class TaskTemplateService : ITaskTemplateService
    {
        private readonly IDataPersistenceService _dataPersistenceService;
        private ObservableCollection<TaskTemplate> _templates;
        private const string TemplatesFileName = "task_templates.json";

        public TaskTemplateService(IDataPersistenceService dataPersistenceService)
        {
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            _templates = new ObservableCollection<TaskTemplate>();
        }

        public ObservableCollection<TaskTemplate> GetTemplates()
        {
            return _templates;
        }

        public void AddTemplate(TaskTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            if (_templates.Any(t => t.Name.Equals(template.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"「{template.Name}」という名前のテンプレートは既に存在します。");
            }

            template.CreatedAt = DateTime.Now;
            template.UpdatedAt = DateTime.Now;
            _templates.Add(template);
        }

        public void UpdateTemplate(TaskTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var existingTemplate = _templates.FirstOrDefault(t => t.Id == template.Id);
            if (existingTemplate == null)
            {
                throw new InvalidOperationException("更新対象のテンプレートが見つかりません。");
            }

            var duplicateName = _templates.FirstOrDefault(t => t.Id != template.Id && 
                t.Name.Equals(template.Name, StringComparison.OrdinalIgnoreCase));
            if (duplicateName != null)
            {
                throw new InvalidOperationException($"「{template.Name}」という名前のテンプレートは既に存在します。");
            }

            template.UpdatedAt = DateTime.Now;
            var index = _templates.IndexOf(existingTemplate);
            _templates[index] = template;
        }

        public void RemoveTemplate(TaskTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            _templates.Remove(template);
        }

        public TaskTemplate? GetTemplateById(Guid id)
        {
            return _templates.FirstOrDefault(t => t.Id == id);
        }

        public List<TaskTemplate> GetTemplatesByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category) || category == "すべて")
            {
                return _templates.ToList();
            }

            return _templates
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<TaskTemplate> SearchTemplates(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return _templates.ToList();
            }

            return _templates
                .Where(t => t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           t.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           t.TaskTitle.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           t.TaskDescription.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           t.Category.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                           t.Tags.Any(tag => tag.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public List<string> GetAllTemplateCategories()
        {
            return _templates
                .Select(t => t.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();
        }

        public async Task<TaskTemplate> CreateTemplateFromTaskAsync(PomodoroTask task, string templateName, string? description = null)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(templateName)) throw new ArgumentException("テンプレート名は必須です。", nameof(templateName));

            var template = new TaskTemplate(templateName, task.Title)
            {
                Description = description ?? $"「{task.Title}」から作成されたテンプレート",
                TaskDescription = task.Description,
                Category = task.Category,
                EstimatedPomodoros = (int)Math.Ceiling((double)task.EstimatedMinutes / 25),
                Priority = task.Priority,
                TagsText = task.TagsText
            };

            // チェックリスト情報をコピー
            template.DefaultChecklist.Clear();
            foreach (var checklistItem in task.Checklist)
            {
                template.DefaultChecklist.Add(new ChecklistItem(checklistItem.Text)
                {
                    IsChecked = checklistItem.IsChecked
                });
            }

            // リンク情報をコピー
            template.DefaultLinks.Clear();
            foreach (var linkItem in task.Links)
            {
                template.DefaultLinks.Add(new LinkItem(linkItem.Title, linkItem.Url)
                {
                    CreatedAt = linkItem.CreatedAt
                });
            }

            AddTemplate(template);
            await SaveTemplatesAsync();

            return template;
        }

        public PomodoroTask CreateTaskFromTemplate(TaskTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var task = template.CreateTask();
            RecordTemplateUsage(template);
            
            return task;
        }

        public TaskTemplate DuplicateTemplate(TaskTemplate template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var duplicatedTemplate = template.Clone();
            AddTemplate(duplicatedTemplate);

            return duplicatedTemplate;
        }

        public void RecordTemplateUsage(TaskTemplate template)
        {
            if (template == null) return;

            template.UsageCount++;
            template.LastUsedAt = DateTime.Now;
            template.UpdatedAt = DateTime.Now;
        }

        public List<TaskTemplate> GetFrequentlyUsedTemplates(int count = 5)
        {
            return _templates
                .Where(t => t.UsageCount > 0)
                .OrderByDescending(t => t.UsageCount)
                .ThenByDescending(t => t.LastUsedAt)
                .Take(count)
                .ToList();
        }

        public List<TaskTemplate> GetRecentlyUsedTemplates(int count = 5)
        {
            return _templates
                .Where(t => t.LastUsedAt.HasValue)
                .OrderByDescending(t => t.LastUsedAt)
                .Take(count)
                .ToList();
        }

        public async Task SaveTemplatesAsync()
        {
            try
            {
                await _dataPersistenceService.SaveDataAsync(TemplatesFileName, _templates.ToList());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テンプレートの保存に失敗しました: {ex.Message}", ex);
            }
        }

        public async Task LoadTemplatesAsync()
        {
            try
            {
                var templates = await _dataPersistenceService.LoadDataAsync<List<TaskTemplate>>(TemplatesFileName);
                if (templates != null)
                {
                    _templates.Clear();
                    foreach (var template in templates)
                    {
                        _templates.Add(template);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テンプレートの読み込みに失敗しました: {ex.Message}");
                _templates.Clear();
            }
        }

        public async Task ExportTemplatesToJsonAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("ファイルパスが指定されていません。", nameof(filePath));

            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(_templates.ToList(), options);
                await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"テンプレートのエクスポートに失敗しました: {ex.Message}", ex);
            }
        }

        public async Task ImportTemplatesFromJsonAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("ファイルパスが指定されていません。", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"ファイルが見つかりません: {filePath}");

            try
            {
                var json = await File.ReadAllTextAsync(filePath, System.Text.Encoding.UTF8);
                var importedTemplates = JsonSerializer.Deserialize<List<TaskTemplate>>(json);

                if (importedTemplates == null || !importedTemplates.Any())
                {
                    throw new InvalidOperationException("インポートするテンプレートが見つかりませんでした。");
                }

                var successCount = 0;
                var skippedCount = 0;
                var errors = new List<string>();

                foreach (var template in importedTemplates)
                {
                    try
                    {
                        if (_templates.Any(t => t.Name.Equals(template.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            template.Name = $"{template.Name} (インポート)";
                        }

                        template.Id = Guid.NewGuid();
                        template.CreatedAt = DateTime.Now;
                        template.UpdatedAt = DateTime.Now;
                        template.UsageCount = 0;
                        template.LastUsedAt = null;

                        _templates.Add(template);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        skippedCount++;
                        errors.Add($"テンプレート「{template.Name}」: {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    var errorMessage = string.Join("\n", errors);
                    throw new InvalidOperationException($"一部のテンプレートのインポートに失敗しました:\n{errorMessage}\n\n成功: {successCount}件, 失敗: {skippedCount}件");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"JSONファイルの解析に失敗しました: {ex.Message}", ex);
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"テンプレートのインポートに失敗しました: {ex.Message}", ex);
            }
        }
    }
}
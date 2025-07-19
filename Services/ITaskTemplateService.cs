using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    public interface ITaskTemplateService
    {
        ObservableCollection<TaskTemplate> GetTemplates();
        void AddTemplate(TaskTemplate template);
        void UpdateTemplate(TaskTemplate template);
        void RemoveTemplate(TaskTemplate template);
        TaskTemplate? GetTemplateById(Guid id);
        List<TaskTemplate> GetTemplatesByCategory(string category);
        List<TaskTemplate> SearchTemplates(string searchText);
        List<string> GetAllTemplateCategories();
        Task<TaskTemplate> CreateTemplateFromTaskAsync(PomodoroTask task, string templateName, string? description = null);
        PomodoroTask CreateTaskFromTemplate(TaskTemplate template);
        TaskTemplate DuplicateTemplate(TaskTemplate template);
        Task SaveTemplatesAsync();
        Task LoadTemplatesAsync();
        Task ExportTemplatesToJsonAsync(string filePath);
        Task ImportTemplatesFromJsonAsync(string filePath);
        void RecordTemplateUsage(TaskTemplate template);
        List<TaskTemplate> GetFrequentlyUsedTemplates(int count = 5);
        List<TaskTemplate> GetRecentlyUsedTemplates(int count = 5);
    }
}
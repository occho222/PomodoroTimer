using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PomodoroTimer.ViewModels
{
    public partial class TaskTemplateDialogViewModel : ObservableObject
    {
        private readonly ITaskTemplateService _templateService;
        private readonly IPomodoroService _pomodoroService;

        [ObservableProperty]
        private ObservableCollection<TaskTemplate> templates = new();

        [ObservableProperty]
        private ObservableCollection<TaskTemplate> filteredTemplates = new();

        [ObservableProperty]
        private ObservableCollection<string> templateCategories = new();

        [ObservableProperty]
        private TaskTemplate? selectedTemplate;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedTemplateCategory = "すべて";

        [ObservableProperty]
        private bool isEditMode = false;

        [ObservableProperty]
        private string templateEditName = string.Empty;

        [ObservableProperty]
        private string templateEditDescription = string.Empty;

        [ObservableProperty]
        private string templateEditTaskTitle = string.Empty;

        [ObservableProperty]
        private string templateEditTaskDescription = string.Empty;

        [ObservableProperty]
        private string templateEditCategory = string.Empty;


        [ObservableProperty]
        private TaskPriority templateEditPriority = TaskPriority.Medium;

        [ObservableProperty]
        private string templateEditTagsText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ChecklistItem> templateEditChecklist = new();

        [ObservableProperty]
        private ObservableCollection<LinkItem> templateEditLinks = new();

        public event Action<PomodoroTask>? TaskCreated;

        public TaskTemplateDialogViewModel(ITaskTemplateService templateService, IPomodoroService pomodoroService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));

            PropertyChanged += OnPropertyChanged;
            LoadTemplates();
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedTemplateCategory))
            {
                ApplyFilters();
            }
            else if (e.PropertyName == nameof(SelectedTemplate))
            {
                LoadSelectedTemplateForEdit();
            }
        }

        private async void LoadTemplates()
        {
            try
            {
                await _templateService.LoadTemplatesAsync();
                Templates = _templateService.GetTemplates();
                UpdateCategories();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テンプレートの読み込みに失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateCategories()
        {
            TemplateCategories.Clear();
            TemplateCategories.Add("すべて");
            
            var categories = _templateService.GetAllTemplateCategories();
            foreach (var category in categories)
            {
                TemplateCategories.Add(category);
            }
        }

        private void ApplyFilters()
        {
            var filtered = Templates.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = _templateService.SearchTemplates(SearchText);
            }

            if (!string.IsNullOrWhiteSpace(SelectedTemplateCategory) && SelectedTemplateCategory != "すべて")
            {
                filtered = filtered.Where(t => t.Category.Equals(SelectedTemplateCategory, StringComparison.OrdinalIgnoreCase));
            }

            FilteredTemplates.Clear();
            foreach (var template in filtered.OrderBy(t => t.Name))
            {
                FilteredTemplates.Add(template);
            }
        }

        private void LoadSelectedTemplateForEdit()
        {
            if (SelectedTemplate == null)
            {
                IsEditMode = false;
                ClearEditFields();
                return;
            }

            IsEditMode = true;
            TemplateEditName = SelectedTemplate.Name;
            TemplateEditDescription = SelectedTemplate.Description;
            TemplateEditTaskTitle = SelectedTemplate.TaskTitle;
            TemplateEditTaskDescription = SelectedTemplate.TaskDescription;
            TemplateEditCategory = SelectedTemplate.Category;
            TemplateEditPriority = SelectedTemplate.Priority;
            TemplateEditTagsText = SelectedTemplate.TagsText;

            // チェックリストもロード
            TemplateEditChecklist.Clear();
            foreach (var item in SelectedTemplate.DefaultChecklist)
            {
                TemplateEditChecklist.Add(new ChecklistItem(item.Text) { IsChecked = item.IsChecked });
            }

            // リンクもロード
            TemplateEditLinks.Clear();
            foreach (var item in SelectedTemplate.DefaultLinks)
            {
                TemplateEditLinks.Add(new LinkItem(item.Title, item.Url) { CreatedAt = item.CreatedAt });
            }
        }

        private void ClearEditFields()
        {
            TemplateEditName = string.Empty;
            TemplateEditDescription = string.Empty;
            TemplateEditTaskTitle = string.Empty;
            TemplateEditTaskDescription = string.Empty;
            TemplateEditCategory = string.Empty;
            TemplateEditPriority = TaskPriority.Medium;
            TemplateEditTagsText = string.Empty;
            TemplateEditChecklist.Clear();
            TemplateEditLinks.Clear();
        }

        [RelayCommand]
        private void CreateTemplate()
        {
            IsEditMode = true;
            SelectedTemplate = null;
            ClearEditFields();
        }

        [RelayCommand]
        private async Task SaveTemplate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TemplateEditName))
                {
                    MessageBox.Show("テンプレート名を入力してください。", "入力エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(TemplateEditTaskTitle))
                {
                    MessageBox.Show("タスクタイトルを入力してください。", "入力エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                var template = SelectedTemplate ?? new TaskTemplate();
                template.Name = TemplateEditName;
                template.Description = TemplateEditDescription;
                template.TaskTitle = TemplateEditTaskTitle;
                template.TaskDescription = TemplateEditTaskDescription;
                template.Category = TemplateEditCategory;
                template.Priority = TemplateEditPriority;
                template.TagsText = TemplateEditTagsText;

                // チェックリストも保存
                template.DefaultChecklist.Clear();
                foreach (var item in TemplateEditChecklist)
                {
                    template.DefaultChecklist.Add(new ChecklistItem(item.Text) { IsChecked = item.IsChecked });
                }

                // リンクも保存
                template.DefaultLinks.Clear();
                foreach (var item in TemplateEditLinks)
                {
                    template.DefaultLinks.Add(new LinkItem(item.Title, item.Url) { CreatedAt = item.CreatedAt });
                }

                if (SelectedTemplate == null)
                {
                    _templateService.AddTemplate(template);
                }
                else
                {
                    _templateService.UpdateTemplate(template);
                }

                await _templateService.SaveTemplatesAsync();
                UpdateCategories();
                ApplyFilters();
                
                SelectedTemplate = template;
                IsEditMode = false;

                MessageBox.Show("テンプレートを保存しました。", "保存完了", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テンプレートの保存に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            if (SelectedTemplate != null)
            {
                LoadSelectedTemplateForEdit();
            }
            else
            {
                IsEditMode = false;
                ClearEditFields();
            }
        }

        [RelayCommand]
        private void EditTemplate(TaskTemplate template)
        {
            if (template == null) return;
            
            SelectedTemplate = template;
            LoadSelectedTemplateForEdit();
        }

        [RelayCommand]
        private async Task DuplicateTemplate(TaskTemplate template)
        {
            if (template == null) return;

            try
            {
                var duplicated = _templateService.DuplicateTemplate(template);
                await _templateService.SaveTemplatesAsync();
                ApplyFilters();
                SelectedTemplate = duplicated;

                MessageBox.Show("テンプレートを複製しました。", "複製完了", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テンプレートの複製に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private async Task DeleteTemplate(TaskTemplate template)
        {
            if (template == null) return;

            var result = MessageBox.Show($"テンプレート「{template.Name}」を削除しますか？", "削除確認", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _templateService.RemoveTemplate(template);
                    await _templateService.SaveTemplatesAsync();
                    UpdateCategories();
                    ApplyFilters();

                    if (SelectedTemplate == template)
                    {
                        SelectedTemplate = null;
                        IsEditMode = false;
                    }

                    MessageBox.Show("テンプレートを削除しました。", "削除完了", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"テンプレートの削除に失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void CreateTaskFromTemplate(TaskTemplate template)
        {
            if (template == null) return;

            try
            {
                var task = _templateService.CreateTaskFromTemplate(template);
                _pomodoroService.AddTask(task);
                
                TaskCreated?.Invoke(task);

                MessageBox.Show($"テンプレート「{template.Name}」からタスクを作成しました。", "タスク作成", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"タスクの作成に失敗しました: {ex.Message}", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedTemplateCategory = "すべて";
        }

        [RelayCommand]
        private async Task ImportTemplates()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSONファイル (*.json)|*.json",
                Title = "テンプレートのインポート"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _templateService.ImportTemplatesFromJsonAsync(openFileDialog.FileName);
                    await _templateService.SaveTemplatesAsync();
                    UpdateCategories();
                    ApplyFilters();

                    MessageBox.Show("テンプレートのインポートが完了しました。", "インポート完了", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"テンプレートのインポートに失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ExportTemplates()
        {
            if (!Templates.Any())
            {
                MessageBox.Show("エクスポートするテンプレートがありません。", "情報", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSONファイル (*.json)|*.json",
                Title = "テンプレートのエクスポート",
                FileName = $"task_templates_{DateTime.Now:yyyyMMdd_HHmmss}.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    await _templateService.ExportTemplatesToJsonAsync(saveFileDialog.FileName);
                    MessageBox.Show("テンプレートのエクスポートが完了しました。", "エクスポート完了", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"テンプレートのエクスポートに失敗しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void AddChecklistItem()
        {
            TemplateEditChecklist.Add(new ChecklistItem("新しいアイテム"));
        }

        [RelayCommand]
        private void RemoveChecklistItem(ChecklistItem item)
        {
            if (item != null)
            {
                TemplateEditChecklist.Remove(item);
            }
        }

        [RelayCommand]
        private void AddLinkItem()
        {
            TemplateEditLinks.Add(new LinkItem("新しいリンク", "https://"));
        }

        [RelayCommand]
        private void RemoveLinkItem(LinkItem item)
        {
            if (item != null)
            {
                TemplateEditLinks.Remove(item);
            }
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;

namespace PomodoroTimer.ViewModels
{
    public partial class QuickTemplateDialogViewModel : ObservableObject
    {
        private readonly ITaskTemplateService _templateService;

        [ObservableProperty]
        private ObservableCollection<TaskTemplate> templates = new();

        [ObservableProperty]
        private ObservableCollection<TaskTemplate> filteredTemplates = new();

        [ObservableProperty]
        private ObservableCollection<TaskTemplate> frequentTemplates = new();

        [ObservableProperty]
        private ObservableCollection<string> templateCategories = new();

        [ObservableProperty]
        private TaskTemplate? selectedTemplate;

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private string selectedTemplateCategory = "すべて";

        [ObservableProperty]
        private bool hasFrequentTemplates = false;

        [ObservableProperty]
        private bool hasSelectedTemplate = false;

        public event Action<TaskTemplate>? TemplateSelected;

        public QuickTemplateDialogViewModel(ITaskTemplateService templateService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));

            PropertyChanged += OnPropertyChanged;
            LoadTemplates();
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(SelectedTemplateCategory))
            {
                ApplyFilter();
            }
            else if (e.PropertyName == nameof(SelectedTemplate))
            {
                HasSelectedTemplate = SelectedTemplate != null;
            }
        }

        private async void LoadTemplates()
        {
            try
            {
                await _templateService.LoadTemplatesAsync();
                Templates = _templateService.GetTemplates();
                LoadCategories();
                LoadFrequentTemplates();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テンプレートの読み込みに失敗しました: {ex.Message}");
            }
        }

        private void LoadFrequentTemplates()
        {
            FrequentTemplates.Clear();
            var frequentList = _templateService.GetFrequentlyUsedTemplates(5);
            
            foreach (var template in frequentList)
            {
                FrequentTemplates.Add(template);
            }

            HasFrequentTemplates = FrequentTemplates.Any();
        }

        private void LoadCategories()
        {
            TemplateCategories.Clear();
            TemplateCategories.Add("すべて");
            
            var categories = _templateService.GetAllTemplateCategories();
            foreach (var category in categories)
            {
                TemplateCategories.Add(category);
            }
        }

        private void ApplyFilter()
        {
            FilteredTemplates.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Templates.AsEnumerable()
                : _templateService.SearchTemplates(SearchText);

            if (!string.IsNullOrWhiteSpace(SelectedTemplateCategory) && SelectedTemplateCategory != "すべて")
            {
                filtered = filtered.Where(t => t.Category.Equals(SelectedTemplateCategory, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var template in filtered.OrderBy(t => t.Name))
            {
                FilteredTemplates.Add(template);
            }
        }

        [RelayCommand]
        private void SelectTemplate(TaskTemplate template)
        {
            if (template != null)
            {
                TemplateSelected?.Invoke(template);
            }
        }

        [RelayCommand]
        private void ShowTemplateDetails(TaskTemplate template)
        {
            if (template != null)
            {
                SelectedTemplate = template;
            }
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedTemplateCategory = "すべて";
        }
    }
}
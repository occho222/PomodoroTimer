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
        private TaskTemplate? selectedTemplate;

        [ObservableProperty]
        private string searchText = string.Empty;

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
            if (e.PropertyName == nameof(SearchText))
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

        private void ApplyFilter()
        {
            FilteredTemplates.Clear();

            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Templates.ToList()
                : _templateService.SearchTemplates(SearchText);

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
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}
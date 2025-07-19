using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    public partial class QuickTemplateDialog : Window
    {
        public QuickTemplateDialogViewModel ViewModel { get; }
        public TaskTemplate? SelectedTemplate { get; private set; }

        public QuickTemplateDialog(QuickTemplateDialogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = ViewModel;

            ViewModel.TemplateSelected += OnTemplateSelected;
        }

        private void OnTemplateSelected(TaskTemplate template)
        {
            SelectedTemplate = template;
            DialogResult = true;
            Close();
        }

        private void TemplateListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TemplateListBox.SelectedItem is TaskTemplate template)
            {
                ViewModel.SelectTemplateCommand.Execute(template);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
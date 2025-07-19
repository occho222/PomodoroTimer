using System.Windows;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    public partial class TaskTemplateDialog : Window
    {
        public TaskTemplateDialogViewModel ViewModel { get; }

        public TaskTemplateDialog(TaskTemplateDialogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
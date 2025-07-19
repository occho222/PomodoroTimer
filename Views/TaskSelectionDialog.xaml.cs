using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTimer.Views
{
    public partial class TaskSelectionDialog : Window
    {
        public TaskSelectionDialogViewModel ViewModel { get; }
        
        public TaskSelectionDialog(TaskSelectionDialogViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = viewModel;
            
            // ダイアログの結果を監視
            ViewModel.DialogResultChanged += OnDialogResultChanged;
        }

        private void OnDialogResultChanged(bool? result)
        {
            DialogResult = result;
        }

        private void TaskItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is PomodoroTask task)
            {
                ViewModel.SelectedTask = task;
                
                // ダブルクリックで即座に選択
                if (e.ClickCount == 2)
                {
                    ViewModel.ContinueWithSelectedTaskCommand.Execute(null);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel.DialogResultChanged -= OnDialogResultChanged;
            base.OnClosed(e);
        }
    }
}
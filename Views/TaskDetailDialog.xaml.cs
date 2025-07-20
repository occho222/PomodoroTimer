using PomodoroTimer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTimer.Views
{
    public partial class TaskDetailDialog : Window
    {
        public TaskDetailDialogViewModel ViewModel { get; }

        public TaskDetailDialog(TaskDetailDialogViewModel viewModel)
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


        private void NewChecklistItem_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = sender as System.Windows.Controls.TextBox;
                if (textBox != null && !string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ViewModel.AddChecklistItemCommand.Execute(textBox.Text);
                    textBox.Clear();
                    e.Handled = true;
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
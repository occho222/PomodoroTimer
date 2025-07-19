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
            
            // リッチテキストボックスの変更を監視
            DetailedDescriptionRichTextBox.TextChanged += OnRichTextChanged;
        }

        private void OnDialogResultChanged(bool? result)
        {
            DialogResult = result;
        }

        private void OnRichTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // リッチテキストの内容をViewModelに反映
            var richTextBox = sender as System.Windows.Controls.RichTextBox;
            if (richTextBox != null)
            {
                var textRange = new System.Windows.Documents.TextRange(
                    richTextBox.Document.ContentStart,
                    richTextBox.Document.ContentEnd);
                ViewModel.DetailedDescription = textRange.Text;
            }
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
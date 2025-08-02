using PomodoroTimer.ViewModels;
using PomodoroTimer.Helpers;
using System.Windows;
using System.Windows.Input;
using System.IO;

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


        /// <summary>
        /// 説明欄での画像ペーストイベントハンドラー
        /// </summary>
        private void DescriptionEditor_ImagePasted(object sender, ImagePasteEventArgs e)
        {
            try
            {
                var attachmentPath = ViewModel.SaveImageToAttachmentFolder(e.Image);
                if (ViewModel.AddAttachmentToList(attachmentPath))
                {
                    e.ImagePath = attachmentPath;
                    e.IsHandled = true;
                    ViewModel.AutoSave();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"画像の貼り付けに失敗しました: {ex.Message}", "エラー", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            ViewModel.DialogResultChanged -= OnDialogResultChanged;
            base.OnClosed(e);
        }
    }
}
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

        private void DescriptionTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // デバッグ用：キーイベントを確認
            System.Diagnostics.Debug.WriteLine($"PreviewKeyDown: Key={e.Key}, Modifiers={Keyboard.Modifiers}");
            
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("Ctrl+V detected!");
                    
                    // クリップボードの内容を確認
                    bool hasImage = System.Windows.Clipboard.ContainsImage();
                    bool hasText = System.Windows.Clipboard.ContainsText();
                    System.Diagnostics.Debug.WriteLine($"Clipboard: HasImage={hasImage}, HasText={hasText}");
                    
                    if (hasImage)
                    {
                        System.Diagnostics.Debug.WriteLine("Executing PasteImageCommand...");
                        ViewModel.PasteImageCommand.Execute(null);
                        e.Handled = true; // テキストの貼り付けを防ぐ
                        System.Diagnostics.Debug.WriteLine("Image paste completed!");
                        return;
                    }
                    // テキストの場合はデフォルトの貼り付け動作を実行（e.Handledはfalseのまま）
                    System.Diagnostics.Debug.WriteLine("No image in clipboard, allowing default paste behavior");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in paste operation: {ex}");
                    System.Windows.MessageBox.Show($"画像の貼り付けに失敗しました: {ex.Message}", "エラー", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    e.Handled = true; // エラーの場合も処理済みとする
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
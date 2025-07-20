using PomodoroTimer.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// プロジェクトとタグの管理ダイアログ
    /// </summary>
    public partial class ProjectTagManagerDialog : Window
    {
        public ProjectTagManagerDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 新規プロジェクト入力でのキー入力処理
        /// </summary>
        private void NewProject_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is ProjectTagManagerViewModel viewModel && sender is System.Windows.Controls.TextBox textBox)
            {
                viewModel.AddProjectCommand?.Execute(textBox.Text);
                textBox.Clear();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 新規タグ入力でのキー入力処理
        /// </summary>
        private void NewTag_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is ProjectTagManagerViewModel viewModel && sender is System.Windows.Controls.TextBox textBox)
            {
                viewModel.AddTagCommand?.Execute(textBox.Text);
                textBox.Clear();
                e.Handled = true;
            }
        }
    }
}
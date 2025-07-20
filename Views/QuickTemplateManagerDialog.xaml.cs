using PomodoroTimer.ViewModels;
using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// クイックテンプレート管理ダイアログ
    /// </summary>
    public partial class QuickTemplateManagerDialog : Window
    {
        public QuickTemplateManagerDialog(QuickTemplateManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // ダイアログクローズイベントの設定
            viewModel.DialogClosing += () => Close();
        }
    }
}
using PomodoroTimer.ViewModels;
using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// タスク一括操作ダイアログ
    /// </summary>
    public partial class BulkOperationDialog : Window
    {
        public BulkOperationDialog(BulkOperationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // ダイアログクローズイベントの設定
            viewModel.DialogClosing += () => Close();
        }
    }
}
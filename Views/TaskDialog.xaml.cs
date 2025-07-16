using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// タスク追加ダイアログ
    /// </summary>
    public partial class TaskDialog : Window
    {
        /// <summary>
        /// タスクのタイトル
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// 予定ポモドーロ数
        /// </summary>
        public int EstimatedPomodoros { get; set; } = 1;

        /// <summary>
        /// タスクの説明
        /// </summary>
        public string TaskDescription { get; set; } = string.Empty;

        public TaskDialog()
        {
            InitializeComponent();
            DataContext = new TaskDialogViewModel();
        }

        /// <summary>
        /// OKボタンがクリックされた時の処理
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (TaskDialogViewModel)DataContext;
            
            // 入力値の検証
            if (string.IsNullOrWhiteSpace(viewModel.TaskTitle))
            {
                MessageBox.Show("Please enter a task name.", "Input Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.EstimatedPomodoros < 1 || viewModel.EstimatedPomodoros > 10)
            {
                MessageBox.Show("Estimated pomodoros must be between 1 and 10.", "Input Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // プロパティに値を設定
            TaskTitle = viewModel.TaskTitle;
            EstimatedPomodoros = viewModel.EstimatedPomodoros;
            TaskDescription = viewModel.TaskDescription;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセルボタンがクリックされた時の処理
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// TaskDialog用のViewModel
    /// </summary>
    public partial class TaskDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private int estimatedPomodoros = 1;

        [ObservableProperty]
        private string taskDescription = string.Empty;
    }
}
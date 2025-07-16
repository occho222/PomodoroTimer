using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Models;

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

        /// <summary>
        /// タスクのカテゴリ
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// タスクのタグ
        /// </summary>
        public string TagsText { get; set; } = string.Empty;

        /// <summary>
        /// タスクの優先度
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskDialog()
        {
            InitializeComponent();
            DataContext = new TaskDialogViewModel();
        }

        /// <summary>
        /// 既存タスクを編集用に開くコンストラクタ
        /// </summary>
        /// <param name="task">編集するタスク</param>
        public TaskDialog(PomodoroTask task)
        {
            InitializeComponent();
            
            var viewModel = new TaskDialogViewModel
            {
                TaskTitle = task.Title,
                EstimatedPomodoros = task.EstimatedPomodoros,
                TaskDescription = task.Description,
                Category = task.Category,
                TagsText = task.TagsText,
                Priority = task.Priority
            };
            
            DataContext = viewModel;
            Title = "タスクの編集";
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
                MessageBox.Show("タスク名を入力してください。", "入力エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.EstimatedPomodoros < 1 || viewModel.EstimatedPomodoros > 10)
            {
                MessageBox.Show("予定ポモドーロ数は1から10の間で設定してください。", "入力エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // プロパティに値を設定
            TaskTitle = viewModel.TaskTitle;
            EstimatedPomodoros = viewModel.EstimatedPomodoros;
            TaskDescription = viewModel.TaskDescription;
            Category = viewModel.Category;
            TagsText = viewModel.TagsText;
            Priority = viewModel.Priority;

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

        [ObservableProperty]
        private string category = string.Empty;

        [ObservableProperty]
        private string tagsText = string.Empty;

        [ObservableProperty]
        private TaskPriority priority = TaskPriority.Medium;
    }
}
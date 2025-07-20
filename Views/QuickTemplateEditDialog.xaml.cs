using PomodoroTimer.Models;
using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// クイックテンプレート編集ダイアログ
    /// </summary>
    public partial class QuickTemplateEditDialog : Window
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskDescription { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public int EstimatedMinutes { get; set; } = 25;
        public string BackgroundColor { get; set; } = "#3B82F6";

        public QuickTemplateEditDialog()
        {
            InitializeComponent();
            InitializeForm();
        }

        public QuickTemplateEditDialog(QuickTemplate template) : this()
        {
            if (template != null)
            {
                LoadTemplate(template);
            }
        }

        private void InitializeForm()
        {
            // 優先度の初期値設定
            PriorityComboBox.SelectedItem = TaskPriority.Medium;
            
            // カテゴリの初期値設定
            CategoryComboBox.Text = "一般";
        }

        private void LoadTemplate(QuickTemplate template)
        {
            DisplayName = template.DisplayName;
            Description = template.Description;
            TaskTitle = template.TaskTitle;
            TaskDescription = template.TaskDescription;
            Category = template.Category;
            Tags = string.Join(", ", template.Tags);
            Priority = template.Priority;
            EstimatedMinutes = template.EstimatedMinutes;
            BackgroundColor = template.BackgroundColor;

            // フォームに値を設定
            DisplayNameTextBox.Text = DisplayName;
            DescriptionTextBox.Text = Description;
            TaskTitleTextBox.Text = TaskTitle;
            TaskDescriptionTextBox.Text = TaskDescription;
            CategoryComboBox.Text = Category;
            TagsTextBox.Text = Tags;
            PriorityComboBox.SelectedItem = Priority;
            EstimatedMinutesSlider.Value = EstimatedMinutes;

            // 背景色の設定
            SetBackgroundColorRadio(BackgroundColor);
        }

        private void SetBackgroundColorRadio(string color)
        {
            switch (color)
            {
                case "#3B82F6":
                    BlueColorRadio.IsChecked = true;
                    break;
                case "#10B981":
                    GreenColorRadio.IsChecked = true;
                    break;
                case "#F59E0B":
                    YellowColorRadio.IsChecked = true;
                    break;
                case "#8B5CF6":
                    PurpleColorRadio.IsChecked = true;
                    break;
                case "#EF4444":
                    RedColorRadio.IsChecked = true;
                    break;
                case "#6B7280":
                    GrayColorRadio.IsChecked = true;
                    break;
                default:
                    BlueColorRadio.IsChecked = true;
                    break;
            }
        }

        private string GetSelectedBackgroundColor()
        {
            if (BlueColorRadio.IsChecked == true) return "#3B82F6";
            if (GreenColorRadio.IsChecked == true) return "#10B981";
            if (YellowColorRadio.IsChecked == true) return "#F59E0B";
            if (PurpleColorRadio.IsChecked == true) return "#8B5CF6";
            if (RedColorRadio.IsChecked == true) return "#EF4444";
            if (GrayColorRadio.IsChecked == true) return "#6B7280";
            return "#3B82F6"; // デフォルト
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 入力値の検証
            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                System.Windows.MessageBox.Show("表示名を入力してください。", "入力エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                DisplayNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TaskTitleTextBox.Text))
            {
                System.Windows.MessageBox.Show("タスクタイトルを入力してください。", "入力エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TaskTitleTextBox.Focus();
                return;
            }

            // フォームから値を取得
            DisplayName = DisplayNameTextBox.Text.Trim();
            Description = DescriptionTextBox.Text.Trim();
            TaskTitle = TaskTitleTextBox.Text.Trim();
            TaskDescription = TaskDescriptionTextBox.Text.Trim();
            Category = string.IsNullOrWhiteSpace(CategoryComboBox.Text) ? "一般" : CategoryComboBox.Text.Trim();
            Tags = TagsTextBox.Text.Trim();
            Priority = (TaskPriority)(PriorityComboBox.SelectedItem ?? TaskPriority.Medium);
            EstimatedMinutes = (int)EstimatedMinutesSlider.Value;
            BackgroundColor = GetSelectedBackgroundColor();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
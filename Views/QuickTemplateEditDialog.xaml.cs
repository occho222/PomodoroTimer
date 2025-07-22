using PomodoroTimer.Models;
using System.Windows;
using System.Windows.Controls;

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
        public List<ChecklistItem> DefaultChecklist { get; set; } = new();

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

            // チェックリストの設定
            DefaultChecklist = new List<ChecklistItem>(template.DefaultChecklist);
            RefreshChecklistUI();
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

            // チェックリストを更新
            UpdateChecklistFromUI();

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddChecklistItemButton_Click(object sender, RoutedEventArgs e)
        {
            DefaultChecklist.Add(new ChecklistItem("新しいアイテム"));
            RefreshChecklistUI();
        }

        private void RefreshChecklistUI()
        {
            ChecklistPanel.Children.Clear();
            
            for (int i = 0; i < DefaultChecklist.Count; i++)
            {
                var item = DefaultChecklist[i];
                var itemPanel = new Grid();
                itemPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                itemPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                itemPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
                itemPanel.Margin = new Thickness(0, 2, 0, 2);

                var checkBox = new System.Windows.Controls.CheckBox
                {
                    IsChecked = item.IsChecked,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                checkBox.SetValue(Grid.ColumnProperty, 0);
                var itemIndex = i;
                checkBox.Checked += (s, e) => DefaultChecklist[itemIndex].IsChecked = true;
                checkBox.Unchecked += (s, e) => DefaultChecklist[itemIndex].IsChecked = false;

                var textBox = new System.Windows.Controls.TextBox
                {
                    Text = item.Text,
                    BorderThickness = new Thickness(0),
                    Background = System.Windows.Media.Brushes.Transparent,
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                textBox.SetValue(Grid.ColumnProperty, 1);
                textBox.TextChanged += (s, e) => DefaultChecklist[itemIndex].Text = textBox.Text;

                var deleteButton = new System.Windows.Controls.Button
                {
                    Content = "×",
                    Width = 20,
                    Height = 20,
                    Background = System.Windows.Media.Brushes.LightCoral,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 10
                };
                deleteButton.SetValue(Grid.ColumnProperty, 2);
                deleteButton.Click += (s, e) =>
                {
                    DefaultChecklist.RemoveAt(itemIndex);
                    RefreshChecklistUI();
                };

                itemPanel.Children.Add(checkBox);
                itemPanel.Children.Add(textBox);
                itemPanel.Children.Add(deleteButton);
                ChecklistPanel.Children.Add(itemPanel);
            }
        }

        private void UpdateChecklistFromUI()
        {
            // UIから既に更新済みなので特に処理なし
        }
    }
}
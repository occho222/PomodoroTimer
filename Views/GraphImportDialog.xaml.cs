using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// Microsoft Graphインポートダイアログ
    /// </summary>
    public partial class GraphImportDialog : Window
    {
        /// <summary>
        /// Microsoft To-Doからインポートするかどうか
        /// </summary>
        public bool ImportFromMicrosoftToDo => MicrosoftToDoCheckBox.IsChecked ?? false;

        /// <summary>
        /// Microsoft Plannerからインポートするかどうか
        /// </summary>
        public bool ImportFromPlanner => PlannerCheckBox.IsChecked ?? false;

        /// <summary>
        /// Outlookからインポートするかどうか
        /// </summary>
        public bool ImportFromOutlook => OutlookCheckBox.IsChecked ?? false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GraphImportDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// インポートボタンクリック
        /// </summary>
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            // 少なくとも一つのソースが選択されているかチェック
            if (!ImportFromMicrosoftToDo && !ImportFromPlanner && !ImportFromOutlook)
            {
                System.Windows.MessageBox.Show("少なくとも一つのインポート元を選択してください。", "選択エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
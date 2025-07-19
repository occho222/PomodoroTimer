using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// チェックリスト項目のモデル
    /// </summary>
    public partial class ChecklistItem : ObservableObject
    {
        /// <summary>
        /// チェックリスト項目の一意のID
        /// </summary>
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        /// <summary>
        /// チェックリスト項目のテキスト
        /// </summary>
        [ObservableProperty]
        private string text = string.Empty;

        /// <summary>
        /// チェック状態
        /// </summary>
        [ObservableProperty]
        private bool isChecked = false;

        /// <summary>
        /// 作成日時
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        /// <summary>
        /// 完了日時
        /// </summary>
        [ObservableProperty]
        private DateTime? completedAt;

        public ChecklistItem()
        {
        }

        public ChecklistItem(string text)
        {
            Text = text ?? string.Empty;
        }

        /// <summary>
        /// チェック状態を切り替える
        /// </summary>
        public void ToggleCheck()
        {
            IsChecked = !IsChecked;
            CompletedAt = IsChecked ? DateTime.Now : null;
        }
    }
}
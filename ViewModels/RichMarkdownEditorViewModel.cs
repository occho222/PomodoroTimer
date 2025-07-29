using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Documents;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// リッチマークダウンエディタのViewModel
    /// </summary>
    public partial class RichMarkdownEditorViewModel : ObservableObject
    {
        /// <summary>
        /// マークダウンテキスト
        /// </summary>
        [ObservableProperty]
        private string markdownText = string.Empty;

        /// <summary>
        /// FlowDocument
        /// </summary>
        [ObservableProperty]
        private FlowDocument? document;

        /// <summary>
        /// 読み取り専用かどうか
        /// </summary>
        [ObservableProperty]
        private bool isReadOnly = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RichMarkdownEditorViewModel()
        {
            Document = new FlowDocument();
        }

        /// <summary>
        /// マークダウンテキストが変更された時の処理
        /// </summary>
        partial void OnMarkdownTextChanged(string value)
        {
            // ViewのRichMarkdownEditorが自動的に処理するため、ここでは何もしない
        }
    }
}
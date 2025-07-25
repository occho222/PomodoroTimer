using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// リンク項目のモデル
    /// </summary>
    public partial class LinkItem : ObservableObject
    {
        /// <summary>
        /// リンク項目の一意のID
        /// </summary>
        [ObservableProperty]
        private Guid id = Guid.NewGuid();

        /// <summary>
        /// リンクのタイトル（表示名）
        /// </summary>
        [ObservableProperty]
        private string title = string.Empty;

        /// <summary>
        /// リンクのURL
        /// </summary>
        [ObservableProperty]
        private string url = string.Empty;

        /// <summary>
        /// 作成日時
        /// </summary>
        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        public LinkItem()
        {
        }

        public LinkItem(string title, string url)
        {
            Title = title ?? string.Empty;
            Url = url ?? string.Empty;
        }

        /// <summary>
        /// URLが有効かどうかを確認
        /// </summary>
        public bool IsValidUrl()
        {
            if (string.IsNullOrWhiteSpace(Url))
                return false;

            var urlToCheck = Url;
            if (!urlToCheck.StartsWith("http://") && !urlToCheck.StartsWith("https://"))
                urlToCheck = "https://" + urlToCheck;

            return Uri.TryCreate(urlToCheck, UriKind.Absolute, out _);
        }

        /// <summary>
        /// 正規化されたURLを取得
        /// </summary>
        public string GetNormalizedUrl()
        {
            if (string.IsNullOrWhiteSpace(Url))
                return string.Empty;

            if (!Url.StartsWith("http://") && !Url.StartsWith("https://"))
                return "https://" + Url;

            return Url;
        }
    }
}
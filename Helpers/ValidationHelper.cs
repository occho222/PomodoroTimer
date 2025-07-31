using System;

namespace PomodoroTimer.Helpers
{
    /// <summary>
    /// 入力検証の共通処理ヘルパー
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// 必須項目の検証を行う
        /// </summary>
        /// <param name="value">検証対象の値</param>
        /// <param name="fieldName">項目名</param>
        /// <returns>検証結果（true: 有効, false: 無効）</returns>
        public static bool ValidateRequired(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ErrorHandler.ShowWarning($"{fieldName}を入力してください。");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 数値範囲の検証を行う
        /// </summary>
        /// <param name="value">検証対象の値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <param name="fieldName">項目名</param>
        /// <returns>検証結果（true: 有効, false: 無効）</returns>
        public static bool ValidateRange(int value, int min, int max, string fieldName)
        {
            if (value < min || value > max)
            {
                ErrorHandler.ShowWarning($"{fieldName}は{min}〜{max}の範囲で入力してください。");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// 文字列長の検証を行う
        /// </summary>
        /// <param name="value">検証対象の値</param>
        /// <param name="maxLength">最大文字数</param>
        /// <param name="fieldName">項目名</param>
        /// <returns>検証結果（true: 有効, false: 無効）</returns>
        public static bool ValidateMaxLength(string? value, int maxLength, string fieldName)
        {
            if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
            {
                ErrorHandler.ShowWarning($"{fieldName}は{maxLength}文字以内で入力してください。");
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// URLの形式検証を行う
        /// </summary>
        /// <param name="url">検証対象のURL</param>
        /// <param name="fieldName">項目名</param>
        /// <returns>検証結果（true: 有効, false: 無効）</returns>
        public static bool ValidateUrl(string? url, string fieldName = "URL")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return true; // 空の場合は有効とする（必須チェックは別途行う）
            }
            
            // httpまたはhttpsプロトコルの自動補完
            var processedUrl = url;
            if (!processedUrl.StartsWith("http://") && !processedUrl.StartsWith("https://"))
            {
                processedUrl = "https://" + processedUrl;
            }
            
            if (!Uri.TryCreate(processedUrl, UriKind.Absolute, out var uri) || 
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                ErrorHandler.ShowWarning($"無効な{fieldName}です。");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 複数の検証を組み合わせる（すべて通過した場合のみtrue）
        /// </summary>
        /// <param name="validations">検証関数の配列</param>
        /// <returns>すべての検証が通過した場合true</returns>
        public static bool ValidateAll(params Func<bool>[] validations)
        {
            bool allValid = true;
            foreach (var validation in validations)
            {
                if (!validation())
                {
                    allValid = false;
                    // 最初のエラーで止めずに、すべてのチェックを実行
                }
            }
            return allValid;
        }
    }
}
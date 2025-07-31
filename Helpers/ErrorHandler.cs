using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PomodoroTimer.Helpers
{
    /// <summary>
    /// エラーハンドリングの共通化クラス
    /// </summary>
    public static class ErrorHandler
    {
        /// <summary>
        /// エラーメッセージを表示する
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        /// <param name="ex">例外オブジェクト（オプション）</param>
        public static void ShowError(string message, Exception? ex = null)
        {
            var errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
            MessageBox.Show(errorMessage, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        /// <summary>
        /// 警告メッセージを表示する
        /// </summary>
        /// <param name="message">警告メッセージ</param>
        public static void ShowWarning(string message)
        {
            MessageBox.Show(message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        /// <summary>
        /// 情報メッセージを表示する
        /// </summary>
        /// <param name="message">情報メッセージ</param>
        public static void ShowInfo(string message)
        {
            MessageBox.Show(message, "情報", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        /// <summary>
        /// 確認ダイアログを表示する
        /// </summary>
        /// <param name="message">確認メッセージ</param>
        /// <returns>ユーザーの選択結果</returns>
        public static bool ShowConfirmation(string message)
        {
            return MessageBox.Show(message, "確認", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }
    }
}
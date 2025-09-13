using System.Reflection;

namespace PomodoroTimer.Helpers
{
    /// <summary>
    /// アプリケーションバージョン情報を提供するヘルパークラス
    /// </summary>
    public static class AppVersionHelper
    {
        /// <summary>
        /// アプリケーションバージョンを取得する
        /// </summary>
        /// <returns>バージョン文字列（例：1.7.5）</returns>
        public static string GetVersion()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                return assembly.GetName().Version?.ToString(3) ?? "1.0.0";
            }
            catch
            {
                return "1.0.0";
            }
        }

        /// <summary>
        /// アプリケーション名を取得する
        /// </summary>
        /// <returns>アプリケーション名</returns>
        public static string GetProductName()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var productAttribute = assembly.GetCustomAttribute<AssemblyProductAttribute>();
                return productAttribute?.Product ?? "Pomoban";
            }
            catch
            {
                return "Pomoban";
            }
        }

        /// <summary>
        /// アプリケーション説明を取得する
        /// </summary>
        /// <returns>アプリケーション説明</returns>
        public static string GetDescription()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var descriptionAttribute = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
                return descriptionAttribute?.Description ?? "効率的なタスク管理とポモドーロテクニックを組み合わせたタイマーアプリケーション";
            }
            catch
            {
                return "効率的なタスク管理とポモドーロテクニックを組み合わせたタイマーアプリケーション";
            }
        }

        /// <summary>
        /// ウィンドウタイトル用の文字列を取得する
        /// </summary>
        /// <param name="suffix">タイトルに追加する接尾辞（オプション）</param>
        /// <returns>ウィンドウタイトル文字列</returns>
        public static string GetWindowTitle(string suffix = "")
        {
            return $"Pomoban v{GetVersion()}{suffix}";
        }
    }
}
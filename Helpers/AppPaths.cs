using System;
using System.IO;

namespace PomodoroTimer.Helpers
{
    /// <summary>
    /// アプリケーションで使用するパスの統一管理クラス
    /// </summary>
    public static class AppPaths
    {
        /// <summary>
        /// アプリケーションデータディレクトリのパス
        /// </summary>
        public static string AppDataDirectory => 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PomodoroTimer");
        
        /// <summary>
        /// 添付ファイル保存用ディレクトリのパス
        /// </summary>
        public static string AttachmentDirectory => 
            Path.Combine(AppDataDirectory, "Attachments");
        
        /// <summary>
        /// タスクデータファイルのパス
        /// </summary>
        public static string TasksFilePath => 
            Path.Combine(AppDataDirectory, "tasks.json");
        
        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        public static string SettingsFilePath => 
            Path.Combine(AppDataDirectory, "settings.json");
        
        /// <summary>
        /// 統計データファイルのパス
        /// </summary>
        public static string StatisticsFilePath => 
            Path.Combine(AppDataDirectory, "statistics.json");
        
        /// <summary>
        /// テンプレートファイルのパス
        /// </summary>
        public static string TemplatesFilePath => 
            Path.Combine(AppDataDirectory, "templates.json");
        
        /// <summary>
        /// 指定されたファイル名でアプリケーションデータディレクトリ内のパスを取得
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>フルパス</returns>
        public static string GetDataFilePath(string fileName) => 
            Path.Combine(AppDataDirectory, fileName);
        
        /// <summary>
        /// アプリケーションデータディレクトリが存在しない場合は作成する
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(AppDataDirectory))
            {
                Directory.CreateDirectory(AppDataDirectory);
            }
            
            if (!Directory.Exists(AttachmentDirectory))
            {
                Directory.CreateDirectory(AttachmentDirectory);
            }
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace PomodoroTimer.Models
{
    /// <summary>
    /// ホットキー（ショートカット）設定
    /// </summary>
    public partial class HotkeySettings : ObservableObject
    {
        /// <summary>
        /// 開始/一時停止のショートカットキー
        /// </summary>
        [ObservableProperty]
        private string startPauseHotkey = "Ctrl+Space";

        /// <summary>
        /// 停止のショートカットキー
        /// </summary>
        [ObservableProperty]
        private string stopHotkey = "Ctrl+Shift+S";

        /// <summary>
        /// スキップのショートカットキー
        /// </summary>
        [ObservableProperty]
        private string skipHotkey = "Ctrl+Shift+N";

        /// <summary>
        /// 新しいタスク追加のショートカットキー
        /// </summary>
        [ObservableProperty]
        private string addTaskHotkey = "Ctrl+N";

        /// <summary>
        /// 設定を開くショートカットキー
        /// </summary>
        [ObservableProperty]
        private string openSettingsHotkey = "Ctrl+,";

        /// <summary>
        /// 統計を開くショートカットキー
        /// </summary>
        [ObservableProperty]
        private string openStatisticsHotkey = "Ctrl+R";

        /// <summary>
        /// 集中モードのショートカットキー
        /// </summary>
        [ObservableProperty]
        private string focusModeHotkey = "F11";

        /// <summary>
        /// クイックタスク追加のショートカットキー
        /// </summary>
        [ObservableProperty]
        private string quickAddTaskHotkey = "Ctrl+Shift+Q";

        /// <summary>
        /// AI分析データエクスポートのショートカットキー
        /// </summary>
        [ObservableProperty]
        private string exportAIAnalysisHotkey = "";
    }

    /// <summary>
    /// ホットキー項目
    /// </summary>
    public partial class HotkeyItem : ObservableObject
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// 説明
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// ホットキー文字列
        /// </summary>
        [ObservableProperty]
        private string hotkey = "";

        /// <summary>
        /// プロパティ名（設定保存用）
        /// </summary>
        public string PropertyName { get; set; } = "";

        /// <summary>
        /// 有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;
    }
}
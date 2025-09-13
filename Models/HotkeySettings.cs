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

        // 各ホットキーの有効/無効設定
        /// <summary>
        /// 開始/一時停止ショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool startPauseHotkeyEnabled = true;

        /// <summary>
        /// 停止ショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool stopHotkeyEnabled = true;

        /// <summary>
        /// スキップショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool skipHotkeyEnabled = true;

        /// <summary>
        /// 新しいタスク追加ショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool addTaskHotkeyEnabled = true;

        /// <summary>
        /// 設定を開くショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool openSettingsHotkeyEnabled = true;

        /// <summary>
        /// 統計を開くショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool openStatisticsHotkeyEnabled = true;

        /// <summary>
        /// 集中モードショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool focusModeHotkeyEnabled = true;

        /// <summary>
        /// クイックタスク追加ショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool quickAddTaskHotkeyEnabled = true;

        /// <summary>
        /// AI分析データエクスポートショートカットキーが有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool exportAIAnalysisHotkeyEnabled = false;

        // 各ホットキーの非アクティブ時動作設定
        /// <summary>
        /// 開始/一時停止ショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool startPauseHotkeyGlobal = false;

        /// <summary>
        /// 停止ショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool stopHotkeyGlobal = false;

        /// <summary>
        /// スキップショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool skipHotkeyGlobal = false;

        /// <summary>
        /// 新しいタスク追加ショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool addTaskHotkeyGlobal = false;

        /// <summary>
        /// 設定を開くショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool openSettingsHotkeyGlobal = false;

        /// <summary>
        /// 統計を開くショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool openStatisticsHotkeyGlobal = false;

        /// <summary>
        /// 集中モードショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool focusModeHotkeyGlobal = false;

        /// <summary>
        /// クイックタスク追加ショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool quickAddTaskHotkeyGlobal = false;

        /// <summary>
        /// AI分析データエクスポートショートカットキーを非アクティブ時も有効にするかどうか
        /// </summary>
        [ObservableProperty]
        private bool exportAIAnalysisHotkeyGlobal = false;
    }

    /// <summary>
    /// ホットキー項目
    /// </summary>
    public partial class HotkeyItem : ObservableObject
    {
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName
        {
            get => displayName;
            set => SetProperty(ref displayName, value);
        }
        private string displayName = "";

        /// <summary>
        /// 説明
        /// </summary>
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        private string description = "";

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
        /// 有効フラグのプロパティ名（設定保存用）
        /// </summary>
        public string EnabledPropertyName { get; set; } = "";

        /// <summary>
        /// 有効かどうか
        /// </summary>
        [ObservableProperty]
        private bool isEnabled = true;

        /// <summary>
        /// グローバル（非アクティブ時も有効）かどうか
        /// </summary>
        [ObservableProperty]
        private bool isGlobal = false;

        /// <summary>
        /// グローバル設定のプロパティ名（設定保存用）
        /// </summary>
        public string GlobalPropertyName { get; set; } = "";
    }
}
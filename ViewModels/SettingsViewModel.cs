using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Models;
using System.Collections.ObjectModel;
using System.Reflection;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// 設定画面のビューモデル
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly AppSettings _settings;

        /// <summary>
        /// ホットキー項目一覧
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<HotkeyItem> hotkeyItems = new();

        /// <summary>
        /// 設定データへの参照
        /// </summary>
        public AppSettings Settings => _settings;

        // タイマー設定プロパティ
        public int WorkSessionMinutes
        {
            get => _settings.WorkSessionMinutes;
            set => SetProperty(_settings.WorkSessionMinutes, value, _settings, (s, v) => s.WorkSessionMinutes = v);
        }

        public int ShortBreakMinutes
        {
            get => _settings.ShortBreakMinutes;
            set => SetProperty(_settings.ShortBreakMinutes, value, _settings, (s, v) => s.ShortBreakMinutes = v);
        }

        public int LongBreakMinutes
        {
            get => _settings.LongBreakMinutes;
            set => SetProperty(_settings.LongBreakMinutes, value, _settings, (s, v) => s.LongBreakMinutes = v);
        }

        public int LongBreakInterval
        {
            get => _settings.LongBreakInterval;
            set => SetProperty(_settings.LongBreakInterval, value, _settings, (s, v) => s.LongBreakInterval = v);
        }

        public bool ShowNotifications
        {
            get => _settings.ShowNotifications;
            set => SetProperty(_settings.ShowNotifications, value, _settings, (s, v) => s.ShowNotifications = v);
        }

        public bool TopmostNotification
        {
            get => _settings.TopmostNotification;
            set => SetProperty(_settings.TopmostNotification, value, _settings, (s, v) => s.TopmostNotification = v);
        }

        public bool MinimizeToTray
        {
            get => _settings.MinimizeToTray;
            set => SetProperty(_settings.MinimizeToTray, value, _settings, (s, v) => s.MinimizeToTray = v);
        }

        public bool AutoStartNextSession
        {
            get => _settings.AutoStartNextSession;
            set => SetProperty(_settings.AutoStartNextSession, value, _settings, (s, v) => s.AutoStartNextSession = v);
        }

        public int QuickTaskDefaultMinutes
        {
            get => _settings.QuickTaskDefaultMinutes;
            set => SetProperty(_settings.QuickTaskDefaultMinutes, value, _settings, (s, v) => s.QuickTaskDefaultMinutes = v);
        }

        public SettingsViewModel(AppSettings settings)
        {
            _settings = settings;
            InitializeHotkeyItems();
        }

        /// <summary>
        /// ホットキー項目を初期化
        /// </summary>
        private void InitializeHotkeyItems()
        {
            HotkeyItems.Clear();

            var items = new List<HotkeyItem>
            {
                new HotkeyItem
                {
                    DisplayName = "開始/一時停止",
                    Description = "タイマーの開始と一時停止を切り替えます",
                    Hotkey = _settings.HotkeySettings.StartPauseHotkey,
                    PropertyName = nameof(HotkeySettings.StartPauseHotkey),
                    IsEnabled = _settings.HotkeySettings.StartPauseHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.StartPauseHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.StartPauseHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.StartPauseHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "停止",
                    Description = "タイマーを停止してセッションをリセットします",
                    Hotkey = _settings.HotkeySettings.StopHotkey,
                    PropertyName = nameof(HotkeySettings.StopHotkey),
                    IsEnabled = _settings.HotkeySettings.StopHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.StopHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.StopHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.StopHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "スキップ",
                    Description = "現在のセッションをスキップして次に進みます",
                    Hotkey = _settings.HotkeySettings.SkipHotkey,
                    PropertyName = nameof(HotkeySettings.SkipHotkey),
                    IsEnabled = _settings.HotkeySettings.SkipHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.SkipHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.SkipHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.SkipHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "新しいタスク",
                    Description = "新しいタスクを追加するダイアログを開きます",
                    Hotkey = _settings.HotkeySettings.AddTaskHotkey,
                    PropertyName = nameof(HotkeySettings.AddTaskHotkey),
                    IsEnabled = _settings.HotkeySettings.AddTaskHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.AddTaskHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.AddTaskHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.AddTaskHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "設定",
                    Description = "設定ダイアログを開きます",
                    Hotkey = _settings.HotkeySettings.OpenSettingsHotkey,
                    PropertyName = nameof(HotkeySettings.OpenSettingsHotkey),
                    IsEnabled = _settings.HotkeySettings.OpenSettingsHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.OpenSettingsHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.OpenSettingsHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.OpenSettingsHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "統計",
                    Description = "統計ダイアログを開きます",
                    Hotkey = _settings.HotkeySettings.OpenStatisticsHotkey,
                    PropertyName = nameof(HotkeySettings.OpenStatisticsHotkey),
                    IsEnabled = _settings.HotkeySettings.OpenStatisticsHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.OpenStatisticsHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.OpenStatisticsHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.OpenStatisticsHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "集中モード",
                    Description = "集中モードウィンドウを開きます",
                    Hotkey = _settings.HotkeySettings.FocusModeHotkey,
                    PropertyName = nameof(HotkeySettings.FocusModeHotkey),
                    IsEnabled = _settings.HotkeySettings.FocusModeHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.FocusModeHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.FocusModeHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.FocusModeHotkeyGlobal)
                },
                new HotkeyItem
                {
                    DisplayName = "クイックタスク追加",
                    Description = "クイックタスク追加ダイアログを開きます",
                    Hotkey = _settings.HotkeySettings.QuickAddTaskHotkey,
                    PropertyName = nameof(HotkeySettings.QuickAddTaskHotkey),
                    IsEnabled = _settings.HotkeySettings.QuickAddTaskHotkeyEnabled,
                    EnabledPropertyName = nameof(HotkeySettings.QuickAddTaskHotkeyEnabled),
                    IsGlobal = _settings.HotkeySettings.QuickAddTaskHotkeyGlobal,
                    GlobalPropertyName = nameof(HotkeySettings.QuickAddTaskHotkeyGlobal)
                }
            };

            foreach (var item in items)
            {
                HotkeyItems.Add(item);
            }

            // プロパティ変更イベントを監視
            foreach (var item in HotkeyItems)
            {
                item.PropertyChanged += HotkeyItem_PropertyChanged;
            }
        }

        /// <summary>
        /// ホットキー項目のプロパティ変更処理
        /// </summary>
        private void HotkeyItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is HotkeyItem item)
            {
                if (e.PropertyName == nameof(HotkeyItem.Hotkey))
                {
                    // ホットキー設定を更新
                    UpdateHotkeySetting(item);
                }
                else if (e.PropertyName == nameof(HotkeyItem.IsEnabled))
                {
                    // 有効/無効設定を更新
                    UpdateHotkeyEnabledSetting(item);
                }
                else if (e.PropertyName == nameof(HotkeyItem.IsGlobal))
                {
                    // グローバル設定を更新
                    UpdateHotkeyGlobalSetting(item);
                }
            }
        }

        /// <summary>
        /// ホットキー設定を更新
        /// </summary>
        /// <param name="item">ホットキー項目</param>
        private void UpdateHotkeySetting(HotkeyItem item)
        {
            try
            {
                var property = typeof(HotkeySettings).GetProperty(item.PropertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_settings.HotkeySettings, item.Hotkey);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ホットキー設定更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ホットキーの有効/無効設定を更新
        /// </summary>
        /// <param name="item">ホットキー項目</param>
        private void UpdateHotkeyEnabledSetting(HotkeyItem item)
        {
            try
            {
                var property = typeof(HotkeySettings).GetProperty(item.EnabledPropertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_settings.HotkeySettings, item.IsEnabled);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ホットキー有効設定更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ホットキーのグローバル設定を更新
        /// </summary>
        /// <param name="item">ホットキー項目</param>
        private void UpdateHotkeyGlobalSetting(HotkeyItem item)
        {
            try
            {
                var property = typeof(HotkeySettings).GetProperty(item.GlobalPropertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_settings.HotkeySettings, item.IsGlobal);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ホットキーグローバル設定更新エラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ホットキーをデフォルトに戻す
        /// </summary>
        public void ResetHotkeysToDefault()
        {
            var defaultSettings = new HotkeySettings();
            
            foreach (var item in HotkeyItems)
            {
                try
                {
                    // ホットキー文字列をデフォルトに戻す
                    var property = typeof(HotkeySettings).GetProperty(item.PropertyName);
                    if (property != null && property.CanRead)
                    {
                        var defaultValue = property.GetValue(defaultSettings) as string ?? "";
                        item.Hotkey = defaultValue;
                    }

                    // 有効フラグをデフォルトに戻す
                    var enabledProperty = typeof(HotkeySettings).GetProperty(item.EnabledPropertyName);
                    if (enabledProperty != null && enabledProperty.CanRead)
                    {
                        var defaultEnabled = (bool)(enabledProperty.GetValue(defaultSettings) ?? true);
                        item.IsEnabled = defaultEnabled;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"デフォルト値取得エラー: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// ホットキーが重複していないかチェック
        /// </summary>
        /// <returns>重複がある場合はfalse</returns>
        public bool ValidateHotkeys()
        {
            var hotkeys = HotkeyItems
                .Where(item => item.IsEnabled && !string.IsNullOrEmpty(item.Hotkey))
                .Select(item => item.Hotkey.Trim())
                .ToList();

            return hotkeys.Count == hotkeys.Distinct().Count();
        }

        /// <summary>
        /// 無効なホットキーをチェック
        /// </summary>
        /// <returns>無効なホットキーの一覧</returns>
        public List<HotkeyItem> GetInvalidHotkeys()
        {
            return HotkeyItems
                .Where(item => item.IsEnabled && 
                              !string.IsNullOrEmpty(item.Hotkey) && 
                              !Services.HotkeyService.IsValidHotkey(item.Hotkey))
                .ToList();
        }
    }
}
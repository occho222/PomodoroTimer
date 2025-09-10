using PomodoroTimer.Models;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// ホットキー管理サービス
    /// </summary>
    public class HotkeyService : IDisposable
    {
        // Win32 API
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // モディファイアキー
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        // WM_HOTKEY メッセージ
        private const int WM_HOTKEY = 0x0312;

        private readonly Dictionary<int, Action> _hotkeyActions = new();
        private readonly Dictionary<string, int> _hotkeyIds = new();
        private IntPtr _windowHandle;
        private HwndSource? _hwndSource;
        private int _nextHotkeyId = 1;
        private bool _disposed = false;

        public event EventHandler<string>? HotkeyPressed;

        /// <summary>
        /// ホットキーサービスを初期化
        /// </summary>
        /// <param name="window">メインウィンドウ</param>
        public void Initialize(Window window)
        {
            try
            {
                _windowHandle = new WindowInteropHelper(window).Handle;
                
                if (_windowHandle == IntPtr.Zero)
                {
                    throw new InvalidOperationException("ウィンドウハンドルの取得に失敗しました。ウィンドウがまだ作成されていない可能性があります。");
                }
                
                _hwndSource = HwndSource.FromHwnd(_windowHandle);
                if (_hwndSource == null)
                {
                    throw new InvalidOperationException("HwndSourceの作成に失敗しました。");
                }
                
                _hwndSource.AddHook(HotkeyHook);
                System.Diagnostics.Debug.WriteLine("ホットキーサービスが正常に初期化されました");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ホットキーサービス初期化エラー: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// ホットキーを登録
        /// </summary>
        /// <param name="name">ホットキー名</param>
        /// <param name="hotkey">ホットキー文字列（例: "Ctrl+Space"）</param>
        /// <param name="action">実行するアクション</param>
        public bool RegisterHotkey(string name, string hotkey, Action action)
        {
            try
            {
                // 既存のホットキーを削除
                if (_hotkeyIds.ContainsKey(name))
                {
                    UnregisterHotkey(name);
                }

                var (modifiers, key) = ParseHotkey(hotkey);
                System.Diagnostics.Debug.WriteLine($"ホットキー解析: {hotkey} -> modifiers=0x{modifiers:X}, key=0x{key:X}");
                
                if (key == 0) 
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録失敗: 無効なキー '{hotkey}'");
                    return false;
                }

                var hotkeyId = _nextHotkeyId++;
                
                if (RegisterHotKey(_windowHandle, hotkeyId, modifiers, key))
                {
                    _hotkeyIds[name] = hotkeyId;
                    _hotkeyActions[hotkeyId] = action;
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録成功: {name} -> {hotkey} (ID: {hotkeyId})");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録失敗: {name} -> {hotkey} - システムによる拒否");
                    var lastError = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"Win32エラーコード: {lastError}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ホットキー登録エラー: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// ホットキーを削除
        /// </summary>
        /// <param name="name">ホットキー名</param>
        public void UnregisterHotkey(string name)
        {
            if (_hotkeyIds.TryGetValue(name, out var hotkeyId))
            {
                UnregisterHotKey(_windowHandle, hotkeyId);
                _hotkeyIds.Remove(name);
                _hotkeyActions.Remove(hotkeyId);
            }
        }

        /// <summary>
        /// すべてのホットキーを削除
        /// </summary>
        public void UnregisterAllHotkeys()
        {
            foreach (var hotkeyId in _hotkeyActions.Keys.ToList())
            {
                UnregisterHotKey(_windowHandle, hotkeyId);
            }
            _hotkeyActions.Clear();
            _hotkeyIds.Clear();
        }

        /// <summary>
        /// ホットキー設定から一括登録（個別グローバル設定対応）
        /// </summary>
        /// <param name="hotkeySettings">ホットキー設定</param>
        /// <param name="actions">アクション辞書</param>
        public void RegisterHotkeysFromSettings(HotkeySettings hotkeySettings, Dictionary<string, Action> actions)
        {
            var hotkeyMappings = new Dictionary<string, (string hotkey, bool enabled, bool global)>
            {
                { "StartPause", (hotkeySettings.StartPauseHotkey, hotkeySettings.StartPauseHotkeyEnabled, hotkeySettings.StartPauseHotkeyGlobal) },
                { "Stop", (hotkeySettings.StopHotkey, hotkeySettings.StopHotkeyEnabled, hotkeySettings.StopHotkeyGlobal) },
                { "Skip", (hotkeySettings.SkipHotkey, hotkeySettings.SkipHotkeyEnabled, hotkeySettings.SkipHotkeyGlobal) },
                { "AddTask", (hotkeySettings.AddTaskHotkey, hotkeySettings.AddTaskHotkeyEnabled, hotkeySettings.AddTaskHotkeyGlobal) },
                { "OpenSettings", (hotkeySettings.OpenSettingsHotkey, hotkeySettings.OpenSettingsHotkeyEnabled, hotkeySettings.OpenSettingsHotkeyGlobal) },
                { "OpenStatistics", (hotkeySettings.OpenStatisticsHotkey, hotkeySettings.OpenStatisticsHotkeyEnabled, hotkeySettings.OpenStatisticsHotkeyGlobal) },
                { "FocusMode", (hotkeySettings.FocusModeHotkey, hotkeySettings.FocusModeHotkeyEnabled, hotkeySettings.FocusModeHotkeyGlobal) },
                { "QuickAddTask", (hotkeySettings.QuickAddTaskHotkey, hotkeySettings.QuickAddTaskHotkeyEnabled, hotkeySettings.QuickAddTaskHotkeyGlobal) }
            };

            foreach (var mapping in hotkeyMappings)
            {
                // 無効化されているホットキーはスキップ
                if (!mapping.Value.enabled)
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録スキップ: {mapping.Key} -> (無効)");
                    continue;
                }

                // 空の文字列やnullのホットキーはスキップ
                if (string.IsNullOrWhiteSpace(mapping.Value.hotkey))
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録スキップ: {mapping.Key} -> (空文字列)");
                    continue;
                }

                // グローバル設定がfalseの場合はスキップ（ローカルホットキーのみ使用）
                if (!mapping.Value.global)
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキー登録スキップ: {mapping.Key} -> (ローカル設定)");
                    continue;
                }

                if (actions.TryGetValue(mapping.Key, out var action))
                {
                    var success = RegisterHotkey(mapping.Key, mapping.Value.hotkey, action);
                    System.Diagnostics.Debug.WriteLine($"グローバルホットキー登録: {mapping.Key} -> {mapping.Value.hotkey} : {(success ? "成功" : "失敗")}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ホットキーアクションが見つかりません: {mapping.Key}");
                }
            }
        }

        /// <summary>
        /// ホットキー文字列をパース
        /// </summary>
        /// <param name="hotkey">ホットキー文字列</param>
        /// <returns>モディファイアとキーコード</returns>
        private (uint modifiers, uint key) ParseHotkey(string hotkey)
        {
            if (string.IsNullOrEmpty(hotkey)) return (0, 0);

            var parts = hotkey.Split('+');
            if (parts.Length == 0) return (0, 0);

            uint modifiers = 0;
            var keyString = parts[^1]; // 最後の要素がキー

            // モディファイアキーをチェック
            foreach (var part in parts[..^1])
            {
                modifiers |= part.Trim().ToLower() switch
                {
                    "ctrl" or "control" => MOD_CONTROL,
                    "alt" => MOD_ALT,
                    "shift" => MOD_SHIFT,
                    "win" or "windows" => MOD_WIN,
                    _ => 0
                };
            }

            // キーコードを取得
            var key = GetKeyCode(keyString.Trim());
            return (modifiers, key);
        }

        /// <summary>
        /// キー名からキーコードを取得
        /// </summary>
        /// <param name="keyName">キー名</param>
        /// <returns>キーコード</returns>
        private uint GetKeyCode(string keyName)
        {
            return keyName.ToUpper() switch
            {
                "SPACE" => 0x20,
                "ENTER" => 0x0D,
                "ESCAPE" or "ESC" => 0x1B,
                "TAB" => 0x09,
                "BACKSPACE" => 0x08,
                "DELETE" or "DEL" => 0x2E,
                "INSERT" or "INS" => 0x2D,
                "HOME" => 0x24,
                "END" => 0x23,
                "PAGEUP" or "PGUP" => 0x21,
                "PAGEDOWN" or "PGDN" => 0x22,
                "UP" => 0x26,
                "DOWN" => 0x28,
                "LEFT" => 0x25,
                "RIGHT" => 0x27,
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                _ when keyName.Length == 1 && char.IsLetterOrDigit(keyName[0]) => (uint)keyName[0],
                _ => 0
            };
        }

        /// <summary>
        /// ホットキーメッセージハンドラー
        /// </summary>
        private IntPtr HotkeyHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                var hotkeyId = wParam.ToInt32();
                if (_hotkeyActions.TryGetValue(hotkeyId, out var action))
                {
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
                        handled = true;

                        // イベント発火
                        var hotkeyName = _hotkeyIds.FirstOrDefault(x => x.Value == hotkeyId).Key;
                        if (!string.IsNullOrEmpty(hotkeyName))
                        {
                            HotkeyPressed?.Invoke(this, hotkeyName);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"ホットキーアクション実行エラー: {ex.Message}");
                    }
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// ホットキー文字列の妥当性チェック
        /// </summary>
        /// <param name="hotkey">ホットキー文字列</param>
        /// <returns>妥当かどうか</returns>
        public static bool IsValidHotkey(string hotkey)
        {
            if (string.IsNullOrWhiteSpace(hotkey)) return false;

            var parts = hotkey.Split('+');
            if (parts.Length == 0) return false;

            var keyString = parts[^1].Trim();
            return !string.IsNullOrWhiteSpace(keyString);
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                UnregisterAllHotkeys();
                _hwndSource?.RemoveHook(HotkeyHook);
                _disposed = true;
            }
        }
    }
}
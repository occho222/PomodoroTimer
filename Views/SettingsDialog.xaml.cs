using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Models;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 設定ダイアログ
    /// </summary>
    public partial class SettingsDialog : Window
    {
        /// <summary>
        /// 設定データ
        /// </summary>
        public AppSettings Settings { get; private set; }

        public SettingsDialog(AppSettings currentSettings)
        {
            InitializeComponent();
            
            // 現在の設定をコピーして編集用ViewModelを作成
            var viewModel = new SettingsViewModel
            {
                WorkSessionMinutes = currentSettings.WorkSessionMinutes,
                ShortBreakMinutes = currentSettings.ShortBreakMinutes,
                LongBreakMinutes = currentSettings.LongBreakMinutes,
                PomodorosBeforeLongBreak = currentSettings.PomodorosBeforeLongBreak,
                EnableSoundNotification = currentSettings.EnableSoundNotification,
                EnableDesktopNotification = currentSettings.EnableDesktopNotification,
                AutoStartNextSession = currentSettings.AutoStartNextSession,
                UseDarkTheme = currentSettings.UseDarkTheme,
                SaveStatistics = currentSettings.SaveStatistics,
                MinimizeToTray = currentSettings.MinimizeToTray
            };

            DataContext = viewModel;
            Settings = currentSettings;
        }

        /// <summary>
        /// OK ボタンがクリックされた時の処理
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (SettingsViewModel)DataContext;
            
            // 入力値の検証
            if (!ValidateSettings(viewModel))
                return;

            // 設定を更新
            Settings = new AppSettings
            {
                WorkSessionMinutes = viewModel.WorkSessionMinutes,
                ShortBreakMinutes = viewModel.ShortBreakMinutes,
                LongBreakMinutes = viewModel.LongBreakMinutes,
                PomodorosBeforeLongBreak = viewModel.PomodorosBeforeLongBreak,
                EnableSoundNotification = viewModel.EnableSoundNotification,
                EnableDesktopNotification = viewModel.EnableDesktopNotification,
                AutoStartNextSession = viewModel.AutoStartNextSession,
                UseDarkTheme = viewModel.UseDarkTheme,
                SaveStatistics = viewModel.SaveStatistics,
                MinimizeToTray = viewModel.MinimizeToTray
            };

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセル ボタンがクリックされた時の処理
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// リセット ボタンがクリックされた時の処理
        /// </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "すべての設定をデフォルト値にリセットしますか？",
                "設定リセット",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var defaultSettings = new AppSettings();
                var viewModel = (SettingsViewModel)DataContext;
                
                viewModel.WorkSessionMinutes = defaultSettings.WorkSessionMinutes;
                viewModel.ShortBreakMinutes = defaultSettings.ShortBreakMinutes;
                viewModel.LongBreakMinutes = defaultSettings.LongBreakMinutes;
                viewModel.PomodorosBeforeLongBreak = defaultSettings.PomodorosBeforeLongBreak;
                viewModel.EnableSoundNotification = defaultSettings.EnableSoundNotification;
                viewModel.EnableDesktopNotification = defaultSettings.EnableDesktopNotification;
                viewModel.AutoStartNextSession = defaultSettings.AutoStartNextSession;
                viewModel.UseDarkTheme = defaultSettings.UseDarkTheme;
                viewModel.SaveStatistics = defaultSettings.SaveStatistics;
                viewModel.MinimizeToTray = defaultSettings.MinimizeToTray;
            }
        }

        /// <summary>
        /// 設定値を検証する
        /// </summary>
        private bool ValidateSettings(SettingsViewModel viewModel)
        {
            // 作業時間の検証（1-120分）
            if (viewModel.WorkSessionMinutes < 1 || viewModel.WorkSessionMinutes > 120)
            {
                System.Windows.MessageBox.Show("作業セッション時間は1分から120分の間で設定してください。", 
                    "設定エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 短休憩時間の検証（1-60分）
            if (viewModel.ShortBreakMinutes < 1 || viewModel.ShortBreakMinutes > 60)
            {
                System.Windows.MessageBox.Show("短い休憩時間は1分から60分の間で設定してください。", 
                    "設定エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 長休憩時間の検証（1-120分）
            if (viewModel.LongBreakMinutes < 1 || viewModel.LongBreakMinutes > 120)
            {
                System.Windows.MessageBox.Show("長い休憩時間は1分から120分の間で設定してください。", 
                    "設定エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // 長休憩前のポモドーロ数の検証（1-10）
            if (viewModel.PomodorosBeforeLongBreak < 1 || viewModel.PomodorosBeforeLongBreak > 10)
            {
                System.Windows.MessageBox.Show("長い休憩前のポモドーロ数は1から10の間で設定してください。", 
                    "設定エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// 設定ダイアログ用のViewModel
    /// </summary>
    public partial class SettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private int workSessionMinutes = 25;

        [ObservableProperty]
        private int shortBreakMinutes = 5;

        [ObservableProperty]
        private int longBreakMinutes = 15;

        [ObservableProperty]
        private int pomodorosBeforeLongBreak = 4;

        [ObservableProperty]
        private bool enableSoundNotification = true;

        [ObservableProperty]
        private bool enableDesktopNotification = true;

        [ObservableProperty]
        private bool autoStartNextSession = false;

        [ObservableProperty]
        private bool useDarkTheme = false;

        [ObservableProperty]
        private bool saveStatistics = true;

        [ObservableProperty]
        private bool minimizeToTray = false;
    }
}
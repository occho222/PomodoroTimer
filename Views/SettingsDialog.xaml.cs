using System.Windows;
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
            
            // 現在の設定をコピーして編集用にする
            Settings = new AppSettings
            {
                WorkSessionMinutes = currentSettings.WorkSessionMinutes,
                ShortBreakMinutes = currentSettings.ShortBreakMinutes,
                LongBreakMinutes = currentSettings.LongBreakMinutes,
                LongBreakInterval = currentSettings.LongBreakInterval,
                ShowNotifications = currentSettings.ShowNotifications,
                MinimizeToTray = currentSettings.MinimizeToTray,
                AutoStartNextSession = currentSettings.AutoStartNextSession
            };
            
            DataContext = Settings;
        }

        /// <summary>
        /// OKボタンクリック時の処理
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// キャンセルボタンクリック時の処理
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
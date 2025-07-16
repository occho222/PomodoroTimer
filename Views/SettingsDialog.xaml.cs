using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PomodoroTimer.Models;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// �ݒ�_�C�A���O
    /// </summary>
    public partial class SettingsDialog : Window
    {
        /// <summary>
        /// �ݒ�f�[�^
        /// </summary>
        public AppSettings Settings { get; private set; }

        public SettingsDialog(AppSettings currentSettings)
        {
            InitializeComponent();
            
            // ���݂̐ݒ���R�s�[���ĕҏW�pViewModel���쐬
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
        /// OK �{�^�����N���b�N���ꂽ���̏���
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (SettingsViewModel)DataContext;
            
            // ���͒l�̌���
            if (!ValidateSettings(viewModel))
                return;

            // �ݒ���X�V
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
        /// �L�����Z�� �{�^�����N���b�N���ꂽ���̏���
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// ���Z�b�g �{�^�����N���b�N���ꂽ���̏���
        /// </summary>
        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to reset all settings to default values?",
                "Reset Settings",
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
        /// �ݒ�l�����؂���
        /// </summary>
        private bool ValidateSettings(SettingsViewModel viewModel)
        {
            // ��Ǝ��Ԃ̌��؁i1-120���j
            if (viewModel.WorkSessionMinutes < 1 || viewModel.WorkSessionMinutes > 120)
            {
                MessageBox.Show("Work session must be between 1 and 120 minutes.", 
                    "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // �Z�x�e���Ԃ̌��؁i1-60���j
            if (viewModel.ShortBreakMinutes < 1 || viewModel.ShortBreakMinutes > 60)
            {
                MessageBox.Show("Short break must be between 1 and 60 minutes.", 
                    "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // ���x�e���Ԃ̌��؁i1-120���j
            if (viewModel.LongBreakMinutes < 1 || viewModel.LongBreakMinutes > 120)
            {
                MessageBox.Show("Long break must be between 1 and 120 minutes.", 
                    "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // ���x�e�O�̃|���h�[�����̌��؁i1-10�j
            if (viewModel.PomodorosBeforeLongBreak < 1 || viewModel.PomodorosBeforeLongBreak > 10)
            {
                MessageBox.Show("Pomodoros before long break must be between 1 and 10.", 
                    "Invalid Setting", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// �ݒ�_�C�A���O�p��ViewModel
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
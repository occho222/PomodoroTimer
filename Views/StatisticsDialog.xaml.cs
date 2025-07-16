using System.Windows;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 統計画面のダイアログ
    /// </summary>
    public partial class StatisticsDialog : Window
    {
        public StatisticsDialog(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel(statisticsService, pomodoroService);
            
            // ウィンドウサイズと位置の設定
            Width = 1000;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// 閉じるボタンのクリックイベント
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
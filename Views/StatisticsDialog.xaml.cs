using System.Windows;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// 統計ダイアログ
    /// </summary>
    public partial class StatisticsDialog : Window
    {
        public StatisticsDialog(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            InitializeComponent();
            
            // 統計ViewModelを作成してDataContextに設定
            var viewModel = new StatisticsViewModel(statisticsService, pomodoroService);
            DataContext = viewModel;
        }

        /// <summary>
        /// 閉じるボタンクリック時の処理
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
using System.Windows;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// ���v��ʂ̃_�C�A���O
    /// </summary>
    public partial class StatisticsDialog : Window
    {
        public StatisticsDialog(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            InitializeComponent();
            DataContext = new StatisticsViewModel(statisticsService, pomodoroService);
            
            // �E�B���h�E�T�C�Y�ƈʒu�̐ݒ�
            Width = 1000;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        /// <summary>
        /// ����{�^���̃N���b�N�C�x���g
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
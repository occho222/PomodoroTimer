using System.Windows;
using PomodoroTimer.Services;
using PomodoroTimer.ViewModels;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// ���v�_�C�A���O
    /// </summary>
    public partial class StatisticsDialog : Window
    {
        public StatisticsDialog(IStatisticsService statisticsService, IPomodoroService pomodoroService)
        {
            InitializeComponent();
            
            // ���vViewModel���쐬����DataContext�ɐݒ�
            var viewModel = new StatisticsViewModel(statisticsService, pomodoroService);
            DataContext = viewModel;
        }

        /// <summary>
        /// ����{�^���N���b�N���̏���
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
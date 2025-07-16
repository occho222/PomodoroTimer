using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// �^�X�N�ǉ��_�C�A���O
    /// </summary>
    public partial class TaskDialog : Window
    {
        /// <summary>
        /// �^�X�N�̃^�C�g��
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// �\��|���h�[����
        /// </summary>
        public int EstimatedPomodoros { get; set; } = 1;

        /// <summary>
        /// �^�X�N�̐���
        /// </summary>
        public string TaskDescription { get; set; } = string.Empty;

        public TaskDialog()
        {
            InitializeComponent();
            DataContext = new TaskDialogViewModel();
        }

        /// <summary>
        /// OK�{�^�����N���b�N���ꂽ���̏���
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (TaskDialogViewModel)DataContext;
            
            // ���͒l�̌���
            if (string.IsNullOrWhiteSpace(viewModel.TaskTitle))
            {
                MessageBox.Show("Please enter a task name.", "Input Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (viewModel.EstimatedPomodoros < 1 || viewModel.EstimatedPomodoros > 10)
            {
                MessageBox.Show("Estimated pomodoros must be between 1 and 10.", "Input Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // �v���p�e�B�ɒl��ݒ�
            TaskTitle = viewModel.TaskTitle;
            EstimatedPomodoros = viewModel.EstimatedPomodoros;
            TaskDescription = viewModel.TaskDescription;

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// �L�����Z���{�^�����N���b�N���ꂽ���̏���
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// TaskDialog�p��ViewModel
    /// </summary>
    public partial class TaskDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private int estimatedPomodoros = 1;

        [ObservableProperty]
        private string taskDescription = string.Empty;
    }
}
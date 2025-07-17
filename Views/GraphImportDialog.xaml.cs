using System.Windows;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// Microsoft Graph�C���|�[�g�_�C�A���O
    /// </summary>
    public partial class GraphImportDialog : Window
    {
        /// <summary>
        /// Microsoft To-Do����C���|�[�g���邩�ǂ���
        /// </summary>
        public bool ImportFromMicrosoftToDo => MicrosoftToDoCheckBox.IsChecked ?? false;

        /// <summary>
        /// Microsoft Planner����C���|�[�g���邩�ǂ���
        /// </summary>
        public bool ImportFromPlanner => PlannerCheckBox.IsChecked ?? false;

        /// <summary>
        /// Outlook����C���|�[�g���邩�ǂ���
        /// </summary>
        public bool ImportFromOutlook => OutlookCheckBox.IsChecked ?? false;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        public GraphImportDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// �C���|�[�g�{�^���N���b�N
        /// </summary>
        private void Import_Click(object sender, RoutedEventArgs e)
        {
            // ���Ȃ��Ƃ���̃\�[�X���I������Ă��邩�`�F�b�N
            if (!ImportFromMicrosoftToDo && !ImportFromPlanner && !ImportFromOutlook)
            {
                System.Windows.MessageBox.Show("���Ȃ��Ƃ���̃C���|�[�g����I�����Ă��������B", "�I���G���[", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        /// <summary>
        /// �L�����Z���{�^���N���b�N
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
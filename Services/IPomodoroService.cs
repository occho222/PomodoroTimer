using PomodoroTimer.Models;
using System.Collections.ObjectModel;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// �|���h�[���^�C�}�[�̃r�W�l�X���W�b�N���`����C���^�[�t�F�[�X
    /// </summary>
    public interface IPomodoroService
    {
        /// <summary>
        /// �^�X�N���X�g���擾����
        /// </summary>
        ObservableCollection<PomodoroTask> GetTasks();

        /// <summary>
        /// �^�X�N��ǉ�����
        /// </summary>
        /// <param name="task">�ǉ�����^�X�N</param>
        void AddTask(PomodoroTask task);

        /// <summary>
        /// �^�X�N���폜����
        /// </summary>
        /// <param name="task">�폜����^�X�N</param>
        void RemoveTask(PomodoroTask task);

        /// <summary>
        /// �^�X�N�̏�����ύX����
        /// </summary>
        /// <param name="sourceIndex">�ړ����̃C���f�b�N�X</param>
        /// <param name="targetIndex">�ړ���̃C���f�b�N�X</param>
        void ReorderTasks(int sourceIndex, int targetIndex);

        /// <summary>
        /// �^�X�N�������ɂ���
        /// </summary>
        /// <param name="task">��������^�X�N</param>
        void CompleteTask(PomodoroTask task);

        /// <summary>
        /// �^�X�N�̃|���h�[�����𑝉�����
        /// </summary>
        /// <param name="task">�Ώۂ̃^�X�N</param>
        void IncrementTaskPomodoro(PomodoroTask task);
    }
}
using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// Microsoft Graph API�T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface IGraphService
    {
        /// <summary>
        /// Microsoft Graph����F�؂��s��
        /// </summary>
        /// <returns>�F�ؐ����̉�</returns>
        Task<bool> AuthenticateAsync();

        /// <summary>
        /// Microsoft To Do ����^�X�N���C���|�[�g����
        /// </summary>
        /// <returns>�C���|�[�g���ꂽ�^�X�N�̃��X�g</returns>
        Task<List<PomodoroTask>> ImportTasksFromMicrosoftToDoAsync();

        /// <summary>
        /// Microsoft Planner����^�X�N���C���|�[�g����
        /// </summary>
        /// <returns>�C���|�[�g���ꂽ�^�X�N�̃��X�g</returns>
        Task<List<PomodoroTask>> ImportTasksFromPlannerAsync();

        /// <summary>
        /// Outlook�^�X�N����^�X�N���C���|�[�g����
        /// </summary>
        /// <returns>�C���|�[�g���ꂽ�^�X�N�̃��X�g</returns>
        Task<List<PomodoroTask>> ImportTasksFromOutlookAsync();

        /// <summary>
        /// ���݂̔F�؏�Ԃ��m�F����
        /// </summary>
        /// <returns>�F�؂���Ă��邩�ǂ���</returns>
        bool IsAuthenticated { get; }

        /// <summary>
        /// ���O�A�E�g����
        /// </summary>
        /// <returns>���O�A�E�g�����^�X�N</returns>
        Task LogoutAsync();
    }
}
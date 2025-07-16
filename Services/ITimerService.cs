namespace PomodoroTimer.Services
{
    /// <summary>
    /// �^�C�}�[�T�[�r�X�̃C���^�[�t�F�[�X
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// �^�C�}�[���J�n���ꂽ���ɔ�������C�x���g
        /// </summary>
        event Action? TimerStarted;

        /// <summary>
        /// �^�C�}�[����~���ꂽ���ɔ�������C�x���g
        /// </summary>
        event Action? TimerStopped;

        /// <summary>
        /// �^�C�}�[���ꎞ��~���ꂽ���ɔ�������C�x���g
        /// </summary>
        event Action? TimerPaused;

        /// <summary>
        /// �^�C�}�[���ĊJ���ꂽ���ɔ�������C�x���g
        /// </summary>
        event Action? TimerResumed;

        /// <summary>
        /// �^�C�}�[�̎��Ԃ��X�V���ꂽ���ɔ�������C�x���g
        /// </summary>
        event Action<TimeSpan>? TimeUpdated;

        /// <summary>
        /// �Z�b�V�����������������ɔ�������C�x���g
        /// </summary>
        event Action? SessionCompleted;

        /// <summary>
        /// �^�C�}�[�����s�����ǂ���
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// ���݂̎c�莞��
        /// </summary>
        TimeSpan RemainingTime { get; }

        /// <summary>
        /// �Z�b�V�����̍��v����
        /// </summary>
        TimeSpan SessionDuration { get; }

        /// <summary>
        /// �^�C�}�[���J�n����
        /// </summary>
        /// <param name="duration">�Z�b�V��������</param>
        void Start(TimeSpan duration);

        /// <summary>
        /// �^�C�}�[���~����
        /// </summary>
        void Stop();

        /// <summary>
        /// �^�C�}�[���ꎞ��~����
        /// </summary>
        void Pause();

        /// <summary>
        /// �^�C�}�[���ĊJ����
        /// </summary>
        void Resume();

        /// <summary>
        /// ���݂̃Z�b�V�������X�L�b�v����
        /// </summary>
        void Skip();
    }
}
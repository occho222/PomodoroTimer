using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using PomodoroTimer.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace PomodoroTimer.Views
{
    /// <summary>
    /// ���C���E�B���h�E�̃R�[�h�r�n�C���h
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private PomodoroTask? _draggedTask;

        /// <summary>
        /// DI�R���e�i���璍�������R���X�g���N�^
        /// </summary>
        /// <param name="viewModel">���C���r���[���f��</param>
        public MainWindow(MainViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = _viewModel;

            // �z�b�g�L�[�̓o�^
            RegisterHotKeys();
        }

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^�i�f�U�C�i�[�p�j
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            try
            {
                // DI�R���e�i����擾�����s
                var app = (App)Application.Current;
                _viewModel = app.Services.GetRequiredService<MainViewModel>();
            }
            catch
            {
                // DI�R���e�i���ݒ肳��Ă��Ȃ��ꍇ�̃t�H�[���o�b�N
                var pomodoroService = new PomodoroService();
                var timerService = new TimerService();
                _viewModel = new MainViewModel(pomodoroService, timerService);
            }
            
            DataContext = _viewModel;

            // �z�b�g�L�[�̓o�^
            RegisterHotKeys();
        }

        /// <summary>
        /// �z�b�g�L�[��o�^����
        /// </summary>
        private void RegisterHotKeys()
        {
            // Ctrl+Space: �J�n/�ꎞ��~
            var startPauseCommand = new RoutedCommand();
            startPauseCommand.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(startPauseCommand, (s, e) => _viewModel.StartPauseCommand.Execute(null)));

            // Ctrl+S: ��~
            var stopCommand = new RoutedCommand();
            stopCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(stopCommand, (s, e) => _viewModel.StopCommand.Execute(null)));

            // Ctrl+N: ���̃Z�b�V����
            var skipCommand = new RoutedCommand();
            skipCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(skipCommand, (s, e) => _viewModel.SkipCommand.Execute(null)));
        }

        #region �h���b�O&�h���b�v����

        /// <summary>
        /// �^�X�N�A�C�e�����}�E�X�ŃN���b�N���ꂽ���̏���
        /// </summary>
        private void TaskItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is PomodoroTask task)
            {
                _draggedTask = task;
                DragDrop.DoDragDrop(border, task, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// �^�X�N�A�C�e�����h���b�v���ꂽ���̏���
        /// </summary>
        private void TaskItem_Drop(object sender, DragEventArgs e)
        {
            if (sender is Border border && border.DataContext is PomodoroTask targetTask && _draggedTask != null)
            {
                _viewModel.ReorderTasks(_draggedTask, targetTask);
                _draggedTask = null;
            }
        }

        /// <summary>
        /// �h���b�O�I�[�o�[���̏���
        /// </summary>
        private void TaskItem_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(PomodoroTask)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        #endregion

        /// <summary>
        /// �E�B���h�E�������鎞�̏���
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.SaveSettings();
        }
    }
}
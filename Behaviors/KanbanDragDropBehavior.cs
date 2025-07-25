using Microsoft.Xaml.Behaviors;
using PomodoroTimer.Models;
using PomodoroTimer.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TaskStatus = PomodoroTimer.Models.TaskStatus;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using DragEventArgs = System.Windows.DragEventArgs;
using Point = System.Windows.Point;
using DataObject = System.Windows.DataObject;
using DragDropEffects = System.Windows.DragDropEffects;
using Brushes = System.Windows.Media.Brushes;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;

namespace PomodoroTimer.Behaviors
{
    /// <summary>
    /// カンバンボード用ドラッグアンドドロップ動作
    /// </summary>
    public class KanbanDragDropBehavior : Behavior<Border>
    {
        private bool _isDragging = false;
        private Point _startPoint;
        private PomodoroTask? _draggedTask;

        public static readonly DependencyProperty TargetStatusProperty =
            DependencyProperty.Register(nameof(TargetStatus), typeof(TaskStatus), typeof(KanbanDragDropBehavior));

        public TaskStatus TargetStatus
        {
            get => (TaskStatus)GetValue(TargetStatusProperty);
            set => SetValue(TargetStatusProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.Drop += OnDrop;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.Drop -= OnDrop;
            base.OnDetaching();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _draggedTask = GetTaskFromElement(e.OriginalSource as FrameworkElement);
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
            {
                Point position = e.GetPosition(null);

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    StartDrag(e);
                }
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _draggedTask = null;
        }

        private void StartDrag(MouseEventArgs e)
        {
            if (_draggedTask == null) return;

            _isDragging = true;
            
            DataObject dragData = new DataObject("PomodoroTask", _draggedTask);
            DragDrop.DoDragDrop(AssociatedObject, dragData, DragDropEffects.Move);
            
            _isDragging = false;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("PomodoroTask"))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            // ドラッグエンター時の視覚的フィードバック
            AssociatedObject.Background = new SolidColorBrush(Color.FromArgb(50, 59, 130, 246)); // 半透明の青
            e.Effects = DragDropEffects.Move;
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("PomodoroTask"))
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            var task = e.Data.GetData("PomodoroTask") as PomodoroTask;
            if (task == null)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            // ドロップ可能かどうかをチェック
            if (CanDropTask(task, TargetStatus))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            // 視覚的フィードバックを元に戻す
            AssociatedObject.Background = Brushes.Transparent;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            // 視覚的フィードバックを元に戻す
            AssociatedObject.Background = Brushes.Transparent;

            if (!e.Data.GetDataPresent("PomodoroTask"))
            {
                return;
            }

            var task = e.Data.GetData("PomodoroTask") as PomodoroTask;
            if (task == null || !CanDropTask(task, TargetStatus))
            {
                return;
            }

            // MainViewModelを取得してタスクのステータスを変更
            var mainWindow = Window.GetWindow(AssociatedObject);
            var viewModel = mainWindow?.DataContext as MainViewModel;
            
            if (viewModel != null)
            {
                ChangeTaskStatus(task, TargetStatus, viewModel);
            }

            e.Handled = true;
        }

        private PomodoroTask? GetTaskFromElement(FrameworkElement? element)
        {
            while (element != null)
            {
                if (element.DataContext is PomodoroTask task)
                {
                    return task;
                }
                element = VisualTreeHelper.GetParent(element) as FrameworkElement;
            }
            return null;
        }

        private bool CanDropTask(PomodoroTask task, TaskStatus targetStatus)
        {
            // ドロップのルールを定義
            switch (targetStatus)
            {
                case TaskStatus.Todo:
                    // 待機中や完了からは戻せる
                    return task.Status == TaskStatus.Waiting || task.Status == TaskStatus.Completed;
                
                case TaskStatus.Waiting:
                    // 未開始や実行中から移動可能
                    return task.Status == TaskStatus.Todo || task.Status == TaskStatus.Executing;
                
                case TaskStatus.Executing:
                    // 待機中からのみ移動可能
                    return task.Status == TaskStatus.Waiting;
                
                case TaskStatus.Completed:
                    // 実行中や待機中からは完了にできる
                    return task.Status == TaskStatus.Executing || task.Status == TaskStatus.Waiting;
                
                default:
                    return false;
            }
        }

        private void ChangeTaskStatus(PomodoroTask task, TaskStatus newStatus, MainViewModel viewModel)
        {
            try
            {
                var oldStatus = task.Status;
                
                // 完了状態から他の状態に変更する場合は統計からundo
                if (oldStatus == TaskStatus.Completed && newStatus != TaskStatus.Completed)
                {
                    // StatisticsServiceにアクセスするためのプロパティがMainViewModelに必要
                    var undoMethod = viewModel.GetType().GetMethod("UndoTaskCompleteInStatistics", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    undoMethod?.Invoke(viewModel, new object[] { task });
                }
                
                // 実行中タスクを他の状態に移動する場合の特別処理
                if (oldStatus == TaskStatus.Executing && newStatus != TaskStatus.Executing)
                {
                    // CurrentTaskからクリア
                    if (viewModel.CurrentTask == task)
                    {
                        viewModel.CurrentTask = null;
                    }
                    
                    // タスクの実行状態をリセット
                    task.StopExecution();
                }
                
                // ステータス変更
                task.Status = newStatus;
                
                // ステータスに応じて他のプロパティも更新
                switch (newStatus)
                {
                    case TaskStatus.Todo:
                        task.StartedAt = null;
                        task.CompletedAt = null;
                        task.IsCompleted = false;
                        break;
                        
                    case TaskStatus.Waiting:
                        if (task.StartedAt == null)
                            task.StartedAt = DateTime.Now;
                        task.CompletedAt = null;
                        task.IsCompleted = false;
                        break;
                        
                    case TaskStatus.Executing:
                        if (task.StartedAt == null)
                            task.StartedAt = DateTime.Now;
                        task.CompletedAt = null;
                        task.IsCompleted = false;
                        
                        // 実行中タスクに設定
                        if (viewModel.CurrentTask != null && viewModel.CurrentTask != task)
                        {
                            // 既存の実行中タスクを待機中に戻す
                            viewModel.CurrentTask.StopExecution();
                        }
                        
                        // タスクを実行中に設定
                        task.StartExecution();
                        viewModel.CurrentTask = task;
                        
                        // セッション開始時刻を記録
                        task.CurrentSessionStartTime = DateTime.Now;
                        break;
                        
                    case TaskStatus.Completed:
                        if (task.StartedAt == null)
                            task.StartedAt = DateTime.Now;
                        task.CompletedAt = DateTime.Now;
                        task.IsCompleted = true;
                        
                        // 統計に記録
                        var recordMethod = viewModel.GetType().GetMethod("RecordTaskCompleteInStatistics", 
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        recordMethod?.Invoke(viewModel, new object[] { task });
                        
                        // 実行中タスクだった場合はクリア（既に上で処理済み）
                        break;
                }
                
                // UIを更新
                Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    viewModel.UpdateKanbanColumns();
                    
                    // 実行中に移動した場合はリアルタイム更新も開始
                    if (newStatus == TaskStatus.Executing)
                    {
                        viewModel.NotifyTaskExecutionStarted();
                    }
                    
                    // データを保存
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await viewModel.SaveTasksAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"タスクの保存に失敗しました: {ex.Message}");
                        }
                    });
                });

                Console.WriteLine($"タスク「{task.Title}」のステータスを {oldStatus} から {newStatus} に変更しました");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ドラッグアンドドロップでのステータス変更に失敗しました: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// タスクカード用ドラッグアンドドロップ動作
/// </summary>
public class TaskCardDragBehavior : Behavior<Border>
{
    private bool _isDragging = false;
    private Point _startPoint;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
        base.OnDetaching();
    }

    private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _startPoint = e.GetPosition(null);
    }

    private void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && !_isDragging)
        {
            Point position = e.GetPosition(null);

            if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                StartDrag(e);
            }
        }
    }

    private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _isDragging = false;
    }

    private void StartDrag(MouseEventArgs e)
    {
        var task = AssociatedObject.DataContext as PomodoroTask;
        if (task == null) return;

        _isDragging = true;
        
        // ドラッグ中の視覚的フィードバック
        AssociatedObject.Opacity = 0.7;
        
        DataObject dragData = new DataObject("PomodoroTask", task);
        var result = DragDrop.DoDragDrop(AssociatedObject, dragData, DragDropEffects.Move);
        
        // ドラッグ終了後に元に戻す
        AssociatedObject.Opacity = 1.0;
        _isDragging = false;
    }
}
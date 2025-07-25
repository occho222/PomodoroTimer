using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using Clipboard = System.Windows.Clipboard;
using TaskStatus = PomodoroTimer.Models.TaskStatus;

namespace PomodoroTimer.ViewModels
{
    public partial class TaskDetailDialogViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;
        private readonly PomodoroTask _originalTask;
        private readonly bool _isEditMode;

        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string taskCategory = string.Empty;

        [ObservableProperty]
        private string taskTags = string.Empty;

        [ObservableProperty]
        private TaskStatus selectedStatus = TaskStatus.Todo;

        [ObservableProperty]
        private TaskPriority selectedPriority = TaskPriority.Medium;

        [ObservableProperty]
        private DateTime? dueDate;

        [ObservableProperty]
        private int estimatedMinutes = 25;

        [ObservableProperty]
        private int actualMinutes = 0;

        [ObservableProperty]
        private ObservableCollection<ChecklistItem> checklistItems = new();

        [ObservableProperty]
        private ObservableCollection<AttachmentItem> attachmentItems = new();

        [ObservableProperty]
        private double progressPercentage = 0;

        [ObservableProperty]
        private double checklistCompletionPercentage = 0;

        [ObservableProperty]
        private DateTime createdAt = DateTime.Now;

        [ObservableProperty]
        private DateTime? startedAt;

        [RelayCommand]
        private void SetToday()
        {
            DueDate = DateTime.Today;
        }

        [RelayCommand]
        private void SetTomorrow()
        {
            DueDate = DateTime.Today.AddDays(1);
        }

        [RelayCommand]
        private void SetDayAfterTomorrow()
        {
            DueDate = DateTime.Today.AddDays(2);
        }

        [RelayCommand]
        private void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;

            var currentTags = TaskTags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            if (!currentTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                currentTags.Add(tag);
                TaskTags = string.Join(", ", currentTags);
            }
        }

        public ObservableCollection<TaskStatus> AvailableStatuses { get; } = new()
        {
            TaskStatus.Todo,
            TaskStatus.Waiting,
            TaskStatus.Executing,
            TaskStatus.Completed
        };

        public ObservableCollection<TaskPriority> AvailablePriorities { get; } = new()
        {
            TaskPriority.Low,
            TaskPriority.Medium,
            TaskPriority.High,
            TaskPriority.Urgent
        };

        [ObservableProperty]
        private ObservableCollection<string> availableProjects = new();

        [ObservableProperty]
        private ObservableCollection<string> popularTags = new();

        public event Action<bool?>? DialogResultChanged;

        public TaskDetailDialogViewModel(IPomodoroService pomodoroService, PomodoroTask? task = null)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _originalTask = task ?? new PomodoroTask();
            _isEditMode = task != null;

            LoadTaskData();
            PropertyChanged += OnPropertyChanged;
        }

        private void LoadTaskData()
        {
            TaskTitle = _originalTask.Title;
            Description = _originalTask.Description;
            TaskCategory = _originalTask.Category;
            TaskTags = _originalTask.TagsText;
            SelectedStatus = _originalTask.Status;
            SelectedPriority = _originalTask.Priority;
            DueDate = _originalTask.DueDate;
            EstimatedMinutes = _originalTask.EstimatedMinutes;
            ActualMinutes = _originalTask.ActualMinutes;
            CreatedAt = _originalTask.CreatedAt;
            StartedAt = _originalTask.StartedAt;

            // チェックリストをコピー
            ChecklistItems.Clear();
            foreach (var item in _originalTask.Checklist)
            {
                ChecklistItems.Add(item);
            }

            // 添付ファイルをコピー
            AttachmentItems.Clear();
            foreach (var attachment in _originalTask.Attachments)
            {
                AttachmentItems.Add(new AttachmentItem { FilePath = attachment });
            }

            UpdateProgress();
            LoadProjectsAndTags();
        }

        private void LoadProjectsAndTags()
        {
            // 既存のタスクからプロジェクト（カテゴリ）を取得
            var allTasks = _pomodoroService.GetTasks();
            var projects = allTasks
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            AvailableProjects.Clear();
            foreach (var project in projects)
            {
                AvailableProjects.Add(project);
            }

            // よく使うタグを取得（出現頻度上位10個）
            var allTags = allTasks
                .SelectMany(t => t.Tags)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            PopularTags.Clear();
            foreach (var tag in allTags)
            {
                PopularTags.Add(tag);
            }
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EstimatedMinutes) || e.PropertyName == nameof(ActualMinutes))
            {
                UpdateProgress();
            }
            else if (e.PropertyName == nameof(ChecklistItems))
            {
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            // 時間ベースの進捗
            if (EstimatedMinutes > 0)
            {
                ProgressPercentage = Math.Min(100, Math.Max(0, (double)ActualMinutes / EstimatedMinutes * 100));
            }
            else
            {
                ProgressPercentage = 0;
            }

            // チェックリストの進捗
            if (ChecklistItems.Count > 0)
            {
                var completedCount = ChecklistItems.Count(item => item.IsChecked);
                ChecklistCompletionPercentage = (double)completedCount / ChecklistItems.Count * 100;
            }
            else
            {
                ChecklistCompletionPercentage = 0;
            }
        }

        [RelayCommand]
        private void AddChecklistItem(string? text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var item = new ChecklistItem(text.Trim());
                ChecklistItems.Add(item);
                UpdateProgress();
            }
        }

        [RelayCommand]
        private void RemoveChecklistItem(ChecklistItem item)
        {
            if (item != null)
            {
                ChecklistItems.Remove(item);
                UpdateProgress();
            }
        }

        [RelayCommand]
        private void AddAttachment()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "ファイルを選択",
                Filter = "すべてのファイル (*.*)|*.*|画像ファイル (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|文書ファイル (*.pdf;*.doc;*.docx;*.txt)|*.pdf;*.doc;*.docx;*.txt",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var fileName in openFileDialog.FileNames)
                {
                    // ファイルをアプリのデータフォルダにコピー
                    var attachmentPath = CopyFileToAttachmentFolder(fileName);
                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        AttachmentItems.Add(new AttachmentItem { FilePath = attachmentPath });
                    }
                }
            }
        }

        [RelayCommand]
        private void PasteImage()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        // 画像を一時ファイルとして保存
                        var tempFileName = $"clipboard_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var attachmentPath = SaveImageToAttachmentFolder(image, tempFileName);
                        
                        if (!string.IsNullOrEmpty(attachmentPath))
                        {
                            AttachmentItems.Add(new AttachmentItem { FilePath = attachmentPath });
                        }
                    }
                }
                else
                {
                    MessageBox.Show("クリップボードに画像がありません。", "情報", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"画像の貼り付けに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void OpenAttachment(AttachmentItem attachment)
        {
            if (attachment != null && File.Exists(attachment.FilePath))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = attachment.FilePath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ファイルを開けませんでした: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void RemoveAttachment(AttachmentItem attachment)
        {
            if (attachment != null)
            {
                AttachmentItems.Remove(attachment);
                
                // ファイルも削除するか確認
                var result = MessageBox.Show("ファイルも削除しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes && File.Exists(attachment.FilePath))
                {
                    try
                    {
                        File.Delete(attachment.FilePath);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"ファイルの削除に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TaskTitle))
                {
                    MessageBox.Show("タイトルを入力してください。", "入力エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // タスクデータを更新
                _originalTask.Title = TaskTitle;
                _originalTask.Description = Description;
                _originalTask.Category = TaskCategory;
                _originalTask.TagsText = TaskTags;
                _originalTask.Status = SelectedStatus;
                _originalTask.Priority = SelectedPriority;
                _originalTask.DueDate = DueDate;
                _originalTask.EstimatedMinutes = EstimatedMinutes;
                _originalTask.ActualMinutes = ActualMinutes;

                // チェックリストを更新
                _originalTask.Checklist.Clear();
                foreach (var item in ChecklistItems)
                {
                    _originalTask.Checklist.Add(item);
                }

                // 添付ファイルを更新
                _originalTask.Attachments.Clear();
                foreach (var attachment in AttachmentItems)
                {
                    _originalTask.Attachments.Add(attachment.FilePath);
                }

                if (_isEditMode)
                {
                    _pomodoroService.UpdateTask(_originalTask);
                }
                else
                {
                    _pomodoroService.AddTask(_originalTask);
                }

                DialogResultChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResultChanged?.Invoke(false);
        }

        private string CopyFileToAttachmentFolder(string sourceFilePath)
        {
            try
            {
                var attachmentDir = GetAttachmentDirectory();
                var fileName = Path.GetFileName(sourceFilePath);
                var uniqueFileName = GenerateUniqueFileName(attachmentDir, fileName);
                var destinationPath = Path.Combine(attachmentDir, uniqueFileName);

                File.Copy(sourceFilePath, destinationPath);
                return destinationPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ファイルのコピーに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private string SaveImageToAttachmentFolder(System.Windows.Media.Imaging.BitmapSource image, string fileName)
        {
            try
            {
                var attachmentDir = GetAttachmentDirectory();
                var uniqueFileName = GenerateUniqueFileName(attachmentDir, fileName);
                var destinationPath = Path.Combine(attachmentDir, uniqueFileName);

                using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                {
                    var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                return destinationPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"画像の保存に失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private string GetAttachmentDirectory()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var attachmentDir = Path.Combine(appDataPath, "PomodoroTimer", "Attachments");
            
            if (!Directory.Exists(attachmentDir))
            {
                Directory.CreateDirectory(attachmentDir);
            }
            
            return attachmentDir;
        }

        private string GenerateUniqueFileName(string directory, string fileName)
        {
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var counter = 1;
            var uniqueFileName = fileName;

            while (File.Exists(Path.Combine(directory, uniqueFileName)))
            {
                uniqueFileName = $"{nameWithoutExtension}_{counter}{extension}";
                counter++;
            }

            return uniqueFileName;
        }
    }

    public class AttachmentItem : ObservableObject
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
        public string FileSize 
        { 
            get 
            {
                try
                {
                    var info = new FileInfo(FilePath);
                    return FormatFileSize(info.Length);
                }
                catch
                {
                    return "不明";
                }
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
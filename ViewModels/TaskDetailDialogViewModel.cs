using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using PomodoroTimer.Helpers;
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
        private readonly AppSettings? _appSettings;

        [ObservableProperty]
        private string taskTitle = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private string taskCategory = string.Empty;

        [ObservableProperty]
        private string taskTags = string.Empty;

        [ObservableProperty]
        private string url = string.Empty;

        [ObservableProperty]
        private ObservableCollection<LinkItem> linkItems = new();

        [ObservableProperty]
        private StatusDisplayItem? selectedStatusDisplay;

        [ObservableProperty]
        private PriorityDisplayItem? selectedPriorityDisplay;

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

        [ObservableProperty]
        private bool hasUnsavedChanges = false;


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

        public ObservableCollection<StatusDisplayItem> AvailableStatuses { get; } = new()
        {
            new StatusDisplayItem(TaskStatus.Todo, "未開始"),
            new StatusDisplayItem(TaskStatus.Waiting, "待機中"),
            new StatusDisplayItem(TaskStatus.Executing, "実行中"),
            new StatusDisplayItem(TaskStatus.Completed, "完了")
        };

        public ObservableCollection<PriorityDisplayItem> AvailablePriorities { get; } = new()
        {
            new PriorityDisplayItem(TaskPriority.Low, "低"),
            new PriorityDisplayItem(TaskPriority.Medium, "中"),
            new PriorityDisplayItem(TaskPriority.High, "高"),
            new PriorityDisplayItem(TaskPriority.Urgent, "緊急")
        };

        [ObservableProperty]
        private ObservableCollection<string> availableProjects = new();

        [ObservableProperty]
        private ObservableCollection<string> popularTags = new();

        [ObservableProperty]
        private double dialogWidth = 900;

        [ObservableProperty]
        private double dialogHeight = 700;

        public event Action<bool?>? DialogResultChanged;

        public TaskDetailDialogViewModel(IPomodoroService pomodoroService, PomodoroTask? task = null, AppSettings? appSettings = null)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            _originalTask = task ?? new PomodoroTask();
            _isEditMode = task != null;
            _appSettings = appSettings;

            // AppSettingsからサイズを設定
            if (_appSettings != null)
            {
                DialogWidth = _appSettings.TaskDetailDialogWidth;
                DialogHeight = _appSettings.TaskDetailDialogHeight;
            }

            // コマンドを初期化
            PasteImageCommand = new RelayCommand(PasteImage);

            LoadTaskData();
            PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// ダイアログサイズが変更されたときの処理
        /// </summary>
        public void OnSizeChanged(double width, double height)
        {
            if (_appSettings != null)
            {
                _appSettings.TaskDetailDialogWidth = width;
                _appSettings.TaskDetailDialogHeight = height;
                
                // ViewModelのプロパティも更新
                DialogWidth = width;
                DialogHeight = height;
            }
        }

        private void LoadTaskData()
        {
            TaskTitle = _originalTask.Title;
            Description = _originalTask.Description;
            TaskCategory = _originalTask.Category;
            TaskTags = _originalTask.TagsText;
            Url = _originalTask.Url;
            SelectedStatusDisplay = AvailableStatuses.FirstOrDefault(s => s.Status == _originalTask.Status);
            SelectedPriorityDisplay = AvailablePriorities.FirstOrDefault(p => p.Priority == _originalTask.Priority);
            DueDate = _originalTask.DueDate;
            EstimatedMinutes = _originalTask.EstimatedMinutes;
            ActualMinutes = _originalTask.ActualMinutes;
            CreatedAt = _originalTask.CreatedAt;
            StartedAt = _originalTask.StartedAt;

            // デバッグ用：タスクのDescriptionを確認

            // チェックリストをコピー
            ChecklistItems.Clear();
            foreach (var item in _originalTask.Checklist)
            {
                ChecklistItems.Add(item);
            }

            // 添付ファイルをコピー（存在確認付き）
            AttachmentItems.Clear();
            foreach (var attachment in _originalTask.Attachments.ToList()) // ToListで安全にイテレート
            {
                if (File.Exists(attachment))
                {
                    AttachmentItems.Add(new AttachmentItem { FilePath = attachment });
                }
                else
                {
                    // 存在しないファイルは元のリストからも削除
                    _originalTask.Attachments.Remove(attachment);
                }
            }

            // リンクアイテムをコピー（データマイグレーション含む）
            _originalTask.MigrateUrlToLinks();
            LinkItems.Clear();
            foreach (var link in _originalTask.Links)
            {
                LinkItems.Add(link);
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
                HasUnsavedChanges = true;
            }
            else if (e.PropertyName == nameof(ChecklistItems))
            {
                UpdateProgress();
                HasUnsavedChanges = true;
            }
            else if (e.PropertyName == nameof(TaskTitle) || 
                     e.PropertyName == nameof(Description) || 
                     e.PropertyName == nameof(TaskCategory) || 
                     e.PropertyName == nameof(TaskTags) ||
                     e.PropertyName == nameof(DueDate))
            {
                HasUnsavedChanges = true;
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
                var addedCount = 0;
                foreach (var fileName in openFileDialog.FileNames)
                {
                    var attachmentPath = CopyFileToAttachmentFolder(fileName);
                    if (AddAttachmentToList(attachmentPath))
                    {
                        addedCount++;
                    }
                }
                
                if (addedCount > 0)
                {
                    AutoSave();
                }
            }
        }

        [RelayCommand]
        private void AddLinkItem()
        {
            var newLink = new LinkItem("新しいリンク", "https://");
            LinkItems.Add(newLink);
        }

        [RelayCommand]
        private void RemoveLinkItem(LinkItem linkItem)
        {
            if (linkItem != null)
            {
                LinkItems.Remove(linkItem);
            }
        }

        public IRelayCommand PasteImageCommand { get; }

        private void PasteImage()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    var image = Clipboard.GetImage();
                    if (image != null)
                    {
                        var attachmentPath = SaveImageToAttachmentFolder(image);
                        if (AddAttachmentToList(attachmentPath))
                        {
                            AutoSave();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError("画像の貼り付けに失敗しました", ex);
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
                    ErrorHandler.ShowError("ファイルを開けませんでした", ex);
                }
            }
        }

        [RelayCommand]
        private void RemoveAttachment(AttachmentItem attachment)
        {
            if (attachment != null)
            {
                AttachmentItems.Remove(attachment);
                HasUnsavedChanges = true;
                
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
                        ErrorHandler.ShowError("ファイルの削除に失敗しました", ex);
                    }
                }
                
                // 添付ファイル削除時に自動保存を実行
                AutoSave();
            }
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (!ValidationHelper.ValidateRequired(TaskTitle, "タイトル"))
                {
                    return;
                }

                UpdateTaskFromViewModel();

                if (_isEditMode)
                {
                    _pomodoroService.UpdateTask(_originalTask);
                }
                else
                {
                    _pomodoroService.AddTask(_originalTask);
                }

                HasUnsavedChanges = false;
                DialogResultChanged?.Invoke(true);
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError("保存に失敗しました", ex);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResultChanged?.Invoke(false);
        }

        /// <summary>
        /// 自動保存を実行（画像添付時など）
        /// </summary>
        public void AutoSave()
        {
            try
            {
                if (!_isEditMode) return; // 新規作成時は自動保存しない

                UpdateTaskFromViewModel();
                _pomodoroService.UpdateTask(_originalTask);
                
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                ErrorHandler.ShowError("自動保存に失敗しました", ex);
            }
        }

        /// <summary>
        /// ViewModelの値でタスクデータを更新する共通メソッド
        /// </summary>
        private void UpdateTaskFromViewModel()
        {
            // 基本情報を更新
            _originalTask.Title = TaskTitle;
            _originalTask.Description = Description;
            _originalTask.Category = TaskCategory;
            _originalTask.TagsText = TaskTags;
            _originalTask.Url = Url;
            _originalTask.Status = SelectedStatusDisplay?.Status ?? TaskStatus.Todo;
            _originalTask.Priority = SelectedPriorityDisplay?.Priority ?? TaskPriority.Medium;
            _originalTask.DueDate = DueDate;
            _originalTask.EstimatedMinutes = EstimatedMinutes;
            _originalTask.ActualMinutes = ActualMinutes;

            // チェックリストを更新（新しいリストを作成して代入することでOnChecklistChangedを発火）
            _originalTask.Checklist = new List<ChecklistItem>(ChecklistItems);

            // 添付ファイルを更新（存在確認付き）
            _originalTask.Attachments.Clear();
            foreach (var attachment in AttachmentItems)
            {
                if (File.Exists(attachment.FilePath))
                {
                    _originalTask.Attachments.Add(attachment.FilePath);
                }
            }

            // リンクアイテムを更新
            _originalTask.Links.Clear();
            foreach (var link in LinkItems)
            {
                _originalTask.Links.Add(link);
            }
        }

        /// <summary>
        /// 添付ファイルをリストに追加する共通メソッド
        /// </summary>
        public bool AddAttachmentToList(string attachmentPath)
        {
            if (string.IsNullOrEmpty(attachmentPath)) return false;

            AttachmentItems.Add(new AttachmentItem { FilePath = attachmentPath });
            HasUnsavedChanges = true;
            return true;
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
                ErrorHandler.ShowError("ファイルのコピーに失敗しました", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// 画像を添付ファイルフォルダに保存（オーバーロード：ビットマップから）
        /// </summary>
        public string SaveImageToAttachmentFolder(System.Windows.Media.Imaging.BitmapSource image, string? customFileName = null)
        {
            try
            {
                var fileName = customFileName ?? $"clipboard_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
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
                ErrorHandler.ShowError("画像の保存に失敗しました", ex);
                return string.Empty;
            }
        }

        private string GetAttachmentDirectory()
        {
            AppPaths.EnsureDirectoriesExist();
            return AppPaths.AttachmentDirectory;
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

    public class StatusDisplayItem
    {
        public TaskStatus Status { get; }
        public string DisplayText { get; }
        public string Color { get; }

        public StatusDisplayItem(TaskStatus status, string displayText)
        {
            Status = status;
            DisplayText = displayText;
            Color = status switch
            {
                TaskStatus.Todo => "#6B7280",      // グレー
                TaskStatus.Waiting => "#F59E0B",   // オレンジ
                TaskStatus.Executing => "#3B82F6", // ブルー
                TaskStatus.Completed => "#10B981", // グリーン
                _ => "#6B7280"
            };
        }

        public override string ToString() => DisplayText;
    }

    public class PriorityDisplayItem
    {
        public TaskPriority Priority { get; }
        public string DisplayText { get; }
        public string Color { get; }
        public string Icon { get; }

        public PriorityDisplayItem(TaskPriority priority, string displayText)
        {
            Priority = priority;
            DisplayText = displayText;
            (Color, Icon) = priority switch
            {
                TaskPriority.Low => ("#10B981", "🔵"),      // グリーン + 青丸
                TaskPriority.Medium => ("#F59E0B", "🟡"),   // オレンジ + 黄丸
                TaskPriority.High => ("#EF4444", "🟠"),     // レッド + オレンジ丸
                TaskPriority.Urgent => ("#DC2626", "🔴"),   // ダークレッド + 赤丸
                _ => ("#F59E0B", "🟡")
            };
        }

        public override string ToString() => DisplayText;
    }
}
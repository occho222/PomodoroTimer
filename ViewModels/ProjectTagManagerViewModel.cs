using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// プロジェクトとタグの管理ViewModel
    /// </summary>
    public partial class ProjectTagManagerViewModel : ObservableObject
    {
        private readonly IPomodoroService _pomodoroService;

        [ObservableProperty]
        private ObservableCollection<ProjectItem> projects = new();

        [ObservableProperty]
        private ObservableCollection<TagItem> tags = new();

        public event Action? DialogClosing;

        public ProjectTagManagerViewModel(IPomodoroService pomodoroService)
        {
            _pomodoroService = pomodoroService ?? throw new ArgumentNullException(nameof(pomodoroService));
            LoadData();
        }

        [RelayCommand]
        private void AddProject(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName)) return;

            projectName = projectName.Trim();

            // 既存チェック
            if (Projects.Any(p => string.Equals(p.Name, projectName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"プロジェクト '{projectName}' は既に存在します。", "重複エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var projectItem = new ProjectItem { Name = projectName, TaskCount = 0 };
            Projects.Add(projectItem);

            MessageBox.Show($"プロジェクト '{projectName}' を追加しました。", "追加完了", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task EditProject(ProjectItem projectItem)
        {
            if (projectItem == null) return;

            var result = PromptForInput("プロジェクト名の編集", "新しいプロジェクト名を入力してください:", projectItem.Name);

            if (string.IsNullOrWhiteSpace(result) || result == projectItem.Name) return;

            var newName = result.Trim();

            // 既存チェック
            if (Projects.Any(p => p != projectItem && string.Equals(p.Name, newName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"プロジェクト '{newName}' は既に存在します。", "重複エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 実際のタスクのカテゴリも更新
            var tasks = _pomodoroService.GetTasks();
            var affectedTasks = tasks.Where(t => string.Equals(t.Category, projectItem.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var task in affectedTasks)
            {
                task.Category = newName;
            }

            if (affectedTasks.Any())
            {
                await _pomodoroService.SaveTasksAsync();
            }

            projectItem.Name = newName;

            MessageBox.Show($"プロジェクト名を '{newName}' に変更しました。\n{affectedTasks.Count}個のタスクが更新されました。", 
                "編集完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task DeleteProject(ProjectItem projectItem)
        {
            if (projectItem == null) return;

            var result = MessageBox.Show(
                $"プロジェクト '{projectItem.Name}' を削除しますか？\n\n" +
                $"このプロジェクトに属する{projectItem.TaskCount}個のタスクのプロジェクト設定は解除されます。\n" +
                "（タスク自体は削除されません）",
                "プロジェクト削除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // 実際のタスクのカテゴリをクリア
            var tasks = _pomodoroService.GetTasks();
            var affectedTasks = tasks.Where(t => string.Equals(t.Category, projectItem.Name, StringComparison.OrdinalIgnoreCase)).ToList();

            foreach (var task in affectedTasks)
            {
                task.Category = string.Empty;
            }

            if (affectedTasks.Any())
            {
                await _pomodoroService.SaveTasksAsync();
            }

            Projects.Remove(projectItem);

            MessageBox.Show($"プロジェクト '{projectItem.Name}' を削除しました。\n{affectedTasks.Count}個のタスクのプロジェクト設定を解除しました。", 
                "削除完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void AddTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;

            tagName = tagName.Trim();

            // 既存チェック
            if (Tags.Any(t => string.Equals(t.Name, tagName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"タグ '{tagName}' は既に存在します。", "重複エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var tagItem = new TagItem { Name = tagName, UsageCount = 0 };
            Tags.Add(tagItem);

            MessageBox.Show($"タグ '{tagName}' を追加しました。", "追加完了", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task EditTag(TagItem tagItem)
        {
            if (tagItem == null) return;

            var result = PromptForInput("タグ名の編集", "新しいタグ名を入力してください:", tagItem.Name);

            if (string.IsNullOrWhiteSpace(result) || result == tagItem.Name) return;

            var newName = result.Trim();

            // 既存チェック
            if (Tags.Any(t => t != tagItem && string.Equals(t.Name, newName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"タグ '{newName}' は既に存在します。", "重複エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 実際のタスクのタグも更新
            var tasks = _pomodoroService.GetTasks();
            var affectedTasks = new List<Models.PomodoroTask>();

            foreach (var task in tasks)
            {
                var tags = task.Tags.ToList();
                var index = tags.FindIndex(t => string.Equals(t, tagItem.Name, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    tags[index] = newName;
                    task.Tags = tags;
                    affectedTasks.Add(task);
                }
            }

            if (affectedTasks.Any())
            {
                await _pomodoroService.SaveTasksAsync();
            }

            tagItem.Name = newName;

            MessageBox.Show($"タグ名を '{newName}' に変更しました。\n{affectedTasks.Count}個のタスクが更新されました。", 
                "編集完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task DeleteTag(TagItem tagItem)
        {
            if (tagItem == null) return;

            var result = MessageBox.Show(
                $"タグ '{tagItem.Name}' を削除しますか？\n\n" +
                $"このタグが付けられた{tagItem.UsageCount}個のタスクからタグが削除されます。\n" +
                "（タスク自体は削除されません）",
                "タグ削除の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // 実際のタスクからタグを削除
            var tasks = _pomodoroService.GetTasks();
            var affectedTasks = new List<Models.PomodoroTask>();

            foreach (var task in tasks)
            {
                var tags = task.Tags.ToList();
                var index = tags.FindIndex(t => string.Equals(t, tagItem.Name, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    tags.RemoveAt(index);
                    task.Tags = tags;
                    affectedTasks.Add(task);
                }
            }

            if (affectedTasks.Any())
            {
                await _pomodoroService.SaveTasksAsync();
            }

            Tags.Remove(tagItem);

            MessageBox.Show($"タグ '{tagItem.Name}' を削除しました。\n{affectedTasks.Count}個のタスクからタグを削除しました。", 
                "削除完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void Close()
        {
            DialogClosing?.Invoke();
        }

        private void LoadData()
        {
            var tasks = _pomodoroService.GetTasks();

            // プロジェクト（カテゴリ）データを読み込み
            var projectGroups = tasks
                .Where(t => !string.IsNullOrEmpty(t.Category))
                .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key)
                .ToList();

            Projects.Clear();
            foreach (var group in projectGroups)
            {
                Projects.Add(new ProjectItem 
                { 
                    Name = group.Key, 
                    TaskCount = group.Count() 
                });
            }

            // タグデータを読み込み
            var tagGroups = tasks
                .SelectMany(t => t.Tags)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ToList();

            Tags.Clear();
            foreach (var group in tagGroups)
            {
                Tags.Add(new TagItem 
                { 
                    Name = group.Key, 
                    UsageCount = group.Count() 
                });
            }
        }

        private string? PromptForInput(string title, string prompt, string defaultValue = "")
        {
            var result = "";
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // シンプルな入力ダイアログ
                var input = new System.Windows.Controls.TextBox
                {
                    Text = defaultValue,
                    Width = 300,
                    Height = 25,
                    Margin = new Thickness(10)
                };

                var dialog = new Window
                {
                    Title = title,
                    Width = 350,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Content = new System.Windows.Controls.StackPanel
                    {
                        Children =
                        {
                            new System.Windows.Controls.TextBlock
                            {
                                Text = prompt,
                                Margin = new Thickness(10, 10, 10, 5),
                                TextWrapping = TextWrapping.Wrap
                            },
                            input,
                            new System.Windows.Controls.StackPanel
                            {
                                Orientation = System.Windows.Controls.Orientation.Horizontal,
                                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                                Margin = new Thickness(10),
                                Children =
                                {
                                    new System.Windows.Controls.Button
                                    {
                                        Content = "OK",
                                        Width = 60,
                                        Height = 25,
                                        Margin = new Thickness(5, 0, 0, 0),
                                        IsDefault = true
                                    },
                                    new System.Windows.Controls.Button
                                    {
                                        Content = "キャンセル",
                                        Width = 80,
                                        Height = 25,
                                        Margin = new Thickness(5, 0, 0, 0),
                                        IsCancel = true
                                    }
                                }
                            }
                        }
                    }
                };

                input.Focus();
                input.SelectAll();

                if (dialog.ShowDialog() == true)
                {
                    result = input.Text;
                }
            });

            return result;
        }
    }

    /// <summary>
    /// プロジェクト項目
    /// </summary>
    public partial class ProjectItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private int taskCount = 0;
    }

    /// <summary>
    /// タグ項目
    /// </summary>
    public partial class TagItem : ObservableObject
    {
        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private int usageCount = 0;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PomodoroTimer.Models;
using PomodoroTimer.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.Text.Json;
using System.IO;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace PomodoroTimer.ViewModels
{
    /// <summary>
    /// クイックテンプレート管理ViewModel
    /// </summary>
    public partial class QuickTemplateManagerViewModel : ObservableObject
    {
        private readonly IDataPersistenceService _dataPersistenceService;

        [ObservableProperty]
        private ObservableCollection<QuickTemplate> templates = new();

        [ObservableProperty]
        private QuickTemplate? selectedTemplate;

        public event Action? DialogClosing;

        public QuickTemplateManagerViewModel(IDataPersistenceService dataPersistenceService)
        {
            _dataPersistenceService = dataPersistenceService ?? throw new ArgumentNullException(nameof(dataPersistenceService));
            LoadTemplates();
        }

        /// <summary>
        /// テンプレートを読み込む
        /// </summary>
        private async void LoadTemplates()
        {
            try
            {
                var loadedTemplates = await _dataPersistenceService.LoadDataAsync<List<QuickTemplate>>("quick_templates.json");
                Templates.Clear();

                if (loadedTemplates != null && loadedTemplates.Count > 0)
                {
                    foreach (var template in loadedTemplates)
                    {
                        Templates.Add(template);
                    }
                }
                else
                {
                    // デフォルトテンプレートを追加
                    AddDefaultTemplates();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テンプレートの読み込みに失敗しました: {ex.Message}");
                AddDefaultTemplates();
            }
        }

        /// <summary>
        /// デフォルトテンプレートを追加
        /// </summary>
        private void AddDefaultTemplates()
        {
            var defaultTemplates = new List<QuickTemplate>
            {
                new QuickTemplate
                {
                    Id = "coding",
                    DisplayName = "💻 コーディング",
                    TaskTitle = "コーディング作業",
                    TaskDescription = "開発作業を実施します",
                    Category = "開発",
                    Tags = new List<string> { "開発", "プログラミング" },
                    Priority = TaskPriority.High,
                    EstimatedMinutes = 50,
                    BackgroundColor = "#3B82F6"
                },
                new QuickTemplate
                {
                    Id = "review",
                    DisplayName = "👀 レビュー",
                    TaskTitle = "レビュー作業",
                    TaskDescription = "レビューを実施します",
                    Category = "レビュー",
                    Tags = new List<string> { "レビュー", "品質管理" },
                    Priority = TaskPriority.Medium,
                    EstimatedMinutes = 25,
                    BackgroundColor = "#10B981"
                },
                new QuickTemplate
                {
                    Id = "document",
                    DisplayName = "📄 ドキュメント",
                    TaskTitle = "ドキュメント作成",
                    TaskDescription = "ドキュメントの作成・更新を行います",
                    Category = "ドキュメント",
                    Tags = new List<string> { "ドキュメント", "仕様書" },
                    Priority = TaskPriority.Medium,
                    EstimatedMinutes = 25,
                    BackgroundColor = "#F59E0B"
                },
                new QuickTemplate
                {
                    Id = "learning",
                    DisplayName = "📚 学習",
                    TaskTitle = "学習・研修",
                    TaskDescription = "技術習得や学習を行います",
                    Category = "学習",
                    Tags = new List<string> { "学習", "スキルアップ" },
                    Priority = TaskPriority.Low,
                    EstimatedMinutes = 25,
                    BackgroundColor = "#8B5CF6"
                },
                new QuickTemplate
                {
                    Id = "meeting",
                    DisplayName = "📝 会議",
                    TaskTitle = "会議参加",
                    TaskDescription = "会議への参加",
                    Category = "会議",
                    Tags = new List<string> { "会議", "ミーティング" },
                    Priority = TaskPriority.Medium,
                    EstimatedMinutes = 30,
                    BackgroundColor = "#8B5CF6"
                },
                new QuickTemplate
                {
                    Id = "email",
                    DisplayName = "📧 メール処理",
                    TaskTitle = "メール処理",
                    TaskDescription = "メールの確認・返信作業",
                    Category = "コミュニケーション",
                    Tags = new List<string> { "メール", "連絡" },
                    Priority = TaskPriority.Medium,
                    EstimatedMinutes = 25,
                    BackgroundColor = "#10B981"
                }
            };

            foreach (var template in defaultTemplates)
            {
                Templates.Add(template);
            }
        }

        /// <summary>
        /// 新規テンプレート追加コマンド
        /// </summary>
        [RelayCommand]
        private void AddNewTemplate()
        {
            try
            {
                var editDialog = new Views.QuickTemplateEditDialog();
                if (editDialog.ShowDialog() == true)
                {
                    var newTemplate = new QuickTemplate
                    {
                        Id = Guid.NewGuid().ToString(),
                        DisplayName = editDialog.DisplayName,
                        TaskTitle = editDialog.TaskTitle,
                        TaskDescription = editDialog.TaskDescription,
                        Category = editDialog.Category,
                        Tags = editDialog.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(t => t.Trim()).ToList(),
                        Priority = editDialog.Priority,
                        EstimatedMinutes = editDialog.EstimatedMinutes,
                        BackgroundColor = editDialog.BackgroundColor,
                        DefaultChecklist = editDialog.DefaultChecklist,
                        DefaultLinks = editDialog.DefaultLinks
                    };

                    Templates.Add(newTemplate);
                    SaveTemplates();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テンプレートの追加に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 選択テンプレート編集コマンド
        /// </summary>
        [RelayCommand]
        private void EditSelectedTemplate()
        {
            if (SelectedTemplate == null) return;

            try
            {
                var editDialog = new Views.QuickTemplateEditDialog(SelectedTemplate);
                if (editDialog.ShowDialog() == true)
                {
                    // 選択されたテンプレートを更新
                    SelectedTemplate.DisplayName = editDialog.DisplayName;
                    SelectedTemplate.TaskTitle = editDialog.TaskTitle;
                    SelectedTemplate.TaskDescription = editDialog.TaskDescription;
                    SelectedTemplate.Category = editDialog.Category;
                    SelectedTemplate.Tags = editDialog.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim()).ToList();
                    SelectedTemplate.Priority = editDialog.Priority;
                    SelectedTemplate.EstimatedMinutes = editDialog.EstimatedMinutes;
                    SelectedTemplate.BackgroundColor = editDialog.BackgroundColor;
                    SelectedTemplate.DefaultChecklist = editDialog.DefaultChecklist;
                    SelectedTemplate.DefaultLinks = editDialog.DefaultLinks;

                    SaveTemplates();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テンプレートの編集に失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 選択テンプレート削除コマンド
        /// </summary>
        [RelayCommand]
        private void DeleteSelectedTemplate()
        {
            if (SelectedTemplate == null) return;

            var result = MessageBox.Show($"テンプレート「{SelectedTemplate.DisplayName}」を削除しますか？",
                "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Templates.Remove(SelectedTemplate);
                SelectedTemplate = null;
                SaveTemplates();
            }
        }

        /// <summary>
        /// テンプレートインポートコマンド
        /// </summary>
        [RelayCommand]
        private async Task ImportTemplates()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "JSONファイル (*.json)|*.json|すべてのファイル (*.*)|*.*",
                    Title = "テンプレートファイルを選択"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var jsonContent = await File.ReadAllTextAsync(openFileDialog.FileName);
                    var importedTemplates = JsonSerializer.Deserialize<List<QuickTemplate>>(jsonContent);

                    if (importedTemplates != null)
                    {
                        var duplicateCount = 0;
                        var newCount = 0;

                        foreach (var template in importedTemplates)
                        {
                            if (Templates.Any(t => t.Id == template.Id))
                            {
                                duplicateCount++;
                            }
                            else
                            {
                                Templates.Add(template);
                                newCount++;
                            }
                        }

                        MessageBox.Show($"インポート完了:\n新規追加: {newCount}件\n重複スキップ: {duplicateCount}件",
                            "インポート結果", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (newCount > 0)
                        {
                            SaveTemplates();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"インポートに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// テンプレートエクスポートコマンド
        /// </summary>
        [RelayCommand]
        private async Task ExportTemplates()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "JSONファイル (*.json)|*.json",
                    DefaultExt = "json",
                    Title = "テンプレートファイルを保存"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var jsonContent = JsonSerializer.Serialize(Templates.ToList(), new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    await File.WriteAllTextAsync(saveFileDialog.FileName, jsonContent);
                    MessageBox.Show($"{Templates.Count}件のテンプレートをエクスポートしました。",
                        "エクスポート完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エクスポートに失敗しました: {ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 閉じるコマンド
        /// </summary>
        [RelayCommand]
        private void Close()
        {
            DialogClosing?.Invoke();
        }

        /// <summary>
        /// テンプレートを保存
        /// </summary>
        private async void SaveTemplates()
        {
            try
            {
                await _dataPersistenceService.SaveDataAsync("quick_templates.json", Templates.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"テンプレートの保存に失敗しました: {ex.Message}");
            }
        }
    }

}
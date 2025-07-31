using PomodoroTimer.Helpers;
using PomodoroTimer.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// JSONファイルを使用したデータ永続化サービス
    /// </summary>
    public class JsonDataPersistenceService : IDataPersistenceService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDataPersistenceService()
        {
            // JSON設定
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            // 必要なディレクトリを作成
            AppPaths.EnsureDirectoriesExist();
        }

        public async Task SaveDataAsync(ObservableCollection<PomodoroTask> tasks, AppSettings settings, SessionStatistics statistics)
        {
            try
            {
                // タスクを保存
                var tasksJson = JsonSerializer.Serialize(tasks.ToList(), _jsonOptions);
                await File.WriteAllTextAsync(AppPaths.TasksFilePath, tasksJson);

                // 設定を保存
                var settingsJson = JsonSerializer.Serialize(settings, _jsonOptions);
                await File.WriteAllTextAsync(AppPaths.SettingsFilePath, settingsJson);

                // 統計を保存
                var statisticsJson = JsonSerializer.Serialize(statistics, _jsonOptions);
                await File.WriteAllTextAsync(AppPaths.StatisticsFilePath, statisticsJson);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"データの保存に失敗しました: {ex.Message}", ex);
            }
        }

        public async Task<(ObservableCollection<PomodoroTask> Tasks, AppSettings Settings, SessionStatistics Statistics)> LoadDataAsync()
        {
            try
            {
                // タスクを読み込み
                var tasks = new ObservableCollection<PomodoroTask>();
                if (File.Exists(AppPaths.TasksFilePath))
                {
                    var tasksJson = await File.ReadAllTextAsync(AppPaths.TasksFilePath);
                    if (!string.IsNullOrWhiteSpace(tasksJson))
                    {
                        var tasksList = JsonSerializer.Deserialize<List<PomodoroTask>>(tasksJson, _jsonOptions) ?? new List<PomodoroTask>();
                        tasks = new ObservableCollection<PomodoroTask>(tasksList);
                    }
                }

                // 設定を読み込み
                var settings = new AppSettings();
                if (File.Exists(AppPaths.SettingsFilePath))
                {
                    var settingsJson = await File.ReadAllTextAsync(AppPaths.SettingsFilePath);
                    if (!string.IsNullOrWhiteSpace(settingsJson))
                    {
                        settings = JsonSerializer.Deserialize<AppSettings>(settingsJson, _jsonOptions) ?? new AppSettings();
                    }
                }

                // 統計を読み込み
                var statistics = new SessionStatistics();
                if (File.Exists(AppPaths.StatisticsFilePath))
                {
                    var statisticsJson = await File.ReadAllTextAsync(AppPaths.StatisticsFilePath);
                    if (!string.IsNullOrWhiteSpace(statisticsJson))
                    {
                        statistics = JsonSerializer.Deserialize<SessionStatistics>(statisticsJson, _jsonOptions) ?? new SessionStatistics();
                    }
                }

                return (tasks, settings, statistics);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"データの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        public bool DataExists()
        {
            return File.Exists(AppPaths.TasksFilePath) || File.Exists(AppPaths.SettingsFilePath) || File.Exists(AppPaths.StatisticsFilePath);
        }

        public async Task ResetDataAsync()
        {
            try
            {
                if (File.Exists(AppPaths.TasksFilePath))
                    File.Delete(AppPaths.TasksFilePath);

                if (File.Exists(AppPaths.SettingsFilePath))
                    File.Delete(AppPaths.SettingsFilePath);

                if (File.Exists(AppPaths.StatisticsFilePath))
                    File.Delete(AppPaths.StatisticsFilePath);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"データのリセットに失敗しました: {ex.Message}", ex);
            }
        }

        public async Task SaveDataAsync<T>(string fileName, T data)
        {
            try
            {
                var filePath = Path.Combine(AppPaths.AppDataDirectory, fileName);
                var json = JsonSerializer.Serialize(data, _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"データの保存に失敗しました: {ex.Message}", ex);
            }
        }

        public async Task<T?> LoadDataAsync<T>(string fileName)
        {
            try
            {
                var filePath = Path.Combine(AppPaths.AppDataDirectory, fileName);
                
                if (!File.Exists(filePath))
                    return default(T);

                var json = await File.ReadAllTextAsync(filePath);
                
                if (string.IsNullOrWhiteSpace(json))
                    return default(T);

                // タスクリストの場合はnull値をフィルタリング
                if (typeof(T) == typeof(List<PomodoroTask>))
                {
                    var tasksList = JsonSerializer.Deserialize<List<PomodoroTask?>>(json, _jsonOptions);
                    if (tasksList != null)
                    {
                        var filteredTasks = tasksList.Where(t => t != null).Cast<PomodoroTask>().ToList();
                        return (T)(object)filteredTasks;
                    }
                }

                return JsonSerializer.Deserialize<T>(json, _jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"データの読み込みに失敗しました: {ex.Message}");
                Console.WriteLine($"スタックトレース: {ex.StackTrace}");
                return default(T);
            }
        }

        public bool FileExists(string fileName)
        {
            var filePath = Path.Combine(AppPaths.AppDataDirectory, fileName);
            return File.Exists(filePath);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            try
            {
                var filePath = Path.Combine(AppPaths.AppDataDirectory, fileName);
                if (File.Exists(filePath))
                {
                    await Task.Run(() => File.Delete(filePath));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ファイルの削除に失敗しました: {ex.Message}", ex);
            }
        }
    }
}
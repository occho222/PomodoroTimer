using System.Windows.Threading;
using PomodoroTimer.Models;

namespace PomodoroTimer.Services
{
    /// <summary>
    /// タイマーサービスの実装
    /// </summary>
    public class TimerService : ITimerService
    {
        private DispatcherTimer _timer;
        private TimeSpan _sessionDuration;
        private TimeSpan _remainingTime;
        private bool _isRunning;
        private SessionType _currentSessionType;
        private int _completedPomodoros;
        private AppSettings _settings;
        private INotificationService _notificationService;

        public event Action? TimerStarted;
        public event Action? TimerStopped;
        public event Action? TimerPaused;
        public event Action? TimerResumed;
        public event Action<TimeSpan>? TimeUpdated;
        public event Action<SessionType>? SessionCompleted;
        public event Action<SessionType>? SessionTypeChanged;

        public bool IsRunning => _isRunning;
        public TimeSpan RemainingTime => _remainingTime;
        public TimeSpan SessionDuration => _sessionDuration;
        public SessionType CurrentSessionType => _currentSessionType;
        public int CompletedPomodoros => _completedPomodoros;

        public TimerService(INotificationService? notificationService = null)
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            
            _currentSessionType = SessionType.Work;
            _completedPomodoros = 0;
            _settings = new AppSettings(); // デフォルト設定
            _notificationService = notificationService ?? new NotificationService();
        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _notificationService.UpdateSettings(settings.EnableSoundNotification, settings.EnableDesktopNotification);
        }

        public void Start(TimeSpan duration)
        {
            _sessionDuration = duration;
            _remainingTime = duration;
            _isRunning = true;
            _timer.Start();
            
            TimerStarted?.Invoke();
            TimeUpdated?.Invoke(_remainingTime);
        }

        public void StartNewPomodoroCycle()
        {
            _currentSessionType = SessionType.Work;
            _completedPomodoros = 0;
            
            var duration = TimeSpan.FromMinutes(_settings.WorkSessionMinutes);
            Start(duration);
            
            SessionTypeChanged?.Invoke(_currentSessionType);
        }

        public void Stop()
        {
            _timer.Stop();
            _isRunning = false;
            _remainingTime = _sessionDuration;
            
            TimerStopped?.Invoke();
            TimeUpdated?.Invoke(_remainingTime);
        }

        public void Pause()
        {
            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
                TimerPaused?.Invoke();
            }
        }

        public void Resume()
        {
            if (!_isRunning && _remainingTime > TimeSpan.Zero)
            {
                _timer.Start();
                _isRunning = true;
                TimerResumed?.Invoke();
            }
        }

        public void Skip()
        {
            _timer.Stop();
            _isRunning = false;
            
            HandleSessionCompletion();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            TimeUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= TimeSpan.Zero)
            {
                // セッション完了
                _timer.Stop();
                _isRunning = false;
                HandleSessionCompletion();
            }
        }

        private void HandleSessionCompletion()
        {
            // 通知とサウンド再生
            ShowSessionCompletionNotification();

            // セッション完了イベントを発火
            SessionCompleted?.Invoke(_currentSessionType);

            // 現在のセッションがワークセッションの場合、ポモドーロ数をインクリメント
            if (_currentSessionType == SessionType.Work)
            {
                _completedPomodoros++;
            }

            // 自動で次のセッションに移行するかどうか
            if (_settings.AutoStartNextSession)
            {
                StartNextSession();
            }
            else
            {
                // 次のセッションの準備だけする
                PrepareNextSession();
            }
        }

        private void StartNextSession()
        {
            var nextSessionType = GetNextSessionType();
            var duration = GetSessionDuration(nextSessionType);
            
            _currentSessionType = nextSessionType;
            SessionTypeChanged?.Invoke(_currentSessionType);
            
            Start(duration);
        }

        private void PrepareNextSession()
        {
            var nextSessionType = GetNextSessionType();
            var duration = GetSessionDuration(nextSessionType);
            
            _currentSessionType = nextSessionType;
            _sessionDuration = duration;
            _remainingTime = duration;
            
            SessionTypeChanged?.Invoke(_currentSessionType);
            TimeUpdated?.Invoke(_remainingTime);
        }

        private SessionType GetNextSessionType()
        {
            switch (_currentSessionType)
            {
                case SessionType.Work:
                    // 4ポモドーロ毎に長休憩
                    return (_completedPomodoros % _settings.PomodorosBeforeLongBreak == 0) 
                        ? SessionType.LongBreak 
                        : SessionType.ShortBreak;
                
                case SessionType.ShortBreak:
                case SessionType.LongBreak:
                    return SessionType.Work;
                
                default:
                    return SessionType.Work;
            }
        }

        private TimeSpan GetSessionDuration(SessionType sessionType)
        {
            return sessionType switch
            {
                SessionType.Work => TimeSpan.FromMinutes(_settings.WorkSessionMinutes),
                SessionType.ShortBreak => TimeSpan.FromMinutes(_settings.ShortBreakMinutes),
                SessionType.LongBreak => TimeSpan.FromMinutes(_settings.LongBreakMinutes),
                _ => TimeSpan.FromMinutes(_settings.WorkSessionMinutes)
            };
        }

        private void ShowSessionCompletionNotification()
        {
            var sessionName = _currentSessionType switch
            {
                SessionType.Work => "Work Session",
                SessionType.ShortBreak => "Short Break",
                SessionType.LongBreak => "Long Break",
                _ => "Session"
            };

            var title = "Pomodoro Timer";
            var message = $"{sessionName} completed! ";
            
            if (_currentSessionType == SessionType.Work)
            {
                var nextSession = GetNextSessionType();
                var nextSessionName = nextSession switch
                {
                    SessionType.ShortBreak => "Time for a short break!",
                    SessionType.LongBreak => "Time for a long break!",
                    _ => "Great work!"
                };
                message += nextSessionName;
            }
            else
            {
                message += "Ready to get back to work?";
            }

            // 音声とデスクトップ通知
            _notificationService.PlayNotificationSound();
            _notificationService.ShowDesktopNotification(title, message);
        }
    }
}
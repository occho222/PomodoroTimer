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
        private AppSettings _settings;
        private TimeSpan _remainingTime;
        private TimeSpan _sessionDuration;
        private SessionType _currentSessionType;
        private int _completedPomodoros;
        private int _currentCycleCount;

        public event Action? TimerStarted;
        public event Action? TimerStopped;
        public event Action? TimerPaused;
        public event Action? TimerResumed;
        public event Action<TimeSpan>? TimeUpdated;
        public event Action<SessionType>? SessionCompleted;
        public event Action<SessionType>? SessionTypeChanged;

        public bool IsRunning => _timer?.IsEnabled ?? false;
        public TimeSpan RemainingTime => _remainingTime;
        public TimeSpan SessionDuration => _sessionDuration;
        public SessionType CurrentSessionType => _currentSessionType;
        public int CompletedPomodoros => _completedPomodoros;

        public TimerService()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
            
            _settings = new AppSettings();
            _currentSessionType = SessionType.Work;
            _remainingTime = TimeSpan.FromMinutes(_settings.WorkSessionMinutes);
            _sessionDuration = _remainingTime;
            _completedPomodoros = 0;
            _currentCycleCount = 0;
        }

        public void Start(TimeSpan duration)
        {
            _sessionDuration = duration;
            _remainingTime = duration;
            _timer.Start();
            TimerStarted?.Invoke();
        }

        public void StartNewPomodoroCycle()
        {
            _currentSessionType = SessionType.Work;
            var duration = TimeSpan.FromMinutes(_settings.WorkSessionMinutes);
            _sessionDuration = duration;
            _remainingTime = duration;
            _timer.Start();
            TimerStarted?.Invoke();
            SessionTypeChanged?.Invoke(_currentSessionType);
        }

        public void Stop()
        {
            _timer.Stop();
            _remainingTime = _sessionDuration;
            TimerStopped?.Invoke();
        }

        public void Pause()
        {
            _timer.Stop();
            TimerPaused?.Invoke();
        }

        public void Resume()
        {
            _timer.Start();
            TimerResumed?.Invoke();
        }

        public void Skip()
        {
            _timer.Stop();
            OnSessionCompleted();
        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            // 現在停止中の場合は新しい設定を適用
            if (!IsRunning)
            {
                var duration = _currentSessionType switch
                {
                    SessionType.Work => TimeSpan.FromMinutes(_settings.WorkSessionMinutes),
                    SessionType.ShortBreak => TimeSpan.FromMinutes(_settings.ShortBreakMinutes),
                    SessionType.LongBreak => TimeSpan.FromMinutes(_settings.LongBreakMinutes),
                    _ => TimeSpan.FromMinutes(_settings.WorkSessionMinutes)
                };
                
                _sessionDuration = duration;
                _remainingTime = duration;
                TimeUpdated?.Invoke(_remainingTime);
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
            TimeUpdated?.Invoke(_remainingTime);

            if (_remainingTime <= TimeSpan.Zero)
            {
                _timer.Stop();
                OnSessionCompleted();
            }
        }

        private void OnSessionCompleted()
        {
            var completedSessionType = _currentSessionType;
            SessionCompleted?.Invoke(completedSessionType);

            if (_currentSessionType == SessionType.Work)
            {
                _completedPomodoros++;
                _currentCycleCount++;

                // 次のセッションタイプを決定
                if (_currentCycleCount % _settings.LongBreakInterval == 0)
                {
                    _currentSessionType = SessionType.LongBreak;
                    _sessionDuration = TimeSpan.FromMinutes(_settings.LongBreakMinutes);
                }
                else
                {
                    _currentSessionType = SessionType.ShortBreak;
                    _sessionDuration = TimeSpan.FromMinutes(_settings.ShortBreakMinutes);
                }
            }
            else
            {
                // 休憩から作業セッションに戻る
                _currentSessionType = SessionType.Work;
                _sessionDuration = TimeSpan.FromMinutes(_settings.WorkSessionMinutes);
            }

            _remainingTime = _sessionDuration;
            SessionTypeChanged?.Invoke(_currentSessionType);
            TimeUpdated?.Invoke(_remainingTime);
        }

        public void AddTime(TimeSpan timeToAdd)
        {
            _remainingTime = _remainingTime.Add(timeToAdd);
            
            // 残り時間が負の値にならないようにする
            if (_remainingTime < TimeSpan.Zero)
            {
                _remainingTime = TimeSpan.Zero;
            }
            
            // セッション合計時間も更新
            _sessionDuration = _sessionDuration.Add(timeToAdd);
            if (_sessionDuration < TimeSpan.Zero)
            {
                _sessionDuration = TimeSpan.Zero;
            }
            
            TimeUpdated?.Invoke(_remainingTime);
        }
    }
}
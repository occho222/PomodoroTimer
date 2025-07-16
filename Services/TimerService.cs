using System.Windows.Threading;

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

        public event Action? TimerStarted;
        public event Action? TimerStopped;
        public event Action? TimerPaused;
        public event Action? TimerResumed;
        public event Action<TimeSpan>? TimeUpdated;
        public event Action? SessionCompleted;

        public bool IsRunning => _isRunning;
        public TimeSpan RemainingTime => _remainingTime;
        public TimeSpan SessionDuration => _sessionDuration;

        public TimerService()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
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
            
            SessionCompleted?.Invoke();
            TimerStopped?.Invoke();
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
                SessionCompleted?.Invoke();
            }
        }
    }
}
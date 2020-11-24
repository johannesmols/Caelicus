using System;
using System.Timers;

namespace SimulationCore.Services
{
    public class TimerService
    {
        private Timer _timer;

        public void SetTimer(double interval, bool repeat)
        {
            _timer = new Timer(interval);
            _timer.Elapsed += NotifyTimerElapsed;
            _timer.Enabled = true;
            _timer.AutoReset = repeat;
        }

        public event Action OnElapsed;

        private void NotifyTimerElapsed(object source, ElapsedEventArgs e)
        {
            OnElapsed?.Invoke();
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}

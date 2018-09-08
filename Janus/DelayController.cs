using System;
using System.Windows.Threading;

namespace Janus
{
    public class DelayController
    {
        private readonly DispatcherTimer _timer;
        public bool Running => _timer.IsEnabled;
        public Action DelayedAction;
        public TimeSpan Delay
        {
            get => _timer.Interval;
            set
            {
                var reset = false;
                if (_timer.IsEnabled)
                {
                    reset = true;
                    _timer.Stop();
                }

                _timer.Interval = value;

                if (reset)
                    _timer.Start();
            }
        }

        public DelayController(TimeSpan delay, Action action)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += (_, __) =>
            {
                Logging.WriteLine("Enacting delayed action.");
                action();
            };

            Delay = delay;
        }

        public void ResetTimer()
        {
            Logging.WriteLine("Resetting delay.");
            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
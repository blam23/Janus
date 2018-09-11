using System;
using System.Windows.Threading;

namespace Janus
{
    public class DelayController
    {
        private readonly DispatcherTimer _timer;
        public bool Running => _timer.IsEnabled;
        public Action DelayedAction;

        public event Action DelayActionStarting;
        public event Action DelayActionCompleted;
        public event Action DelayReset;

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
            Delay = delay;
            DelayedAction = action;

            _timer.Tick += (_, __) =>
            {
                EnactDelayedAction();
            };
        }

        private void EnactDelayedAction()
        {
            Logging.WriteLine("Enacting delayed action.");
            OnDelayActionStarting();
            DelayedAction();
            OnDelayActionCompleted();
        }

        public void ResetTimer()
        {
            Logging.WriteLine("Resetting delay.");
            _timer.Stop();
            _timer.Start();
            OnDelayReset();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        protected virtual void OnDelayActionStarting()
        {
            DelayActionStarting?.Invoke();
        }

        protected virtual void OnDelayActionCompleted()
        {
            DelayActionCompleted?.Invoke();
        }

        protected virtual void OnDelayReset()
        {
            DelayReset?.Invoke();
        }

        internal void EnactNow()
        {
            Stop();
            EnactDelayedAction();
        }
    }
}
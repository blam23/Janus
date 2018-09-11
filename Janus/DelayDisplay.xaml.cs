using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Janus
{
    /// <summary>
    /// Interaction logic for DelayDisplay.xaml
    /// </summary>
    public partial class DelayDisplay : Window
    {
        private static readonly List<DelayDisplay> List = new List<DelayDisplay>();
        private readonly DispatcherTimer _refreshTimer;
        private DispatcherTimer _fadeTimer;
        private DateTime _started;
        private DateTime _endTime;
        private TimeSpan _delayDuration;
        private DelayController _controller;

        public DelayDisplay()
        {
            InitializeComponent();

            _refreshTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(10)};
            _refreshTimer.Tick += (_,__) => UpdateProgress();
        }

        private void UpdateProgress()
        {
            var progress = (DateTime.Now - _started).TotalMilliseconds / _delayDuration.TotalMilliseconds;
            pbProgress.Value = progress * 100;
        }

        public event Action<DelayDisplay> Complete;

        public static DelayDisplay CreateNewDelayDisplay()
        {
            return new DelayDisplay();
        }

        public void SetupDelay(DelayController controller)
        {
            _controller = controller;
            _delayDuration = controller.Delay;
            controller.DelayReset += OnDelayReset;
            controller.DelayActionStarting += () =>
            {
                pbProgress.Foreground = new SolidColorBrush(Color.FromRgb(255,255,100));
            };
            controller.DelayActionCompleted += () =>
            {
                _refreshTimer.Stop();
                pbProgress.Value = 100;
                pbProgress.Foreground = new SolidColorBrush(Color.FromRgb(100, 255, 100));
                //var delay = new DelayController(TimeSpan.FromSeconds(1), Hide);
                //delay.ResetTimer();
                _fadeTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(10)};
                _fadeTimer.Tick += (_, __) =>
                {
                    Opacity -= 0.01;
                    if (Opacity > 0.01) return;
                    _fadeTimer.Stop();
                    Hide();
                };
                _fadeTimer.Start();
            };
        }

        private void OnDelayReset()
        {
            _started = DateTime.Now;
            _endTime = DateTime.Now + _delayDuration;
            if (!_refreshTimer.IsEnabled) _refreshTimer.Start();
        }

        public new void Hide()
        {
            lock (List)
            {
                List.Remove(this);
                base.Hide();
            }
        }

        public new void Show()
        {
            lock (List)
            {
                _fadeTimer?.Stop();
                Opacity = 1;
                pbProgress.Foreground = new SolidColorBrush(Color.FromRgb(0, 232, 255));

                if (List.Contains(this)) return;

                List.Add(this);
                var desktopWorkingArea = SystemParameters.WorkArea;
                Left = desktopWorkingArea.Right - Width - 10;
                Top = desktopWorkingArea.Bottom - (Height * List.Count) - 10;
                base.Show();
            }
        }

        protected virtual void OnComplete()
        {
            Complete?.Invoke(this);
        }

        public void SetFileCount(int i)
        {
            lblFileCount.Content = i.ToString();
        }

        private void btnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            _controller.EnactNow();
        }
    }
}

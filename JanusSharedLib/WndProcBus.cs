using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using WindProcFunc = System.Func<System.IntPtr, System.IntPtr, bool>;

namespace JanusSharedLib
{
    /// <summary>
    /// WPF does not allow Windows to override WndProc as WinForms does.
    /// This class therefore requires a WPF Window handle, which is used
    /// to get the internal WndProc handler.
    /// </summary>
    public class WndProcBus
    {
        public IntPtr WindowHandle { get; private set; }
        public bool Started { get; private set; }

        private readonly Dictionary<int, WindProcFunc> _handlerMap = new Dictionary<int, WindProcFunc>();

        public void Init(Window window)
        {
            WindowHandle = new WindowInteropHelper(window).Handle;
            var source = HwndSource.FromHwnd(WindowHandle);

            if (source == null)
            {
                Logging.WriteLine("Unable to start WndProcBus.");
                return;
            }

            source.AddHook(WndProc);
            Started = true;
        }

        public void Register(int message, WindProcFunc handler)
        {
            if (_handlerMap.ContainsKey(message))
            {
                throw new ArgumentException("WndProcBus already registered.", nameof(message));
            }

            _handlerMap[message] = handler;
        }

        public void Unregister(int message)
        {
            _handlerMap.Remove(message);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var ret = IntPtr.Zero;

            if (_handlerMap.TryGetValue(msg, out var func))
            {
                handled = func(wParam, lParam);
            }

            return ret;
        }
    }
}
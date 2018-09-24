using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using WindProcFunc = System.Func<System.IntPtr, System.IntPtr, bool>;

namespace JanusSharedLib
{
    /// <summary>
    /// WPF does not allow Windows to override WndProc as WinForms does.
    /// This class could be made non-static and initialised with the Window.
    /// For now leaving static as there's no need for looking for Windows messages
    ///  outside of the global hot key code.
    /// </summary>
    public static class WndProcBus
    {
        public static IntPtr MainWindowHandle { get; private set; }
        private static HwndSource _source;
        public static bool Started { get; private set; }
        private static readonly Dictionary<int, WindProcFunc> HandlerMap = new Dictionary<int, WindProcFunc>();

        public static void Init(Window window)
        {
            MainWindowHandle = new WindowInteropHelper(window).Handle;
            _source = HwndSource.FromHwnd(MainWindowHandle);

            if (_source == null)
            {
                Logging.WriteLine("Unable to start WndProcBus.");
                return;
            }

            _source.AddHook(WndProc);
            Started = true;
        }

        public static void Register(int message, WindProcFunc handler)
        {
            if (HandlerMap.ContainsKey(message))
            {
                throw new ArgumentException("WndProcBus already registered.", nameof(message));
            }

            HandlerMap[message] = handler;
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var ret = IntPtr.Zero;

            if (HandlerMap.TryGetValue(msg, out var func))
            {
                handled = func(wParam, lParam);
            }

            return ret;
        }
    }
}
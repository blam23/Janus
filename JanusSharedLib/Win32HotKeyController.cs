using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace JanusSharedLib
{
    /// <summary>
    /// Used to keep register, keep track and unregsiter global hotkeys.
    /// Requires a WndProcBus to register for WmHotKey events.
    /// </summary>
    public class Win32HotKeyController : IDisposable
    {
        #region PINVOKE

        // Get references for the required native win32 functions

        // https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-registerhotkey
        [DllImport("user32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hwnd, int id, uint fsModifiers, uint vk);

        // https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-unregisterhotkey
        [DllImport("user32", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);

        // Consts

        [Flags]
        public enum Modifiers : uint
        {
            Alt = 0x0001,
            Ctrl = 0x0002,
            Shift = 0x0004,
            WinKey = 0x0008,
            NoRepeat = 0x4000,
        }

        #endregion

        private const int WmHotkey = 0x312;

        private readonly Dictionary<int, Win32HotKey> _registeredKeys = new Dictionary<int, Win32HotKey>();
        private int _registrationId;

        private WndProcBus _bus;

        public void RegisterForEvents(WndProcBus bus)
        {
            _bus = bus;
            if (!_bus.Started)
            {
                throw new InvalidOperationException("WndProcBus not inititialised.");
            }

            _bus.Register(WmHotkey, (wparam, lparam) =>
            {
                Action action;

                lock (_registeredKeys)
                {
                    if (!_registeredKeys.TryGetValue((int)wparam, out var hotkey))
                    {
                        return false;
                    }

                    action = hotkey.Func;
                }

                action();
                return true;
            });
        }

        public bool Register(Win32HotKey hotkey)
        {
            var atom = Interlocked.Increment(ref _registrationId);
            lock (_registeredKeys)
            {
                var ret = RegisterHotKey(_bus.WindowHandle, atom, (uint)hotkey.Modifiers, hotkey.KeyId);

                if (ret)
                {
                    _registeredKeys[atom] = hotkey;
                    hotkey.Registered = true;
                    Logging.WriteLine($"Successfully registered hotkey:'{hotkey.KeyId}+{hotkey.Modifiers}' with id {atom}.");
                    hotkey.Atom = atom;
                    return true;
                }

                Logging.WriteLine(
                    $"Unable to register hotkey id {atom}; key:'{hotkey.KeyId}+{hotkey.Modifiers}', last error: {Marshal.GetLastWin32Error()}");

                return false;
            }
        }

        public void Unregister(Win32HotKey hotkey)
        {
            if (!hotkey.Registered || hotkey.Atom == 0) return;

            var ret = UnregisterHotKey(_bus.WindowHandle, hotkey.Atom);

            lock (_registeredKeys)
            {
                _registeredKeys.Remove(hotkey.Atom);
            }

            Logging.WriteLine(
                ret == 0
                    ? $"Unregistered hotkey id {hotkey.Atom} succesfully; key:'{hotkey.KeyId}+{hotkey.Modifiers}'."
                    : $"Unable to unregister hotkey id {hotkey.Atom}; key:'{hotkey.KeyId}+{hotkey.Modifiers}', last error: {Marshal.GetLastWin32Error()}"
            );
        }

        public void Dispose()
        {
            foreach (var kvp in _registeredKeys)
            {
                Unregister(kvp.Value);
            }

            _bus.Unregister(WmHotkey);
        }
    }
}
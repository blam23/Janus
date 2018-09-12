using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Janus
{
    internal class Win32HotKey : IDisposable
    {
        private static int _registrationId = 0;

        private bool _registered;
        private readonly uint _keyId;
        private readonly Modifiers _modifiers;
        private readonly int _atom;

        private static readonly Dictionary<int, Action> RegisteredKeys = new Dictionary<int, Action>();
        private const int WmHotkey = 0x312;

        public static void RegisterForEvents()
        {
            if (!WndProcBus.Started)
            {
                throw new InvalidOperationException("WndProcBus not inititialised.");
            }

            WndProcBus.Register(WmHotkey, (wparam, lparam) =>
            {
                if (!RegisteredKeys.TryGetValue((int) wparam, out var action))
                {
                    return false;
                }

                action();
                return true;
            });
        }

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
            Alt      = 0x0001,
            Ctrl     = 0x0002,
            Shift    = 0x0004,
            WinKey   = 0x0008,
            NoRepeat = 0x4000,
        }

        #endregion

        public Win32HotKey(Modifiers modifiers, uint keyId)
        {
            // Don't need to use atom API as this it's registed per Window Handle
            //  and therefore has it's own ID space.
            _atom = Interlocked.Increment(ref _registrationId);

            _modifiers = modifiers;
            _keyId = keyId;
        }

        public bool Register(Action func)
        {
            lock (RegisteredKeys)
            {
                _registered = RegisterHotKey(WndProcBus.MainWindowHandle, _atom, (uint) _modifiers, _keyId);

                if (_registered)
                {
                    RegisteredKeys[_atom] = func;
                    Logging.WriteLine($"Successfully registered hotkey:'{_keyId}+{_modifiers}' with id {_atom}.");
                }
                else
                {
                    Logging.WriteLine(
                        $"Unable to register hotkey id {_atom}; key:'{_keyId}+{_modifiers}', last error: {Marshal.GetLastWin32Error()}");
                }

                return _registered;
            }
        }

        public void Dispose()
        {
            Unregister();
        }

        private void Unregister()
        {
            if (!_registered || _atom == 0) return;

            var ret = UnregisterHotKey(WndProcBus.MainWindowHandle, _atom);
            lock (RegisteredKeys)
            {
                RegisteredKeys.Remove(_atom);
            }

            if (ret != 0)
            {
                Logging.WriteLine(
                    $"Unable to unregister hotkey id {_atom}; key:'{_keyId}+{_modifiers}', last error: {Marshal.GetLastWin32Error()}");
            }
        }
    }
}

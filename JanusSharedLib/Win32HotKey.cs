using System;

namespace JanusSharedLib
{
    /// <summary>
    /// Stores data about a HotKey.
    /// Used by Win32HotKeyController.
    /// </summary>
    public class Win32HotKey
    {
        public readonly uint KeyId;
        public readonly Win32HotKeyController.Modifiers Modifiers;
        public readonly Action Func;

        public int Atom;
        public bool Registered;

        public Win32HotKey(Win32HotKeyController.Modifiers modifiers, uint keyId, Action func)
        {
            Modifiers = modifiers;
            KeyId = keyId;
            Func = func;
        }
    }
}

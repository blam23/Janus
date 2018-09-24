using System;

namespace JanusSharedLib
{
    /// <summary>
    /// Stores data about a HotKey.
    /// Used by Win32HotKeyController.
    /// </summary>
    public class Win32HotKey
    {
        public uint KeyId { get; }
        public Win32HotKeyController.Modifiers Modifiers { get; }
        public Action Func { get; }

        public int Atom { get; set; }
        public bool Registered { get; set; }

        public Win32HotKey(Win32HotKeyController.Modifiers modifiers, uint keyId, Action func)
        {
            Modifiers = modifiers;
            KeyId = keyId;
            Func = func;
        }
    }
}

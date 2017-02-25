using System;
using System.Diagnostics;
using System.Threading;

namespace Janus
{
    public static class Logging
    {
        private static readonly int ProcessId = Process.GetCurrentProcess().Id;
        public static string Catagory => $"{DateTime.Now.ToShortDateString()}-{DateTime.Now.ToLongTimeString()}-{ProcessId}-{Thread.CurrentThread.ManagedThreadId}";
        public static void WriteLine(string x)
        {
            Trace.WriteLine(x, Catagory);
        }

        public static void WriteLine(object x)
        {
            WriteLine(x.ToString());
        }

        public static void WriteLine(string x, params object[] objects)
        {
            WriteLine(string.Format(x, objects));
        }
    }
}

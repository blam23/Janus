using System;
using System.IO;
using System.Security.AccessControl;

namespace Janus
{
    class DataStore
    {
        public static long Version = 0x0;
        public static readonly char[] HeaderBytes = { (char)0x04, 'j', 'w', (char)0x23};
        public static readonly char[] Copy = { (char)0x03, 'a', 'd', 'd' };
        public static readonly char[] Delete = { (char)0x02, 'r', 'm' };
        private const string StoreName = ".watch";

        public static void Store(Watcher watcher)
        {
            var storeFile = Path.Combine(watcher.WatchPath, StoreName);

            var fileInfo = new FileInfo(storeFile)
            {
                Attributes = FileAttributes.Normal
            };

            using (var fs = File.Create(storeFile))
            {
                // TODO: BinaryWriter
            }

            // Make file hidden
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
        }

        public static Watcher Load(string path)
        {
            var storeFile = Path.Combine(path, StoreName);

            var fileInfo = new FileInfo(storeFile)
            {
                Attributes = FileAttributes.Normal
            };


            using (var fs = File.OpenRead(storeFile))
            {
                // TODO: BinaryReader
            }

            // Make file hidden
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;

            return null;
        }
    }
}

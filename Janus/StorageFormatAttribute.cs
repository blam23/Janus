using System;

namespace Janus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StorageFormatAttribute : Attribute
    {
        public long VersionNumber;

        public StorageFormatAttribute(long versionNumber)
        {
            VersionNumber = versionNumber;
        }
    }
}

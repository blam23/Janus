using System;

namespace Janus
{
    /// <summary>
    /// The DataStore class uses this via Reflection to detect all
    ///  IDataStorageFormat classes it can load from a DLL
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class StorageFormatAttribute : Attribute
    {
        public readonly long VersionNumber;

        public StorageFormatAttribute(long versionNumber)
        {
            VersionNumber = versionNumber;
        }
    }
}

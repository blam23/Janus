using System.IO;

namespace Janus
{
    /// <summary>
    /// Generic interface for DataStorage.
    /// These should be implemented in the StorageFormats project,
    ///  this is to help maintain backwards compatibility when
    ///  upgrading.
    /// </summary>
    public interface IDataStorageFormat
    {
        JanusData Read(BinaryReader reader);
        void Save(BinaryWriter writer, JanusData data);
    }
}

using System.IO;

namespace Janus
{
    public interface IDataStorageFormat
    {
        JanusData Read(BinaryReader reader);
        void Save(BinaryWriter writer, JanusData data);
    }
}

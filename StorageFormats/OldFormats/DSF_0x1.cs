using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Janus;
using Janus.Filters;

namespace StorageFormats.OldFormats
{
    [StorageFormat(0x1)]
    // ReSharper disable once InconsistentNaming
    public class DSF_0x1 : IDataStorageFormat
    {
        private const char Start  = '[';
        private const char End    = ']';
        private const char Switch = '.';
        private const char EoF    = '#';

        public JanusData Read(BinaryReader reader)
        {
            var data = new JanusData();
            var watchMode = true;
            var dataMode = false;

            var startChar = reader.ReadChar();

            if (startChar != Start)
            {
                throw new Exception($"Invalid format. Start expected found: '{startChar}' instead");
            }

            while (watchMode)
            {
                var watchPath = reader.ReadString();
                var endPath = reader.ReadString();
                reader.ReadString(); // filter - unused
                var recursive = reader.ReadBoolean();
                var addFiles = reader.ReadBoolean();
                var deleteFiles = reader.ReadBoolean();
                var endChar = reader.ReadChar();

                if (endChar != End)
                {
                    throw new Exception($"Invalid format. End expected found: '{endChar}' instead");
                }

                var filters = new ObservableCollection<IFilter>();
                data.Watchers.Add(new Watcher(watchPath, watchPath, endPath, addFiles, deleteFiles, filters, recursive));

                var next = reader.ReadChar();

                if (next == Switch)
                {
                    watchMode = false;
                    dataMode = true;
                }
                else if (next != Start)
                {
                    throw new Exception($"Invalid format. Start expected found: '{next}' instead");
                }
            }

            startChar = reader.ReadChar();

            if (startChar != Start)
            {
                if (startChar == EoF)
                {
                    return data;
                }
                throw new Exception($"Invalid format. Start expected found: '{startChar}' instead");
            }

            while (dataMode)
            {


                var key = reader.ReadString();
                var type = reader.ReadChar();
                object value;
                switch (type)
                {
                    case 's':
                        value = reader.ReadString();
                        break;
                    case 'i':
                        value = reader.ReadInt32();
                        break;
                    case 'd':
                        value = reader.ReadDouble();
                        break;
                    case 'b':
                        value = reader.ReadBoolean();
                        break;
                    default:
                        throw new Exception($"Invalid format. Unknown DataType: '{type}' instead");
                }

                data.DataProvider.Add(key, value);

                var next = reader.ReadChar();

                if (next == EoF)
                {
                    dataMode = false;
                }
                else if (next != Start)
                {
                    throw new Exception($"Invalid format. Start expected found: '{next}' instead");
                }
            }

            return data;
        }

        public void Save(BinaryWriter writer, JanusData data)
        {
            var watchers = data.Watchers;

            foreach (var watcher in watchers)
            {
                writer.Write(Start);
                writer.Write(watcher.Data.WatchDirectory);
                writer.Write(watcher.Data.SyncDirectory);
                writer.Write("*");
                writer.Write(watcher.Data.Recursive);
                writer.Write(watcher.Data.AutoAddFiles);
                writer.Write(watcher.Data.AutoDeleteFiles);
                writer.Write(End);
            }

            writer.Write(Switch);

            foreach (var kvp in data.DataProvider.Data)
            {
                writer.Write(Start);
                writer.Write(kvp.Key);
                if (kvp.Value is string)
                {
                    writer.Write('s');
                    writer.Write((string)kvp.Value);
                }
                else if (kvp.Value is int)
                {
                    writer.Write('i');
                    writer.Write((int)kvp.Value);
                }
                else if (kvp.Value is double)
                {
                    writer.Write('d');
                    writer.Write((double)kvp.Value);
                }
                else if (kvp.Value is bool)
                {
                    writer.Write('b');
                    writer.Write((bool)kvp.Value);
                }
                writer.Write(End);
            }

            writer.Write(EoF);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Janus;
using Janus.Filters;

namespace StorageFormats.OldFormats
{
    /* CHANGELOG FROM 0x2:
     * 
     *  Added support for IFilter storage.
     *  
     *  Added by: Elliot
     */

    [StorageFormat(0x3)]
    // ReSharper disable once InconsistentNaming
    public class DSF_0x3 : IDataStorageFormat
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
                if (startChar == Switch)
                {
                    watchMode = false;
                    dataMode = true;
                }
                else
                {
                    throw new Exception($"Invalid format. Start expected found: '{startChar}' instead");
                }
            }

            while (watchMode)
            {
                var watchPath = reader.ReadString();
                var endPath = reader.ReadString();
                var filterCount = reader.ReadInt32();
                var filters = new ObservableCollection<IFilter>();
                for (var i = 0; i < filterCount; i++)
                {
                    reader.ReadUInt32(); // Filter Behaviour - Unused.
                    var type = reader.ReadString();
                    switch (type)
                    {
                        case "EFF":
                            var patterns = ExtractPatterns(reader);
                            filters.Add(new ExcludeFileFilter(patterns));
                            break;
                        case "EF":
                            patterns = ExtractPatterns(reader);
                            filters.Add(new ExcludeFilter(patterns));
                            break;
                        case "IF":
                            patterns = ExtractPatterns(reader);
                            filters.Add(new IncludeFilter(patterns));
                            break;
                        default:
                            throw new Exception($"Invalid format. Unknown filter: '{type}' found.");
                    }
                }
                var recursive = reader.ReadBoolean();
                var addFiles = reader.ReadBoolean();
                var deleteFiles = reader.ReadBoolean();
                var observe = reader.ReadBoolean();
                var endChar = reader.ReadChar();

                if (endChar != End)
                {
                    throw new Exception($"Invalid format. End expected found: '{endChar}' instead");
                }

                data.Watchers.Add(new Watcher(watchPath, watchPath, endPath, addFiles, deleteFiles, filters, recursive, observe));

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

                var endChar = reader.ReadChar();
                if (endChar != End)
                {
                    throw new Exception($"Invalid format. End expected found: '{endChar}' instead");
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

        private static string[] ExtractPatterns(BinaryReader reader)
        {
            var patternCount = reader.ReadInt32();
            var patterns = new string[patternCount];
            for (var j = 0; j < patternCount; j++)
            {
                patterns[j] = reader.ReadString();
            }
            return patterns;
        }

        public void Save(BinaryWriter writer, JanusData data)
        {
            var watchers = data.Watchers;

            foreach (var watcher in watchers)
            {
                writer.Write(Start);
                writer.Write(watcher.Data.WatchDirectory);
                writer.Write(watcher.Data.SyncDirectory);
                if (watcher.Data.Filters == null)
                {
                    writer.Write(0);
                }
                else
                {
                    writer.Write(watcher.Data.Filters.Count);
                    foreach (var filter in watcher.Data.Filters)
                    {
                        writer.Write((uint)filter.Behaviour);
                        var excludeFilter = filter as ExcludeFilter;
                        var excludeFileFilter = filter as ExcludeFileFilter;
                        var includeFilter = filter as IncludeFilter;
                        if (excludeFilter != null)
                        {
                            writer.Write("EF");
                            writer.Write(excludeFilter.Filters.Count);
                            foreach (var pattern in excludeFilter.Filters)
                            {
                                writer.Write(pattern);
                            }
                        }
                        else if (excludeFileFilter != null)
                        {
                            writer.Write("EFF");
                            writer.Write(excludeFileFilter.Filters.Count);
                            foreach (var pattern in excludeFileFilter.Filters)
                            {
                                writer.Write(pattern);
                            }
                        }
                        else if (includeFilter != null)
                        {
                            writer.Write("IF");
                            writer.Write(includeFilter.Filters.Count);
                            foreach (var pattern in includeFilter.Filters)
                            {
                                writer.Write(pattern);
                            }
                        }
                        else
                        {
                            writer.Write("???");
                        }
                    }
                }
                writer.Write(watcher.Data.Recursive);
                writer.Write(watcher.Data.AutoAddFiles);
                writer.Write(watcher.Data.AutoDeleteFiles);
                writer.Write(watcher.Observe);
                writer.Write(End);
            }

            writer.Write(Switch);

            foreach (var kvp in data.DataProvider.Data)
            {
                writer.Write(Start);
                writer.Write(kvp.Key);
                // TODO: Change to the nice new C#7 Pattern Match Switch statement when supported.
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

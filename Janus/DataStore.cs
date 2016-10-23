using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Janus
{
    static class DataStore
    {
        public static readonly string AppData      = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string DataLocation = Path.Combine(AppData, Assembly.GetEntryAssembly().GetName().Name);

        /// <summary>
        /// Maps Version numbers to data loaders.
        /// Enables backwards compatibility.
        /// </summary>
        private static readonly Dictionary<long, IDataStorageFormat> DataLoaders = new Dictionary<long, IDataStorageFormat>();
        private static readonly string LoaderName = "StorageFormats.dll";

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                Console.WriteLine(path);
                return Path.GetDirectoryName(path);
            }
        }

        private static readonly string LoaderLocation = Path.Combine(AssemblyDirectory, LoaderName);
        private const long Version = 0x1;

        private static readonly byte[] HeaderBytes = { 0x04, (byte)'j', (byte)'w', 0x23};
        public  static readonly char[] Copy        = { (char)0x03, 'a', 'd', 'd' };
        public  static readonly char[] Delete      = { (char)0x02, 'r', 'm' };
        private static readonly string StoreName   = Path.Combine(DataLocation, "watchdata");

        public static void Initialise()
        {
            Console.WriteLine(AssemblyDirectory);
            Console.WriteLine(LoaderLocation);
            var a = Assembly.UnsafeLoadFrom(LoaderLocation);
            foreach (var t in a.GetTypes())
            {
                foreach (var attr in t.GetCustomAttributes(true))
                {
                    var formatAttr = attr as StorageFormatAttribute;
                    if (formatAttr == null) continue;
                    var c = (IDataStorageFormat)Activator.CreateInstance(t);
                    DataLoaders.Add(formatAttr.VersionNumber, c);
                }
            }
        }

        public static void Store(JanusData data)
        {
            if (!Directory.Exists(DataLocation))
            {
                Directory.CreateDirectory(DataLocation);
            }

            using (var fs = File.Create(StoreName))
            {
                using (var writer = new BinaryWriter(fs))
                {
                    writer.Write(HeaderBytes);
                    writer.Write(Version);
                    IDataStorageFormat format;
                    if (DataLoaders.TryGetValue(Version, out format))
                    {
                        format.Save(writer, data);
                    }
                    // To get here the user's install must be corrupted.

                    // TODO: Add user dialog prompting a reinstall here
                    // TODO: Check this on startup so we don't only error on save.
                    // TODO: Automatically pull correct IDataStore from server?
                    //        Could apply to loading too. 
                }
            }
        }

        public static JanusData Load()
        {
            if (!Directory.Exists(DataLocation))
            {
                Directory.CreateDirectory(DataLocation);
            }

            if (!File.Exists(StoreName))
            {
                return new JanusData();
            }

            using (var fs = File.OpenRead(StoreName))
            {
                // TODO: BinaryReader
                using (var reader = new BinaryReader(fs))
                {
                    var header = reader.ReadBytes(4);
                    if (!header.SequenceEqual(HeaderBytes))
                    {
                        fs.Close();
                        InvalidDataStore("Invalid header");
                        return new JanusData();
                    }
                    var version = reader.ReadInt64();
                    IDataStorageFormat format;
                    if (DataLoaders.TryGetValue(version, out format))
                    {
                        try
                        {
                            return format.Read(reader);
                        }
                        catch (Exception e)
                        {
                            fs.Close();
                            InvalidDataStore(e.Message);
                        }
                    }
                    fs.Close();
                    InvalidDataStore("Unsupported format");
                    return new JanusData();
                }
            }
        }

        private static void InvalidDataStore(string message)
        {
            Debug.WriteLine($"Invalid DataStore file found: '{message}'; removing file.");
            File.Delete(StoreName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Janus
{
    public class DataStore
    {
        public DataStore(string pathOverride = null)
        {
            DataLocation   = pathOverride ?? Path.Combine(AppData, Assembly.GetEntryAssembly().GetName().Name);
            DataLocation   = Path.GetFullPath(DataLocation);
            LoaderLocation = Path.Combine(AssemblyDirectory, LoaderName);
            StoreName      = Path.Combine(DataLocation, "watchdata");
        }

        public static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public readonly string DataLocation;

        /// <summary>
        /// Maps Version numbers to data loaders.
        /// Enables backwards compatibility.
        /// </summary>
        public readonly Dictionary<long, IDataStorageFormat> DataLoaders = new Dictionary<long, IDataStorageFormat>();
        private readonly string LoaderName = "StorageFormats.dll";

        public string AssemblyDirectory
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

        public readonly string LoaderLocation;
        public const long Version = 0x2;

        private readonly byte[] HeaderBytes = { 0x04, (byte)'j', (byte)'w', 0x23};
        private readonly char[] Copy        = { (char)0x03, 'a', 'd', 'd' };
        private readonly char[] Delete      = { (char)0x02, 'r', 'm' };
        private readonly string StoreName;

        public void Initialise()
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

        public void Store(JanusData data)
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

        public JanusData Load()
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

        private void InvalidDataStore(string message)
        {
            Console.WriteLine($"Invalid DataStore file found: '{message}'; removing file.");
            File.Delete(StoreName);
        }
    }
}

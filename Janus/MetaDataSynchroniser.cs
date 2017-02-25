using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Janus.Properties;

#if MD5_CHECK
using System.Security.Cryptography;
using System.Text;
#endif

namespace Janus
{
    /// <summary>
    /// Handles all of the actual file synchronisation functionality
    /// </summary>
    public class MetaDataSynchroniser : ISynchroniser
    {
        public SyncData Data { get; set; }

        public MetaDataSynchroniser(SyncData data)
        {
            Data = data;

        }

#if MD5_CHECK
        // TODO: Move this to A HashSynchroniser class
        public string CalculateMD5(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }
#endif

        public async Task TryFullSynchroniseAsync()
        {
            await Task.Run(new Action(TryFullSynchronise));
        }

        /// <summary>
        /// Attempts to make the EndPath directory a 1:1 copy of the
        /// Watcher's WatchPath.
        /// </summary>
        public async void TryFullSynchronise()
        {
            if (!Data.AddFiles && !Data.DeleteFiles) return;

            var start = Directory.GetFiles(
                Data.WatchDirectory,
                "*",
                Data.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );

            if (!Directory.Exists(Data.SyncDirectory))
            {
                Directory.CreateDirectory(Data.SyncDirectory);
            }

            var end = Directory.GetFiles(
                Data.SyncDirectory,
                "*",
                Data.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
            );

            var toAdd = new List<string>();
            foreach (var file in start)
            {
                var filtered = false;
                foreach (var filter in Data.Filters)
                {
                    if (!filter.ShouldExcludeFile(file)) continue;
                    filtered = true;
                }
                if (filtered) continue;
                toAdd.Add(file.Substring(Data.WatchDirectory.Length + 1));
            }

            if (Data.AddFiles)
            {
                foreach (var file in toAdd)
                {
                    await AddAsync(file, false);
                }
            }

            if (!Data.DeleteFiles) return;
            var toDelete = new List<string>();
            for (var i = 0; i < end.Length; i++)
            {
                var filtered = false;
                foreach (var filter in Data.Filters)
                {
                    if (!filter.ShouldExcludeFile(toDelete[i])) continue;
                    filtered = true;
                }
                if (filtered) continue;
                end[i] = end[i].Substring(Data.SyncDirectory.Length + 1);
                var match = toAdd.IndexOf(end[i]);
                if (match >= 0)
                {
#if MD5_CHECK
                        if (CalculateMD5(Path.Combine(Watcher.WatchPath, toAdd[match])) == CalculateMD5(Path.Combine(EndPath, end[i])))
                        {
                            toAdd.RemoveAt(match);
                        }
#else
                    var lastWriteA = File.GetLastWriteTime(Path.Combine(Data.WatchDirectory, toAdd[match]));
                    var lastWriteB = File.GetLastWriteTime(Path.Combine(Data.SyncDirectory, end[i]));
                    if (lastWriteA == lastWriteB)
                    {
                        toAdd.RemoveAt(match);
                    }
#endif
                }
                else
                {
                    toDelete.Add(end[i]);
                }
            }

            foreach (var file in toDelete)
            {
                await DeleteAsync(file, false);
            }

            // TODO: Add path sync
        }

        /// <summary>
        /// Adds a file from the WatchPath to the EndPath.
        /// </summary>
        /// <param name="path">The path of the file that you want to add</param>
        /// <param name="isPathFull">If the path is a full path or relative to the WatchPath</param>
        /// <param name="count">Amount of times to retry on failure</param>
        public async Task AddAsync(string path, bool isPathFull = true, int count = 5)
        {
            if (count <= 0) return;
            var partPath = isPathFull ? path.Substring(Data.WatchDirectory.Length+1) : path;
            try
            {
                var endFilePath = Path.Combine(Data.SyncDirectory, partPath);
                var name = Directory.GetParent(endFilePath).FullName;
                if (!Directory.Exists(name))
                {
                    Directory.CreateDirectory(name);
                }
                File.Copy(Path.Combine(Data.WatchDirectory, partPath), endFilePath, true);
            }
            catch (Exception e)
            {
                Logging.WriteLine(Resources.Copy_Error, partPath, e.Message);
                await Task.Delay(300);
                await AddAsync(path, isPathFull, count-1);
            }
        }


        /// <summary>
        /// Removes a file from the EndPath
        /// </summary>
        /// <param name="path">The path of the file that you want to add</param>
        /// <param name="isPathFull">If the path is a full path or relative to the WatchPath</param>
        /// <param name="count">Amount of times to retry on failure</param>
        public async Task DeleteAsync(string path, bool isPathFull = true, int count = 5)
        {
            if (count <= 0) return;
            var partPath = isPathFull ? path.Substring(Data.WatchDirectory.Length+1) : path;
            try
            {
                File.Delete(Path.Combine(Data.SyncDirectory, partPath));
            }
            catch (Exception e)
            {
                Logging.WriteLine(Resources.Delete_Error, partPath, e.Message);
                await Task.Delay(300);
                await DeleteAsync(path, isPathFull, count - 1);
            }
        }

        /// <summary>
        /// Equality Check wrapper.
        /// </summary>
        /// <param name="obj">Object to compare (always false if not Synchroniser type)</param>
        /// <returns>True or false if object is a Synchroniser object with matching properties</returns>
        public override bool Equals(object obj)
        {
            var sobj = obj as MetaDataSynchroniser;
            return sobj != null && Equals(sobj);
        }

        /// <summary>
        /// Equality check.
        /// Used in test.
        /// </summary>
        /// <param name="other">Synchroniser to compare</param>
        /// <returns>True or false if "other" has same peroperties</returns>
        private bool Equals(ISynchroniser other)
        {
            return Equals(Data, other.Data);
        }

        /// <summary>
        /// Calculates a "unique" hash code based on the properties of this object.
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Data.AddFiles.GetHashCode();
                hashCode = (hashCode*397) ^ Data.DeleteFiles.GetHashCode();
                hashCode = (hashCode*397) ^ (Data.SyncDirectory?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#if MD5_CHECK
using System.Security.Cryptography;
using System.Text;
#endif

namespace Janus
{
    public class Sync
    {

        public Sync(string endPath, Watcher parent, bool addFiles, bool deleteFiles)
        {
            EndPath = endPath;
            _addFiles = addFiles;
            _deleteFiles = deleteFiles;
            Watcher = parent;
        }

        public string  EndPath { get; set; }

        private bool _addFiles;
        private bool _deleteFiles;

        /// <summary>
        /// If true files will be automatically added to the EndPath directory
        /// when they are added to the Watcher's WatchPath.
        /// </summary>
        public bool AddFiles
        {
            get { return _addFiles; }
            set
            {
                MainWindow.UpdateStore();
                _addFiles = value;
            }
        }

        /// <summary>
        /// If true files will be automatically deleted from the EndPath directory
        /// when they are removed from the Watcher's WatchPath.
        /// </summary>
        public bool DeleteFiles
        {
            get { return _deleteFiles; }
            set
            {
                MainWindow.UpdateStore();
                _deleteFiles = value;
            }
        }

        /// <summary>
        /// Parent watcher that contains the event logic
        /// </summary>
        public Watcher Watcher { get; set; }

#if MD5_CHECK
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

        /// <summary>
        /// Attempts to make the EndPath directory a 1:1 copy of the
        /// Watcher's WatchPath.
        /// </summary>
        public void TryFullSynchronise()
        {
            if (!AddFiles && !DeleteFiles) return;

            var start = Directory.GetFiles(
                Watcher.WatchPath, 
                Watcher.Filter, 
                Watcher.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
                );

            var end = Directory.GetFiles(
               EndPath,
               Watcher.Filter,
               Watcher.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly
               );

            var toAdd = start.Select(t => t.Substring(Watcher.WatchPath.Length + 1)).ToList();

            if (AddFiles)
            {
                foreach (var file in toAdd)
                {
                    Add(file, false);
                }
            }

            if (DeleteFiles)
            {
                var toDelete = new List<string>();
                for (var i = 0; i < end.Length; i++)
                {
                    end[i] = end[i].Substring(EndPath.Length + 1);
                    var match = toAdd.IndexOf(end[i]);
                    if (match >= 0)
                    {
#if MD5_CHECK
                    if (CalculateMD5(Path.Combine(Watcher.WatchPath, toAdd[match])) == CalculateMD5(Path.Combine(EndPath, end[i])))
                    {
                        toAdd.RemoveAt(match);
                    }
#else
                        var lastWriteA = File.GetLastWriteTime(Path.Combine(Watcher.WatchPath, toAdd[match]));
                        var lastWriteB = File.GetLastWriteTime(Path.Combine(EndPath, end[i]));
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
                    Delete(file, false);
                }
            }

            // TODO: Add path sync
        }

        /// <summary>
        /// Adds a file from the WatchPath to the EndPath.
        /// </summary>
        /// <param name="path">The path of the file that you want to add</param>
        /// <param name="isPathFull">If the path is a full path or relative to the WatchPath</param>
        /// <param name="count">Amount of times to retry on failure</param>
        public async void Add(string path, bool isPathFull = true, int count = 5)
        {
            if (count <= 0) return;
            var partPath = isPathFull ? path.Substring(Watcher.WatchPath.Length+1) : path;
            try
            {
                var endFilePath = Path.Combine(EndPath, partPath);
                var name = Directory.GetParent(endFilePath).FullName;
                if (!Directory.Exists(name))
                {
                    Directory.CreateDirectory(name);
                }
                File.Copy(Path.Combine(Watcher.WatchPath, partPath), endFilePath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while copying file {1}:\n\t", partPath, e.Message);
                await Task.Delay(300);
                Add(path, isPathFull, count-1);
            }
        }


        /// <summary>
        /// Removes a file from the EndPath
        /// </summary>
        /// <param name="path">The path of the file that you want to add</param>
        /// <param name="isPathFull">If the path is a full path or relative to the WatchPath</param>
        /// <param name="count">Amount of times to retry on failure</param>
        public async void Delete(string path, bool isPathFull = true, int count = 5)
        {
            if (count <= 0) return;
            var partPath = isPathFull ? path.Substring(Watcher.WatchPath.Length+1) : path;
            try
            {
                File.Delete(Path.Combine(EndPath, partPath));
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occured while deleting file {1}:\n\t", partPath, e.Message);
                await Task.Delay(300);
                Delete(path, isPathFull, count - 1);
            }
        }

        /// <summary>
        /// Equality Check wrapper.
        /// </summary>
        /// <param name="obj">Object to compare (always false if not Sync type)</param>
        /// <returns>True or false if object is a Sync object with matching properties</returns>
        public override bool Equals(object obj)
        {
            var sobj = obj as Sync;
            return sobj != null && Equals(sobj);
        }

        /// <summary>
        /// Equality check.
        /// Used in test.
        /// </summary>
        /// <param name="other">Sync to compare</param>
        /// <returns>True or false if "other" has same peroperties</returns>
        private bool Equals(Sync other)
        {
            return _addFiles == other._addFiles &&
                _deleteFiles == other._deleteFiles && 
                string.Equals(EndPath, other.EndPath);
        }

        /// <summary>
        /// Calculates a "unique" hash code based on the properties of this object.
        /// </summary>
        /// <returns>A hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _addFiles.GetHashCode();
                hashCode = (hashCode*397) ^ _deleteFiles.GetHashCode();
                hashCode = (hashCode*397) ^ (EndPath?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
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

        public bool AddFiles
        {
            get { return _addFiles; }
            set
            {
                MainWindow.UpdateStore();
                _addFiles = value;
            }
        }

        public bool DeleteFiles
        {
            get { return _deleteFiles; }
            set
            {
                MainWindow.UpdateStore();
                _deleteFiles = value;
            }
        }

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
    }
}
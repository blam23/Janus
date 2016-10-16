using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Janus
{
    public class Watcher
    {
        public string WatchPath { get; }
        public Sync Sync { get; }

        public string Filter { get; }
        public bool Recursive { get; }

        public List<string> Delete = new List<string>();
        public List<string> Copy = new List<string>();

        private FileSystemWatcher _watcher;

        public Watcher(string watchPath, string endPath, bool addFiles, bool deleteFiles, string filter, bool recursive)
        {
            WatchPath = watchPath;

            Sync = new Sync
            {
                EndPath = endPath,
                Watcher = this,
                AddFiles = addFiles,
                DeleteFiles = deleteFiles
            };

            Filter = filter;
            Recursive = recursive;

            _watcher = new FileSystemWatcher
            {
                Path = watchPath,
            };

            _watcher.Changed += Watcher_Changed;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.EnableRaisingEvents = true;
        }

        public Task DoInitialSynchronise()
        {
            return Task.Run(() => Sync.TryFullSynchronise());
        }

        public void Synchronise()
        {
            foreach (var file in Copy) 
            {
                Sync.Add(file);
            }

            foreach (var file in Delete)
            {
                Sync.Delete(file);
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if(Copy.Contains(e.FullPath))
            {
                Copy.Remove(e.FullPath);
            }
            if (Sync.DeleteFiles)
            {
                Sync.Delete(e.FullPath);
            }
            else
            {
                Delete.Add(e.FullPath);
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (Delete.Contains(e.FullPath))
            {
                Delete.Remove(e.FullPath);
            }
            if (Sync.AddFiles)
            {
                Sync.Add(e.FullPath);
            }
            else
            {
                Copy.Add(e.FullPath);
            }
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}

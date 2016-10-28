using System;
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

        public readonly bool Observe = false;

        private FileSystemWatcher _writeWatcher;
        private FileSystemWatcher _deleteWatcher;

        public Watcher(string watchPath, string endPath, bool addFiles, bool deleteFiles, string filter, bool recursive, bool observe = false)
        {
            WatchPath = watchPath;

            Sync = new Sync(endPath, this, addFiles, deleteFiles);

            Filter = filter;
            Recursive = recursive;

            if (observe)
            {
                Observe = true;
                return;
            }

            _writeWatcher = new FileSystemWatcher
            {
                Path = watchPath,
                NotifyFilter = NotifyFilters.LastWrite
            };

            _deleteWatcher = new FileSystemWatcher
            {
                Path = watchPath,
            };


            _writeWatcher.Changed += WriteWatcherChanged;
            _deleteWatcher.Deleted += WriteWatcherDeleted;
            EnableEvents();
        }

        public void EnableEvents()
        {
            _writeWatcher.EnableRaisingEvents = true;
            _deleteWatcher.EnableRaisingEvents = true;
        }

        public Task DoInitialSynchronise()
        {
            return Task.Run(() => Sync.TryFullSynchronise());
        }

        public void Synchronise()
        {
            foreach (var file in Copy) 
            {
                Console.WriteLine("[Manual] Copying: {0}", file);
                Sync.Add(file);
            }

            foreach (var file in Delete)
            {
                Console.WriteLine("[Manual] Deleting: {0}", file);
                Sync.Delete(file);
            }

            Copy.Clear();
            Delete.Clear();
        }


        private void WriteWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            if(Copy.Contains(e.FullPath))
            {
                Console.WriteLine("Removing from copy list: {0}", e.FullPath);
                var succ = Copy.Remove(e.FullPath);
                Console.WriteLine("Removed from copy list? {0}", succ);
            }
            if (Sync.DeleteFiles)
            {
                Console.WriteLine("Deleting: {0}", e.FullPath);
                Sync.Delete(e.FullPath);
            }
            else
            {
                Console.WriteLine("Marking for delete: {0}", e.FullPath);
                Delete.Add(e.FullPath);
            }
        }

        private string _lastPath = "";
        private void WriteWatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (_lastPath == e.FullPath)
            {
                return;
            }
            _lastPath = e.FullPath;
            Console.WriteLine(e.ChangeType);
            if (Delete.Contains(e.FullPath))
            {
                Console.WriteLine("Removing from delete list: {0}", e.FullPath);
                var succ = Delete.Remove(e.FullPath);
                Console.WriteLine("Removed from delete list? {0}", succ);
            }
            if (Sync.AddFiles)
            {
                Console.WriteLine("Copying: {0}", e.FullPath);
                Sync.Add(e.FullPath);
            }
            else
            {
                Console.WriteLine("Marking for copy: {0}", e.FullPath);
                Copy.Add(e.FullPath);
            }
        }

        public void Stop()
        {
            Console.WriteLine("Stopping watcher for '{0}'", WatchPath);
            DisableEvents();
            _writeWatcher.Dispose();
            _writeWatcher.Dispose();
        }

        public void DisableEvents()
        {
            _deleteWatcher.EnableRaisingEvents = false;
            _writeWatcher.EnableRaisingEvents = false;
        }

        public override bool Equals(object obj)
        {
            var wobj = obj as Watcher;
            return wobj != null && Equals(wobj);
        }

        private bool Equals(Watcher other)
        {
            return Observe == other.Observe && 
                string.Equals(WatchPath, other.WatchPath) && 
                Equals(Sync, other.Sync) && 
                string.Equals(Filter, other.Filter) && 
                Recursive == other.Recursive;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Observe.GetHashCode();
                hashCode = (hashCode*397) ^ (WatchPath?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Sync?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Filter?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Recursive.GetHashCode();
                return hashCode;
            }
        }
    }
}

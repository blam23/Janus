using Janus.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Janus
{
    public class Watcher
    {
        /// <summary>
        /// The input path where changes are being made by something else
        /// </summary>
        public string WatchPath { get; }
        public Sync Sync { get; }

        /// <summary>
        /// Filters that will be applied (in order).
        /// </summary>
        public List<IFilter> Filters { get; }

        /// <summary>
        /// If enabled all subdirectories and their files will be copied/deleted.
        /// TODO: Test this
        /// </summary>
        public bool Recursive { get; }

        /// <summary>
        /// List of files that have been deleted in the WatchPath directory.
        /// Used for manual synchronisation.
        /// Will be empty when it's automatic sync.
        /// </summary>
        public List<string> Delete = new List<string>();

        /// <summary>
        /// List of files that have been added to or modified in the WatchPath directory.
        /// Used for manual synchronisation.
        /// Will be empty when it's automatic sync.
        /// </summary>
        public List<string> Copy = new List<string>();

        /// <summary>
        /// This can only be set when instantiating.
        /// This will disable all file watching.
        /// Used for testing, should never be true in normal use.
        /// </summary>
        public readonly bool Observe = false;

        /// <summary>
        /// Watches for file additions + modifications in the WatchPath directory.
        /// This only triggers on "LastWrite" so as to help mitigate double events, 
        ///  one for initial creation and one for when it's written to.
        /// </summary>
        private FileSystemWatcher _writeWatcher;

        /// <summary>
        /// Watches for file deletions in the WatchPath directory.
        /// </summary>
        private FileSystemWatcher _deleteWatcher;

        public Watcher(string watchPath, string endPath, bool addFiles, bool deleteFiles, List<IFilter> filters, bool recursive, bool observe = false)
        {
            WatchPath = watchPath;

            Sync = new Sync(endPath, this, addFiles, deleteFiles);

            Filters = filters;
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

        /// <summary>
        /// Enables events from both FileSystemWatcher classes
        /// (copy + delete)
        /// </summary>
        public void EnableEvents()
        {
            _writeWatcher.EnableRaisingEvents = true;
            _deleteWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Asynchronously does a full synchronisation, making sure the 
        /// WatchPath matches up with the EndPath.
        /// </summary>
        /// <returns>Async Task</returns>
        public Task DoInitialSynchronise()
        {
            return Task.Run(() => Sync.TryFullSynchronise());
        }

        /// <summary>
        /// Called when the user prompts for a manual synchronisation.
        /// It will copy all the files that have been modified + deleted
        /// since we started tracking this session.
        /// </summary>
        public void Synchronise()
        {
            foreach (var file in Copy) 
            {
                Console.WriteLine("[Manual] Copying: {0}", file);
                Sync.AddAsync(file);
            }

            foreach (var file in Delete)
            {
                Console.WriteLine("[Manual] Deleting: {0}", file);
                Sync.DeleteAsync(file);
            }

            Copy.Clear();
            Delete.Clear();
        }

        /// <summary>
        /// Event recieved when a file in WatchPath is deleted
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Event Parameters (contains file path)</param>
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
                Sync.DeleteAsync(e.FullPath);
            }
            else
            {
                Console.WriteLine("Marking for delete: {0}", e.FullPath);
                Delete.Add(e.FullPath);
            }
        }

        /// <summary>
        /// Stores the last path that was seen.
        /// Used for filtering out "double" events.
        /// TODO: Make this a lot better, very hacky!
        /// </summary>
        private string _lastPath = "";

        /// <summary>
        /// Event recieved when a file in WatchPath is modified / created
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Event Parameters (contains file path)</param>
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
                Sync.AddAsync(e.FullPath);
            }
            else
            {
                Console.WriteLine("Marking for copy: {0}", e.FullPath);
                Copy.Add(e.FullPath);
            }
        }

        /// <summary>
        /// Stops all events and cleans up the FileSystemWatcher classes.
        /// After Stop is called the class cannot start watching again.
        /// To disable events temporarily use DisableEvents.
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("Stopping watcher for '{0}'", WatchPath);
            DisableEvents();
            _writeWatcher.Dispose();
            _writeWatcher.Dispose();
        }

        /// <summary>
        /// Stops any events from being raised.
        /// Use EnableEvents to turn events back on.
        /// </summary>
        public void DisableEvents()
        {
            _deleteWatcher.EnableRaisingEvents = false;
            _writeWatcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Equality comparison check wrapper.
        /// Discards any objects that aren't Watchers.
        /// </summary>
        /// <param name="obj">Watcher to compare with</param>
        /// <returns>If object is a Watcher that has equal properties</returns>
        public override bool Equals(object obj)
        {
            var wobj = obj as Watcher;
            return wobj != null && Equals(wobj);
        }

        /// <summary>
        /// Compares properties with specified watcher to see if they are
        /// equal to each other.
        /// Used in tests.
        /// </summary>
        /// <param name="other">Watcher to check against</param>
        /// <returns>If Watcher is equal to this one</returns>
        private bool Equals(Watcher other)
        {
            return Observe == other.Observe && 
                string.Equals(WatchPath, other.WatchPath) && 
                Equals(Sync, other.Sync) && 
                Recursive == other.Recursive;
        }

        /// <summary>
        /// Computes a "unique" hash code for this Watcher.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Observe.GetHashCode();
                hashCode = (hashCode*397) ^ (WatchPath?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Sync?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Recursive.GetHashCode();
                return hashCode;
            }
        }
    }
}

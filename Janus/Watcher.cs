using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Janus.Filters;
using Janus.Properties;
using JanusSharedLib;

namespace Janus
{
    public class Watcher
    {
        public SyncData Data { get; }
        public ISynchroniser Synchroniser { get; }

        /// <summary>
        /// List of files that have been deleted in the WatchPath directory.
        /// Used for manual synchronisation.
        /// Will be empty when it's automatic sync.
        /// </summary>
        private readonly ObservableSet<string> _delete = new ObservableSet<string>();
        public ObservableSet<string> MarkedForDeletion => _delete;

        /// <summary>
        /// List of files that have been added to or modified in the WatchPath directory.
        /// Used for manual synchronisation.
        /// Will be empty when it's automatic sync.
        /// </summary>
        private readonly ObservableSet<string> _copy = new ObservableSet<string>();
        public ObservableSet<string> MarkedForCopy => _copy;

        /// <summary>
        /// List of files that have been added to or modified in the WatchPath directory.
        /// Used for manual synchronisation.
        /// Will be empty when it's automatic sync.
        /// </summary>
        private readonly ObservableSet<(string, string)> _rename = new ObservableSet<(string, string)>();
        public ObservableSet<(string, string)> MarkedForRename => _rename;

        /// <summary>
        /// This can only be set when instantiating.
        /// This will disable all file watching.
        /// Used for testing, should never be true in normal use.
        /// </summary>
        public readonly bool Observe;

        /// <summary>
        /// Watches for file events in the watch directory
        /// </summary>
        private readonly FileSystemWatcher _watcher;

        /// <summary>
        /// Delay Controller, if null -> No delay is used
        /// </summary>
        internal DelayController Delay;

        /// <summary>
        /// Delay Display Window
        /// </summary>
        private DelayDisplay _display;

        /// <summary>
        /// Display name of the Watcher (shown in GUI).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Required to be taken when adding or processing any operation on a file.
        /// Without this the collections can be modified during a Synchronise, causing an exception.
        /// </summary>
        private readonly Mutex _fileOperationMutex = new Mutex();

        public Watcher(string name, string watchPath, string endPath, bool addFiles, bool deleteFiles, ObservableCollection<IFilter> filters, bool recursive, ulong delay = 0, bool observe = false)
        {
            Name = name;
            Data = new SyncData
            {
                AutoAddFiles = addFiles,
                AutoDeleteFiles = deleteFiles,
                Filters = filters,
                Recursive = recursive,
                WatchDirectory = watchPath,
                SyncDirectory = endPath,
                Delay = delay
            };

            Synchroniser = new MetaDataSynchroniser(Data);

            if (observe)
            {
                Observe = true;
                return;
            }

            _watcher = new FileSystemWatcher
            {
                Path = watchPath,
            };

            _watcher.Created += WriteWatcherChanged;   // Required for copied files
            _watcher.Changed += WriteWatcherChanged;
            _watcher.Deleted += WriteWatcherDeleted;
            _watcher.Renamed += WriteWatcherRenamed;

            SetupDelay(delay);

            EnableEvents();
        }

        public void SetupDelay(ulong delay)
        {
            if (delay > 0)
            {
                Delay = new DelayController(TimeSpan.FromMilliseconds(delay), () => Synchronise());

                _display = DelayDisplay.CreateNewDelayDisplay();
                _display.SetupDelay(Delay);
            }
            else
                Delay = null;
        }

        public void AddFilter(IFilter filter)
        {
            Data.Filters.Add(filter);
        }

        /// <summary>
        /// Enables events from both FileSystemWatcher classes
        /// (copy + delete)
        /// </summary>
        private void EnableEvents()
        {
            _watcher.EnableRaisingEvents = true;
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Asynchronously does a full synchronisation, making sure the
        /// WatchPath matches up with the EndPath.
        /// </summary>
        /// <returns>Async Task</returns>
        public Task DoInitialSynchronise() => Synchroniser.TryFullSynchroniseAsync();

        /// <summary>
        /// Called when the user prompts for a manual synchronisation.
        /// It will copy all the files that have been modified + deleted
        /// since we started tracking this session.
        /// </summary>
        public void Synchronise(bool notify = true)
        {
            _fileOperationMutex.WaitOne();
            try
            {
                var copyCount = _copy.Count;
                var renameCount = _rename.Count;
                var deleteCount = _delete.Count;

                if (copyCount + deleteCount + renameCount == 0)
                {
                    if(notify)
                        NotificationSystem.Default.Push(NotifcationType.Info, "Sync Completed.", "No files were changed.");

                    return;
                }

                foreach (var file in _copy)
                {
                    Logging.WriteLine(Resources.Manual_Copying_Target, file);
                    Synchroniser.AddAsync(file);
                }

                foreach (var file in _delete)
                {
                    Logging.WriteLine(Resources.Manual_Deleting_Target, file);
                    Synchroniser.DeleteAsync(file);
                }

                foreach (var file in _rename)
                {
                    Logging.WriteLine("[Manual] Renaming: {0} to {1}", file.Item1, file.Item2);
                    Synchroniser.RenameAsync(file.Item1, file.Item2);
                }

                _copy.Clear();
                _delete.Clear();
                _rename.Clear();

                if (!notify) return;

                NotificationSystem.Default.Push(NotifcationType.Info, "Sync Completed.",
                    $"Finished copying {copyCount} files, renaming {renameCount} files, and deleting {deleteCount} files.");
            }
            finally
            {
                _fileOperationMutex.ReleaseMutex();
            }
        }

        public async Task SynchroniseAsync()
        {
            await Task.Run(() => Synchronise()).ConfigureAwait(false);
        }

        private bool ShouldFilter(string file)
        {
            foreach (var filter in Data.Filters)
            {
                if (filter.ShouldExcludeFile(file))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Event recieved when a file in WatchPath is deleted
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Event Parameters (contains file path)</param>
        private void WriteWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            MarkFileDelete(e.FullPath);
        }

        /// <summary>
        /// Marks a given file for deletion. If the watcher is set to automatically delete
        /// then it will perform the delete.
        /// </summary>
        /// <param name="file">File to mark</param>
        public void MarkFileDelete(string file)
        {
            if (ShouldFilter(file))
            {
                return;
            }

            ResetDelay();

            _fileOperationMutex.WaitOne();
            try
            {

                if (_copy.Contains(file))
                {
                    Logging.WriteLine(Resources.Auto_Removing_Target, file);
                    _copy.Remove(file);
                }

                if (Data.AutoDeleteFiles && Delay == null)
                {
                    Logging.WriteLine(Resources.Auto_Deleting_Target, file);
                    Synchroniser.DeleteAsync(file);
                    Logging.WriteLine(Resources.Auto_Deleted_Target, file);
                }
                else
                {
                    Logging.WriteLine(Resources.Auto_Mark_Delete_Target, file);
                    _delete.Add(file);
                }
            }
            finally
            {
                _fileOperationMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Resets the Delay timer and shows the Delay notification window.
        /// </summary>
        private void ResetDelay()
        {
            if (_display != null)
            {
                Application.Current.Dispatcher.Invoke(_display.Show);
                Application.Current.Dispatcher.Invoke(() => _display.SetFileCount(MarkedForCopy.Count + MarkedForDeletion.Count + MarkedForRename.Count));
            }

            Delay?.ResetTimer();
        }

        /// <summary>
        /// Event recieved when a file in WatchPath is modified / created
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Event Parameters (contains file path)</param>
        private void WriteWatcherChanged(object sender, FileSystemEventArgs e)
        {
            MarkFileCopy(e.FullPath);
        }

        /// <summary>
        /// Marks a given file to be copied. If the watcher is set to automatically copy
        /// then it will perform the copy.
        /// </summary>
        /// <param name="file">File to mark</param>
        public void MarkFileCopy(string file)
        {
            if (ShouldFilter(file))
            {
                return;
            }

            ResetDelay();

            _fileOperationMutex.WaitOne();
            try
            {
                if (_delete.Contains(file))
                {
                    Logging.WriteLine(Resources.Auto_Remove_Delete_Target, file);
                    _delete.Remove(file);
                }

                if (Data.AutoAddFiles && Delay == null)
                {
                    Logging.WriteLine(Resources.Auto_Copying_Target, file);
                    Synchroniser.AddAsync(file);
                    Logging.WriteLine(Resources.Auto_Copied_Target, file);
                }
                else
                {
                    Logging.WriteLine(Resources.Auto_Mark_Copy_Target, file);
                    _copy.Add(file);
                }
            }
            finally
            {
                _fileOperationMutex.ReleaseMutex();
            }
        }


        /// <summary>
        /// Event recieved when a file in WatchPath is modified / created
        /// </summary>
        /// <param name="sender">FileSystemWatcher</param>
        /// <param name="e">Event Parameters (contains file path)</param>
        private void WriteWatcherRenamed(object sender, FileSystemEventArgs e)
        {
            if(e is RenamedEventArgs renameArgs)
            {
                MarkFileRenamed(renameArgs.OldFullPath, renameArgs.FullPath);
            }
        }

        /// <summary>
        /// Marks a given file to be copied. If the watcher is set to automatically copy
        /// then it will perform the copy.
        /// </summary>
        /// <param name="oldPath">Pre-rename path</param>
        /// <param name="newPath">Post-rename path</param>
        private void MarkFileRenamed(string oldPath, string newPath)
        {
            // TODO: Is filtering on just newPath correct?
            if (ShouldFilter(newPath))
            {
                return;
            }

            ResetDelay();

            Logging.WriteLine(Resources.Renaming_File_From_To, oldPath, newPath);

            _fileOperationMutex.WaitOne();
            try
            {

                if (_delete.Contains(oldPath))
                {
                    Logging.WriteLine(Resources.Auto_Remove_Delete_Target, oldPath);
                    _delete.Remove(oldPath);
                }

                if (_copy.Contains(oldPath))
                {
                    Logging.WriteLine(Resources.Auto_Removing_Target, oldPath);
                    _copy.Remove(oldPath);
                }

                if (Data.AutoAddFiles && Delay == null)
                {
                    Synchroniser.RenameAsync(oldPath, newPath);
                }
                else
                {
                    Logging.WriteLine(Resources.Mark_File_Rename, oldPath);
                    _rename.Add((oldPath, newPath));
                }
            }
            finally
            {
                _fileOperationMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Stops all events and cleans up the FileSystemWatcher classes.
        /// After Stop is called the class cannot start watching again.
        /// To disable events temporarily use DisableEvents.
        /// </summary>
        public void Stop()
        {
            Logging.WriteLine(Resources.Watcher_Stop_Target, Data.WatchDirectory);
            DisableEvents();
            Delay?.Stop();
            _display?.Hide();
            _watcher?.Dispose();
        }

        /// <summary>
        /// Stops any events from being raised.
        /// Use EnableEvents to turn events back on.
        /// </summary>
        public void DisableEvents()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Equality comparison check wrapper.
        /// Discards any objects that aren't Watchers.
        /// </summary>
        /// <param name="obj">Watcher to compare with</param>
        /// <returns>If object is a Watcher that has equal properties</returns>
        public override bool Equals(object obj)
        {
            return obj is Watcher wobj && Equals(wobj);
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
                   Name == other.Name &&
                   Data.Equals(other.Data);
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
                hashCode = (hashCode*397) ^ (Data.WatchDirectory?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Synchroniser?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ Data.Recursive.GetHashCode();
                hashCode = (hashCode*397) ^ Name.GetHashCode();
                return hashCode;
            }
        }
    }
}

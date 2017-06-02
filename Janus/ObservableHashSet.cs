using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Janus
{
    public class ObservableSet<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext _syncContext;

        public ObservableSet()
        {
            _syncContext = SynchronizationContext.Current;
        }

        private void NotifyCountChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, object item = null)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item));
        }

        private bool _add(T item)
        {
            var index = IndexOf(item);
            if (index >= 0)
                return false;

            base.Add(item);

            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            NotifyCountChanged();

            return true;
        }

        new public void Add(T item)
        {
            if (_syncContext != null)
            {
                _syncContext.Send(x => _add(item), null);
            }
            else
            {
                _add(item);
            }
        }

        private bool _remove(T item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;

            bool changed = base.Remove(item);

            if (changed)
            {
                NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
                NotifyCountChanged();
            }

            return true;
        }

        new public void Remove(T item)
        {
            if (_syncContext != null)
            {
                _syncContext.Send(x => _remove(item), null);
            }
            else
            {
                _remove(item);
            }
        }

        private void _clear()
        {
            if (Count == 0)
                return;

            Clear();

            NotifyCollectionChanged(NotifyCollectionChangedAction.Reset);
            NotifyCountChanged();
        }

        new public void Clear()
        {
            if (_syncContext != null)
            {
                _syncContext.Send(x => _clear(), null);
            }
            else
            {
                _clear();
            }
        }
    }
}

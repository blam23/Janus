using System.Collections.ObjectModel;

namespace Janus
{
    /// <summary>
    /// Class that gets written out and read in by DataStorage.
    /// </summary>
    public class JanusData
    {
        public readonly ObservableCollection<Watcher> Watchers;
        public readonly DataProvider DataProvider;

        public JanusData(ObservableCollection<Watcher> watchers, DataProvider dataProvider)
        {
            Watchers = watchers;
            DataProvider = dataProvider;
        }
        public JanusData()
        {
            Watchers = new ObservableCollection<Watcher>();
            DataProvider = new DataProvider();
        }
    }
}
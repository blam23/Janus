using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Janus
{
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
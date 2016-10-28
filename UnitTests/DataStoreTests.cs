#define TEST
using System.Collections.ObjectModel;
using Janus;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class DataStoreTests
    {
        private JanusData _data = new JanusData();

        public void Reset()
        {
            _data = new JanusData();
        }

        private Watcher AddWatcher(string watchDir, string syncDir, bool addFiles, bool deleteFiles, string filter,
            bool recursive)
        {
            var watcher = new Watcher(
                watchDir,
                syncDir,
                addFiles,
                deleteFiles,
                filter,
                recursive,
                true
            );

            _data.Watchers.Add(watcher);

            return watcher;
        }

        private void RemoveWatcher(Watcher w)
        {
            _data.Watchers.Remove(w);
        }


        private int count = 1;

        private void AssertLoadedWatchersAreCorrect(JanusData storedList)
        {
            Assert.AreEqual(storedList.Watchers.Count, _data.Watchers.Count,
                "[{0}] Number of watchers in loaded list does not match saved list.", count);

            for (var i = 0; i < storedList.Watchers.Count; i++)
            {
                Assert.AreEqual(storedList.Watchers[i], _data.Watchers[i],
                    "[{0}-{1}] Stored watcher was not the same as saved watcher.", count, i);
            }

            count++;
        }

        private void AssertDataProvidersAreEqual(JanusData storedList)
        {
            Assert.AreEqual(storedList.DataProvider.Dict.Count, _data.DataProvider.Dict.Count,
                "[{0}] Number of values in loaded data provider did not match.", count);

            foreach (var kvp in _data.DataProvider.Dict)
            {
                AssertLoadedDataProviderContains(storedList, kvp.Key, kvp.Value);
            }
            count++;
        }

        private void AssertLoadedDataProviderContains<T>(JanusData storedList, string key, T expected)
        {
            var data = storedList.DataProvider.Get<T>(key);
            Assert.AreEqual(data, expected, 
                "[{0}] Loaded DataProvider value did not match.", count);
        }

        private JanusData StoreAndLoad(DataStore testStore)
        {
            testStore.Store(_data);
            var storedList = testStore.Load();
            return storedList;
        }


        [TestMethod]
        public void DataStoreChange()
        {
            var testStore = new DataStore("test");

            Assert.IsFalse(string.IsNullOrEmpty(DataStore.AppData),
                "DataStore did not get the AppData folder.");
            Assert.IsFalse(string.IsNullOrEmpty(testStore.AssemblyDirectory),
                "DataStore did not get the AssemblyDirectory.");
            Assert.IsFalse(string.IsNullOrEmpty(testStore.DataLocation),
                "DataStore did not set the DataLocation.");

            testStore.Initialise();

            Assert.IsTrue(testStore.DataLoaders.ContainsKey(DataStore.Version),
                "DataStore did not load a storage format matching the specified version: {0:X}", DataStore.Version);

            var w1 = AddWatcher("C:\\test\\directory", "C:\\out\\directory", false, false, "*", true);
            var data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);

            RemoveWatcher(w1);
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);

            _data.DataProvider.Add("Test", "Hello World");
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertLoadedDataProviderContains(data, "Test", "Hello World");

            var w2 = AddWatcher("C:\\test\\directory2", "C:\\out\\directory2", true, false,
                "#.*\"sdfas'd\t;lfk^&*%^$%-{}+=", false);
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertLoadedDataProviderContains(data, "Test", "Hello World");

            _data.DataProvider.Add("AnotherTest", true);
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertLoadedDataProviderContains(data, "Test", "Hello World");
            AssertLoadedDataProviderContains(data, "AnotherTest", true);

            _data.DataProvider.Remove("Test");
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertDataProvidersAreEqual(data);


            var w3 = AddWatcher("C:\\test\\directory3", "C:\\out\\directory3", false, true, "", true);
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertDataProvidersAreEqual(data);

            RemoveWatcher(w2);
            _data.DataProvider.Remove("AnotherTest");
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertDataProvidersAreEqual(data);

            RemoveWatcher(w3);
            data = StoreAndLoad(testStore);
            AssertLoadedWatchersAreCorrect(data);
            AssertDataProvidersAreEqual(data);
        }

        [TestMethod]
        public void DataProviderTypes()
        {
            var testStore = new DataStore("test2");

            testStore.Initialise();

            _data.DataProvider.Add("tc-str", "Hello World");
            _data.DataProvider.Add("tc-int", 42);
            _data.DataProvider.Add("tc-neg-int", -123123);
            _data.DataProvider.Add("tc-bool-f", false);
            _data.DataProvider.Add("tc-bool-t", true);
            _data.DataProvider.Add("tc-double", 0.2342341234);
            _data.DataProvider.Add("tc-double-2", -1231231.231231);

            var data = StoreAndLoad(testStore);

            AssertDataProvidersAreEqual(data);
        }
    }
}
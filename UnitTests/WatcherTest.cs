using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Janus;
using Janus.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests.Helpers;

namespace UnitTests
{
    [TestClass]
    public class WatcherTest : DataTestTemplate
    {
        public WatcherTest() : base("Watcher") { }

        [TestMethod]
        public void StandardWatcherTest()
        {
            SetupData("Standard");

            var watcher = new Watcher
                (
                    "StandardWatcherTest",
                    TestInput,
                    TestOutput,
                    true,
                    true,
                    new ObservableCollection<IFilter>(),
                    true
                );

            var fileIn = Path.Combine(TestInput, "test.txt");
            var fileOut = Path.Combine(TestOutput, "test.txt");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(400);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            File.Delete(fileIn);
            Thread.Sleep(200);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not delete file.");

            watcher.DisableEvents();
        }

        [TestMethod]
        public void FilteredWatcherTest()
        {
            SetupData("Filter");
            var filters = new ObservableCollection<IFilter>
            {
                new ExcludeFilter("*.txt;*.ini".SplitEscapable(';'))
            };

            var watcher = new Watcher
                (
                    "FilterWatcherTest",
                    TestInput,
                    TestOutput,
                    true,
                    true,
                    filters,
                    true
                );

            Task.WaitAll(watcher.DoInitialSynchronise());

            Assert.IsFalse(File.Exists(Path.Combine(TestOutput, "hello.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(TestOutput, "test.ini")));
            Assert.IsTrue(File.Exists(Path.Combine(TestOutput, "othertest.bork")));

            var fileIn = Path.Combine(TestInput, "test.txt");
            var fileOut = Path.Combine(TestOutput, "test.txt");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(200);

            Assert.IsFalse(File.Exists(fileOut), "Watcher copied over filtered created file.");

            fileIn = Path.Combine(TestInput, "test");
            fileOut = Path.Combine(TestOutput, "test");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World.");
            }
            Thread.Sleep(200);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            File.Delete(fileIn);
            Thread.Sleep(200);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not delete file.");

            watcher.DisableEvents();
        }
    }
}

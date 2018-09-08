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
            var fileInRenamed = Path.Combine(TestInput, "test-renamed.txt");
            var fileOut = Path.Combine(TestOutput, "test.txt");
            var fileOutRenamed = Path.Combine(TestOutput, "test-renamed.txt");

            // Add File
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(1000);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            // Rename File
            File.Move(fileIn, fileInRenamed);
            Thread.Sleep(1000);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not rename file - found old file name");
            Assert.IsTrue(File.Exists(fileOutRenamed), "Watcher did not delete file - did not find new file name");

            // Delete File
            File.Delete(fileInRenamed);
            Thread.Sleep(1000);

            Assert.IsFalse(File.Exists(fileOutRenamed), "Watcher did not delete file.");

            watcher.DisableEvents();
        }

        [TestMethod]
        public void ManualCopyTest()
        {
            SetupData("Manual");
            var filters = new ObservableCollection<IFilter>();

            var watcher = new Watcher
            (
                "ManualCopyTest",
                TestInput,
                TestOutput,
                false,
                false,
                filters,
                true
            );

            //
            // One By One
            // 

            var fileIn = Path.Combine(TestInput, "test.txt");
            var fileInRenamed = Path.Combine(TestInput, "test-renamed.txt");
            var fileOut = Path.Combine(TestOutput, "test.txt");
            var fileOutRenamed = Path.Combine(TestOutput, "test-renamed.txt");

            // Add File
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(400);
            watcher.Synchronise(false);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            // Rename File
            File.Move(fileIn, fileInRenamed);
            Thread.Sleep(400);
            watcher.Synchronise(false);


            Assert.IsFalse(File.Exists(fileOut), "Watcher did not rename file - found old file name");
            Assert.IsTrue(File.Exists(fileOutRenamed), "Watcher did not delete file - did not find new file name");

            // Delete File
            File.Delete(fileInRenamed);
            Thread.Sleep(400);
            watcher.Synchronise(false);

            Assert.IsFalse(File.Exists(fileOutRenamed), "Watcher did not delete file.");

            //
            // Batch
            //

            // Clear out directory
            Directory.Delete(TestOutput, true);
            Directory.CreateDirectory(TestOutput);

            // Add File
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(400);

            // Rename File
            File.Move(fileIn, fileInRenamed);
            Thread.Sleep(400);

            watcher.Synchronise(false);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not rename file - found old file name");
            Assert.IsTrue(File.Exists(fileOutRenamed), "Watcher did not delete file - did not find new file name");


            watcher.DisableEvents();
        }

        [TestMethod]
        public void SetOfFilesTest()
        {
            var filters = new ObservableCollection<IFilter>();

            var watcher = new Watcher
               (
                   "SetOfFilesTest",
                   TestInput,
                   TestOutput,
                   false,
                   false,
                   filters,
                   true,
                   observe: true
               );

            watcher.MarkFileCopy("test.txt");
            watcher.MarkFileCopy("test.txt");
            watcher.MarkFileCopy("test.txt");

            // Copy and Delete lists are sets - should only have unique entries
            Assert.AreEqual(1, watcher.MarkedForCopy.Count);
            Assert.AreEqual(0, watcher.MarkedForDeletion.Count);

            watcher.MarkFileDelete("test.txt");
            watcher.MarkFileDelete("test.txt");

            // Marking a file for deletion should remove it from copy set
            Assert.AreEqual(0, watcher.MarkedForCopy.Count);
            Assert.AreEqual(1, watcher.MarkedForDeletion.Count);

            watcher.MarkFileCopy("test.txt");

            // Similarly adding a file to copy list should remove it from delete list.
            Assert.AreEqual(1, watcher.MarkedForCopy.Count);
            Assert.AreEqual(0, watcher.MarkedForDeletion.Count);
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

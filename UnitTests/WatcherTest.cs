using System;
using System.Collections.Generic;
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
                    _testInput,
                    _testOutput,
                    true,
                    true,
                    new List<IFilter>(), 
                    true
                );

            var fileIn = Path.Combine(_testInput, "test.txt");
            var fileOut = Path.Combine(_testOutput, "test.txt");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(100);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            File.Delete(fileIn);
            Thread.Sleep(100);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not delete file.");
        }

        [TestMethod]
        public void FilteredWatcherTest()
        {
            SetupData("Filter");
            var filters = new List<IFilter>
            {
                new ExcludeFilter("*.txt;*.ini".SplitEscapable(';'))
            };

            var watcher = new Watcher
                (
                    _testInput,
                    _testOutput,
                    true,
                    true,
                    filters,
                    true
                );

            Task.WaitAll(watcher.DoInitialSynchronise());

            Assert.IsFalse(File.Exists(Path.Combine(_testOutput, "hello.txt")));
            Assert.IsFalse(File.Exists(Path.Combine(_testOutput, "test.ini")));
            Assert.IsTrue(File.Exists(Path.Combine(_testOutput, "othertest.bork")));

            var fileIn = Path.Combine(_testInput, "test.txt");
            var fileOut = Path.Combine(_testOutput, "test.txt");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World");
            }
            Thread.Sleep(100);

            Assert.IsFalse(File.Exists(fileOut), "Watcher copied over filtered created file.");

            fileIn = Path.Combine(_testInput, "test");
            fileOut = Path.Combine(_testOutput, "test");
            using (var writer = File.CreateText(fileIn))
            {
                writer.WriteLine("Hello World.");
            }
            Thread.Sleep(50);

            Assert.IsTrue(File.Exists(fileOut), "Watcher did not copy over created file.");

            File.Delete(fileIn);
            Thread.Sleep(50);

            Assert.IsFalse(File.Exists(fileOut), "Watcher did not delete file.");
        }
    }
}

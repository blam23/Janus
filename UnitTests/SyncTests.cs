using Janus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using UnitTests.Helpers;

namespace UnitTests
{
    [TestClass]
    public class SyncTests
    {
        private readonly string _path = Path.GetFullPath(@"Tests\Sync");
        private readonly string _dataPath = Path.GetFullPath(@"..\..\Data\Sync");

        private string _testInput;
        private string _testOutput;
        private Watcher _watcher;


        private void Setup(string test, bool addFiles, bool deleteFiles, bool recursive)
        {
            var dataInput = Path.Combine(_dataPath, test, @"in");

            _testInput = Path.Combine(_path, test, @"in");
            _testOutput = Path.Combine(_path, test, @"out");

            // Clear existing test data
            if (Directory.Exists(_testInput))
                Directory.Delete(_testInput, true);
            if (Directory.Exists(_testOutput))
                Directory.Delete(_testOutput, true);

            // Copy needed files from data directory
            TestHelper.CopyDirectory(_testInput, dataInput);

            _watcher = new Watcher(
                _testInput,
                _testOutput,
                addFiles,
                deleteFiles,
                null,
                recursive
            );
        }



        [TestMethod]
        public void Add()
        {
            const string testName = "Copy";
            Setup(testName, false, false, false);
            var testFile1 = Path.Combine(_testInput, "add_test_1.txt");
            const string testFile2 = "add_test_2.txt";


            _watcher.Synchroniser.AddAsync(testFile1, true);

            Assert.IsTrue(File.Exists(Path.Combine(_testOutput, "add_test_1.txt")),
                "First test file was not copied to out dir.");


            _watcher.Synchroniser.AddAsync(testFile2, false);

            Assert.IsTrue(File.Exists(Path.Combine(_testOutput, "add_test_2.txt")),
                "Second test file was not copied to out dir.");
        }

        [TestMethod]
        public void Delete()
        {
            const string testName = "Delete";
            Setup(testName, false, false, false);
            var testFile1 = Path.Combine(_testInput, "delete_test_1.txt");

            _watcher.Synchroniser.AddAsync(testFile1, true);

            Assert.IsTrue(File.Exists(Path.Combine(_testOutput, "delete_test_1.txt")),
                "Delete test file was not copied to out dir.");

            _watcher.Synchroniser.DeleteAsync(testFile1);

            Assert.IsFalse(File.Exists(Path.Combine(_testOutput, "delete_test_1.txt")),
                "Delete test file was not removed from out dir.");
        }
    }
}
using Janus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class SyncTests
    {
        readonly string path = Path.GetFullPath(@"Tests\Sync");
        readonly string dataPath = Path.GetFullPath(@"..\..\Data\Sync");

        string testInput;
        string testOutput;
        Watcher watcher;


        public void Setup(string test, bool addFiles, bool deleteFiles, string filter, bool recursive)
        {
            string dataInput = Path.Combine(dataPath, test, @"in");

            testInput = Path.Combine(path, test, @"in");
            testOutput = Path.Combine(path, test, @"out");

            // Clear existing test data
            if (Directory.Exists(testInput))
                Directory.Delete(testInput, true);
            if (Directory.Exists(testOutput))
                Directory.Delete(testOutput, true);

            // Copy needed files from data directory
            TestHelper.CopyDirectory(testInput, dataInput);

            watcher = new Watcher(
                testInput,
                testOutput,
                addFiles,
                deleteFiles,
                filter,
                recursive
            );
        }



        [TestMethod]
        public void Add()
        {
            const string testName = "Copy";
            Setup(testName, false, false, "*", false);
            var testFile1 = Path.Combine(testInput, "add_test_1.txt");
            var testFile2 = "add_test_2.txt";


            watcher.Sync.Add(testFile1, true);

            Assert.IsTrue(File.Exists(Path.Combine(testOutput, "add_test_1.txt")));


            watcher.Sync.Add(testFile2, false);

            Assert.IsTrue(File.Exists(Path.Combine(testOutput, "add_test_2.txt")));
        }

        [TestMethod]
        public void Delete()
        {
            const string testName = "Delete";
            Setup(testName, false, false, "*", false);
            var testFile1 = Path.Combine(testInput, "delete_test_1.txt");

            watcher.Sync.Add(testFile1, true);

            Assert.IsTrue(File.Exists(Path.Combine(testOutput, "delete_test_1.txt")));


            watcher.Sync.Delete(testFile1, false);

            Assert.IsFalse(File.Exists(Path.Combine(testOutput, "delete_test_1.txt")));
        }
    }
}
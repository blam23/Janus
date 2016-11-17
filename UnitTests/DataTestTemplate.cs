using System.IO;

namespace UnitTests
{
    public class DataTestTemplate
    {
        internal readonly string _path;
        internal readonly string _dataPath;

        internal string _testInput;
        internal string _testOutput;

        public DataTestTemplate(string name)
        {
            _path = Path.GetFullPath($"Tests\\{name}");
            _dataPath = Path.GetFullPath($"..\\..\\Data\\{name}");
        }

        internal void SetupData(string test)
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
        }
    }
}

using System.IO;

namespace UnitTests.Helpers
{
    public class DataTestTemplate
    {
        internal readonly string StartPath;
        internal readonly string DataPath;

        internal string TestInput;
        internal string TestOutput;

        public DataTestTemplate(string name)
        {
            StartPath = Path.GetFullPath($"Tests\\{name}");
            DataPath = Path.GetFullPath($"..\\..\\Data\\{name}");
        }

        internal void SetupData(string test)
        {
            var dataInput = Path.Combine(DataPath, test, @"in");

            TestInput = Path.Combine(StartPath, test, @"in");
            TestOutput = Path.Combine(StartPath, test, @"out");

            // Clear existing test data
            if (Directory.Exists(TestInput))
                Directory.Delete(TestInput, true);
            if (Directory.Exists(TestOutput))
                Directory.Delete(TestOutput, true);

            // Copy needed files from data directory
            TestHelper.CopyDirectory(TestInput, dataInput);
        }
    }
}

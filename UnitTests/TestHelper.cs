using System.Diagnostics;

namespace UnitTests
{
    public static class TestHelper
    {
        public static void CopyDirectory(string testInput, string dataInput)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    UseShellExecute = true,
                    FileName = @"C:\WINDOWS\system32\xcopy.exe",
                    Arguments = $@"""{dataInput}"" ""{testInput}"" /E /I /Y"
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}

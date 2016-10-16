using System.Diagnostics;

namespace UnitTests
{
    public static class TestHelper
    {
        public static void CopyDirectory(System.String testInput, System.String dataInput)
        {
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = @"C:\WINDOWS\system32\xcopy.exe";
            proc.StartInfo.Arguments = $@"""{dataInput}"" ""{testInput}"" /E /I /Y";
            proc.Start();
            proc.WaitForExit();
        }
    }
}

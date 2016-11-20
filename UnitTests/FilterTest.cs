using Microsoft.VisualStudio.TestTools.UnitTesting;
using Janus.Filters;
using System.IO;
using UnitTests.Helpers;

namespace UnitTests
{
    [TestClass]
    public class FilterTest : DataTestTemplate
    {
        public FilterTest() : base("Filter") { }
        
        [TestMethod]
        public void ExcludeFileTest()
        {
            SetupData("exclude");

            var filter = new ExcludeFileFilter(".*", "testfile");

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "..")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "... ")), true);

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "false")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "fa.lse")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile2")), false);
        }
    }
}

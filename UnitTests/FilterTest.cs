using System.IO;
using Janus.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            SetupData("exclude-file");

            var filter = new ExcludeFileFilter(".*", "testfile");

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, ".testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, ".")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "..")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "... ")), true);

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "false")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "fa.lse")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "testfile2")), false);
        }

        [TestMethod]
        public void ExcludeTest()
        {
            SetupData("exclude");

            var filter = new ExcludeFilter("*\\out\\*", "*testfile");

            // These have "/out/" in their path
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, ".testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "asdf")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "out")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "krout")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "out\\")), true);

            // Matches against second filter
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "testfile")), true);

            // Shouldn't match against either filter, unless
            // test system somewhere has an out in it! D:
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "false")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "out")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "fa.lse")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "testfile2")), false);
        }

        [TestMethod]
        public void IncludeTest()
        {
            SetupData("include");

            var filter = new IncludeFilter("*\\out\\*", "*testfile");

            // These have "/out/" in their path
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "testfile")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, ".testfile")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "asdf")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "out")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestOutput, "krout")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "out\\")), false);

            // Matches against second filter
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "testfile")), false);

            // Shouldn't match against either filter, unless
            // test system somewhere has an out in it! D:
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "false")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "out")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "fa.lse")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(TestInput, "testfile2")), true);
        }
    }
}

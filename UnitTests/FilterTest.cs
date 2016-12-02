using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Janus.Filters;
using System.IO;
using UnitTests.Helpers;
using System.Collections.Generic;

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

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "..")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "... ")), true);

            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "false")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "fa.lse")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile2")), false);
        }

        [TestMethod]
        public void ExcludeTest()
        {
            SetupData("exclude");

            var filter = new ExcludeFilter("*\\out\\*", "*testfile");

            // These have "/out/" in their path
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".testfile")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "asdf")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "out")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "krout")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "out\\")), true);

            // Matches against second filter
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "testfile")), true);

            // Shouldn't match against either filter, unless
            // test system somewhere has an out in it! D:
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "false")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "out")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "fa.lse")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "testfile2")), false);
        }

        [TestMethod]
        public void IncludeTest()
        {
            SetupData("include");

            var filter = new IncludeFilter("*\\out\\*", "*testfile");

            // These have "/out/" in their path
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "testfile")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, ".testfile")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "asdf")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "out")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testOutput, "krout")), false);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "out\\")), false);

            // Matches against second filter
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "testfile")), false);

            // Shouldn't match against either filter, unless
            // test system somewhere has an out in it! D:
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "false")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "out")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "fa.lse")), true);
            Assert.AreEqual(filter.ShouldExcludeFile(Path.Combine(_testInput, "testfile2")), true);
        }
    }
}

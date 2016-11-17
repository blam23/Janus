using Microsoft.VisualStudio.TestTools.UnitTesting;
using Janus.Filter;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class FilterTest : DataTestTemplate
    {
        public FilterTest() : base("Filter") { }

        [TestMethod]
        public void MatchTest()
        {
            var filter = new ExcludeFileFilter("*foo*");

            Assert.AreEqual(filter.Matches("ufoou"), true);
            Assert.AreEqual(filter.Matches("foo"), true);
            Assert.AreEqual(filter.Matches("fooba"), true);
            Assert.AreEqual(filter.Matches("foobar"), true);
            Assert.AreEqual(filter.Matches("barfoo"), true);

            Assert.AreEqual(filter.Matches("fo"), false);
            Assert.AreEqual(filter.Matches("crackers"), false);
            Assert.AreEqual(filter.Matches("oof"), false);


            filter = new ExcludeFileFilter("*egg*foo*");

            Assert.AreEqual(filter.Matches("eggyfoob"), true);
            Assert.AreEqual(filter.Matches("eggfoo"), true);
            Assert.AreEqual(filter.Matches("it is known that eggs are food"), true);

            Assert.AreEqual(filter.Matches("fooegg"), false);
            Assert.AreEqual(filter.Matches("egg"), false);
            Assert.AreEqual(filter.Matches("egfgoo"), false);
            Assert.AreEqual(filter.Matches("efgogo"), false);

            filter = new ExcludeFileFilter(".*");

            Assert.AreEqual(filter.Matches(".vimrc"), true);
            Assert.AreEqual(filter.Matches(".vs"), true);
            Assert.AreEqual(filter.Matches(".g.i.t"), true);
            Assert.AreEqual(filter.Matches("."), true);
            Assert.AreEqual(filter.Matches(".."), true);

            Assert.AreEqual(filter.Matches("vimrc"), false);


            filter = new ExcludeFileFilter("*.txt");

            Assert.AreEqual(filter.Matches("note.txt"), true);
            Assert.AreEqual(filter.Matches("bes.tx.txt"), true);
            Assert.AreEqual(filter.Matches("corn.txt"), true);


            Assert.AreEqual(filter.Matches("corn.tx"), false);
            Assert.AreEqual(filter.Matches(".txttest"), false);


            filter = new ExcludeFileFilter("txt");

            Assert.AreEqual(filter.Matches("txt"), true);

            Assert.AreEqual(filter.Matches("nottxt"), false);
            Assert.AreEqual(filter.Matches("xt"), false);
            Assert.AreEqual(filter.Matches("t"), false);
            Assert.AreEqual(filter.Matches("tx"), false);
            Assert.AreEqual(filter.Matches("txtt"), false);
            Assert.AreEqual(filter.Matches("ttxt"), false);
            Assert.AreEqual(filter.Matches("ttxtt"), false);


        }

        [TestMethod]
        public void ExcludeFileTest()
        {
            SetupData("exclude");

            var filter = new ExcludeFileFilter(".*", "testfile");

            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "testfile")), true);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, ".testfile")), true);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, ".")), true);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "..")), true);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "... ")), true);

            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "false")), false);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "fa.lse")), false);
            Assert.AreEqual(filter.ExcludeFile(Path.Combine(_testOutput, "testfile2")), false);
        }
    }
}

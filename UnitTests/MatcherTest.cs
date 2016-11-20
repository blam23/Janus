using Microsoft.VisualStudio.TestTools.UnitTesting;
using Janus.Matchers;

namespace UnitTests
{
    [TestClass]
    public class MatcherTest
    {
        [TestMethod]
        public void SimpleStringMatcherTest()
        {
            var matcher = new SimpleStringMatcher();

            var pattern = "*foo*";
            Assert.AreEqual(true, matcher.Matches("ufoou", pattern));
            Assert.AreEqual(true, matcher.Matches("foo", pattern));
            Assert.AreEqual(true, matcher.Matches("fooba", pattern));
            Assert.AreEqual(true, matcher.Matches("foobar", pattern));
            Assert.AreEqual(true, matcher.Matches("barfoo", pattern));
            Assert.AreEqual(false, matcher.Matches("fo", pattern));
            Assert.AreEqual(false, matcher.Matches("crackers", pattern));
            Assert.AreEqual(false, matcher.Matches("oof", pattern));

            pattern = "*egg*foo*";
            Assert.AreEqual(true, matcher.Matches("eggyfoob", pattern));
            Assert.AreEqual(true, matcher.Matches("eggfoo", pattern));
            Assert.AreEqual(true, matcher.Matches("it is known that eggs are food", pattern));
            Assert.AreEqual(false, matcher.Matches("fooegg", pattern));
            Assert.AreEqual(false, matcher.Matches("egg", pattern));
            Assert.AreEqual(false, matcher.Matches("egfgoo", pattern));
            Assert.AreEqual(false, matcher.Matches("efgogo", pattern));

            pattern = ".*";
            Assert.AreEqual(true, matcher.Matches(".vimrc", pattern));
            Assert.AreEqual(true, matcher.Matches(".vs", pattern));
            Assert.AreEqual(true, matcher.Matches(".g.i.t", pattern));
            Assert.AreEqual(true, matcher.Matches(".", pattern));
            Assert.AreEqual(true, matcher.Matches("..", pattern));
            Assert.AreEqual(false, matcher.Matches("vimrc", pattern));


            pattern = "*.txt";
            Assert.AreEqual(true, matcher.Matches("note.txt", pattern));
            Assert.AreEqual(true, matcher.Matches("bes.tx.txt", pattern));
            Assert.AreEqual(true, matcher.Matches("corn.txt", pattern));
            Assert.AreEqual(false, matcher.Matches("corn.tx", pattern));
            Assert.AreEqual(false, matcher.Matches(".txttest", pattern));


            pattern = "txt";
            Assert.AreEqual(true, matcher.Matches("txt", pattern));
            Assert.AreEqual(false, matcher.Matches("nottxt", pattern));
            Assert.AreEqual(false, matcher.Matches("xt", pattern));
            Assert.AreEqual(false, matcher.Matches("t", pattern));
            Assert.AreEqual(false, matcher.Matches("tx", pattern));
            Assert.AreEqual(false, matcher.Matches("txtt", pattern));
            Assert.AreEqual(false, matcher.Matches("ttxt", pattern));
            Assert.AreEqual(false, matcher.Matches("ttxtt", pattern));


            Assert.AreEqual(true, matcher.Matches("Hello World", "H* W*"));
        }

    }
}

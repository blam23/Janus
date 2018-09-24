using System.Collections.Generic;
using JanusSharedLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class ExtensionsTest
    {

        [TestMethod]
        public void SplitEscapable()
        {
            var expected = new List<string> {"hello", "world"};
            var split = "hello;world".SplitEscapable(';');
            CollectionAssert.AreEqual(expected, split);

            expected = new List<string> {"hello", "wor;ld"};
            split = "hello;wor\\;ld".SplitEscapable(';');
            CollectionAssert.AreEqual(expected, split);

            expected = new List<string> {"test"};
            split = "test".SplitEscapable('\'');
            CollectionAssert.AreEqual(expected, split);

            expected = new List<string> {"a","b","c","d","e,", "f"};
            split = "a,b,c,d,e\\,,f".SplitEscapable(',');
            CollectionAssert.AreEqual(expected, split);

            expected = new List<string> { "a" };
            split = ",,,,,a,".SplitEscapable(',');
            CollectionAssert.AreEqual(expected, split);
        }
    }
}
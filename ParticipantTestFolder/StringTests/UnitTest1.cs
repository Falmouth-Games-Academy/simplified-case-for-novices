namespace ER_CompBase
{
    using NUnit.Framework;

    [TestFixture]
    public class Tests
    {
        [Test(Description = "RepeatString() Tests")]
        public void RepeatStringTests()
        {
            Assert.AreEqual("***", StringManipulation.RepeatString(3, "*"));
            Assert.AreEqual("#####", StringManipulation.RepeatString(5, "#"));
            Assert.AreEqual("ha ha ", StringManipulation.RepeatString(2, "ha "));
            Assert.AreEqual("I'm the string!I'm the string!I'm the string!I'm the string!I'm the string!", StringManipulation.RepeatString(5, "I'm the string!"));
            Assert.AreEqual("  82 is a valid num.  82 is a valid num.", StringManipulation.RepeatString(2, "  82 is a valid num."));
        }

        [Test(Description = "ReverseString() Tests")]
        public void ReverseStringTests()
        {
            Assert.AreEqual("world! hello", StringManipulation.ReverseString("hello world!"));
            Assert.AreEqual("this like speak doesn't yoda", StringManipulation.ReverseString("yoda doesn't speak like this"));
            Assert.AreEqual("foobar", StringManipulation.ReverseString("foobar"));
            Assert.AreEqual("sentence reversed a demonstrating", StringManipulation.ReverseString("demonstrating a reversed sentence"));
            Assert.AreEqual("boat your row row row", StringManipulation.ReverseString("row row row your boat"));
        }

        [Test(Description = "InvertCasing() Tests")]
        public void InvertCasingTests()
        {
            Assert.AreEqual("HELLO WORLD", StringManipulation.InvertCasing("hello world"));
            Assert.AreEqual("hello world", StringManipulation.InvertCasing("HELLO WORLD"));
            Assert.AreEqual("HELLO world", StringManipulation.InvertCasing("hello WORLD"));
            Assert.AreEqual("hEllO wOrld", StringManipulation.InvertCasing("HeLLo WoRLD"));
            Assert.AreEqual("12345", StringManipulation.InvertCasing("12345"));
            Assert.AreEqual("1A2B3C4D5E", StringManipulation.InvertCasing("1a2b3c4d5e"));
            Assert.AreEqual("sTRING.tOaLTERNATINGcASE", StringManipulation.InvertCasing("String.ToAlternatingCase"));
            Assert.AreEqual("Hello World", StringManipulation.InvertCasing(StringManipulation.InvertCasing("Hello World")), "Hello World => hELLO wORLD => Hello World");
        }

        [Test(Description = "RemoveChars() Tests")]
        public void RemoveChars()
        {
            Assert.AreEqual("*", StringManipulation.RemoveChars("*", "abcx".ToCharArray()));
            Assert.AreEqual("Cetin In", StringManipulation.RemoveChars("Correction Ink", "cork".ToCharArray()));
            Assert.AreEqual("Spacesare", StringManipulation.RemoveChars("Spaces are wild ", " wild".ToCharArray()));
            Assert.AreEqual("I'm th strng!", StringManipulation.RemoveChars("I'm the string!", "aeiou".ToCharArray()));
            Assert.AreEqual("      ", StringManipulation.RemoveChars("  82 is a valid num.", "82isavldnum.".ToCharArray()));
        }
    }
}
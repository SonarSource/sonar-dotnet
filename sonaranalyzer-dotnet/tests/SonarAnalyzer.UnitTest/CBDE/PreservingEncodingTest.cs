extern alias csharp;

using System;
using System.Text;
using csharp::SonarAnalyzer.CBDE;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.CBDE
{
    [TestClass]
    public class PreservingEncodingTest
    {
        private readonly Encoding encoder = System.Text.Encoding.GetEncoding("ASCII", new PreservingEncodingFallback(), DecoderFallback.ExceptionFallback);

        public string Encode(string name)
        {
            Byte[] encodedBytes = encoder.GetBytes(name);
            return encoder.GetString(encodedBytes);

        }

        public void checkEncoding(string source, string encoded)
        {
            Assert.AreEqual(encoded, Encode(source));
        }
        [TestMethod]
        public void SimpleCharsAreUnchanged()
        {
            checkEncoding("ABCDEF abcdef", "ABCDEF abcdef");
        }

        [TestMethod]
        public void AccentedCharacterAreChanged()
        {
            checkEncoding("àÅéÈïİøÒùÛçµ", ".E0.C5.E9.C8.EF.130.F8.D2.F9.DB.E7.B5");
        }

        [TestMethod]
        public void CharactersWithLongEncodingAreChanged()
        {
            checkEncoding("𤭢𐐷", ".D852DF62.D801DC37");
        }

    }
}

using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class PreventUnicodeVulnerabilitiesTest
    {
        [TestMethod]
        public void PreventUnicodeVulnerabilities_CS()
            => Verifier.VerifyAnalyzer(
                @"TestCases\PreventUnicodeVulnerabilities.cs",
                new CS.PreventUnicodeVulnerabilities());

        [TestMethod]
        public void PreventUnicodeVulnerabilities_VB()
            => Verifier.VerifyAnalyzer(
                @"TestCases\PreventUnicodeVulnerabilities.cs",
                new VB.PreventUnicodeVulnerabilities());

        [DataTestMethod]
        [DataRow(0x00600, 0x006FF, "Arabic")]
        [DataRow(0x00750, 0x0077F, "Arabic Supplement")]
        [DataRow(0x00870, 0x0089F, "Arabic Extended-B")]
        [DataRow(0x008A0, 0x008FF, "Arabic Extended-A")]
        [DataRow(0x0FB50, 0x0FDFF, "Arabic Presentation Forms-A")]
        [DataRow(0x0FE70, 0x0FEFF, "Arabic Presentation Forms-B")]
        [DataRow(0x10E60, 0x10E7F, "Rumi Numeral Symbols")]
        [DataRow(0x1EC70, 0x1ECBF, "Indic Siyaq Numbers")]
        [DataRow(0x1ED00, 0x1ED4F, "Ottoman Siyaq Numbers")]
        [DataRow(0x1EE00, 0x1EEFF, "Arabic Mathematical Alphabetic Symbols")]
        [DataRow(0x04E00, 0X09FEF, "CJK Unified Ideographs")]
        [DataRow(0x03400, 0X04DBF, "CJK Unified Ideographs Extension A")]
        [DataRow(0x20000, 0X2A6DF, "CJK Unified Ideographs Extension B")]
        [DataRow(0x2A700, 0X2B73F, "CJK Unified Ideographs Extension C")]
        [DataRow(0x2B740, 0X2B81F, "CJK Unified Ideographs Extension D")]
        [DataRow(0x2B820, 0X2CEAF, "CJK Unified Ideographs Extension E")]
        [DataRow(0x2CEB0, 0X2EBEF, "CJK Unified Ideographs Extension F")]
        [DataRow(0x03007, 0X03007, "block CJK Symbols and Punctuation")]
        [DataRow(0x00400, 0x004FF, "Cyrillic")]
        [DataRow(0x00500, 0x0052F, "Cyrillic Supplement")]
        [DataRow(0x02DE0, 0x02DFF, "Cyrillic Extended-A")]
        [DataRow(0x0A640, 0x0A69F, "Cyrillic Extended-B")]
        [DataRow(0x01C80, 0x01C8F, "Cyrillic Extended-C")]
        [DataRow(0x01D2B, 0x01D78, "Phonetic Extensions")]
        [DataRow(0x0FE2E, 0x0FE2F, "Combining Half Marks")]
        public void NoVulnerability(int start, int end, string range)
        {
            var utf32s = Enumerable.Range(start, 1 + end - start);
            var vulnerabilities = utf32s.Where(utf32 => PreventUnicodeVulnerabilitiesBase.IsVulnerability(utf32));
            vulnerabilities.Should().BeEmpty(because: "U+{0:X}-U+{1:X} represents {2}.", start, end, range);
        }
    }
}

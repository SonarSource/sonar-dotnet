using System.IO;
using System.Text.RegularExpressions;
using SonarAnalyzer.Common;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common;

[TestClass]
public class ReadMeTest
{
    [TestMethod]
    public void MentionsTheRightNumbers()
    {
        var cs = RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.CSharp).Count();
        var vb = RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.VisualBasic).Count();

        var match = Regex.Match(Readme(), @"\[(?<cs>[0-9]{3,4})\+ C# rules\].+ and \[(?<vb>[0-9]{3,4})\+ VB\.");

        var cs_l = Int(match, nameof(cs));
        var vb_l = Int(match, nameof(vb));

        cs.Should().BeInRange(cs_l, cs_l + 10);
        vb.Should().BeInRange(vb_l, vb_l + 10);

        static string Readme()
        {
            var readme = new FileInfo("./Common/Resources/README.md");
            using var reader = readme.OpenText();
            return reader.ReadToEnd();
        }
        static int Int(Match match, string group) => int.Parse(match.Groups[group].Value);
    }
}

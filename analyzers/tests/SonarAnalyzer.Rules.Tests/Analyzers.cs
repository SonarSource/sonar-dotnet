using System.Linq;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using SonarAnalyzer.Rules.Tests.Framework;

namespace Analyzers
{
    public class Contains
    {
        private static readonly AnalyzerInfo[] Infos = AnalyzerInfo.FromAssembly(
            typeof(SonarAnalyzer.Rules.CSharp.ArrayCovariance).Assembly,
            typeof(SonarAnalyzer.Rules.VisualBasic.ArrayCreationLongSyntax).Assembly)
            .ToArray();

        [Test]
        public void Contain_545_Analyzers() =>
            Assert.AreEqual(545, Infos.Length);

        [Test]
        public void Contain_378_CSharp_Analyzers() =>
            Assert.AreEqual(378, Infos.Count(analyzer => analyzer.Language == LanguageNames.CSharp));

        [Test]
        public void Contain_167_VisualBasic_Analyzers() =>
            Assert.AreEqual(167, Infos.Count(analyzer => analyzer.Language == LanguageNames.VisualBasic));
    }
}

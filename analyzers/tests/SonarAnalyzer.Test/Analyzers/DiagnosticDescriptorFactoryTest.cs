/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

namespace SonarAnalyzer.Test.Analyzers
{
    [TestClass]
    public class DiagnosticDescriptorFactoryTest
    {
        [TestMethod]
        public void GetUtilityDescriptor_Should_Contain_NotConfigurable_CustomTag()
        {
            var result = DiagnosticDescriptorFactory.CreateUtility("Sxxx", "Title");
#if DEBUG
            result.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable);
#else
            result.CustomTags.Should().Contain(WellKnownDiagnosticTags.NotConfigurable);
#endif
        }

        [TestMethod]
        public void Create_ConfiguresProperties_CS()
        {
            var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, false);

            result.Id.Should().Be("Sxxxx");
            result.Title.ToString().Should().Be("Sxxxx Title");
            result.MessageFormat.ToString().Should().Be("Sxxxx Message");
            result.Category.Should().Be("Major Bug");
            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            result.IsEnabledByDefault.Should().BeTrue();
            result.Description.ToString().Should().Be("Sxxxx Description");
            result.HelpLinkUri.Should().Be("https://rules.sonarsource.com/csharp/RSPEC-xxxx");
            result.CustomTags.Should().BeEquivalentTo(LanguageNames.CSharp, DiagnosticDescriptorFactory.MainSourceScopeTag, DiagnosticDescriptorFactory.SonarWayTag);
        }

        [TestMethod]
        public void Create_ConfiguresProperties_VB()
        {
            var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.VisualBasic, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, false);

            result.Id.Should().Be("Sxxxx");
            result.Title.ToString().Should().Be("Sxxxx Title");
            result.MessageFormat.ToString().Should().Be("Sxxxx Message");
            result.Category.Should().Be("Major Bug");
            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            result.IsEnabledByDefault.Should().BeTrue();
            result.Description.ToString().Should().Be("Sxxxx Description");
            result.HelpLinkUri.Should().Be("https://rules.sonarsource.com/vbnet/RSPEC-xxxx");
            result.CustomTags.Should().BeEquivalentTo(LanguageNames.VisualBasic, DiagnosticDescriptorFactory.MainSourceScopeTag, DiagnosticDescriptorFactory.SonarWayTag);
        }

        [TestMethod]
        public void Create_FadeOutCode_HasUnnecessaryTag_HasInfoSeverity()
        {
            var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, true);

            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Info);
            result.CustomTags.Should().Contain(WellKnownDiagnosticTags.Unnecessary);
        }

        [TestMethod]
        public void Create_HasCorrectSonarWayTag()
        {
            CreateTags(true).Should().Contain(DiagnosticDescriptorFactory.SonarWayTag);
            CreateTags(false).Should().NotContain(DiagnosticDescriptorFactory.SonarWayTag);

            static IEnumerable<string> CreateTags(bool sonarWay) =>
                DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, sonarWay), "Sxxxx Message", null, false).CustomTags;
        }

        [TestMethod]
        public void Create_HasCorrectScopeTags()
        {
            CreateTags(SourceScope.Main).Should().Contain(DiagnosticDescriptorFactory.MainSourceScopeTag).And.NotContain(DiagnosticDescriptorFactory.TestSourceScopeTag);
            CreateTags(SourceScope.Tests).Should().Contain(DiagnosticDescriptorFactory.TestSourceScopeTag).And.NotContain(DiagnosticDescriptorFactory.MainSourceScopeTag);
            CreateTags(SourceScope.All).Should().Contain(DiagnosticDescriptorFactory.MainSourceScopeTag, DiagnosticDescriptorFactory.TestSourceScopeTag);

            static IEnumerable<string> CreateTags(SourceScope scope) =>
                DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(scope, true), "Sxxxx Message", null, false).CustomTags;
        }

        [TestMethod]
        public void Create_UnexpectedType_Throws()
        {
            var rule = new RuleDescriptor("Sxxxx", string.Empty, "Lorem Ipsum", string.Empty, string.Empty, SourceScope.Main, true, string.Empty);
            var f = () => DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, rule, string.Empty, null, false);
            f.Should().Throw<UnexpectedValueException>().WithMessage("Unexpected Type value: Lorem Ipsum");
        }

        [DataTestMethod]
        [DataRow("Minor", "BUG", "Minor Bug")]
        [DataRow("Major", "BUG", "Major Bug")]
        [DataRow("Major", "CODE_SMELL", "Major Code Smell")]
        [DataRow("Major", "VULNERABILITY", "Major Vulnerability")]
        [DataRow("Major", "SECURITY_HOTSPOT", "Major Security Hotspot")]
        [DataRow("Critical", "BUG", "Critical Bug")]
        [DataRow("Blocker", "BUG", "Blocker Bug")]
        [DataRow("Whatever Xxx", "BUG", "Whatever Xxx Bug")]
        public void Create_ComputesCategory(string severity, string type, string expected)
        {
            var rule = new RuleDescriptor("Sxxxx", string.Empty, type, severity, string.Empty, SourceScope.Main, true, string.Empty);
            DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, rule, "Sxxxx Message", null, false).Category.Should().Be(expected);
        }

        private static RuleDescriptor CreateRuleDescriptor(SourceScope scope, bool sonarWay) =>
            new("Sxxxx", "Sxxxx Title", "BUG", "Major", string.Empty, scope, sonarWay, "Sxxxx Description");
    }
}

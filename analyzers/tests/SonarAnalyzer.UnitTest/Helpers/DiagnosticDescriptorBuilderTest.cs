/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

extern alias csharp;
extern alias vbnet;
using System.Resources;
using Moq;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class DiagnosticDescriptorBuilderTest
    {
        private const string LanguageValue = "language";

        [TestMethod]
        public void GetHelpLink_CSharp()
        {
            var helpLink = DiagnosticDescriptorBuilder
                .GetHelpLink(csharp.SonarAnalyzer.RspecStrings.ResourceManager, "S1234");
            helpLink.Should().Be("https://rules.sonarsource.com/csharp/RSPEC-1234");
        }

        [TestMethod]
        public void GetHelpLink_VisualBasic()
        {
            var helpLink = DiagnosticDescriptorBuilder
                .GetHelpLink(vbnet.SonarAnalyzer.RspecStrings.ResourceManager, "S1234");
            helpLink.Should().Be("https://rules.sonarsource.com/vbnet/RSPEC-1234");
        }

        [TestMethod]
        public void GetDescriptor_SetsIsEnabledByDefaultToGivenValue()
        {
            // Arrange
            var diagnosticId = "FooBar";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, isActivatedByDefault: true);

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager, true)
                .IsEnabledByDefault
                .Should()
                .BeTrue();

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager, false)
                .IsEnabledByDefault
                .Should()
                .BeFalse();

            DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager)
                .IsEnabledByDefault
                .Should()
                .BeTrue();
        }

        [TestMethod]
        public void GetDescriptor_WhenIsActivatedByDefaultAndIdeVisibilityNotHidden_HasOnlySonarWayAndLanguageTags()
        {
            // Arrange
            var diagnosticId = "foo";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, true);

            // Act
            var result = DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager);

            // Assert
            result.CustomTags.Should().OnlyContain(DiagnosticDescriptorBuilder.SonarWayTag, DiagnosticDescriptorBuilder.MainSourceScopeTag, LanguageValue);
        }

        [TestMethod]
        public void GetDescriptor_WhenIsNotActivatedByDefaultAndIdeVisibilityNotHidden_ContainsOnlyLanguage()
        {
            // Arrange
            var diagnosticId = "foo";
            var mockedResourceManager = CreateMockedResourceManager(diagnosticId, false);

            // Act
            var result = DiagnosticDescriptorBuilder.GetDescriptor(diagnosticId, "", mockedResourceManager);

            // Assert
            result.CustomTags.Should().OnlyContain(DiagnosticDescriptorBuilder.MainSourceScopeTag, LanguageValue);
        }

        [TestMethod]
        public void GetUtilityDescriptor_Should_Contain_NotConfigurable_CustomTag()
        {
            var result = DiagnosticDescriptorBuilder.GetUtilityDescriptor("Foo", "");
#if DEBUG
            result.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable);
#else
            result.CustomTags.Should().Contain(WellKnownDiagnosticTags.NotConfigurable);
#endif
        }

        [TestMethod]
        public void Create_ConfiguresProperties_CS()
        {
            var result = DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", false);

            result.Id.Should().Be("Sxxxx");
            result.Title.ToString().Should().Be("Sxxxx Title");
            result.MessageFormat.ToString().Should().Be("Sxxxx Message");
            result.Category.Should().Be("Major Bug");
            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            result.IsEnabledByDefault.Should().BeTrue();
            result.Description.ToString().Should().Be("Sxxxx Description");
            result.HelpLinkUri.Should().Be("https://rules.sonarsource.com/csharp/RSPEC-xxxx");
            result.CustomTags.Should().OnlyContain(LanguageNames.CSharp, DiagnosticDescriptorBuilder.MainSourceScopeTag, DiagnosticDescriptorBuilder.SonarWayTag);
        }

        [TestMethod]
        public void Create_ConfiguresProperties_VB()
        {
            var result = DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.VisualBasic, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", false);

            result.Id.Should().Be("Sxxxx");
            result.Title.ToString().Should().Be("Sxxxx Title");
            result.MessageFormat.ToString().Should().Be("Sxxxx Message");
            result.Category.Should().Be("Major Bug");
            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
            result.IsEnabledByDefault.Should().BeTrue();
            result.Description.ToString().Should().Be("Sxxxx Description");
            result.HelpLinkUri.Should().Be("https://rules.sonarsource.com/vbnet/RSPEC-xxxx");
            result.CustomTags.Should().OnlyContain(LanguageNames.VisualBasic, DiagnosticDescriptorBuilder.MainSourceScopeTag, DiagnosticDescriptorBuilder.SonarWayTag);
        }

        [TestMethod]
        public void Create_FadeOutCode_HasUnnecessaryTag_HasInfoSeverity()
        {
            var result = DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", true);

            result.DefaultSeverity.Should().Be(DiagnosticSeverity.Info);
            result.CustomTags.Should().Contain(WellKnownDiagnosticTags.Unnecessary);
        }

        [TestMethod]
        public void Create_HasCorrectSonarWayTag()
        {
            CreateTags(true).Should().Contain(DiagnosticDescriptorBuilder.SonarWayTag);
            CreateTags(false).Should().NotContain(DiagnosticDescriptorBuilder.SonarWayTag);

            static IEnumerable<string> CreateTags(bool sonarWay) =>
                DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, sonarWay), "Sxxxx Message", false).CustomTags;
        }

        [TestMethod]
        public void Create_HasCorrectScopeTags()
        {
            CreateTags(SourceScope.Main).Should().Contain(DiagnosticDescriptorBuilder.MainSourceScopeTag).And.NotContain(DiagnosticDescriptorBuilder.TestSourceScopeTag);
            CreateTags(SourceScope.Tests).Should().Contain(DiagnosticDescriptorBuilder.TestSourceScopeTag).And.NotContain(DiagnosticDescriptorBuilder.MainSourceScopeTag);
            CreateTags(SourceScope.All).Should().Contain(DiagnosticDescriptorBuilder.MainSourceScopeTag, DiagnosticDescriptorBuilder.TestSourceScopeTag);

            static IEnumerable<string> CreateTags(SourceScope scope) =>
                DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(scope, true), "Sxxxx Message", false).CustomTags;
        }

        [TestMethod]
        public void Create_UnexpectedType_Throws()
        {
            var rule = new RuleDescriptor("Sxxxx", string.Empty, "Lorem Ipsum", string.Empty, SourceScope.Main, true, string.Empty);
            Func<DiagnosticDescriptor> f = () => DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, rule, string.Empty, false);
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
            var rule = new RuleDescriptor("Sxxxx", string.Empty, type, severity, SourceScope.Main, true, string.Empty);
            DiagnosticDescriptorBuilder.Create(AnalyzerLanguage.CSharp, rule, "Sxxxx Message", false).Category.Should().Be(expected);
        }

        private static RuleDescriptor CreateRuleDescriptor(SourceScope scope, bool sonarWay) =>
            new("Sxxxx", "Sxxxx Title", "BUG", "Major", scope, sonarWay, "Sxxxx Description");

        private static ResourceManager CreateMockedResourceManager(string diagnosticId, bool isActivatedByDefault)
        {
            var mockedResourceManager = new Mock<ResourceManager>();
            mockedResourceManager.Setup(x => x.GetString("HelpLinkFormat")).Returns("bar");
            mockedResourceManager.Setup(x => x.GetString("RoslynLanguage")).Returns(LanguageValue);

            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Title")).Returns("title");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Category")).Returns("category");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Description")).Returns("description");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_Scope")).Returns("Main");
            mockedResourceManager.Setup(x => x.GetString($"{diagnosticId}_IsActivatedByDefault")).Returns(isActivatedByDefault.ToString());

            return mockedResourceManager.Object;
        }
    }
}

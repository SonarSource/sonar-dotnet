/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Test.Analyzers;

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
        var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, false, false);

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
        var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.VisualBasic, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, false, false);

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
        var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, true, false);

        result.DefaultSeverity.Should().Be(DiagnosticSeverity.Info);
        result.CustomTags.Should().Contain(WellKnownDiagnosticTags.Unnecessary);
    }

    [CombinatorialDataTestMethod]
    public void Create_CompilationEndDiagnostic([DataValues(LanguageNames.CSharp, LanguageNames.VisualBasic)] string language, [DataValues(true, false)] bool isCompilationEnd)
    {
        var result = DiagnosticDescriptorFactory.Create(AnalyzerLanguage.FromName(language), CreateRuleDescriptor(SourceScope.Main, true), "Sxxxx Message", null, false, isCompilationEnd);

        if (isCompilationEnd)
        {
            result.CustomTags.Should().Contain(DiagnosticDescriptorFactory.CompilationEnd);
        }
        else
        {
            result.CustomTags.Should().NotContain(DiagnosticDescriptorFactory.CompilationEnd);
        }
    }

    [TestMethod]
    public void Create_HasCorrectSonarWayTag()
    {
        CreateTags(true).Should().Contain(DiagnosticDescriptorFactory.SonarWayTag);
        CreateTags(false).Should().NotContain(DiagnosticDescriptorFactory.SonarWayTag);

        static IEnumerable<string> CreateTags(bool sonarWay) =>
            DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(SourceScope.Main, sonarWay), "Sxxxx Message", null, false, false).CustomTags;
    }

    [TestMethod]
    public void Create_HasCorrectScopeTags()
    {
        CreateTags(SourceScope.Main).Should().Contain(DiagnosticDescriptorFactory.MainSourceScopeTag).And.NotContain(DiagnosticDescriptorFactory.TestSourceScopeTag);
        CreateTags(SourceScope.Tests).Should().Contain(DiagnosticDescriptorFactory.TestSourceScopeTag).And.NotContain(DiagnosticDescriptorFactory.MainSourceScopeTag);
        CreateTags(SourceScope.All).Should().Contain(DiagnosticDescriptorFactory.MainSourceScopeTag, DiagnosticDescriptorFactory.TestSourceScopeTag);

        static IEnumerable<string> CreateTags(SourceScope scope) =>
            DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, CreateRuleDescriptor(scope, true), "Sxxxx Message", null, false, false).CustomTags;
    }

    [TestMethod]
    public void Create_UnexpectedType_Throws()
    {
        var rule = new RuleDescriptor("Sxxxx", string.Empty, "Lorem Ipsum", string.Empty, string.Empty, SourceScope.Main, true, string.Empty);
        var f = () => DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, rule, string.Empty, null, false, false);
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
        DiagnosticDescriptorFactory.Create(AnalyzerLanguage.CSharp, rule, "Sxxxx Message", null, false, false).Category.Should().Be(expected);
    }

    private static RuleDescriptor CreateRuleDescriptor(SourceScope scope, bool sonarWay) =>
        new("Sxxxx", "Sxxxx Title", "BUG", "Major", string.Empty, scope, sonarWay, "Sxxxx Description");
}

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

using System.Reflection;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.Helpers;

using static SonarAnalyzer.UnitTest.TestHelper;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class RuleTest
    {
        [Ignore][TestMethod]
        public void CodeFixes_Named_Properly()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                var analyzerName = codeFix.FullName.Replace("CodeFix", string.Empty);
                codeFix.Assembly.GetType(analyzerName).Should().NotBeNull("CodeFix '{0}' has no matching DiagnosticAnalyzer.", codeFix.Name);
            }
        }

        [Ignore][TestMethod]
        public void RulesDoNotThrow_CS()
        {
            var analyzers = RuleFinder.CreateAnalyzers(AnalyzerLanguage.CSharp, true).ToArray();

            VerifyNoExceptionThrown(@"TestCases\RuleFailure\InvalidSyntax.cs", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\SpecialCases.cs", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\PerformanceTestCases.cs", analyzers, CompilationErrorBehavior.Ignore);
        }

        [Ignore][TestMethod]
        public void RulesDoNotThrow_VB()
        {
            var analyzers = RuleFinder.CreateAnalyzers(AnalyzerLanguage.VisualBasic, true).ToArray();

            VerifyNoExceptionThrown(@"TestCases\RuleFailure\InvalidSyntax.vb", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\SpecialCases.vb", analyzers, CompilationErrorBehavior.Ignore);
        }

        [Ignore][TestMethod]
        public void AllAnalyzers_InheritSonarDiagnosticAnalyzer()
        {
            foreach (var analyzer in RuleFinder.AllAnalyzerTypes)
            {
                analyzer.Should().BeAssignableTo<SonarDiagnosticAnalyzer>($"{analyzer.Name} is not a subclass of SonarDiagnosticAnalyzer");
            }
        }

        [Ignore][TestMethod]
        public void CodeFixes_InheritSonarCodeFix()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                codeFix.Should().BeAssignableTo<SonarCodeFix>($"{codeFix.Name} is not a subclass of SonarCodeFix");
            }
        }

        [Ignore][TestMethod]
        public void Rules_WithDiagnosticAnalyzerAttribute_AreNotAbstract()
        {
            foreach (var analyzer in RuleFinder.AllAnalyzerTypes)
            {
                analyzer.IsAbstract.Should().BeFalse();
            }
        }

        [Ignore][TestMethod]
        public void CodeFixes_WithExportCodeFixProviderAttribute_AreNotAbstract()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                codeFix.IsAbstract.Should().BeFalse();
            }
        }

        [Ignore][TestMethod]
        public void Verify_ConcurrentExecutionIsEnabledByDefault()
        {
            var reader = new ConcurrentExecutionReader();
            reader.IsConcurrentExecutionEnabled.Should().BeNull();
            VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", new[] { reader });
            reader.IsConcurrentExecutionEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("true")]
        [DataRow("tRUE")]
        [DataRow("loremipsum")]
        public void Verify_ConcurrentExecutionIsExplicitlyEnabled(string value)
        {
            using var scope = new EnvironmentVariableScope(false);
            scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, value);
            var reader = new ConcurrentExecutionReader();
            reader.IsConcurrentExecutionEnabled.Should().BeNull();
            VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", new[] { reader });
            reader.IsConcurrentExecutionEnabled.Should().BeTrue();
        }

        [Ignore][TestMethod]
        [DataRow("false")]
        [DataRow("fALSE")]
        public void Verify_ConcurrentExecutionIsExplicitlyDisabled(string value)
        {
            using var scope = new EnvironmentVariableScope(false);
            scope.SetVariable(SonarDiagnosticAnalyzer.EnableConcurrentExecutionVariable, value);
            var reader = new ConcurrentExecutionReader();
            reader.IsConcurrentExecutionEnabled.Should().BeNull();
            VerifyNoExceptionThrown("TestCases\\AsyncVoidMethod.cs", new[] { reader });
            reader.IsConcurrentExecutionEnabled.Should().BeFalse();
        }

        [Ignore][TestMethod]
        public void AllParameterizedRules_AreDisabledByDefault() =>
            RuleFinder.RuleAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Where(analyzer => !IsSecurityHotspot(analyzer))
                .ToList()
                .ForEach(diagnostic => diagnostic.IsEnabledByDefault.Should().BeFalse());

        [Ignore][TestMethod]
        public void AllRulesEnabledByDefault_ContainSonarWayCustomTag()
        {
            var descriptors = RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics)
                // Security hotspots are enabled by default, but they will report issues only
                // when their ID is contained in SonarLint.xml
                .Where(descriptor => !IsSecurityHotspot(descriptor));

            foreach (var descriptor in descriptors)
            {
                if (descriptor.IsEnabledByDefault)
                {
                    descriptor.CustomTags.Should().Contain(DiagnosticDescriptorFactory.SonarWayTag, $"{descriptor.Id} should be in SonarWay");
                }
            }
        }

        [Ignore][TestMethod]
        public void AllCSharpRules_HaveCSharpTag() =>
            SupportedDiagnostics(AnalyzerLanguage.CSharp).Should().OnlyContain(diagnostic => diagnostic.CustomTags.Contains(LanguageNames.CSharp));

        [Ignore][TestMethod]
        public void AllVbNetRules_HaveVbNetTag() =>
            SupportedDiagnostics(AnalyzerLanguage.VisualBasic).Should().OnlyContain(diagnostic => diagnostic.CustomTags.Contains(LanguageNames.VisualBasic));

        [Ignore][TestMethod]
        public void DeprecatedRules_AreNotInSonarWay()
        {
            foreach (var diagnostic in RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics).Where(IsDeprecated))
            {
                IsSonarWay(diagnostic).Should().BeFalse($"{diagnostic.Id} is deprecated and should be removed from SonarWay.");
            }
        }

        [Ignore][TestMethod]
        public void AllRules_DoNotHaveUtilityTag()
        {
            foreach (var diagnostic in RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().NotContain(DiagnosticDescriptorFactory.UtilityTag);
            }
        }

        [Ignore][TestMethod]
        public void UtilityAnalyzers_HaveUtilityTag()
        {
            foreach (var diagnostic in RuleFinder.UtilityAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().Contain(DiagnosticDescriptorFactory.UtilityTag);
            }
        }

        [Ignore][TestMethod]
        public void AllRules_SonarWayTagPresenceMatchesIsEnabledByDefault()
        {
            var parameterized = RuleFinder.RuleAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .SelectMany(type => ((DiagnosticAnalyzer)Activator.CreateInstance(type)).SupportedDiagnostics)
                .ToHashSet();

            foreach (var diagnostic in RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                if (IsSecurityHotspot(diagnostic))
                {
                    // Security hotspots are enabled by default, but they will report issues only when their ID is contained in SonarLint.xml
                    // DiagnosticDescriptorFactory adds WellKnownDiagnosticTags.NotConfigurable to prevent rule supression and deactivation.
                    diagnostic.IsEnabledByDefault.Should().BeTrue($"{diagnostic.Id} should be enabled by default");
                }
                else if (IsDeprecated(diagnostic))
                {
                    // Deprecated rules should be removed from SonarWay
                    diagnostic.IsEnabledByDefault.Should().BeFalse($"{diagnostic.Id} is deprecated and should be disabled by default (removed from SonarWay)");
                }
                else if (parameterized.Contains(diagnostic))
                {
                    // Even if a a parametrized rule is in Sonar way profile, it is still disabled by default.
                    // See https://github.com/SonarSource/sonar-dotnet/issues/1274
                    diagnostic.IsEnabledByDefault.Should().BeFalse($"{diagnostic.Id} has parameters and should be disabled by default");
                }
                else if (IsSonarWay(diagnostic))
                {
                    diagnostic.IsEnabledByDefault.Should().BeTrue($"{diagnostic.Id} is in SonarWay");
                }
                else
                {
                    diagnostic.IsEnabledByDefault.Should().BeFalse($"{diagnostic.Id} is not in SonarWay");
                }
            }
        }

        [Ignore][TestMethod]
        public void OnlySecurityHotspots_AreNotConfigurable_CS() =>
            OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage.CSharp);

        [Ignore][TestMethod]
        public void OnlySecurityHotspots_AreNotConfigurable_VB() =>
            OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage.VisualBasic);

        [Ignore][TestMethod]
        public void RulesAreInNamespace_CS()
        {
            foreach (var type in RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.CSharp))
            {
                type.Namespace.Should().Be("SonarAnalyzer.Rules.CSharp", $"rule {type.FullName} will not be recognized by the ParseBuildOutput tool on Peach.");
            }
        }

        [Ignore][TestMethod]
        public void RulesAreInNamespace_VB()
        {
            foreach (var type in RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.VisualBasic))
            {
                type.Namespace.Should().Be("SonarAnalyzer.Rules.VisualBasic", $"rule {type.FullName} will not be recognized by the ParseBuildOutput tool on Peach.");
            }
        }

        [AssertionMethod]
        private static void OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage language)
        {
            foreach (var diagnostic in SupportedDiagnostics(language))
            {
                if (IsSecurityHotspot(diagnostic))
                {
                    diagnostic.CustomTags.Should().Contain(WellKnownDiagnosticTags.NotConfigurable, diagnostic.Id + " is a Security Hotspot and should not be configurable");
                }
                else
                {
                    diagnostic.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable, diagnostic.Id + " is not a Security Hotspot and should be configurable");
                }
            }
        }

        private static IEnumerable<DiagnosticDescriptor> SupportedDiagnostics(AnalyzerLanguage language) =>
            RuleFinder.GetAnalyzerTypes(language).SelectMany(SupportedDiagnostics);

        private static IEnumerable<DiagnosticDescriptor> SupportedDiagnostics(Type type) =>
            ((DiagnosticAnalyzer)Activator.CreateInstance(type)).SupportedDiagnostics;

        private static bool IsSonarWay(DiagnosticDescriptor diagnostic) =>
            diagnostic.CustomTags.Contains(DiagnosticDescriptorFactory.SonarWayTag);

        private static bool IsDeprecated(DiagnosticDescriptor diagnostic)
        {
            return LanguageRules()[diagnostic.Id].Status == "deprecated";

            Dictionary<string, RuleDescriptor> LanguageRules()
            {
                if (diagnostic.CustomTags.Contains(LanguageNames.CSharp))
                {
                    return csharp::SonarAnalyzer.RuleCatalog.Rules;
                }
                else if (diagnostic.CustomTags.Contains(LanguageNames.VisualBasic))
                {
                    return vbnet::SonarAnalyzer.RuleCatalog.Rules;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(AllCSharpRules_HaveCSharpTag)} or {nameof(AllVbNetRules_HaveVbNetTag)} should fail, fix them first.");
                }
            }
        }

        private static void VerifyNoExceptionThrown(string path, DiagnosticAnalyzer[] diagnosticAnalyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();

            var diagnostics = DiagnosticVerifier.GetAnalyzerDiagnostics(compilation, diagnosticAnalyzers, checkMode);
            DiagnosticVerifier.VerifyNoExceptionThrown(diagnostics);
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class ConcurrentExecutionReader : SonarDiagnosticAnalyzer
        {
            private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorFactory.CreateUtility("S9999", "Rule test");

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
            public new bool? IsConcurrentExecutionEnabled { get; private set; }

            protected override void Initialize(SonarAnalysisContext context) =>
                IsConcurrentExecutionEnabled = EnableConcurrentExecution;
        }
    }
}

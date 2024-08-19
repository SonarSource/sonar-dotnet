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

extern alias csharp;
extern alias vbnet;

using System.Reflection;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using SonarAnalyzer.Test.PackagingTests;
using SonarAnalyzer.Test.TestFramework;

namespace SonarAnalyzer.Test.Common
{
    [TestClass]
    public class RuleTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CodeFixes_Named_Properly()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                var analyzerName = codeFix.FullName.Replace("CodeFix", string.Empty);
                codeFix.Assembly.GetType(analyzerName).Should().NotBeNull("CodeFix '{0}' has no matching DiagnosticAnalyzer.", codeFix.Name);
            }
        }

        [TestMethod]
        public void RulesDoNotThrow_CS()
        {
            var analyzers = RuleFinder.CreateAnalyzers(AnalyzerLanguage.CSharp, true).ToArray();

            VerifyNoExceptionThrown(@"TestCases\RuleFailure\InvalidSyntax.cs", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\SpecialCases.cs", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\PerformanceTestCases.cs", analyzers, CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void RulesDoNotThrow_VB()
        {
            var analyzers = RuleFinder.CreateAnalyzers(AnalyzerLanguage.VisualBasic, true).ToArray();

            VerifyNoExceptionThrown(@"TestCases\RuleFailure\InvalidSyntax.vb", analyzers, CompilationErrorBehavior.Ignore);
            VerifyNoExceptionThrown(@"TestCases\RuleFailure\SpecialCases.vb", analyzers, CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void AllAnalyzers_InheritSonarDiagnosticAnalyzer()
        {
            foreach (var analyzer in RuleFinder.AllAnalyzerTypes)
            {
                analyzer.Should().BeAssignableTo<SonarDiagnosticAnalyzer>($"{analyzer.Name} is not a subclass of SonarDiagnosticAnalyzer");
            }
        }

        [TestMethod]
        public void AllClassesWithDiagnosticAnalyzerAttribute_InheritSonarDiagnosticAnalyzer()
        {
            foreach (var analyzer in RuleFinder.AllTypesWithDiagnosticAnalyzerAttribute)
            {
                analyzer.Should().BeAssignableTo<SonarDiagnosticAnalyzer>($"{analyzer.Name} is not a subclass of SonarDiagnosticAnalyzer");
            }
        }

        [TestMethod]
        public void CodeFixes_InheritSonarCodeFix()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                codeFix.Should().BeAssignableTo<SonarCodeFix>($"{codeFix.Name} is not a subclass of SonarCodeFix");
            }
        }

        [TestMethod]
        public void Rules_WithDiagnosticAnalyzerAttribute_AreNotAbstract()
        {
            foreach (var analyzer in RuleFinder.AllAnalyzerTypes)
            {
                analyzer.IsAbstract.Should().BeFalse();
            }
        }

        [TestMethod]
        public void CodeFixes_WithExportCodeFixProviderAttribute_AreNotAbstract()
        {
            foreach (var codeFix in RuleFinder.CodeFixTypes)
            {
                codeFix.IsAbstract.Should().BeFalse();
            }
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void AllParameterizedRules_AreDisabledByDefault() =>
            RuleFinder.RuleAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Where(analyzer => !IsSecurityHotspot(analyzer))
                .ToList()
                .ForEach(diagnostic => diagnostic.IsEnabledByDefault.Should().BeFalse());

        [TestMethod]
        public void ParameterKey_DoesNotContainWhitespace()
        {
            foreach (var analyzer in RuleFinder.RuleAnalyzerTypes.Where(RuleFinder.IsParameterized))
            {
                foreach (var parameter in analyzer.GetRuntimeProperties().Select(x => x.GetCustomAttributes<RuleParameterAttribute>().SingleOrDefault()).WhereNotNull())
                {
                    parameter.Key.Any(char.IsWhiteSpace).Should().BeFalse($"{analyzer.FullName} should not contain whitespace in '{parameter.Key}' parameter key.");
                }
            }
        }

        [TestMethod]
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

        [TestMethod]
        public void AllCSharpRules_HaveCSharpTag() =>
            SupportedDiagnostics(AnalyzerLanguage.CSharp).Should().OnlyContain(diagnostic => diagnostic.CustomTags.Contains(LanguageNames.CSharp));

        [TestMethod]
        public void AllVbNetRules_HaveVbNetTag() =>
            SupportedDiagnostics(AnalyzerLanguage.VisualBasic).Should().OnlyContain(diagnostic => diagnostic.CustomTags.Contains(LanguageNames.VisualBasic));

        [TestMethod]
        public void DeprecatedRules_AreNotInSonarWay()
        {
            foreach (var diagnostic in RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics).Where(IsDeprecated))
            {
                IsSonarWay(diagnostic).Should().BeFalse($"{diagnostic.Id} is deprecated and should be removed from SonarWay.");
            }
        }

        [TestMethod]
        public void AllRules_DoNotHaveUtilityTag()
        {
            foreach (var diagnostic in RuleFinder.RuleAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().NotContain(DiagnosticDescriptorFactory.UtilityTag);
            }
        }

        [TestMethod]
        public void UtilityAnalyzers_HaveUtilityTag()
        {
            foreach (var diagnostic in RuleFinder.UtilityAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().Contain(DiagnosticDescriptorFactory.UtilityTag);
            }
        }

        [TestMethod]
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

        [TestMethod]
        public void AllRulesAreConfigurable_CS() =>
            AllRulesAreConfigurable(AnalyzerLanguage.CSharp);

        [TestMethod]
        public void AllRulesAreConfigurable_VB() =>
            AllRulesAreConfigurable(AnalyzerLanguage.VisualBasic);

        [TestMethod]
        public void RulesAreInNamespace_CS()
        {
            foreach (var type in RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.CSharp))
            {
                type.Namespace.Should().Be("SonarAnalyzer.Rules.CSharp", $"rule {type.FullName} will not be recognized by the ParseBuildOutput tool on Peach.");
            }
        }

        [TestMethod]
        public void RulesAreInNamespace_VB()
        {
            foreach (var type in RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.VisualBasic))
            {
                type.Namespace.Should().Be("SonarAnalyzer.Rules.VisualBasic", $"rule {type.FullName} will not be recognized by the ParseBuildOutput tool on Peach.");
            }
        }

        [DataTestMethod]
        [DataRow(@"SymbolicExecution\Roslyn\NullPointerDereference.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_SymbolicExecutionRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder()
                .AddAnalyzer(() => new SymbolicExecutionRunner(AnalyzerConfiguration.AlwaysEnabled))
                .AddPaths(@"SymbolicExecution\Roslyn\NullPointerDereference.cs")
                .WithOnlyDiagnostics(NullPointerDereference.S2259);
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        [DataTestMethod]
        [DataRow("ClassNotInstantiatable.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_SymbolBasedRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder<ClassNotInstantiatable>().AddPaths("ClassNotInstantiatable.cs");
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        [DataTestMethod]
        [DataRow("AbstractTypesShouldNotHaveConstructors.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_SyntaxNodesBasedRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder<AbstractTypesShouldNotHaveConstructors>().AddPaths("AbstractTypesShouldNotHaveConstructors.cs");
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        [DataTestMethod]
        [DataRow("FileLines20.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_SyntaxTreeBasedRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder().AddAnalyzer(() => new FileLines { Maximum = 10 }).AddPaths("FileLines20.cs").WithAutogenerateConcurrentFiles(false);
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        [DataTestMethod]
        [DataRow(@"Hotspots\LooseFilePermissions.Windows.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_CompilationStartBasedRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder().AddAnalyzer(() => new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled)).AddPaths(@"Hotspots\LooseFilePermissions.Windows.cs");
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        [DataTestMethod]
        [DataRow("UnusedPrivateMember.cs", true)]
        [DataRow("SomeOtherFile.cs", false)]
        public void UnchangedFiles_ReportDiagnosticIfNonGeneratedBasedRule(string unchangedFileName, bool expectEmptyResults)
        {
            var builder = new VerifierBuilder<UnusedPrivateMember>().AddPaths("UnusedPrivateMember.cs");
            UnchangedFiles_Verify(builder, unchangedFileName, expectEmptyResults);
        }

        private static void AllRulesAreConfigurable(AnalyzerLanguage language)
        {
            foreach (var diagnostic in SupportedDiagnostics(language))
            {
                diagnostic.CustomTags.Should().NotContain(WellKnownDiagnosticTags.NotConfigurable, diagnostic.Id + " should be configurable");
            }
        }

        private void UnchangedFiles_Verify(VerifierBuilder builder, string unchangedFileName, bool expectEmptyResults)
        {
            builder = builder.WithConcurrentAnalysis(false).WithAdditionalFilePath(AnalysisScaffolding.CreateSonarProjectConfigWithUnchangedFiles(TestContext, unchangedFileName));
            if (expectEmptyResults)
            {
                builder.VerifyNoIssuesIgnoreErrors();
            }
            else
            {
                builder.Verify();
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

        private static void VerifyNoExceptionThrown(string path, DiagnosticAnalyzer[] analyzers, CompilationErrorBehavior checkMode = CompilationErrorBehavior.Default)
        {
            var compilation = SolutionBuilder
                .Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddDocument(path)
                .GetCompilation();
            ((Action)(() => DiagnosticVerifier.AnalyzerDiagnostics(compilation, analyzers, checkMode))).Should().NotThrow();
        }

        private static bool IsSecurityHotspot(DiagnosticDescriptor diagnostic)
        {
            var type = RuleTypeMappingCS.Rules.GetValueOrDefault(diagnostic.Id) ?? RuleTypeMappingVB.Rules.GetValueOrDefault(diagnostic.Id);
            return type == "SECURITY_HOTSPOT";
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

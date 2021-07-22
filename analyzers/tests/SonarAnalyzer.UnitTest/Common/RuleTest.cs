/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Resources;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using SonarAnalyzer.Utilities;

using static SonarAnalyzer.UnitTest.TestHelper;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class RuleTest
    {
        private const string IsConcurrentProcessing = "SONAR_DOTNET_ENABLE_CONCURRENT_PROCESSING";

        [TestMethod]
        public void DiagnosticAnalyzerHasRuleAttribute()
        {
            foreach (var analyzer in new RuleFinder().AllAnalyzerTypes)
            {
                var ruleDescriptors = analyzer.GetCustomAttributes<RuleAttribute>();
                ruleDescriptors.Should().NotBeEmpty("RuleAttribute is missing from DiagnosticAnalyzer '{0}'", analyzer.Name);
            }
        }

        [TestMethod]
        public void AbstractDiagnosticAnalyzer_Should_Have_No_RuleAttribute()
        {
            var analyzers = RuleFinder.PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)) && t.IsAbstract);
            foreach (var analyzer in analyzers)
            {
                analyzer.GetCustomAttributes<RuleAttribute>().Should().BeEmpty("At least one RuleAttribute is added to the abstract DiagnosticAnalyzer '{0}'", analyzer.Name);
            }
        }

        [TestMethod]
        public void CodeFixProviders_Named_Properly()
        {
            foreach (var codeFixProvider in GetCodeFixProviderTypes(RuleFinder.PackagedRuleAssemblies))
            {
                var analyzerName = codeFixProvider.FullName.Replace(RuleDetailBuilder.CodeFixProviderSuffix, "");
                codeFixProvider.Assembly.GetType(analyzerName).Should().NotBeNull("CodeFixProvider '{0}' has no matching DiagnosticAnalyzer.", codeFixProvider.Name);
            }
        }

        [TestMethod]
        public void CodeFixProviders_Have_Title()
        {
            foreach (var codeFixProvider in GetCodeFixProviderTypes(RuleFinder.PackagedRuleAssemblies))
            {
                RuleDetailBuilder.GetCodeFixTitles(codeFixProvider).Should().NotBeEmpty("CodeFixProvider '{0}' has no title field.", codeFixProvider.Name);
            }
        }

        [TestMethod]
        public void RulesDoNotThrow_CS()
        {
            var analyzers = RuleFinder.GetAnalyzers(AnalyzerLanguage.CSharp).ToList();

            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\InvalidSyntax.cs", analyzers, CompilationErrorBehavior.Ignore);
            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\SpecialCases.cs", analyzers, CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void RulesDoNotThrow_VB()
        {
            var analyzers = RuleFinder.GetAnalyzers(AnalyzerLanguage.VisualBasic).ToList();

            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\InvalidSyntax.vb", analyzers, CompilationErrorBehavior.Ignore);
            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\SpecialCases.vb", analyzers, CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void SonarDiagnosticAnalyzer_IsUsedInAllRules()
        {
            var analyzers = RuleFinder.PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)) && t != typeof(SonarDiagnosticAnalyzer));

            foreach (var analyzer in analyzers)
            {
                analyzer.Should().BeAssignableTo<SonarDiagnosticAnalyzer>($"{analyzer.Name} is not a subclass of SonarDiagnosticAnalyzer");
            }
        }

        [TestMethod]
        public void Verify_ConcurrentProcessingDisabledByDefault()
        {
            var retriever = new ConcurrentProcessingRetriever();
            retriever.IsConcurrentProcessingDisabled.Should().BeNull();
            Verifier.VerifyAnalyzer(new[] { "TestCasesForRuleFailure\\SpecialCases.cs" }, retriever);
            retriever.IsConcurrentProcessingDisabled.Should().BeTrue();
        }

        [TestMethod]
        public void Verify_ConcurrentProcessingGetsEnabled()
        {
            var variableValue = Environment.GetEnvironmentVariable(IsConcurrentProcessing);
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, "true");
            var retriever = new ConcurrentProcessingRetriever();
            retriever.IsConcurrentProcessingDisabled.Should().BeNull();
            Verifier.VerifyAnalyzer(new[] { "TestCasesForRuleFailure\\SpecialCases.cs" }, retriever);
            retriever.IsConcurrentProcessingDisabled.Should().BeFalse();
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, variableValue);
        }

        [TestMethod]
        public void Verify_ConcurrentProcessingGetsExplicitlyDisabled()
        {
            var variableValue = Environment.GetEnvironmentVariable(IsConcurrentProcessing);
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, "false");
            var retriever = new ConcurrentProcessingRetriever();
            retriever.IsConcurrentProcessingDisabled.Should().BeNull();
            Verifier.VerifyAnalyzer(new[] { "TestCasesForRuleFailure\\SpecialCases.cs" }, retriever);
            retriever.IsConcurrentProcessingDisabled.Should().BeTrue();
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, variableValue);
        }

        [TestMethod]
        public void Verify_ConcurrentProcessingGetsDisabledOnWrongValue()
        {
            var variableValue = Environment.GetEnvironmentVariable(IsConcurrentProcessing);
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, "loremipsum");
            var retriever = new ConcurrentProcessingRetriever();
            retriever.IsConcurrentProcessingDisabled.Should().BeNull();
            Verifier.VerifyAnalyzer(new[] { "TestCasesForRuleFailure\\SpecialCases.cs" }, retriever);
            retriever.IsConcurrentProcessingDisabled.Should().BeTrue();
            Environment.SetEnvironmentVariable(IsConcurrentProcessing, variableValue);
        }

        [TestMethod]
        public void AllParameterizedRules_AreDisabledByDefault() =>
            new RuleFinder().AllAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .Where(analyzer => !IsSecurityHotspot(analyzer))
                .ToList()
                .ForEach(diagnostic => diagnostic.IsEnabledByDefault.Should().BeFalse());

        [TestMethod]
        public void AllRulesEnabledByDefault_ContainSonarWayCustomTag()
        {
            var descriptors = new RuleFinder().AllAnalyzerTypes.SelectMany(SupportedDiagnostics)
                // Security hotspots are enabled by default, but they will report issues only
                // when their ID is contained in SonarLint.xml
                .Where(descriptor => !IsSecurityHotspot(descriptor));

            foreach (var descriptor in descriptors)
            {
                if (descriptor.IsEnabledByDefault)
                {
                    descriptor.CustomTags.Should().Contain(DiagnosticDescriptorBuilder.SonarWayTag, $"{descriptor.Id} should be in SonarWay");
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
            foreach (var diagnostic in new RuleFinder().AllAnalyzerTypes.SelectMany(SupportedDiagnostics).Where(IsDeprecated))
            {
                IsSonarWay(diagnostic).Should().BeFalse($"{diagnostic.Id} is deprecated and should be removed from SonarWay.");
            }
        }

        [TestMethod]
        public void AllRules_DoNotHaveUtilityTag()
        {
            foreach (var diagnostic in new RuleFinder().AllAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().NotContain(DiagnosticDescriptorBuilder.UtilityTag);
            }
        }

        [TestMethod]
        public void UtilityAnalyzers_HaveUtilityTag()
        {
            foreach (var diagnostic in new RuleFinder().UtilityAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                diagnostic.CustomTags.Should().Contain(DiagnosticDescriptorBuilder.UtilityTag);
            }
        }

        [TestMethod]
        public void AllRules_SonarWayTagPresenceMatchesIsEnabledByDefault()
        {
            var parameterized = new RuleFinder().AllAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .SelectMany(type => ((DiagnosticAnalyzer)Activator.CreateInstance(type)).SupportedDiagnostics)
                .ToHashSet();

            foreach (var diagnostic in new RuleFinder().AllAnalyzerTypes.SelectMany(SupportedDiagnostics))
            {
                if (IsSecurityHotspot(diagnostic))
                {
                    // Security hotspots are enabled by default, but they will report issues only when their ID is contained in SonarLint.xml
                    // Rule activation is done in DiagnosticDescriptorBuilder.WithNotConfigurable() to prevent rule supression and deactivation.
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
        [TestCategory("Hotspot")]
        public void OnlySecurityHotspots_AreNotConfigurable_CS() =>
            OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage.CSharp);

        [TestMethod]
        [TestCategory("Hotspot")]
        public void OnlySecurityHotspots_AreNotConfigurable_VB() =>
            OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage.VisualBasic);

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
            new RuleFinder().GetAnalyzerTypes(language).SelectMany(SupportedDiagnostics);

        private static IEnumerable<DiagnosticDescriptor> SupportedDiagnostics(Type type) =>
            typeof(DiagnosticAnalyzer).IsAssignableFrom(type)
                ? ((DiagnosticAnalyzer)Activator.CreateInstance(type)).SupportedDiagnostics
                : ((IRuleFactory)Activator.CreateInstance(type)).SupportedDiagnostics;

        private static IEnumerable<Type> GetCodeFixProviderTypes(IEnumerable<Assembly> assemblies) =>
            assemblies.SelectMany(assembly => assembly.GetTypes()).Where(t => t.IsSubclassOf(typeof(SonarCodeFixProvider)));

        private static bool IsSonarWay(DiagnosticDescriptor diagnostic) =>
            diagnostic.CustomTags.Contains(DiagnosticDescriptorBuilder.SonarWayTag);

        private static bool IsDeprecated(DiagnosticDescriptor diagnostic)
        {
            return LanguageResources().GetString(diagnostic.Id + "_Status") switch
            {
                null => throw new InvalidOperationException($"Missing {diagnostic.Id}_Status in {nameof(csharp::SonarAnalyzer.RspecStrings)} resources"),
                "deprecated" => true,
                _ => false
            };

            ResourceManager LanguageResources()
            {
                if (diagnostic.CustomTags.Contains(LanguageNames.CSharp))
                {
                    return csharp::SonarAnalyzer.RspecStrings.ResourceManager;
                }
                else if (diagnostic.CustomTags.Contains(LanguageNames.VisualBasic))
                {
                    return vbnet::SonarAnalyzer.RspecStrings.ResourceManager;
                }
                else
                {
                    throw new InvalidOperationException($"{nameof(AllCSharpRules_HaveCSharpTag)} or {nameof(AllVbNetRules_HaveVbNetTag)} should fail, fix them first.");
                }
            }
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class ConcurrentProcessingRetriever : SonarDiagnosticAnalyzer
        {
            private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetUtilityDescriptor("S9999", "Rule test");

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
            public bool? IsConcurrentProcessingDisabled { get; private set; }
            protected override void Initialize(SonarAnalysisContext context) =>
                IsConcurrentProcessingDisabled = ConcurrentProcessingDisabled;
        }
    }
}

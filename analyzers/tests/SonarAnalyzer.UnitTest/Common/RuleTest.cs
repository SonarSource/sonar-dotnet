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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
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
                var attributes = analyzer.GetCustomAttributes<RuleAttribute>();
                attributes.Should().BeEmpty("At least one RuleAttribute is added to the abstract DiagnosticAnalyzer '{0}'", analyzer.Name);
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
                var titles = RuleDetailBuilder.GetCodeFixTitles(codeFixProvider);
                titles.Should().NotBeEmpty("CodeFixProvider '{0}' has no title field.", codeFixProvider.Name);
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
                    // Security hotspots are enabled by default, but they will report issues only
                    // when their ID is contained in SonarLint.xml
                    diagnostic.IsEnabledByDefault.Should().BeTrue($"{diagnostic.Id} should be enabled by default");
                }
                else if (parameterized.Contains(diagnostic))
                {
                    // Even if a a parametrized rule is in Sonar way profile, it is still disabled by default.
                    // See https://github.com/SonarSource/sonar-dotnet/issues/1274
                    diagnostic.IsEnabledByDefault.Should().BeFalse($"{diagnostic.Id} has parameters and should be disabled by default");
                }
                else if (diagnostic.CustomTags.Contains(DiagnosticDescriptorBuilder.SonarWayTag))
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

        private static void OnlySecurityHotspots_AreNotConfigurable(AnalyzerLanguage language)
        {
            foreach (var diagnostic in SupportedDiagnostics(language))
            {
                if (IsSecurityHotspot(diagnostic))
                {
                    diagnostic.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable).Should().BeTrue(diagnostic.Id + " is a Security Hotspot and should not be configurable");
                }
                else
                {
                    diagnostic.CustomTags.Contains(WellKnownDiagnosticTags.NotConfigurable).Should().BeFalse(diagnostic.Id + " is not a Security Hotspot and should be configurable");
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
    }
}

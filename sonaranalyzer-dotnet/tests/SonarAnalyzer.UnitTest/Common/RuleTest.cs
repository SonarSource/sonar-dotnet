/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class RuleTest
    {
        private static IEnumerable<Type> GetCodeFixProviderTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(SonarCodeFixProvider)));
        }

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
                .Where(t =>
                    t.IsSubclassOf(typeof(DiagnosticAnalyzer)) &&
                    t.IsAbstract)
                .ToList();

            foreach (var analyzer in analyzers)
            {
                var attributes = analyzer.GetCustomAttributes<RuleAttribute>();
                attributes.Should().BeEmpty("At least one RuleAttribute is added to the abstract DiagnosticAnalyzer '{0}'", analyzer.Name);
            }
        }

        [TestMethod]
        public void CodeFixProviders_Named_Properly()
        {
            var codeFixProviders = GetCodeFixProviderTypes(RuleFinder.PackagedRuleAssemblies);

            foreach (var codeFixProvider in codeFixProviders)
            {
                var analyzerName = codeFixProvider.FullName.Replace(RuleDetailBuilder.CodeFixProviderSuffix, "");

                codeFixProvider.Assembly.GetType(analyzerName)
                    .Should().NotBeNull("CodeFixProvider '{0}' has no matching DiagnosticAnalyzer.", codeFixProvider.Name);
            }
        }

        [TestMethod]
        public void CodeFixProviders_Have_Title()
        {
            var codeFixProviders = GetCodeFixProviderTypes(RuleFinder.PackagedRuleAssemblies);

            foreach (var codeFixProvider in codeFixProviders)
            {
                var titles = RuleDetailBuilder.GetCodeFixTitles(codeFixProvider);

                titles.Should().NotBeEmpty("CodeFixProvider '{0}' has no title field.", codeFixProvider.Name);
            }
        }

        [TestMethod]
        public void Rules_DoNot_Throw()
        {
            var analyzers = new RuleFinder().GetAnalyzerTypes(AnalyzerLanguage.CSharp)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .ToList();

            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\InvalidSyntax.cs", analyzers, CompilationErrorBehavior.Ignore);
            Verifier.VerifyNoExceptionThrown(@"TestCasesForRuleFailure\SpecialCases.cs", analyzers, CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        public void SonarDiagnosticAnalyzer_IsUsedInAllRules()
        {
            var analyzers = RuleFinder.PackagedRuleAssemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DiagnosticAnalyzer)) && t != typeof(SonarDiagnosticAnalyzer))
                .ToList();

            foreach (var analyzer in analyzers)
            {
                analyzer.Should().BeAssignableTo<SonarDiagnosticAnalyzer>(
                    $"{analyzer.Name} is not a subclass of SonarDiagnosticAnalyzer");
            }
        }

        [TestMethod]
        public void AllParameterizedRules_AreDisabledByDefault()
        {
            new RuleFinder().AllAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .ToList()
                .ForEach(diagnostic => diagnostic.IsEnabledByDefault.Should().BeFalse());
        }

        [TestMethod]
        public void AllRulesEnabledByDefault_ContainSonarWayCustomTag()
        {
            var descriptors = new RuleFinder().AllAnalyzerTypes
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                // Security hotspots are enabled by default, but they will report issues only
                // when their ID is contained in SonarLint.xml
                .Where(descriptor => !IsSecurityHotspot(descriptor))
                .ToList();

            foreach (var descriptor in descriptors)
            {
                if (descriptor.IsEnabledByDefault)
                {
                    descriptor.CustomTags.Should().Contain(
                        DiagnosticDescriptorBuilder.SonarWayTag,
                        $"{descriptor.Id} should be in SonarWay");
                }
            }
        }

        [TestMethod]
        public void AllCSharpRules_HaveCSharpTag()
        {
            new RuleFinder()
                .GetAnalyzerTypes(AnalyzerLanguage.CSharp)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .ToList()
                .ForEach(diagnostic => diagnostic.CustomTags.Should().Contain(LanguageNames.CSharp));
        }

        [TestMethod]
        public void AllVbNetRules_HaveVbNetTag()
        {
            new RuleFinder()
                .GetAnalyzerTypes(AnalyzerLanguage.VisualBasic)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .ToList()
                .ForEach(diagnostic => diagnostic.CustomTags.Should().Contain(LanguageNames.VisualBasic));
        }

        [TestMethod]
        public void AllRules_SonarWayTagPresenceMatchesIsEnabledByDefault()
        {
            var allAnalyzers = new RuleFinder().AllAnalyzerTypes
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .ToList();

            var parameterizedAnalyzers = new RuleFinder().AllAnalyzerTypes
                .Where(RuleFinder.IsParameterized)
                .Select(type => (DiagnosticAnalyzer)Activator.CreateInstance(type))
                .SelectMany(analyzer => analyzer.SupportedDiagnostics)
                .ToHashSet();

            foreach (var analyzer in allAnalyzers)
            {
                var isInSonarWay = analyzer.CustomTags.Contains(DiagnosticDescriptorBuilder.SonarWayTag);

                if (parameterizedAnalyzers.Contains(analyzer))
                {
                    analyzer.IsEnabledByDefault.Should().BeFalse($"{analyzer.Id} has parameters and should be disabled by default");
                }
                else if (IsSecurityHotspot(analyzer))
                {
                    // Security hotspots are enabled by default, but they will report issues only
                    // when their ID is contained in SonarLint.xml
                    analyzer.IsEnabledByDefault.Should().BeTrue($"{analyzer.Id} should be enabled by default");
                }
                else if (isInSonarWay)
                {
                    analyzer.IsEnabledByDefault.Should().BeTrue($"{analyzer.Id} is in SonarWay");
                }
                else
                {
                    analyzer.IsEnabledByDefault.Should().BeFalse($"{analyzer.Id} is not in SonarWay");
                }
            }
        }

        [TestMethod]
        public void SecurityHotspots_Rules_Not_Configurable()
        {
            var hotspotDiagnosticDescriptors = new RuleFinder()
                .GetAnalyzerTypes(AnalyzerLanguage.CSharp)
                .Select(t => (SonarDiagnosticAnalyzer)Activator.CreateInstance(t))
                .SelectMany(diagnosticAnalyzer => diagnosticAnalyzer.SupportedDiagnostics)
                .Where(IsSecurityHotspot)
                .ToList();

            foreach (var descriptor in hotspotDiagnosticDescriptors)
            {
                descriptor.CustomTags.Should().Contain(
                    WellKnownDiagnosticTags.NotConfigurable,
                    because: $"{descriptor.Id} is hotspot and should not be configurable");
            }
        }

        private static bool IsSecurityHotspot(DiagnosticDescriptor diagnostic) =>
            diagnostic.Category.IndexOf("hotspot", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

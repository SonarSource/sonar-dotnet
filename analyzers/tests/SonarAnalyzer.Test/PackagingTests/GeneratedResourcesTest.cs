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

using System.IO;
using System.Text.RegularExpressions;
using SonarAnalyzer.Test.TestFramework;

namespace SonarAnalyzer.Test.PackagingTests
{
    [TestClass]
    public class GeneratedResourcesTest
    {
        [TestMethod]
        public void AnalyzersHaveCorrespondingResource_CSharp()
        {
            var rulesFromResources = SortedRulesFromResources(Path.Combine(Paths.Rspec, "cs")); // Unique list
            var rulesFromTypes = SortedRulesFromTypes(AnalyzerLanguage.CSharp).Distinct().ToArray(); // Same ruleId can be in multiple classes (see InvalidCastToInterface)
            rulesFromResources.Should().Equal(rulesFromTypes);
        }

        [TestMethod]
        public void AnalyzersHaveCorrespondingResource_VisualBasic()
        {
            var rulesFromResources = SortedRulesFromResources(Path.Combine(Paths.Rspec, "vbnet")); // Unique list
            var rulesFromTypes = SortedRulesFromTypes(AnalyzerLanguage.VisualBasic).Distinct().ToArray(); // Same ruleId can be in multiple classes (see InvalidCastToInterface)
            rulesFromResources.Should().Equal(rulesFromTypes);
        }

        [TestMethod]
        public void ThereShouldBeRuleDetailsForAllCSharpRuleClasses()
        {
            var ruleDetailsKeys = SortedRulesFromDetailBuilder(AnalyzerLanguage.CSharp);
            var rulesFromTypes = SortedRulesFromTypes(AnalyzerLanguage.CSharp);
            ruleDetailsKeys.Should().Equal(rulesFromTypes);
        }

        [TestMethod]
        public void ThereShouldBeRuleDetailsForAllVbNetRuleClasses()
        {
            var ruleDetailsKeys = SortedRulesFromDetailBuilder(AnalyzerLanguage.VisualBasic);
            var rulesFromTypes = SortedRulesFromTypes(AnalyzerLanguage.VisualBasic);
            ruleDetailsKeys.Should().Equal(rulesFromTypes);
        }

        private static string[] SortedRulesFromTypes(AnalyzerLanguage language) =>
            RuleFinder.CreateAnalyzers(language, false)
                .SelectMany(x => x.SupportedDiagnostics.Select(descriptor => descriptor.Id))
                .Distinct() // One class can have the same ruleId multiple times, see S3240, same ruleId can be in multiple classes (see InvalidCastToInterface)
                .OrderBy(x => x)
                .ToArray();

        private static string[] SortedRulesFromResources(string relativePath)
        {
            var resourcesRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), relativePath));
            return Directory.GetFiles(resourcesRoot)
                .Where(IsHtmlFile)
                .Select(Path.GetFileNameWithoutExtension)
                .Select(RuleFromFileName)
                .OrderBy(name => name)
                .ToArray();
        }

        private static string[] SortedRulesFromDetailBuilder(AnalyzerLanguage language) =>
            RuleFinder.CreateAnalyzers(language, false).SelectMany(x => x.SupportedDiagnostics).Select(x => x.Id).Distinct().OrderBy(x => x).ToArray();

        private static string RuleFromFileName(string fileName)
        {
            var match = Regex.Match(fileName, @"(S\d+)");    // S1234
            return match.Success ? match.Groups[1].Value : null;
        }

        private static bool IsHtmlFile(string name) =>
            Path.GetExtension(name).Equals(".html", StringComparison.OrdinalIgnoreCase);
    }
}

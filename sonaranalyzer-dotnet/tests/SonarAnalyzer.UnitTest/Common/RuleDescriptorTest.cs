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

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Utilities;
using System.Linq;
using System.Reflection;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class RuleDescriptorTest
    {
        [TestMethod]
        public void CheckAllAnalyzersHaveRuleId()
        {
            CheckLanguageSpecificAnalyzersHaveRuleId(AnalyzerLanguage.CSharp);
            CheckLanguageSpecificAnalyzersHaveRuleId(AnalyzerLanguage.VisualBasic);
        }

        private static void CheckLanguageSpecificAnalyzersHaveRuleId(AnalyzerLanguage language)
        {
            new RuleFinder()
                .GetAnalyzerTypes(language)
                .Any(at => !at.GetCustomAttributes<RuleAttribute>().Any())
                .Should()
                .BeFalse();
        }

        [TestMethod]
        public void CheckParameterlessRuleDescriptorsHaveRuleId()
        {
            CheckLanguageSpecificParameterlessRuleDescriptorsHaveRuleId(AnalyzerLanguage.CSharp);
            CheckLanguageSpecificParameterlessRuleDescriptorsHaveRuleId(AnalyzerLanguage.VisualBasic);
        }

        private static void CheckLanguageSpecificParameterlessRuleDescriptorsHaveRuleId(AnalyzerLanguage language)
        {
            new RuleFinder().GetParameterlessAnalyzerTypes(language)
                .Any(at => !at.GetCustomAttributes<RuleAttribute>().Any())
                .Should()
                .BeFalse();
        }

        [TestMethod]
        public void RuleDescriptors_NotEmpty()
        {
            CheckRuleDescriptorsNotEmpty(AnalyzerLanguage.CSharp);
            CheckRuleDescriptorsNotEmpty(AnalyzerLanguage.VisualBasic);
        }

        private static void CheckRuleDescriptorsNotEmpty(AnalyzerLanguage language)
        {
            var ruleDetails = RuleDetailBuilder.GetAllRuleDetails(language).ToList();
            foreach (var ruleDetail in ruleDetails)
            {
                ruleDetail.Should().NotBeNull();
                ruleDetail.Type.Should().NotBeNull();
                // SECURITY_HOTSPOTS are converted to VULNERABILITY to ensure compatiblity with old versions of SonarQube
                ruleDetail.Type.Should().BeOneOf("CODE_SMELL", "BUG", "VULNERABILITY");
                ruleDetail.Description.Should().NotBeNull();
                ruleDetail.Key.Should().NotBeNull();
                ruleDetail.Title.Should().NotBeNull();
            }

            ruleDetails.Should().OnlyHaveUniqueItems();
        }
    }
}

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
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.RuleDescriptors;
using SonarAnalyzer.Utilities;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class RuleDetailTest
    {
        [TestMethod]
        public void RuleDetails_NotEmpty()
        {
            CheckRuleDetailsNotEmpty(AnalyzerLanguage.CSharp);
            CheckRuleDetailsNotEmpty(AnalyzerLanguage.VisualBasic);
        }

        [TestMethod]
        public void GetAllRuleDetails_UnexpectedLanguage_Throws() =>
            ((Action)(() => RuleDetailBuilder.GetAllRuleDetails(AnalyzerLanguage.Both))).Should().Throw<NotSupportedException>();

        [TestMethod]
        public void RuleParameter_Constructor_CopiesValues()
        {
            var att = new RuleParameterAttribute("key", PropertyType.Password, "Description", "Secret");
            var sut = new RuleParameter(att);
            sut.Key.Should().Be("key");
            sut.Description.Should().Be("Description");
            sut.Type.Should().Be("PASSWORD");
            sut.DefaultValue.Should().Be("Secret");
        }

        private static void CheckRuleDetailsNotEmpty(AnalyzerLanguage language)
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

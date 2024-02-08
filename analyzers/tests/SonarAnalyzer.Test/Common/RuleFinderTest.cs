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

using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Test.TestFramework;

namespace SonarAnalyzer.Test.Common
{
    [TestClass]
    public class RuleFinderTest
    {
        [TestMethod]
        public void GetAnalyzerTypes()
        {
            var analyzers = RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.CSharp);
            analyzers.Should().Contain(typeof(EmptyStatement));
        }

        [TestMethod]
        public void GetAllAnalyzerTypes()
        {
            var countParameterless = RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.CSharp).Count(x => !RuleFinder.IsParameterized(x));
            RuleFinder.RuleAnalyzerTypes.Should().HaveCountGreaterThan(countParameterless);

            countParameterless = RuleFinder.GetAnalyzerTypes(AnalyzerLanguage.VisualBasic).Count(x => !RuleFinder.IsParameterized(x));
            RuleFinder.RuleAnalyzerTypes.Should().HaveCountGreaterThan(countParameterless);
        }
    }
}

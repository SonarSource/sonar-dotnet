/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class NumberPatternShouldBeRegularTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void NumberPatternShouldBeRegular_BeforeCSharp7()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\NumberPatternShouldBeRegular.cs",
                new NumberPatternShouldBeRegular(),
                ParseOptionsHelper.BeforeCSharp7,
                CompilationErrorBehavior.Ignore);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void NumberPatternShouldBeRegular_FromCSharp7()
        {
            Verifier.VerifyAnalyzer(@"TestCases\NumberPatternShouldBeRegular.cs",
                new NumberPatternShouldBeRegular(),
                ParseOptionsHelper.FromCSharp7);
        }

        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow("1_1_1", "Group size of 1")]
        [DataRow("123_12", "First group is bigger then the second")]
        [DataRow("1234_123_123", "First group is bigger then the others")]
        [DataRow("123_123_12_123", "Another group has a different size")]
        [DataRow("1_123.123_1234", "Last decimal group is bigger")]
        [DataRow(".123_123_1234", "No group before the dot")]
        public void HasIrregularPattern(string numericToken, string message)
        {
            Assert.IsTrue(NumberPatternShouldBeRegular.HasIrregularPattern(numericToken), message);
        }

        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow(".123_123_123_1", "No group before the dot")]
        [DataRow("123", "No group character")]
        [DataRow("1_123_123UL", "With UL suffix")]
        [DataRow("1_123_123L", "With L suffix")]
        [DataRow("1.1.1", "Two dots")]
        [DataRow("0b1010_1010", "With binary prefix")]
        [DataRow("0xFF_FF_12", "With hexadecimal prefix")]
        [DataRow("1_123_123", "first group smaller")]
        [DataRow("123_123_123", "All blocks equal size")]
        [DataRow("1_123.123_123", "All decimal groups have the same size")]
        [DataRow("1_123.123_123_12", "Last decimal group is smaller")]
        [DataRow("1_123.1234567", "Only one group of decimals")]
        public void HasRegularPattern(string numericToken, string message)
        {
            Assert.IsFalse(NumberPatternShouldBeRegular.HasIrregularPattern(numericToken), message);
        }
    }
}

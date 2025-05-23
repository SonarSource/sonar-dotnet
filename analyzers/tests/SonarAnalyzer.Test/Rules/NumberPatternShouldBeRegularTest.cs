﻿/*
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class NumberPatternShouldBeRegularTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<NumberPatternShouldBeRegular>();

        [TestMethod]
        public void NumberPatternShouldBeRegular_BeforeCSharp7() =>
            builder.AddPaths("NumberPatternShouldBeRegular.cs").WithOptions(ParseOptionsHelper.BeforeCSharp7).WithErrorBehavior(CompilationErrorBehavior.Ignore).VerifyNoIssues();

        [TestMethod]
        public void NumberPatternShouldBeRegular_FromCSharp7() =>
            builder.AddPaths("NumberPatternShouldBeRegular.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

#if NET

        [TestMethod]
        public void NumberPatternShouldBeRegular_FromCSharp9() =>
            builder.AddPaths("NumberPatternShouldBeRegular.CSharp9.cs").WithTopLevelStatements().Verify();

#endif

        [DataTestMethod]
        [DataRow("1_1_1", "Group size of 1")]
        [DataRow("123_12", "First group is bigger then the second")]
        [DataRow("1234_123_123", "First group is bigger then the others")]
        [DataRow("123_123_12_123", "Another group has a different size")]
        [DataRow("1_123.123_1234", "Last decimal group is bigger")]
        [DataRow(".123_123_1234", "No group before the dot")]
        [DataRow("0xFF_FF_FFF_FF", "3rd group is bigger")]
        [DataRow("0xFF_FF_FFF", "Last group bigger than 2")]
        [DataRow("0xFFFF_FFFF_FFFFF", "Last group bigger than 4")]
        [DataRow("0xFFFF_FFFF_FFFFF", "Last group bigger than 4")]
        [DataRow("1.234_5678E2", "Exponential format with bigger last group")]
        [DataRow("____", "Only underscores")]
        [DataRow("__.__", "Only underscores and a dot")]
        [DataRow("0xFF___FF___FF", "Multiple _'s as separator")]
        [DataRow("0xFF________FF___FF", "Multiple irregular _'s as separator")]
        public void HasIrregularPattern(string numericToken, string message) => Assert.IsTrue(NumberPatternShouldBeRegular.HasIrregularPattern(numericToken), message);

        [DataTestMethod]
        [DataRow(".123_123_123_1", "No group before the dot")]
        [DataRow("123", "No group character")]
        [DataRow("1_123_123LU", "With LU suffix")]
        [DataRow("2_123_123lu", "With lu suffix")]
        [DataRow("3_123_123lU", "With lU suffix")]
        [DataRow("1_123_123UL", "With UL suffix")]
        [DataRow("2_123_123ul", "With ul suffix")]
        [DataRow("1_123_123L", "With L suffix")]
        [DataRow("1.1.1", "Two dots")]
        [DataRow("0b1010_1010", "With binary prefix")]
        [DataRow("0xFF_FF_12", "With hexadecimal prefix")]
        [DataRow("0xFF_FF_E2", "With hexadecimal prefix with E but not exponential")]
        [DataRow("1_123_123", "first group smaller")]
        [DataRow("123_123_123", "All blocks equal size")]
        [DataRow("1_123.123_123", "All decimal groups have the same size")]
        [DataRow("1_123.123_123_12", "Last decimal group is smaller")]
        [DataRow("1_123.1234567", "Only one group of decimals")]
        [DataRow("1.234_567E2", "Scientific format with regular size")]
        [DataRow("1.234_5E2", "Scientific format with smaller last group")]
        [DataRow("134.45E-2f", "Scientific format f suffix")]
        [DataRow("134.45E12M", "Scientific format M suffix")]
        [DataRow("1.0", "Simple floating point")]
        [DataRow("3D", "Floating point with D suffix")]
        [DataRow("4d", "Floating point with d suffix")]
        [DataRow("5M", "Floating point with M suffix")]
        [DataRow("6m", "Floating point with m suffix")]
        [DataRow("3_000.5F", "Floating point with group size")]
        public void HasRegularPattern(string numericToken, string message) => Assert.IsFalse(NumberPatternShouldBeRegular.HasIrregularPattern(numericToken), message);
    }
}

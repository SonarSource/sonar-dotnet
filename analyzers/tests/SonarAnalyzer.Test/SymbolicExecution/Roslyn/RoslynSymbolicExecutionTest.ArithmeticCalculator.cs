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

using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.Test.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.Test.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [DataTestMethod]
    [DataRow("i + j", 47)]
    [DataRow("i + 1", 43)]
    [DataRow("i + j + 1", 48)]
    [DataRow("(i + 1) + j", 48)]
    [DataRow("(i + j) + 1", 48)]
    [DataRow("i + (1 + j)", 48)]
    [DataRow("1 + (i + j)", 48)]
    [DataRow("i - j", 37)]
    [DataRow("j - i", -37)]
    [DataRow("i - 1", 41)]
    [DataRow("i - j - 1", 36)]
    [DataRow("(i - 1) - j", 36)]
    [DataRow("(i - j) - 1", 36)]
    [DataRow("i - (j - 1)", 38)]
    [DataRow("i - (1 - j)", 46)]
    [DataRow("1 - (i - j)", -36)]
    [DataRow("i + j - 1", 46)]
    [DataRow("(i + j) - 1", 46)]
    [DataRow("(i + 1) - j", 38)]
    [DataRow("i + (j - 1)", 46)]
    [DataRow("i + (1 - j)", 38)]
    [DataRow("1 + (i - j)", 38)]
    [DataRow("i - j + 1", 38)]
    [DataRow("(i - j) + 1", 38)]
    [DataRow("(i - 1) + j", 46)]
    [DataRow("i - (j + 1)", 36)]
    [DataRow("1 - (i + j)", -46)]
    [DataRow("i += j", 47)]
    [DataRow("i -= j", 37)]
    [DataRow("i += (true ? 1 : 0)", 43)]
    [DataRow("i -= (true ? 1 : 0)", 41)]
    public void Calculate_PlusAndMinus(string expression, int expected)
    {
        var code = $"""
            var i = 42;
            var j = 5;
            var value =  {expression};
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow(5, 0, 0)]
    [DataRow(5, 1, 5)]
    [DataRow(5, -1, -5)]
    [DataRow(5, 4, 20)]
    [DataRow(5, -4, -20)]
    [DataRow(-5, 0, 0)]
    [DataRow(-5, 1, -5)]
    [DataRow(-5, -1, 5)]
    [DataRow(-5, 4, -20)]
    [DataRow(-5, -4, 20)]
    public void Calculate_Multiplication_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};

            var binary =  left * right;
            Tag("Binary", binary);

            var compound = left *= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  3 && j >=  5", 15, null)]
    [DataRow("i >=  3 && j >= -5", null, null)]
    [DataRow("i >=  3 && j <=  5", null, null)]
    [DataRow("i >=  3 && j <= -5", null, -15)]
    [DataRow("i >= -3 && j >=  5", null, null)]
    [DataRow("i >= -3 && j >= -5", null, null)]
    [DataRow("i >= -3 && j <=  5", null, null)]
    [DataRow("i >= -3 && j <= -5", null, null)]
    [DataRow("i <=  3 && j >=  5", null, null)]
    [DataRow("i <=  3 && j >= -5", null, null)]
    [DataRow("i <=  3 && j <=  5", null, null)]
    [DataRow("i <=  3 && j <= -5", null, null)]
    [DataRow("i <= -3 && j >=  5", null, -15)]
    [DataRow("i <= -3 && j >= -5", null, null)]
    [DataRow("i <= -3 && j <=  5", null, null)]
    [DataRow("i <= -3 && j <= -5", 15, null)]
    [DataRow("i ==  3 && j >=  5", 15, null)]
    [DataRow("i ==  3 && j >= -5", -15, null)]
    [DataRow("i ==  3 && j <=  5", null, 15)]
    [DataRow("i ==  3 && j <= -5", null, -15)]
    [DataRow("i == -3 && j >=  5", null, -15)]
    [DataRow("i == -3 && j >= -5", null, 15)]
    [DataRow("i == -3 && j <=  5", -15, null)]
    [DataRow("i == -3 && j <= -5", 15, null)]
    public void Calculate_Multiplication_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i * j;
                Tag("Binary", binary);

                var compound = i *= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(21, 4, 5)]
    [DataRow(21, -4, -5)]
    [DataRow(-21, 4, -5)]
    [DataRow(-21, -4, 5)]
    [DataRow(4, 5, 0)]
    [DataRow(-4, 5, 0)]
    [DataRow(4, -5, 0)]
    [DataRow(-4, -5, 0)]
    [DataRow(5, 1, 5)]
    [DataRow(5, -1, -5)]
    [DataRow(-5, 1, -5)]
    [DataRow(-5, -1, 5)]
    public void Calculate_Division_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var binary =  left / right;
            Tag("Binary", binary);

            var compound = left /= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  21 && j >=  5", 0, null)]
    [DataRow("i >=  21 && j >= -5", null, null)]
    [DataRow("i >=  21 && j ==  5", 4, null)]
    [DataRow("i >=  21 && j == -5", null, -4)]
    [DataRow("i >=  21 && j <=  5", null, null)]
    [DataRow("i >=  21 && j <= -5", null, 0)]
    [DataRow("i >= -21 && j >=  5", -4, null)]
    [DataRow("i >= -21 && j >= -5", null, null)]
    [DataRow("i >= -21 && j ==  5", -4, null)]
    [DataRow("i >= -21 && j == -5", null, 4)]
    [DataRow("i >= -21 && j <=  5", null, null)]
    [DataRow("i >= -21 && j <= -5", null, 4)]
    [DataRow("i ==  21 && j >=  5", 0, 4)]
    [DataRow("i ==  21 && j >= -5", -21, 21)]
    [DataRow("i ==  21 && j <=  5", -21, 21)]
    [DataRow("i ==  21 && j <= -5", -4, 0)]
    [DataRow("i == -21 && j >=  5", -4, 0)]
    [DataRow("i == -21 && j >= -5", -21, 21)]
    [DataRow("i == -21 && j <=  5", -21, 21)]
    [DataRow("i == -21 && j <= -5", 0, 4)]
    [DataRow("i <=  21 && j >=  5", null, 4)]
    [DataRow("i <=  21 && j >= -5", null, null)]
    [DataRow("i <=  21 && j ==  5", null, 4)]
    [DataRow("i <=  21 && j == -5", -4, null)]
    [DataRow("i <=  21 && j <=  5", null, null)]
    [DataRow("i <=  21 && j <= -5", -4, null)]
    [DataRow("i <= -21 && j >=  5", null, 0)]
    [DataRow("i <= -21 && j >= -5", null, null)]
    [DataRow("i <= -21 && j ==  5", null, -4)]
    [DataRow("i <= -21 && j == -5", 4, null)]
    [DataRow("i <= -21 && j <=  5", null, null)]
    [DataRow("i <= -21 && j <= -5", 0, null)]
    [DataRow("i >= -21 && i <= 15 && j >= -5", -21, 21)]
    [DataRow("i >= -15 && i <= 21 && j >= -5", -21, 21)]
    [DataRow("i >=  21 && j >= 0", 0, null)]
    [DataRow("i >=  21 && j == 0", null, null)]
    [DataRow("i >=  21 && j <= 0", null, 0)]
    [DataRow("i >= -21 && j >= 0", -21, null)]
    [DataRow("i >= -21 && j == 0", null, null)]
    [DataRow("i >= -21 && j <= 0", null, 21)]
    [DataRow("i <=  21 && j >= 0", null, 21)]
    [DataRow("i <=  21 && j == 0", null, null)]
    [DataRow("i <=  21 && j <= 0", -21, null)]
    [DataRow("i <= -21 && j >= 0", null, 0)]
    [DataRow("i <= -21 && j == 0", null, null)]
    [DataRow("i <= -21 && j <= 0", 0, null)]
    public void Calculate_Division_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i / j;
                Tag("Binary", binary);

                var compound = i /= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(21, 4, 1)]
    [DataRow(21, -4, 1)]
    [DataRow(-21, 4, -1)]
    [DataRow(-21, -4, -1)]
    [DataRow(4, 5, 4)]
    [DataRow(-4, 5, -4)]
    [DataRow(4, -5, 4)]
    [DataRow(-4, -5, -4)]
    [DataRow(5, 5, 0)]
    [DataRow(5, -5, 0)]
    [DataRow(-5, 5, 0)]
    [DataRow(-5, -5, 0)]
    [DataRow(5, 1, 0)]
    [DataRow(5, -1, 0)]
    [DataRow(-5, 1, 0)]
    [DataRow(-5, -1, 0)]
    public void Calculate_Remainder_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var binary =  left % right;
            Tag("Binary", binary);

            var compound = left %= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  21 && j >=  5", 0, null)]
    [DataRow("i >=  21 && j >= -5", 0, null)]
    [DataRow("i >=  21 && j ==  5", 0, 4)]
    [DataRow("i >=  21 && j == -5", 0, 4)]
    [DataRow("i >=  21 && j <=  5", 0, null)]
    [DataRow("i >=  21 && j <= -5", 0, null)]
    [DataRow("i >= -21 && j >=  5", -21, null)]
    [DataRow("i >= -21 && j >= -5", -21, null)]
    [DataRow("i >= -21 && j ==  5", -4, 4)]
    [DataRow("i >= -21 && j == -5", -4, 4)]
    [DataRow("i >= -21 && j <=  5", -21, null)]
    [DataRow("i >= -21 && j <= -5", -21, null)]
    [DataRow("i ==  21 && j >=  5", 0, 21)]
    [DataRow("i ==  21 && j >= -5", 0, 21)]
    [DataRow("i ==  21 && j <=  5", 0, 21)]
    [DataRow("i ==  21 && j <= -5", 0, 21)]
    [DataRow("i == -21 && j >=  5", -21, 0)]
    [DataRow("i == -21 && j >= -5", -21, 0)]
    [DataRow("i == -21 && j <=  5", -21, 0)]
    [DataRow("i == -21 && j <= -5", -21, 0)]
    [DataRow("i <=  21 && j >=  5", null, 21)]
    [DataRow("i <=  21 && j >= -5", null, 21)]
    [DataRow("i <=  21 && j ==  5", -4, 4)]
    [DataRow("i <=  21 && j == -5", -4, 4)]
    [DataRow("i <=  21 && j <=  5", null, 21)]
    [DataRow("i <=  21 && j <= -5", null, 21)]
    [DataRow("i <= -21 && j >=  5", null, 0)]
    [DataRow("i <= -21 && j >= -5", null, 0)]
    [DataRow("i <= -21 && j ==  5", -4, 0)]
    [DataRow("i <= -21 && j == -5", -4, 0)]
    [DataRow("i <= -21 && j <=  5", null, 0)]
    [DataRow("i <= -21 && j <= -5", null, 0)]
    [DataRow("i >=  21 && j >= 0", 0, null)]
    [DataRow("i >=  21 && j == 0", null, null)]
    [DataRow("i >=  21 && j <= 0", 0, null)]
    [DataRow("i >= -21 && j >= 0", -21, null)]
    [DataRow("i >= -21 && j == 0", null, null)]
    [DataRow("i >= -21 && j <= 0", -21, null)]
    [DataRow("i <=  21 && j >= 0", null, 21)]
    [DataRow("i <=  21 && j == 0", null, null)]
    [DataRow("i <=  21 && j <= 0", null, 21)]
    [DataRow("i <= -21 && j >= 0", null, 0)]
    [DataRow("i <= -21 && j == 0", null, null)]
    [DataRow("i <= -21 && j <= 0", null, 0)]
    [DataRow("i >=  21 && j == 1", 0, 0)]
    [DataRow("i >= -21 && j == 1", 0, 0)]
    [DataRow("i <=  21 && j == 1", 0, 0)]
    [DataRow("i <= -21 && j == 1", 0, 0)]
    [DataRow("i >=  21 && j >=   5 && j <= 10", 0, 9)]
    [DataRow("i >=  21 && j >=  -5 && j <= 10", 0, 9)]
    [DataRow("i >=  21 && j >= -10 && j <=  5", 0, 9)]
    [DataRow("i >=  21 && j >= -10 && j <= -5", 0, 9)]
    [DataRow("i >= -21 && j >=   5 && j <= 10", -9, 9)]
    [DataRow("i >= -21 && j >=  -5 && j <= 10", -9, 9)]
    [DataRow("i >= -21 && j >= -10 && j <=  5", -9, 9)]
    [DataRow("i >= -21 && j >= -10 && j <= -5", -9, 9)]
    [DataRow("i <=  21 && j >=   5 && j <= 10", -9, 9)]
    [DataRow("i <=  21 && j >=  -5 && j <= 10", -9, 9)]
    [DataRow("i <=  21 && j >= -10 && j <=  5", -9, 9)]
    [DataRow("i <=  21 && j >= -10 && j <= -5", -9, 9)]
    [DataRow("i <= -21 && j >=   5 && j <= 10", -9, 0)]
    [DataRow("i <= -21 && j >=  -5 && j <= 10", -9, 0)]
    [DataRow("i <= -21 && j >= -10 && j <=  5", -9, 0)]
    [DataRow("i <= -21 && j >= -10 && j <= -5", -9, 0)]
    [DataRow("i >=   5 && i <=  10 && j >=  21", 5, 10)]
    [DataRow("i >=   5 && i <=  10 && j <= -21", 5, 10)]
    [DataRow("i >=  -5 && i <=  10 && j >=  21", -5, 10)]
    [DataRow("i >=  -5 && i <=  10 && j <= -21", -5, 10)]
    [DataRow("i >= -10 && i <=   5 && j >=  21", -10, 5)]
    [DataRow("i >= -10 && i <=   5 && j <= -21", -10, 5)]
    [DataRow("i >= -10 && i <=  -5 && j >=  21", -10, -5)]
    [DataRow("i >= -10 && i <=  -5 && j <= -21", -10, -5)]
    [DataRow("i >=   4 && i <=   7 && j >=   5 && j <= 10", 0, 7)]
    [DataRow("i <    0 && j == 2", -1, 0)]
    [DataRow("i <    0 && j == -2", -1, 0)]
    [DataRow("i <    0 && j > 0", null, 0)]
    [DataRow("i >    0 && j == 2", 0, 1)]
    [DataRow("i ==   5 && j == 0", null, null)]
    public void Calculate_Remainder_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i % j;
                Tag("Binary", binary);

                var compound = i %= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0101)]
    [DataRow(0b0101, 0b0001, 0b0001)]
    [DataRow(0b1010, 0b0110, 0b0010)]
    [DataRow(0b1010, 0b0000, 0b0000)]
    [DataRow(0b1111, 0b1111, 0b1111)]
    [DataRow(5, -5, 1)]
    [DataRow(5, -4, 4)]
    [DataRow(-5, -5, -5)]
    [DataRow(-5, -4, -8)]
    public void Calculate_BitAnd_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var binary =  left & right;
            Tag("Binary", binary);

            var compound = left &= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  3 && j >=  5", 0, null)]
    [DataRow("i >=  3 && j >= -5", 0, null)]
    [DataRow("i >=  3 && j <=  5", 0, null)]
    [DataRow("i >=  3 && j <= -5", 0, null)]
    [DataRow("i >= -3 && j >=  5", 0, null)]
    [DataRow("i >= -3 && j >= -5", -8, null)]
    [DataRow("i >= -3 && j <=  5", null, null)]
    [DataRow("i >= -3 && j <= -5", null, null)]
    [DataRow("i ==  3 && j >=  5", 0, 3)]
    [DataRow("i ==  3 && j >= -5", 0, 3)]
    [DataRow("i ==  3 && j <=  5", 0, 3)]
    [DataRow("i ==  3 && j <= -5", 0, 3)]
    [DataRow("i == -3 && j >=  5", 0, null)]
    [DataRow("i == -3 && j >= -5", -8, null)]
    [DataRow("i == -3 && j <=  5", null, 5)]
    [DataRow("i == -3 && j <= -5", null, -5)]
    [DataRow("i <=  3 && j >=  5", 0, null)]
    [DataRow("i <=  3 && j >= -5", null, null)]
    [DataRow("i <=  3 && j <=  5", null, 5)]
    [DataRow("i <=  3 && j <= -5", null, 3)]
    [DataRow("i <= -3 && j >=  5", 0, null)]
    [DataRow("i <= -3 && j >= -5", null, null)]
    [DataRow("i <= -3 && j <=  5", null, 5)]
    [DataRow("i <= -3 && j <= -5", null, -5)]
    public void Calculate_BitAnd_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i & j;
                Tag("Binary", binary);

                var compound = i &= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;
        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0101)]
    [DataRow(0b0101, 0b0001, 0b0101)]
    [DataRow(0b1010, 0b0110, 0b1110)]
    [DataRow(0b1010, 0b0000, 0b1010)]
    [DataRow(0b1111, 0b1111, 0b1111)]
    [DataRow(5, -5, -1)]
    [DataRow(5, -4, -3)]
    [DataRow(-5, -5, -5)]
    [DataRow(-5, -4, -1)]
    public void Calculate_BitOr_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var binary =  left | right;
            Tag("Binary", binary);

            var compound = left |= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  4 && j >=  6", 6, null)]
    [DataRow("i >=  4 && j >= -6", -6, null)]
    [DataRow("i >=  4 && j <=  6", null, null)]
    [DataRow("i >=  4 && j <= -6", null, -1)]
    [DataRow("i >= -4 && j >=  6", -4, null)]
    [DataRow("i >= -4 && j >= -6", -6, null)]
    [DataRow("i >= -4 && j <=  6", null, null)]
    [DataRow("i >= -4 && j <= -6", null, -1)]
    [DataRow("i ==  4 && j >=  6", 6, null)]
    [DataRow("i ==  4 && j >= -6", -6, null)] // (i | j) >= -4, we only get a lower bound of min
    [DataRow("i ==  4 && j <=  6", null, 7)]
    [DataRow("i ==  4 && j <= -6", null, -1)] // (i | j) <= -2, we only get an upper bound of max
    [DataRow("i == -4 && j >=  6", -4, -1)]
    [DataRow("i == -4 && j >= -6", -4, -1)]
    [DataRow("i == -4 && j <=  6", -4, -1)]
    [DataRow("i == -4 && j <= -6", -4, -1)]
    [DataRow("i <=  4 && j >=  6", null, null)]
    [DataRow("i <=  4 && j >= -6", null, null)]
    [DataRow("i <=  4 && j <=  6", null, 7)]
    [DataRow("i <=  4 && j <= -6", null, -1)]
    [DataRow("i <= -4 && j >=  6", null, -1)]
    [DataRow("i <= -4 && j >= -6", null, -1)]
    [DataRow("i <= -4 && j <=  6", null, -1)]
    [DataRow("i <= -4 && j <= -6", null, -1)]
    public void Calculate_BitOr_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i | j;
                Tag("Binary", binary);

                var compound = i |= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;

        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        if (expectedMin is not null || expectedMax is not null)
        {
            validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
            validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
            validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        }
        else
        {
            validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
            validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
            validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        }
    }

    [DataTestMethod]
    [DataRow(0b0000, 0b0000, 0b0000)]
    [DataRow(0b0101, 0b0101, 0b0000)]
    [DataRow(0b0101, 0b0001, 0b0100)]
    [DataRow(0b1010, 0b0110, 0b1100)]
    [DataRow(0b1010, 0b0000, 0b1010)]
    [DataRow(0b1111, 0b1111, 0b0000)]
    [DataRow(5, -5, -2)]
    [DataRow(5, -4, -7)]
    [DataRow(-5, -5, 0)]
    [DataRow(-5, -4, 7)]
    public void Calculate_BitXor_SingleValue(int left, int right, int expected)
    {
        var code = $"""
            var left = {left};
            var right = {right};
            var binary =  left ^ right;
            Tag("Binary", binary);

            var compound = left ^= right;
            Tag("Compound", compound);
            Tag("Left", left);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [DataTestMethod]
    [DataRow("i >=  4 && j >=  6", 0, null)]
    [DataRow("i >=  4 && j >= -6", null, null)]
    [DataRow("i >=  4 && j <=  6", null, null)]
    [DataRow("i >=  4 && j <= -6", null, -1)]
    [DataRow("i >= -4 && j >=  6", null, null)]
    [DataRow("i >= -4 && j >= -6", null, null)]
    [DataRow("i >= -4 && j <=  6", null, null)]
    [DataRow("i >= -4 && j <= -6", null, null)]  // exact range: null, -1
    [DataRow("i ==  4 && j >=  6", 2, null)]
    [DataRow("i ==  4 && j >= -6", -8, null)]
    [DataRow("i ==  4 && j <=  6", null, 7)]
    [DataRow("i ==  4 && j <= -6", null, -1)]    // exact range: null, -2
    [DataRow("i == -4 && j >=  6", null, -1)]    // exact range: null, -5
    [DataRow("i == -4 && j >= -6", null, 7)]
    [DataRow("i == -4 && j <=  6", -8, null)]
    [DataRow("i == -4 && j <= -6", 2, null)]     // exact range: 4, null
    [DataRow("i <=  4 && j >=  6", null, null)]
    [DataRow("i <=  4 && j >= -6", null, null)]
    [DataRow("i <=  4 && j <=  6", null, null)]
    [DataRow("i <=  4 && j <= -6", null, null)]
    [DataRow("i <= -4 && j >=  6", null, -1)]
    [DataRow("i <= -4 && j >= -6", null, null)]
    [DataRow("i <= -4 && j <=  6", null, null)]
    [DataRow("i <= -4 && j <= -6", 0, null)]
    [DataRow("i >=  4 && j >=  6 && j <=  8", 0, null)]
    [DataRow("i >=  4 && j >=  1 && j <=  3", 1, null)]           // exact range: 4, null
    [DataRow("i >=  4 && i <=  6 && j >=  6", 0, null)]
    [DataRow("i >=  4 && i <=  6 && j >=  6 && j <= 8", 0, 15)]   // exact range: 0, 14
    [DataRow("i >=  4 && i <=  5 && j >=  6 && j <= 8", 1, 15)]   // exact range: 2, 13
    [DataRow("i >= -3 && i <= -1 && j >= -4 && j <=-2", 0, 7)]    // exact range: 0, 3
    [DataRow("i >= -4 && i <= -3 && j >= -2 && j <=-1", 1, 7)]    // exact range: 2, 3
    [DataRow("i >= -4 && i <=  6 && j >= -3 && j <= 8", -16, 15)] // exact range: -12, 14
    public void Calculate_BitXor_Range(string expression, int? expectedMin, int? expectedMax)
    {
        var code = $$"""
            if ({{expression}})
            {
                var binary =  i ^ j;
                Tag("Binary", binary);

                var compound = i ^= j;
                Tag("Compound", compound);
                Tag("I", i);
            }
            """;

        var validator = SETestContext.CreateCS(code, "int i, int j").Validator;
        if (expectedMin is not null || expectedMax is not null)
        {
            validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
            validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
            validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expectedMin, expectedMax));
        }
        else
        {
            validator.TagValue("Binary").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
            validator.TagValue("Compound").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
            validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        }
    }

    [DataTestMethod]
    [DataRow(5, 5, 10, null)]
    [DataRow(5, -5, null, null)]
    [DataRow(-5, 5, null, null)]
    [DataRow(-5, -5, null, -10)]
    public void Calculate_InLoop_Addition(int i, int j, int? valMin, int? valMax)
    {
        var code = $$"""
            var i = {{i}};
            var j = {{j}};
            while (Condition)
            {
                var value = i + j;
                Tag("Value", value);
            }
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        if (valMin.HasValue || valMax.HasValue)
        {
            validator.TagValues("Value").Should().AllSatisfy(x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(valMin, valMax)));
        }
        else
        {
            validator.TagValues("Value").Should().AllSatisfy(x => x.Should().HaveOnlyConstraints(ObjectConstraint.NotNull));
        }
    }
}

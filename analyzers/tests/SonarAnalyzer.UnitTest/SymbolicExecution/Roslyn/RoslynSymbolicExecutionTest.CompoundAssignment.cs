/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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
using SonarAnalyzer.UnitTest.TestFramework.SymbolicExecution;

namespace SonarAnalyzer.UnitTest.SymbolicExecution.Roslyn;

public partial class RoslynSymbolicExecutionTest
{
    [TestMethod]
    public void CompoundAssignment_UntrackedSymbol()
    {
        const string code = """
            var result = arg.Property += 1;
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code, "Sample arg").Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [TestMethod]
    public void CompoundAssignment_TrackedSymbolOnRightSide()
    {
        const string code = """
            var left = 40;
            var right = 2;
            var result = left += right;
            Tag("Left", left);
            Tag("Right", right);
            Tag("Result", result);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Left").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("Right").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(2));
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [DataTestMethod]
    [DataRow("i += j", 43)]
    [DataRow("i += 2", 44)]
    [DataRow("i += (condition ? 1 : 0)", 43)]
    [DataRow("i -= j", 41)]
    [DataRow("i -= 2", 40)]
    [DataRow("i -= (condition ? 1 : 0)", 41)]
    public void Compound_PlusAndMinus(string expression, int expected)
    {
        var code = $"""
            bool condition = true;
            var i = 42;
            var j = 1;
            {expression};
            Tag("I", i);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("I").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(expected));
    }

    [TestMethod]
    public void CompoundAssignment_SelfUpdate()
    {
        const string code = """
            var value = 21;
            var result = value += value;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull, NumberConstraint.From(42));
    }

    [DataTestMethod]
  // [DataRow("+=")]
  // [DataRow("-=")]
    [DataRow("*=")]
    [DataRow("/=")]
    [DataRow("%=")]
    [DataRow("&=")]
    [DataRow("|=")]
    [DataRow("^=")]
    [DataRow("<<=")]
    [DataRow(">>=")]
    [DataRow(">>>=")]
    public void CompoundAssignment_Arithmetics_Int(string op)
    {
        var code = $"""
            var value = 42;
            var result = value {op} 1;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("+=")]
    [DataRow("-=")]
    [DataRow("*=")]
    [DataRow("/=")]
    [DataRow("%=")]
    [DataRow("&=")]
    [DataRow("|=")]
    [DataRow("^=")]
    [DataRow("<<=")]
    [DataRow(">>=")]
    [DataRow(">>>=")]
    public void CompoundAssignment_Arithmetics_Char(string op)
    {
        var code = $"""
            var value = 'z';
            var result = value {op} 'a';
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }

    [DataTestMethod]
    [DataRow("&=")]
    [DataRow("|=")]
    [DataRow("^=")]
    public void CompoundAssignment_Logical(string op)
    {
        var code = $"""
            var value = true;
            var result = value {op} true;
            Tag("Result", result);
            Tag("Value", value);
            """;
        var validator = SETestContext.CreateCS(code).Validator;
        validator.TagValue("Result").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
        validator.TagValue("Value").Should().HaveOnlyConstraints(ObjectConstraint.NotNull);
    }
}

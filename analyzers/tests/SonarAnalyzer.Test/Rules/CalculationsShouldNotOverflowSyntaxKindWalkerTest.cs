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

using CalculationsShouldNotOverflow = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp.CalculationsShouldNotOverflow;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class CalculationsShouldNotOverflowSyntaxKindWalkerTest
{
    [DataTestMethod]
    [DataRow("_ = i + i;")]
    [DataRow("_ = i - i;")]
    [DataRow("_ = i * i;")]
    [DataRow("_ = ++i;")]
    [DataRow("_ = --i;")]
    [DataRow("_ = i++;")]
    [DataRow("_ = i--;")]
    [DataRow("i += 1;")]
    [DataRow("i -= 1;")]
    [DataRow("i *= 1;")]
    [DataRow("_ = (i + i, 0);")]
    [DataRow("""
        _ = i + i;
        _ = 1 / i;
        """)]
    [DataRow("""
        _ = ++i;
        _ = +i;
        """)]
    [DataRow("""
        _ = i++;
        _ = i!;
        """)]
    [DataRow("""
        i += 1;
        i /= 1;
        """)]
    public void HasOverflowExpressions_True(string statement)
    {
        var tree = TestHelper.CompileCS(WrapInMethod($"""
            var i = 0;
            {statement}
            """)).Tree;
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeTrue();
        sut.IsUnchecked.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("_ = i / 1;")]
    [DataRow("_ = i % 1;")]
    [DataRow("_ = i == 1;")]
    [DataRow("_ = i != 1;")]
    [DataRow("_ = i > 1;")]
    [DataRow("_ = i >= 1;")]
    [DataRow("_ = i < 1;")]
    [DataRow("_ = i <= 1;")]
    [DataRow("_ = i >>> 1;")]
    [DataRow("_ = i << 1;")]
    [DataRow("_ = i | 1;")]
    [DataRow("_ = i & 1;")]
    [DataRow("_ = b && b;")]
    [DataRow("_ = b || b;")]
    [DataRow("_ = b ^ b;")]
    [DataRow("_ = null ?? new object();")]
    [DataRow("_ = i is int;")]
    [DataRow("_ = i as int?;")]
    [DataRow("i /= 1;")]
    [DataRow("i %= 1;")]
    public void HasOverflowExpressions_False(string statement)
    {
        var tree = TestHelper.CompileCS(WrapInMethod($"""
            var i = 0; var b = true;
            {statement}
            """)).Tree;
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeFalse();
        sut.IsUnchecked.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""
        unchecked
        {
            _ = i + i;
        }
        """)]
    [DataRow("_ = unchecked(i + i);")]
    public void HasOverflowExpressions_IsUnchecked_True(string statement)
    {
        var tree = TestHelper.CompileCS(WrapInMethod($"""
            var i = 0;
            {statement}
            """)).Tree;
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeFalse();
        sut.IsUnchecked.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""
        checked
        {
            _ = i + i;
        }
        """, true, false)]
    [DataRow("_ = checked(i + i);", true, false)]
    [DataRow("_ = checked(unchecked(i + i));", false, true)]
    [DataRow("_ = checked(unchecked(i) + i);", true, true)]
    [DataRow("_ = unchecked(checked(i + i));", false, true)]
    [DataRow("_ = checked(unchecked(checked(i + i)));", false, true)]
    [DataRow("_ = unchecked(checked(unchecked(i + i)));", false, true)]
    [DataRow("_ = unchecked(i + i + checked(i + i));", false, true)]
    [DataRow("_ = unchecked(i + i + checked(i / i) + i + i);", false, true)]
    [DataRow("_ = unchecked(i + i + checked(i / unchecked(i + i)) + i + i);", false, true)]
    [DataRow("_ = unchecked(i / checked(i + i));", false, true)]
    [DataRow("_ = unchecked(i + checked(i / i));", false, true)]
    [DataRow("_ = unchecked(unchecked(i / checked(checked(i + i))));", false, true)]
    [DataRow("_ = unchecked(unchecked(i + checked(checked(i / i))));", false, true)]
    [DataRow("_ = checked(unchecked(i / unchecked(checked(i + i))));", false, true)]
    [DataRow("_ = checked(unchecked(i + unchecked(checked(i / i))));", false, true)]
    [DataRow("_ = checked(unchecked(i + unchecked(i + i)) + i);", true, true)]
    [DataRow("""
        unchecked
        {
            _ = i + i;
            checked
            {
                _ = i / i;
            }
            _ = i + i;
        }
        """, false, true)]
    [DataRow("""
        _ = i + i;
        unchecked
        {
        }
        """, true, true)]
    [DataRow("""
        unchecked
        {
            _ = i + i;
            checked
            {
                unchecked
                {
                    _ = i + i;
                }
                _ = i / i;
            }
            _ = i + i;
        }
        """, false, true)]
    [DataRow("""
        unchecked
        {
            checked
            {
                unchecked
                {
                }
                _ = i + i;
            }
        }
        """, false, true)]
    public void HasOverflowExpressions_Unchecked_Nesting(string statement, bool expectedHasOverflow, bool expectedIsUnchecked)
    {
        var tree = TestHelper.CompileCS(WrapInMethod($"""
            var i = 0;
            {statement}
            """)).Tree;
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().Be(expectedHasOverflow);
        sut.IsUnchecked.Should().Be(expectedIsUnchecked);
    }

    [DataTestMethod]
    [DataRow("_ = 1 + 1;", true)]            // Should be false as it is checked at compile time
    [DataRow("_ = int.MaxValue + 0;", true)] // OK: We would need to consult the semanticModel to learn that int.MaxValue is also a constant
    [DataRow("_ = i + 1;", true)]
    [DataRow("_ = 1 - 1;", true)]            // Should be false as it is checked at compile time
    [DataRow("_ = 1 * 1;", true)]            // Should be false as it is checked at compile time
    [DataRow("_ = i + 0;", true)]            // Should be false as one operand is the identity element of addition
    [DataRow("_ = i - 0;", true)]            // Should be false as one operand is the identity element of subtraction
    [DataRow("_ = i * 1;", true)]            // Should be false as one operand is the identity element of multiplication
    [DataRow("_ = i * 0;", true)]            // Should be false as the result is always 0
    [DataRow("""_ = "" + "";""", true)]      // Should be false as both sides are string literals
    [DataRow("""_ = s + "";""", true)]       // Should be false as right side is a string literal
    [DataRow("""_ = "" + s;""", true)]       // Should be false as left side is a string literal
    [DataRow("""_ = "" + s + s;""", true)]   // Should be false as one of the operands is a string literal
    [DataRow("""_ = s + "" + s;""", true)]   // Should be false as one of the operands is a string literal
    [DataRow("""_ = s + s + "";""", true)]   // Should be false as one of the operands is a string literal
    public void HasOverflowExpressions_Literals(string statement, bool expected)
    {
        var tree = TestHelper.CompileCS(WrapInMethod($"""
            var i = 0; var s = "";
            {statement}
            """)).Tree;
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().Be(expected);
        sut.IsUnchecked.Should().BeFalse();
    }

    private static string WrapInMethod(string body) =>
        $$"""
        using System;

        public class Test
        {
            public void M()
            {
                {{body}}
            }
        }
        """;
}

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

using CalculationsShouldNotOverflow = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp.CalculationsShouldNotOverflow;

namespace SonarAnalyzer.UnitTest.Rules;

[TestClass]
public class CalculationsShouldNotOverflowSyntaxWalkerTest
{
    [DataTestMethod]
    [DataRow("_ = 1 + 1;")]
    [DataRow("_ = 1 - 1;")]
    [DataRow("_ = 1 * 1;")]
    [DataRow("_ = ++i;")]
    [DataRow("_ = --i;")]
    [DataRow("_ = i++;")]
    [DataRow("_ = i--;")]
    [DataRow("i += 1;")]
    [DataRow("i -= 1;")]
    [DataRow("i *= 1;")]
    [DataRow("_ = (1 + 1, 0);")] // Test for "Stop visiting"
    public void HasOverflowExpressions_True(string statement)
    {
        var (tree, _) = TestHelper.CompileCS(WrapInMethod($$"""
            var i = 0;
            {{statement}}
            """));
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("_ = 1 / 1;")]
    [DataRow("_ = 1 % 1;")]
    [DataRow("_ = 1 == 1;")]
    [DataRow("_ = 1 != 1;")]
    [DataRow("_ = 1 > 1;")]
    [DataRow("_ = 1 >= 1;")]
    [DataRow("_ = 1 < 1;")]
    [DataRow("_ = 1 <= 1;")]
    [DataRow("_ = i >>> 1;")]
    [DataRow("_ = i << 1;")]
    [DataRow("_ = i | 1;")]
    [DataRow("_ = i & 1;")]
    [DataRow("_ = true && true;")]
    [DataRow("_ = true || true;")]
    [DataRow("_ = true ^ true;")]
    [DataRow("_ = null ?? new object();")]
    [DataRow("_ = i is int;")]
    [DataRow("_ = i as int?;")]
    [DataRow("i /= 1;")]
    [DataRow("i %= 1;")]
    public void HasOverflowExpressions_False(string statement)
    {
        var (tree, _) = TestHelper.CompileCS(WrapInMethod($$"""
            var i = 0;
            {{statement}}
            """));
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""
        unchecked
        {
            _ = 1 + 1;
        }
        """)]
    [DataRow("_ = unchecked(1 + 1);")]
    public void HasOverflowExpressions_False_InUnchecked(string statement)
    {
        var (tree, _) = TestHelper.CompileCS(WrapInMethod($$"""
            var i = 0;
            {{statement}}
            """));
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""
        checked
        {
            _ = 1 + 1;
        }
        """, true)]
    [DataRow("_ = checked(1 + 1);", true)]
    [DataRow("_ = checked(unchecked(1 + 1));", false)]
    [DataRow("_ = unchecked(checked(1 + 1));", true)]
    public void HasOverflowExpressions_Unchecked_Nesting(string statement, bool exepected)
    {
        var (tree, _) = TestHelper.CompileCS(WrapInMethod($$"""
            var i = 0;
            {{statement}}
            """));
        var sut = new CalculationsShouldNotOverflow.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.HasOverflow.Should().Be(exepected);
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

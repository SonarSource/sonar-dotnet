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

using VB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules.SymbolicExecution;

[TestClass]
public class ConditionEvaluatesToConstantSyntaxKindWalkerTest
{
    [DataTestMethod]
    [DataRow("""Dim x = If(True, 1, 2)""")]
    [DataRow("""Dim x = If("", "")""")]
    [DataRow("""Dim x = 1 AndAlso 2""")]
    [DataRow("""Dim x = 1 OrElse 2""")]
    [DataRow("""Dim x = Nothing?.ToString""")]
    [DataRow("""
        Do Until True
        Loop
    """)]
    [DataRow("""
        Do While True
        Loop
    """)]
    [DataRow("""
        Do
        Loop Until True
    """)]
    [DataRow("""
        Do
        Loop While True
    """)]
    [DataRow("""
        While True
        End While
    """)]
    [DataRow("""
        If True
        End If
    """)]
    [DataRow("""If True Then ToString""")]
    [DataRow("""If True Then ToString Else ToString""")]
    [DataRow("""
        If True
        End If
    """)]
    [DataRow("""
        Select case True
        End Select
    """)]
    public void SyntaxKindsChecks_True(string statement)
    {
        var tree = TestHelper.CompileVB(WrapInMethod($"""
            {statement}
            """)).Tree;
        var sut = new VB.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.ContainsCondition.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""Dim x = 1 And 2""")]
    [DataRow("""Dim x = 1 Or 2""")]
    [DataRow("""Dim x = 1 Xor 2""")]
    [DataRow("""Dim x = 1 Mod 2""")]
    [DataRow("""Dim x = 1 + 2""")]
    [DataRow("""Dim x = 1 - 2""")]
    [DataRow("""Dim x = 1 * 2""")]
    [DataRow("""Dim x = 1 / 2""")]
    [DataRow("""Dim x = 1 \ 2""")]
    [DataRow("""Dim x = 1 ^ 2""")]
    [DataRow("""Dim x = 1 << 2""")]
    [DataRow("""Dim x = 1 >> 2""")]
    [DataRow("""Dim x = 1 = 2""")]
    [DataRow("""Dim x = 1 <> 2""")]
    [DataRow("""Dim x = 1 > 2""")]
    [DataRow("""Dim x = 1 >= 2""")]
    [DataRow("""Dim x = 1 < 2""")]
    [DataRow("""Dim x = 1 <= 2""")]
    [DataRow("""Dim x = "" & "" """)]
    [DataRow("""Dim x = Nothing is Nothing """)]
    [DataRow("""Dim x = "" Like "" """)]
    [DataRow("""
        Dim x = From y in New Integer() { }
                Where True
    """)]
    public void SyntaxKindsChecks_False(string statement)
    {
        var tree = TestHelper.CompileVB(WrapInMethod($"""
            {statement}
            """)).Tree;
        var sut = new VB.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(tree.GetRoot());
        sut.ContainsCondition.Should().BeFalse();
    }

    private static string WrapInMethod(string statements)
        => $$"""
        Imports System

        Public Class Test
            Public Sub M()
                {{statements}}
            End Sub
        End Class
        """;
}

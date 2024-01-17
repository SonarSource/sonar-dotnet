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

using CS = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.CSharp;
using VB = SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

namespace SonarAnalyzer.Test.Rules.SymbolicExecution;

[TestClass]
public class ConditionEvaluatesToConstantSyntaxKindWalkerTest
{
    [DataTestMethod]
    [DataRow("""Dim x = If("", "")""")]
    [DataRow("Dim x = If(True, 1, 2)")]
    [DataRow("Dim x = True AndAlso False")]
    [DataRow("Dim x = True OrElse False")]
    [DataRow("Dim x = Nothing?.ToString")]
    [DataRow("Do Until True : Loop")]
    [DataRow("Do While True : Loop")]
    [DataRow("Do : Loop Until True")]
    [DataRow("Do : Loop While True")]
    [DataRow("While True : End While")]
    [DataRow("If True : End If")]
    [DataRow("If True Then ToString")]
    [DataRow("If True Then ToString Else ToString")]
    [DataRow("Select Case True : End Select")]
    public void SyntaxKindsChecks_VB_True(string statement)
    {
        var root = TestHelper.CompileVB(WrapInMethod_VB(statement)).Tree.GetRoot();
        var sut = new VB.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(root);
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
    [DataRow("""For i As Integer = 1 To 3 : Next""")]
    [DataRow("""For Each i As Integer In { 1, 2, 3 } : Next""")]
    public void SyntaxKindsChecks_VB_False(string statement)
    {
        var root = TestHelper.CompileVB(WrapInMethod_VB(statement)).Tree.GetRoot();
        var sut = new VB.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.ContainsCondition.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("_ = new object() ?? null;")]
    [DataRow("_ = true || false;")]
    [DataRow("_ = true && false;")]
    [DataRow("(new object())?.ToString();")]
    [DataRow("_ = true ? true : false;")]
    [DataRow("do { } while (true);")]
    [DataRow("for(;true;) { }")]
    [DataRow("if (true) { }")]
    [DataRow("switch (true) { }")]
    [DataRow("while (true) { }")]
    [DataRow("object o = null; o ??= new object();")]
    [DataRow("_ = true switch { _ => true };")]
    public void SyntaxKindsChecks_CS_True(string statement)
    {
        var root = TestHelper.CompileCS(WrapInMethod_CS(statement)).Tree.GetRoot();
        var sut = new CS.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.ContainsCondition.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("_ = 1 + 1;")]
    [DataRow("_ = 1 - 1;")]
    [DataRow("_ = 1 * 1;")]
    [DataRow("_ = 1 / 1;")]
    [DataRow("_ = 1 % 1;")]
    [DataRow("_ = 1 >> 1;")]
    [DataRow("_ = 1 << 1;")]
    [DataRow("_ = true | false;")]
    [DataRow("_ = true & false;")]
    [DataRow("_ = true ^ false;")]
    [DataRow("_ = 1 == 1;")]
    [DataRow("_ = 1 != 1;")]
    [DataRow("_ = 1 < 1;")]
    [DataRow("_ = 1 <= 1;")]
    [DataRow("_ = 1 >= 1;")]
    [DataRow("_ = 1 is 1;")]
    [DataRow("_ = new object() as object;")]
    [DataRow("for(;;) { }")]
    [DataRow("for(var i = 0; ; i++) { }")]
    // Undetected conditions
    [DataRow("try { } catch when (true) { }")]
    [DataRow("_ = from x in new int[] { } where x > 0 select x;")]
    [DataRow("_ = 1 is >2;")]
    [DataRow("_ = default(Exception) is { Message.Length: 1 };")]
    public void SyntaxKindsChecks_CS_False(string statement)
    {
        var root = TestHelper.CompileCS(WrapInMethod_CS(statement)).Tree.GetRoot();
        var sut = new CS.ConditionEvaluatesToConstant.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.ContainsCondition.Should().BeFalse();
    }

    private static string WrapInMethod_VB(string statements) =>
        $$"""
        Public Class Test
            Public Sub M()
                {{statements}}
            End Sub
        End Class
        """;

    private static string WrapInMethod_CS(string statements) =>
        $$"""
        using System;
        using System.Linq;
        public class Test
        {
            public void M()
            {
                {{statements}}
            }
        }
        """;
}

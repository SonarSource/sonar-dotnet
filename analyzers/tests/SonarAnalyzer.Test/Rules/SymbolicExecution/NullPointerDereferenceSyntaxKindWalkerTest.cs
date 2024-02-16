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
public class NullPointerDereferenceSyntaxKindWalkerTest
{
    [DataTestMethod]
    [DataRow("""await Task.Yield();""")]
    [DataRow("""await (Task)null;""")]
    [DataRow("""foreach(var i in new int[0]) { }""")]
    [DataRow("""this.ToString();""")]
    [DataRow("""_ = Int32.MaxValue;""")]
    [DataRow("""_ = (new int[0])[0];""")]
#if NET
    [DataRow("""foreach(var (i, j) in new(int, int)[0]) { }""")]
#endif
    public void SyntaxKindChecks_CS_True(string statement)
    {
        var root = TestHelper.CompileCS(WrapInMethod_CS(statement)).Tree.GetRoot();
        var sut = new CS.NullPointerDereference.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.Result.Should().BeTrue();
    }

    [TestMethod]
    public void SyntaxKindChecks_CS_PointerMemberAccess()
    {
        var root = TestHelper.CompileCS("""
            public class Test
            {
                public unsafe void M(int i)
                {
                    int* iPtr = &i;
                    iPtr->ToString();
                }
            }
            """).Tree.GetRoot();
        var sut = new CS.NullPointerDereference.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.Result.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""ToString();""")]
    [DataRow("""(ToString()?.Length)?.ToString();""")]
    public void SyntaxKindChecks_CS_False(string statement)
    {
        var root = TestHelper.CompileCS(WrapInMethod_CS(statement)).Tree.GetRoot();
        var sut = new CS.NullPointerDereference.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.Result.Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow("""Await Task.Yield()""")]
    [DataRow("""Dim t As Task : Await t""")]
    [DataRow("""For Each i In New Integer() { } : Next""")]
    [DataRow("""ToString()""")]
    [DataRow("""call ToString""")]
    [DataRow("""Dim arr(0) As Integer : Dim i = arr(0)""")]
    [DataRow("""Dim i = Int32.MaxValue""")]
    [DataRow("""Dim dict = New Dictionary(Of String, Integer) : Dim i = dict!Test""")]
    public void SyntaxKindChecks_VB_True(string statement)
    {
        var root = TestHelper.CompileVB(WrapInMethod_VB(statement)).Tree.GetRoot();
        var sut = new VB.NullPointerDereference.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.Result.Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow("""Dim i As System.Int32""")]
    [DataRow("""Dim i = 1 + 1""")]
    public void SyntaxKindChecks_VB_False(string statement)
    {
        var root = TestHelper.CompileVB(WrapInMethod_VB(statement)).Tree.GetRoot();
        var sut = new VB.NullPointerDereference.SyntaxKindWalker();
        sut.SafeVisit(root);
        sut.Result.Should().BeFalse();
    }

    private static string WrapInMethod_VB(string statements) =>
        $$"""
        Imports System.Threading.Tasks
        Public Class Test
            Public Async Function M() As Task
                {{statements}}
            End Function
        End Class
        """;

    private static string WrapInMethod_CS(string statements) =>
        $$"""
        using System;
        using System.Threading.Tasks;
        public class Test
        {
            public async Task M()
            {
                {{statements}}
            }
        }
        """;
}

/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class FieldAccessTrackerTest
    {
        private const string TestInputCS = @"
public class Sample
{
    private int assignConst;
    private int assignVariable;
    private int read;
    private int invocationArg;

    private void Usage()
    {
        var x = read;
        assignConst = 42;
        assignVariable = x;
        Method(invocationArg);
    }

    private void Method(int arg) { }
}";

        private const string TestInputVB = @"
Public Class Sample
    Private AssignConst As Integer
    Private AssignVariable As Integer
    Private Read As Integer
    Private InvocationArg As Integer

    Public Sub Usage()
        Dim X As Integer = Read
        AssignConst = 42
        AssignVariable = X
        Method(InvocationArg)
    End Sub

    Private Sub Method(Arg As Integer)
    End Sub
End Class";

        [TestMethod]
        public void MatchSet_CS()
        {
            var tracker = new CSharpFieldAccessTracker();
            var context = CreateContext<CSharpSyntax.IdentifierNameSyntax>(TestInputCS, "assignConst", AnalyzerLanguage.CSharp);
            tracker.MatchSet()(context).Should().BeTrue();

            context = CreateContext<CSharpSyntax.IdentifierNameSyntax>(TestInputCS, "read", AnalyzerLanguage.CSharp);
            tracker.MatchSet()(context).Should().BeFalse();
        }

        [TestMethod]
        public void MatchSet_VB()
        {
            var tracker = new VisualBasicFieldAccessTracker();
            var context = CreateContext<VBSyntax.IdentifierNameSyntax>(TestInputVB, "AssignConst", AnalyzerLanguage.VisualBasic);
            tracker.MatchSet()(context).Should().BeTrue();

            context = CreateContext<VBSyntax.IdentifierNameSyntax>(TestInputVB, "Read", AnalyzerLanguage.VisualBasic);
            tracker.MatchSet()(context).Should().BeFalse();
        }

        [TestMethod]
        public void AssignedValueIsConstant_CS()
        {
            var tracker = new CSharpFieldAccessTracker();
            var context = CreateContext<CSharpSyntax.IdentifierNameSyntax>(TestInputCS, "assignConst", AnalyzerLanguage.CSharp);
            tracker.AssignedValueIsConstant()(context).Should().BeTrue();

            context = CreateContext<CSharpSyntax.IdentifierNameSyntax>(TestInputCS, "assignVariable", AnalyzerLanguage.CSharp);
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.IdentifierNameSyntax>(TestInputCS, "invocationArg", AnalyzerLanguage.CSharp);
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();
        }

        [TestMethod]
        public void AssignedValueIsConstant_VB()
        {
            var tracker = new VisualBasicFieldAccessTracker();
            var context = CreateContext<VBSyntax.IdentifierNameSyntax>(TestInputVB, "AssignConst", AnalyzerLanguage.VisualBasic);
            tracker.AssignedValueIsConstant()(context).Should().BeTrue();

            context = CreateContext<VBSyntax.IdentifierNameSyntax>(TestInputVB, "AssignVariable", AnalyzerLanguage.VisualBasic);
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();

            context = CreateContext<VBSyntax.IdentifierNameSyntax>(TestInputVB, "InvocationArg", AnalyzerLanguage.VisualBasic);
            tracker.AssignedValueIsConstant()(context).Should().BeFalse();
        }

        private static FieldAccessContext CreateContext<TSyntaxNodeType>(string testInput, string fieldName, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, true, language);
            var node = testCode.GetNodes<TSyntaxNodeType>().First(x => x.ToString() == fieldName);
            return new FieldAccessContext(testCode.CreateAnalysisContext(node), fieldName);
        }
    }
}

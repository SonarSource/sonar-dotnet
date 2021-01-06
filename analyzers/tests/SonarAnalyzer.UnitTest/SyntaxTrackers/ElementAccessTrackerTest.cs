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

using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class ElementAccessTrackerTest
    {
        private const string TestInputCS = @"
public class Sample
{
    public int this[string key]
    {
        get => 42;
        set { }
    }

    public void Usage(Sample s, string arg)
    {
        s[""key""] = 42;
        this[""key""] = 42;
        s[arg] = 42;
        undefined[""key""] = 42;
    }
}";

        private const string TestInputVB = @"
Public Class Sample

    Default Public Property Item(Key As String) As Integer
        Get
            Return 42
        End Get
        Set(value As Integer)
        End Set
    End Property

    Public Sub NoArgs()
    End Sub

    Public Sub Usage(S As Sample, Arg As String)
        S(""key"") = 42
        Me(""key"") = 42
        S(Arg) = 42
        NoArgs 'and no ParameterList
        Undefined(""Key"") = 42
    End Sub

End Class";

        [TestMethod]
        public void ArgumentAtIndexIs_CS()
        {
            var context = CreateContext<CSharpSyntax.ElementAccessExpressionSyntax>(TestInputCS, 0, AnalyzerLanguage.CSharp);
            var tracker = new CSharpElementAccessTracker(null, null);
            tracker.ArgumentAtIndexIs(0, KnownType.System_String)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIs(0, KnownType.System_Int32)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIs(42, KnownType.System_String)(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.ElementAccessExpressionSyntax>(TestInputCS, 3, AnalyzerLanguage.CSharp);
            tracker.ArgumentAtIndexIs(0, KnownType.System_String)(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexIs_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 0, AnalyzerLanguage.VisualBasic);
            var tracker = new CSharpElementAccessTracker(null, null);
            tracker.ArgumentAtIndexIs(0, KnownType.System_String)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIs(0, KnownType.System_Int32)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIs(42, KnownType.System_String)(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 4, AnalyzerLanguage.VisualBasic);
            tracker.ArgumentAtIndexIs(0, KnownType.System_String)(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexEquals_CS()
        {
            var context = CreateContext<CSharpSyntax.ElementAccessExpressionSyntax>(TestInputCS, 0, AnalyzerLanguage.CSharp);
            var tracker = new CSharpElementAccessTracker(null, null);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.ElementAccessExpressionSyntax>(TestInputCS, 1, AnalyzerLanguage.CSharp);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();

            context = CreateContext<CSharpSyntax.ElementAccessExpressionSyntax>(TestInputCS, 2, AnalyzerLanguage.CSharp);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexEquals_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 0, AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicElementAccessTracker(null, null);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 1, AnalyzerLanguage.VisualBasic);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 2, AnalyzerLanguage.VisualBasic);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(0, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(42, "key")(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, 3, AnalyzerLanguage.VisualBasic);
            tracker.ArgumentAtIndexEquals(0, "key")(context).Should().BeFalse();
        }

        private static ElementAccessContext CreateContext<TSyntaxNodeType>(string testInput, int skip, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, true, language);
            var invocation = testCode.GetNodes<TSyntaxNodeType>().Skip(skip).First();
            var context = new ElementAccessContext(invocation, testCode.SemanticModel);
            return context;
        }
    }
}

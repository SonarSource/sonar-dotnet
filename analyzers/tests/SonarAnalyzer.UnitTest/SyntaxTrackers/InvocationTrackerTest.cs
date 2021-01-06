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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class InvocationTrackerTest
    {
        private const string TestInputCS = @"
public class Base
{
    private void MyMethod(string a, string b, bool c, int d, object e) {}

    private void Usage(string notAConst)
    {
        MyMethod(notAConst, ""myConst"", true, 4, new object());
        Undefined();
    }
}";

        private const string TestInputVB = @"
Public Class Base
    Public Sub MyMethod(ByVal a As String, b As String, ByVal c As Boolean, ByVal d As Integer, ByRef e As Integer, ByVal f As Object)
    End Sub

    Public Sub NoArgs()
    End Sub

    Public Sub Usage(ByVal notAConst As String)
        MyMethod(notAConst, ""myConst"", True, 4, 5, New Object())
        NoArgs 'and no ParameterList
    End Sub
End Class";

        [TestMethod]
        public void ConstArgumentForParameter_CS()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
            var tracker = new CSharpInvocationTracker(null, null);

            tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
            tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
            tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
            tracker.ConstArgumentForParameter(context, "e").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
        }

        [TestMethod]
        public void ArgumentIsBoolConstant_CS()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
            var tracker = new CSharpInvocationTracker(null, null);

            tracker.ArgumentIsBoolConstant("a", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("a", false)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("c", true)(context).Should().Be(true);
            tracker.ArgumentIsBoolConstant("c", false)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", false)(context).Should().Be(false);
        }

        [TestMethod]
        public void ConstArgumentForParameter_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);

            tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
            tracker.ConstArgumentForParameter(context, "c").Should().Be(true);
            tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
            tracker.ConstArgumentForParameter(context, "e").Should().Be(5);
            tracker.ConstArgumentForParameter(context, "f").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
        }

        [TestMethod]
        public void ArgumentIsBoolConstant_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);

            tracker.ArgumentIsBoolConstant("a", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("a", false)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("c", true)(context).Should().Be(true);
            tracker.ArgumentIsBoolConstant("c", false)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", false)(context).Should().Be(false);
        }

        [TestMethod]
        public void ArgumentAtIndexIsConstant_CS()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
            var tracker = new CSharpInvocationTracker(null, null);
            tracker.ArgumentAtIndexIsConstant(0)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIsConstant(1)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(2)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(3)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(4)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIsConstant(42)(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexIsConstant_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);
            tracker.ArgumentAtIndexIsConstant(0)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIsConstant(1)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(2)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(3)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(4)(context).Should().BeTrue();
            tracker.ArgumentAtIndexIsConstant(5)(context).Should().BeFalse();
            tracker.ArgumentAtIndexIsConstant(42)(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "NoArgs", AnalyzerLanguage.VisualBasic, 1);
            tracker.ArgumentAtIndexIsConstant(0)(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexEquals_CS()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(TestInputCS, "MyMethod", AnalyzerLanguage.CSharp);
            var tracker = new CSharpInvocationTracker(null, null);
            tracker.ArgumentAtIndexEquals(0, "myConst")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(1, "myConst")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(1, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(2, "true")(context).Should().BeFalse();   // Not a string
            tracker.ArgumentAtIndexEquals(42, "myConst")(context).Should().BeFalse();
        }

        [TestMethod]
        public void ArgumentAtIndexEquals_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);
            tracker.ArgumentAtIndexEquals(0, "myConst")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(1, "myConst")(context).Should().BeTrue();
            tracker.ArgumentAtIndexEquals(1, "foo")(context).Should().BeFalse();
            tracker.ArgumentAtIndexEquals(2, "true")(context).Should().BeFalse();   // Not a string
            tracker.ArgumentAtIndexEquals(42, "myConst")(context).Should().BeFalse();

            context = CreateContext<VBSyntax.InvocationExpressionSyntax>(TestInputVB, "NoArgs", AnalyzerLanguage.VisualBasic, 1);
            tracker.ArgumentAtIndexEquals(0, "myConst")(context).Should().BeFalse();
        }

        [TestMethod]
        public void InvocationConditionForUndefinedMethod()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(TestInputCS, "Undefined", AnalyzerLanguage.CSharp, 1);
            var tracker = new CSharpInvocationTracker(null, null);
            tracker.MethodIsStatic()(context).Should().BeFalse();
            tracker.MethodIsExtension()(context).Should().BeFalse();
            tracker.MethodHasParameters(0)(context).Should().BeFalse();
            tracker.MethodReturnTypeIs(KnownType.Void)(context).Should().BeFalse();
        }

        private static InvocationContext CreateContext<TSyntaxNodeType>(string testInput, string methodName, AnalyzerLanguage language, int skip = 0) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, true, language);
            var invocationSyntaxNode = testCode.GetNodes<TSyntaxNodeType>().Skip(skip).First();
            var context = new InvocationContext(invocationSyntaxNode, methodName, testCode.SemanticModel);
            return context;
        }
    }
}

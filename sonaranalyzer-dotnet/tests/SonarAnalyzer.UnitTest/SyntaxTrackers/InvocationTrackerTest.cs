/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
    public class InvocationTrackerTest
    {
        private const string testInputCS = @"
namespace NS
{
  public class Base
  {
    private void MyMethod(string a, string b, bool c, int d, object e) {}

    private void Usage(string notAConst)
    {
      MyMethod(notAConst, ""myConst"", true, 4, new object());
    }
  }
}
";

        private const string testInputVB = @"
Namespace NS
    Public Class Base
        Public Sub MyMethod(ByVal a As String, b As String, ByVal c As Boolean, ByVal d As Integer, ByRef e As Integer, ByVal f As Object)
        End Sub

        Public Sub Usage(ByVal notAConst As String)
            MyMethod(notAConst, ""myConst"", False, 4, 5, New Object())
        End Sub
    End Class
End Namespace
";

        [TestMethod]
        public void ConstArgumentForParameter_CS()
        {
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(testInputCS, "MyMethod", AnalyzerLanguage.CSharp);
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
            var context = CreateContext<CSharpSyntax.InvocationExpressionSyntax>(testInputCS, "MyMethod", AnalyzerLanguage.CSharp);
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
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(testInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);

            tracker.ConstArgumentForParameter(context, "a").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "b").Should().Be("myConst");
            tracker.ConstArgumentForParameter(context, "c").Should().Be(false);
            tracker.ConstArgumentForParameter(context, "d").Should().Be(4);
            tracker.ConstArgumentForParameter(context, "e").Should().Be(5);
            tracker.ConstArgumentForParameter(context, "f").Should().BeNull();
            tracker.ConstArgumentForParameter(context, "nonExistingParameterName").Should().BeNull();
        }

        [TestMethod]
        public void ArgumentIsBoolConstant_VB()
        {
            var context = CreateContext<VBSyntax.InvocationExpressionSyntax>(testInputVB, "MyMethod", AnalyzerLanguage.VisualBasic);
            var tracker = new VisualBasicInvocationTracker(null, null);

            tracker.ArgumentIsBoolConstant("a", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("a", false)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("c", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("c", false)(context).Should().Be(true);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", true)(context).Should().Be(false);
            tracker.ArgumentIsBoolConstant("nonExistingParameterName", false)(context).Should().Be(false);
        }

        private static InvocationContext CreateContext<TSyntaxNodeType>(string testInput, string methodName, AnalyzerLanguage language) where TSyntaxNodeType : SyntaxNode
        {
            var testCode = new SnippetCompiler(testInput, ignoreErrors: false, language: language);
            var invocationSyntaxNode = testCode.GetNodes<TSyntaxNodeType>().First();
            var context = new InvocationContext(invocationSyntaxNode, methodName, testCode.SemanticModel);
            return context;
        }
    }
}

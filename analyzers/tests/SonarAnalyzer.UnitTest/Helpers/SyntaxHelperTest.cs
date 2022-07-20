/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Extensions;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SyntaxHelperTest
    {
        private const string CsSourceInputToString =
@"class Example
{
    string Method(object input)
    {
        System.Exception qualified;
        global::System.Exception global;
        return input.ToString();
    }
}";

        private const string VbSourceInputToString =
@"Class Example
    Function Method(Input As Object) As String
        Dim Qualified As System.Exception
        Dim GlobalName As Global.System.Exception
        Return Input.ToString()
    End Function
End Class";

        [Ignore][TestMethod]
        public void GetName_CS()
        {
            var nodes = Parse_CS(CsSourceInputToString);
            nodes.OfType<CS.MemberAccessExpressionSyntax>().Single().GetName().Should().Be("ToString");
            nodes.OfType<CS.IdentifierNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("System", "Exception", "global", "System", "Exception", "input", "ToString");
            nodes.OfType<CS.QualifiedNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("Exception", "Exception");
            nodes.OfType<CS.InvocationExpressionSyntax>().Single().GetName().Should().BeEmpty();
        }

        [Ignore][TestMethod]
        public void GetName_VB()
        {
            var nodes = Parse_VB(VbSourceInputToString);
            nodes.OfType<VB.ClassStatementSyntax>().Single().GetName().Should().Be("Example");
            nodes.OfType<VB.MethodBlockSyntax>().Single().GetName().Should().Be("Method");
            nodes.OfType<VB.MethodStatementSyntax>().Single().GetName().Should().Be("Method");
            nodes.OfType<VB.ParameterSyntax>().Single().GetName().Should().Be("Input");
            nodes.OfType<VB.PredefinedTypeSyntax>().First().GetName().Should().Be("Object");
            nodes.OfType<VB.MemberAccessExpressionSyntax>().Single().GetName().Should().Be("ToString");
            nodes.OfType<VB.IdentifierNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("System", "Exception", "System", "Exception", "Input", "ToString");
            nodes.OfType<VB.QualifiedNameSyntax>().Select(x => x.GetName()).Should().BeEquivalentTo("Exception", "Exception", "System");
            nodes.OfType<VB.InvocationExpressionSyntax>().Single().GetName().Should().Be("ToString");
            nodes.OfType<VB.ReturnStatementSyntax>().Single().GetName().Should().BeEmpty();
        }

        [Ignore][TestMethod]
        public void NameIs_CS()
        {
            var toString = Parse_CS(CsSourceInputToString).OfType<CS.MemberAccessExpressionSyntax>().Single();
            toString.NameIs("ToString").Should().BeTrue();
            toString.NameIs("TOSTRING").Should().BeFalse();
            toString.NameIs("tostring").Should().BeFalse();
            toString.NameIs("test").Should().BeFalse();
            toString.NameIs("").Should().BeFalse();
            toString.NameIs(null).Should().BeFalse();
        }

        [Ignore][TestMethod]
        public void NameIs_VB()
        {
            var toString = Parse_VB(VbSourceInputToString).OfType<VB.MemberAccessExpressionSyntax>().Single();
            toString.NameIs("ToString").Should().BeTrue();
            toString.NameIs("TOSTRING").Should().BeTrue();
            toString.NameIs("tostring").Should().BeTrue();
            toString.NameIs("test").Should().BeFalse();
            toString.NameIs("").Should().BeFalse();
            toString.NameIs(null).Should().BeFalse();
        }

        [Ignore][TestMethod]
        public void IsAssignmentToTuple_IsTrue()
        {
            const string code = @"
public class Sample
{
    public void Method()
    {
        int a, b;
        (a, b) = (42, 42);
    }
}";
            var argument = Parse_CS(code).OfType<CS.ArgumentSyntax>().First();
            argument.IsInTupleAssignmentTarget().Should().BeTrue();
        }

        [Ignore][TestMethod]
        public void IsAssignmentToTuple_IsFalse()
        {
            const string code = @"
public class Sample
{
    public void TupleArg((int, int) tuple) { }

    public void Method(int methodArgument)
    {
        int a = 42;
        var t = (a, methodArgument);
        var x = (42, 42);
        var nested = (42, (42, 42));
        TupleArg((42, 42));
        Method(0);
    }
}";
            var arguments = Parse_CS(code).OfType<CS.ArgumentSyntax>().ToArray();

            arguments.Should().HaveCount(12);
            foreach (var argument in arguments)
            {
                argument.IsInTupleAssignmentTarget().Should().BeFalse();
            }
        }

        [DataTestMethod]
        // Simple tuple
        [DataRow("($$1, (2, 3))", "(1, (2, 3))")]
        [DataRow("(1, ($$2, 3))", "(1, (2, 3))")]
        [DataRow("(1, (2, $$3))", "(1, (2, 3))")]
        // With method call with single argument
        [DataRow("($$1, (M(2), 3))", "(1, (M(2), 3))")]
        [DataRow("(1, ($$M(2), 3))", "(1, (M(2), 3))")]
        [DataRow("(1, (M($$2), 3))", null)]
        // With method call with two arguments
        [DataRow("(1, $$M(2, 3))", "(1, M(2, 3))")]
        [DataRow("(1, M($$2, 3))", null)]
        [DataRow("(1, M(2, $$3))", null)]
        // With method call with tuple argument
        [DataRow("($$M((1, 2)), 3)", "(M((1, 2)), 3)")]
        [DataRow("(M($$(1, 2)), 3)", null)]
        [DataRow("(M(($$1, 2)), 3)", "(1, 2)")]
        public void OutermostTuple_DifferentPositions(string tuple, string expectedOuterTuple)
        {
            // Arrange
            var code =
@$"
public class C
{{
    public void Test()
    {{
        _ = {tuple};
    }}

    static int M(int a) => 0;
    static int M(int a, int b) => 0;
    static int M((int a, int b) t) => 0;
}}";
            var nodePosition = code.IndexOf("$$");
            code = code.Replace("$$", string.Empty);
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            syntaxTree.GetDiagnostics().Should().BeEmpty();
            var nodeAtPosition = syntaxTree.GetRoot().FindNode(new TextSpan(nodePosition, 0), getInnermostNodeForTie: true);
            var argument = nodeAtPosition?.AncestorsAndSelf().OfType<CS.ArgumentSyntax>().First();
            // Act
            var outerMostTuple = argument.OutermostTuple();
            // Assert
            if (expectedOuterTuple is null)
            {
                outerMostTuple.Should().BeNull();
            }
            else
            {
                outerMostTuple.Should().NotBeNull();
                outerMostTuple.Value.SyntaxNode.ToString().Should().Be(expectedOuterTuple);
            }
        }

        [Ignore][TestMethod]
        public void IsNullLiteral_Null_CS() =>
            CSharpSyntaxHelper.IsNullLiteral(null).Should().BeFalse();

        [Ignore][TestMethod]
        public void IsNothingLiteral_Null_VB() =>
            VisualBasicSyntaxHelper.IsNothingLiteral(null).Should().BeFalse();

        private static SyntaxNode[] Parse_CS(string source) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();

        private static SyntaxNode[] Parse_VB(string source) =>
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();
    }
}

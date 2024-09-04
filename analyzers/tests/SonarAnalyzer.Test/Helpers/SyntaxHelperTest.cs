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

using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.CSharp.Core.Syntax.Extensions;
using SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;
using CS = Microsoft.CodeAnalysis.CSharp.Syntax;
using VB = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Test.Helpers
{
    [TestClass]
    public class SyntaxHelperTest
    {
        private const string CsSourceInputToString =
@"
using System;
using C = System.Collections;
namespace MyNamespace.MyNamespaceNested
{
    class Example
    {
        delegate void MyDelegate();
        enum MyEnum { MyEnumValue };

        Example() { }
        ~Example() { }
        int MyProp { get; }

        unsafe ref byte Method<T>(byte[] input) where T: new()
        {
            int? i = null;
            int* iPtr;
            System.Exception qualified;
            global::System.Exception global;
            input.ToString()?.ToString();
            Func<Action> fun = () => () => {};
            fun()();
            ref byte result = ref input[0];
            return ref result;
        }
        public static explicit operator int(Example e) => 0;
        public static int operator +(Example e) => 0;
        ref struct MyRefStruct { }
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

        [TestMethod]
        public void GetName_CS()
        {
            var nodes = Parse_CS(CsSourceInputToString);
            Assert<CS.AliasQualifiedNameSyntax>("global");
            Assert<CS.ArrayTypeSyntax>("byte");
            Assert<CS.BaseTypeDeclarationSyntax>("Example", "MyEnum", "MyRefStruct");
            Assert<CS.ConstructorDeclarationSyntax>("Example");
            Assert<CS.ConversionOperatorDeclarationSyntax>("int");
            Assert<CS.DelegateDeclarationSyntax>("MyDelegate");
            Assert<CS.DestructorDeclarationSyntax>("Example");
            Assert<CS.EnumMemberDeclarationSyntax>("MyEnumValue");
            Assert<CS.IdentifierNameSyntax>("System", "C", "System", "Collections", "MyNamespace", "MyNamespaceNested", "T", "System", "Exception", "global", "System", "Exception", "input", "ToString", "ToString", "Action", "fun", "input", "result", "Example", "Example");
            Assert<CS.InvocationExpressionSyntax>("ToString", "ToString", string.Empty, "fun");
            Assert<CS.MethodDeclarationSyntax>("Method");
            Assert<CS.MemberAccessExpressionSyntax>("ToString");
            Assert<CS.MemberBindingExpressionSyntax>("ToString");
            Assert<CS.NamespaceDeclarationSyntax>("MyNamespaceNested");
            Assert<CS.NullableTypeSyntax>("int");
            Assert<CS.OperatorDeclarationSyntax>("+");
            Assert<CS.ParameterSyntax>("input", "e", "e");
            Assert<CS.PropertyDeclarationSyntax>("MyProp");
            Assert<CS.PointerTypeSyntax>("int");
            Assert<CS.PredefinedTypeSyntax>("void", "int", "int", "int", "int", "int", "byte", "byte", "byte");
            Assert<CS.QualifiedNameSyntax>("Collections", "MyNamespaceNested", "Exception", "Exception");
            Assert<CS.SimpleNameSyntax>("System", "C", "System", "Collections", "MyNamespace", "MyNamespaceNested", "T", "System", "Exception", "global", "System", "Exception", "input", "ToString", "ToString", "Func", "Action", "fun", "input", "result", "Example", "Example");
            Assert<CS.TypeParameterConstraintClauseSyntax>("T");
            Assert<CS.TypeParameterSyntax>("T");
            Assert<CS.UsingDirectiveSyntax>(string.Empty, "C");
            Assert<CS.VariableDeclaratorSyntax>("i", "iPtr", "qualified", "global", "fun", "result");
            Assert<CS.RefTypeSyntax>("byte", "byte");
            Assert<CS.ReturnStatementSyntax>(string.Empty);

            void Assert<T>(params string[] expectedNames) where T : SyntaxNode =>
                nodes.OfType<T>().Select(x => SyntaxNodeExtensionsCSharp.GetName(x)).Should().BeEquivalentTo(expectedNames, because: "GetName for {0} should return the identifier", typeof(T));
        }

        [TestMethod]
        public void GetName_VB()
        {
            var nodes = Parse_VB(VbSourceInputToString);
            GetName(nodes.OfType<VB.ClassStatementSyntax>().Single()).Should().Be("Example");
            GetName(nodes.OfType<VB.MethodBlockSyntax>().Single()).Should().Be("Method");
            GetName(nodes.OfType<VB.MethodStatementSyntax>().Single()).Should().Be("Method");
            GetName(nodes.OfType<VB.ParameterSyntax>().Single()).Should().Be("Input");
            GetName(nodes.OfType<VB.PredefinedTypeSyntax>().First()).Should().Be("Object");
            GetName(nodes.OfType<VB.MemberAccessExpressionSyntax>().Single()).Should().Be("ToString");
            nodes.OfType<VB.IdentifierNameSyntax>().Select(x => GetName(x)).Should().BeEquivalentTo("System", "Exception", "System", "Exception", "Input", "ToString");
            nodes.OfType<VB.QualifiedNameSyntax>().Select(x => GetName(x)).Should().BeEquivalentTo("Exception", "Exception", "System");
            GetName(nodes.OfType<VB.InvocationExpressionSyntax>().Single()).Should().Be("ToString");
            GetName(nodes.OfType<VB.ReturnStatementSyntax>().Single()).Should().BeEmpty();

            static string GetName(SyntaxNode node) => SyntaxNodeExtensionsVisualBasic.GetName(node);
        }

        [TestMethod]
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

        [DataTestMethod]
        [DataRow(true, "Test")]
        [DataRow(true, "Test", "Test")]
        [DataRow(true, "Other", "Test")]
        [DataRow(false)]
        [DataRow(false, "TEST")]
        public void NameIsOrNames_CS(bool expected, params string[] orNames)
        {
            var identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Test");
            identifier.NameIs("other", orNames).Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("Strasse", "Straße", false)] // StringComparison.InvariantCulture returns in this case and so do other cultures like de-DE
        [DataRow("\u00F6", "\u006F\u0308", false)] // 00F6 = ö; 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
        [DataRow("ö", "Ö", false)]
        [DataRow("ö", "\u00F6", true)]
        public void NameIs_CultureSensitivity(string identifierName, string actual, bool expected)
        {
            var identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifierName);
            identifier.NameIs(actual).Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow(false, "Strasse", "Straße")] // StringComparison.InvariantCulture returns in this case and so do other cultures like de-DE
        [DataRow(false, "\u00F6", "\u006F\u0308")] // 00F6 = ö; 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
        [DataRow(false, "ö", "\u006F\u0308", "ä", "oe")] // 006F = o; 0308 = https://www.fileformat.info/info/unicode/char/0308/index.htm
        [DataRow(false, "Köln", "Koeln", "Cologne, ", "köln")]
        [DataRow(true, "Köln", "Koeln", "Cologne, ", "K\u00F6ln")] // 00F6 = ö
        public void NameIsOrNames_CultureSensitivity(bool expected, string identifierName, string name, params string[] orNames)
        {
            var identifier = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(identifierName);
            identifier.NameIs(name, orNames).Should().Be(expected);
        }

        [TestMethod]
        public void NameIsOrNamesNodeWithoutName_CS()
        {
            var returnStatement = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement();
            returnStatement.NameIs("A", "B", "C").Should().BeFalse();
        }

        [TestMethod]
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

        [DataTestMethod]
        [DataRow("($$a, b) = (42, 42);")]
        [DataRow("(a, $$b) = (42, 42);")]
        [DataRow("(a, (b, $$c)) = (42, (42, 42));")]
        [DataRow("(a, (b, ($$c, d))) = (42, (42, (42, 42)));")]
        public void IsInTupleAssignmentTarget_IsTrue(string assignment)
        {
            var code = $@"
public class Sample
{{
    public void Method()
    {{
        int a, b, c, d;
        {assignment}
    }}
}}";
            var argument = GetTupleArgumentAtMarker(ref code);
            argument.IsInTupleAssignmentTarget().Should().BeTrue();
        }

        [TestMethod]
        public void IsInTupleAssignmentTarget_IsFalse()
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
            var argument = GetTupleArgumentAtMarker(ref code);
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

        [TestMethod]
        public void IsNullLiteral_Null_CS() =>
            SyntaxNodeExtensionsCSharp.IsNullLiteral(null).Should().BeFalse();

        [TestMethod]
        public void IsNothingLiteral_Null_VB() =>
            SyntaxNodeExtensionsVisualBasic.IsNothingLiteral(null).Should().BeFalse();

        private static SyntaxNode[] Parse_CS(string source) =>
            Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();

        private static SyntaxNode[] Parse_VB(string source) =>
            Microsoft.CodeAnalysis.VisualBasic.VisualBasicSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();

        private static CS.ArgumentSyntax GetTupleArgumentAtMarker(ref string code)
        {
            var nodePosition = code.IndexOf("$$");
            code = code.Replace("$$", string.Empty);
            var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(code);
            syntaxTree.GetDiagnostics().Should().BeEmpty();
            var nodeAtPosition = syntaxTree.GetRoot().FindNode(new TextSpan(nodePosition, 0), getInnermostNodeForTie: true);
            var argument = nodeAtPosition?.AncestorsAndSelf().OfType<CS.ArgumentSyntax>().First();
            return argument;
        }
    }
}

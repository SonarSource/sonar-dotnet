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

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBSyntax = Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class MethodDescriptorTest
    {
        private static InvocationContext xmlNodeCloneNodeInvocationContext;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            const string code = @"
namespace Test
{
    using System.Xml;

    class Class1
    {
        public void DoStuff(XmlNode node)
        {
            node.CloneNode(true);
        }
    }
}
";
            var snippet = new SnippetCompiler(code, MetadataReferenceFacade.SystemXml);
            xmlNodeCloneNodeInvocationContext = CreateContextForMethod("XmlNode.CloneNode", snippet);
        }

        [TestMethod]
        public void IsMatch_WhenMethodNameIsNull_ReturnsFalse()
        {
            var underTest = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            underTest.IsMatch(null, new Mock<ITypeSymbol>().Object, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
        }

        [TestMethod]
        public void IsMatch_WhenTypeSymbolIsNull_ReturnsFalse()
        {
            var underTest = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            underTest.IsMatch("CloneNode", null, StringComparison.OrdinalIgnoreCase).Should().BeFalse();
        }

        [DataRow(null, StringComparison.InvariantCultureIgnoreCase)]
        [DataRow("", StringComparison.InvariantCultureIgnoreCase)]
        [DataRow("Clone", StringComparison.InvariantCultureIgnoreCase)]
        [DataRow("clonenode", StringComparison.InvariantCulture)]
        [DataTestMethod]
        public void IsMatch_WhenMethodNameDoesNotMatch_ReturnsFalseDoesNotEvaluateSymbol(string memberName, StringComparison stringComparison)
        {
            var underTest = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            var shouldNotBeUsed = new Lazy<IMethodSymbol>(() => throw new NotSupportedException());
            underTest.IsMatch(memberName, shouldNotBeUsed, stringComparison).Should().BeFalse();
        }

        [TestMethod]
        public void IsMatch_WhenTypeMatchesButNameIsDifferentCase_ReturnsFalse()
        {
            var underTest = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            underTest.IsMatch("clonenode", xmlNodeCloneNodeInvocationContext.MethodSymbol, StringComparison.InvariantCulture).Should().BeFalse();
        }

        [TestMethod]
        public void IsMatch_WhenMethodNameAndTypeMatch_ReturnsTrue()
        {
            const string code = @"
namespace Test
{
    using System.Xml;

    class Class1
    {
        public void DoStuff(XmlNode node)
        {
            node.CloneNode(true);
        }
    }
}
";
            var snippet = new SnippetCompiler(code, MetadataReferenceFacade.SystemXml);
            var cloneNodeContext = CreateContextForMethod("XmlNode.CloneNode", snippet);

            var underTest = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            underTest.IsMatch("CloneNode", cloneNodeContext.MethodSymbol, StringComparison.InvariantCulture).Should().BeTrue();
            underTest.IsMatch("clonenode", cloneNodeContext.MethodSymbol, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
        }

        [TestMethod]
        public void ExactMatchOnly_OverridesAreNotMatched_CS()
        {
            const string code = @"
namespace Test
{
  class Class1
  {
    public void DoStuff()
    {
        System.Console.WriteLine();
        this.WriteLine();
    }

    private void WriteLine() {}
  }
}
";
            var snippet = new SnippetCompiler(code);

            CheckExactMatchOnly_OverridesAreNotMatched(snippet);
        }

        [TestMethod]
        public void ExactMatchOnly_OverridesAreNotMatched_VB()
        {
            const string code = @"
Namespace Test
    Class Class1
        Public Sub DoStuff()

            System.Console.WriteLine()
            Me.WriteLine()
        End Sub

        Private Sub WriteLine()
            ' empty
        End Sub
    End Class
End Namespace
";
            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);

            CheckExactMatchOnly_OverridesAreNotMatched(snippet);
        }

        [TestMethod]
        public void ExactMatch_DoesNotMatchOverrides_CS()
        {
            const string code = @"
namespace Test
{
    using System.Xml;

    class Class1
    {
        public void DoStuff(XmlNode node, XmlDocument doc)
        {
            node.WriteTo(null);
            doc.WriteTo(null);
        }
    }
}
";
            var snippet = new SnippetCompiler(code, MetadataReferenceFacade.SystemXml);
            CheckExactMatch_DoesNotMatchOverrides(snippet);
        }

        [TestMethod]
        public void ExactMatch_DoesNotMatchOverrides_VB()
        {
            const string code = @"
Imports System.Xml
Namespace Test
    Class Class1
        Public Sub DoStuff(node As XmlNode, doc As XmlDocument)
            node.WriteTo(Nothing)
            doc.WriteTo(Nothing)
        End Sub
    End Class
End Namespace
";
            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic,
                MetadataReferenceFacade.SystemXml);
            CheckExactMatch_DoesNotMatchOverrides(snippet);
        }

        [TestMethod]
        public void MatchesAny_AndCheckingOverrides_DoesMatchOverrides_CS()
        {
            const string code = @"
namespace Test
{
    using System.Xml;

    class Class1
    {
        public void DoStuff(XmlNode node, XmlDocument doc)
        {
            node.WriteTo(null);
            doc.WriteTo(null);
        }
    }
}
";
            var snippet = new SnippetCompiler(code, MetadataReferenceFacade.SystemXml);
            CheckMatchesAny_AndCheckingOverrides_DoesMatchOverrides(snippet);
        }

        [TestMethod]
        public void MatchesAny_MethodAndTypeCombination_FindsCorrectOne()
        {
            const string code = @"
namespace Test
{
    using System.Xml;

    class Class1
    {
        public void DoStuff(XmlNode node)
        {
            node.CloneNode(true);
        }
    }
}
";
            var snippet = new SnippetCompiler(code, MetadataReferenceFacade.SystemXml);

            var nodeClone = new MemberDescriptor(KnownType.System_Xml_XmlNode, "Clone");
            var nodeCloneNode = new MemberDescriptor(KnownType.System_Xml_XmlNode, "CloneNode");
            var docCloneNode = new MemberDescriptor(KnownType.System_Xml_XmlDocument, "CloneNode");

            var underTest = CreateContextForMethod("XmlNode.CloneNode", snippet);
            // this should be false, because on XmlNode we check only Clone(), not CloneNode()
            // the implementation should correctly map the method name with the type
            CheckIsMethodOrDerived(false, underTest, nodeClone, docCloneNode);
            // here, we verify if XmlNode.CloneNode() is called, which is true
            CheckIsMethodOrDerived(true, underTest, nodeCloneNode, docCloneNode);
        }

        [TestMethod]
        public void MatchesAny_AndCheckingOverrides_DoesMatchOverrides_VB()
        {
            const string code = @"
Imports System.Xml
Namespace Test
    Class Class1
        Public Sub DoStuff(node As XmlNode, doc As XmlDocument)
            node.WriteTo(Nothing)
            doc.WriteTo(Nothing)
        End Sub
    End Class
End Namespace
";
            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic, MetadataReferenceFacade.SystemXml);
            CheckMatchesAny_AndCheckingOverrides_DoesMatchOverrides(snippet);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_CS()
        {
            const string code = @"
namespace Test
{
    sealed class Class1 : System.IDisposable
    {
        public void Test()
        {
            this.Dispose();
        }

        public void Dispose() { /* no-op */ }
    }
}
";

            var snippet = new SnippetCompiler(code);
            DoCheckMatch_InterfaceMethods(snippet);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_VB()
        {
            const string code = @"
Namespace Test
    NotInheritable Class Class1
        Implements System.IDisposable

        Public Sub Test()
            Dispose()
        End Sub

        Public Sub Dispose() Implements System.IDisposable.Dispose
            ' no-op
        End Sub
    End Class
End Namespace
";

            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
            DoCheckMatch_InterfaceMethods(snippet);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_NameMatchButNotOverride_CS()
        {
            const string code = @"
namespace Test
{
    sealed class Class1 : System.IDisposable
    {
        public void Test()
        {
            Dispose(42);      // <-- FALSE POSITIVE: shouldn't match since not implementing IDisposable.Dispose
        }

        public void Dispose(int data) {  /* no-op */ }

        void System.IDisposable.Dispose() { /* no-op */ }
    }
}
";
            var snippet = new SnippetCompiler(code);
            DoCheckMatch_InterfaceMethods(snippet);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_NameMatchButNotOverride_VB()
        {
            const string code = @"
Namespace Test
    NotInheritable Class Class1
        Implements System.IDisposable

        Public Sub Test()
            Dispose(42)     ' <-- FALSE POSITIVE: shouldn't match since not implementing IDisposable.Dispose
        End Sub

        Public Sub Dispose(Data As Integer)
            ' no-op
        End Sub

        Private Sub Dispose() Implements System.IDisposable.Dispose
            ' no-op
        End Sub
    End Class
End Namespace
";

            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
            DoCheckMatch_InterfaceMethods(snippet);
        }

        [TestMethod]
        public void CheckMatch_CaseInsensitivity()
        {
            const string code = @"
Namespace Test
    NotInheritable Class Class1
        Implements System.IDisposable

        Public Sub Test()
            Dispose()
        End Sub

        Public Sub Dispose() Implements System.IDisposable.Dispose
            ' no-op
        End Sub
    End Class
End Namespace
";
            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic);
            var dispose = new MemberDescriptor(KnownType.System_IDisposable, "Dispose");
            var callToDispose = CreateContextForMethod("Class1.Dispose", snippet);

            var result = MemberDescriptor.MatchesAny("Dispose", callToDispose.MethodSymbol, true, StringComparison.Ordinal, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("dispose", callToDispose.MethodSymbol, true, StringComparison.Ordinal, dispose);
            result.Should().Be(false);
            result = MemberDescriptor.MatchesAny("DISPOSE", callToDispose.MethodSymbol, true, StringComparison.Ordinal, dispose);
            result.Should().Be(false);

            result = MemberDescriptor.MatchesAny("Dispose", callToDispose.MethodSymbol, true, StringComparison.OrdinalIgnoreCase, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("dispose", callToDispose.MethodSymbol, true, StringComparison.OrdinalIgnoreCase, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("DISPOSE", callToDispose.MethodSymbol, true, StringComparison.OrdinalIgnoreCase, dispose);
            result.Should().Be(true);
        }

        private static InvocationContext CreateContextForMethod(string typeAndMethodName, SnippetCompiler snippet)
        {
            var nameParts = typeAndMethodName.Split('.');

            var identifierPairs = snippet.IsCSharp() ? GetCSharpNodes() : GetVbNodes();

            foreach (var (invocation, methodName) in identifierPairs)
            {
                var symbol = snippet.GetSymbol<IMethodSymbol>(invocation);
                if (symbol.Name == nameParts[1] &&
                    symbol.ContainingType.Name == nameParts[0])
                {
                    return new InvocationContext(invocation, methodName, snippet.SemanticModel);
                }
            }

            Assert.Fail($"Test setup error: could not find method call in test code snippet: {typeAndMethodName}");
            return null;

            IEnumerable<(SyntaxNode node, string name)> GetCSharpNodes() =>
                snippet.GetNodes<CSharpSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, n.Expression.GetIdentifier()?.Identifier.ValueText));

            IEnumerable<(SyntaxNode node, string name)> GetVbNodes() =>
                snippet.GetNodes<VBSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, VisualBasicSyntaxHelper.GetIdentifier(n.Expression)?.Identifier.ValueText));
        }

        private static void CheckExactMatchOnly_OverridesAreNotMatched(SnippetCompiler snippet)
        {
            // Testing for calls to Console.WriteLine
            var targetMethodSignature = new MemberDescriptor(KnownType.System_Console, "WriteLine");

            // 1. Should match Console.WriteLine
            var callToConsoleWriteLine = CreateContextForMethod("Console.WriteLine", snippet);
            CheckExactMethod(true, callToConsoleWriteLine, targetMethodSignature);

            // 2. Should not match call to xxx.WriteLine
            var callClass1WriteLine = CreateContextForMethod("Class1.WriteLine", snippet);
            CheckExactMethod(false, callClass1WriteLine, new MemberDescriptor(KnownType.System_Console, "Foo"),
                targetMethodSignature,
                new MemberDescriptor(KnownType.System_Data_DataSet, ".ctor"));

            // 3. Should match if Console.WriteLine is in the list of candidates
            CheckExactMethod(false, callClass1WriteLine, targetMethodSignature);
        }

        private static void CheckExactMatch_DoesNotMatchOverrides(SnippetCompiler snippet)
        {
            // XmlDocument derives from XmlNode
            var nodeWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckExactMethod(true, callToNodeWriteTo, nodeWriteTo);
            CheckExactMethod(false, callToNodeWriteTo, docWriteTo);

            // 2. Call to doc.WriteTo should only match for XmlDocument
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckExactMethod(false, callToDocWriteTo, nodeWriteTo);
            CheckExactMethod(true, callToDocWriteTo, docWriteTo);
        }

        private static void CheckMatchesAny_AndCheckingOverrides_DoesMatchOverrides(SnippetCompiler snippet)
        {
            // XmlDocument derives from XmlNode
            var nodeWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToNodeWriteTo, nodeWriteTo);
            CheckIsMethodOrDerived(false, callToNodeWriteTo, docWriteTo);

            // 2. Call to doc.WriteTo should match for XmlDocument and XmlNode
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToDocWriteTo, nodeWriteTo);
            CheckIsMethodOrDerived(true, callToDocWriteTo, docWriteTo);
        }

        private static void DoCheckMatch_InterfaceMethods(SnippetCompiler snippet)
        {
            var dispose = new MemberDescriptor(KnownType.System_IDisposable, "Dispose");
            var callToDispose = CreateContextForMethod("Class1.Dispose", snippet);

            // Exact match should not match, but matching "derived" methods should
            CheckExactMethod(false, callToDispose, dispose);
            CheckIsMethodOrDerived(true, callToDispose, dispose);
        }

        private static void CheckExactMethod(bool expectedOutcome, InvocationContext invocationContext, params MemberDescriptor[] targetMethodSignatures) =>
            CheckMatchesAny(false, expectedOutcome, invocationContext, targetMethodSignatures);

        private static void CheckIsMethodOrDerived(bool expectedOutcome, InvocationContext invocationContext, params MemberDescriptor[] targetMethodSignatures) =>
            CheckMatchesAny(true, expectedOutcome, invocationContext, targetMethodSignatures);

        private static void CheckMatchesAny(bool checkDerived, bool expectedOutcome, InvocationContext invocationContext, params MemberDescriptor[] targetMethodSignatures)
        {
            var result = MemberDescriptor.MatchesAny(invocationContext.MethodName,
                invocationContext.MethodSymbol, checkDerived, StringComparison.Ordinal, targetMethodSignatures);

            result.Should().Be(expectedOutcome);
        }
    }
}

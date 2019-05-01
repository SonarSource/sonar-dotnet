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

extern alias csharp;
extern alias vbnet;
using System.Collections.Generic;
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
    public class MethodSignatureHelperTest
    {
        [TestMethod]
        public void ExactMatchOnly_OverridesAreNotMatched_CS()
        {
            var code = @"
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
            var code = @"
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

        private void CheckExactMatchOnly_OverridesAreNotMatched(SnippetCompiler snippet)
        {
            // Testing for calls to Console.WriteLine
            var targetMethodSignature = new MemberDescriptor(KnownType.System_Console, "WriteLine");

            // 1. Should match Console.WriteLine
            var callToConsoleWriteLine = CreateContextForMethod("Console.WriteLine", snippet);
            CheckExactMethod(true, callToConsoleWriteLine, snippet, targetMethodSignature);

            // 2. Should not match call to xxx.WriteLine
            var callToDoStuffWriteLine = CreateContextForMethod("Class1.WriteLine", snippet);
            CheckExactMethod(false, callToDoStuffWriteLine, snippet,
                new MemberDescriptor(KnownType.System_Console, "Foo"),
                targetMethodSignature,
                new MemberDescriptor(KnownType.System_Data_DataSet, ".ctor"));

            // 3. Should match if Console.WriteLine is in the list of candidates
            CheckExactMethod(false, callToDoStuffWriteLine, snippet, targetMethodSignature);
        }

        [TestMethod]
        public void ExactMatch_DoesNotMatchOverrides_CS()
        {
            var code = @"
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
            var snippet = new SnippetCompiler(code, FrameworkMetadataReference.SystemXml);
            CheckExactMatch_DoesNotMatchOverrides(snippet);
        }

        [TestMethod]
        public void ExactMatch_DoesNotMatchOverrides_VB()
        {
            var code = @"
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
                FrameworkMetadataReference.SystemXml);
            CheckExactMatch_DoesNotMatchOverrides(snippet);
        }

        private static void CheckExactMatch_DoesNotMatchOverrides(SnippetCompiler snippet)
        {
            // XmlDocument derives from XmlNode
            var nodeWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckExactMethod(true, callToNodeWriteTo, snippet, nodeWriteTo);
            CheckExactMethod(false, callToNodeWriteTo, snippet, docWriteTo);

            // 2. Call to doc.WriteTo should only match for XmlDocument
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckExactMethod(false, callToDocWriteTo, snippet, nodeWriteTo);
            CheckExactMethod(true, callToDocWriteTo, snippet, docWriteTo);
        }

        [TestMethod]
        public void IsMatch_AndCheckingOverrides_DoesMatchOverrides_CS()
        {
            var code = @"
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
            var snippet = new SnippetCompiler(code, FrameworkMetadataReference.SystemXml);
            CheckIsMatch_AndCheckingOverrides_DoesMatchOverrides(snippet);
        }

        [TestMethod]
        public void IsMatch_AndCheckingOverrides_DoesMatchOverrides_VB()
        {
            var code = @"
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
            var snippet = new SnippetCompiler(code, false, AnalyzerLanguage.VisualBasic, FrameworkMetadataReference.SystemXml);
            CheckIsMatch_AndCheckingOverrides_DoesMatchOverrides(snippet);
        }

        private void CheckIsMatch_AndCheckingOverrides_DoesMatchOverrides(SnippetCompiler snippet)
        {
            // XmlDocument derives from XmlNode
            var nodeWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MemberDescriptor(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToNodeWriteTo, snippet, nodeWriteTo);
            CheckIsMethodOrDerived(false, callToNodeWriteTo, snippet, docWriteTo);

            // 2. Call to doc.WriteTo should match for XmlDocument and XmlNode
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToDocWriteTo, snippet, nodeWriteTo);
            CheckIsMethodOrDerived(true, callToDocWriteTo, snippet, docWriteTo);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_CS()
        {
            var code = @"
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
            var code = @"
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

        private void DoCheckMatch_InterfaceMethods(SnippetCompiler snippet)
        {
            var dispose = new MemberDescriptor(KnownType.System_IDisposable, "Dispose");
            var callToDispose = CreateContextForMethod("Class1.Dispose", snippet);

            // Exact match should not match, but matching "derived" methods should
            CheckExactMethod(false, callToDispose, snippet, dispose);
            CheckIsMethodOrDerived(true, callToDispose, snippet, dispose);
        }

        [TestMethod]
        public void CheckMatch_InterfaceMethods_NameMatchButNotOverride_CS()
        {
            var code = @"
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
            var code = @"
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
            var code = @"
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

            var result = MemberDescriptor.MatchesAny("Dispose", callToDispose.MethodSymbol, true, false, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("dispose", callToDispose.MethodSymbol, true, false, dispose);
            result.Should().Be(false);
            result = MemberDescriptor.MatchesAny("DISPOSE", callToDispose.MethodSymbol, true, false, dispose);
            result.Should().Be(false);

            result = MemberDescriptor.MatchesAny("Dispose", callToDispose.MethodSymbol, true, true, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("dispose", callToDispose.MethodSymbol, true, true, dispose);
            result.Should().Be(true);
            result = MemberDescriptor.MatchesAny("DISPOSE", callToDispose.MethodSymbol, true, true, dispose);
            result.Should().Be(true);
        }

        private static InvocationContext CreateContextForMethod(string typeAndMethodName, SnippetCompiler snippet)
        {
            var nameParts = typeAndMethodName.Split('.');

            IEnumerable<(SyntaxNode node, string name)> invocation_identifierPairs = null;
            if (snippet.IsCSharp())
            {
                invocation_identifierPairs = snippet.GetNodes<CSharpSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, n.Expression.GetIdentifier()?.Identifier.ValueText));
            }
            else
            {
                invocation_identifierPairs = snippet.GetNodes<VBSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, vbnet::SonarAnalyzer.Helpers.VisualBasic.VisualBasicSyntaxHelper.GetIdentifier(n.Expression)?.Identifier.ValueText));
            }

            foreach (var (invocation, methodName) in invocation_identifierPairs)
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
        }

        private static void CheckExactMethod(bool expectedOutcome, InvocationContext invocationContext,
            SnippetCompiler snippet, params MemberDescriptor[] targetMethodSignatures) =>
                CheckMatch(false, expectedOutcome, invocationContext, snippet, targetMethodSignatures);

        private static void CheckIsMethodOrDerived(bool expectedOutcome, InvocationContext invocationContext, SnippetCompiler snippet,
            params MemberDescriptor[] targetMethodSignatures) =>
            CheckMatch(true, expectedOutcome, invocationContext, snippet, targetMethodSignatures);

        private static void CheckMatch(bool checkDerived, bool expectedOutcome, InvocationContext invocationContext,
            SnippetCompiler snippet, params MemberDescriptor[] targetMethodSignatures)
        {
            var result = MemberDescriptor.MatchesAny(invocationContext.MethodName,
                invocationContext.MethodSymbol, checkDerived, false, targetMethodSignatures);

            result.Should().Be(expectedOutcome);
        }
    }
}

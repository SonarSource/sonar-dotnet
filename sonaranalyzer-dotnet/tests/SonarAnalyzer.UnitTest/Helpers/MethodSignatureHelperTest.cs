/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpHelpers = csharp::SonarAnalyzer.Helpers;
using CSharpSyntax = Microsoft.CodeAnalysis.CSharp.Syntax;
using VBHelpers = vbnet::SonarAnalyzer.Helpers;
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
            var targetMethodSignature = new MethodSignature(KnownType.System_Console, "WriteLine");

            // 1. Should match Console.WriteLine
            var callToConsoleWriteLine = CreateContextForMethod("Console.WriteLine", snippet);
            CheckExactMethod(true, callToConsoleWriteLine, snippet, targetMethodSignature);

            // 2. Should not match call to xxx.WriteLine
            var callToDoStuffWriteLine = CreateContextForMethod("Class1.WriteLine", snippet);
            CheckExactMethod(false, callToDoStuffWriteLine, snippet,
                new MethodSignature(KnownType.System_Console, "Foo"),
                targetMethodSignature,
                new MethodSignature(KnownType.System_Data_DataSet, ".ctor"));

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
            var nodeWriteTo = new MethodSignature(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MethodSignature(KnownType.System_Xml_XmlDocument, "WriteTo");

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
            var nodeWriteTo = new MethodSignature(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MethodSignature(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToNodeWriteTo, snippet, nodeWriteTo);
            CheckIsMethodOrDerived(false, callToNodeWriteTo, snippet, docWriteTo);

            // 2. Call to doc.WriteTo should match for XmlDocument and XmlNode
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckIsMethodOrDerived(true, callToDocWriteTo, snippet, nodeWriteTo);
            CheckIsMethodOrDerived(true, callToDocWriteTo, snippet, docWriteTo);
        }

        private static InvocationContext CreateContextForMethod(string typeAndMethodName, SnippetCompiler snippet)
        {
            var nameParts = typeAndMethodName.Split('.');

            IEnumerable<ValueTuple<SyntaxNode, SyntaxNode>> invocation_identifierPairs = null;
            if (snippet.IsCSharp())
            {
                invocation_identifierPairs = snippet.GetNodes<CSharpSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, (SyntaxNode)n.Expression.GetIdentifier()));
            }
            else
            {
                invocation_identifierPairs = snippet.GetNodes<VBSyntax.InvocationExpressionSyntax>()
                    .Select(n => ((SyntaxNode)n, (SyntaxNode)vbnet::SonarAnalyzer.Helpers.VisualBasic.VisualBasicSyntaxHelper.GetIdentifier(n.Expression)));                
            }

            foreach (var (invocation, identifier) in invocation_identifierPairs)
            {
                var symbol = snippet.GetSymbol<IMethodSymbol>(identifier);
                if (symbol.Name == nameParts[1] &&
                    symbol.ContainingType.Name == nameParts[0])
                {
                    return new InvocationContext(invocation, identifier, snippet.SemanticModel);
                }
            }

            Assert.Fail($"Test setup error: could not find method call in test code snippet: {typeAndMethodName}");
            return null;
        }
       
        private static void CheckExactMethod(bool expectedOutcome, InvocationContext invocationContext,
            SnippetCompiler snippet, params MethodSignature[] targetMethodSignatures) =>
                CheckMatch(false, expectedOutcome, invocationContext, snippet, targetMethodSignatures);

        private static void CheckIsMethodOrDerived(bool expectedOutcome, InvocationContext invocationContext, SnippetCompiler snippet,
            params MethodSignature[] targetMethodSignatures) =>
            CheckMatch(true, expectedOutcome, invocationContext, snippet, targetMethodSignatures);

        private static void CheckMatch(bool checkDerived, bool expectedOutcome, InvocationContext invocationContext,
            SnippetCompiler snippet, params MethodSignature[] targetMethodSignatures)
        {
            bool result;
            if (snippet.IsCSharp())
            {
                result = CSharpHelpers.MethodSignatureHelper.IsMatch(invocationContext.Identifier as CSharpSyntax.SimpleNameSyntax,
                    snippet.SemanticModel, invocationContext.InvokedMethodSymbol, checkDerived,
                    targetMethodSignatures);
            }
            else
            {
                result = VBHelpers.MethodSignatureHelper.IsMatch(invocationContext.Identifier as VBSyntax.SimpleNameSyntax,
                    snippet.SemanticModel, invocationContext.InvokedMethodSymbol, checkDerived,
                    targetMethodSignatures);
            }

            result.Should().Be(expectedOutcome);
        }
    }
}

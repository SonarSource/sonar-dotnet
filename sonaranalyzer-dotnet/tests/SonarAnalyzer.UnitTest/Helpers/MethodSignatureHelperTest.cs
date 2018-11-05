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
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharpHelpers = csharp::SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class GetMethodSignatureHelperTest
    {
        [TestMethod]
        public void ExactMatch()
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

            // Testing for calls to Console.WriteLine
            var targetMethodSignature = new MethodSignature(KnownType.System_Console, "WriteLine");

            // 1. Should match Console.WriteLine
            var callToConsoleWriteLine = CreateContextForMethod("Console.WriteLine", snippet);
            CheckExactMethodIsMatched(callToConsoleWriteLine, snippet, targetMethodSignature);

            // 2. Should not match call to xxx.WriteLine
            var callToDoStuffWriteLine = CreateContextForMethod("Class1.WriteLine", snippet);
            CheckExactMethodIsNotMatched(callToDoStuffWriteLine, snippet,

                new MethodSignature(KnownType.System_Console, "Foo"),
                targetMethodSignature,
                new MethodSignature(KnownType.System_Data_DataSet, ".ctor"));

            // 3. Should match if Console.WriteLine is in the list of candidates
            CheckExactMethodIsNotMatched(callToDoStuffWriteLine, snippet, targetMethodSignature);
        }

        [TestMethod]
        public void ExactMatch_DoesNotMatchOverrides()
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

            // XmlDocument derives from XmlNode
            var nodeWriteTo = new MethodSignature(KnownType.System_Xml_XmlNode, "WriteTo");
            var docWriteTo = new MethodSignature(KnownType.System_Xml_XmlDocument, "WriteTo");

            // 1. Call to node.WriteTo should only match for XmlNode
            var callToNodeWriteTo = CreateContextForMethod("XmlNode.WriteTo", snippet);
            CheckExactMethodIsMatched(callToNodeWriteTo, snippet, nodeWriteTo);
            CheckExactMethodIsNotMatched(callToNodeWriteTo, snippet, docWriteTo);

            // 2. Call to doc.WriteTo should only match for XmlDocument
            var callToDocWriteTo = CreateContextForMethod("XmlDocument.WriteTo", snippet);
            CheckExactMethodIsNotMatched(callToDocWriteTo, snippet, nodeWriteTo);
            CheckExactMethodIsMatched(callToDocWriteTo, snippet, docWriteTo);
        }


        private static InvocationContext CreateContextForMethod(string typeAndMethodName, SnippetCompiler snippet)
        {
            var nameParts = typeAndMethodName.Split('.');

            foreach (var invocation in snippet.GetNodes<InvocationExpressionSyntax>())
            {
                var identifier = invocation.Expression.GetIdentifier();
                var symbol = snippet.GetSymbol<IMethodSymbol>(identifier);
                if (symbol.Name == nameParts[1] &&
                    symbol.ContainingType.Name == nameParts[0])
                {
                    return new InvocationContext(invocation, identifier, snippet.SemanticModel);
                }
            }

            Assert.Fail($"Test setup error: could not find method call in test code snipper: {typeAndMethodName}");
            return null;
        }

        private static void CheckExactMethodIsMatched(InvocationContext invocationContext, SnippetCompiler snippet,
            params MethodSignature[] targetMethodSignatures)
        {
            CheckExactMethod(true, invocationContext, snippet, targetMethodSignatures);
        }

        private static void CheckExactMethodIsNotMatched(InvocationContext invocationContext, SnippetCompiler snippet,
            params MethodSignature[] targetMethodSignatures)
        {
            CheckExactMethod(false, invocationContext, snippet, targetMethodSignatures);
        }

        private static void CheckExactMethod(bool expectedOutcome, InvocationContext invocationContext,
            SnippetCompiler snippet, params MethodSignature[] targetMethodSignatures)
        {
            var result = CSharpHelpers.MethodSignatureHelper.IsExactMatch(invocationContext.Identifier as SimpleNameSyntax,
                    snippet.SemanticModel, invocationContext.InvokedMethodSymbol, targetMethodSignatures);

            result.Should().Be(expectedOutcome);
        }
    }
}

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using csharp::SonarAnalyzer.ControlFlowGraph.CSharp;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.UnitTest.ControlFlowGraph
{
    internal static class UcfgVerifier
    {
        public static UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code,
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc("3.0.20105.1"))
                .GetMethod(methodName);

            return BuildUcfg(method.Body, method, semanticModel.GetDeclaredSymbol(method), semanticModel);
        }

        public static UCFG GetUcfgForConstructor(string code, string ctorName)
        {
            var (syntaxTree, semanticModel) = TestHelper.Compile(code,
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc("3.0.20105.1"));

            var ctor = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == ctorName);

            return BuildUcfg(ctor.Body, ctor, semanticModel.GetDeclaredSymbol(ctor), semanticModel);
        }

        public static UCFG GetUcfgForPropertyGetter(string code, string propertyName)
        {
            (var property, var semanticModel) = TestHelper.Compile(code,
                additionalReferences: NuGetMetadataReference.MicrosoftAspNetMvc("3.0.20105.1"))
                .GetProperty(propertyName);

            var getterSymbol = semanticModel.GetDeclaredSymbol(property).GetMethod;
            var getterNode = (AccessorDeclarationSyntax)getterSymbol.DeclaringSyntaxReferences.First().GetSyntax();

            return BuildUcfg(getterNode.Body, getterNode, getterSymbol, semanticModel);
        }

        private static UCFG BuildUcfg(CSharpSyntaxNode cfgStartNode, CSharpSyntaxNode ucfgStartNode,
            IMethodSymbol methodSymbol, SemanticModel semanticModel)
        {
            var builder = new UcfgFactory(semanticModel);
            var cfg = CSharpControlFlowGraph.Create(cfgStartNode, semanticModel);
            var ucfg = builder.Create(ucfgStartNode, methodSymbol, cfg);

            //var serializedCfg = CfgSerializer.Serialize(methodName, cfg);
            //var serializedUcfg = UcfgSerializer.Serialize(ucfg);

            return ucfg;
        }

        public static UCFG VerifyInstructions(string codeSnippet, string methodName, bool isCtor = false)
        {
            var ucfg = isCtor ? GetUcfgForConstructor(codeSnippet, methodName) : GetUcfgForMethod(codeSnippet, methodName);
            VerifyInstructions(codeSnippet, ucfg);
            return ucfg;
        }

        public static UCFG VerifyInstructionsForPropertyGetter(string codeSnippet, string propertyName)
        {
            var ucfg = GetUcfgForPropertyGetter(codeSnippet, propertyName);
            VerifyInstructions(codeSnippet, ucfg);
            return ucfg;
        }

        public static IList<Instruction> GetEntryPointInstructions(UCFG ucfg)
        {
            return ucfg.BasicBlocks.SelectMany(
                b => b.Instructions.Where(i => i.Assigncall?.MethodId == UcfgBuiltInMethodId.EntryPoint)).ToList();
        }

        private static void VerifyInstructions(string codeSnippet, UCFG ucfg)
        {
            var expectedInstructions = UcfgInstructionCollector.Collect(codeSnippet).ToList();

            var actualInstructions = ucfg.BasicBlocks
                .SelectMany(b => b.Instructions)
                .Select(UcfgTestHelper.ToTestString)
                .ToList();

            Console.WriteLine("Expected instructions:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, expectedInstructions));
            Console.WriteLine();
            Console.WriteLine("Actual instructions:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, actualInstructions));

            // Fluent assertion bug: just comparing the collections gives an incorrect error message if the expected
            // list is empty but the actual list isn't ("Expected collection to be equal to {empty}, but found empty collection.")
            // To avoid this, do an assertion on the number of items first.
            actualInstructions.Count.Should().Be(expectedInstructions.Count);

            actualInstructions.Should().Equal(expectedInstructions);
        }

        internal static class UcfgInstructionCollector
        {
            private const string UCFG_NEW_OBJECT = @"// (?<instruction>.+? := new .+?)(\r\n|\n)";
            private const string UCFG_CALL = @"// (?<instruction>.+? := .+? \[ .*? \])";
            private const string UCFG_INSTRUCTION = UCFG_NEW_OBJECT + "|" + UCFG_CALL;

            public static IEnumerable<string> Collect(string codeSnippet)
            {
                var matches = Regex.Matches(codeSnippet, UCFG_INSTRUCTION);
                foreach (Match match in matches)
                {
                    yield return match.Groups["instruction"].Value;
                }
            }
        }
    }
}

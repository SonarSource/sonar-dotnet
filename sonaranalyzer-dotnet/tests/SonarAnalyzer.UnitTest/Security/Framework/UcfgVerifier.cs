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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.UnitTest.Security.Framework
{
    internal static class UcfgVerifier
    {
        public static UCFG GetUcfgForMethod(string code, string methodName)
        {
            (var method, var semanticModel) = TestHelper.Compile(code, Verifier.SystemWebMvcAssembly).GetMethod(methodName);

            var builder = new UcfgBuilder(semanticModel);

            var cfg = CSharpControlFlowGraph.Create(method.Body, semanticModel);
            var ucfg = builder.Build(method, semanticModel.GetDeclaredSymbol(method), cfg);

            //var serializedCfg = CfgSerializer.Serialize(methodName, cfg);
            //var serualizedUcfg = UcfgSerializer.Serialize(ucfg);

            return ucfg;
        }

        public static UCFG GetUcfgForConstructor(string code, string ctorName)
        {
            var (syntaxTree, semanticModel) = TestHelper.Compile(code, Verifier.SystemWebMvcAssembly);

            var ctor = syntaxTree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == ctorName);

            var builder = new UcfgBuilder(semanticModel);

            var cfg = CSharpControlFlowGraph.Create(ctor.Body, semanticModel);
            var ucfg = builder.Build(ctor, semanticModel.GetDeclaredSymbol(ctor), cfg);

            //var serializedCfg = CfgSerializer.Serialize(methodName, cfg);
            //var serualizedUcfg = UcfgSerializer.Serialize(ucfg);

            return ucfg;
        }

        public static void VerifyInstructions(string codeSnippet, string methodName, bool isCtor = false)
        {
            var ucfg = isCtor ? GetUcfgForConstructor(codeSnippet, methodName) : GetUcfgForMethod(codeSnippet, methodName);
            var expectedInstructions = UcfgInstructionCollector.Collect(codeSnippet).ToList();

            var actualInstructions = ucfg.BasicBlocks
                .SelectMany(b => b.Instructions)
                .Select(ConvertInstructionToInstructionComment)
                .ToList();

            actualInstructions.Should().Equal(expectedInstructions);
        }

        private static string ConvertInstructionToInstructionComment(Instruction instruction)
        {
            switch (instruction.InstrCase)
            {
                case Instruction.InstrOneofCase.Assigncall:
                    return $"{instruction.Assigncall.Variable} := {instruction.Assigncall.MethodId} " +
                        $"[ {string.Join(" ", instruction.Assigncall.Args.Select(ConvertExpressionToString))} ]";

                case Instruction.InstrOneofCase.NewObject:
                    return $"{instruction.NewObject.Variable} := new {instruction.NewObject.Type}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(instruction));
            }

            string ConvertExpressionToString(Expression expression)
            {
                if (expression.Var != null)
                {
                    return expression.Var.Name;
                }
                else if (expression.Const != null)
                {
                    return "const";
                }
                else if (expression.This != null)
                {
                    return "this";
                }
                else if (expression.Classname != null)
                {
                    return expression.Classname.Classname;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(expression));
                }
            }
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

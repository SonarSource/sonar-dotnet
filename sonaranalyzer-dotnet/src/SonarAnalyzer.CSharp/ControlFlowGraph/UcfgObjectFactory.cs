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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;
using UcfgLocation = SonarAnalyzer.Protobuf.Ucfg.Location;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    /// <summary>
    /// Low level UCFG object (e.g. protobuf generated classes) factory. The SyntaxNode is passed for location and
    /// expression mapping purposes. Hides the SyntaxNode/Expression mapping from the rest of the classes.
    /// </summary>
    public class UcfgObjectFactory
    {
        /// <summary>
        /// The string constant representation in the Sonar Security engine (Java part). When
        /// an instruction receives or returns a type that is not string we use this instead
        /// of a variable.
        /// </summary>
        private static readonly Expression ConstantExpression = new Expression
        {
            Const = new Constant { Value = "\"\"" }
        };

        private readonly Dictionary<SyntaxNode, Expression> nodeExpressionMap = new Dictionary<SyntaxNode, Expression>();

        private int tempVariablesCounter;

        public UCFG CreateUcfg(SyntaxNode syntaxNode, UcfgMethod method) =>
            new UCFG
            {
                Location = GetLocation(syntaxNode),
                MethodId = method.ToString(),
            };

        public BasicBlock CreateUcfgBlock(string id) =>
            new BasicBlock { Id = id };

        public Instruction CreateMethodCall(SyntaxNode syntaxNode, UcfgMethod method, params SyntaxNode[] arguments) =>
            CreateInstruction(syntaxNode, method, CreateTempVariable(), arguments);

        public Instruction CreateConcatenation(SyntaxNode syntaxNode, params SyntaxNode[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.Concatenation, CreateTempVariable(), arguments);

        public Instruction CreateAssignment(SyntaxNode syntaxNode, string variableName, SyntaxNode argument) =>
            CreateInstruction(syntaxNode, UcfgMethod.Assignment, variableName, argument);

        public Instruction CreateEntryPoint(SyntaxNode syntaxNode, params SyntaxNode[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.EntryPoint, CreateTempVariable(), arguments);

        public Instruction CreateParameterAnnotation(string parameterName, SyntaxNode attributeSyntax) =>
            CreateInstruction(attributeSyntax, UcfgMethod.Annotation, parameterName, attributeSyntax);

        public Instruction CreateVariable(SyntaxNode syntaxNode, string variableName)
        {
            nodeExpressionMap[syntaxNode] = CreateVariableExpression(variableName);
            return null;
        }

        public Instruction CreateConstant(SyntaxNode syntaxNode)
        {
            nodeExpressionMap[syntaxNode] = ConstantExpression;
            return null;
        }

        public Return CreateReturnExpression(SyntaxNode syntaxNode = null, SyntaxNode returnedValue = null) =>
            new Return
            {
                Location = syntaxNode == null
                    ? null
                    : GetLocation(syntaxNode),
                ReturnedExpression = returnedValue == null
                    ? ConstantExpression
                    : GetMappedExpression(returnedValue),
            };

        public Jump CreateJump(params string[] blockIds)
        {
            var jump = new Jump();
            jump.Destinations.AddRange(blockIds);
            return jump;
        }

        public bool IsVariable(SyntaxNode syntaxNode) =>
            GetMappedExpression(syntaxNode).Var != null;

        private Expression GetMappedExpression(SyntaxNode syntaxNode) =>
            nodeExpressionMap.GetValueOrDefault(syntaxNode.RemoveParentheses());

        private Instruction CreateInstruction(SyntaxNode syntaxNode, UcfgMethod method, string returnVariable,
            params SyntaxNode[] arguments)
        {
            var instruction = new Instruction
            {
                Location = GetLocation(syntaxNode),
                MethodId = method.ToString(),
                Variable = returnVariable,
            };
            instruction.Args.AddRange(arguments.Select(GetMappedExpression));
            CreateVariable(syntaxNode, instruction.Variable);

            return instruction;
        }

        private string CreateTempVariable() =>
            $"%{tempVariablesCounter++}";

        private static Expression CreateVariableExpression(string name) =>
            new Expression { Var = new Variable { Name = name } };

        /// <summary>
        /// Returns UCFG Location that represents the location of the provided SyntaxNode
        /// in SonarQube coordinates - 1-based line numbers and 0-based columns (line offsets).
        /// Roslyn coordinates are 0-based.
        /// </summary>
        private static UcfgLocation GetLocation(SyntaxNode syntaxNode)
        {
            var location = syntaxNode.GetLocation();
            var lineSpan = location.GetLineSpan();
            return new UcfgLocation
            {
                FileId = location.SourceTree.FilePath,
                StartLine = lineSpan.StartLinePosition.Line + 1,
                StartLineOffset = lineSpan.StartLinePosition.Character,
                EndLine = lineSpan.EndLinePosition.Line + 1,
                EndLineOffset = lineSpan.EndLinePosition.Character - 1,
            };
        }
    }
}

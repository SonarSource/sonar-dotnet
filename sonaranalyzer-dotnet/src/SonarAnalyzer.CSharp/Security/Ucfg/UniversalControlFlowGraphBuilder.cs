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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;
using UcfgLocation = SonarAnalyzer.Protobuf.Ucfg.Location;

namespace SonarAnalyzer.Security.Ucfg
{
    public class UniversalControlFlowGraphBuilder
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

        private readonly BlockIdMap blockId = new BlockIdMap();
        private readonly SemanticModel semanticModel;
        private readonly IControlFlowGraph cfg;

        public UniversalControlFlowGraphBuilder(SemanticModel semanticModel, IControlFlowGraph cfg)
        {
            this.semanticModel = semanticModel;
            this.cfg = cfg;
        }

        public UCFG Build(SyntaxNode syntaxNode, IMethodSymbol methodSymbol)
        {
            var ucfg = new UCFG
            {
                MethodId = GetMethodId(methodSymbol),
                Location = GetLocation(syntaxNode),
            };

            ucfg.BasicBlocks.AddRange(cfg.Blocks.Select(CreateBasicBlock));
            ucfg.Parameters.AddRange(methodSymbol.GetParameters().Select(p => p.Name));
            ucfg.Entries.Add(blockId.Get(cfg.EntryBlock));
            return ucfg;
        }

        private BasicBlock CreateBasicBlock(Block block)
        {
            var basicBlock = new BasicBlock
            {
                Id = blockId.Get(block),
            };

            var instructionBuilder = new InstructionBuilder(semanticModel, basicBlock);

            foreach (var instruction in block.Instructions)
            {
                instructionBuilder.BuildInstruction(instruction);
            }

            if (block is JumpBlock jump)
            {
                instructionBuilder.BuildInstruction(jump.JumpNode);
            }

            if (block is ExitBlock exit)
            {
                basicBlock.Ret = new Return { ReturnedExpression = ConstantExpression };
            }

            if (basicBlock.TerminatorCase == BasicBlock.TerminatorOneofCase.None)
            {
                // No return was created from JumpBlock or ExitBlock, wire up the successor blocks
                basicBlock.Jump = new Jump();
                basicBlock.Jump.Destinations.AddRange(block.SuccessorBlocks.Select(blockId.Get));
            }

            return basicBlock;
        }

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

        public static string GetMethodId(IMethodSymbol methodSymbol)
        {
            // Generic methods and methods declared in a base class
            var originalSymbol = methodSymbol?.ReducedFrom ?? methodSymbol?.OriginalDefinition;
            return originalSymbol?.ToDisplayString() ?? KnownMethodId.Unknown;
        }

        private class InstructionBuilder
        {
            private readonly Dictionary<SyntaxNode, Expression> nodeExpressionMap = new Dictionary<SyntaxNode, Expression>();
            private readonly SemanticModel semanticModel;
            private readonly BasicBlock basicBlock;

            private int tempVariablesCounter;

            public InstructionBuilder(SemanticModel semanticModel, BasicBlock basicBlock)
            {
                this.semanticModel = semanticModel;
                this.basicBlock = basicBlock;
            }

            public Expression BuildInstruction(SyntaxNode syntaxNode)
            {
                syntaxNode = syntaxNode.RemoveParentheses();

                Expression expression;
                if (nodeExpressionMap.TryGetValue(syntaxNode, out expression))
                {
                    return expression;
                }

                switch (syntaxNode.Kind())
                {
                    case SyntaxKind.AddExpression:
                        expression = BuildBinaryExpression((BinaryExpressionSyntax)syntaxNode);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression:
                        expression = BuildAssignment((AssignmentExpressionSyntax)syntaxNode);
                        break;

                    case SyntaxKind.InvocationExpression:
                        expression = BuildInvocation((InvocationExpressionSyntax)syntaxNode);
                        break;

                    case SyntaxKind.IdentifierName:
                        expression = BuildIdentifierName((IdentifierNameSyntax)syntaxNode);
                        break;

                    case SyntaxKind.VariableDeclarator:
                        BuildVariableDeclarator((VariableDeclaratorSyntax)syntaxNode);
                        expression = null;
                        break;

                    case SyntaxKind.ReturnStatement:
                        BuildReturn((ReturnStatementSyntax)syntaxNode);
                        expression = null;
                        break;

                    default:
                        // do nothing
                        expression = ConstantExpression;
                        break;
                }

                nodeExpressionMap.Add(syntaxNode, expression);
                return expression;
            }

            private Expression BuildIdentifierName(IdentifierNameSyntax identifier)
            {
                var identifierSymbol = GetSymbol(identifier);

                if (identifierSymbol is IPropertySymbol property)
                {
                    var instruction = CreateInstruction(
                        identifier,
                        methodId: GetMethodId(property.GetMethod),
                        variable: CreateTempVariable());
                    return CreateVariableExpression(instruction.Variable);
                }
                else if (IsLocalVarOrParameterOfTypeString(identifierSymbol))
                {
                    return CreateVariableExpression(identifierSymbol.Name);
                }
                else
                {
                    return ConstantExpression;
                }
            }

            private void BuildVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
            {
                if (variableDeclarator.Initializer == null)
                {
                    return;
                }

                var variable = semanticModel.GetDeclaredSymbol(variableDeclarator);
                if (IsLocalVarOrParameterOfTypeString(variable))
                {
                    CreateInstruction(
                        variableDeclarator,
                        methodId: KnownMethodId.Assignment,
                        variable: variable.Name,
                        arguments: BuildInstruction(variableDeclarator.Initializer.Value));
                }
            }

            private Expression BuildBinaryExpression(BinaryExpressionSyntax binaryExpression)
            {
                var instruction = CreateInstruction(
                    binaryExpression,
                    methodId: KnownMethodId.Concatenation,
                    variable: CreateTempVariable(),
                    arguments: new[] { BuildInstruction(binaryExpression.Right), BuildInstruction(binaryExpression.Left) });

                return CreateVariableExpression(instruction.Variable);
            }

            private void BuildReturn(ReturnStatementSyntax returnStatement)
            {
                basicBlock.Ret = new Return
                {
                    Location = GetLocation(returnStatement),
                    ReturnedExpression = returnStatement.Expression != null
                        ? BuildInstruction(returnStatement.Expression)
                        : ConstantExpression,
                };
            }

            private Expression BuildInvocation(InvocationExpressionSyntax invocation)
            {
                var methodSymbol = GetSymbol(invocation) as IMethodSymbol;
                if (methodSymbol == null)
                {
                    return ConstantExpression;
                }

                var arguments = BuildArguments(invocation, methodSymbol).ToArray();

                var instruction = CreateInstruction(
                    invocation,
                    methodId: GetMethodId(methodSymbol),
                    variable: CreateTempVariable(),
                    arguments: arguments);

                return methodSymbol.ReturnType.Is(KnownType.System_String)
                    ? CreateVariableExpression(instruction.Variable)
                    : ConstantExpression;
            }

            private IEnumerable<Expression> BuildArguments(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
            {
                if (IsInstanceMethodOnString(methodSymbol) ||
                    IsExtensionMethodCalledAsExtension(methodSymbol))
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        // add the string to the beginning of the arguments list
                        yield return BuildInstruction(memberAccess.Expression);
                    }
                }

                if (invocation.ArgumentList == null)
                {
                    yield break;
                }

                foreach (var argument in invocation.ArgumentList.Arguments)
                {
                    yield return BuildInstruction(argument.Expression);
                }
            }

            private Expression BuildAssignment(AssignmentExpressionSyntax assignment)
            {
                var left = GetSymbol(assignment.Left);

                if (IsLocalVarOrParameterOfTypeString(left))
                {
                    var instruction = CreateInstruction(
                        assignment,
                        methodId: KnownMethodId.Assignment,
                        variable: left.Name,
                        arguments: BuildInstruction(assignment.Right));
                    return CreateVariableExpression(instruction.Variable);
                }
                else if (left is IPropertySymbol property)
                {
                    var instruction = CreateInstruction(
                        assignment,
                        methodId: GetMethodId(property.SetMethod),
                        variable: CreateTempVariable(),
                        arguments: BuildInstruction(assignment.Right));
                    return CreateVariableExpression(instruction.Variable);
                }
                else
                {
                    return ConstantExpression;
                }
            }

            private static bool IsExtensionMethodCalledAsExtension(IMethodSymbol methodSymbol) =>
                methodSymbol.ReducedFrom != null;

            private static bool IsInstanceMethodOnString(IMethodSymbol methodSymbol) =>
                methodSymbol.ContainingType.Is(KnownType.System_String) && !methodSymbol.IsStatic;

            private static bool IsLocalVarOrParameterOfTypeString(ISymbol symbol) =>
                symbol is ILocalSymbol local && local.Type.Is(KnownType.System_String) ||
                symbol is IParameterSymbol parameter && parameter.Type.Is(KnownType.System_String);

            private Instruction CreateInstruction(SyntaxNode syntaxNode, string methodId, string variable, params Expression[] arguments)
            {
                var instruction = new Instruction
                {
                    Location = GetLocation(syntaxNode),
                    MethodId = methodId,
                    Variable = variable ?? ConstantExpression.Const.Value,
                };
                instruction.Args.AddRange(arguments);
                basicBlock.Instructions.Add(instruction);
                return instruction;
            }

            private string CreateTempVariable() =>
                $"%{tempVariablesCounter++}";

            private static Expression CreateVariableExpression(string name) =>
                new Expression { Var = new Variable { Name = name } };

            private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
                semanticModel.GetSymbolInfo(syntaxNode).Symbol;
        }

        private class BlockIdMap
        {
            private readonly Dictionary<Block, string> map = new Dictionary<Block, string>();
            private int counter;

            public string Get(Block cfgBlock) =>
                map.GetOrAdd(cfgBlock, b => $"{counter++}");
        }
    }
}

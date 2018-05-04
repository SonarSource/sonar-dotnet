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
        public static readonly string Const = "\"\""; // Same as Java implementation
        private readonly BlockIdMap blockId = new BlockIdMap();
        private readonly SemanticModel semanticModel;
        private readonly InstructionBuilder instructionBuilder;
        private readonly IControlFlowGraph cfg;

        public UniversalControlFlowGraphBuilder(SemanticModel semanticModel, IControlFlowGraph cfg)
        {
            this.semanticModel = semanticModel;
            instructionBuilder = new InstructionBuilder(semanticModel);
            this.cfg = cfg;
        }

        public IEnumerable<BasicBlock> Build() =>
            cfg.Blocks.Where(IsSupportedBlock).Select(CreateBasicBlock);

        public BasicBlock CreateBasicBlock(Block block)
        {
            var basicBlock = new BasicBlock
            {
                Id = blockId.Get(block),
            };

            foreach (var instruction in block.Instructions)
            {
                instructionBuilder.BuildInstruction(instruction, basicBlock);
            }

            if (block is JumpBlock jump)
            {
                instructionBuilder.BuildInstruction(jump.JumpNode, basicBlock);
            }
            else
            {
                if (block.SuccessorBlocks.All(cfg.ExitBlock.Equals))
                {
                    basicBlock.Ret = new Return { ReturnedExpression = new Expression { Const = new Constant { Value = Const } } };
                }
                else
                {
                    basicBlock.Jump = new Jump();
                    basicBlock.Jump.Destinations.AddRange(block.SuccessorBlocks.Select(blockId.Get));
                }
            }

            return basicBlock;
        }

        private static bool IsSupportedBlock(Block block) =>
            !(block is ExitBlock);

        public static UcfgLocation GetLocation(SyntaxNode syntaxNode)
        {
            var location = syntaxNode.GetLocation();
            var lineSpan = location.GetLineSpan();
            return new UcfgLocation
            {
                FileId = location.SourceTree.FilePath,
                StartLine = lineSpan.StartLinePosition.Line,
                StartLineOffset = lineSpan.StartLinePosition.Character,
                EndLine = lineSpan.EndLinePosition.Line,
                EndLineOffset = lineSpan.EndLinePosition.Character,
            };
        }

        private class InstructionBuilder
        {
            private readonly SemanticModel semanticModel;
            private int tempVariablesCounter;
            private Dictionary<SyntaxNode, Expression> nodeExpressionMap = new Dictionary<SyntaxNode, Expression>();

            public InstructionBuilder(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public Expression BuildInstruction(SyntaxNode syntaxNode, BasicBlock block)
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
                        expression = BuildBinaryExpression((BinaryExpressionSyntax)syntaxNode, block);
                        break;

                    case SyntaxKind.SimpleAssignmentExpression:
                        expression = BuildAssignment((AssignmentExpressionSyntax)syntaxNode, block);
                        break;

                    case SyntaxKind.InvocationExpression:
                        expression = BuildInvocation((InvocationExpressionSyntax)syntaxNode, block);
                        break;

                    case SyntaxKind.IdentifierName:
                        expression = BuildIdentifier((IdentifierNameSyntax)syntaxNode, block);
                        break;

                    case SyntaxKind.VariableDeclarator:
                        BuildVariableDeclarator((VariableDeclaratorSyntax)syntaxNode, block);
                        expression = null;
                        break;

                    case SyntaxKind.ReturnStatement:
                        BuildReturn((ReturnStatementSyntax)syntaxNode, block);
                        expression = null;
                        break;

                    default:
                        // do nothing
                        expression = CreateConstant();
                        break;
                }

                nodeExpressionMap.Add(syntaxNode, expression);
                return expression;
            }

            private Expression BuildIdentifier(IdentifierNameSyntax identifier, BasicBlock block)
            {
                var identifierSymbol = GetSymbol(identifier);
                if (identifierSymbol is IPropertySymbol property)
                {
                    var tempVariable = GetTempVariable();
                    block.Instructions.Add(new Instruction
                    {
                        MethodId = MethodIdProvider.Create(property.GetMethod),
                        Variable = tempVariable,
                    });
                    return CreateVariable(tempVariable);
                }
                else
                {
                    return CreateExpression(identifier);
                }
            }

            private void BuildVariableDeclarator(VariableDeclaratorSyntax variableDeclarator, BasicBlock block)
            {
                if (variableDeclarator.Initializer == null)
                {
                    return;
                }

                var left = semanticModel.GetDeclaredSymbol(variableDeclarator);
                if (IsLocalVarOrParameterOfTypeString(left))
                {
                    var instruction = new Instruction
                    {
                        Location = GetLocation(variableDeclarator),
                        Variable = left.Name,
                        MethodId = KnownMethodId.Assignment,
                        Args = { BuildInstruction(variableDeclarator.Initializer.Value, block) },
                    };
                    block.Instructions.Add(instruction);
                }
            }

            private Expression BuildBinaryExpression(BinaryExpressionSyntax binaryExpression, BasicBlock block)
            {
                var instruction = new Instruction
                {
                    Location = GetLocation(binaryExpression),
                    MethodId = KnownMethodId.Concatenation,
                    Args = { BuildInstruction(binaryExpression.Right, block), BuildInstruction(binaryExpression.Left, block) },
                    Variable = GetTempVariable(),
                };
                block.Instructions.Add(instruction);
                return CreateVariable(instruction.Variable);
            }

            public void BuildReturn(ReturnStatementSyntax returnStatement, BasicBlock block)
            {
                block.Ret = new Return
                {
                    Location = GetLocation(returnStatement),
                    ReturnedExpression = returnStatement.Expression != null ? BuildInstruction(returnStatement.Expression, block) : CreateConstant(),
                };
            }

            private Expression BuildInvocation(InvocationExpressionSyntax invocation, BasicBlock block)
            {
                var methodSymbol = GetSymbol(invocation) as IMethodSymbol;
                if (methodSymbol == null)
                {
                    return CreateConstant();
                }

                var instruction = new Instruction
                {
                    Location = GetLocation(invocation),
                    MethodId = MethodIdProvider.Create(methodSymbol),
                };

                if (invocation.ArgumentList != null)
                {
                    instruction.Args
                        .AddRange(invocation.ArgumentList.Arguments.Select(a => BuildInstruction(a.Expression, block)));
                }

                if (IsInstanceMethodOnString(methodSymbol) ||
                    IsExtensionMethodCalledAsExtension(methodSymbol))
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        // add the string to the beginning of the arguments list
                        instruction.Args.Insert(0, BuildInstruction(memberAccess.Expression, block));
                    }
                }

                block.Instructions.Add(instruction);

                if (methodSymbol.ReturnType.Is(KnownType.System_String))
                {
                    instruction.Variable = GetTempVariable();
                    return CreateVariable(instruction.Variable);
                }

                return CreateConstant();
            }

            private Expression BuildAssignment(AssignmentExpressionSyntax assignment, BasicBlock block)
            {
                var left = GetSymbol(assignment.Left);

                if (IsLocalVarOrParameterOfTypeString(left))
                {
                    block.Instructions.Add(new Instruction
                    {
                        Location = GetLocation(assignment),
                        Variable = left.Name,
                        MethodId = KnownMethodId.Assignment,
                        Args = { BuildInstruction(assignment.Right, block) },
                    });
                    return CreateVariable(left.Name);
                }
                else if (left is IPropertySymbol property)
                {
                    block.Instructions.Add(new Instruction
                    {
                        Location = GetLocation(assignment),
                        MethodId = MethodIdProvider.Create(property.SetMethod),
                        Args = { BuildInstruction(assignment.Right, block) },
                    });
                    return CreateConstant();
                }
                else
                {
                    return CreateConstant();
                }
            }

            private static bool IsExtensionMethodCalledAsExtension(IMethodSymbol methodSymbol) =>
                methodSymbol.ReducedFrom != null;

            private static bool IsInstanceMethodOnString(IMethodSymbol methodSymbol) =>
                methodSymbol.ContainingType.Is(KnownType.System_String) && !methodSymbol.IsStatic;

            private static bool IsLocalVarOrParameterOfTypeString(ISymbol symbol) =>
                symbol is ILocalSymbol local && local.Type.Is(KnownType.System_String) ||
                symbol is IParameterSymbol parameter && parameter.Type.Is(KnownType.System_String);

            private string GetTempVariable() =>
                $"%{tempVariablesCounter++}";

            private Expression CreateExpression(IdentifierNameSyntax identifierName)
            {
                var symbol = GetSymbol(identifierName);
                return IsLocalVarOrParameterOfTypeString(symbol) ? CreateVariable(symbol.Name) : CreateConstant();
            }

            public static Expression CreateVariable(string name) =>
                new Expression { Var = new Variable { Name = name } };

            public static Expression CreateConstant() =>
                new Expression { Const = new Constant { Value = Const } };

            private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
                semanticModel.GetSymbolInfo(syntaxNode).Symbol;
        }

        private class BlockIdMap
        {
            private readonly Dictionary<Block, string> map = new Dictionary<Block, string>();
            private int counter;

            public string Get(Block block) =>
                map.GetOrAdd(block, b => $"{counter++}");
        }
    }
}

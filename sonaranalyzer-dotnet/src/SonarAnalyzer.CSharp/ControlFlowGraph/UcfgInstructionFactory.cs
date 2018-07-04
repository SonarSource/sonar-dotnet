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
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    /// <summary>
    /// High level UCFG Instruction factory that controls UcfgObjectFactory to create objects
    /// depending on the provided SyntaxNodes.
    /// </summary>
    internal class UcfgInstructionFactory
    {
        private readonly SemanticModel semanticModel;
        private readonly UcfgExpressionService expressionService;

        private int tempVariablesCounter;

        public UcfgInstructionFactory(SemanticModel semanticModel, UcfgExpressionService expressionService)
        {
            this.semanticModel = semanticModel;
            this.expressionService = expressionService;
        }

        public IEnumerable<Instruction> Create(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case ObjectCreationExpressionSyntax objectCreation:
                    return CreateFromObjectCreationExpression(objectCreation);

                case IdentifierNameSyntax identifierName:
                    return CreateFromIdentifierName(identifierName);

                case VariableDeclaratorSyntax variableDeclarator:
                    return CreateFromVariableDeclarator(variableDeclarator);

                case BinaryExpressionSyntax binaryExpression:
                    return CreateFromBinaryExpression(binaryExpression);

                case InvocationExpressionSyntax invocationExpression:
                    return CreateFromInvocationExpression(invocationExpression);

                case AssignmentExpressionSyntax assignmentExpression:
                    return CreateFromAssignmentExpression(assignmentExpression);

                case BaseMethodDeclarationSyntax methodDeclaration:
                    return CreateFromBaseMethodDeclaration(methodDeclaration);

                case InstanceExpressionSyntax instanceExpression:
                    return CreateFromInstanceExpression(instanceExpression);

                default:
                    return CreateFromUnhandledSyntaxNode(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor,
            string parameterName) =>
            new[]
            {
                CreateMethodCallInstruction(attributeSyntax, attributeCtor),
                CreateMethodCallInstruction(attributeSyntax, UcfgIdentifier.Annotation, parameterName,
                    expressionService.Get(attributeSyntax)),
            };

        private IEnumerable<Instruction> CreateFromUnhandledSyntaxNode(SyntaxNode node) =>
            CreateConstant(node);

        private IEnumerable<Instruction> CreateFromObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var ctorSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (ctorSymbol == null)
            {
                return CreateConstant(objectCreationExpression);
            }

            // A call to a constructor should look like:
            // %X := new Ctor()
            // %X+1 := Ctor_MethodId [ %X params ]
            // variable := __id [ %X ]
            // As all instructions creation result in the SyntaxNode being associated with the return variable, we would end up
            // with variable := __id [ %X+1 ] (the objectCreationExpression node being now associated to %X+1).
            // To avoid this behavior, we associate the method call to the type of the objectCreationExpression
            return new[]
            {
                CreateNewObjectInstruction(objectCreationExpression, UcfgIdentifier.CreateTypeId(ctorSymbol.ContainingType),
                    CreateTempVariable()),
                CreateMethodCallInstruction(objectCreationExpression.Type, ctorSymbol, BuildArguments())
            };

            Expression[] BuildArguments()
            {
                // When building the args of the method call we need to pass the instance creation as first argument.
                var methodCallArgs = new List<Expression> { expressionService.Get(objectCreationExpression) };

                if (objectCreationExpression.ArgumentList != null)
                {
                    methodCallArgs.AddRange(
                        objectCreationExpression.ArgumentList.Arguments.Select(a => a.Expression).Select(expressionService.Get));
                }

                return methodCallArgs.ToArray();
            }
        }

        private IEnumerable<Instruction> CreateFromIdentifierName(IdentifierNameSyntax identifierName)
        {
            var identifierSymbol = GetSymbol(identifierName);

            if (identifierSymbol is IPropertySymbol property)
            {
                yield return CreateMethodCallInstruction(identifierName, property.GetMethod, BuildPropertyGetterArguments().ToArray());
            }
            else if (IsLocalVarOrParameter(identifierSymbol))
            {
                CreateVariable(identifierName, identifierName.Identifier.Text);
            }
            else
            {
                CreateConstant(identifierName);
            }

            IEnumerable<Expression> BuildPropertyGetterArguments()
            {
                if (property.IsStatic)
                {
                    yield return UcfgExpression.FromNamedType(property.ContainingType);
                }
                else
                {
                    yield return UcfgExpression.This;
                }
            }
        }

        private IEnumerable<Instruction> CreateFromVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer == null)
            {
                return Enumerable.Empty<Instruction>();
            }

            var variable = semanticModel.GetDeclaredSymbol(variableDeclarator);
            if (IsLocalVarOrParameter(variable))
            {
                return new[] { CreateAssignment(variableDeclarator, variable.Name,
                    expressionService.Get(variableDeclarator.Initializer.Value)) };
            }

            return Enumerable.Empty<Instruction>();
        }

        private IEnumerable<Instruction> CreateFromBinaryExpression(BinaryExpressionSyntax binaryExpression) =>
             new[]
             {
                 CreateMethodCallInstruction(binaryExpression, UcfgIdentifier.Concatenation, CreateTempVariable(),
                    expressionService.Get(binaryExpression.Right),
                    expressionService.Get(binaryExpression.Left))
             };

        private IEnumerable<Instruction> CreateFromInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodSymbol = GetSymbol(invocationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return CreateConstant(invocationExpression);
            }

            return new[] { CreateMethodCallInstruction(invocationExpression, methodSymbol, BuildArguments().ToArray()) };

            IEnumerable<Expression> BuildArguments()
            {
                if (methodSymbol.IsStatic ||
                    methodSymbol.ReducedFrom != null)
                {
                    yield return UcfgExpression.FromNamedType(methodSymbol.ContainingType);
                }

                if (!methodSymbol.IsStatic)
                {
                    if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        yield return expressionService.Get(memberAccess.Expression);
                    }
                    else
                    {
                        yield return UcfgExpression.This;
                    }
                }

                if (invocationExpression.ArgumentList == null)
                {
                    yield break;
                }

                foreach (var argument in invocationExpression.ArgumentList.Arguments)
                {
                    yield return expressionService.Get(argument.Expression);
                }
            }
        }

        private IEnumerable<Instruction> CreateFromAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var left = GetSymbol(assignmentExpression.Left);

            if (IsLocalVarOrParameter(left))
            {
                return new[] { CreateAssignment(assignmentExpression, left.Name,
                    expressionService.Get(assignmentExpression.Right)) };
            }
            else if (left is IPropertySymbol property && property.SetMethod != null)
            {
                return new[] { CreateMethodCallInstruction(assignmentExpression, property.SetMethod,
                    BuildPropertySetterArguments(property).ToArray()) };
            }
            else
            {
                return CreateConstant(assignmentExpression);
            }

            IEnumerable<Expression> BuildPropertySetterArguments(IPropertySymbol property)
            {
                if (property.IsStatic)
                {
                    yield return UcfgExpression.FromNamedType(property.ContainingType);
                }
                else if (assignmentExpression.Parent is InitializerExpressionSyntax initializerExpression)
                {
                    // We should return the variable associated to the ObjectCreation
                    yield return expressionService.Get(initializerExpression.Parent);
                }
                else if (assignmentExpression.Left is MemberAccessExpressionSyntax memberAccess)
                {
                    yield return expressionService.Get(memberAccess.Expression);
                }
                else
                {
                    yield return UcfgExpression.This;
                }

                yield return expressionService.Get(assignmentExpression.Right);
            }
        }

        private IEnumerable<Instruction> CreateFromBaseMethodDeclaration(BaseMethodDeclarationSyntax methodDeclaration)
        {
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                CreateVariable(parameter, parameter.Identifier.ValueText);
            }

            return new[] { CreateMethodCallInstruction(methodDeclaration, UcfgIdentifier.EntryPoint, CreateTempVariable(),
                methodDeclaration.ParameterList.Parameters.Select(expressionService.Get).ToArray()) };
        }

        private IEnumerable<Instruction> CreateFromInstanceExpression(InstanceExpressionSyntax instanceExpression)
        {
            expressionService.RegisterAsThis(instanceExpression);
            return Enumerable.Empty<Instruction>();
        }

        public IEnumerable<Instruction> CreateVariable(SyntaxNode syntaxNode, string variableName)
        {
            expressionService.RegisterAsVariable(syntaxNode, variableName);
            return Enumerable.Empty<Instruction>();
        }

        public IEnumerable<Instruction> CreateConstant(SyntaxNode syntaxNode)
        {
            expressionService.RegisterAsConstant(syntaxNode);
            return Enumerable.Empty<Instruction>();
        }

        public Instruction CreateAssignment(SyntaxNode syntaxNode, string variableName, Expression argument) =>
            CreateMethodCallInstruction(syntaxNode, UcfgIdentifier.Assignment, variableName, argument);

        private Instruction CreateMethodCallInstruction(SyntaxNode invocation, IMethodSymbol methodSymbol,
            params Expression[] arguments) =>
            CreateMethodCallInstruction(invocation, UcfgIdentifier.CreateMethodId(methodSymbol), CreateTempVariable(), arguments);

        private Instruction CreateMethodCallInstruction(SyntaxNode syntaxNode, UcfgIdentifier method, string returnVariable,
            params Expression[] arguments)
        {
            Debug.Assert(!(syntaxNode is ObjectCreationExpressionSyntax),
                "This method should not be called for nodes of type 'ObjectCreationExpressionSyntax'.");

            // Associate node to the UCFG variable
            CreateVariable(syntaxNode, returnVariable);

            // Create the instruction
            var methodCall = new AssignCall
            {
                Location = syntaxNode.GetUcfgLocation(),
                MethodId = method.ToString(),
                Variable = returnVariable,
            };
            methodCall.Args.AddRange(arguments);
            return new Instruction { Assigncall = methodCall };
        }

        private Instruction CreateNewObjectInstruction(SyntaxNode syntaxNode, UcfgIdentifier method, string returnVariable)
        {
            Debug.Assert(syntaxNode is ObjectCreationExpressionSyntax,
                "This method should be called only for nodes of type 'ObjectCreationExpressionSyntax'.");

            // Associate node to the UCFG variable
            CreateVariable(syntaxNode, returnVariable);

            // Create the instruction
            return new Instruction
            {
                NewObject = new NewObject
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    Type = method.ToString(),
                    Variable = returnVariable,
                }
            };
        }

        private string CreateTempVariable() =>
            $"%{tempVariablesCounter++}";

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private static bool IsLocalVarOrParameter(ISymbol symbol) =>
            symbol is ILocalSymbol local ||
            symbol is IParameterSymbol parameter;
    }
}

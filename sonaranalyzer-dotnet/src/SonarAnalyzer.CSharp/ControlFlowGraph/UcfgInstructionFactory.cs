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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Protobuf.Ucfg;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    /// <summary>
    /// High level UCFG Instruction factory that controls UcfgObjectFactory to create objects
    /// depending on the provided SyntaxNodes.
    /// </summary>
    public class UcfgInstructionFactory
    {
        private readonly SemanticModel semanticModel;
        private readonly UcfgObjectFactory objectFactory;

        public UcfgInstructionFactory(SemanticModel semanticModel, UcfgObjectFactory objectFactory)
        {
            this.semanticModel = semanticModel;
            this.objectFactory = objectFactory;
        }

        public Instruction Create(SyntaxNode syntaxNode)
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
                    return CreateEntryPointInstruction(methodDeclaration);

                default:
                    return DefaultCreate(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor, string parameterName) =>
            new[]
            {
                objectFactory.CreateMethodCall(attributeSyntax, UcfgMethod.Create(attributeCtor)),
                objectFactory.CreateParameterAnnotation(parameterName, attributeSyntax),
            };

        private Instruction DefaultCreate(SyntaxNode node) =>
            objectFactory.CreateConstant(node);

        private Instruction CreateFromObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var ctorSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (ctorSymbol == null)
            {
                return objectFactory.CreateConstant(objectCreationExpression);
            }

            var arguments = BuildArguments(objectCreationExpression, ctorSymbol).ToArray();

            return AddMethodCall(objectCreationExpression, ctorSymbol, arguments);
        }

        private Instruction CreateFromIdentifierName(IdentifierNameSyntax identifierName)
        {
            var identifierSymbol = GetSymbol(identifierName);

            if (identifierSymbol is IPropertySymbol property)
            {
                return AddMethodCall(identifierName, property.GetMethod);
            }
            else if (IsLocalVarOrParameterOfTypeString(identifierSymbol))
            {
                return objectFactory.CreateVariable(identifierName, identifierName.Identifier.Text);
            }
            else
            {
                return objectFactory.CreateConstant(identifierName);
            }
        }

        private Instruction CreateFromVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer != null)
            {
                var variable = semanticModel.GetDeclaredSymbol(variableDeclarator);
                if (IsLocalVarOrParameterOfTypeString(variable))
                {
                    return objectFactory.CreateAssignment(
                        variableDeclarator,
                        variable.Name,
                        variableDeclarator.Initializer.Value);
                }
            }

            return null;
        }

        private Instruction CreateFromBinaryExpression(BinaryExpressionSyntax binaryExpression) =>
            objectFactory.CreateConcatenation(binaryExpression, binaryExpression.Right, binaryExpression.Left);

        private Instruction CreateFromInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodSymbol = GetSymbol(invocationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return objectFactory.CreateConstant(invocationExpression);
            }

            var arguments = BuildArguments(invocationExpression, methodSymbol).ToArray();

            return AddMethodCall(invocationExpression, methodSymbol, arguments);
        }

        private Instruction CreateFromAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var left = GetSymbol(assignmentExpression.Left);

            if (IsLocalVarOrParameterOfTypeString(left))
            {
                return objectFactory.CreateAssignment(assignmentExpression, left.Name, assignmentExpression.Right);
            }
            else if (left is IPropertySymbol property &&
                property.SetMethod != null)
            {
                return AddMethodCall(assignmentExpression, property.SetMethod, assignmentExpression.Right);
            }
            else
            {
                return objectFactory.CreateConstant(assignmentExpression);
            }
        }

        private Instruction CreateEntryPointInstruction(BaseMethodDeclarationSyntax methodDeclaration)
        {
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                objectFactory.CreateVariable(parameter, parameter.Identifier.ValueText);
            }

            return objectFactory.CreateEntryPoint(methodDeclaration, methodDeclaration.ParameterList.Parameters.ToArray());
        }

        private IEnumerable<SyntaxNode> BuildArguments(ObjectCreationExpressionSyntax objectCreation, IMethodSymbol methodSymbol) =>
            objectCreation.ArgumentList == null
                ? Enumerable.Empty<SyntaxNode>()
                : objectCreation.ArgumentList.Arguments.Select(a => a.Expression);

        private IEnumerable<SyntaxNode> BuildArguments(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        {
            if (IsInstanceMethodOnString(methodSymbol) ||
                IsExtensionMethodCalledAsExtension(methodSymbol))
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    // add the string to the beginning of the arguments list
                    yield return memberAccess.Expression;
                }
            }

            if (invocation.ArgumentList == null)
            {
                yield break;
            }

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                yield return argument.Expression;
            }
        }

        private static bool IsExtensionMethodCalledAsExtension(IMethodSymbol methodSymbol) =>
            methodSymbol.ReducedFrom != null;

        private static bool IsInstanceMethodOnString(IMethodSymbol methodSymbol) =>
            methodSymbol.ContainingType.Is(KnownType.System_String) && !methodSymbol.IsStatic;

        private static bool IsLocalVarOrParameterOfTypeString(ISymbol symbol) =>
            symbol is ILocalSymbol local && local.Type.Is(KnownType.System_String) ||
            symbol is IParameterSymbol parameter && parameter.Type.Is(KnownType.System_String);

        private Instruction AddMethodCall(SyntaxNode invocation, IMethodSymbol methodSymbol, params SyntaxNode[] arguments)
        {
            // Add instruction to UCFG only when the method accepts/returns string,
            // or when at least one of its arguments is known to be a string.
            // Since we generate Const expressions for everything that is not
            // a string, checking if the arguments are Var expressions should
            // be enough to ensure they are strings.
            if (!AcceptsOrReturnsString(methodSymbol) &&
                !arguments.Any(objectFactory.IsVariable))
            {
                return objectFactory.CreateConstant(invocation);
            }

            var instruction = objectFactory.CreateMethodCall(invocation, UcfgMethod.Create(methodSymbol), arguments);

            if (!ReturnsString(methodSymbol))
            {
                // Overwrite the created varible with a constant, in case the method
                // is called as nested invocation in another invocation: Foo(Bar())
                objectFactory.CreateConstant(invocation);
            }

            return instruction;
        }

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private static bool ReturnsString(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.Is(KnownType.System_String);

        private static bool AcceptsOrReturnsString(IMethodSymbol methodSymbol) =>
            ReturnsString(methodSymbol) ||
            methodSymbol.Parameters.Any(p => p.Type.Is(KnownType.System_String));
    }
}

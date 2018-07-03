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

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    /// <summary>
    /// High level UCFG Instruction factory that controls UcfgObjectFactory to create objects
    /// depending on the provided SyntaxNodes.
    /// </summary>
    public class UcfgInstructionBuilder
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

        private readonly SemanticModel semanticModel;
        private readonly Dictionary<SyntaxNode, Expression> syntaxNodeToUcfgExpressionCache
            = new Dictionary<SyntaxNode, Expression>();

        private int tempVariablesCounter;

        public UcfgInstructionBuilder(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
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
                    return CreateDefaultInstruction(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor, string parameterName) =>
            new[]
            {
                CreateMethodCall(attributeSyntax, UcfgMethod.Create(attributeCtor)),
                CreateParameterAnnotation(parameterName, attributeSyntax),
            };

        private Instruction CreateDefaultInstruction(SyntaxNode node) =>
            CreateConstant(node);

        private Instruction CreateFromObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var ctorSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (ctorSymbol == null)
            {
                return CreateConstant(objectCreationExpression);
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
                return CreateVariable(identifierName, identifierName.Identifier.Text);
            }
            else
            {
                return CreateConstant(identifierName);
            }
        }

        private Instruction CreateFromVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer != null)
            {
                var variable = semanticModel.GetDeclaredSymbol(variableDeclarator);
                if (IsLocalVarOrParameterOfTypeString(variable))
                {
                    return CreateAssignment(
                        variableDeclarator,
                        variable.Name,
                        variableDeclarator.Initializer.Value);
                }
            }

            return null;
        }

        private Instruction CreateFromBinaryExpression(BinaryExpressionSyntax binaryExpression) =>
            CreateConcatenation(binaryExpression, binaryExpression.Right, binaryExpression.Left);

        private Instruction CreateFromInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodSymbol = GetSymbol(invocationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return CreateConstant(invocationExpression);
            }

            var arguments = BuildArguments(invocationExpression, methodSymbol).ToArray();

            return AddMethodCall(invocationExpression, methodSymbol, arguments);
        }

        private Instruction CreateFromAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var left = GetSymbol(assignmentExpression.Left);

            if (IsLocalVarOrParameterOfTypeString(left))
            {
                return CreateAssignment(assignmentExpression, left.Name, assignmentExpression.Right);
            }
            else if (left is IPropertySymbol property &&
                property.SetMethod != null)
            {
                return AddMethodCall(assignmentExpression, property.SetMethod, assignmentExpression.Right);
            }
            else
            {
                return CreateConstant(assignmentExpression);
            }
        }

        private Instruction CreateEntryPointInstruction(BaseMethodDeclarationSyntax methodDeclaration)
        {
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                CreateVariable(parameter, parameter.Identifier.ValueText);
            }

            return CreateEntryPoint(methodDeclaration, methodDeclaration.ParameterList.Parameters.ToArray());
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

        private Instruction AddMethodCall(SyntaxNode invocation, IMethodSymbol methodSymbol, params SyntaxNode[] arguments)
        {
            // Add instruction to UCFG only when the method accepts/returns string,
            // or when at least one of its arguments is known to be a string.
            // Since we generate Const expressions for everything that is not
            // a string, checking if the arguments are Var expressions should
            // be enough to ensure they are strings.
            if (!AcceptsOrReturnsString(methodSymbol) &&
                !arguments.Any(IsVariable))
            {
                return CreateConstant(invocation);
            }

            var instruction = CreateMethodCall(invocation, UcfgMethod.Create(methodSymbol), arguments);

            if (!ReturnsString(methodSymbol))
            {
                // Overwrite the created varible with a constant, in case the method
                // is called as nested invocation in another invocation: Foo(Bar())
                CreateConstant(invocation);
            }

            return instruction;
        }

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
            syntaxNodeToUcfgExpressionCache[syntaxNode] = CreateVariableExpression(variableName);
            return null;
        }

        public Instruction CreateConstant(SyntaxNode syntaxNode)
        {
            syntaxNodeToUcfgExpressionCache[syntaxNode] = ConstantExpression;
            return null;
        }

        public Return CreateReturnExpression(SyntaxNode syntaxNode = null, SyntaxNode returnedValue = null) =>
            new Return
            {
                Location = syntaxNode == null
                    ? null
                    : syntaxNode.GetUcfgLocation(),
                ReturnedExpression = returnedValue == null
                    ? ConstantExpression
                    : GetMappedExpression(returnedValue),
            };

        public bool IsVariable(SyntaxNode syntaxNode) =>
            GetMappedExpression(syntaxNode).Var != null;

        private Expression GetMappedExpression(SyntaxNode syntaxNode) =>
            syntaxNodeToUcfgExpressionCache.GetValueOrDefault(syntaxNode.RemoveParentheses())
            // In some cases the CFG does not contain all syntax nodes that were used in
            // an expression, for example when ternary operator is passed as an argument.
            // This could potentially be improved, but for the time being the constant
            // expression fallback will do what we used to do before.
            ?? ConstantExpression;

        private Instruction CreateInstruction(SyntaxNode syntaxNode, UcfgMethod method, string returnVariable,
            params SyntaxNode[] arguments)
        {
            var methodCall = new AssignCall
            {
                Location = syntaxNode.GetUcfgLocation(),
                MethodId = method.ToString(),
                Variable = returnVariable,
            };
            methodCall.Args.AddRange(arguments.Select(GetMappedExpression));
            CreateVariable(syntaxNode, methodCall.Variable);

            return new Instruction { Assigncall = methodCall };
        }

        private string CreateTempVariable() =>
            $"%{tempVariablesCounter++}";

        private static Expression CreateVariableExpression(string name) =>
            new Expression { Var = new Variable { Name = name } };

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private static bool ReturnsString(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.Is(KnownType.System_String);

        private static bool AcceptsOrReturnsString(IMethodSymbol methodSymbol) =>
            ReturnsString(methodSymbol) ||
            methodSymbol.Parameters.Any(p => p.Type.Is(KnownType.System_String));

        private static bool IsExtensionMethodCalledAsExtension(IMethodSymbol methodSymbol) =>
            methodSymbol.ReducedFrom != null;

        private static bool IsInstanceMethodOnString(IMethodSymbol methodSymbol) =>
            methodSymbol.ContainingType.Is(KnownType.System_String) && !methodSymbol.IsStatic;

        private static bool IsLocalVarOrParameterOfTypeString(ISymbol symbol) =>
            symbol is ILocalSymbol local && local.Type.Is(KnownType.System_String) ||
            symbol is IParameterSymbol parameter && parameter.Type.Is(KnownType.System_String);
    }
}

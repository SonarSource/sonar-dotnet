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
                    return CreateEntryPointInstruction(methodDeclaration);

                default:
                    return CreateDefault(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax,
            IMethodSymbol attributeCtor, string parameterName) =>
                new[]
                {
                    CreateMethodCall(attributeSyntax, UcfgMethod.Create(attributeCtor)),
                    CreateParameterAnnotation(parameterName, attributeSyntax)
                };

        public Return CreateReturnExpression(SyntaxNode syntaxNode = null, SyntaxNode returnedValue = null) =>
            new Return
            {
                Location = syntaxNode.GetUcfgLocation(),
                ReturnedExpression = returnedValue == null
                    ? ConstantExpression
                    : GetMappedExpression(returnedValue),
            };

        private IEnumerable<Instruction> CreateDefault(SyntaxNode syntaxNode) =>
            new[] { CreateConstant(syntaxNode) };

        private Instruction CreateMethodCall(SyntaxNode syntaxNode, UcfgMethod method, params Expression[] arguments) =>
            CreateInstruction(syntaxNode, method, CreateTempVariable(), arguments);

        private Instruction CreateConcatenation(SyntaxNode syntaxNode, params Expression[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.Concatenation, CreateTempVariable(), arguments);

        private Instruction CreateAssignment(SyntaxNode syntaxNode, string variableName, Expression argument) =>
            CreateInstruction(syntaxNode, UcfgMethod.Assignment, variableName, argument);

        private Instruction CreateEntryPoint(SyntaxNode syntaxNode, params Expression[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.EntryPoint, CreateTempVariable(), arguments);

        private Instruction CreateParameterAnnotation(string parameterName, SyntaxNode attributeSyntax) =>
            CreateInstruction(attributeSyntax, UcfgMethod.Annotation, parameterName, GetMappedExpression(attributeSyntax));

        private Instruction CreateVariable(SyntaxNode syntaxNode, string variableName)
        {
            syntaxNodeToUcfgExpressionCache[syntaxNode] = CreateVariableExpression(variableName);
            return null;
        }

        private Instruction CreateConstant(SyntaxNode syntaxNode)
        {
            syntaxNodeToUcfgExpressionCache[syntaxNode] = ConstantExpression;
            return null;
        }

        private static Expression CreateVariableExpression(string name) =>
            new Expression { Var = new Variable { Name = name } };

        private Instruction CreateInstruction(SyntaxNode syntaxNode, UcfgMethod method, string returnVariable,
            params Expression[] arguments)
        {
            var methodCall = new AssignCall
            {
                Location = syntaxNode.GetUcfgLocation(),
                MethodId = method.ToString(),
                Variable = returnVariable,
            };
            methodCall.Args.AddRange(arguments);
            CreateVariable(syntaxNode, methodCall.Variable);

            return new Instruction { Assigncall = methodCall };
        }

        private string CreateTempVariable() =>
            $"%{tempVariablesCounter++}";

        private IEnumerable<Instruction> CreateFromObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var ctorSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (ctorSymbol == null)
            {
                return new[] { CreateConstant(objectCreationExpression) };
            }

            // A call to a constructor should look like:
            // %X := new Ctor()
            // %X+1 := Ctor_MethodId [ params ]
            // variable := __id [ %X ]
            // Because all UCFG instructions associate the processed SyntaxNode to the variable created, we need to:
            // 1. create the variable linked to the new
            // 2. create the method call
            // 3. create the new call (this will override to which variable the node is associated)
            // 4. return the instructions in the right order
            var initializationVariable = CreateTempVariable();
            var methodCallInstruction = AddMethodCall(objectCreationExpression, ctorSymbol, BuildArguments(objectCreationExpression, ctorSymbol).ToArray());
            var newInstruction = CreateInstruction(objectCreationExpression, UcfgMethod.Assignment, initializationVariable, ConstantExpression);

            return new[] { newInstruction, methodCallInstruction };
        }

        private IEnumerable<Instruction> CreateFromIdentifierName(IdentifierNameSyntax identifierName)
        {
            var identifierSymbol = GetSymbol(identifierName);

            if (identifierSymbol is IPropertySymbol property)
            {
                yield return AddMethodCall(identifierName, property.GetMethod);
            }
            else if (IsLocalVarOrParameter(identifierSymbol))
            {
                yield return CreateVariable(identifierName, identifierName.Identifier.Text);
            }
            else
            {
                yield return CreateConstant(identifierName);
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
                return new[] { CreateAssignment(variableDeclarator, variable.Name, GetMappedExpression((variableDeclarator.Initializer.Value))) };
            }

            return Enumerable.Empty<Instruction>();
        }

        private IEnumerable<Instruction> CreateFromBinaryExpression(BinaryExpressionSyntax binaryExpression) =>
            new[] { CreateConcatenation(binaryExpression, GetMappedExpression(binaryExpression.Right), GetMappedExpression((binaryExpression.Left))) };

        private IEnumerable<Instruction> CreateFromInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodSymbol = GetSymbol(invocationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return new[] { CreateConstant(invocationExpression) };
            }

            var arguments = BuildArguments(invocationExpression, methodSymbol).ToArray();
            return new[] { AddMethodCall(invocationExpression, methodSymbol, arguments) };
        }

        private IEnumerable<Instruction> CreateFromAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var left = GetSymbol(assignmentExpression.Left);

            if (IsLocalVarOrParameter(left))
            {
                return new[] { CreateAssignment(assignmentExpression, left.Name, GetMappedExpression(assignmentExpression.Right)) };
            }
            else if (left is IPropertySymbol property && property.SetMethod != null)
            {
                return new[] { AddMethodCall(assignmentExpression, property.SetMethod, GetMappedExpression(assignmentExpression.Right)) };
            }
            else
            {
                return new[] { CreateConstant(assignmentExpression) };
            }
        }

        private IEnumerable<Instruction> CreateEntryPointInstruction(BaseMethodDeclarationSyntax methodDeclaration)
        {
            foreach (var parameter in methodDeclaration.ParameterList.Parameters)
            {
                CreateVariable(parameter, parameter.Identifier.ValueText);
            }

            return new[] { CreateEntryPoint(methodDeclaration, methodDeclaration.ParameterList.Parameters.Select(GetMappedExpression).ToArray()) };
        }

        private IEnumerable<Expression> BuildArguments(ObjectCreationExpressionSyntax objectCreation, IMethodSymbol methodSymbol)
        {
            return objectCreation.ArgumentList == null
                ? Enumerable.Empty<Expression>()
                : objectCreation.ArgumentList.Arguments.Select(a => a.Expression).Select(GetMappedExpression);
        }

        private IEnumerable<Expression> BuildArguments(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.IsStatic ||
                methodSymbol.ReducedFrom != null)
            {
                yield return CreateVariableExpression(methodSymbol.ContainingType.ToDisplayString());
            }

            if (!methodSymbol.IsStatic)
            {
                if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    // add the string to the beginning of the arguments list
                    yield return GetMappedExpression(memberAccess.Expression);
                }
                else
                {
                    yield return CreateVariableExpression("this");
                }
            }

            if (invocation.ArgumentList == null)
            {
                yield break;
            }

            foreach (var argument in invocation.ArgumentList.Arguments)
            {
                yield return GetMappedExpression(argument.Expression);
            }
        }

        private Instruction AddMethodCall(SyntaxNode invocation, IMethodSymbol methodSymbol, params Expression[] arguments)
        {
            var instruction = CreateMethodCall(invocation, UcfgMethod.Create(methodSymbol), arguments);
            return instruction;
        }

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private static bool IsLocalVarOrParameter(ISymbol symbol) =>
            symbol is ILocalSymbol local ||
            symbol is IParameterSymbol parameter;

        private static bool ReturnsString(IMethodSymbol methodSymbol) =>
            methodSymbol.ReturnType.Is(KnownType.System_String);

        private Expression GetMappedExpression(SyntaxNode syntaxNode) =>
            syntaxNodeToUcfgExpressionCache.GetValueOrDefault(syntaxNode.RemoveParentheses());
    }
}

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
                    return CreateDefaultInstruction(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor,
            string parameterName) =>
            new[]
            {
                AddMethodCall(attributeSyntax, attributeCtor),
                CreateParameterAnnotation(parameterName, attributeSyntax),
            };

        private IEnumerable<Instruction> CreateDefaultInstruction(SyntaxNode node) =>
            new[] { CreateConstant(node) };

        private IEnumerable<Instruction> CreateFromObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var ctorSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (ctorSymbol == null)
            {
                return new[] { CreateConstant(objectCreationExpression) };
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
                CreateInstruction(objectCreationExpression, UcfgMethod.Assignment, CreateTempVariable(), ConstantExpression),
                AddMethodCall(objectCreationExpression.Type, ctorSymbol, BuildArguments(objectCreationExpression).ToArray())
            };
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
                return new[] { CreateAssignment(variableDeclarator, variable.Name,
                    GetMappedExpression((variableDeclarator.Initializer.Value))) };
            }

            return Enumerable.Empty<Instruction>();
        }

        private IEnumerable<Instruction> CreateFromBinaryExpression(BinaryExpressionSyntax binaryExpression) =>
             new[] { CreateConcatenation(binaryExpression, GetMappedExpression(binaryExpression.Right),
                 GetMappedExpression((binaryExpression.Left))) };

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

            return new[] { CreateEntryPoint(methodDeclaration,
                methodDeclaration.ParameterList.Parameters.Select(GetMappedExpression).ToArray()) };
        }

        private IEnumerable<Expression> BuildArguments(ObjectCreationExpressionSyntax objectCreation)
        {
            // A call to a constructor should look like:
            // %X := new Ctor()
            // %X+1 := Ctor_MethodId [ %X params ]
            // variable := __id [ %X ]
            // So when building the args of the method call we need to pass the instance as first argument.
            var arguments = new List<Expression> { GetMappedExpression(objectCreation) };

            if (objectCreation.ArgumentList != null)
            {
                arguments.AddRange(objectCreation.ArgumentList.Arguments.Select(a => a.Expression).Select(GetMappedExpression));
            }

            return arguments;
        }

        private IEnumerable<Expression> BuildArguments(InvocationExpressionSyntax invocation, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.IsStatic ||
                methodSymbol.ReducedFrom != null)
            {
                yield return CreateStaticCallExpression(methodSymbol.ContainingType.ToDisplayString());
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
                    yield return CreateThisExpression();
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
            var instruction = CreateInstruction(invocation, UcfgMethod.Create(methodSymbol), CreateTempVariable(), arguments);
            return instruction;
        }

        public Instruction CreateConcatenation(SyntaxNode syntaxNode, params Expression[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.Concatenation, CreateTempVariable(), arguments);

        public Instruction CreateAssignment(SyntaxNode syntaxNode, string variableName, Expression argument) =>
            CreateInstruction(syntaxNode, UcfgMethod.Assignment, variableName, argument);

        public Instruction CreateEntryPoint(SyntaxNode syntaxNode, params Expression[] arguments) =>
            CreateInstruction(syntaxNode, UcfgMethod.EntryPoint, CreateTempVariable(), arguments);

        public Instruction CreateParameterAnnotation(string parameterName, SyntaxNode attributeSyntax) =>
            CreateInstruction(attributeSyntax, UcfgMethod.Annotation, parameterName, GetMappedExpression(attributeSyntax));

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

        private static Expression CreateVariableExpression(string name) =>
            new Expression { Var = new Variable { Name = name } };

        private static Expression CreateThisExpression() =>
            new Expression { This = new This() };

        private static Expression CreateStaticCallExpression(string className) =>
            new Expression { Classname = new ClassName { Classname = className } };

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private static bool IsLocalVarOrParameter(ISymbol symbol) =>
            symbol is ILocalSymbol local ||
            symbol is IParameterSymbol parameter;
    }
}

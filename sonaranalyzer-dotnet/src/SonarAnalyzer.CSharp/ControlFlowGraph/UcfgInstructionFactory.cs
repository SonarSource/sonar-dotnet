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
    internal class UcfgInstructionFactory
    {
        private static readonly IEnumerable<Instruction> NoInstructions = Enumerable.Empty<Instruction>();

        private readonly SemanticModel semanticModel;
        private readonly UcfgExpressionService expressionService;

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
                    return ProcessObjectCreationExpression(objectCreation);

                case ArrayCreationExpressionSyntax arrayCreation:
                    return ProcessArrayCreationExpression(arrayCreation);

                case IdentifierNameSyntax identifierName:
                    return ProcessIdentifierName(identifierName);

                case GenericNameSyntax genericName:
                    return ProcessGenericName(genericName);

                case VariableDeclaratorSyntax variableDeclarator:
                    return ProcessVariableDeclarator(variableDeclarator);

                case BinaryExpressionSyntax binaryExpression:
                    return ProcessBinaryExpression(binaryExpression);

                case InvocationExpressionSyntax invocationExpression:
                    return ProcessInvocationExpression(invocationExpression);

                case AssignmentExpressionSyntax assignmentExpression:
                    return ProcessAssignmentExpression(assignmentExpression);

                case BaseMethodDeclarationSyntax methodDeclaration:
                    return ProcessBaseMethodDeclaration(methodDeclaration);

                case InstanceExpressionSyntax instanceExpression:
                    expressionService.Associate(instanceExpression, UcfgExpression.This);
                    return NoInstructions;

                case MemberAccessExpressionSyntax memberAccessExpression:
                    return ProcessMemberAccessExpression(memberAccessExpression);

                case ConstructorInitializerSyntax constructorInitializer:
                    return CreateFromConstructorInitializer(constructorInitializer);

                case ElementAccessExpressionSyntax elementAccessExpression:
                    return ProcessElementAccessExpression(elementAccessExpression);

                default:
                    expressionService.Associate(syntaxNode, UcfgExpression.Constant);
                    return NoInstructions;
            }
        }

        private IEnumerable<Instruction> CreateFromConstructorInitializer(ConstructorInitializerSyntax constructorInitializer)
        {
            var chainedCtor = GetSymbol(constructorInitializer) as IMethodSymbol;
            if (chainedCtor == null)
            {
                return Enumerable.Empty<Instruction>();
            }

            var arguments = new[] { UcfgExpression.This }
                .Concat(constructorInitializer.ArgumentList?.Arguments
                    .Select(a => a.Expression)
                    .Select(expressionService.GetExpression)
                    ?? Enumerable.Empty<UcfgExpression>());

            return CreateAssignCall(constructorInitializer, chainedCtor, arguments.ToArray());
        }

        private IEnumerable<Instruction> ProcessElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpression)
        {
            var targetObject = expressionService.GetExpression(elementAccessExpression.Expression);

            var elementAccess = expressionService.CreateArrayAccess(
                semanticModel.GetSymbolInfo(elementAccessExpression.Expression).Symbol, targetObject);

            // handling for parenthesized left side of an assignment (x[5]) = s
            var topParenthesized = elementAccessExpression.GetSelfOrTopParenthesizedExpression();

            // When the array access is on the left side of an assignment expression we will generate the
            // set instruction in the assignment expression handler, hence we just associate the two
            // syntax and the ucfg expression.
            if (IsLeftSideOfAssignment(topParenthesized))
            {
                expressionService.Associate(elementAccessExpression, elementAccess);
                return NoInstructions;
            }

            // for anything else we generate __arrayGet instruction
            return CreateAssignCall(
                elementAccessExpression,
                UcfgMethodId.ArrayGet,
                expressionService.CreateVariable(elementAccess.TypeSymbol),
                targetObject);

            bool IsLeftSideOfAssignment(SyntaxNode syntaxNode) =>
                syntaxNode.Parent is AssignmentExpressionSyntax assignmentExpression &&
                assignmentExpression.Left == syntaxNode;
        }

        public IEnumerable<Instruction> CreateAttributeInstructions(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor,
            string parameterName)
        {
            var targetOfAttribute = expressionService.GetExpression(attributeSyntax.Parent.Parent);

            return CreateAssignCall(attributeSyntax, UcfgMethodId.Annotate,
                    expressionService.CreateVariable(attributeCtor.ReturnType), expressionService.CreateConstant(attributeCtor),
                    targetOfAttribute)
                .Concat(CreateAssignCall(attributeSyntax, UcfgMethodId.Annotation, targetOfAttribute,
                    expressionService.GetExpression(attributeSyntax)));
        }

        private IEnumerable<Instruction> ProcessObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var methodSymbol = GetSymbol(objectCreationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                return NoInstructions;
            }

            // A call to a constructor should look like:
            // %X := new Ctor()
            // %X+1 := Ctor_MethodId [ %X params ]
            // variable := __id [ %X ]
            // As all instructions creation result in the SyntaxNode being associated with the return variable, we would
            // end up with variable := __id [ %X+1 ] (the objectCreationExpression node being now associated to %X+1).
            // To avoid this behavior, we associate the method call to the type of the objectCreationExpression

            var arguments = objectCreationExpression.ArgumentList?.Arguments
                .Select(a => a.Expression)
                .Select(expressionService.GetExpression)
                ?? Enumerable.Empty<UcfgExpression>();

            return CreateNewObject(objectCreationExpression, methodSymbol,
                    expressionService.CreateVariable(methodSymbol.ReturnType))
                .Concat(CreateAssignCall(objectCreationExpression.Type, methodSymbol,
                    new[] { expressionService.GetExpression(objectCreationExpression) }.Concat(arguments).ToArray()));
        }

        private IEnumerable<Instruction> ProcessArrayCreationExpression(ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            var arrayTypeSymbol = semanticModel.GetTypeInfo(arrayCreationExpression).Type as IArrayTypeSymbol;
            if (arrayTypeSymbol == null)
            {
                return NoInstructions;
            }

            // A call that constructs an array should look like:
            // Code: var x = new string[42];
            // %0 := new string[]       // <-- created by this method
            // x = __id [ %0 ]          // <-- created by the method that handles the assignment

            return CreateNewArray(arrayCreationExpression, arrayTypeSymbol,
                    expressionService.CreateVariable(arrayTypeSymbol));
        }

        private IEnumerable<Instruction> ProcessGenericName(GenericNameSyntax genericName)
        {
            var namedTypeSymbol = GetSymbol(genericName) as INamedTypeSymbol;

            UcfgExpression target = null;

            if (namedTypeSymbol != null)
            {
                target = namedTypeSymbol.IsStatic
                ? expressionService.CreateClassName(namedTypeSymbol)
                : UcfgExpression.This;
            }

            var ucfgExpression = expressionService.Create(namedTypeSymbol, target);
            expressionService.Associate(genericName, ucfgExpression);

            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessIdentifierName(IdentifierNameSyntax identifierName)
        {
            if (identifierName.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name == identifierName)
            {
                return NoInstructions;
            }

            var target = UcfgExpression.Unknown;
            var assignmentExpression = identifierName.Parent as AssignmentExpressionSyntax;

            if (assignmentExpression != null &&
                assignmentExpression.Parent is InitializerExpressionSyntax initializerExpression)
            {
                // When we process a field or property, and it is part of a new class initialization we should retrieve the
                // correct target (i.e. the class instantiation).
                target = expressionService.GetExpression(initializerExpression.Parent);
            }

            var symbol = GetSymbol(identifierName);

            if (target == UcfgExpression.Unknown)
            {
                if (symbol.IsStatic)
                {
                    target = symbol is INamedTypeSymbol namedTypeSymbol
                        ? expressionService.CreateClassName(namedTypeSymbol)
                        : expressionService.CreateClassName(symbol.ContainingType);
                }
                else
                {
                    target = UcfgExpression.This;
                }
            }

            var ucfgExpression = expressionService.Create(symbol, target);
            expressionService.Associate(identifierName, ucfgExpression);

            if (assignmentExpression?.Left != identifierName &&
                ucfgExpression is UcfgExpression.PropertyAccessExpression propertyExpression)
            {
                return CreateAssignCall(identifierName, propertyExpression.GetMethodSymbol, propertyExpression.Target);
            }

            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer == null)
            {
                return NoInstructions;
            }

            var leftExpression = expressionService.Create(semanticModel.GetDeclaredSymbol(variableDeclarator), null);
            var rightExpression = expressionService.GetExpression(variableDeclarator.Initializer.Value);

            return CreateAssignCall(variableDeclarator, UcfgMethodId.Assignment, leftExpression, rightExpression);
        }

        private IEnumerable<Instruction> ProcessBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            var binaryExpressionTypeSymbol = this.semanticModel.GetTypeInfo(binaryExpression).ConvertedType;

            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.PlusToken))
            {
                var leftExpression = expressionService.GetExpression(binaryExpression.Left);
                var rightExpression = expressionService.GetExpression(binaryExpression.Right);

                // TODO: Handle property (for non string) get on left or right
                // TODO: Handle implicit ToString
                if (leftExpression.TypeSymbol.Is(KnownType.System_String) ||
                    rightExpression.TypeSymbol.Is(KnownType.System_String))
                {
                    return CreateAssignCall(binaryExpression, UcfgMethodId.Concatenation,
                        expressionService.CreateVariable(binaryExpressionTypeSymbol), leftExpression, rightExpression);
                }
            }

            expressionService.Associate(binaryExpression, UcfgExpression.Constant);
            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessInvocationExpression(InvocationExpressionSyntax invocationExpression)
        {
            var methodSymbol = GetSymbol(invocationExpression) as IMethodSymbol;
            if (methodSymbol == null)
            {
                expressionService.Associate(invocationExpression, UcfgExpression.Constant);
                return NoInstructions;
            }

            var methodExpression = expressionService.GetExpression(invocationExpression.Expression)
                as UcfgExpression.MethodAccessExpression;
            if (methodExpression == UcfgExpression.Unknown)
            {
                expressionService.Associate(invocationExpression, UcfgExpression.Constant);
                return NoInstructions;
            }

            var arguments = new List<UcfgExpression>();

            if (methodSymbol.ReducedFrom != null)
            {
                arguments.Add(expressionService.CreateClassName(methodSymbol.ContainingType));
                if (invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    arguments.Add(expressionService.GetExpression(memberAccessExpression.Expression));
                }
                else
                {
                    throw new UcfgException();
                }
            }
            else
            {
                arguments.Add(methodExpression.Target);
            }

            if (invocationExpression.ArgumentList != null)
            {
                arguments.AddRange(invocationExpression.ArgumentList.Arguments.Select(a => a.Expression)
                    .Select(expressionService.GetExpression));
            }

            return CreateAssignCall(invocationExpression, methodSymbol, arguments.ToArray());
        }

        private IEnumerable<Instruction> ProcessAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var instructions = new List<Instruction>();

            var leftExpression = expressionService.GetExpression(assignmentExpression.Left);

            // Because of the current shape of the CFG, it is possible not to have the left expression already processed but
            // only when left is identifier for field, local variable or parameter.
            // In this case, we need to manually call process identifier on left part before being able to retrieve from the cache.
            if (leftExpression == UcfgExpression.Unknown &&
                assignmentExpression.Left is IdentifierNameSyntax identifierNameSyntax)
            {
                instructions.AddRange(ProcessIdentifierName(identifierNameSyntax));
                leftExpression = expressionService.GetExpression(assignmentExpression.Left);
            }

            var rightExpression = expressionService.GetExpression(assignmentExpression.Right);

            // handle left part of the assignment
            switch (leftExpression)
            {
                case UcfgExpression.PropertyAccessExpression leftPropertyExpression
                    when (leftPropertyExpression.SetMethodSymbol != null):
                    instructions.AddRange(CreateAssignCall(assignmentExpression, leftPropertyExpression.SetMethodSymbol,
                        leftPropertyExpression.Target, rightExpression));
                    break;

                case UcfgExpression.FieldAccessExpression fieldExpression:
                case UcfgExpression.VariableExpression variableExpression:
                    instructions.AddRange(CreateAssignCall(assignmentExpression, UcfgMethodId.Assignment,
                        leftExpression, rightExpression));
                    break;

                case UcfgExpression.ElementAccessExpression elementExpression:
                    instructions.AddRange(
                        CreateAssignCall(
                            assignmentExpression,
                            UcfgMethodId.ArraySet,
                            expressionService.CreateVariable(elementExpression.TypeSymbol),
                            elementExpression.Target,
                            rightExpression));
                    break;

                default:
                    break;
            }

            return instructions;
        }

        private IEnumerable<Instruction> ProcessBaseMethodDeclaration(BaseMethodDeclarationSyntax methodDeclaration)
        {
            var methodSymbol = this.semanticModel.GetDeclaredSymbol(methodDeclaration);

            foreach (var parameter in methodSymbol.Parameters)
            {
                expressionService.Associate(parameter.DeclaringSyntaxReferences.First().GetSyntax(),
                    expressionService.Create(parameter, null));
            }

            return CreateAssignCall(methodDeclaration, UcfgMethodId.EntryPoint,
                expressionService.CreateVariable(methodSymbol.ReturnType),
                methodDeclaration.ParameterList.Parameters.Select(expressionService.GetExpression).ToArray());
        }

        private IEnumerable<Instruction> ProcessMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            var memberAccessSymbol = GetSymbol(memberAccessExpression);
            var leftSideExpression = expressionService.GetExpression(memberAccessExpression.Expression);

            var instructions = new List<Instruction>();

            if (leftSideExpression is UcfgExpression.FieldAccessExpression fieldExpression
                && memberAccessSymbol is IFieldSymbol fieldSymbol)
            {
                instructions.AddRange(CreateAssignCall(memberAccessExpression.Expression, UcfgMethodId.Assignment,
                    expressionService.CreateVariable(fieldExpression.TypeSymbol), fieldExpression));
                leftSideExpression = expressionService.GetExpression(memberAccessExpression.Expression);
            }

            var ucfgExpression = expressionService.Create(memberAccessSymbol, leftSideExpression);
            expressionService.Associate(memberAccessExpression, ucfgExpression);

            var assignmentExpression = memberAccessExpression.Parent as AssignmentExpressionSyntax;
            if (assignmentExpression?.Left != memberAccessExpression &&
                ucfgExpression is UcfgExpression.PropertyAccessExpression propertyExpression)
            {
                instructions.AddRange(CreateAssignCall(memberAccessExpression, propertyExpression.GetMethodSymbol,
                    propertyExpression.Target));
            }

            return instructions;
        }

        private IEnumerable<Instruction> CreateAssignCall(SyntaxNode invocation, IMethodSymbol methodSymbol,
            params UcfgExpression[] arguments) =>
            CreateAssignCall(invocation, UcfgMethodId.CreateMethodId(methodSymbol),
                expressionService.CreateVariable(methodSymbol.ReturnType), arguments);

        private IEnumerable<Instruction> CreateAssignCall(SyntaxNode syntaxNode, UcfgMethodId identifier,
            UcfgExpression callTarget, params UcfgExpression[] arguments)
        {
            if (syntaxNode is ObjectCreationExpressionSyntax)
            {
                throw new UcfgException("Expecting this method not to be called for nodes of type 'ObjectCreationExpressionSyntax'.");
            }

            // TODO: Uncomment this check when the attribute handling is changed. Currently this fails because no args are passed
            //       to the method call
            //if (UcfgIdentifier.IsMethodId(identifier))
            //{
            //    if (arguments.Length < 1 ||
            //        arguments[0].ExprCase == Expression.ExprOneofCase.Const ||
            //        arguments[0].ExprCase == Expression.ExprOneofCase.FieldAccess)
            //    {
            //        var actualArgValue = arguments.Length == 0 ? "nothing" : arguments[0].ExprCase.ToString();
            //        throw new UcfgBusinessException("Expecting to have the first argument of this call to be of type " +
            //            $"'Variable', 'This' or 'Classname' but got '{actualArgValue}'");
            //    }
            //}

            expressionService.Associate(syntaxNode, callTarget);

            var instruction = new Instruction
            {
                Assigncall = new AssignCall
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    MethodId = identifier.ToString()
                }
            };
            instruction.Assigncall.Args.AddRange(arguments.Select(a => a.Expression));
            callTarget.ApplyAsTarget(instruction);

            return new[] { instruction };
        }

        private IEnumerable<Instruction> CreateNewObject(ObjectCreationExpressionSyntax syntaxNode,
            IMethodSymbol ctorSymbol, UcfgExpression callTarget)
        {
            expressionService.Associate(syntaxNode, callTarget);

            var instruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    Type = UcfgMethodId.CreateTypeId(ctorSymbol.ContainingType).ToString()
                }
            };
            callTarget.ApplyAsTarget(instruction);

            return new[] { instruction };
        }

        private IEnumerable<Instruction> CreateNewArray(ArrayCreationExpressionSyntax syntaxNode,
            IArrayTypeSymbol arrayTypeSymbol, UcfgExpression callTarget)
        {
            expressionService.Associate(syntaxNode, callTarget);

            var instruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    Type = UcfgMethodId.CreateArrayTypeId(arrayTypeSymbol).ToString()
                }
            };
            callTarget.ApplyAsTarget(instruction);

            return new[] { instruction };
        }

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;
    }
}

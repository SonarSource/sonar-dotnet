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

        private static readonly ISet<KnownType> UnsupportedVariableTypes =
            new[] { KnownType.System_Boolean }
            .Union(KnownType.IntegralNumbers)
            .Union(KnownType.NonIntegralNumbers)
            .Union(KnownType.PointerTypes)
            .ToHashSet();

        private readonly UcfgExpressionService expressionService;
        private readonly SemanticModel semanticModel;

        public UcfgInstructionFactory(SemanticModel semanticModel, UcfgExpressionService expressionService)
        {
            this.semanticModel = semanticModel;
            this.expressionService = expressionService;
        }

        public IEnumerable<Instruction> CreateFrom(SyntaxNode syntaxNode)
        {
            // We might need to process some nodes in a different order from the block builder
            // supplies them e.g. an array creation can have an initializer that creates a new
            // object. In that case, we need to process the new object creation as part of the
            // array creation, and ignore that syntax node when the block builder supplies it later.
            if (expressionService.IsAlreadyProcessed(syntaxNode))
            {
                return NoInstructions;
            }

            switch (syntaxNode)
            {
                // ========================================================================
                // Handles instructions related to the creation of a new instance
                // ========================================================================
                case ObjectCreationExpressionSyntax objectCreation:
                    return ProcessObjectCreation(objectCreation);

                case ArrayCreationExpressionSyntax arrayCreation:
                    return ProcessArrayCreation(arrayCreation);

                case ImplicitArrayCreationExpressionSyntax implicitArray:
                    return ProcessImplicitArrayCreation(implicitArray);

                // ========================================================================
                // Association
                // ========================================================================
                case InstanceExpressionSyntax instanceExpression:
                    expressionService.Associate(instanceExpression, expressionService.This);
                    return NoInstructions;

                // ========================================================================
                // Handles instructions related to function calls
                // ========================================================================
                case VariableDeclaratorSyntax variableDeclarator:
                    return ProcessVariableDeclarator(variableDeclarator);

                case AssignmentExpressionSyntax assignmentExpression:
                    return ProcessAssignmentExpression(assignmentExpression);

                case BinaryExpressionSyntax binaryExpression:
                    return ProcessBinaryExpression(binaryExpression);

                case InvocationExpressionSyntax invocationExpression:
                    return ProcessInvocationExpression(invocationExpression);

                case BaseMethodDeclarationSyntax methodDeclaration:
                    return ProcessBaseMethodDeclaration(methodDeclaration);

                case ConstructorInitializerSyntax constructorInitializer:
                    return ProcessConstructorInitializer(constructorInitializer);

                case ElementAccessExpressionSyntax elementAccess:
                    return ProcessElementAccess(elementAccess);

                default:
                    return ProcessReadSyntaxNode(syntaxNode);
            }
        }

        public IEnumerable<Instruction> CreateFromAttributeSyntax(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor,
            string parameterName)
        {
            // Only variable expressions can be annotated.
            // Constants (e.g. parameters of type int) cannot be annotated.
            var targetOfAttribute = expressionService.GetOrDefault(attributeSyntax.Parent.Parent);

            if (targetOfAttribute == UcfgExpressionService.UnknownExpression ||
                targetOfAttribute.ExprCase != Expression.ExprOneofCase.Var)
            {
                return NoInstructions;
            }

            return NoInstructions
                .Concat(BuildAnnotateCall(attributeSyntax, attributeCtor, targetOfAttribute))
                .Concat(BuildAnnotationCall(attributeSyntax, targetOfAttribute, expressionService.GetOrDefault(attributeSyntax)));
        }

        private void ApplyAsTarget(Expression expression, Instruction instruction)
        {
            switch (instruction.InstrCase)
            {
                case Instruction.InstrOneofCase.Assigncall:
                    switch (expression.ExprCase)
                    {
                        case Expression.ExprOneofCase.Var:
                            instruction.Assigncall.Variable = expression.Var;
                            break;

                        case Expression.ExprOneofCase.FieldAccess:
                            instruction.Assigncall.FieldAccess = expression.FieldAccess;
                            break;

                        default:
                            throw new UcfgException("Unexpected type of target for the instruction.");
                    }
                    break;

                case Instruction.InstrOneofCase.NewObject:
                    switch (expression.ExprCase)
                    {
                        case Expression.ExprOneofCase.Var:
                            instruction.NewObject.Variable = expression.Var;
                            break;

                        case Expression.ExprOneofCase.FieldAccess:
                            instruction.NewObject.FieldAccess = expression.FieldAccess;
                            break;

                        default:
                            throw new UcfgException("Unexpected type of target for the instruction.");
                    }
                    break;

                default:
                    throw new UcfgException("Unexpected instruction type.");
            }
        }

        private IEnumerable<Instruction> BuildAnnotateCall(SyntaxNode syntaxNode, IMethodSymbol attributeMethodSymbol,
            Expression target) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.Annotate, expressionService.CreateVariable(),
                expressionService.CreateConstant(attributeMethodSymbol.ToUcfgMethodId()), target);

        private IEnumerable<Instruction> BuildAnnotationCall(SyntaxNode syntaxNode, Expression destination, Expression value) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.Annotation, destination, value);

        private IEnumerable<Instruction> BuildArrayGetCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol,
            Expression target) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.ArrayGet,
                expressionService.CreateVariable(), target);

        private IEnumerable<Instruction> BuildArraySetCall(SyntaxNode syntaxNode, Expression destination, Expression value) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.ArraySet,
                expressionService.CreateVariable(), destination, value);

        private IEnumerable<Instruction> BuildConcatCall(SyntaxNode syntaxNode, Expression left, Expression right) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.Concatenation,
                expressionService.CreateVariable(), left, right);

        private IEnumerable<Instruction> BuildEntryPointCall(BaseMethodDeclarationSyntax baseMethodDeclarationSyntax)
        {
            var location = GetMethodIdentifierLocation();

            return BuildFunctionCall(baseMethodDeclarationSyntax, UcfgBuiltInMethodId.EntryPoint,
                expressionService.CreateVariable(), location,
                baseMethodDeclarationSyntax.ParameterList.Parameters.Select(expressionService.GetOrDefault).ToArray());

            Protobuf.Ucfg.Location GetMethodIdentifierLocation()
            {
                switch (baseMethodDeclarationSyntax)
                {
                    case ConstructorDeclarationSyntax constructorDeclarationSyntax:
                        return constructorDeclarationSyntax.Identifier.GetUcfgLocation();

                    case MethodDeclarationSyntax methodDeclarationSyntax:
                        return methodDeclarationSyntax.Identifier.GetUcfgLocation();
                        
                    case OperatorDeclarationSyntax operatorDeclarationSyntax:
                        return operatorDeclarationSyntax.OperatorToken.GetUcfgLocation();

                    default:
                        return baseMethodDeclarationSyntax.GetUcfgLocation();
                }
            }
        }

        private IEnumerable<Instruction> BuildFunctionCall(SyntaxNode syntaxNode, string signature,
            Expression destination, Protobuf.Ucfg.Location location, params Expression[] expressionList)
        {
            var functionCallInstruction = new Instruction
            {
                Assigncall = new AssignCall
                {
                    Location = location,
                    MethodId = signature
                }
            };
            functionCallInstruction.Assigncall.Args.AddRange(expressionList);
            ApplyAsTarget(destination, functionCallInstruction);

            expressionService.Associate(syntaxNode, destination);

            return new[] { functionCallInstruction };
        }

        private IEnumerable<Instruction> BuildFunctionCall(SyntaxNode syntaxNode, string signature,
            Expression destination, params Expression[] expressionList)
        {
            return BuildFunctionCall(syntaxNode, signature, destination, syntaxNode.GetUcfgLocation(), expressionList);
        }

        private IEnumerable<Instruction> BuildFunctionCall(SyntaxNode syntaxNode, IMethodSymbol methodSymbol, Expression target,
            params Expression[] additionalArguments)
        {
            if (target.ExprCase == Expression.ExprOneofCase.Const)
            {
                expressionService.Associate(syntaxNode, target);
                return NoInstructions;
            }

            return BuildFunctionCall(syntaxNode, methodSymbol.ToUcfgMethodId(),
                expressionService.CreateVariable(), new[] { target }.Concat(additionalArguments).ToArray());
        }

        private IEnumerable<Instruction> BuildIdentityCall(SyntaxNode syntaxNode, Expression destination, Expression value) =>
            BuildFunctionCall(syntaxNode, UcfgBuiltInMethodId.Identity, destination, value);

        private IEnumerable<Instruction> BuildNewInstance(ExpressionSyntax expressionSyntax, ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                expressionService.Associate(expressionSyntax, expressionService.CreateConstant());
                return NoInstructions;
            }

            var newObjectInstruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = expressionSyntax.GetUcfgLocation(),
                    Type = typeSymbol.ToDisplayString()
                }
            };

            var callTarget = expressionService.CreateVariable();
            expressionService.Associate(expressionSyntax, callTarget);
            ApplyAsTarget(callTarget, newObjectInstruction);

            return new[] { newObjectInstruction };
        }

        private IEnumerable<Instruction> BuildNewArrayInstance(ExpressionSyntax syntaxNode,
            IArrayTypeSymbol arrayTypeSymbol, InitializerExpressionSyntax arrayInitializationNode)
        {
            var newInstructions = BuildNewInstance(syntaxNode, arrayTypeSymbol);

            var callTarget = expressionService.GetOrDefault(syntaxNode);
            return newInstructions.Concat(ProcessArrayInitializer(arrayInitializationNode, callTarget));
        }

        private IEnumerable<Instruction> BuildOperatorCall(SyntaxNode syntaxNode, Expression leftExpression,
            Expression rightExpression)
        {
            var operatorMethodSymbol = this.semanticModel.GetSymbolInfo(syntaxNode).Symbol as IMethodSymbol;
            if (operatorMethodSymbol == null)
            {
                expressionService.Associate(syntaxNode, expressionService.CreateConstant());
                return NoInstructions;
            }

            var isStringConcat = operatorMethodSymbol.MethodKind == MethodKind.BuiltinOperator &&
                operatorMethodSymbol.Parameters.Length == 2 &&
                operatorMethodSymbol.Name == "op_Addition" &&
                operatorMethodSymbol.ContainingType.Is(KnownType.System_String);

            if (isStringConcat)
            {
                return BuildConcatCall(syntaxNode, leftExpression, rightExpression);
            }

            if (!operatorMethodSymbol.ContainingType.IsAny(UnsupportedVariableTypes))
            {
                return BuildFunctionCall(syntaxNode, operatorMethodSymbol,
                    expressionService.CreateClassName(operatorMethodSymbol.ContainingType), leftExpression, rightExpression);
            }

            expressionService.Associate(syntaxNode, expressionService.CreateConstant());
            return NoInstructions;
        }

        private Expression CreateExpressionFromType(ISymbol symbol)
        {
            return IsConstant()
                ? expressionService.CreateConstant()
                : expressionService.CreateVariable(symbol.Name);

            bool IsConstant() =>
                symbol == null ||
                (symbol.Kind != SymbolKind.Local && symbol.Kind != SymbolKind.Parameter) ||
                symbol.GetSymbolType().IsAny(UnsupportedVariableTypes);
        }

        private Expression[] GetInvocationExpressions(IList<IParameterSymbol> parameters, BaseArgumentListSyntax argumentList)
        {
            if (argumentList == null)
            {
                return new Expression[0];
            }

            var expressions = new List<Expression>();

            // Handle un-named arguments and without default value first.
            // This kind of argument always appear first in the list of arguments
            var classicArgumentIndex = 0;
            while (classicArgumentIndex < argumentList.Arguments.Count &&
                argumentList.Arguments[classicArgumentIndex].NameColon == null)
            {
                expressions.Add(expressionService.GetOrDefault(argumentList.Arguments[classicArgumentIndex].Expression));
                classicArgumentIndex++;
            }

            // Handle named arguments and default values
            for (var otherArgumentIndex = classicArgumentIndex; otherArgumentIndex < parameters.Count; otherArgumentIndex++)
            {
                var matchingArgument = argumentList.Arguments
                    .FirstOrDefault(x => x.NameColon?.Name.Identifier.ValueText == parameters[otherArgumentIndex].Name);

                expressions.Add(matchingArgument != null
                    // this is a named argument, let's retrieve its value
                    ? expressionService.GetOrDefault(matchingArgument.Expression)
                    // argument not specified, use its default value (always constant)
                    : expressionService.CreateConstant());
            }

            return expressions.ToArray();
        }

        private ISymbol GetNodeSymbol(SyntaxNode node)
        {
            var currentNode = node.RemoveParentheses();

            var needsParentCall = false;
            while (currentNode is ElementAccessExpressionSyntax elementAccess)
            {
                needsParentCall = true;
                currentNode = elementAccess.Expression.RemoveParentheses();
            }

            // We need to be on the last level of element access to have the correct kind of symbol (which will tell whether this
            // is an array access or a property indexer access).
            if (needsParentCall)
            {
                currentNode = currentNode.Parent;
            }

            return this.semanticModel.GetSymbolInfo(currentNode).Symbol;
        }

        private Expression GetTargetExpression(SyntaxNode syntaxNode, ISymbol nodeSymbol)
        {
            if (syntaxNode is MemberAccessExpressionSyntax memberAccessExpressionSyntax)
            {
                return expressionService.GetOrDefault(memberAccessExpressionSyntax.Expression);
            }

            if (syntaxNode is ElementAccessExpressionSyntax elementAccessExpressionSyntax)
            {
                return expressionService.GetOrDefault(elementAccessExpressionSyntax.Expression);
            }

            if (syntaxNode.GetSelfOrTopParenthesizedExpression().Parent
                    is AssignmentExpressionSyntax assignmentExpressionSyntax &&
                assignmentExpressionSyntax.GetSelfOrTopParenthesizedExpression().Parent
                    is InitializerExpressionSyntax initializerExpressionSyntax)
            {
                return expressionService.GetOrDefault(initializerExpressionSyntax.GetSelfOrTopParenthesizedExpression().Parent);
            }

            if (!nodeSymbol.IsStatic)
            {
                return expressionService.This;
            }

            if (nodeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return expressionService.CreateClassName(namedTypeSymbol);
            }

            return expressionService.CreateClassName(nodeSymbol.ContainingType);
        }

        private IEnumerable<Instruction> ProcessArrayCreation(ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            var typeInfo = semanticModel.GetTypeInfo(arrayCreationExpression);
            var arrayTypeSymbol = typeInfo.Type as IArrayTypeSymbol ??
                typeInfo.ConvertedType as IArrayTypeSymbol;

            if (arrayTypeSymbol == null)
            {
                return NoInstructions;
            }

            // A call that constructs an array should look like: var x = new string[42]
            // %0 := new string[]       // <-- created by this method
            // x = __id [ %0 ]          // <-- created by the method that handles the assignment
            return BuildNewArrayInstance(arrayCreationExpression, arrayTypeSymbol, arrayCreationExpression.Initializer);
        }

        private IEnumerable<Instruction> ProcessImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax implicitArrayExpression)
        {
            // e.g. string[] a2 = new[] { data, data }
            // Need to look at the ConvertedType in this case since the implicit array creation type is null
            var arrayType = this.semanticModel.GetTypeInfo(implicitArrayExpression).ConvertedType as IArrayTypeSymbol;
            if (arrayType == null)
            {
                return NoInstructions;
            }

            return BuildNewArrayInstance(implicitArrayExpression, arrayType, implicitArrayExpression.Initializer);
        }

        private IEnumerable<Instruction> ProcessArrayInitializer(InitializerExpressionSyntax initializerExpression, Expression target)
        {
            if (initializerExpression == null)
            {
                return NoInstructions;
            }

            var newInstructions = new List<Instruction>();
            foreach (ExpressionSyntax expNode in initializerExpression.Expressions)
            {
                // Some of the syntax nodes might already have been processed (e.g. the identifier names)
                // Call CreateFrom to process any nodes that have not already been processed.
                var argInstructions = CreateFrom(expNode);
                newInstructions.AddRange(argInstructions);
                var initializerArg = expressionService.GetOrDefault(expNode);

                var arraySetInstructions = BuildArraySetCall(expNode, target, initializerArg);
                newInstructions.AddRange(arraySetInstructions);
            }
            return newInstructions;
        }

        private IEnumerable<Instruction> ProcessAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var instructions = new List<Instruction>();

            var leftPartSymbol = GetNodeSymbol(assignmentExpression.Left);
            var leftExpression = expressionService.GetOrDefault(assignmentExpression.Left);
            var rightExpression = expressionService.GetOrDefault(assignmentExpression.Right);

            // If the right expression was previously a left expression it's possible that the expression needs to be simplified.
            // For example:
            // field = someString
            // var x = field
            rightExpression = SimplifyFunctionExpression(assignmentExpression.Right, rightExpression, instructions);

            var shouldCreateLeftSide = leftExpression == UcfgExpressionService.UnknownExpression;

            // Because of the current shape of the CFG, if the left part of the assignment is field, local variable or
            // parameter and wasn't used before in the code, it won't have been already processed so we have to force the
            // creation of the right kind of instruction/expression
            if (assignmentExpression.IsKind(SyntaxKind.AddAssignmentExpression))
            {
                // Handle the special add assignment (+=). We decided to treat it as if it was expanded (i.e. we will handle
                // only the add part) and let the rest of the code handle the assignment part.

                // 1. If the left part wasn't processed, force its processing
                if (leftExpression == UcfgExpressionService.UnknownExpression)
                {
                    ProcessReadSyntaxNode(assignmentExpression.Left);
                    leftExpression = expressionService.GetOrDefault(assignmentExpression.Left);
                }

                // 2. Generate the operator call instruction
                instructions.AddRange(BuildOperatorCall(assignmentExpression, leftExpression, rightExpression));
                rightExpression = expressionService.GetOrDefault(assignmentExpression);
                shouldCreateLeftSide = true;
            }

            var leftAsElementAccess = assignmentExpression.Left.RemoveParentheses() as ElementAccessExpressionSyntax;
            if (shouldCreateLeftSide)
            {
                if (leftAsElementAccess != null)
                {
                    leftExpression = expressionService.GetOrDefault(leftAsElementAccess.Expression);
                }
                else if (leftPartSymbol is IFieldSymbol fieldSymbol)
                {
                    leftExpression = expressionService.CreateFieldAccess(fieldSymbol.Name,
                        GetTargetExpression(assignmentExpression.Left, fieldSymbol));
                }
                else if (leftPartSymbol is ILocalSymbol || leftPartSymbol is IParameterSymbol)
                {
                    leftExpression = expressionService.CreateVariable(leftPartSymbol.Name);
                }
                else
                {
                    leftExpression = expressionService.CreateConstant();
                }

                expressionService.Associate(assignmentExpression.Left, leftExpression);
            }

            // handle left part of the assignment
            if (leftAsElementAccess != null &&
                this.semanticModel.GetTypeInfo(leftAsElementAccess.Expression).ConvertedType.TypeKind == TypeKind.Array)
            {
                instructions.AddRange(BuildArraySetCall(assignmentExpression, leftExpression, rightExpression));
                return instructions;
            }

            if (leftPartSymbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.SetMethod != null)
                {
                    var propertySetterArguments = leftAsElementAccess != null
                        ? GetInvocationExpressions(propertySymbol.Parameters, leftAsElementAccess.ArgumentList)
                        : new[] { rightExpression };
                    instructions.AddRange(BuildFunctionCall(assignmentExpression, propertySymbol.SetMethod,
                        GetTargetExpression(assignmentExpression.Left, propertySymbol), propertySetterArguments));
                }
                return instructions;
            }

            instructions.AddRange(BuildIdentityCall(assignmentExpression, leftExpression, rightExpression));
            return instructions;
        }

        private IEnumerable<Instruction> ProcessBaseMethodDeclaration(BaseMethodDeclarationSyntax methodDeclaration)
        {
            var methodSymbol = this.semanticModel.GetDeclaredSymbol(methodDeclaration);

            foreach (var parameter in methodSymbol.Parameters)
            {
                expressionService.Associate(parameter.DeclaringSyntaxReferences.First().GetSyntax(),
                    CreateExpressionFromType(parameter));
            }

            return BuildEntryPointCall(methodDeclaration);
        }

        private IEnumerable<Instruction> ProcessBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            var leftExpression = expressionService.GetOrDefault(binaryExpression.Left);
            var rightExpression = expressionService.GetOrDefault(binaryExpression.Right);

            return BuildOperatorCall(binaryExpression, leftExpression, rightExpression);
        }

        private IEnumerable<Instruction> ProcessConstructorInitializer(ConstructorInitializerSyntax constructorInitializer)
        {
            var thisOrBaseCtorMethodSymbol = this.semanticModel.GetSymbolInfo(constructorInitializer).Symbol as IMethodSymbol;
            if (thisOrBaseCtorMethodSymbol == null)
            {
                return NoInstructions;
            }

            return BuildFunctionCall(constructorInitializer, thisOrBaseCtorMethodSymbol, expressionService.This,
                GetInvocationExpressions(thisOrBaseCtorMethodSymbol.Parameters, constructorInitializer.ArgumentList));
        }

        private IEnumerable<Instruction> ProcessElementAccess(ElementAccessExpressionSyntax elementAccessSyntax)
        {
            if (elementAccessSyntax.GetSelfOrTopParenthesizedExpression().Parent is AssignmentExpressionSyntax assignmentSyntax &&
                assignmentSyntax.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignmentSyntax.Left.RemoveParentheses() == elementAccessSyntax)
            {
                return NoInstructions;
            }

            var elementAccessType = this.semanticModel.GetTypeInfo(elementAccessSyntax.Expression).ConvertedType;
            if (elementAccessType == null)
            {
                return NoInstructions;
            }

            if (elementAccessType.TypeKind == TypeKind.Array)
            {
                return BuildArrayGetCall(elementAccessSyntax, elementAccessType, GetTargetExpression(elementAccessSyntax,
                    elementAccessType));
            }

            var indexerPropertySymbol = GetNodeSymbol(elementAccessSyntax) as IPropertySymbol;
            if (indexerPropertySymbol == null)
            {
                return NoInstructions;
            }

            return BuildFunctionCall(elementAccessSyntax, indexerPropertySymbol.GetMethod,
                GetTargetExpression(elementAccessSyntax, indexerPropertySymbol),
                GetInvocationExpressions(indexerPropertySymbol.Parameters, elementAccessSyntax.ArgumentList));
        }

        private IEnumerable<Instruction> ProcessInvocationExpression(InvocationExpressionSyntax invocationSyntax)
        {
            var methodSymbol = this.semanticModel.GetSymbolInfo(invocationSyntax).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                expressionService.Associate(invocationSyntax, expressionService.CreateConstant());
                return NoInstructions;
            }

            var additionalArguments = new List<Expression>();
            Expression targetExpression;
            if (IsCalledAsExtension(methodSymbol))
            {
                var unparenthesisedExpression = invocationSyntax.Expression.RemoveParentheses();
                if (unparenthesisedExpression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    // First argument is the class name (static method call)
                    targetExpression = expressionService.CreateClassName(methodSymbol.ContainingType);
                    // Second argument is the left side of the invocation
                    additionalArguments.Add(expressionService.GetOrDefault(memberAccessExpression.Expression));
                }
                else
                {
                    throw new UcfgException("Unexpected state, method called as extension of a member but there is no " +
                        "member access available.");
                }
            }
            else
            {
                targetExpression = GetTargetExpression(invocationSyntax.Expression, methodSymbol);
            }

            additionalArguments.AddRange(GetInvocationExpressions(methodSymbol.Parameters, invocationSyntax.ArgumentList));

            return BuildFunctionCall(invocationSyntax, methodSymbol, targetExpression, additionalArguments.ToArray());

            bool IsCalledAsExtension(IMethodSymbol method) =>
                method.ReducedFrom != null;
        }

        private IEnumerable<Instruction> ProcessObjectCreation(ObjectCreationExpressionSyntax objectCreation)
        {
            var methodSymbol = this.semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return NoInstructions;
            }

            return NoInstructions
                .Concat(BuildNewInstance(objectCreation, methodSymbol?.ContainingType?.ConstructedFrom))
                .Concat(BuildFunctionCall(objectCreation.Type, methodSymbol, expressionService.GetOrDefault(objectCreation),
                    GetInvocationExpressions(methodSymbol.Parameters, objectCreation.ArgumentList)));
        }

        private IEnumerable<Instruction> ProcessReadSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode.GetSelfOrTopParenthesizedExpression().Parent is AssignmentExpressionSyntax assignmentSyntax &&
                assignmentSyntax.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignmentSyntax.Left.RemoveParentheses() == syntaxNode)
            {
                return NoInstructions;
            }

            var nodeSymbol = GetNodeSymbol(syntaxNode);

            switch (nodeSymbol)
            {
                case IFieldSymbol fieldSymbol:
                    return BuildIdentityCall(syntaxNode, expressionService.CreateVariable(),
                        expressionService.CreateFieldAccess(fieldSymbol.Name, GetTargetExpression(syntaxNode, fieldSymbol)));

                case IPropertySymbol propertySymbol:
                    return BuildFunctionCall(syntaxNode, propertySymbol.GetMethod, GetTargetExpression(syntaxNode, propertySymbol));

                case INamedTypeSymbol namedTypeSymbol:
                    expressionService.Associate(syntaxNode, expressionService.CreateClassName(namedTypeSymbol));
                    break;

                case ILocalSymbol localSymbol:
                    expressionService.Associate(syntaxNode, expressionService.CreateVariable(localSymbol.Name));
                    break;

                default:
                    expressionService.Associate(syntaxNode, CreateExpressionFromType(nodeSymbol));
                    break;
            }

            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer == null)
            {
                // No need to associate the variable to a variable expression because further usage will rely on a
                // IdentifierNameSyntax object which doesn't yet exist.
                return NoInstructions;
            }

            // Handle the case "string[] a1 = { data }"
            var instructions = NoInstructions;

            if (variableDeclarator.Initializer.Value.IsKind(SyntaxKind.ArrayInitializerExpression) &&
                variableDeclarator.Initializer.Value is InitializerExpressionSyntax initializerExpressionSyntax &&
                // Note: we need to use the ConvertedType here (i.e. after the implicit conversion): the Type
                // returned is null because it isn't specified.
                semanticModel.GetTypeInfo(initializerExpressionSyntax).ConvertedType is IArrayTypeSymbol arrayType)
            {
                instructions = BuildNewArrayInstance(variableDeclarator.Initializer.Value,
                    arrayType, initializerExpressionSyntax);
            }

            return instructions.Concat(
                BuildIdentityCall(variableDeclarator, expressionService.CreateVariable(variableDeclarator.Identifier.Text),
                expressionService.GetOrDefault(variableDeclarator.Initializer.Value)));
        }

        private Expression SimplifyFunctionExpression(SyntaxNode node, Expression expression, List<Instruction> instructions)
        {
            if (expression.ExprCase == Expression.ExprOneofCase.FieldAccess)
            {
                var newVariable = expressionService.CreateVariable();
                instructions.AddRange(BuildIdentityCall(node, newVariable, expression));
                return newVariable;
            }

            return expression;
        }
    }
}

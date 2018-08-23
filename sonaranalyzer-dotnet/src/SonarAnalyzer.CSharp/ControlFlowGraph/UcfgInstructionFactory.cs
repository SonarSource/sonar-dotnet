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
            try
            {
                return UnsafeCreateFrom(syntaxNode);
            }
            catch (System.Exception e)
            {
                var sb = new System.Text.StringBuilder();

                sb.AppendLine("Error processing node in CreateFrom:");
                sb.AppendLine($"Syntax node kind: {syntaxNode.Kind()}");
                sb.AppendLine($"Node file: {syntaxNode.GetLocation()?.GetLineSpan().Path ?? "{unknown}"}");
                sb.AppendLine($"Node start: {syntaxNode.GetLocation()?.GetLineSpan().StartLinePosition.ToString() ?? "{unknown}"}");
                sb.AppendLine($"Node end: {syntaxNode.GetLocation()?.GetLineSpan().EndLinePosition.ToString() ?? "{unknown}"}");
                sb.AppendLine($"Value: {syntaxNode.GetText()}");
                sb.AppendLine($"Inner exception: {e.ToString()}");

                throw new UcfgException(sb.ToString());
            }
        }

        internal IEnumerable<Instruction> UnsafeCreateFrom(SyntaxNode syntaxNode)
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
                    expressionService.Associate(instanceExpression, UcfgExpressionService.This);
                    return NoInstructions;

                case MemberBindingExpressionSyntax memberBinding:
                    expressionService.Associate(memberBinding, expressionService.GetOrDefault(memberBinding.Name));
                    return NoInstructions;

                case ConditionalAccessExpressionSyntax conditionalAccess:
                    expressionService.Associate(conditionalAccess, expressionService.GetOrDefault(conditionalAccess.WhenNotNull));
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

                case ElementBindingExpressionSyntax elementBinding:
                    return ProcessElementAccess(elementBinding);

                case CastExpressionSyntax castExpression:
                    return BuildIdentityCall(castExpression, expressionService.CreateVariable(),
                        expressionService.GetOrDefault(castExpression.Expression));

                case InitializerExpressionSyntax initializerExpression:
                    return ProcessInitializerExpression(initializerExpression);

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

        private IEnumerable<Instruction> ApplyAsTarget(Expression expression, Instruction instruction)
        {
            var instructionsToReturn = new[] { instruction };

            switch (instruction.InstrCase)
            {
                case Instruction.InstrOneofCase.Assigncall:
                    switch (expression.ExprCase)
                    {
                        case Expression.ExprOneofCase.Var:
                            instruction.Assigncall.Variable = expression.Var;
                            return instructionsToReturn;

                        case Expression.ExprOneofCase.FieldAccess:
                            instruction.Assigncall.FieldAccess = expression.FieldAccess;
                            return instructionsToReturn;

                        default:
                            return NoInstructions;
                    }

                case Instruction.InstrOneofCase.NewObject:
                    switch (expression.ExprCase)
                    {
                        case Expression.ExprOneofCase.Var:
                            instruction.NewObject.Variable = expression.Var;
                            return instructionsToReturn;

                        case Expression.ExprOneofCase.FieldAccess:
                            instruction.NewObject.FieldAccess = expression.FieldAccess;
                            return instructionsToReturn;

                        default:
                            return NoInstructions;
                    }

                default:
                    return NoInstructions;
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
            expressionService.Associate(syntaxNode, destination);
            var functionCallInstruction = new Instruction
            {
                Assigncall = new AssignCall
                {
                    Location = location,
                    MethodId = signature
                }
            };
            functionCallInstruction.Assigncall.Args.AddRange(expressionList);

            return ApplyAsTarget(destination, functionCallInstruction);
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

            var callTarget = expressionService.CreateVariable();
            expressionService.Associate(expressionSyntax, callTarget);

            var newObjectInstruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = expressionSyntax.GetUcfgLocation(),
                    Type = typeSymbol.ToDisplayString()
                }
            };

            return ApplyAsTarget(callTarget, newObjectInstruction);
        }

        private IEnumerable<Instruction> BuildNewArrayInstance(ExpressionSyntax syntaxNode, IArrayTypeSymbol arrayTypeSymbol)
        {
            var newInstructions = BuildNewInstance(syntaxNode, arrayTypeSymbol);

            return newInstructions;
        }

        private IEnumerable<Instruction> BuildOperatorCall(SyntaxNode syntaxNode, Expression leftExpression,
            Expression rightExpression)
        {
            if (!(this.semanticModel.GetSymbolInfo(syntaxNode).Symbol is IMethodSymbol operatorMethodSymbol))
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

        private Expression[] GetInvocationExpressions(IList<IParameterSymbol> parameters, BaseArgumentListSyntax argumentList,
            List<Instruction> additionalInstructions)
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
                expressions.Add(GetOrProcessExpression(argumentList.Arguments[classicArgumentIndex].Expression, additionalInstructions));
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

            if (currentNode is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                if (conditionalAccess.Expression.IsKind(SyntaxKind.ThisExpression) ||
                    conditionalAccess.Expression.IsKind(SyntaxKind.BaseExpression))
                {
                    currentNode = conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax bindingExpressionSyntax
                        ? bindingExpressionSyntax.Name
                        : conditionalAccess.WhenNotNull;
                }
                else
                {
                    currentNode = conditionalAccess.Expression;
                }
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

            if (syntaxNode is ElementBindingExpressionSyntax ||
                syntaxNode.Parent is ElementBindingExpressionSyntax ||
                syntaxNode is MemberBindingExpressionSyntax ||
                syntaxNode.Parent is MemberBindingExpressionSyntax)
            {
                var conditionalAccess = syntaxNode.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>();
                return expressionService.GetOrDefault(conditionalAccess.Expression);
            }

            if (syntaxNode.GetFirstNonParenthesizedParent()
                    is AssignmentExpressionSyntax assignmentExpressionSyntax &&
                assignmentExpressionSyntax.GetFirstNonParenthesizedParent()
                    is InitializerExpressionSyntax initializerExpressionSyntax)
            {
                return expressionService.GetOrDefault(initializerExpressionSyntax.GetFirstNonParenthesizedParent());
            }

            if (!nodeSymbol.IsStatic)
            {
                return UcfgExpressionService.This;
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
            return BuildNewArrayInstance(arrayCreationExpression, arrayTypeSymbol);
        }

        private IEnumerable<Instruction> ProcessImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax implicitArrayExpression)
        {
            // e.g. string[] a2 = new[] { data, data }
            // Need to look at the ConvertedType in this case since the implicit array creation type is null
            if (!(this.semanticModel.GetTypeInfo(implicitArrayExpression).ConvertedType is IArrayTypeSymbol arrayType))
            {
                return NoInstructions;
            }

            return BuildNewArrayInstance(implicitArrayExpression, arrayType);
        }

        private IEnumerable<Instruction> ProcessInitializerExpression(InitializerExpressionSyntax initializerExpression)
        {
            if (!initializerExpression.IsKind(SyntaxKind.ArrayInitializerExpression))
            {
                return NoInstructions;
            }

            var instructions = new List<Instruction>();

            // The target of this initialization should be the parent
            var parent = initializerExpression.Parent;

            // When parent is EqualsValueClause or InitializerExpression we need to go up one more level. See examples:
            // var x = { 1, 2 }
            // var y = new object[,] { { 1, 2 }, { 3, 4 } }
            if (parent is EqualsValueClauseSyntax ||
                parent is InitializerExpressionSyntax)
            {
                parent = parent.Parent;
            }

            var destination = GetOrProcessExpression(parent, instructions);

            foreach (var expression in initializerExpression.Expressions)
            {
                // When the initializer item is another initializer we already processed it so we should skip it now.
                // var y = new object[,] { { 1, 2 }, { 3, 4 } }
                if (!(expression is InitializerExpressionSyntax))
                {
                    var associatedExpression = GetOrProcessExpression(expression, instructions);
                    instructions.AddRange(BuildArraySetCall(initializerExpression, destination, associatedExpression));
                }
            }

            return instructions;
        }

        private IEnumerable<Instruction> ProcessAssignmentExpression(AssignmentExpressionSyntax assignmentExpression)
        {
            var instructions = new List<Instruction>();

            var leftPartSymbol = GetNodeSymbol(assignmentExpression.Left);
            var leftExpression = GetOrProcessExpression(assignmentExpression.Left, instructions);
            var rightExpression = GetOrProcessExpression(assignmentExpression.Right, instructions);

            // If the right expression was previously a left expression it's possible that the expression needs to be simplified.
            // For example:
            // field = someString
            // var x = field
            rightExpression = SimplifyFunctionExpression(assignmentExpression.Right, rightExpression, instructions);

            var shouldCreateLeftSide = leftExpression.ExprCase == Expression.ExprOneofCase.Const;

            // Because of the current shape of the CFG, if the left part of the assignment is field, local variable or
            // parameter and wasn't used before in the code, it won't have been already processed so we have to force the
            // creation of the right kind of instruction/expression
            if (assignmentExpression.IsKind(SyntaxKind.AddAssignmentExpression))
            {
                // Handle the special add assignment (+=). We decided to treat it as if it was expanded (i.e. we will handle
                // only the add part) and let the rest of the code handle the assignment part.
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

            if (leftPartSymbol is IEventSymbol)
            {
                return instructions;
            }

            if (leftPartSymbol is IPropertySymbol propertySymbol)
            {
                if (propertySymbol.SetMethod != null)
                {
                    var propertySetterArguments = leftAsElementAccess != null
                        ? GetInvocationExpressions(propertySymbol.Parameters, leftAsElementAccess.ArgumentList, instructions)
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

            if (binaryExpression.OperatorToken.IsKind(SyntaxKind.AsKeyword))
            {
                return BuildIdentityCall(binaryExpression, expressionService.CreateVariable(), leftExpression);
            }

            return BuildOperatorCall(binaryExpression, leftExpression, rightExpression);
        }

        private IEnumerable<Instruction> ProcessConstructorInitializer(ConstructorInitializerSyntax constructorInitializer)
        {
            if (!(this.semanticModel.GetSymbolInfo(constructorInitializer).Symbol is IMethodSymbol thisOrBaseCtorMethodSymbol))
            {
                return NoInstructions;
            }

            var instructions = new List<Instruction>();

            var expressions = GetInvocationExpressions(thisOrBaseCtorMethodSymbol.Parameters, constructorInitializer.ArgumentList,
                instructions);
            instructions.AddRange(BuildFunctionCall(constructorInitializer, thisOrBaseCtorMethodSymbol, UcfgExpressionService.This,
                expressions));

            return instructions;
        }

        private IEnumerable<Instruction> ProcessElementAccess(ElementAccessExpressionSyntax elementAccessSyntax)
        {
            if (elementAccessSyntax.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax assignmentSyntax &&
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

            if (!(GetNodeSymbol(elementAccessSyntax) is IPropertySymbol indexerPropertySymbol))
            {
                return NoInstructions;
            }


            var instructions = new List<Instruction>();
            var expressions = GetInvocationExpressions(indexerPropertySymbol.Parameters, elementAccessSyntax.ArgumentList,
                instructions);

            instructions.AddRange(BuildFunctionCall(elementAccessSyntax, indexerPropertySymbol.GetMethod,
               GetTargetExpression(elementAccessSyntax, indexerPropertySymbol), expressions));

            return instructions;
        }

        private IEnumerable<Instruction> ProcessElementAccess(ElementBindingExpressionSyntax elementAccessSyntax)
        {
            if (elementAccessSyntax.GetSelfOrTopParenthesizedExpression().Parent is AssignmentExpressionSyntax assignmentSyntax &&
                assignmentSyntax.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignmentSyntax.Left.RemoveParentheses() == elementAccessSyntax)
            {
                return NoInstructions;
            }

            var conditionalAccess = elementAccessSyntax.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>();

            var elementAccessType = this.semanticModel.GetTypeInfo(conditionalAccess.Expression).ConvertedType;
            if (elementAccessType == null)
            {
                return NoInstructions;
            }

            if (elementAccessType.TypeKind == TypeKind.Array)
            {
                return BuildArrayGetCall(elementAccessSyntax, elementAccessType, GetTargetExpression(elementAccessSyntax,
                    elementAccessType));
            }

            if (!(GetNodeSymbol(elementAccessSyntax) is IPropertySymbol indexerPropertySymbol))
            {
                return NoInstructions;
            }


            var instructions = new List<Instruction>();
            var expressions = GetInvocationExpressions(indexerPropertySymbol.Parameters, elementAccessSyntax.ArgumentList,
                instructions);

            instructions.AddRange(BuildFunctionCall(elementAccessSyntax, indexerPropertySymbol.GetMethod,
               GetTargetExpression(elementAccessSyntax, indexerPropertySymbol), expressions));

            return instructions;
        }

        private IEnumerable<Instruction> ProcessInvocationExpression(InvocationExpressionSyntax invocationSyntax)
        {
            if (!(this.semanticModel.GetSymbolInfo(invocationSyntax).Symbol is IMethodSymbol methodSymbol))
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
                else if (unparenthesisedExpression is MemberBindingExpressionSyntax memberBindingExpression)
                {
                    var conditionalAccess = memberBindingExpression.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>();
                    // First argument is the class name (static method call)
                    targetExpression = expressionService.CreateClassName(methodSymbol.ContainingType);
                    // Second argument is the left side of the invocation
                    additionalArguments.Add(expressionService.GetOrDefault(conditionalAccess.Expression));
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

            var instructions = new List<Instruction>();
            additionalArguments.AddRange(GetInvocationExpressions(methodSymbol.Parameters, invocationSyntax.ArgumentList,
                instructions));

            instructions.AddRange(BuildFunctionCall(invocationSyntax, methodSymbol, targetExpression,
                additionalArguments.ToArray()));

            return instructions;

            bool IsCalledAsExtension(IMethodSymbol method) =>
                method.ReducedFrom != null;
        }

        private IEnumerable<Instruction> ProcessObjectCreation(ObjectCreationExpressionSyntax objectCreation)
        {
            if (!(this.semanticModel.GetSymbolInfo(objectCreation).Symbol is IMethodSymbol methodSymbol))
            {
                return NoInstructions;
            }

            var instructions = new List<Instruction>();

            instructions.AddRange(BuildNewInstance(objectCreation, methodSymbol?.ContainingType?.ConstructedFrom));

            var expressions = GetInvocationExpressions(methodSymbol.Parameters, objectCreation.ArgumentList, instructions);
            instructions.AddRange(BuildFunctionCall(objectCreation.Type, methodSymbol,
                expressionService.GetOrDefault(objectCreation), expressions));

            return instructions;
        }

        private IEnumerable<Instruction> ProcessReadSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode.GetFirstNonParenthesizedParent() is AssignmentExpressionSyntax assignmentSyntax &&
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
            var instructions = new List<Instruction>();

            if (variableDeclarator.Initializer.Value.IsKind(SyntaxKind.ArrayInitializerExpression) &&
                variableDeclarator.Initializer.Value is InitializerExpressionSyntax initializerExpressionSyntax &&
                // Note: we need to use the ConvertedType here (i.e. after the implicit conversion): the Type
                // returned is null because it isn't specified.
                semanticModel.GetTypeInfo(initializerExpressionSyntax).ConvertedType is IArrayTypeSymbol arrayType)
            {
                instructions.AddRange(BuildNewArrayInstance(variableDeclarator.Initializer.Value, arrayType));
            }

            var initializerExpression = GetOrProcessExpression(variableDeclarator.Initializer.Value, instructions);

            instructions.AddRange(BuildIdentityCall(variableDeclarator,
               expressionService.CreateVariable(variableDeclarator.Identifier.Text), initializerExpression));

            return instructions;
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

        private Expression GetOrProcessExpression(SyntaxNode syntaxNode, List<Instruction> additionalInstructions)
        {
            additionalInstructions.AddRange(CreateFrom(syntaxNode));
            return expressionService.GetOrDefault(syntaxNode);
        }
    }
}

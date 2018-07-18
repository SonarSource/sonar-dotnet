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
        private static readonly ISet<SymbolKind> UnsupportedIdentifierSymbolKinds =
           new HashSet<SymbolKind>
           {
                SymbolKind.Alias,
                SymbolKind.Assembly,
                SymbolKind.DynamicType,
                SymbolKind.ErrorType,
                SymbolKind.Namespace,
                SymbolKind.NetModule,
                SymbolKind.Preprocessing
           };

        private static readonly IEnumerable<Instruction> NoInstructions = Enumerable.Empty<Instruction>();

        private readonly SemanticModel semanticModel;
        private readonly UcfgExpressionService expressionService;

        public UcfgInstructionFactory(SemanticModel semanticModel, UcfgExpressionService expressionService)
        {
            this.semanticModel = semanticModel;
            this.expressionService = expressionService;
        }

        public IEnumerable<Instruction> CreateFromAttributeSyntax(AttributeSyntax attributeSyntax, IMethodSymbol attributeCtor,
            string parameterName)
        {
            // Only variable expressions can be annotated.
            // Constants (e.g. parameters of type int) cannot be annotated.
            var targetOfAttribute = expressionService.GetExpression(attributeSyntax.Parent.Parent)
                as UcfgExpression.VariableExpression;

            if (targetOfAttribute == null)
            {
                return NoInstructions;
            }

            return CreateAnnotateCall(attributeSyntax, attributeCtor.ReturnType, attributeCtor, targetOfAttribute)
                .Concat(CreateAnnotationCall(attributeSyntax, targetOfAttribute, expressionService.GetExpression(attributeSyntax)));
        }

        public IEnumerable<Instruction> CreateFrom(SyntaxNode syntaxNode)
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

                case MemberAccessExpressionSyntax memberAccessExpression:
                    return ProcessMemberAccessExpression(memberAccessExpression);

                case ConstructorInitializerSyntax constructorInitializer:
                    return ProcessConstructorInitializer(constructorInitializer);

                case ElementAccessExpressionSyntax elementAccessExpression:
                    return ProcessElementAccessExpression(elementAccessExpression);

                case InstanceExpressionSyntax instanceExpression:
                    expressionService.Associate(instanceExpression, UcfgExpression.This);
                    return NoInstructions;

                case PredefinedTypeSyntax predefinedType:
                    var namedTypeSymbol = GetSymbol<INamedTypeSymbol>(predefinedType);
                    expressionService.Associate(predefinedType, expressionService.CreateClassName(namedTypeSymbol));
                    return NoInstructions;

                default:
                    var typeSymbol = semanticModel.GetTypeInfo(syntaxNode).ConvertedType;
                    expressionService.Associate(syntaxNode, expressionService.CreateConstant(typeSymbol));
                    return NoInstructions;
            }
        }

        private IEnumerable<Instruction> ProcessConstructorInitializer(ConstructorInitializerSyntax constructorInitializer)
        {
            var chainedCtor = GetSymbol<IMethodSymbol>(constructorInitializer);
            if (chainedCtor == null)
            {
                return NoInstructions;
            }

            return CreateMethodCall(constructorInitializer, chainedCtor, UcfgExpression.This,
                GetAdditionalArguments(constructorInitializer.ArgumentList));
        }

        private IEnumerable<Instruction> ProcessElementAccessExpression(ElementAccessExpressionSyntax elementAccessExpression)
        {
            if (!IsArray(elementAccessExpression.Expression))
            {
                expressionService.Associate(elementAccessExpression, expressionService.CreateConstant());
                return NoInstructions;
            }

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
            return CreateArrayGetCall(elementAccessExpression, elementAccess.TypeSymbol, targetObject);

            bool IsLeftSideOfAssignment(SyntaxNode syntaxNode) =>
                syntaxNode.Parent is AssignmentExpressionSyntax assignmentExpression &&
                assignmentExpression.Left == syntaxNode;

            bool IsArray(ExpressionSyntax expression)
            {
                var elementAccessType = semanticModel.GetTypeInfo(expression).ConvertedType;
                return elementAccessType != null
                    && elementAccessType.TypeKind == TypeKind.Array;
            }
        }

        private IEnumerable<Instruction> ProcessObjectCreationExpression(ObjectCreationExpressionSyntax objectCreationExpression)
        {
            var methodSymbol = GetSymbol<IMethodSymbol>(objectCreationExpression);
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
            return CreateNewObject(objectCreationExpression, methodSymbol)
                .Concat(
                    CreateMethodCall(objectCreationExpression.Type, methodSymbol,
                        expressionService.GetExpression(objectCreationExpression),
                        GetAdditionalArguments(objectCreationExpression.ArgumentList)));
        }

        private IEnumerable<Instruction> ProcessArrayCreationExpression(ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            var arrayTypeSymbol = semanticModel.GetTypeInfo(arrayCreationExpression).Type as IArrayTypeSymbol;
            if (arrayTypeSymbol == null)
            {
                expressionService.Associate(arrayCreationExpression, expressionService.CreateConstant());
                return NoInstructions;
            }

            // A call that constructs an array should look like: var x = new string[42]
            // %0 := new string[]       // <-- created by this method
            // x = __id [ %0 ]          // <-- created by the method that handles the assignment
            return CreateNewArray(arrayCreationExpression, arrayTypeSymbol);
        }

        private IEnumerable<Instruction> ProcessGenericName(GenericNameSyntax genericName)
        {
            var namedTypeSymbol = GetSymbol<INamedTypeSymbol>(genericName);

            UcfgExpression target = null;

            if (namedTypeSymbol != null)
            {
                target = namedTypeSymbol.IsStatic
                ? expressionService.CreateClassName(namedTypeSymbol)
                : UcfgExpression.This;
            }

            var ucfgExpression = expressionService.Create(genericName, namedTypeSymbol, target);
            expressionService.Associate(genericName, ucfgExpression);

            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessIdentifierName(IdentifierNameSyntax identifierName)
        {
            if (identifierName.Parent is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name == identifierName)
            {
                // Identifier is part of a member access and will be handled there, let's bail out
                return NoInstructions;
            }

            var symbol = GetSymbol(identifierName);
            if (UnsupportedIdentifierSymbolKinds.Contains(symbol.Kind))
            {
                // This are some identifier we do not care about, let's bail out
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

            if (target == UcfgExpression.Unknown)
            {
                if (symbol.IsStatic)
                {
                    switch (symbol)
                    {
                        case null:
                            target = UcfgExpression.Unknown;
                            break;

                        case INamedTypeSymbol namedTypeSymbol:
                            target = expressionService.CreateClassName(namedTypeSymbol);
                            break;

                        default:
                            target = expressionService.CreateClassName(symbol.ContainingType);
                            break;
                    }
                }
                else
                {
                    target = UcfgExpression.This;
                }
            }

            var ucfgExpression = expressionService.Create(identifierName, symbol, target);
            expressionService.Associate(identifierName, ucfgExpression);

            return NoInstructions;
        }

        private IEnumerable<Instruction> ProcessVariableDeclarator(VariableDeclaratorSyntax variableDeclarator)
        {
            if (variableDeclarator.Initializer == null)
            {
                // When creating a variable without any initializer there is no need to generate any instruction, let's bail out
                return NoInstructions;
            }

            var toExpression = expressionService.Create(variableDeclarator, semanticModel.GetDeclaredSymbol(variableDeclarator), null);
            var fromExpression = expressionService.GetExpression(variableDeclarator.Initializer.Value);

            return CreateIdCall(variableDeclarator, toExpression, fromExpression);
        }

        private IEnumerable<Instruction> ProcessBinaryExpression(BinaryExpressionSyntax binaryExpression)
        {
            var binaryExpressionSymbol = GetSymbol<IMethodSymbol>(binaryExpression);
            if (binaryExpressionSymbol == null)
            {
                expressionService.Associate(binaryExpression, expressionService.CreateConstant());
                return NoInstructions;
            }

            var leftExpression = expressionService.GetExpression(binaryExpression.Left);
            var rightExpression = expressionService.GetExpression(binaryExpression.Right);

            // TODO: Handle implicit ToString
            var isStringConcat = binaryExpressionSymbol.MethodKind == MethodKind.BuiltinOperator &&
                binaryExpressionSymbol.Parameters.Length == 2 &&
                binaryExpressionSymbol.Name == "op_Addition" &&
                binaryExpressionSymbol.ContainingType.Is(KnownType.System_String);

            // If the method symbol is on string we should generate a concat instruction, otherwise let's do a method call
            // instruction.
            return isStringConcat
                ? CreateConcatCall(binaryExpression, binaryExpressionSymbol.ContainingType, leftExpression, rightExpression)
                : CreateMethodCall(binaryExpression, binaryExpressionSymbol,
                    expressionService.CreateClassName(binaryExpressionSymbol.ContainingType), leftExpression, rightExpression);
        }

        private IEnumerable<Instruction> ProcessInvocationExpression(InvocationExpressionSyntax invocationSyntax)
        {
            var methodSymbol = GetSymbol<IMethodSymbol>(invocationSyntax);
            if (methodSymbol == null)
            {
                expressionService.Associate(invocationSyntax, expressionService.CreateConstant());
                return NoInstructions;
            }

            var invocationExpression = expressionService.GetExpression(invocationSyntax.Expression);
            var methodExpression = invocationExpression as UcfgExpression.MethodAccessExpression;
            if (methodExpression == null)
            {
                expressionService.Associate(invocationSyntax, expressionService.CreateConstant(invocationExpression.TypeSymbol));
                return NoInstructions;
            }

            UcfgExpression targetExpression;
            UcfgExpression memberAccessArgument = null;
            if (IsCalledAsExtension(methodSymbol))
            {
                if (invocationSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpression)
                {
                    // First argument is the class name (static method call)
                    targetExpression = expressionService.CreateClassName(methodSymbol.ContainingType);
                    // Second argument is the left side of the invocation
                    memberAccessArgument = expressionService.GetExpression(memberAccessExpression.Expression);
                }
                else
                {
                    throw new UcfgException("Unexpected state, method called as extension of a member but there is no " +
                        "member access available.");
                }
            }
            else
            {
                targetExpression = methodExpression.Target;
            }

            var additionalArguments = new List<UcfgExpression>();
            if (memberAccessArgument != null)
            {
                additionalArguments.Add(memberAccessArgument);
            }
            additionalArguments.AddRange(GetAdditionalArguments(invocationSyntax.ArgumentList));

            return CreateMethodCall(invocationSyntax, methodSymbol, targetExpression, additionalArguments.ToArray());
        }

        private static bool IsCalledAsExtension(IMethodSymbol methodSymbol) =>
            methodSymbol.ReducedFrom != null;

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

            // Handle the special add assignment (+=)
            // In this block we will only handle the concatenation part, not the assignment (that will be handled right after)
            if (assignmentExpression.IsKind(SyntaxKind.AddAssignmentExpression))
            {
                var addOperatorSymbol = GetSymbol<IMethodSymbol>(assignmentExpression);
                if (addOperatorSymbol != null)
                {
                    // TODO: Handle implicit ToString
                    // If the method symbol is on string we should generate a concat instruction, otherwise let's do a method call
                    // instruction.
                    instructions.AddRange(
                        addOperatorSymbol.ContainingType.Is(KnownType.System_String)
                            ? CreateConcatCall(assignmentExpression, addOperatorSymbol.ContainingType, leftExpression,
                                rightExpression)
                            : CreateMethodCall(assignmentExpression, addOperatorSymbol,
                                expressionService.CreateClassName(addOperatorSymbol.ContainingType), leftExpression, rightExpression));

                    // We have just created a new instruction so we need to make sure we will use the new temp variable
                    rightExpression = expressionService.GetExpression(assignmentExpression);
                }
            }

            // handle left part of the assignment
            switch (leftExpression)
            {
                case UcfgExpression.PropertyAccessExpression leftPropertyExpression
                    when (leftPropertyExpression.SetMethodSymbol != null):
                    instructions.AddRange(CreateMethodCall(assignmentExpression, leftPropertyExpression.SetMethodSymbol,
                        leftPropertyExpression.Target, rightExpression));
                    break;

                case UcfgExpression.FieldAccessExpression fieldExpression:
                case UcfgExpression.VariableExpression variableExpression:
                    instructions.AddRange(CreateIdCall(assignmentExpression, leftExpression, rightExpression));
                    break;

                case UcfgExpression.ElementAccessExpression elementExpression:
                    instructions.AddRange(CreateArraySetCall(assignmentExpression, elementExpression.TypeSymbol,
                        elementExpression.Target, rightExpression));
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
                    expressionService.Create(methodDeclaration, parameter, null));
            }

            return CreateEntryPointCall(methodDeclaration, methodSymbol.ReturnType, methodDeclaration.ParameterList);
        }

        private IEnumerable<Instruction> ProcessMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression)
        {
            var memberAccessSymbol = GetSymbol(memberAccessExpression);
            if (memberAccessSymbol is INamespaceSymbol)
            {
                // Every access of a namespace, type or method inside a namespace is represented
                // by a MemberAccessExpressionSyntax
                // e.g. Ns1.Ns2.Class1.Class2.Method1(); // has four MemberAccessExpressionSyntax nodes
                return NoInstructions;
            }

            var leftSideExpression = expressionService.GetExpression(memberAccessExpression.Expression);

            var instructions = new List<Instruction>();

            if (leftSideExpression is UcfgExpression.FieldAccessExpression fieldExpression
                && memberAccessSymbol is IFieldSymbol fieldSymbol)
            {
                var fieldAccessAsNewVariable = expressionService.CreateVariable(fieldExpression.TypeSymbol);
                instructions.AddRange(CreateIdCall(memberAccessExpression.Expression, fieldAccessAsNewVariable, fieldExpression));
                leftSideExpression = fieldAccessAsNewVariable;
            }

            var ucfgExpression = expressionService.Create(memberAccessExpression, memberAccessSymbol, leftSideExpression);
            expressionService.Associate(memberAccessExpression, ucfgExpression);

            return instructions;
        }

        public IEnumerable<Instruction> CreateConcatCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol, UcfgExpression left,
            UcfgExpression right)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.Concatenation, syntaxNode,
                expressionService.CreateVariable(nodeTypeSymbol), left, right);
        }

        public IEnumerable<Instruction> CreateIdCall(SyntaxNode syntaxNode, UcfgExpression to, UcfgExpression value)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.Identity, syntaxNode, to, value);
        }

        public IEnumerable<Instruction> CreateAnnotateCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol,
            IMethodSymbol attributeMethodSymbol, UcfgExpression.VariableExpression target)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.Annotate, syntaxNode, expressionService.CreateVariable(nodeTypeSymbol),
                expressionService.CreateConstant(attributeMethodSymbol), target);
        }

        public IEnumerable<Instruction> CreateAnnotationCall(SyntaxNode syntaxNode, UcfgExpression.VariableExpression to, UcfgExpression value)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.Annotation, syntaxNode, to, value);
        }

        public IEnumerable<Instruction> CreateArrayGetCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol,
            UcfgExpression target)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.ArrayGet, syntaxNode, expressionService.CreateVariable(nodeTypeSymbol),
                target);
        }

        public IEnumerable<Instruction> CreateArraySetCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol,
            UcfgExpression target, UcfgExpression value)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.ArraySet, syntaxNode, expressionService.CreateVariable(nodeTypeSymbol),
                target, value);
        }

        public IEnumerable<Instruction> CreateEntryPointCall(SyntaxNode syntaxNode, ITypeSymbol nodeTypeSymbol,
            ParameterListSyntax parameterList)
        {
            return CreateFunctionCall(UcfgBuiltInMethodId.EntryPoint, syntaxNode, expressionService.CreateVariable(nodeTypeSymbol),
                parameterList.Parameters.Select(expressionService.GetExpression).ToArray());
        }

        private IEnumerable<Instruction> CreateMethodCall(SyntaxNode syntaxNode, IMethodSymbol methodSymbol, UcfgExpression target,
            params UcfgExpression[] additionalArguments)
        {
            if (target is UcfgExpression.ConstantExpression)
            {
                expressionService.Associate(syntaxNode, target);
                return NoInstructions;
            }

            return CreateFunctionCall(methodSymbol.ToUcfgMethodId(), syntaxNode,
                expressionService.CreateVariable(methodSymbol.ReturnType), new[] { target }.Concat(additionalArguments).ToArray());
        }

        private IEnumerable<Instruction> CreateFunctionCall(string methodIdentifier, SyntaxNode syntaxNode,
            UcfgExpression assignedTo, params UcfgExpression[] arguments)
        {
            if (syntaxNode is ObjectCreationExpressionSyntax)
            {
                throw new UcfgException("Expecting this method not to be called for nodes of type 'ObjectCreationExpressionSyntax'.");
            }

            if (arguments.Length == 0 &&
                methodIdentifier != UcfgBuiltInMethodId.EntryPoint)
            {
                throw new UcfgException($"Every UCFG expression  except {UcfgBuiltInMethodId.EntryPoint}  must have at least " +
                    "one argument.  " +
                    $"Identifier: {methodIdentifier},  " +
                    $"File: {syntaxNode.GetLocation()?.GetLineSpan().Path ?? "{unknown}" }  " +
                    $"Line: {syntaxNode.GetLocation()?.GetLineSpan().StartLinePosition}");
            }

            expressionService.Associate(syntaxNode, assignedTo);

            var instruction = new Instruction
            {
                Assigncall = new AssignCall
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    MethodId = methodIdentifier
                }
            };

            var processedArgs = ProcessInstructionArguments(methodIdentifier, arguments, out var newInstructions);
            instruction.Assigncall.Args.AddRange(processedArgs);

            assignedTo.ApplyAsTarget(instruction);
            newInstructions.Add(instruction);

            return newInstructions;
        }

        private IEnumerable<Expression> ProcessInstructionArguments(string methodIdentifier,
            IEnumerable<UcfgExpression> ucfgArguments, out List<Instruction> additionalInstructions)
        {
            additionalInstructions = new List<Instruction>();
            var argumentExpressions = new List<Expression>();

            // Arguments to a method can only be variable, this, class name or constant.
            // Anything else needs to be referenced by a auxiliary variable
            foreach (var ucfgExpression in ucfgArguments)
            {
                switch (ucfgExpression)
                {
                    // When a FieldExpression is passed as argument to an instruction, we need to store it in a temp variable,
                    // which will then be passed to the processed instruction
                    case UcfgExpression.FieldAccessExpression fieldExpression
                        when methodIdentifier != UcfgBuiltInMethodId.Identity:
                        {
                            var tempVariable = expressionService.CreateVariable(ucfgExpression.TypeSymbol);
                            argumentExpressions.Add(tempVariable.Expression);
                            additionalInstructions.AddRange(CreateIdCall(fieldExpression.Node, tempVariable, fieldExpression));
                        }
                        break;

                    // When a PropertyExpression is passed as argument to an instruction, we need to create the getter method
                    // call, and then pass the variable to the processed instruction
                    case UcfgExpression.PropertyAccessExpression propertyExpression:
                        {
                            additionalInstructions.AddRange(CreateMethodCall(propertyExpression.Node,
                                propertyExpression.GetMethodSymbol, propertyExpression.Target));
                            var variableExpression = expressionService.GetExpression(propertyExpression.Node);
                            argumentExpressions.Add(variableExpression.Expression);
                        }
                        break;

                    default:
                        argumentExpressions.Add(
                            ucfgExpression.Expression
                            // Defensive, in case the expression is null.
                            // This can happen e.g. if the method argument is a MemberAccessExpression
                            // that identifies the method being passed as a function (not currently handled)
                            ?? expressionService.CreateConstant().Expression);
                        break;
                }
            }

            return argumentExpressions;
        }

        private IEnumerable<Instruction> CreateNewObject(ObjectCreationExpressionSyntax syntaxNode, IMethodSymbol ctorSymbol)
        {
            var instruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    Type = expressionService.CreateClassName(ctorSymbol.ContainingType).Expression.Classname.Classname
                }
            };

            var callTarget = expressionService.CreateVariable(ctorSymbol.ReturnType);
            expressionService.Associate(syntaxNode, callTarget);
            callTarget.ApplyAsTarget(instruction);

            return new[] { instruction };
        }

        private IEnumerable<Instruction> CreateNewArray(ArrayCreationExpressionSyntax syntaxNode,
            IArrayTypeSymbol arrayTypeSymbol)
        {
            var instruction = new Instruction
            {
                NewObject = new NewObject
                {
                    Location = syntaxNode.GetUcfgLocation(),
                    Type = arrayTypeSymbol.ToDisplayString()
                }
            };

            var callTarget = expressionService.CreateVariable(arrayTypeSymbol);
            expressionService.Associate(syntaxNode, callTarget);
            callTarget.ApplyAsTarget(instruction);

            return new[] { instruction };
        }

        private ISymbol GetSymbol(SyntaxNode syntaxNode) =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol;

        private T GetSymbol<T>(SyntaxNode syntaxNode) where T : class, ISymbol =>
            semanticModel.GetSymbolInfo(syntaxNode).Symbol as T;

        private UcfgExpression[] GetAdditionalArguments(ArgumentListSyntax argumentList)
        {
            if (argumentList == null)
            {
                return new UcfgExpression[0];
            }

            return argumentList.Arguments
                .Select(a => a.Expression)
                .Select(expressionService.GetExpression)
                .ToArray();
        }
    }
}

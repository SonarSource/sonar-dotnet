/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Common.Constraints;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    internal sealed class RestrictDeserializedTypes : ISymbolicExecutionAnalyzer
    {
        internal const string DiagnosticId = "S5773";
        private const string MessageFormat = "Restrict types of objects allowed to be deserialized.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        public ISymbolicExecutionAnalysisContext AddChecks(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly List<SyntaxNode> nodes = new List<SyntaxNode>();

            public AnalysisContext(AbstractExplodedGraph explodedGraph)
            {
                explodedGraph.AddExplodedGraphCheck(new SerializationBinderCheck(explodedGraph, AddNode));
            }

            public bool SupportsPartialResults => true;

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                nodes.Distinct().Select(node => Diagnostic.Create(rule, node.GetLocation()));

            public void Dispose()
            {
                // Nothing to do here.
            }

            private void AddNode(SyntaxNode syntaxNode) => nodes.Add(syntaxNode);
        }

        private sealed class SerializationBinderCheck : ExplodedGraphCheck
        {
            private static readonly ImmutableArray<KnownType> typesWithBinder =
                ImmutableArray.Create(
                    KnownType.System_Runtime_Serialization_Formatters_Binary_BinaryFormatter,
                    KnownType.System_Runtime_Serialization_NetDataContractSerializer,
                    KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter);

            private readonly Dictionary<ITypeSymbol, bool> symbolValidityMap = new Dictionary<ITypeSymbol, bool>();

            private readonly Action<SyntaxNode> addNode;

            public SerializationBinderCheck(AbstractExplodedGraph explodedGraph, Action<SyntaxNode> addNode)
                : base(explodedGraph)
            {
                this.addNode = addNode;
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
            {
                if (instruction is ObjectCreationExpressionSyntax objectCreation)
                {
                    var typeSymbol = semanticModel.GetTypeInfo(instruction).Type;
                    if (IsFormatterWithBinder(typeSymbol) &&
                        !HasSafeBinderInitialization(objectCreation.Initializer))
                    {
                        programState = programState.SetConstraint(symbolicValue, SerializationConstraint.Unsafe);
                    }

                    if (IsJavaScriptSerializer(typeSymbol) &&
                        !HasSafeResolverInitialization(objectCreation))
                    {
                        programState = programState.SetConstraint(symbolicValue, SerializationConstraint.Unsafe);
                    }

                    if (IsLosFormatter(typeSymbol) &&
                        !IsLosFormatterSafe(objectCreation, programState))
                    {
                        // For LosFormatter the rule is raised directly on the constructor.
                        addNode(objectCreation);
                    }
                }

                return base.ObjectCreated(programState, symbolicValue, instruction);
            }

            private bool IsLosFormatterSafe(ObjectCreationExpressionSyntax objectCreation, ProgramState programState)
            {
                // The constructor is safe only if it has 2 arguments and the first argument value is true.
                if (objectCreation.ArgumentList.Arguments.Count != 2)
                {
                    return false;
                }

                var firstArgument = GetEnableMacArgumentSyntax(objectCreation.ArgumentList);
                if (firstArgument.IsKind(SyntaxKind.FalseLiteralExpression))
                {
                    return false;
                }

                if (firstArgument.IsKind(SyntaxKind.TrueLiteralExpression))
                {
                    return true;
                }

                var symbol = semanticModel.GetSymbolInfo(firstArgument).Symbol;
                if (symbol == null)
                {
                    // In this case we cannot determine if the first parameter is true or false so we
                    // assume it's true to avoid FPs.
                    return true;
                }

                var symbolicValue = programState.GetSymbolValue(symbol);
                return symbolicValue == null ||
                       !programState.HasConstraint(symbolicValue, BoolConstraint.False);
            }

            private static ExpressionSyntax GetEnableMacArgumentSyntax(BaseArgumentListSyntax list) =>
                (list.GetArgumentByName("enableMac") ?? list.Arguments[0]).Expression;

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                programState = instruction switch
                {
                    MemberAccessExpressionSyntax memberAccess => VisitMemberAccess(memberAccess, programState),
                    AssignmentExpressionSyntax assignmentExpressionSyntax => VisitAssignmentExpression(assignmentExpressionSyntax, programState),
                    _ => programState
                };

                return base.PreProcessInstruction(programPoint, programState);
            }

            private ProgramState VisitMemberAccess(MemberAccessExpressionSyntax memberAccess, ProgramState programState)
            {
                if (IsDeserializeOnKnownType(memberAccess) && programState.HasValue)
                {
                    // If Deserialize is called on an expression which returns a formatter (e.g. new BinaryFormatter().Deserialize()),
                    // the symbolic value corresponding to the returned value will be available on the top of the stack.
                    var symbolValue = programState.PeekValue();

                    if (programState.HasConstraint(symbolValue, SerializationConstraint.Unsafe))
                    {
                        addNode(memberAccess);
                    }
                }

                return programState;
            }

            private ProgramState VisitAssignmentExpression(AssignmentExpressionSyntax assignmentExpression, ProgramState programState)
            {
                if (!(assignmentExpression.Left is MemberAccessExpressionSyntax memberAccess) || !IsBinderProperty(memberAccess))
                {
                    return programState;
                }

                var typeSymbol = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (!IsFormatterWithBinder(typeSymbol))
                {
                    return programState;
                }

                var formatterSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                var formatterSymbolValue = programState.GetSymbolValue(formatterSymbol);
                if (formatterSymbolValue == null)
                {
                    return programState;
                }

                if (assignmentExpression.Right.IsNullLiteral())
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationConstraint.Unsafe);
                }

                var binderSymbol = semanticModel.GetSymbolInfo(assignmentExpression.Right).Symbol;
                var binderSymbolValue = programState.GetSymbolValue(binderSymbol);
                if (binderSymbolValue != null &&
                    programState.HasConstraint(binderSymbolValue, ObjectConstraint.Null))
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationConstraint.Unsafe);
                }

                var binderType = semanticModel.GetTypeInfo(assignmentExpression.Right).Type;
                var constraint = IsTypeSafe(binderType)
                    ? SerializationConstraint.Safe
                    : SerializationConstraint.Unsafe;

                return programState.SetConstraint(formatterSymbolValue, constraint);
            }

            private bool HasSafeResolverInitialization(ObjectCreationExpressionSyntax objectCreation)
            {
                // JavaScriptSerializer has 2 constructors:
                // - JavaScriptSerializer(): unsafe since it doesn't give the option to set a provider
                // - JavaScriptSerializer(JavaScriptTypeResolver): this one is safe only if the given resolver is safe
                // See: https://docs.microsoft.com/en-us/dotnet/api/system.web.script.serialization.javascriptserializer.-ctor?view=netframework-4.8

                if (objectCreation.ArgumentList.Arguments.Count == 0)
                {
                    return false;
                }

                var resolverType = objectCreation.ArgumentList.Arguments[0];
                if (resolverType.Expression.IsNullLiteral())
                {
                    return false;
                }

                return semanticModel.GetTypeInfo(resolverType.Expression).Type is {} typeSymbol &&
                       !typeSymbol.Is(KnownType.System_Web_Script_Serialization_SimpleTypeResolver) &&
                       IsTypeSafe(typeSymbol);
            }

            private bool HasSafeBinderInitialization(InitializerExpressionSyntax initializer)
            {
                var binderAssignment = initializer?.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .SingleOrDefault(assignment => IsBinderProperty(assignment.Left));

                if (binderAssignment == null ||
                    binderAssignment.Right.IsNullLiteral())
                {
                    return false;
                }

                return semanticModel.GetTypeInfo(binderAssignment.Right).Type is {} typeSymbol &&
                       IsTypeSafe(typeSymbol);
            }

            private bool IsTypeSafe(ITypeSymbol symbol) =>
                symbolValidityMap.GetOrAdd(symbol, typeSymbol =>
                {
                    var declaration = typeSymbol.DerivesFrom(KnownType.System_Web_Script_Serialization_JavaScriptTypeResolver)
                        ? GetResolveTypeMethodDeclaration(typeSymbol)
                        : GetBindToTypeMethodDeclaration(typeSymbol);

                    // Binders and resolvers are considered safe by default (e.g. if the declaration cannot be found).
                    return declaration == null || declaration.ThrowsOrReturnsNull();
                });

            private static MethodDeclarationSyntax GetBindToTypeMethodDeclaration(ISymbol symbol) =>
                symbol.DeclaringSyntaxReferences
                    .SelectMany(GetDescendantNodes)
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsBindToType);

            private static MethodDeclarationSyntax GetResolveTypeMethodDeclaration(ISymbol symbol) =>
                symbol.DeclaringSyntaxReferences
                    .SelectMany(GetDescendantNodes)
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsResolveType);

            private static IEnumerable<SyntaxNode> GetDescendantNodes(SyntaxReference syntaxReference) =>
                syntaxReference.GetSyntax().DescendantNodes();

            private static bool IsBindToType(MethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.Identifier.Text == "BindToType" &&
                methodDeclaration.ReturnType.NameIs("Type") &&
                methodDeclaration.ParameterList.Parameters.Count == 2 &&
                methodDeclaration.ParameterList.Parameters[0].IsString() &&
                methodDeclaration.ParameterList.Parameters[1].IsString();

            private static bool IsResolveType(MethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.Identifier.Text == "ResolveType" &&
                methodDeclaration.ReturnType.NameIs("Type") &&
                methodDeclaration.ParameterList.Parameters.Count == 1 &&
                methodDeclaration.ParameterList.Parameters[0].IsString();

            private bool IsDeserializeOnKnownType(MemberAccessExpressionSyntax memberAccess) =>
                IsDeserializeMethod(memberAccess) &&
                semanticModel.GetTypeInfo(memberAccess.Expression).Type is {} typeSymbol &&
                (IsFormatterWithBinder(typeSymbol) || IsJavaScriptSerializer(typeSymbol));

            private static bool IsFormatterWithBinder(ITypeSymbol typeSymbol) =>
                typeSymbol.IsAny(typesWithBinder);

            private static bool IsBinderProperty(ExpressionSyntax expression) =>
                expression.NameIs("Binder");

            private static bool IsDeserializeMethod(ExpressionSyntax expression) =>
                expression.NameIs("Deserialize");

            private static bool IsJavaScriptSerializer(ITypeSymbol typeSymbol) =>
                typeSymbol.Is(KnownType.System_Web_Script_Serialization_JavaScriptSerializer);

            private static bool IsLosFormatter(ITypeSymbol typeSymbol) =>
                typeSymbol.Is(KnownType.System_Web_UI_LosFormatter);
        }
    }
}

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
                nodes.Select(node => Diagnostic.Create(rule, node.GetLocation()));

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
                    KnownType.System_Runtime_Serialization_Formatters_Soap_SoapFormatter,
                    KnownType.System_Web_UI_ObjectStateFormatter);

            private readonly Dictionary<ITypeSymbol, bool> binderValidityMap = new Dictionary<ITypeSymbol, bool>();

            private readonly Action<SyntaxNode> addNode;

            public SerializationBinderCheck(AbstractExplodedGraph explodedGraph, Action<SyntaxNode> addNode)
                : base(explodedGraph)
            {
                this.addNode = addNode ?? throw new ArgumentNullException(nameof(addNode));
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue,
                SyntaxNode instruction)
            {
                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var typeSymbol = semanticModel.GetTypeInfo(instruction).Type;
                    if (IsFormatterWithBinder(typeSymbol))
                    {
                        // These types are marked as invalid on creation since they are insecure by default.
                        programState = SetUnsafeSerializationBinderConstraint(instruction, symbolicValue, programState);
                    }
                }

                return base.ObjectCreated(programState, symbolicValue, instruction);
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                programState = instruction switch
                {
                    InvocationExpressionSyntax invocation => VisitInvocationExpression(invocation, programState),
                    AssignmentExpressionSyntax assignmentExpressionSyntax => VisitAssignmentExpression(assignmentExpressionSyntax, programState),
                    _ => programState
                };

                return base.PreProcessInstruction(programPoint, programState);
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax invocation, ProgramState programState)
            {
                if (!IsDeserializeMethod(invocation))
                {
                    return programState;
                }

                var formatterSymbol = semanticModel.GetSymbolInfo(invocation).Symbol.ContainingSymbol;
                if (IsFormatterWithBinder(formatterSymbol.GetSymbolType()))
                {
                    var formatterSymbolValue = programState.GetSymbolValue(formatterSymbol);

                    if (programState.HasConstraint(formatterSymbolValue, SerializationBinder.Unsafe))
                    {
                        addNode(invocation);
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

                var binderSymbol = semanticModel.GetSymbolInfo(assignmentExpression.Right).Symbol.ContainingType;
                var binderSymbolValue = programState.GetSymbolValue(binderSymbol);
                if (binderSymbolValue != null &&
                    programState.HasConstraint(binderSymbolValue, ObjectConstraint.Null))
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationBinder.Unsafe);
                }

                var constraint = IsBinderSafe(assignmentExpression.Right, binderSymbol)
                    ? SerializationBinder.Safe
                    : SerializationBinder.Unsafe;

                return programState.SetConstraint(formatterSymbolValue, constraint);
            }

            private ProgramState SetUnsafeSerializationBinderConstraint(SyntaxNode instruction,
                SymbolicValue symbolicValue, ProgramState programState)
            {
                var symbol = semanticModel.GetSymbolInfo(instruction).Symbol.ContainingSymbol;

                programState = programState.StoreSymbolicValue(symbol, symbolicValue);

                return symbol.SetConstraint(SerializationBinder.Unsafe, programState);
            }

            private bool IsBinderSafe(SyntaxNode instruction, ITypeSymbol symbol) =>
                binderValidityMap.GetOrAdd(symbol, typeSymbol =>
                {
                    var declaration = GetBindToTypeMethodDeclaration(instruction);

                    // The binder is considered safe by default (e.g. if the declaration cannot be found).
                    return declaration == null || declaration.ThrowsOrReturnsNull();
                });

            private MethodDeclarationSyntax GetBindToTypeMethodDeclaration(SyntaxNode instruction) =>
                semanticModel.GetSymbolInfo(instruction)
                    .Symbol
                    .ContainingSymbol
                    .DeclaringSyntaxReferences
                    .SelectMany(GetDescendantNodes)
                    .OfType<MethodDeclarationSyntax>()
                    .FirstOrDefault(IsBindToType);

            private static IEnumerable<SyntaxNode> GetDescendantNodes(SyntaxReference syntaxReference) =>
                syntaxReference.SyntaxTree.GetRoot().FindNode(syntaxReference.Span).DescendantNodes();

            private static bool IsBindToType(MethodDeclarationSyntax methodDeclaration) =>
                methodDeclaration.Identifier.Text == "BindToType" &&
                methodDeclaration.ReturnType.NameIs("Type") &&
                methodDeclaration.ParameterList.Parameters.Count == 2 &&
                methodDeclaration.ParameterList.Parameters[0].IsString() &&
                methodDeclaration.ParameterList.Parameters[1].IsString();

            private static bool IsFormatterWithBinder(ITypeSymbol typeSymbol) =>
                typeSymbol.IsAny(typesWithBinder);

            private static bool IsBinderProperty(ExpressionSyntax memberAccess) =>
                memberAccess.NameIs("Binder");

            private static bool IsDeserializeMethod(InvocationExpressionSyntax invocation) =>
                invocation.Expression.NameIs("Deserialize");

            // - Done: add symbolic value constraint when serializers are created
            // - Done: when Deserialize is called check the binder status
            // - Done: check if the binder is valid

            // ToDo:
            // - in progress: when binder is created add constraint (safe or unsafe)
            // - on property set update binder status
            //      - check if the binder exists and is not null
            // - add test cases
            // - add integration tests
            // - add handling for null coalescence
        }
    }
}

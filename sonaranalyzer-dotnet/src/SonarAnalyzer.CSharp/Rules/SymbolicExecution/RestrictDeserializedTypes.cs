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
                if (instruction is ObjectCreationExpressionSyntax objectCreation)
                {
                    var typeSymbol = semanticModel.GetTypeInfo(instruction).Type;
                    if (IsFormatterWithBinder(typeSymbol) &&
                        !HasSafeBinderInitialization(objectCreation.Initializer))
                    {
                        programState = programState.SetConstraint(symbolicValue, SerializationBinder.Unsafe);
                    }
                }

                return base.ObjectCreated(programState, symbolicValue, instruction);
            }

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
                if (IsFormatterDeserialize(memberAccess) && programState.HasValue)
                {
                    // If Deserialize is called on an expression which returns a formatter (e.g. new BinaryFormatter().Deserialize()),
                    // the symbolic value corresponding to the returned value will be available on the top of the stack.
                    var symbolValue = programState.PeekValue();

                    if (programState.HasConstraint(symbolValue, SerializationBinder.Unsafe))
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

                var binderSymbol = semanticModel.GetSymbolInfo(assignmentExpression.Right).Symbol;
                var binderSymbolValue = programState.GetSymbolValue(binderSymbol);
                if (binderSymbolValue != null &&
                    programState.HasConstraint(binderSymbolValue, ObjectConstraint.Null))
                {
                    // The formatter is considered unsafe if the binder is null.
                    return programState.SetConstraint(formatterSymbolValue, SerializationBinder.Unsafe);
                }

                var binderType = semanticModel.GetTypeInfo(assignmentExpression.Right).Type;
                var constraint = IsBinderSafe(binderType)
                    ? SerializationBinder.Safe
                    : SerializationBinder.Unsafe;

                return programState.SetConstraint(formatterSymbolValue, constraint);
            }

            private bool HasSafeBinderInitialization(InitializerExpressionSyntax initializer)
            {
                var binderAssignment = initializer?.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .SingleOrDefault(assignment => IsBinderProperty(assignment.Left));

                if (binderAssignment == null)
                {
                    return false;
                }

                var typeSymbol = semanticModel.GetTypeInfo(binderAssignment.Right).Type;
                return typeSymbol != null && IsBinderSafe(typeSymbol);
            }

            private bool IsBinderSafe(ITypeSymbol symbol) =>
                binderValidityMap.GetOrAdd(symbol, typeSymbol =>
                {
                    var declaration = GetBindToTypeMethodDeclaration(typeSymbol);

                    // The binder is considered safe by default (e.g. if the declaration cannot be found).
                    return declaration == null || declaration.ThrowsOrReturnsNull();
                });

            private static MethodDeclarationSyntax GetBindToTypeMethodDeclaration(ISymbol symbol) =>
                symbol.DeclaringSyntaxReferences
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

            private bool IsFormatterDeserialize(MemberAccessExpressionSyntax memberAccess) =>
                IsDeserializeMethod(memberAccess) &&
                semanticModel.GetTypeInfo(memberAccess.Expression).Type is {} typeSymbol &&
                IsFormatterWithBinder(typeSymbol);

            private static bool IsFormatterWithBinder(ITypeSymbol typeSymbol) =>
                typeSymbol.IsAny(typesWithBinder);

            private static bool IsBinderProperty(ExpressionSyntax expression) =>
                expression.NameIs("Binder");

            private static bool IsDeserializeMethod(ExpressionSyntax expression) =>
                expression.NameIs("Deserialize");
        }
    }
}

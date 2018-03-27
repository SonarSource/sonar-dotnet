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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.ControlFlowGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    public sealed class ObjectsShouldNotBeDisposedMoreThanOnce : ISymbolicExecutionAnalyzerFactory
    {
        internal const string DiagnosticId = "S3966";
        private const string MessageFormat = "Refactor this code to make sure '{0}' is disposed only once.";

        private static readonly ISet<KnownType> typesDisposingUnderlyingStream = new HashSet<KnownType>
            {
                KnownType.System_IO_StreamReader,
                KnownType.System_IO_StreamWriter
            };

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        ISymbolicExecutionAnalyzer ISymbolicExecutionAnalyzerFactory.Create(CSharpExplodedGraph explodedGraph) =>
            new SymbolicExecutionAnalyzer(explodedGraph);

        bool ISymbolicExecutionAnalyzerFactory.IsEnabled(SyntaxNodeAnalysisContext context) => true;

        private sealed class SymbolicExecutionAnalyzer : ISymbolicExecutionAnalyzer
        {
            private readonly List<Diagnostic> diagnostics = new List<Diagnostic>();

            private readonly ObjectDisposedPointerCheck check;

            public SymbolicExecutionAnalyzer(CSharpExplodedGraph explodedGraph)
            {
                check = explodedGraph.GetOrAddCheck(() => new ObjectDisposedPointerCheck(explodedGraph));
                check.ObjectDisposed += ObjectDisposedHandler;
            }

            public void Dispose()
            {
                check.ObjectDisposed -= ObjectDisposedHandler;
            }

            public IEnumerable<Diagnostic> Diagnostics => diagnostics;

            private void ObjectDisposedHandler(object sender, ObjectDisposedEventArgs args)
            {
                diagnostics.Add(Diagnostic.Create(rule, args.Location, args.Name));
            }
        }

        internal class ObjectDisposedEventArgs : EventArgs
        {
            public string Name { get; }
            public Location Location { get; }

            public ObjectDisposedEventArgs(string name, Location location)
            {
                Name = name;
                Location = location;
            }
        }

        internal sealed class ObjectDisposedPointerCheck : ExplodedGraphCheck
        {
            public event EventHandler<ObjectDisposedEventArgs> ObjectDisposed;

            public ObjectDisposedPointerCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            public override ProgramState PreProcessUsingStatement(ProgramPoint programPoint, ProgramState programState)
            {
                var newProgramState = programState;

                var usingFinalizer = (UsingEndBlock)programPoint.Block;

                var disposables = usingFinalizer.Identifiers
                    .Select(i =>
                    new
                    {
                        SyntaxNode = i.Parent,
                        Symbol = semanticModel.GetDeclaredSymbol(i.Parent)
                            ?? semanticModel.GetSymbolInfo(i.Parent).Symbol
                    });

                foreach (var disposable in disposables)
                {
                    newProgramState = ProcessDisposableSymbol(newProgramState, disposable.SyntaxNode, disposable.Symbol);
                }

                newProgramState = ProcessStreamDisposingTypes(newProgramState,
                    (SyntaxNode)usingFinalizer.UsingStatement.Expression ?? usingFinalizer.UsingStatement.Declaration);

                return newProgramState;
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset] as InvocationExpressionSyntax;

                return instruction == null
                    ? programState
                    : VisitInvocationExpression(instruction, programState);
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax instruction, ProgramState programState)
            {
                var newProgramState = programState;

                var disposeMethodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                if (disposeMethodSymbol.IsIDisposableDispose())
                {
                    var disposedObject =
                        // Direct call to Dispose()
                        instruction.Expression as IdentifierNameSyntax
                        // Call to Dispose on local variable, field or this
                        ?? (instruction.Expression as MemberAccessExpressionSyntax)?.Expression;
                    if (disposedObject != null)
                    {
                        var disposableSymbol = semanticModel.GetSymbolInfo(disposedObject).Symbol;

                        if (disposableSymbol is IMethodSymbol ||
                            // Special case - if the parameter symbol is "this" then resolve it to the containing type
                            (disposableSymbol is IParameterSymbol parameter && parameter.IsThis))
                        {
                            disposableSymbol = disposableSymbol.ContainingType;
                        }
                        newProgramState = ProcessDisposableSymbol(newProgramState, disposedObject, disposableSymbol);
                    }
                }

                return newProgramState;
            }

            private ProgramState ProcessStreamDisposingTypes(ProgramState programState, SyntaxNode usingExpression)
            {
                var newProgramState = programState;

                var arguments = usingExpression.DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>()
                    .Where(IsStreamDisposingType)
                    .Select(FirstArgumentOrDefault)
                    .WhereNotNull();

                foreach (var argument in arguments)
                {
                    var streamSymbol = semanticModel.GetSymbolInfo(argument.Expression).Symbol;
                    newProgramState = ProcessDisposableSymbol(newProgramState, argument.Expression, streamSymbol);
                }

                return newProgramState;
            }

            private ProgramState ProcessDisposableSymbol(ProgramState programState, SyntaxNode disposeInstruction,
                ISymbol disposableSymbol)
            {
                if (disposableSymbol == null) // DisposableSymbol is null when we invoke an array element
                {
                    return programState;
                }

                if (disposableSymbol.HasConstraint(DisposableConstraint.Disposed, programState))
                {
                    ObjectDisposed?.Invoke(this, new ObjectDisposedEventArgs(disposableSymbol.Name,
                        disposeInstruction.GetLocation()));
                    return programState;
                }

                // We should not replace Null constraint because having Disposed constraint
                // implies having NotNull constraint, which is incorrect.
                if (disposableSymbol.HasConstraint(ObjectConstraint.Null, programState))
                {
                    return programState;
                }

                var newProgramState = programState;
                if (disposableSymbol is INamedTypeSymbol &&
                    newProgramState.GetSymbolValue(disposableSymbol) == null)
                {
                    // Dispose is called on current instance but we don't usually store a symbol for this
                    // so we store it and then associate the Disposed constraint.
                    newProgramState = newProgramState.StoreSymbolicValue(disposableSymbol, SymbolicValue.This);
                }
                return disposableSymbol.SetConstraint(DisposableConstraint.Disposed, newProgramState);
            }

            private static ArgumentSyntax FirstArgumentOrDefault(ObjectCreationExpressionSyntax objectCreation) =>
                objectCreation.ArgumentList?.Arguments.FirstOrDefault();

            private bool IsStreamDisposingType(ObjectCreationExpressionSyntax objectCreation) =>
                semanticModel.GetSymbolInfo(objectCreation.Type)
                    .Symbol
                    .GetSymbolType()
                    .DerivesOrImplementsAny(typesDisposingUnderlyingStream);
        }
    }
}

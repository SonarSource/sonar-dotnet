/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph.CSharp;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ObjectsShouldNotBeDisposedMoreThanOnce : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3966";
        private const string MessageFormat = "Refactor this code to make sure '{0}' is disposed only once.";
        private const string LeaveOpenParameterName = "leaveOpen";
        private const string StreamParameterName = "stream";

        private static readonly ImmutableDictionary<KnownType, int> typesDisposingUnderlyingStreamWithLeaveOpenArgumentIndex =
            ImmutableDictionary<KnownType, int>.Empty
                .Add(KnownType.System_IO_StreamReader, 4)
                .Add(KnownType.System_IO_StreamWriter, 3);

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            //FIXME: Temporary silence for CFG defork
            //context.RegisterExplodedGraphBasedAnalysis(CheckForMultipleDispose);
        }

        private static void CheckForMultipleDispose(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var objectDisposedCheck = new ObjectDisposedPointerCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(objectDisposedCheck);

            // Store the nodes that should be reported and ignore duplicate reports for the same node.
            // This is needed because we generate two CFG blocks for the finally statements and even
            // though the syntax nodes are the same, when there is a return inside a try/catch block
            // the walked CFG paths could be different and FPs will appear.
            var nodesToReport = new Dictionary<SyntaxNode, string>();

            void memberAccessedHandler(object sender, ObjectDisposedEventArgs args)
            {
                nodesToReport[args.SyntaxNode] = args.SymbolName;
            }

            objectDisposedCheck.ObjectDisposed += memberAccessedHandler;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                objectDisposedCheck.ObjectDisposed -= memberAccessedHandler;
            }

            foreach (var item in nodesToReport)
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, item.Key.GetLocation(), item.Value));
            }
        }

        internal class ObjectDisposedEventArgs : EventArgs
        {
            public string SymbolName { get; }
            public SyntaxNode SyntaxNode { get; }

            public ObjectDisposedEventArgs(string symbolName, SyntaxNode syntaxNode)
            {
                SymbolName = symbolName;
                SyntaxNode = syntaxNode;
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
                        Symbol = this.semanticModel.GetDeclaredSymbol(i.Parent)
                            ?? this.semanticModel.GetSymbolInfo(i.Parent).Symbol
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

                return !(programPoint.Block.Instructions[programPoint.Offset] is InvocationExpressionSyntax instruction)
                    ? programState
                    : VisitInvocationExpression(instruction, programState);
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax instruction, ProgramState programState)
            {
                var newProgramState = programState;

                var disposeMethodSymbol = this.semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                if (disposeMethodSymbol.IsIDisposableDispose())
                {
                    var disposedObject =
                        // Direct call to Dispose()
                        instruction.Expression as IdentifierNameSyntax
                        // Call to Dispose on local variable, field or this
                        ?? (instruction.Expression as MemberAccessExpressionSyntax)?.Expression;
                    if (disposedObject != null)
                    {
                        var disposableSymbol = this.semanticModel.GetSymbolInfo(disposedObject).Symbol;

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
                var objectCreationExpressions = usingExpression.DescendantNodes()
                    .OfType<ObjectCreationExpressionSyntax>();

                foreach (var objectCreationExpression in objectCreationExpressions)
                {
                    if (this.semanticModel.GetSymbolInfo(objectCreationExpression.Type).Symbol is ISymbol symbol &&
                        symbol.GetSymbolType() is ITypeSymbol objectType &&
                        objectType.DerivesOrImplementsAny(typesDisposingUnderlyingStreamWithLeaveOpenArgumentIndex.Keys.ToImmutableArray()) &&
                        objectCreationExpression.ArgumentList?.Arguments is IEnumerable<ArgumentSyntax> argumentList &&
                        argumentList.Any())
                    {
                        // Checks for the 'leaveOpen' argument. It is at position 4 if present for StreamReader constructor, while it is at position 3 for StreamWriter.
                        // See #2491 for more details
                        var leaveOpenArgumentIndex = typesDisposingUnderlyingStreamWithLeaveOpenArgumentIndex
                            .Where(entry => objectType.DerivesOrImplements(entry.Key))
                            .Select(entry => entry.Value)
                            .First();
                        if (!HasLeaveOpenTrueArgument(argumentList, leaveOpenArgumentIndex) &&
                            GetArgument(argumentList, StreamParameterName, 0) is ArgumentSyntax argument)
                        {
                            var streamSymbol = this.semanticModel.GetSymbolInfo(argument.Expression).Symbol;
                            newProgramState = ProcessDisposableSymbol(newProgramState, argument.Expression, streamSymbol);
                        }
                    }
                }

                return newProgramState;
            }

            private static bool HasLeaveOpenTrueArgument(IEnumerable<ArgumentSyntax> argumentList, int leaveOpenArgumentIndex) =>
                GetArgument(argumentList, LeaveOpenParameterName, leaveOpenArgumentIndex) is ArgumentSyntax leaveOpenArgument &&
                CSharpEquivalenceChecker.AreEquivalent(leaveOpenArgument.Expression, CSharpSyntaxHelper.TrueLiteralExpression);

            private static ArgumentSyntax GetArgument(IEnumerable<ArgumentSyntax> argumentList, string argumentName, int defaultArgumentIndex)
            {
                var argument = argumentList.FirstOrDefault(arg => argumentName.Equals(arg.NameColon?.Name.Identifier.ValueText));
                if (argument == null)
                {
                    return argumentList.ElementAtOrDefault(defaultArgumentIndex);
                }
                return argument;
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
                    ObjectDisposed?.Invoke(this, new ObjectDisposedEventArgs(disposableSymbol.Name, disposeInstruction));
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
        }
    }
}

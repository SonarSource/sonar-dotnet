/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;
using SonarAnalyzer.Helpers.FlowAnalysis.CSharp;
using ExplodedGraph = SonarAnalyzer.Helpers.FlowAnalysis.CSharp.ExplodedGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ObjectsShouldNotBeDisposedMoreThanOnce : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3966";
        private const string MessageFormat = "Refactor this code to make sure Dispose is only called once on '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckForNullDereference(e, c));
        }

        private static void CheckForNullDereference(ExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var objectDisposedCheck = new ObjectDisposedPointerCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(objectDisposedCheck);

            var nullIdentifiers = new HashSet<IdentifierNameSyntax>();

            EventHandler<ObjectDisposedEventArgs> memberAccessedHandler =
                (sender, args) =>
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, args.DisposableIdentifier.GetLocation(), args.DisposableIdentifier.GetText()));
                };

            objectDisposedCheck.ObjectDisposed += memberAccessedHandler;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                objectDisposedCheck.ObjectDisposed -= memberAccessedHandler;
            }
        }

        internal class ObjectDisposedEventArgs : EventArgs
        {
            public SyntaxNode DisposableIdentifier { get; }

            public ObjectDisposedEventArgs(SyntaxNode disposableIdentifier)
            {
                DisposableIdentifier = disposableIdentifier;
            }
        }

        internal sealed class ObjectDisposedPointerCheck : ExplodedGraphCheck
        {
            public event EventHandler<ObjectDisposedEventArgs> ObjectDisposed;

            public ObjectDisposedPointerCheck(ExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];
                switch (instruction.Kind())
                {
                    case SyntaxKind.InvocationExpression:
                        return VisitInvocationExpression((InvocationExpressionSyntax)instruction, programState);

                    default:
                        return programState;
                }
            }

            public override ProgramState PreProcessUsingStatement(ProgramPoint programPoint, ProgramState programState)
            {
                var newProgramState = programState;

                var usingFinalizer = (UsingFinalizerBlock)programPoint.Block;

                var disposables = usingFinalizer.Disposables
                    .Select(disposable =>
                    new
                    {
                        Disposable = disposable,
                        Symbol = semanticModel.GetDeclaredSymbol(disposable)
                            ?? semanticModel.GetSymbolInfo(disposable).Symbol
                    });

                foreach (var nodeWithSymbol in disposables)
                {
                    newProgramState = ProcessDisposableSymbol(newProgramState, nodeWithSymbol.Disposable, nodeWithSymbol.Symbol);
                }

                newProgramState = ProcessKnownTypes(newProgramState, (SyntaxNode)usingFinalizer.UsingStatement.Expression ?? usingFinalizer.UsingStatement.Declaration);

                return newProgramState;
            }

            private ProgramState ProcessKnownTypes(ProgramState programState, SyntaxNode usingExpression)
            {
                ISet<KnownType> types = new HashSet<KnownType>
                {
                    KnownType.System_IO_StreamReader,
                    KnownType.System_IO_StreamWriter
                };
                var newProgramState = programState;

                var objectCreations = usingExpression.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                    .Where(objectCreation => semanticModel.GetSymbolInfo(objectCreation.Type).Symbol.GetSymbolType().DerivesOrImplementsAny(types))
                    .Select(objectCreation => objectCreation.ArgumentList?.Arguments.FirstOrDefault())
                    .WhereNotNull();

                foreach (var argument in objectCreations)
                {
                    var streamSymbol = semanticModel.GetSymbolInfo(argument.Expression).Symbol;
                    newProgramState = ProcessDisposableSymbol(newProgramState, argument.Expression, streamSymbol);
                }

                return newProgramState;
            }

            private ProgramState VisitInvocationExpression(InvocationExpressionSyntax instruction, ProgramState programState)
            {
                var newProgramState = programState;

                var disposeMethodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                if (disposeMethodSymbol.IsIDisposableDispose())
                {
                    var disposable = ((MemberAccessExpressionSyntax)instruction.Expression).Expression;

                    var disposableSymbol = semanticModel.GetSymbolInfo(disposable).Symbol;

                    newProgramState = ProcessDisposableSymbol(newProgramState, disposable, disposableSymbol);
                }

                return newProgramState;
            }

            private ProgramState ProcessDisposableSymbol(ProgramState programState, SyntaxNode instruction, ISymbol disposableSymbol)
            {
                if (disposableSymbol == null) // Temporary fix, disposableSymbol is null when we invoke array element
                {
                    return programState;
                }

                if (disposableSymbol.HasConstraint(DisposableConstraint.Disposed, programState))
                {
                    ObjectDisposed?.Invoke(this, new ObjectDisposedEventArgs(instruction));
                    return programState;
                }

                if (disposableSymbol.HasConstraint(ObjectConstraint.Null, programState))
                {
                    return programState;
                }

                return disposableSymbol.SetConstraint(DisposableConstraint.Disposed, programState);
            }
        }
    }
}

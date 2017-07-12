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
        private const string MessageFormat = "";

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
                    context.ReportDiagnostic(Diagnostic.Create(rule, args.DisposableIdentifier.GetLocation()));
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
                        return ProcessMemberAccess(programState, (InvocationExpressionSyntax)instruction);

                    case SyntaxKind.UsingStatement:
                        return ProcessUsingStatement(programState, (UsingStatementSyntax)instruction);

                    default:
                        return programState;
                }
            }

            private ProgramState ProcessUsingStatement(ProgramState programState, UsingStatementSyntax instruction)
            {
                var newProgramState = programState;
                return newProgramState;
            }

            private ProgramState ProcessMemberAccess(ProgramState programState, InvocationExpressionSyntax instruction)
            {
                var newProgramState = programState;

                var disposeMethodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                if (disposeMethodSymbol.IsIDisposableDispose())
                {
                    var disposable = ((MemberAccessExpressionSyntax)instruction.Expression).Expression;
                    var disposableSymbol = semanticModel.GetSymbolInfo(disposable).Symbol;

                    if (disposableSymbol == null) // Temporary fix, disposableSymbol is null when we invoke array element
                    {
                        return newProgramState;
                    }

                    newProgramState = ProcessDisposableSymbol(newProgramState, disposableSymbol, instruction);
                }

                return newProgramState;
            }

            private ProgramState ProcessDisposableSymbol(ProgramState programState, ISymbol disposableSymbol, 
                InvocationExpressionSyntax instruction)
            {
                if (disposableSymbol.HasConstraint(DisposableConstraint.Disposed, programState))
                {
                    ObjectDisposed?.Invoke(this, new ObjectDisposedEventArgs(instruction));
                    return programState;
                }
                else
                {
                    return disposableSymbol.SetConstraint(DisposableConstraint.Disposed, programState);
                }
            }
        }
    }
}

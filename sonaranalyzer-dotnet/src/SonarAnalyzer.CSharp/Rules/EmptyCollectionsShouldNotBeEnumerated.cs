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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyCollectionsShouldNotBeEnumerated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4158";
        private const string MessageFormat = "";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis(CheckForEmptyCollectionAccess);
        }

        private void CheckForEmptyCollectionAccess(ExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var check = new EmptyCollectionAccessedCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(check);


            EventHandler<CollectionAccessedEventArgs> collectionAccessedHandler =
                (sender, args) => context.ReportDiagnostic(Diagnostic.Create(rule, args.iden);

            ////check.CollectionAccessed += collectionAccessedHandler;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                ////check.CollectionAccessed -= memberAccessedHandler;
            }

            ////foreach (var nullIdentifier in nullIdentifiers)
            ////{
            ////    context.ReportDiagnostic(Diagnostic.Create(rule, nullIdentifier.GetLocation(), nullIdentifier.Identifier.ValueText));
            ////}
        }


        internal sealed class EmptyCollectionAccessedCheck : ExplodedGraphCheck
        {
            public event EventHandler<CollectionAccessedEventArgs> CollectionAccessed;

            public EmptyCollectionAccessedCheck(ExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            private void OnCollectionAccessed(SyntaxToken identifier)
            {
                CollectionAccessed?.Invoke(this, new CollectionAccessedEventArgs(identifier));
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                switch (instruction.Kind())
                {
                    case SyntaxKind.InvocationExpression:
                        return ProcessInvocation(programState, (InvocationExpressionSyntax)instruction);
                    default:
                        return programState;
                }
            }

            private ProgramState ProcessInvocation(ProgramState programState, InvocationExpressionSyntax instruction)
            {
                var newProgramState = programState;

                var memberAccess = instruction.Expression as MemberAccessExpressionSyntax;
                if (memberAccess != null)
                {
                    var collectionSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                    var collectionType = (collectionSymbol.GetSymbolType() as INamedTypeSymbol)?.ConstructedFrom;
                    if (collectionType.IsAny(CollectionTypes))
                    {
                        var methodSymbol = semanticModel.GetSymbolInfo(instruction).Symbol as IMethodSymbol;
                        if (AccessMethods.Any(m => m(methodSymbol)) &&
                            collectionSymbol.HasConstraint(CollectionConstraint.Empty, programState))
                        {
                            OnCollectionAccessed(memberAccess.Name.Identifier);
                        }
                        else if (AddMethods.Any(m => m(methodSymbol)))
                        {
                            newProgramState = methodSymbol.SetConstraint(CollectionConstraint.NotEmpty, newProgramState);
                        }
                        else
                        {
                            // do nothing
                        }
                    }
                }
                return newProgramState;
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
            {
                switch (instruction.Kind())
                {
                    case SyntaxKind.ObjectCreationExpression:
                        var collectionSymbol = semanticModel.GetSymbolInfo(instruction).Symbol;
                        var collectionType = collectionSymbol.ContainingType?.ConstructedFrom;
                        if (collectionType.IsAny(CollectionTypes))
                        {
                            return symbolicValue.SetConstraint(CollectionConstraint.Empty, programState);
                        }
                        return programState;
                    case SyntaxKind.ArrayCreationExpression:
                        ////var arrayCreation = (ArrayCreationExpressionSyntax)instruction;
                        ////var rankSpecifiers = arrayCreation.Type.RankSpecifiers;
                        ////if (rankSpecifiers != null && 
                        ////    rankSpecifiers.Count > 0 &&
                        ////    rankSpecifiers[0].le)
                        ////{
                        ////}
                    default:
                        return programState;
                }
            }

            private static readonly ISet<KnownType> CollectionTypes = new HashSet<KnownType>
            {
                KnownType.System_Collections_Generic_List_T
            };

            private static readonly ISet<Func<IMethodSymbol, bool>> AddMethods = new HashSet<Func<IMethodSymbol, bool>>
            {
                symbol => symbol.Name == nameof(ICollection<object>.Add),
            };

            private static readonly ISet<Func<IMethodSymbol, bool>> AccessMethods = new HashSet<Func<IMethodSymbol, bool>>
            {
                symbol => symbol.Name == nameof(ICollection<object>.Contains),
                symbol => symbol.Name == nameof(ICollection<object>.Remove),
            };
        }

        internal sealed class CollectionAccessedEventArgs : EventArgs
        {
            public SyntaxToken Identifier { get; }

            public CollectionAccessedEventArgs(SyntaxToken identifier)
            {
                this.Identifier = identifier;
            }
        }
    }
}

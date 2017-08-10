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
                (sender, args) => context.ReportDiagnostic(Diagnostic.Create(rule, args.Location));

            check.CollectionAccessed += collectionAccessedHandler;
            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                check.CollectionAccessed -= collectionAccessedHandler;
            }
        }


        internal sealed class EmptyCollectionAccessedCheck : ExplodedGraphCheck
        {
            public event EventHandler<CollectionAccessedEventArgs> CollectionAccessed;

            public EmptyCollectionAccessedCheck(ExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            private void OnCollectionAccessed(Location location)
            {
                CollectionAccessed?.Invoke(this, new CollectionAccessedEventArgs(location));
            }

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.Block.Instructions[programPoint.Offset];

                switch (instruction.Kind())
                {
                    case SyntaxKind.InvocationExpression:
                        return ProcessInvocation(programState, (InvocationExpressionSyntax)instruction);
                    case SyntaxKind.ElementAccessExpression:
                        return ProcessElementAccess(programState, (ElementAccessExpressionSyntax)instruction);
                    default:
                        return programState;
                }
            }

            private ProgramState ProcessElementAccess(ProgramState programState, ElementAccessExpressionSyntax instruction)
            {
                var newProgramState = programState;

                var collectionVariableSymbol = semanticModel.GetSymbolInfo(instruction.Expression).Symbol;
                if (collectionVariableSymbol == null)
                {
                    return newProgramState;
                }

                var collectionType = collectionVariableSymbol.GetSymbolType() as INamedTypeSymbol;
                if (collectionType?.ConstructedFrom != null &&
                    collectionType.ConstructedFrom.IsAny(CollectionTypes))
                {
                    if (collectionVariableSymbol.HasConstraint(CollectionConstraint.Empty, programState))
                    {
                        OnCollectionAccessed(instruction.GetLocation());
                    }
                }

                return newProgramState;
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
                        if (methodSymbol == null ||
                            IgnoredMethods.Any(m => m(methodSymbol)))
                        {
                            return newProgramState;
                        }

                        if (AddMethods.Any(m => m(methodSymbol)))
                        {
                            newProgramState = collectionSymbol.SetConstraint(CollectionConstraint.NotEmpty, newProgramState);
                        }
                        else if (methodSymbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T) &&
                            collectionSymbol.HasConstraint(CollectionConstraint.Empty, programState))
                        {
                            OnCollectionAccessed(memberAccess.Name.GetLocation());
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
                var newProgramState = programState;
                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var objectCreation = (ObjectCreationExpressionSyntax)instruction;
                    var constructorSymbol = semanticModel.GetSymbolInfo(objectCreation).Symbol as IMethodSymbol;

                    if (constructorSymbol.ContainingType.ConstructedFrom.IsAny(CollectionTypes) &&
                        (!constructorSymbol.Parameters.Any()
                        || constructorSymbol.Parameters.Count(p => p.IsType(KnownType.System_Int32)) == 1))
                    // TODO: HashSet, Queue, Stack, ObservableCollection, Immutable*
                    {
                        newProgramState = symbolicValue.SetConstraint(CollectionConstraint.Empty, newProgramState);
                    }
                    if (objectCreation.Initializer != null)
                    {
                        var constraint = objectCreation.Initializer.Expressions.Count == 0
                            ? CollectionConstraint.Empty
                            : CollectionConstraint.NotEmpty;
                        newProgramState = symbolicValue.SetConstraint(constraint, newProgramState);
                    }
                }
                return newProgramState;
            }

            private static bool IsListMethod(IMethodSymbol symbol, string name) =>
                symbol != null &&
                symbol.Name == name &&
                symbol.ContainingType.ConstructedFrom.Is(KnownType.System_Collections_Generic_List_T);

            private static bool IsObjectMethod(IMethodSymbol symbol, string name) =>
                symbol != null &&
                symbol.Name == name &&
                symbol.ContainingType.Is(KnownType.System_Object);

            ////private static bool IsAtrrayMethod(IMethodSymbol symbol, string name) =>
            ////    symbol?.Name == name &&
            ////    symbol.ContainingType.Is(KnownType.System_Array);

            private static readonly ISet<KnownType> CollectionTypes = new HashSet<KnownType>
            {
                KnownType.System_Collections_Generic_List_T
            };

            private static readonly IEnumerable<Func<IMethodSymbol, bool>> AddMethods = new List<Func<IMethodSymbol, bool>>
            {
                symbol => IsListMethod(symbol, nameof(List<object>.Add)),
                symbol => IsListMethod(symbol, nameof(List<object>.AddRange)),
                symbol => IsListMethod(symbol, nameof(List<object>.Insert)),
                symbol => IsListMethod(symbol, nameof(List<object>.InsertRange)),
            };

            private static readonly IEnumerable<Func<IMethodSymbol, bool>> IgnoredMethods = new List<Func<IMethodSymbol, bool>>
            {
                symbol => IsListMethod(symbol, nameof(List<object>.AsReadOnly)),
                symbol => IsListMethod(symbol, nameof(List<object>.ToArray)),
                symbol => IsObjectMethod(symbol, nameof(List<object>.GetHashCode)),
                symbol => IsObjectMethod(symbol, nameof(List<object>.Equals)),
                symbol => IsObjectMethod(symbol, nameof(List<object>.GetType)),
                symbol => IsObjectMethod(symbol, nameof(List<object>.ToString)),
            };
        }

        internal sealed class CollectionAccessedEventArgs : EventArgs
        {
            public Location Location { get; }

            public CollectionAccessedEventArgs(Location location)
            {
                Location = location;
            }
        }
    }
}

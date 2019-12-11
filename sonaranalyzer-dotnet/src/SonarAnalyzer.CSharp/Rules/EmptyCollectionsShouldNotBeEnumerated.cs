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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class EmptyCollectionsShouldNotBeEnumerated : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4158";
        private const string MessageFormat = "Remove this call, the collection is known to be empty here.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<KnownType> TrackedCollectionTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_Generic_Dictionary_TKey_TValue,
                KnownType.System_Collections_Generic_List_T,
                KnownType.System_Collections_Generic_Queue_T,
                KnownType.System_Collections_Generic_Stack_T,
                KnownType.System_Collections_Generic_HashSet_T,
                KnownType.System_Collections_ObjectModel_ObservableCollection_T,
                KnownType.System_Array
            );

        private static readonly HashSet<string> AddMethods = new HashSet<string>
        {
            nameof(List<object>.Add),
            nameof(List<object>.AddRange),
            nameof(List<object>.Insert),
            nameof(List<object>.InsertRange),
            nameof(Queue<object>.Enqueue),
            nameof(Stack<object>.Push),
            nameof(HashSet<object>.Add),
            nameof(HashSet<object>.UnionWith),
            "TryAdd" // This is a .NetCore 2.0+ method on Dictionary
        };

        private static readonly HashSet<string> IgnoredMethods = new HashSet<string>
        {
            nameof(List<object>.GetHashCode),
            nameof(List<object>.Equals),
            nameof(List<object>.GetType),
            nameof(List<object>.ToString),
            nameof(List<object>.ToArray),
            nameof(List<object>.Contains),
            nameof(Array.GetLength),
            "GetLongLength",
            nameof(Array.GetLowerBound),
            nameof(Array.GetUpperBound),
            nameof(HashSet<object>.Contains),
            nameof(Dictionary<object, object>.ContainsKey),
            nameof(Dictionary<object, object>.ContainsValue),
            "GetObjectData",
            "OnDeserialization",
            nameof(Dictionary<object, object>.TryGetValue),
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            //FIXME: Temporary silence for CFG defork
            //context.RegisterExplodedGraphBasedAnalysis(CheckForEmptyCollectionAccess);
        }

        private void CheckForEmptyCollectionAccess(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var check = new EmptyCollectionAccessedCheck(explodedGraph);
            explodedGraph.AddExplodedGraphCheck(check);

            var emptyCollections = new HashSet<SyntaxNode>();
            var nonEmptyCollections = new HashSet<SyntaxNode>();

            void explorationEnded(object sender, EventArgs args) => emptyCollections.Except(nonEmptyCollections)
                    .Select(node => Diagnostic.Create(rule, node.GetLocation()))
                    .ToList()
                    .ForEach(d => context.ReportDiagnosticWhenActive(d));

            void collectionAccessedHandler(object sender, CollectionAccessedEventArgs args) =>
                (args.IsEmpty ? emptyCollections : nonEmptyCollections).Add(args.Node);

            explodedGraph.ExplorationEnded += explorationEnded;
            check.CollectionAccessed += collectionAccessedHandler;
            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                check.CollectionAccessed -= collectionAccessedHandler;
                explodedGraph.ExplorationEnded -= explorationEnded;
            }
        }

        internal sealed class EmptyCollectionAccessedCheck : ExplodedGraphCheck
        {
            public event EventHandler<CollectionAccessedEventArgs> CollectionAccessed;

            public EmptyCollectionAccessedCheck(CSharpExplodedGraph explodedGraph)
                : base(explodedGraph)
            {
            }

            private void OnCollectionAccessed(SyntaxNode node, bool empty)
            {
                CollectionAccessed?.Invoke(this, new CollectionAccessedEventArgs(node, empty));
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

            private ProgramState ProcessInvocation(ProgramState programState, InvocationExpressionSyntax invocation)
            {
                // Argument of the nameof expression is not pushed on stack so we need to exit the checks
                if (invocation.IsNameof(this.semanticModel))
                {
                    return programState;
                }

                // Remove collection constraint from all arguments passed to an invocation
                var newProgramState = RemoveCollectionConstraintsFromArguments(invocation.ArgumentList, programState);

                if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                {
                    return newProgramState;
                }

                var collectionSymbol = this.semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                var collectionType = GetCollectionType(collectionSymbol);

                // When invoking a collection method ...
                if (collectionType.IsAny(TrackedCollectionTypes))
                {
                    var methodSymbol = this.semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (IsIgnoredMethod(methodSymbol))
                    {
                        // ... ignore some methods that are irrelevant
                        return newProgramState;
                    }

                    if (methodSymbol.IsExtensionMethod)
                    {
                        // Extension methods could modify the collection, hence we remove the Empty constraint if present
                        return collectionSymbol.RemoveConstraint(CollectionCapacityConstraint.Empty, newProgramState);
                    }

                    if (AddMethods.Contains(methodSymbol.Name))
                    {
                        // ... set constraint if we are adding items
                        newProgramState = collectionSymbol.SetConstraint(CollectionCapacityConstraint.NotEmpty,
                            newProgramState);
                    }
                    else
                    {
                        // ... notify we are accessing the collection
                        OnCollectionAccessed(invocation,
                            collectionSymbol.HasConstraint(CollectionCapacityConstraint.Empty, newProgramState));
                    }
                }

                return newProgramState;
            }

            private ProgramState ProcessElementAccess(ProgramState programState, ElementAccessExpressionSyntax elementAccess)
            {
                var collectionSymbol = this.semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol;
                var collectionType = GetCollectionType(collectionSymbol);

                // When accessing elements from a collection ...
                if (collectionType?.ConstructedFrom != null &&
                    collectionType.ConstructedFrom.IsAny(TrackedCollectionTypes))
                {
                    if (collectionType.ConstructedFrom.Is(KnownType.System_Collections_Generic_Dictionary_TKey_TValue) &&
                        IsDictionarySetItem(elementAccess))
                    {
                        // ... set constraint if we are calling the Dictionary set accessor
                        return collectionSymbol.SetConstraint(CollectionCapacityConstraint.NotEmpty, programState);
                    }
                    else
                    {
                        // ... notify we are accessing the collection
                        OnCollectionAccessed(elementAccess,
                            collectionSymbol.HasConstraint(CollectionCapacityConstraint.Empty, programState));
                    }
                }

                return programState;
            }

            public override ProgramState ObjectCreating(ProgramState programState, SyntaxNode instruction)
            {
                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    // When any object is being created ...
                    var objectCreationSyntax = (ObjectCreationExpressionSyntax)instruction;

                    // Remove collection constraint from all arguments passed to the constructor
                    var newProgramState = RemoveCollectionConstraintsFromArguments(objectCreationSyntax.ArgumentList, programState);
                    return newProgramState;
                }

                return programState;
            }

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue,
                SyntaxNode instruction)
            {
                var newProgramState = programState;
                CollectionCapacityConstraint constraint = null;

                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var objectCreationSyntax = (ObjectCreationExpressionSyntax)instruction;
                    var constructor = this.semanticModel.GetSymbolInfo(objectCreationSyntax).Symbol as IMethodSymbol;
                    // When a collection is being created ...
                    if (IsCollectionConstructor(constructor))
                    {
                        // ... try to devise what constraint could be applied by the constructor or the initializer
                        constraint =
                            GetInitializerConstraint(objectCreationSyntax.Initializer) ??
                            GetCollectionConstraint(constructor);
                    }
                }
                else if (instruction.IsKind(SyntaxKind.ArrayCreationExpression))
                {
                    // When an array is being created ...
                    var arrayCreationSyntax = (ArrayCreationExpressionSyntax)instruction;

                    // ... try to devise what constraint could be applied by the array size or the initializer
                    constraint =
                        GetInitializerConstraint(arrayCreationSyntax.Initializer) ??
                        GetArrayConstraint(arrayCreationSyntax);
                }

                return constraint != null
                    ? newProgramState.SetConstraint(symbolicValue, constraint)
                    : newProgramState;
            }

            private static bool IsIgnoredMethod(IMethodSymbol methodSymbol)
            {
                return methodSymbol == null
                    || IgnoredMethods.Contains(methodSymbol.Name);
            }

            private CollectionCapacityConstraint GetArrayConstraint(ArrayCreationExpressionSyntax arrayCreation)
            {
                // Only one-dimensional arrays can be empty, others are indeterminate, this can be improved in the future
                if (arrayCreation?.Type?.RankSpecifiers == null ||
                    arrayCreation.Type.RankSpecifiers.Count != 1 ||
                    arrayCreation.Type.RankSpecifiers[0].Sizes.Count != 1)
                {
                    return null;
                }

                var size = arrayCreation.Type.RankSpecifiers[0].Sizes[0] as LiteralExpressionSyntax;

                return size?.Token.ValueText == "0"
                    ? CollectionCapacityConstraint.Empty
                    : null;
            }

            private CollectionCapacityConstraint GetCollectionConstraint(IMethodSymbol constructor)
            {
                // Default constructor, or constructor that specifies capacity means empty collection,
                // otherwise do not apply constraint because we cannot be sure what has been passed
                // as arguments.
                var defaultCtorOrCapacityCtor = !constructor.Parameters.Any()
                    || constructor.Parameters.Count(p => p.IsType(KnownType.System_Int32)) == 1;

                return defaultCtorOrCapacityCtor ? CollectionCapacityConstraint.Empty : null;
            }

            private CollectionCapacityConstraint GetInitializerConstraint(InitializerExpressionSyntax initializer)
            {
                if (initializer?.Expressions == null)
                {
                    return null;
                }

                return initializer.Expressions.Count == 0
                    ? CollectionCapacityConstraint.Empty // No items added through the initializer
                    : CollectionCapacityConstraint.NotEmpty;
            }

            private static ProgramState RemoveCollectionConstraintsFromArguments(ArgumentListSyntax argumentList,
                ProgramState programState)
            {
                return GetArgumentSymbolicValues(argumentList, programState)
                    .Aggregate(programState,
                        (state, value) => state.RemoveConstraint(value, CollectionCapacityConstraint.Empty));
            }

            private static IEnumerable<SymbolicValue> GetArgumentSymbolicValues(ArgumentListSyntax argumentList,
                ProgramState programState)
            {
                if (argumentList?.Arguments == null)
                {
                    return Enumerable.Empty<SymbolicValue>();
                }

                var tempProgramState = programState;

                return argumentList.Arguments
                    .Select(argument =>
                    {
                        // We have side effect here, but it is harmless, we only need the symbolic values
                        tempProgramState = tempProgramState.PopValue(out var value);
                        return value;
                    });
            }

            private static bool IsDictionarySetItem(ElementAccessExpressionSyntax elementAccess) =>
                (elementAccess.GetFirstNonParenthesizedParent() as AssignmentExpressionSyntax)
                    ?.Left.RemoveParentheses() == elementAccess;

            private static bool IsCollectionConstructor(IMethodSymbol constructorSymbol) =>
                constructorSymbol?.ContainingType?.ConstructedFrom != null &&
                constructorSymbol.ContainingType.ConstructedFrom.IsAny(TrackedCollectionTypes);

            private static INamedTypeSymbol GetCollectionType(ISymbol collectionSymbol) =>
                (collectionSymbol.GetSymbolType() as INamedTypeSymbol)?.ConstructedFrom ?? // collections
                collectionSymbol.GetSymbolType()?.BaseType; // arrays
        }

        internal sealed class CollectionAccessedEventArgs : EventArgs
        {
            public SyntaxNode Node { get; }
            public bool IsEmpty { get; }

            public CollectionAccessedEventArgs(SyntaxNode node, bool isEmpty)
            {
                Node = node;
                IsEmpty = isEmpty;
            }
        }
    }
}

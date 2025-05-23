﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Sonar.Analyzers
{
    internal sealed class EmptyCollectionsShouldNotBeEnumerated : ISymbolicExecutionAnalyzer
    {
        private const string DiagnosticId = "S4158";
        private const string MessageFormat = "Remove this call, the collection is known to be empty here.";

        internal static readonly DiagnosticDescriptor S4158 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> TrackedCollectionTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_Generic_Dictionary_TKey_TValue,
                KnownType.System_Collections_Generic_List_T,
                KnownType.System_Collections_Generic_Queue_T,
                KnownType.System_Collections_Generic_Stack_T,
                KnownType.System_Collections_Generic_HashSet_T,
                KnownType.System_Collections_ObjectModel_ObservableCollection_T,
                KnownType.System_Array);

        private static readonly HashSet<string> AddMethods = new()
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

        private static readonly HashSet<string> IgnoredMethods = new()
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

        public IEnumerable<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(S4158);

        public ISymbolicExecutionAnalysisContext CreateContext(SonarSyntaxNodeReportingContext context, SonarExplodedGraph explodedGraph) =>
            new AnalysisContext(explodedGraph);

        private sealed class AnalysisContext : ISymbolicExecutionAnalysisContext
        {
            private readonly HashSet<SyntaxNode> emptyCollections = new();
            private readonly HashSet<SyntaxNode> nonEmptyCollections = new();

            public AnalysisContext(SonarExplodedGraph explodedGraph) =>
                explodedGraph.AddExplodedGraphCheck(new EmptyCollectionAccessedCheck(explodedGraph, this));

            public bool SupportsPartialResults => false;

            public IEnumerable<Diagnostic> GetDiagnostics() =>
                emptyCollections.Except(nonEmptyCollections).Select(node => Diagnostic.Create(S4158, node.GetLocation()));

            public void Dispose()
            {
                // Nothing to dispose
            }

            public void AddCollectionAccess(SyntaxNode node, bool isEmpty) =>
                (isEmpty ? emptyCollections : nonEmptyCollections).Add(node);
        }

        private sealed class EmptyCollectionAccessedCheck : ExplodedGraphCheck
        {
            private readonly AnalysisContext context;

            public EmptyCollectionAccessedCheck(AbstractExplodedGraph explodedGraph, AnalysisContext context) : base(explodedGraph) =>
                this.context = context;

            public override ProgramState PreProcessInstruction(ProgramPoint programPoint, ProgramState programState)
            {
                var instruction = programPoint.CurrentInstruction;

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
                if (invocation.IsNameof(semanticModel))
                {
                    return programState;
                }

                // Remove collection constraint from all arguments passed to an invocation
                var newProgramState = RemoveCollectionConstraintsFromArguments(invocation.ArgumentList, programState);

                if (!(invocation.Expression is MemberAccessExpressionSyntax memberAccess))
                {
                    return newProgramState;
                }

                var collectionSymbol = semanticModel.GetSymbolInfo(memberAccess.Expression).Symbol;
                var collectionType = GetCollectionType(collectionSymbol);

                // When invoking a collection method ...
                if (collectionType.IsAny(TrackedCollectionTypes))
                {
                    var methodSymbol = semanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;
                    if (IsIgnoredMethod(methodSymbol))
                    {
                        // ... ignore some methods that are irrelevant
                        return newProgramState;
                    }

                    if (methodSymbol.IsExtensionMethod)
                    {
                        // Extension methods could modify the collection, hence we remove the Empty constraint if present
                        return collectionSymbol.RemoveConstraint(CollectionConstraint.Empty, newProgramState);
                    }

                    if (AddMethods.Contains(methodSymbol.Name))
                    {
                        // ... set constraint if we are adding items
                        newProgramState = collectionSymbol.SetConstraint(CollectionConstraint.NotEmpty, newProgramState);
                    }
                    else
                    {
                        // ... notify we are accessing the collection
                        context.AddCollectionAccess(invocation, collectionSymbol.HasConstraint(CollectionConstraint.Empty, newProgramState));
                    }
                }

                return newProgramState;
            }

            private ProgramState ProcessElementAccess(ProgramState programState, ElementAccessExpressionSyntax elementAccess)
            {
                var collectionSymbol = semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol;
                var collectionType = GetCollectionType(collectionSymbol);

                // When accessing elements from a collection ...
                if (collectionType?.ConstructedFrom != null &&
                    collectionType.ConstructedFrom.IsAny(TrackedCollectionTypes))
                {
                    if (collectionType.ConstructedFrom.Is(KnownType.System_Collections_Generic_Dictionary_TKey_TValue) && IsDictionarySetItem(elementAccess))
                    {
                        // ... set constraint if we are calling the Dictionary set accessor
                        return collectionSymbol.SetConstraint(CollectionConstraint.NotEmpty, programState);
                    }
                    else
                    {
                        // ... notify we are accessing the collection
                        context.AddCollectionAccess(elementAccess, collectionSymbol.HasConstraint(CollectionConstraint.Empty, programState));
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

            public override ProgramState ObjectCreated(ProgramState programState, SymbolicValue symbolicValue, SyntaxNode instruction)
            {
                var newProgramState = programState;
                CollectionConstraint constraint = null;

                if (instruction.IsKind(SyntaxKind.ObjectCreationExpression))
                {
                    var objectCreationSyntax = (ObjectCreationExpressionSyntax)instruction;
                    var constructor = semanticModel.GetSymbolInfo(objectCreationSyntax).Symbol as IMethodSymbol;
                    // When a collection is being created ...
                    if (IsCollectionConstructor(constructor))
                    {
                        // ... try to devise what constraint could be applied by the constructor or the initializer
                        constraint = GetInitializerConstraint(objectCreationSyntax.Initializer)
                            ?? GetCollectionConstraint(constructor);
                    }
                }
                else if (instruction.IsKind(SyntaxKind.ArrayCreationExpression))
                {
                    // When an array is being created ...
                    var arrayCreationSyntax = (ArrayCreationExpressionSyntax)instruction;

                    // ... try to devise what constraint could be applied by the array size or the initializer
                    constraint = GetInitializerConstraint(arrayCreationSyntax.Initializer)
                        ?? GetArrayConstraint(arrayCreationSyntax);
                }

                return constraint == null
                    ? newProgramState
                    : newProgramState.SetConstraint(symbolicValue, constraint);
            }

            private static bool IsIgnoredMethod(ISymbol methodSymbol) =>
                methodSymbol == null || IgnoredMethods.Contains(methodSymbol.Name);

            private static CollectionConstraint GetArrayConstraint(ArrayCreationExpressionSyntax arrayCreation)
            {
                // Only one-dimensional arrays can be empty, others are indeterminate, this can be improved in the future
                if (arrayCreation?.Type?.RankSpecifiers == null
                    || arrayCreation.Type.RankSpecifiers.Count != 1
                    || arrayCreation.Type.RankSpecifiers[0].Sizes.Count != 1)
                {
                    return null;
                }

                var size = arrayCreation.Type.RankSpecifiers[0].Sizes[0] as LiteralExpressionSyntax;

                return size?.Token.ValueText == "0"
                    ? CollectionConstraint.Empty
                    : null;
            }

            private static CollectionConstraint GetCollectionConstraint(IMethodSymbol constructor) =>
                // Default constructor, or constructor that specifies capacity means empty collection,
                // otherwise do not apply constraint because we cannot be sure what has been passed
                // as arguments.
                !constructor.Parameters.Any() || constructor.Parameters.Count(p => p.IsType(KnownType.System_Int32)) == 1
                    ? CollectionConstraint.Empty
                    : null;

            private static CollectionConstraint GetInitializerConstraint(InitializerExpressionSyntax initializer)
            {
                if (initializer?.Expressions == null)
                {
                    return null;
                }

                return initializer.Expressions.Count == 0
                    ? CollectionConstraint.Empty // No items added through the initializer
                    : CollectionConstraint.NotEmpty;
            }

            private static ProgramState RemoveCollectionConstraintsFromArguments(BaseArgumentListSyntax argumentList, ProgramState programState) =>
                GetArgumentSymbolicValues(argumentList, programState)
                .Aggregate(programState, (state, value) => state.RemoveConstraint(value, CollectionConstraint.Empty));

            private static IEnumerable<SymbolicValue> GetArgumentSymbolicValues(BaseArgumentListSyntax argumentList, ProgramState programState)
            {
                if (argumentList?.Arguments == null)
                {
                    return Enumerable.Empty<SymbolicValue>();
                }

                var tempProgramState = programState;

                return argumentList.Arguments
                    .Where(HasAssociatedSymbolicValue)
                    .Select(argument =>
                    {
                        // We have side effect here, but it is harmless, we only need the symbolic values
                        tempProgramState = tempProgramState.PopValue(out var value);
                        return value;
                    });
            }

            private static bool HasAssociatedSymbolicValue(ArgumentSyntax argumentSyntax) =>
                // e.g. Method(out var _)
                !DeclarationExpressionSyntaxWrapper.IsInstance(argumentSyntax.Expression)
                || !((DeclarationExpressionSyntaxWrapper)argumentSyntax.Expression).Designation.SyntaxNode.IsKind(SyntaxKindEx.DiscardDesignation);

            private static bool IsDictionarySetItem(SyntaxNode elementAccess) =>
                (elementAccess.GetFirstNonParenthesizedParent() as AssignmentExpressionSyntax)?.Left.RemoveParentheses() == elementAccess;

            private static bool IsCollectionConstructor(ISymbol constructorSymbol) =>
                constructorSymbol?.ContainingType?.ConstructedFrom != null
                && constructorSymbol.ContainingType.ConstructedFrom.IsAny(TrackedCollectionTypes);

            private static INamedTypeSymbol GetCollectionType(ISymbol collectionSymbol) =>
                (collectionSymbol.GetSymbolType() as INamedTypeSymbol)?.ConstructedFrom  // collections
                ?? collectionSymbol.GetSymbolType()?.BaseType; // arrays
        }
    }
}

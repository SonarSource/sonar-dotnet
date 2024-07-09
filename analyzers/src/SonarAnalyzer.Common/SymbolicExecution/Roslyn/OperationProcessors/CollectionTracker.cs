/*
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

using System.Collections.ObjectModel;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal static class CollectionTracker
{
    public static readonly ImmutableArray<KnownType> CollectionTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Generic_IList_T,
        KnownType.System_Collections_Immutable_IImmutableList_T,
        KnownType.System_Collections_Generic_ICollection_T,
        KnownType.System_Collections_Generic_HashSet_T,
        KnownType.System_Collections_Generic_ISet_T,
        KnownType.System_Collections_Immutable_IImmutableSet_T,
        KnownType.System_Collections_Generic_Queue_T,
        KnownType.System_Collections_Immutable_IImmutableQueue_T,
        KnownType.System_Collections_Generic_Stack_T,
        KnownType.System_Collections_Immutable_IImmutableStack_T,
        KnownType.System_Collections_ObjectModel_ObservableCollection_T,
        KnownType.System_Array,
        KnownType.System_Collections_Immutable_IImmutableArray_T,
        KnownType.System_Collections_Generic_Dictionary_TKey_TValue,
        KnownType.System_Collections_Generic_IDictionary_TKey_TValue,
        KnownType.System_Collections_Immutable_IImmutableDictionary_TKey_TValue);

    private static readonly HashSet<string> AddMethods =
    [
        nameof(ICollection<int>.Add),
        nameof(List<int>.AddRange),
        nameof(List<int>.Insert),
        nameof(List<int>.InsertRange),
        nameof(HashSet<int>.UnionWith),
        nameof(HashSet<int>.SymmetricExceptWith),   // This can add and/or remove items => It should remove all CollectionConstraints.
                                                    // However, just learning NotEmpty (and thus unlearning Empty) is good enough for now.
        nameof(Queue<int>.Enqueue),
        nameof(Stack<int>.Push),
        nameof(Collection<int>.Insert),
        "TryAdd"
    ];

    private static readonly HashSet<string> RemoveMethods =
    [
        nameof(ICollection<int>.Remove),
        nameof(List<int>.RemoveAll),
        nameof(List<int>.RemoveAt),
        nameof(List<int>.RemoveRange),
        nameof(HashSet<int>.ExceptWith),
        nameof(HashSet<int>.IntersectWith),
        nameof(HashSet<int>.RemoveWhere),
        nameof(Queue<int>.Dequeue),
        nameof(Stack<int>.Pop)
    ];

    private static readonly HashSet<string> AddOrRemoveMethods = [.. AddMethods, .. RemoveMethods];

    public static ProgramState LearnFrom(ProgramState state, IObjectCreationOperationWrapper operation)
    {
        if (operation.Type.IsAny(CollectionTypes))
        {
            if (operation.Arguments.SingleOrDefault(IsEnumerable) is { } argument)
            {
                return state.Constraint<CollectionConstraint>(argument) is { } constraint
                    ? state.SetOperationConstraint(operation, constraint)
                    : state;
            }
            else
            {
                return state.SetOperationConstraint(operation, CollectionConstraint.Empty);
            }
        }
        else
        {
            return state;
        }

        static bool IsEnumerable(IOperation operation) =>
            operation.ToArgument().Parameter.Type.DerivesOrImplements(KnownType.System_Collections_IEnumerable);
    }

    public static ProgramState LearnFrom(ProgramState state, IArrayCreationOperationWrapper operation)
    {
        var constraint = operation.DimensionSizes.Any(x => x.ConstantValue.Value is 0)
            ? CollectionConstraint.Empty
            : CollectionConstraint.NotEmpty;
        return state.SetOperationConstraint(operation, constraint);
    }

    public static ProgramState LearnFrom(ProgramState state, IMethodReferenceOperationWrapper operation) =>
        operation.Instance is not null
        && operation.Instance.Type.DerivesOrImplementsAny(CollectionTypes)
        && AddOrRemoveMethods.Contains(operation.Method.Name)
        && state[operation.Instance] is { } value
            ? state.SetOperationAndSymbolValue(operation.Instance, value.WithoutConstraint<CollectionConstraint>())
            : state;

    public static ProgramState LearnFrom(ProgramState state, IPropertyReferenceOperationWrapper operation, ISymbol instanceSymbol)
    {
        if (operation.Instance is not null
            && operation.Property.Name is nameof(Array.Length) or nameof(List<int>.Count))
        {
            if (instanceSymbol is not null
                && state[instanceSymbol]?.Constraint<CollectionConstraint>() is { } constraint)
            {
                var numberConstraint = constraint == CollectionConstraint.Empty
                    ? NumberConstraint.From(0)
                    : NumberConstraint.From(1, null);
                state = state.SetOperationConstraint(operation, numberConstraint);
            }
            else if (operation.Instance.Type.DerivesOrImplementsAny(CollectionTypes))
            {
                state = state.SetOperationConstraint(operation, NumberConstraint.From(0, null));
            }
        }
        else if (operation.Property.IsIndexer)
        {
            state = state.SetOperationConstraint(operation.Instance, CollectionConstraint.NotEmpty);
            if (instanceSymbol is not null)
            {
                state = state.SetSymbolConstraint(instanceSymbol, CollectionConstraint.NotEmpty);
            }
        }
        return state;
    }

    public static ProgramState LearnFrom(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (EnumerableCountConstraint(state, invocation) is { } constraint)
        {
            return state.SetOperationConstraint(invocation, constraint);
        }
        if (invocation.GetInstance(state) is { } instance && instance.Type.DerivesOrImplementsAny(CollectionTypes))
        {
            var targetMethod = invocation.TargetMethod;
            var symbolValue = state[instance] ?? SymbolicValue.Empty;
            if (AddMethods.Contains(targetMethod.Name))
            {
                return SetOperationAndSymbolValue(symbolValue.WithConstraint(CollectionConstraint.NotEmpty));
            }
            else if (RemoveMethods.Contains(targetMethod.Name))
            {
                return SetOperationAndSymbolValue(symbolValue.WithoutConstraint(CollectionConstraint.NotEmpty));
            }
            else if (targetMethod.Name == nameof(ICollection<int>.Clear))
            {
                return SetOperationAndSymbolValue(symbolValue.WithConstraint(CollectionConstraint.Empty));
            }
        }
        return state;

        ProgramState SetOperationAndSymbolValue(SymbolicValue value)
        {
            if (instance.TrackedSymbol(state) is { } symbol)
            {
                state = state.SetSymbolValue(symbol, value);
            }
            return state.SetOperationValue(instance, value);
        }
    }

    private static NumberConstraint EnumerableCountConstraint(ProgramState state, IInvocationOperationWrapper invocation)
    {
        if (invocation.TargetMethod.Is(KnownType.System_Linq_Enumerable, nameof(Enumerable.Count)))
        {
            if (invocation.GetInstance(state).TrackedSymbol(state) is { } symbol
                && state[symbol]?.Constraint<CollectionConstraint>() is { } collection)
            {
                if (collection == CollectionConstraint.Empty)
                {
                    return NumberConstraint.From(0);
                }
                else
                {
                    return HasFilteringPredicate()
                        ? NumberConstraint.From(0, null) // nonEmpty.Count(predicate) can be Empty or NotEmpty
                        : NumberConstraint.From(1, null);
                }
            }
            else
            {
                return NumberConstraint.From(0, null);
            }
        }
        return null;

        bool HasFilteringPredicate() =>
            invocation.Arguments.Any(x => x.ToArgument().Parameter.Type.Is(KnownType.System_Func_T_TResult));
    }
}

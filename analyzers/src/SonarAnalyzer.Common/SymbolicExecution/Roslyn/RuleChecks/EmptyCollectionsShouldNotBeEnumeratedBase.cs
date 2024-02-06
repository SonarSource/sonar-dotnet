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

using System.Collections;
using System.Collections.ObjectModel;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

public abstract class EmptyCollectionsShouldNotBeEnumeratedBase : SymbolicRuleCheck
{
    protected const string DiagnosticId = "S4158";
    protected const string MessageFormat = "Remove this call, the collection is known to be empty here.";

    internal static readonly ImmutableArray<KnownType> TrackedCollectionTypes = new[]
    {
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
        KnownType.System_Collections_Immutable_IImmutableDictionary_TKey_TValue,
    }.ToImmutableArray();

    protected static readonly HashSet<string> RaisingMethods = new()
    {
        nameof(IEnumerable<int>.GetEnumerator),
        nameof(ICollection.CopyTo),
        nameof(ICollection<int>.Clear),
        nameof(ICollection<int>.Contains),
        nameof(ICollection<int>.Remove),
        nameof(List<int>.BinarySearch),
        nameof(List<int>.ConvertAll),
        nameof(List<int>.Exists),
        nameof(List<int>.Find),
        nameof(List<int>.FindAll),
        nameof(List<int>.FindIndex),
        nameof(List<int>.FindLast),
        nameof(List<int>.FindLastIndex),
        nameof(List<int>.ForEach),
        nameof(List<int>.GetRange),
        nameof(List<int>.IndexOf),
        nameof(List<int>.LastIndexOf),
        nameof(List<int>.RemoveAll),
        nameof(List<int>.RemoveAt),
        nameof(List<int>.RemoveRange),
        nameof(List<int>.Reverse),
        nameof(List<int>.Sort),
        nameof(List<int>.TrueForAll),
        nameof(HashSet<int>.ExceptWith),
        nameof(HashSet<int>.IntersectWith),
        nameof(HashSet<int>.IsProperSubsetOf),
        nameof(HashSet<int>.IsProperSupersetOf),
        nameof(HashSet<int>.IsSubsetOf),
        nameof(HashSet<int>.IsSupersetOf),
        nameof(HashSet<int>.Overlaps),
        nameof(HashSet<int>.RemoveWhere),
        nameof(HashSet<int>.SymmetricExceptWith),
        nameof(HashSet<int>.UnionWith),
        nameof(Queue<int>.Dequeue),
        nameof(Queue<int>.Peek),
        "TryDequeue",
        "TryPeek",
        nameof(Stack<int>.Pop),
        "TryPop",
        nameof(ObservableCollection<int>.Move),
        nameof(Array.Clone),
        nameof(Array.GetLength),
        nameof(Array.GetLongLength),
        nameof(Array.GetLowerBound),
        nameof(Array.GetUpperBound),
        nameof(Array.GetValue),
        nameof(Array.Initialize),
        nameof(Array.SetValue),
        nameof(Dictionary<int, int>.ContainsKey),
        nameof(Dictionary<int, int>.ContainsValue),
        nameof(Dictionary<int, int>.TryGetValue)
    };

    private static readonly HashSet<string> AddMethods = new()
    {
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
    };

    private static readonly HashSet<string> RemoveMethods = new()
    {
        nameof(ICollection<int>.Remove),
        nameof(List<int>.RemoveAll),
        nameof(List<int>.RemoveAt),
        nameof(List<int>.RemoveRange),
        nameof(HashSet<int>.ExceptWith),
        nameof(HashSet<int>.IntersectWith),
        nameof(HashSet<int>.RemoveWhere),
        nameof(Queue<int>.Dequeue),
        nameof(Stack<int>.Pop)
    };

    private readonly HashSet<IOperation> emptyAccess = new();
    private readonly HashSet<IOperation> nonEmptyAccess = new();

    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var operation = context.Operation.Instance;
        if (operation.AsArrayCreation() is { } arrayCreation)
        {
            return context.State.SetOperationConstraint(operation, arrayCreation.DimensionSizes.Any(x => x.ConstantValue.Value is 0) ? CollectionConstraint.Empty : CollectionConstraint.NotEmpty);
        }
        else if (operation.AsInvocation() is { } invocation)
        {
            return ProcessInvocation(context, invocation);
        }
        else if (operation.AsMethodReference() is { Instance: not null } methodReference)
        {
            return ProcessAddMethod(context.State, methodReference.Method, methodReference.Instance) ?? context.State;
        }
        else if (operation.AsPropertyReference() is { Property.IsIndexer: true } indexer)
        {
            return ProcessIndexerAccess(context.State, indexer);
        }
        else if (operation.AsPropertyReference() is { Instance: not null } propertyReference && PropertyReferenceConstraint(context.State, propertyReference) is { } constraint)
        {
            return context.State.SetOperationConstraint(operation, constraint);
        }
        else if (operation.Kind == OperationKindEx.Argument
            && operation.TrackedSymbol(context.State) is { } symbol
            && context.State[symbol] is { } symbolValue)
        {
            return context.State.SetSymbolValue(symbol, symbolValue.WithoutConstraint<CollectionConstraint>());
        }
        else
        {
            return context.State;
        }
    }

    public override void ExecutionCompleted()
    {
        foreach (var operation in emptyAccess.Except(nonEmptyAccess))
        {
            ReportIssue(operation, operation.Syntax.ToString());
        }
    }

    private ProgramState ProcessInvocation(SymbolicContext context, IInvocationOperationWrapper invocation)
    {
        var targetMethod = invocation.TargetMethod;
        if (targetMethod.Is(KnownType.System_Linq_Enumerable, nameof(Enumerable.Count))
            && SizeConstraint(context.State, invocation.Instance ?? invocation.Arguments[0].ToArgument().Value, HasFilteringPredicate()) is { } constraint)
        {
            return context.SetOperationConstraint(constraint);
        }
        else if (invocation.Instance is { } instance)
        {
            if (RaisingMethods.Contains(targetMethod.Name))
            {
                if (context.State[instance]?.HasConstraint(CollectionConstraint.Empty) is true)
                {
                    emptyAccess.Add(context.Operation.Instance);
                }
                else
                {
                    nonEmptyAccess.Add(context.Operation.Instance);
                }
            }
            if (instance.Type.DerivesOrImplementsAny(TrackedCollectionTypes))
            {
                return ProcessAddMethod(context.State, targetMethod, instance)
                    ?? ProcessRemoveMethod(context.State, targetMethod, instance)
                    ?? ProcessClearMethod(context.State, targetMethod, instance)
                    ?? context.State;
            }
        }
        return context.State;

        bool HasFilteringPredicate() =>
            invocation.Arguments.Any(x => x.ToArgument().Parameter.Type.Is(KnownType.System_Func_T_TResult));
    }

    private static ProgramState ProcessAddMethod(ProgramState state, IMethodSymbol method, IOperation instance) =>
        AddMethods.Contains(method.Name)
            ? SetOperationAndSymbolConstraint(state, instance, CollectionConstraint.NotEmpty)
            : null;

    private static ProgramState ProcessRemoveMethod(ProgramState state, IMethodSymbol method, IOperation instance) =>
        RemoveMethods.Contains(method.Name)
            ? SetOperationAndSymbolValue(state, instance, (state[instance] ?? SymbolicValue.Empty).WithoutConstraint(CollectionConstraint.NotEmpty))
            : null;

    private static ProgramState ProcessClearMethod(ProgramState state, IMethodSymbol method, IOperation instance) =>
        method.Name == nameof(ICollection<int>.Clear)
            ? SetOperationAndSymbolConstraint(state, instance, CollectionConstraint.Empty)
            : null;

    private static ProgramState SetOperationAndSymbolConstraint(ProgramState state, IOperation instance, SymbolicConstraint constraint) =>
        SetOperationAndSymbolValue(state, instance, (state[instance] ?? SymbolicValue.Empty).WithConstraint(constraint));

    private static ProgramState SetOperationAndSymbolValue(ProgramState state, IOperation instance, SymbolicValue value)
    {
        state = state.SetOperationValue(instance, value);
        if (instance.TrackedSymbol(state) is { } symbol)
        {
            state = state.SetSymbolValue(symbol, value);
        }
        return state;
    }

    private static NumberConstraint PropertyReferenceConstraint(ProgramState state, IPropertyReferenceOperationWrapper propertyReference) =>
        propertyReference.Property.Name is nameof(Array.Length) or nameof(List<int>.Count)
            ? SizeConstraint(state, propertyReference.Instance)
            : null;

    private static NumberConstraint SizeConstraint(ProgramState state, IOperation instance, bool hasFilteringPredicate = false)
    {
        if (instance.TrackedSymbol(state) is { } symbol && state[symbol]?.Constraint<CollectionConstraint>() is { } collection)
        {
            if (collection == CollectionConstraint.Empty)
            {
                return NumberConstraint.From(0);
            }
            else if (!hasFilteringPredicate)    // nonEmpty.Count(predicate) can be Empty or NotEmpty
            {
                return NumberConstraint.From(1, null);
            }
        }
        return instance.Type.DerivesOrImplementsAny(TrackedCollectionTypes) ? NumberConstraint.From(0, null) : null;
    }

    private static ProgramState ProcessIndexerAccess(ProgramState state, IPropertyReferenceOperationWrapper propertyReference)
    {
        state = state.SetOperationConstraint(propertyReference.Instance, CollectionConstraint.NotEmpty);
        return propertyReference.Instance.TrackedSymbol(state) is { } symbol
            ? state.SetSymbolConstraint(symbol, CollectionConstraint.NotEmpty)
            : state;
    }
}

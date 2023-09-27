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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

public sealed record ProgramState : IEquatable<ProgramState>
{
    public static readonly ProgramState Empty = new();

    private ImmutableDictionary<IOperation, SymbolicValue> OperationValue { get; init; }     // Current SymbolicValue result of a given operation
    private ImmutableDictionary<ISymbol, SymbolicValue> SymbolValue { get; init; }
    private ImmutableDictionary<int, int> VisitCount { get; init; }
    private ImmutableDictionary<CaptureId, IOperation> CaptureOperation { get; init; }
    private ImmutableHashSet<ISymbol> PreservedSymbols { get; init; }
    private ImmutableStack<ExceptionState> Exceptions { get; init; }

    public ExceptionState Exception => Exceptions.IsEmpty ? null : Exceptions.Peek();
    public SymbolicValue this[IOperationWrapperSonar operation] => this[operation.Instance];
    public SymbolicValue this[IOperationWrapper operation] => this[operation.WrappedOperation];
    public SymbolicValue this[IOperation operation] => OperationValue.TryGetValue(operation, out var value) ? value : null;
    public SymbolicValue this[ISymbol symbol] => SymbolValue.TryGetValue(symbol, out var value) ? value : null;
    public IOperation this[CaptureId capture] => CaptureOperation.TryGetValue(capture, out var value) ? value : null;

    private ProgramState()
    {
        OperationValue = ImmutableDictionary<IOperation, SymbolicValue>.Empty;
        SymbolValue = ImmutableDictionary<ISymbol, SymbolicValue>.Empty;
        VisitCount = ImmutableDictionary<int, int>.Empty;
        CaptureOperation = ImmutableDictionary<CaptureId, IOperation>.Empty;
        PreservedSymbols = ImmutableHashSet<ISymbol>.Empty;
        Exceptions = ImmutableStack<ExceptionState>.Empty;
    }

    public ProgramState SetOperationAndSymbolValue(IOperation operation, SymbolicValue value)
    {
        var newState = SetOperationValue(operation, value);
        if (operation.TrackedSymbol(this) is { } symbol)
        {
            newState = newState.SetSymbolValue(symbol, value);
        }
        return newState;
    }

    public ProgramState SetOperationValue(IOperationWrapper operation, SymbolicValue value) =>
        operation is null
            ? throw new ArgumentNullException(nameof(operation))
            : SetOperationValue(operation.WrappedOperation, value);

    public ProgramState SetOperationValue(IOperationWrapperSonar operation, SymbolicValue value) =>
        operation.Instance is null
            ? throw new ArgumentNullException(nameof(operation))
            : SetOperationValue(operation.Instance, value);

    public ProgramState SetOperationValue(IOperation operation, SymbolicValue value) =>
        SetOperationValueCore(ResolveCapture(operation), value);

    /// <summary>
    /// Sets state directly to the FlowCaptureReferenceOperation directly, without resolving the capture itself.
    /// </summary>
    public ProgramState SetOperationValue(IFlowCaptureReferenceOperationWrapper capture, SymbolicValue value) =>
        SetOperationValueCore(capture.WrappedOperation, value);

    public ProgramState SetOperationConstraint(IOperationWrapper operation, SymbolicConstraint constraint) =>
        SetOperationConstraint(operation.WrappedOperation, constraint);

    public ProgramState SetOperationConstraint(IOperationWrapperSonar operation, SymbolicConstraint constraint) =>
        SetOperationConstraint(operation.Instance, constraint);

    public ProgramState SetOperationConstraint(IOperation operation, SymbolicConstraint constraint) =>
        SetOperationValue(operation, (this[operation] ?? SymbolicValue.Empty).WithConstraint(constraint));

    public ProgramState SetSymbolValue(ISymbol symbol, SymbolicValue value) =>
        value is null
            ? this with { SymbolValue = SymbolValue.Remove(symbol) }
            : this with { SymbolValue = SymbolValue.SetItem(symbol, value) };

    public ProgramState SetSymbolConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
        SetSymbolValue(symbol, (this[symbol] ?? SymbolicValue.Empty).WithConstraint(constraint));

    public bool HasConstraint(IOperation operation, SymbolicConstraint constraint) =>
        operation is not null
        && this[operation]?.HasConstraint(constraint) is true;

    public bool HasConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
        symbol is not null
        && this[symbol]?.HasConstraint(constraint) is true;

    public T Constraint<T>(IOperation operation) where T : SymbolicConstraint =>
        operation is not null ? this[operation]?.Constraint<T>() : null;

    public T Constraint<T>(ISymbol symbol) where T : SymbolicConstraint =>
        symbol is not null ? this[symbol]?.Constraint<T>() : null;

    public ProgramState SetCapture(CaptureId capture, IOperation operation) =>
        this with { CaptureOperation = CaptureOperation.SetItem(capture, operation) };

    public ProgramState SetException(ExceptionState exception) =>
        this with { Exceptions = ImmutableStack<ExceptionState>.Empty.Push(exception) };

    public ProgramState PushException(ExceptionState exception) =>
        this with { Exceptions = Exceptions.Push(exception) };

    public ProgramState PopException() =>
        this with { Exceptions = Exceptions.Pop() };

    public IEnumerable<ISymbol> SymbolsWith(SymbolicConstraint constraint) =>
        SymbolValue.Where(x => x.Value != null && x.Value.HasConstraint(constraint)).Select(x => x.Key);

    public ProgramState ResetOperations()
    {
        var captured = CaptureOperation.Values.ToHashSet(); // Preserve only captured
        return this with { OperationValue = OperationValue.Where(x => captured.Contains(x.Key)).ToImmutableDictionary() };
    }

    public ProgramState RemoveCapture(CaptureId capture) =>
        this with { CaptureOperation = CaptureOperation.Remove(capture) };

    public IOperation ResolveCapture(IOperation operation) =>
        operation?.Kind == OperationKindEx.FlowCaptureReference
        && this[operation.ToFlowCaptureReference().Id] is { } captured
            ? captured
            : operation;

    public IOperation ResolveCaptureAndUnwrapConversion(IOperation operation) =>
        operation?.Kind switch
        {
            OperationKindEx.FlowCaptureReference => CaptureOperation.ContainsKey(operation.ToFlowCaptureReference().Id) ? ResolveCaptureAndUnwrapConversion(ResolveCapture(operation)) : operation,
            OperationKindEx.Conversion => ResolveCaptureAndUnwrapConversion(operation.UnwrapConversion()),
            _ => operation
        };

    public ProgramState RemoveSymbols(Func<ISymbol, bool> remove) =>
        this with { SymbolValue = SymbolValue.Where(kv => PreservedSymbols.Contains(kv.Key) || !remove(kv.Key)).ToImmutableDictionary() };

    public ProgramState ResetFieldConstraints() =>
        ResetFieldConstraints(field => true);

    public ProgramState ResetStaticFieldConstraints(INamedTypeSymbol containingType) =>
        ResetFieldConstraints(field => field.IsStatic && containingType.DerivesFrom(field.ContainingType));

    public ProgramState AddVisit(int programPointHash) =>
        this with { VisitCount = VisitCount.SetItem(programPointHash, GetVisitCount(programPointHash) + 1) };

    public ProgramState Preserve(ISymbol symbol) =>
        this with { PreservedSymbols = PreservedSymbols.Add(symbol) };

    public int GetVisitCount(int programPointHash) =>
        VisitCount.TryGetValue(programPointHash, out var count) ? count : 0;

    public override int GetHashCode() =>
        // VisitCount is not included, it's not part of Equals
        HashCode.Combine(
            HashCode.DictionaryContentHash(OperationValue),
            HashCode.DictionaryContentHash(SymbolValue),
            HashCode.DictionaryContentHash(CaptureOperation),
            HashCode.EnumerableUnorderedContentHash(PreservedSymbols),
            HashCode.EnumerableOrderedContentHash(Exceptions));

    public bool Equals(ProgramState other) =>
        // VisitCount is not compared, two ProgramState are equal if their current state is equal. No matter what historical path led to it.
        other is not null
        && other.OperationValue.DictionaryEquals(OperationValue)
        && other.SymbolValue.DictionaryEquals(SymbolValue)
        && other.CaptureOperation.DictionaryEquals(CaptureOperation)
        && other.PreservedSymbols.SetEquals(PreservedSymbols)
        && other.Exceptions.SequenceEqual(Exceptions);

    public override string ToString() =>
        Equals(Empty) ? "Empty" + Environment.NewLine : SerializeExceptions() + SerializeSymbols() + SerializeOperations() + SerializeCaptures();

    private ProgramState SetOperationValueCore(IOperation operation, SymbolicValue value) =>
        (operation ?? throw new ArgumentNullException(nameof(operation))) is var _
        && value is null
            ? this with { OperationValue = OperationValue.Remove(operation) }
            : this with { OperationValue = OperationValue.SetItem(operation, value) };

    private string SerializeExceptions() =>
        Exceptions.IsEmpty ? null : Exceptions.JoinStr(string.Empty, x => $"Exception: {x}{Environment.NewLine}");

    private string SerializeSymbols() =>
        Serialize(SymbolValue, "Symbols", x => x.Name, x => x.ToString());

    private string SerializeOperations() =>
        Serialize(OperationValue, "Operations", x => x.Serialize(), x => x.ToString());

    private string SerializeCaptures() =>
        Serialize(CaptureOperation, "Captures", x => "#" + x.GetHashCode(), x => x.Serialize());

    private static string Serialize<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary, string title, Func<TKey, string> serializeKey, Func<TValue, string> serializeValue)
    {
        if (dictionary.IsEmpty)
        {
            return null;
        }
        else
        {
            var sb = new StringBuilder();
            sb.Append(title).AppendLine(":");
            foreach (var kvp in dictionary.Select(x => new KeyValuePair<string, string>(serializeKey(x.Key), serializeValue(x.Value))).OrderBy(x => x.Key))
            {
                sb.Append(kvp.Key).Append(": ").AppendLine(kvp.Value);
            }
            return sb.ToString();
        }
    }

    private ProgramState ResetFieldConstraints(Func<IFieldSymbol, bool> predicate)
    {
        var state = this;
        foreach (var kvp in SymbolValue.Where(x => x.Key is IFieldSymbol field && predicate(field)))
        {
            var symbolValue = kvp.Value;
            if (symbolValue.AllConstraints.Where(x => !x.PreserveOnFieldReset).ToArray() is { Length: > 0 } resetConstraints)
            {
                foreach (var constraint in resetConstraints)
                {
                    symbolValue = symbolValue.WithoutConstraint(constraint);
                }
                state = state.SetSymbolValue(kvp.Key, symbolValue);
            }
        }
        return state;
    }

    [Conditional("DEBUG_TurnedOff")] // Turned off until all consistency check violations are fixed.
    [ExcludeFromCodeCoverage]
    public void CheckConsistency()
    {
        AssertCommonConditions(SymbolValue.Values);
        AssertCommonConditions(OperationValue.Values);

        static void AssertCommonConditions(IEnumerable<SymbolicValue> values)
        {
            foreach (var value in values)
            {
                Debug.Assert(CheckConstraintAlsoHasNotNull<BoolConstraint>(value), "If a BoolConstraint is set, NotNull should also be set.");
                Debug.Assert(CheckConstraintAlsoHasNotNull<LockConstraint>(value), "If a LockConstraint is set, NotNull should also be set.");
                Debug.Assert(CheckOnlyConstraint(value, ObjectConstraint.Null), "If Null is set, no other constraint should be set.");
            }
        }

        static bool CheckConstraintAlsoHasNotNull<T>(SymbolicValue value) where T : SymbolicConstraint =>
            !value.HasConstraint<T>() || value.HasConstraint(ObjectConstraint.NotNull);

        static bool CheckOnlyConstraint(SymbolicValue value, SymbolicConstraint single) =>
            !value.HasConstraint(single) || value.AllConstraints.Count() == 1;
    }
}
